import { Action, Reducer } from 'redux';
import { AppThunkAction } from '.';
import { VideoMetric, Tag, Video, VideoEdits, ArchiveMode } from '../types';
import { ControllerType } from './Chart';
import { ApplicationState } from '.';
import { capitalizeFirst, formatYMD } from '../reducers/helpers';
import { ComponentIds } from '../routes';
import * as _ from 'underscore';
import { authenticatedFetch } from '../ConnectionUtil';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface DataState {
  isLoading: boolean;
  content: VideoMetric[];
  marketing: VideoMetric[];
  videos: Video[];
  metaTags: { [type: string]: Tag[] };
  videoManagementArchiveMode: ArchiveMode;
  metricRequestId: { [pageType: string]: number };
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestDataAction {
  type: 'REQUEST_DATA';
  controllerType: ControllerType;
  metricRequestId: number;
}

interface ReceiveDataAction {
  type: 'RECEIVE_DATA';
  pageType: ControllerType;
  data: VideoMetric[];
  metricRequestId: number;
}

interface ReceiveVideosAction {
  type: 'RECEIVE_VIDEOS';
  data: Video[];
}

interface ReceiveMetaTagsAction {
  type: 'RECEIVE_META_TAGS';
  data: { [type: string]: Tag[] };
}

interface SetVideoManagementArchiveModeAction {
  type: 'SET_VIDEO_MANAGEMENT_ARCHIVE_MODE';
  archiveMode: ArchiveMode;
}

type KnownAction =
  RequestDataAction |
  ReceiveDataAction |
  ReceiveVideosAction |
  ReceiveMetaTagsAction |
  SetVideoManagementArchiveModeAction;

export function requestDataFunc(
  pageType: ControllerType,
  filters: string,
  fromDate: Date,
  toDate: Date,
  noMetrics: boolean,
  metaTagsOnlyOnce: boolean,
  dispatch: Function,
  getState: () => ApplicationState
) {
  let controller = capitalizeFirst(pageType);
  let archiveMode = ArchiveMode.UnArchived;
  if (getState().navMenu.activeMenuItem === ComponentIds.VideoManagement) {
    archiveMode = getState().data.videoManagementArchiveMode;
  }
  authenticatedFetch(
    `api/${controller}Data/GetVideoList/${encodeURIComponent(filters)}/${archiveMode}`
  ).then(response => {
    // If unauthorized, like when the user is logging out, this happens
    if (response.redirected || response.status !== 200) {
      return [];
    }
    return response.json() as Promise<Video[]>;
  })
    .then(data => {
      dispatch({ type: 'RECEIVE_VIDEOS', data: data });
    });

  if (!metaTagsOnlyOnce || _.keys(getState().data.metaTags).length === 0) {
    authenticatedFetch(`api/Tag/GetMetaTags/`)
      .then(response => response.json() as Promise<{ [type: string]: { [id: string]: Tag } }>)
      .then(data => {
        var transform = {} as { [type: string]: Tag[] };
        _.keys(data).map(function(k: string) {
          transform[k] = data[k] ? _.keys(data[k]).map(key => data[k][key]) : [];
        });
        dispatch({ type: 'RECEIVE_META_TAGS', data: transform });
      });
  }

  if (noMetrics) {
    return;
  }

  let from = formatYMD(fromDate);
  let to = formatYMD(toDate);
  const metricRequestId = new Date().getTime();

  // As metrics are obtained per video, and videos are already being filtered,
  // there is no need to filter the metric list query
  let filtersObj: { type: string, value: string }[] = JSON.parse(filters);

  // When getting metrics, we use only platform filters
  let platformFilters = filtersObj.filter(x => x.type === 'platform');
  let filtersStr = JSON.stringify(platformFilters);

  authenticatedFetch(
    `api/${controller}Data/GetMetricList/${from}/${to}/${encodeURIComponent(filtersStr)}`
  ).then(response => {
    // If unauthorized, like when the user is logging out, this happens
    if (response.redirected || response.status !== 200) {
      return [];
    }
    return response.json() as Promise<VideoMetric[]>;
  })
    .then(data => {
      dispatch({ type: 'RECEIVE_DATA', data: data, pageType, metricRequestId });
    });
  dispatch({ type: 'REQUEST_DATA', controllerType: pageType, metricRequestId });
}

export const actionCreators = {
  requestData: (
    controllerType: ControllerType,
    filters: string,
    fromDate: Date,
    toDate: Date,
    noMetrics: boolean,
    metaTagsOnlyOnce?: boolean,
  ): AppThunkAction<KnownAction> =>
    ((dispatch: Function, getState: () => ApplicationState) =>
      requestDataFunc(
        controllerType,
        filters,
        fromDate,
        toDate,
        noMetrics,
        Boolean(metaTagsOnlyOnce),
        dispatch,
        getState
      )
    ),
  updateVideoData: (
    videos: VideoEdits,
    okCallback: () => void,
    failCallback: () => void): AppThunkAction<KnownAction> => (dispatch, getState) => {
      authenticatedFetch(`api/MarketingData/EditVideos/`, {
        method: 'PUT',
        body: JSON.stringify(videos),
        headers: {
          'Content-Type': 'application/json'
        }
      })
        .then(res => res.json() as Promise<VideoEdits>)
        .then(
          (failedEdits: VideoEdits) => {
            const failedUpdates = _.keys(failedEdits.edits).length;
            if (failedUpdates > 0) {
              failCallback();
              return;
            }
            okCallback();
          },
          failCallback
        );
      // TODO Add some checking in case of failure
      // dispatch({ type: 'REQUEST_MARKETING_DATA' });
    },
  setVideoManagementArchiveMode: (archiveMode: ArchiveMode): AppThunkAction<KnownAction> => (dispatch, getState) => {
    dispatch({ type: 'SET_VIDEO_MANAGEMENT_ARCHIVE_MODE', archiveMode });
  }
};

const unloadedState: DataState = {
  content: [],
  marketing: [],
  isLoading: false,
  videos: [],
  metaTags: {},
  videoManagementArchiveMode: ArchiveMode.UnArchived,
  metricRequestId: {},
};

export const reducer: Reducer<DataState> = (state: DataState, incomingAction: Action) => {
  const action = incomingAction as KnownAction;
  switch (action.type) {
    case 'REQUEST_DATA':
      const { controllerType, metricRequestId } = action;
      return {
        ...state,
        isLoading: true,
        [controllerType]: [],
        metricRequestId: {
          ...state.metricRequestId,
          [controllerType]: metricRequestId
        }
      };
    case 'RECEIVE_DATA': {
      const { metricRequestId, pageType } = action;
      const stateRequestId: number = state.metricRequestId[pageType];
      if (stateRequestId === metricRequestId) {
        return { ...state, isLoading: state.videos.length !== 0, [action.pageType]: action.data };
      }
      return { ...state };
    }
    case 'RECEIVE_VIDEOS':
      return { ...state, isLoading: false, videos: action.data };
    case 'RECEIVE_META_TAGS':
      return { ...state, metaTags: action.data };
    case 'SET_VIDEO_MANAGEMENT_ARCHIVE_MODE':
      return { ...state, videoManagementArchiveMode: action.archiveMode };
    default:
      // The following line guarantees that every action in the KnownAction union has been covered by a case above
      const exhaustiveCheck: never = action;
  }

  return state || unloadedState;
};
