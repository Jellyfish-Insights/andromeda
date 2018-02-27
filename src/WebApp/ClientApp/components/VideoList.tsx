import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as React from 'react';
import { Prompt } from 'react-router';
import * as Select from 'react-select';
import { Option } from 'react-select';
import * as _ from 'underscore';
import {
  arrayToObject,
  capitalizeFirst,
  formatAndAddUnit,
  formatMetricUnit,
} from '../reducers/helpers';
import { FilterItemWithColor, Page, ChartDataItemKey, ControllerType } from '../store/Chart';
import { MetricInfo, Tag, Video, VideoMetric, VideoEdits, MetricType } from '../types';
import { filterSources, VideoItem } from './VideoItem';
import {
  SortingDirection,
  SortingType,
  Sorting,
  getMetricValueContent,
  getMetricValueMarketing,
  sortVideoIdsBy,
  VideoItemStateDictionary,
  VideoDictionary
} from './VideoSort';
import { Pagination, getPageVideoIds, OrderBy } from './Pagination';
import { updateTagToSelectedVideos } from './VideoEdit';
import { FilterTagSelect } from './FilterTagSelect';
import { TagWithValues } from './FilterTagSelect';
import { FilterItem } from '../store/Filter';
import update from 'immutability-helper';
import { Operation, getSelectedVideos } from './admin/VideoManagementEdit';
import { setArchive, mergeVideos } from './Edit';
import { updateFailure } from './AbstractPage';
import { isEqual, inRange } from 'lodash';
import { MetricTooltip } from './MetricTooltip';

export const confirmationMessage = 'There are unsaved changes, are you sure?';
const COLORS = ['#f7f7f7', '#eeeeee', '#e6e6e7'];

function flipOrder(sortingDirection: SortingDirection): SortingDirection {
  switch (sortingDirection) {
    case SortingDirection.Disabled: return SortingDirection.Up;
    case SortingDirection.Up: return SortingDirection.Down;
    case SortingDirection.Down: return SortingDirection.Up;
  }
}

export interface VideoListProps {
  activeMetric: ChartDataItemKey;
  availableMetricsInfo: Array<MetricInfo>;
  personas: Array<FilterItemWithColor>;
  videoMetrics: Array<VideoMetric>;
  videos: Array<Video>;
  metaTags: { [type: string]: Tag[] };
  updateVideoData: (videoEdits: VideoEdits, okCallback: () => void, failCallback: () => void) => void;
  requestData: () => void;
  page: Page;
  hidePagination?: boolean;
  hideSortBy?: boolean;
  hideEditButton?: boolean;
  hideViewModeMetricSelector?: boolean;
  hideMetaTagColumn?: boolean;
  sorting?: Sorting;
  limitOfVideosToShow?: number;
  disableManualSorting?: boolean;
  hideInactiveSortingArrows?: boolean;
  showActiveMetricNameAndPageTypeInHeader?: boolean;
  borderBetweenContentMarketing?: boolean;
  dashboardHeader?: boolean;
  canEdit: boolean;
  showPlaylistsInViewMode: boolean;
  setActiveMetric: (page: string, key: ChartDataItemKey) => void;
  filterHeight: number;
}

export interface VideoItemState extends Video, VideoMetric {
  selected: boolean;
  new?: boolean;
}

interface VideoListState {
  activeMetrics: { [key: string]: boolean };
  metricPage: number;
  personaPage: number;
  editMode: boolean;
  editDirty: boolean;
  videoItemStates: { [videoId: string]: VideoItemState };
  allSelected: boolean;
  sorting: Sorting;
  // SortedVideoIds contains the videos ordered by the current metatag. While in
  // edit mode, changing values of the videos does not trigger a resorting, so
  // we keep this variable to know the last sorted state. Once the user goes out
  // of the edit mode, sorting is resumed and the variable is updated every time
  // the sorting state variable is changed. Clicking on the sorting icons also
  // works to force a resort.
  sortedVideoIds: string[];
  videoPage: number;
  videoEdits: VideoEdits;
  originalVideos: { [videoId: string]: Video };
  saved: boolean;
  resetSelectedInNextUpdate: boolean;
  showMetricDescriptionTooltip: boolean;
  activeMetricDescriptionTooltip?: string;
}

function getPersonasPerPage(page: Page, editMode: boolean) {
  if (editMode) {
    return 1;
  }
  return page === 'marketing' ? 3 : 1;
}

function getMetricsPerPage(page: Page, editMode: boolean, activePersonas: number) {
  switch (page) {
    case 'dashboard':
      return 8;
    case 'marketing':
      if (activePersonas === 1) {
        return 6;
      }
      if (activePersonas === 2) {
        return 4;
      }
      break;
    default:
      break;
  }
  return page !== 'marketing' && !editMode ? 4 : 3;
}

interface MetricOption {
  value: string;
  label: string;
}

export class VideoList extends React.Component<VideoListProps, VideoListState> {
  constructor(props: VideoListProps) {
    super(props);
    this.onUnload = this.onUnload.bind(this);
    const { videos, sorting } = this.props;
    this.state = {
      activeMetrics: {},
      metricPage: 0,
      personaPage: 0,
      editMode: false,
      editDirty: false,
      videoItemStates: {},
      allSelected: false,
      sorting: sorting || { direction: SortingDirection.Down, type: SortingType.Date },
      sortedVideoIds: [],
      videoPage: 0,
      videoEdits: { edits: {} },
      originalVideos: arrayToObject(videos, (v: Video) => v.id),
      saved: false,
      resetSelectedInNextUpdate: false,
      showMetricDescriptionTooltip: false,
    };
  }

  onUnload(event: BeforeUnloadEvent) { // the method that will be used for both add and remove event
    if (this.state.editDirty) {
      event.returnValue = confirmationMessage;
    }
  }

  componentDidMount() {
    window.addEventListener('beforeunload', this.onUnload);
    this.update();
    this.updateMetricPage();
  }

  componentDidUpdate(prevProps: VideoListProps) {
    if (prevProps !== this.props) {
      this.update();
    }

    if (prevProps.personas.length !== this.props.personas.length) {
      this.setState({
        personaPage: 0,
        metricPage: 0
      });
    }

    if (!_.isEqual(prevProps.activeMetric, this.props.activeMetric)) {
      this.updateMetricPage();
    }
  }

  private updateMetricPage() {
    const {
      activeMetric,
      page,
      availableMetricsInfo,
      personas,
    } = this.props;

    const {
      editMode
    } = this.state;

    const orderedMetrics = this.getOrderedActiveMetrics();
    const activeMetricInfo = availableMetricsInfo.find(
      x => x.type === activeMetric.metric && x.pageType === activeMetric.pageType
    );

    let metricsPerPage = getMetricsPerPage(page, editMode, personas.length);

    let metricPage = orderedMetrics.findIndex(m => m.typeId === activeMetricInfo.typeId);
    if (!inRange(metricPage, 0, orderedMetrics.length - metricsPerPage)) {
      metricPage = orderedMetrics.length - metricsPerPage;
    }

    const start = metricPage;
    const end = metricPage + metricsPerPage;
    if (!inRange(metricPage - 1, start, end)) {
      this.setState({ metricPage });
    }
  }

  private async update() {
    // This should check if metric values were updated due call
    // updating without need
    let { videoMetrics, videos } = this.props;
    const { resetSelectedInNextUpdate } = this.state;
    let lastVideoItemStates = this.state.videoItemStates;
    let videoItemStatesArray = videos.map((video) => {
      let lastVideoState = lastVideoItemStates[video.id];
      let selected = !resetSelectedInNextUpdate && Boolean(lastVideoState && lastVideoState.selected);
      return {
        ...video,
        ...(videoMetrics.find(x => x.id === video.id) as VideoMetric),
        selected
      } as VideoItemState;
    });
    let videoItemStates = arrayToObject(videoItemStatesArray, (v: VideoItemState) => v.id);
    let availableVideoIds = _.keys(videoItemStates);
    let originalVideos = arrayToObject(videos, (v: Video) => v.id);
    const inputVideosDidNotChange = isEqual(new Set(this.state.sortedVideoIds), new Set(availableVideoIds));
    const { sorting } = this.props;
    if (inputVideosDidNotChange && !sorting) {
      this.setState((prevState) => {
        const sortedVideoIds = this.sortVideoIdsBy(prevState.sorting, videoItemStates, true) as string[];
        return {
          videoItemStates,
          originalVideos,
          sortedVideoIds,
          resetSelectedInNextUpdate: false
        };
      });
    } else {
      this.setState((prevState) => {
        const { type, direction } = prevState.sorting;
        let sortedVideoIds;
        // preferred method of sorting is from props when updating

        // By default page start with value (type: 1, direction: 1) that is a information that comes
        // from content and is used to sort by 'other' what is the selected chart.
        // In the VideoList component the value for 'other' is (type: 0, direction: 2)
        // so we have to check if is selected when any interaction with the page happens
        if ((type == 0 && direction == 2) || (type == 1 && direction == 1)) {
          // Sort by selected charts
          sortedVideoIds = this.sortVideoIdsBy(sorting, videoItemStates, true) as string[];
        } else {
          sortedVideoIds = this.sortVideoIdsBy(prevState.sorting, videoItemStates, true) as string[];
        }
        return {
          sortedVideoIds,
          videoItemStates,
          videoPage: inputVideosDidNotChange ? prevState.videoPage : 0,
          originalVideos,
          sorting: prevState.sorting,
          resetSelectedInNextUpdate: false
        };
      });
    }
  }

  public componentWillMount() {
    this.updateStateFromProps();
  }

  private applySort(sorting: Sorting) {
    const s = sorting.type;
    const d = sorting.direction;
    const { videoItemStates } = this.state;
    this.setState((prevState) => {
      const sorting = { type: s, direction: d };
      const sortedVideoIds = this.sortVideoIdsBy(sorting, videoItemStates, true) || prevState.sortedVideoIds;
      return { sorting, sortedVideoIds };
    }
    );
  }

  private updateStateFromProps() {
    const { availableMetricsInfo, videoMetrics, videos } = this.props;

    let videoItemStatesArray = videos.map((video) => {
      let thisVideoMetric = videoMetrics.find(x => x.id === video.id) as VideoMetric;
      return {
        ...video,
        ...thisVideoMetric,
        selected: false,
      } as VideoItemState;
    });
    let videoItemStates = arrayToObject(videoItemStatesArray, (v: VideoItemState) => v.id);
    this.setState((prevState) => {
      var sortedVideoIds: string[] = _.keys(videoItemStates);
      if (prevState && prevState.sortedVideoIds && prevState.sorting) {
        let nextSorted = this.sortVideoIdsBy(prevState.sorting, videoItemStates, true);
        if (nextSorted) { (sortedVideoIds = nextSorted); }
      }

      const activeMetrics: { [index: string]: boolean } = _.object(
        availableMetricsInfo.map((metric, index) => [`${index}`, true])
      );

      return {
        activeMetrics,
        metricPage: 0,
        personaPage: 0,
        editMode: false,
        editDirty: false,
        videoItemStates,
        allSelected: prevState && prevState.allSelected,
        sorting: prevState.sorting,
        videoPage: prevState && prevState.videoPage,
        sortedVideoIds,
        videoEdits: { edits: {} },
      };
    });
  }

  public componentWillUnmount() {
    window.removeEventListener('beforeunload', this.onUnload);
  }

  getKeyForMetric(metricInfo: MetricInfo): ChartDataItemKey {
    let { activeMetric, page, filterHeight } = this.props;
    const requestKey: ChartDataItemKey = update(activeMetric, {
      metric: { $set: metricInfo.type },
      pageType: { $set: metricInfo.pageType as ControllerType },
      type: (type: string) => page !== 'dashboard' ? type : (metricInfo.pageType === 'marketing' ? 'persona' : 'length')
    });
    return requestKey;
  }

  public render() {
    let { availableMetricsInfo, page } = this.props;

    const options: MetricOption[] = availableMetricsInfo
      .sort((x, y) => x.typeId - y.typeId)
      .map((m, i) => {
        return { value: `${i}`, label: m.type };
      });

    return (
      <div className="video-list">
        <Prompt
          when={this.state.editDirty}
          message={confirmationMessage}
        />
        {this.renderPersonaHeader(page)}
        {this.renderHeader(options)}
        {this.renderVideos()}
      </div>
    );
  }

  private personaHeaderRef = React.createRef<HTMLDivElement>();
  private dashboardHeaderRef = React.createRef<HTMLDivElement>();

  private renderPersonaHeader(page: Page) {
    const {
      personas,
      hidePagination,
      hideSortBy,
      showActiveMetricNameAndPageTypeInHeader,
      activeMetric,
      dashboardHeader,
      filterHeight
    } = this.props;
    const {
      editMode,
      personaPage,
      sorting,
      videoPage,
      sortedVideoIds,
      videoItemStates,
      showMetricDescriptionTooltip
    } = this.state;
    const showArrows = personas.length > 3 || (editMode && personas.length > 1);
    const personasPerPage = getPersonasPerPage(page, editMode);
    const lastPersonaPage = personas.length - personasPerPage;

    if (dashboardHeader) {
      return (
        <div
          className="dashboard-header"
          ref={this.dashboardHeaderRef}
          style={{ top: filterHeight }}
        >
          <div className="placeholder-1-of-4" />
          <div className="placeholder-3-of-4">
            <div className="placeholder-1-of-2">
              Content
            </div>
            <div className="metricDiv" />
            <div className="placeholder-1-of-2">
              Audiences
            </div>
          </div>
        </div>
      );
    }
    let style: React.CSSProperties = { top: filterHeight };
    style = { ...style, zIndex: 999 };
    if (showMetricDescriptionTooltip) {
      style = { ...style, zIndex: 998 };
    }

    return (
      <div className="persona-header" style={style} ref={this.personaHeaderRef}>
        {!hidePagination && <Pagination
          videoPage={videoPage}
          sortedVideoIds={sortedVideoIds}
          videoItemStates={videoItemStates}
          updateVideoPage={(videoPage: number) => this.setState({ videoPage })}
        />}
        {!hideSortBy &&
          [
            (
              <div style={{ fontWeight: 300, marginRight: 5 }} key={1}>
                Sort by
            </div>
            ),
            (
              <OrderBy
                key={2}
                sorting={sorting}
                applySort={(sorting: Sorting) => this.applySort(sorting)}
              />
            )
          ]
        }
        <div className="placeholder" />
        {showActiveMetricNameAndPageTypeInHeader && (
          <div className="title">
            {capitalizeFirst(activeMetric.pageType)} {activeMetric.metric}
          </div>
        )}
        {!showActiveMetricNameAndPageTypeInHeader && (
          <div className={editMode ? 'personas collapsed' : 'personas'}>
            {
              page === 'marketing' &&

              <React.Fragment>
                {showArrows && (
                  <a
                    className={personaPage === 0 ? 'arrow disabled' : 'arrow'}
                    onClick={() => this.setState({ personaPage: personaPage - 1 })}
                  >
                    <FontAwesomeIcon icon="chevron-left" />
                  </a>
                )}
                {personas
                  .slice(personaPage, personaPage + personasPerPage)
                  .map((persona, index) => (
                    <div
                      className="persona"
                      key={index}
                    >
                      {persona.value}
                    </div>
                  ))}
                {showArrows && (
                  <a
                    className={
                      personaPage === lastPersonaPage ? 'arrow disabled' : 'arrow'
                    }
                    onClick={evt => this.setState({ personaPage: personaPage + 1 })}
                  >
                    <FontAwesomeIcon icon="chevron-right" />
                  </a>
                )}
              </React.Fragment>
            }
          </div>
        )}
      </div>
    );
  }

  private renderHeader(options: Array<Option<string>>) {
    const {
      personas,
      metaTags,
      page,
      hideEditButton,
      hideViewModeMetricSelector,
      filterHeight,
      requestData,
      dashboardHeader
    } = this.props;
    const {
      activeMetrics,
      editMode,
      personaPage,
      videoItemStates,
      allSelected,
      saved
    } = this.state;
    let personasPerPage = getPersonasPerPage(page, editMode);
    let genericTags: Set<string> = new Set();
    _.keys(videoItemStates).forEach(
      videoId => videoItemStates[videoId]
        .tags
        .filter(tag => tag.type.toLowerCase() === 'generic')
        .forEach(genericTag => genericTags.add(genericTag.value))
    );
    let top = this.personaHeaderRef.current && (filterHeight + this.personaHeaderRef.current.clientHeight) || 0;
    if (dashboardHeader) {
      top = this.dashboardHeaderRef.current && (filterHeight + this.dashboardHeaderRef.current.clientHeight + 1) || 0;
    }
    let style: React.CSSProperties = { top };

    return (
      <div className={'list-header' + ' ' + page} style={style}>
        <div className="controls wrap">
          {editMode &&
            <input
              name="Selected"
              type="checkbox"
              className="select-all-checkbox"
              checked={allSelected}
              value=""
              onChange={() => {
                this.setState((prevState) => {
                  let newSelected = !prevState.allSelected;
                  return update(prevState, {
                    allSelected: { $set: newSelected },
                    videoItemStates:
                      (states: VideoItemStateDictionary) =>
                        _.mapObject(
                          states,
                          (state: VideoItemState) => {
                            return update(state, {
                              selected: { $set: newSelected }
                            });
                          }
                        )
                  });
                });
              }}
            />
          }
          {!hideEditButton && <a
            className={editMode ? 'small button' : 'small button hollow'}
            onClick={() => this.toggleEdit()}
          >
            <FontAwesomeIcon icon="pencil-alt" />
          </a>
          }
          {(!editMode && !hideViewModeMetricSelector) &&
            <Select.default
              closeOnSelect={false}
              options={options}
              optionRenderer={(metric: Option<string>) => (
                <div className="metric-option">
                  {metric.label}
                  {activeMetrics[metric.value] && (
                    <FontAwesomeIcon icon="check-circle" />
                  )}
                </div>
              )}
              onChange={metric => this.handleOnChange(metric)}
              placeholder={'Metrics'}
              removeSelected={false}
              simpleValue={true}
            />
          }
          {editMode && <Operation
            callback={() => { this.mergeSelectedVideos(); }}
            icon="clone"
            tooltip="Merge"
          />}
          {editMode && <Operation
            callback={() => this.setArchive(true)}
            icon="archive"
            tooltip="Archive"
          />}
          {editMode && (
            <MetricSelectors
              metaTags={metaTags}
              videoItemStates={videoItemStates}
              submitValue={(type: string, value: string) => { this.updateTagToSelectedVideos(type, value); }}
            />
          )}
          {editMode && (
            <a
              className="button cancel"
              onClick={() => this.updateStateFromProps()}
            >
              Cancel
            </a>
          )}
          {editMode && (
            <a
              className="button update"
              onClick={() => {
                this.props.updateVideoData(
                  this.state.videoEdits,
                  () => {
                    this.setState({ saved: true });
                    setTimeout(
                      () => this.setState({ saved: false }),
                      3000
                    );
                    requestData();
                  },
                  updateFailure
                );
                this.setState({
                  editDirty: false,
                  videoEdits: { edits: {} },
                  resetSelectedInNextUpdate: true
                });
                this.props.requestData();
              }}
              style={saved ? { backgroundColor: 'green' } : {}}
            >
              {saved ? 'Saved' : 'Save'}
            </a>
          )}
        </div>
        <div className={editMode ? 'metrics collapsed' : 'metrics'}>
          {page === 'marketing' && personas
            .slice(personaPage, personaPage + personasPerPage)
            .map((persona, index) => this.getMetrics(persona, index))}
          {page !== 'marketing' && this.getMetrics()}
        </div>
      </div>
    );
  }

  private mergeSelectedVideos() {
    this.setState(prevState => {
      const { videoItemStates, videoEdits, sortedVideoIds } = prevState;
      const selected: VideoItemState[] = getSelectedVideos(sortedVideoIds, videoItemStates);
      const deletedIds = _.tail(selected).map(s => s.id);

      // the first video in the array will be updated
      // the other videos will be deleted
      // needs at least two videos
      if (selected.length < 2) {
        alert('You need at least 2 videos to make a merge operation.');
        return { ...prevState };
      }
      const [mergedVideo, newVideoEdits] = mergeVideos(selected, videoEdits);
      return update(prevState, {
        videoItemStates: (v: VideoItemStateDictionary) => {
          return { ..._.omit(v, deletedIds), [mergedVideo.id]: mergedVideo };
        },
        originalVideos: (v: VideoDictionary) => _.omit(v, deletedIds),
        videoEdits: { $set: newVideoEdits },
        editDirty: { $set: true },
        sortedVideoIds: (ids: string[]) => _.without(ids, ...deletedIds)
      });
    });
  }

  private setArchive(value: boolean) {
    this.setState(prevState => {
      const { videoItemStates } = prevState;
      const selected: VideoItemStateDictionary =
        _.pick(videoItemStates, (state: VideoItemState) => state.selected);
      return update(prevState, {
        videoItemStates: (v: VideoItemStateDictionary) => {
          const updated = _.mapObject(
            selected,
            (state: VideoItemState) => update(state, { archived: { $set: value } })
          );
          return update(v, { $merge: updated });
        },
        videoEdits: (videoEdits: VideoEdits) => setArchive(selected, value, videoEdits),
        editDirty: { $set: true },
      });
    });
  }

  private getOrderedActiveMetrics() {
    let { availableMetricsInfo } = this.props;
    let { activeMetrics } = this.state;
    return _.keys(activeMetrics)
      .filter(key => activeMetrics[key])
      .sort((x, y) => {
        const metric1 = availableMetricsInfo[Number(x)];
        const metric2 = availableMetricsInfo[Number(y)];
        return metric1.typeId - metric2.typeId;
      })
      .map(x => availableMetricsInfo[Number(x)]);
  }

  private getSepIndex() {
    const { borderBetweenContentMarketing } = this.props;
    const orderedActiveMetrics = this.getOrderedActiveMetrics();
    if (!borderBetweenContentMarketing) {
      return -1;
    }
    return orderedActiveMetrics
      .findIndex((metric, index) =>
        metric.pageType === 'marketing' && (index > 0) &&
        orderedActiveMetrics[index - 1].pageType === 'content'
      );
  }
  private getMetrics(persona?: FilterItemWithColor, index?: number) {
    let {
      activeMetric,
      page,
      hideMetaTagColumn,
      availableMetricsInfo,
      disableManualSorting,
      personas,
      setActiveMetric
    } = this.props;
    const activeMetricInfo = availableMetricsInfo.find(
      x => x.type === activeMetric.metric && x.pageType === activeMetric.pageType
    );
    let { editMode, metricPage } = this.state;
    let metricsPerPage = getMetricsPerPage(page, editMode, personas.length);
    const orderedActiveMetrics = this.getOrderedActiveMetrics();
    let showArrows = orderedActiveMetrics.length > metricsPerPage;
    let lastMetricPage = orderedActiveMetrics.length - metricsPerPage;
    let styleWhenHidingMetaTag: React.CSSProperties = { paddingLeft: 0 };
    let metricHeader = (
      orderedActiveMetrics
        .slice(metricPage, metricPage + metricsPerPage)
        .map((metric, index) => {
          const sortingActive: boolean = !this.isMetricSortingInactive(
            SortingType.Metric,
            metric,
            persona && (persona as FilterItemWithColor).value
          );

          return (
            <div
              key={index}
              title={metric.type}
              className="metric-header-item"
            >
              <MetricTooltip
                id={'metric' + MetricType[metric.typeId]}
                prevId={this.state.activeMetricDescriptionTooltip}
                innerPresentation={
                  <span
                    title={formatMetricUnit(metric)}
                    style={sortingActive ? { fontWeight: 700 } : {}}
                  >
                    {metric.type}
                  </span>
                }
                metricInfo={metric}
                className="metric"
                afterShow={() => this.setState((prevState) => {
                  return {
                    showMetricDescriptionTooltip: true,
                    activeMetricDescriptionTooltip: 'metric' + MetricType[metric.typeId],
                  };
                })}
                afterHide={() => this.setState({
                  showMetricDescriptionTooltip: false,
                  activeMetricDescriptionTooltip: undefined,
                })}
              />
              <div
                className="sort-button"
                style={disableManualSorting ? { cursor: 'default' } : {}}
                onClick={() => !disableManualSorting && this.setState((prevState) => {
                  const personaName = persona && (persona as FilterItemWithColor).value;
                  let { direction } = prevState.sorting;
                  let previousMetric = prevState.sorting.metric;
                  let previousPersona = prevState.sorting.persona;

                  if (!previousMetric || previousMetric && !_.isEqual(previousMetric, metric)
                    || (!previousPersona && persona)
                    || (persona && personaName !== previousPersona)) {
                    direction = metric.unit === '$' ? SortingDirection.Up : SortingDirection.Down;
                    setActiveMetric(page, this.getKeyForMetric(metric));
                  } else {
                    direction = flipOrder(direction);
                  }

                  const sorting: Sorting = {
                    direction,
                    type: SortingType.Metric,
                    metric,
                    persona: personaName
                  };

                  const { videoItemStates } = prevState;
                  const sortedVideoIds = this.sortVideoIdsBy(sorting, videoItemStates, true)
                    || prevState.sortedVideoIds;

                  return { sorting, sortedVideoIds };
                })}
              >
                {this.getSortIcon(SortingType.Metric, metric, persona && (persona as FilterItemWithColor).value)}
              </div>
            </div>
          );
        }
        )
    );
    const sepIndex = this.getSepIndex();
    if (sepIndex > 0) {
      metricHeader.splice(
        sepIndex,
        0,
        <div className="metricDiv" key="DIV" style={{ width: '1px' }} />
      );
    }
    const sortingActive: boolean = !this.isMetricSortingInactive(
      SortingType.MetaTag,
      activeMetricInfo
    );
    return (
      <div
        className="persona-metric"
        key={index}
        style={hideMetaTagColumn && styleWhenHidingMetaTag}
      >
        {page !== 'marketing' && !editMode && !hideMetaTagColumn && (
          <div className="meta-tag">
            <span
              className="metric"
              style={sortingActive ? { fontWeight: 700 } : {}}
            >
              {activeMetric && capitalizeFirst(activeMetric.type)}
            </span>
            <div
              className="sort-button"
              title="Sort by MetaTag"
              style={disableManualSorting && { cursor: 'default' }}
              onClick={() => !disableManualSorting && this.setState((prevState) => {
                const { direction } = prevState.sorting;
                const sorting: Sorting = {
                  direction: flipOrder(direction),
                  type: SortingType.MetaTag,
                  metric: activeMetricInfo
                };
                const { videoItemStates } = prevState;
                const sortedVideoIds = this.sortVideoIdsBy(sorting, videoItemStates, true) || prevState.sortedVideoIds;
                return { sorting, sortedVideoIds };
              })}
            >
              {this.getSortIcon(SortingType.MetaTag, activeMetricInfo)}
            </div>
          </div>
        )}
        <a
          className={'arrow' + (page !== 'marketing' ? ' content' : '')}
          onClick={() => showArrows
            && this.setState(metricPage === 0 ? { metricPage: lastMetricPage } : { metricPage: metricPage - 1 })}
          style={!showArrows ? { opacity: 0 } : {}}
        >
          <FontAwesomeIcon icon="chevron-left" />
        </a>
        {metricHeader}
        <a
          className={'arrow'}
          onClick={() => showArrows
            && this.setState(metricPage === lastMetricPage ? { metricPage: 0 } : { metricPage: metricPage + 1 })}
          style={!showArrows ? { opacity: 0 } : {}}
        >
          <FontAwesomeIcon icon="chevron-right" />
        </a>
      </div>
    );
  }

  /** The input parameter serves to compare with the current state
   * if the sorting parameter matches the type with the state, it gets
   * the proper sorting arrow, otherwise the default one.
   */
  private getSortIcon(typeIn: SortingType, metricIn?: MetricInfo, personaIn?: string): JSX.Element {
    const { hideInactiveSortingArrows } = this.props;
    const { direction } = this.state.sorting;
    let icon: IconProp = 'sort-down';
    let className = 'down';
    let color = 'grey';
    if (metricIn && metricIn.unit === '$') {
      icon = 'sort-up';
      className = 'up';
    }
    if (this.isMetricSortingInactive(typeIn, metricIn, personaIn)) {
      return (
        <FontAwesomeIcon
          icon={icon as IconProp}
          style={{
            color,
            opacity: hideInactiveSortingArrows ? 0 : 1,
          }}
          className={className}
        />
      );
    }
    if (direction === SortingDirection.Up) {
      color = 'black';
      icon = 'sort-up';
      className = 'up';
    }
    if (direction === SortingDirection.Down) {
      color = 'black';
      icon = 'sort-down';
      className = 'down';
    }
    return <FontAwesomeIcon icon={icon as IconProp} style={{ color }} className={className} />;
  }

  private isMetricSortingInactive(typeIn: SortingType, metricIn?: MetricInfo, personaIn?: string): boolean {
    let { type, metric, persona } = this.state.sorting;
    return (
      typeIn !== type
      || (metricIn.type !== metric.type || metricIn.pageType !== metric.pageType)
      // commented out because metricList needed to be clicked twice to show it was selected(bold)
      // || personaIn !== persona
    );
  }

  private updateTagToSelectedVideos(type: string, value: string) {
    const { videoItemStates, videoEdits, originalVideos } = this.state;
    updateTagToSelectedVideos(
      type,
      value,
      videoItemStates,
      videoEdits,
      originalVideos,
      (updatedVideoObject: VideoItemStateDictionary, videoEdits: VideoEdits) => {
        this.setState((prevState) => {
          return {
            editDirty: true,
            videoItemStates: { ...prevState.videoItemStates, ...updatedVideoObject },
            videoEdits
          };
        });
      });
  }

  private toggleEdit() {
    if (this.state.editMode && this.state.editDirty) {
      if (confirm(confirmationMessage)) {
        this.updateStateFromProps();
      }
    } else {
      this.setState((prevState) => {
        let sortedVideoIds = this.sortVideoIdsBy(
          prevState.sorting,
          this.state.videoItemStates,
          true
        ) || prevState.sortedVideoIds;
        return { editMode: !this.state.editMode, sortedVideoIds };
      });
    }
  }

  private toggleSelected(videoId: string, forceSelected: boolean | null) {
    this.setState((prevState) => {
      let current = prevState.videoItemStates[videoId];
      let selected = forceSelected !== null ? forceSelected : !current.selected;
      return update(prevState, {
        videoItemStates: {
          [videoId]: { selected: { $set: selected } }
        },
      });
    });
  }

  private sortVideoIdsBy(
    sorting: Sorting,
    videoItemStates: { [videoId: string]: VideoItemState },
    force?: boolean
  ) {
    let { activeMetric } = this.props;
    let { editMode, sortedVideoIds } = this.state;

    return sortVideoIdsBy(
      sorting,
      sortedVideoIds,
      videoItemStates,
      editMode,
      activeMetric.type,
      force
    );
  }

  private renderVideos() {
    let { personas, page, limitOfVideosToShow, showPlaylistsInViewMode } = this.props;
    let { editMode, personaPage, videoItemStates, sortedVideoIds, videoPage, videoEdits, originalVideos } = this.state;
    let personasPerPage = getPersonasPerPage(page, editMode);
    let availableSortedVideoIds = getPageVideoIds(sortedVideoIds, videoItemStates, videoPage);
    if (limitOfVideosToShow) {
      availableSortedVideoIds = sortedVideoIds.slice(0, limitOfVideosToShow);
    }

    return (
      <div className="list-container">
        {availableSortedVideoIds.reduce(
          (components, videoId) => {
            const index = components.length;
            const sources = filterSources(videoItemStates[videoId].sources);
            const state = { ...videoItemStates[videoId], sources };
            if (sources.reduce((acc, curr) => acc + curr.videosCount, 0) > 0) {
              components.push(
                <div key={index} className="video-container">
                  <VideoItem
                    canEdit={this.props.canEdit}
                    key={videoId}
                    original={originalVideos[videoId]}
                    editMode={editMode}
                    videoEdits={videoEdits}
                    state={state}
                    updateVideoItemState={
                      (updatedVideoItem: VideoItemState, videoEdits: VideoEdits) => {
                        this.setState((prevState) => {
                          return {
                            ...prevState,
                            editDirty: true,
                            videoItemStates: {
                              ...prevState.videoItemStates,
                              [videoId]: updatedVideoItem
                            },
                            videoEdits,
                            updating: true
                          };
                        });
                      }}
                    resetForceSelected={() => this.setState({ allSelected: false })}
                    toggleSelected={() => this.toggleSelected(videoId, null)}
                    onDrop={() => { return; }}
                    showPlaylistsInViewMode={showPlaylistsInViewMode}
                  />
                  <div
                    className={
                      this.state.editMode
                        ? 'video-metrics collapsed'
                        : 'video-metrics'
                    }
                  >
                    {page === 'marketing' && personas
                      .slice(personaPage, personaPage + personasPerPage)
                      .map((persona, index) => this.renderVideoMetrics(videoId, persona, index + 1))}
                    {page !== 'marketing' && this.renderVideoMetrics(videoId)}
                  </div>
                </div>
              );
            }
            return components;
          },
          []
        )}
      </div>
    );
  }

  private renderVideoMetrics(videoId: string, persona?: FilterItemWithColor, index?: number) {
    let { page, hideMetaTagColumn, personas, videoMetrics } = this.props;
    let { editMode, metricPage, videoItemStates } = this.state;
    let metricsPerPage = getMetricsPerPage(page, editMode, personas.length);
    const orderedActiveMetrics = this.getOrderedActiveMetrics();
    const loading = videoMetrics.length === 0;

    let video = videoItemStates[videoId];
    if (!video) { return; }
    let style: React.CSSProperties = {};
    if (index) {
      style.backgroundColor = COLORS[index - 1];
    }

    let metrics = (
      orderedActiveMetrics
        .slice(metricPage, metricPage + metricsPerPage)
        .map((metric, index): JSX.Element => {
          const value = page !== 'marketing' ?
            getMetricValueContent(video, metric) :
            getMetricValueMarketing(video, (persona as FilterItemWithColor).value, metric);
          let tooltipValue = formatAndAddUnit(
            metric,
            value,
            {
              normalizeBigNumber: false,
              hourFormat: 'HMS'
            }
          );
          return (
            <div
              key={index}
              title={tooltipValue}
              className="metric"
            >
              {loading ? '...' : formatAndAddUnit(
                metric,
                value,
                {
                  normalizeBigNumber: true,
                  hourFormat: 'auto'
                }
              )}
            </div>
          );
        }
        )
    );
    const sepIndex = this.getSepIndex();
    if (sepIndex > 0) {
      metrics.splice(
        sepIndex,
        0,
        <div className="metricDiv" key="DIV" />
      );
    }

    return (
      <div
        key={index}
        className="video-metric"
        style={style}
      >
        {page !== 'marketing' && !editMode && !hideMetaTagColumn && (
          <div className="metric content">
            {this.getActiveMetaTagValueForVideo(video)}
          </div>
        )}
        {metrics}
      </div>
    );
  }

  private getActiveMetaTagValueForVideo(videoItemState: VideoItemState): string {
    let tag = videoItemState.tags.find(x => x.type.toLowerCase() === this.props.activeMetric.type);
    return tag ? tag.value : '';
  }

  // tslint:disable-next-line:no-any
  private handleOnChange(metric: any) {
    this.setState((prevState) => {
      const nextState = update(prevState, {
        activeMetrics: { $toggle: [metric] }
      });
      const activeMetricKeys: string[] = _.keys(_.pick(
        nextState.activeMetrics,
        (value: boolean) => value
      ));
      if (activeMetricKeys.length >= 1) {
        return nextState;
      }
      return { ...prevState };
    });
  }
}

export interface MetricSelectorsProps {
  metaTags: { [type: string]: Tag[] };
  videoItemStates: VideoItemStateDictionary;
  submitValue: (type: string, value: string) => void;
}

export const MetricSelectors = (props: MetricSelectorsProps) => {
  const { metaTags, videoItemStates, submitValue } = props;

  let genericTags: Set<string> = new Set<string>();
  metaTags.Generic.forEach(x => genericTags.add(x.value));
  _.keys(videoItemStates).forEach(
    videoId => videoItemStates[videoId]
      .tags
      .filter(tag => tag.type === 'Generic')
      .forEach(genericTag => genericTags.add(genericTag.value))
  );

  const videoIds = _.keys(videoItemStates);
  const selectedVideos = videoIds.filter(videoId => videoItemStates[videoId].selected);

  let tagWithValues: TagWithValues[];
  let genericTagArray: Tag[] = Array.from(genericTags).map(value => {
    return { type: 'Generic', value } as Tag;
  });
  let tags: { [type: string]: Tag[] } = { ...metaTags, 'Generic': genericTagArray };
  tagWithValues = _.keys(tags).map((t: string) => {
    const values = tags[t].map(({ type, value }) => {
      const active = selectedVideos.length > 0 &&
        selectedVideos
          .map(videoId =>
            videoItemStates[videoId]
              .tags
              .some(tag => (tag.type.toLowerCase() === type.toLowerCase()) && (tag.value === value))
          )
          .every((x: boolean) => x);
      return { type, value, active } as FilterItem;
    });

    return {
      type: t,
      values
    };
  });
  return (
    <FilterTagSelect
      key={0}
      tagWithValues={tagWithValues}
      submitValue={submitValue}
      doNotShowLabelIcon={true}
      activePlaceholder="Select a tag"
      inactivePlaceholder="Select a Video First"
      disabled={selectedVideos.length === 0}
      showIfActive={true}
      highlightFirstSuggestion={true}
    />
  );
};
