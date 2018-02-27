import { addTask } from 'domain-task';
import { authenticatedFetch } from '../ConnectionUtil';
import { Action, Reducer } from 'redux';
import { AppThunkAction } from '.';
import { Tag, MetricInfo } from '../types';
import * as _ from 'underscore';
import { compareString, capitalizeFirst } from '../reducers/helpers';
import { ControllerType, Page } from '../store/Chart';
import { ApplicationState } from '.';
import update from 'immutability-helper';

export interface FilterItem {
  type: string;
  value: string;
  active: boolean;
}

export function sortFilters(filters: FilterItem[]): FilterItem[] {
  return filters
    .filter(x => x.active)
    .sort((x, y) => compareString(x.value, y.value))
    .sort((x, y) => {
      return compareString(x.type, y.type);
    }
    );
}

export function sortFiltersWithSearchFirst(filters: FilterItem[]): FilterItem[] {
  return filters
    .sort((x, y) => compareString(x.value, y.value))
    .sort((x, y) => {
      return -compareString(x.type, y.type);
    }
    );
}

export interface FilterState {
  isFilterLoading: boolean;
  platforms: string[];
  tags: Tag[];
  personas: string[];
  playlists: string[];
  filters: FilterItem[];
  showSelector: boolean;
  availableMetricsInfo: MetricInfo[];
  availableDashboardMetricsInfo: MetricInfo[];
}

interface RequestFiltersAction {
  type: 'REQUEST_FILTERS';
}

interface ReceiveFiltersAction {
  type: 'RECEIVE_FILTERS';
  platforms: string[];
  tags: Tag[];
  playlists: string[];
  sources: string[];
  personas: string[];
}

interface SubmitSearchBarValueAction {
  type: 'SUBMIT_SEARCH_BAR_VALUE';
  value: string;
}

interface SetAllFiltersAction {
  type: 'SET_ALL_FILTERS';
}

export interface SetFilterAction {
  type: 'SET_FILTER';
  tagType: string;
  tagValue: string;
  active: boolean;
}

interface SetMetricInfoAction {
  type: 'SET_METRIC_INFO';
  metricInfo: MetricInfo[];
}

interface SetDashboardMetricInfoAction {
  type: 'SET_DASHBOARD_METRIC_INFO';
  metricInfo: MetricInfo[];
}

type KnownAction = RequestFiltersAction
  | ReceiveFiltersAction
  | SetAllFiltersAction
  | SetFilterAction
  | SubmitSearchBarValueAction
  | SetMetricInfoAction
  | SetDashboardMetricInfoAction;

export function requestMetricInfo(
  controllerType: ControllerType,
  dispatch: Function,
  getState: () => ApplicationState
) {
  let controller = capitalizeFirst(controllerType);
  let availableMetricsInfo = getState().filter.availableMetricsInfo;
  let availableMetricsInfoForPage: boolean = availableMetricsInfo
    .filter(x => x.type === controllerType).length !== 0;
  if (!availableMetricsInfoForPage) {
    authenticatedFetch(`api/${controller}Data/GetMetricInfo`)
      .then(response => response.json() as Promise<{ metricInfo: MetricInfo[] }>)
      .then(data => {
        dispatch({ type: 'SET_METRIC_INFO', metricInfo: data.metricInfo });
      });
  }
}

export const actionCreators = {
  requestFilterData: (
    page: Page,
    onlyVideosIgnorePageType: boolean
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {

    if (!onlyVideosIgnorePageType) {
      if (page !== 'dashboard') {
        requestMetricInfo(page as ControllerType, dispatch, getState);
      } else {
        authenticatedFetch(`api/Dashboard/GetMetricInfo`)
          .then(response => {
            // If unauthorized, like when the user is logging out, this happens
            if (response.redirected || response.status !== 200) {
              return [];
            }
            return response.json() as Promise<MetricInfo[]>;
          })
          .then(data => {
            dispatch({ type: 'SET_DASHBOARD_METRIC_INFO', metricInfo: data });
          });
      }
    }

    if (getState().filter.filters.length === 0) {
      // We only need to get the filters once;
      authenticatedFetch(`api/MarketingData/GetFilters`)
        .then(response => response.json() as Promise<{
          platforms: string[],
          tags: Tag[],
          playlists: string[],
          sources: string[],
          personas: string[]
        }>
        )
        .then(data => {
          dispatch({
            type: 'RECEIVE_FILTERS',
            platforms: data.platforms,
            tags: data.tags,
            playlists: data.playlists,
            sources: data.sources,
            personas: data.personas
          });
        });

      dispatch({ type: 'REQUEST_FILTERS' });
    }
  },
  setAllFilters: () => <SetAllFiltersAction> { type: 'SET_ALL_FILTERS' },
  setFilter: (tagType: string, tagValue: string, active: boolean) => <SetFilterAction> {
    type: 'SET_FILTER',
    tagType: tagType,
    tagValue: tagValue,
    active: active
  },
  submitSearchBarValue: (value: string) => <SubmitSearchBarValueAction> { type: 'SUBMIT_SEARCH_BAR_VALUE', value },
};

const unloadedState: FilterState = {
  platforms: [],
  tags: [],
  playlists: [],
  personas: [],
  isFilterLoading: false,
  filters: [],
  showSelector: false,
  availableMetricsInfo: [],
  availableDashboardMetricsInfo: []
};

export const reducer: Reducer<FilterState> = (state: FilterState, incomingAction: Action) => {
  const action = incomingAction as KnownAction;
  switch (action.type) {
    case 'REQUEST_FILTERS':
      return { ...state, isFilterLoading: true };
    case 'RECEIVE_FILTERS': {
      const { tags, playlists } = action;
      const personas = _.uniq(action.personas);
      const platforms = _.uniq(action.platforms);
      let filters = platforms.map((x: string): FilterItem => {
        return { type: 'platform', value: x, active: false };
      }).concat(tags.map((x: Tag): FilterItem => {
        return { type: x.type, value: x.value, active: false };
      })).concat(playlists.map((x: string): FilterItem => {
        return { type: 'playlist', value: x, active: false };
      }));
      return {
        ...state,
        isFilterLoading: false,
        platforms,
        tags,
        playlists,
        personas,
        filters
      };
    }
    case 'SET_ALL_FILTERS':
      return { ...state, filters: state.filters.map((x: FilterItem): FilterItem => ({ ...x, active: true })) };
    case 'SET_FILTER': {
      // In case unsetting a search filter, removes it from the list
      if (action.tagType === 'search' && !action.active) {
        return {
          ...state,
          filters: state.filters.filter((x: FilterItem) => !(x.type === 'search' && action.tagValue === x.value))
        };
      }
      // Set filter works both for unsetting and setting, depending on the value of the active argument
      return {
        ...state,
        filters: state
          .filters
          .map((x: FilterItem) =>
            (action.tagType === x.type && action.tagValue === x.value)
              ? { ...x, active: action.active }
              : x
          )
      };
    }
    case 'SUBMIT_SEARCH_BAR_VALUE': {
      // Make sure to only add values if they were not already added
      let values =
        action.value
          .split('OR')
          .map(x => x.trim())
          .filter(x => x !== '');
      let alreadyInSearch = state.filters.filter(x => x.type === 'search').map(x => x.value);
      let newValues = values.filter(x => !_.contains(alreadyInSearch, x));
      let nextFiltersState = sortFiltersWithSearchFirst(
        state.filters.concat(newValues.map((x: string) => ({ type: 'search', value: x, active: true })))
      );
      return {
        ...state,
        filters: nextFiltersState
      };
    }
    case 'SET_METRIC_INFO':
      if (state.availableMetricsInfo) {
        return {
          ...state,
          availableMetricsInfo: _.uniq(state.availableMetricsInfo.concat(action.metricInfo), (m) => m.type + m.pageType)
        };
      }
      return { ...state, availableMetricsInfo: action.metricInfo };
    case 'SET_DASHBOARD_METRIC_INFO':
      return update(state, {
        availableDashboardMetricsInfo: { $set: action.metricInfo }
      });
    default:
      const exhaustiveCheck: never = action;
  }

  return state || unloadedState;
};
