import * as React from 'react';
import update from 'immutability-helper';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { VideoItem, SourceComponent, SourceProps } from '../VideoItem';
import { Video, VideoEdits, Tag, SourceObjectType, Source, SourceObject, ArchiveMode } from '../../types';
import { VideoItemState, MetricSelectors, confirmationMessage } from '../VideoList';
import { arrayToObject, compareString } from '../../reducers/helpers';
import {
  SortingDirection,
  SortingType,
  Sorting,
  sortVideoIdsBy,
  VideoDictionary,
  VideoItemStateDictionary
} from '../VideoSort';
import { Pagination, OrderBy, getPageVideoIds } from '../Pagination';
import * as _ from 'underscore';
import { updateTagToSelectedVideos } from '../VideoEdit';
import { Title } from '../Title';
import { Unassociated } from './VideoManagement';
import {
  ConnectDropTarget,
  DropTarget,
  DropTargetMonitor,
  DragDropContext
} from 'react-dnd';
import HTML5Backend from 'react-dnd-html5-backend';
import { moveSource, onlyRemoveSource, onlyAddSource, createNewVideo, setArchive, mergeVideos } from '../Edit';
import * as Select from 'react-select';
import { Option } from 'react-select';
import { Prompt } from 'react-router';
import { updateFailure } from '../AbstractPage';
import { isEqual } from 'lodash';

const archiveSelectOptions = [ArchiveMode.All, ArchiveMode.Archived, ArchiveMode.UnArchived]
  .map(value => { return { value, label: ArchiveMode[value] }; });

export interface VideoManagementEditProps {
  videos: Video[];
  metaTags: { [type: string]: Tag[] };
  updateVideoData: (videoEdits: VideoEdits, okCallback: () => void, failCallback: () => void) => void;
  requestData: () => void;
  unassociated: Unassociated;
  archiveMode: ArchiveMode;
  setArchiveMode: (mode: ArchiveMode) => void;
  canEdit: boolean;
  filterHeight: number;
}

export interface VideoManagementEditState {
  videoPage: number;
  sortedVideoIds: string[];
  videoItemStates: VideoItemStateDictionary;
  sorting: Sorting;
  originalVideos: VideoDictionary;
  videoEdits: VideoEdits;
  allSelected: boolean;
  editDirty: boolean;
  associationsToShow: SourceObjectType;
  unassociated: Unassociated;
  newIdCounter: number;
  search: string;
  saved: boolean;
  resetSelectedInNextUpdate: boolean;
}

@DragDropContext(HTML5Backend)
export class VideoManagementEdit extends React.Component<VideoManagementEditProps, VideoManagementEditState> {
  constructor(props: VideoManagementEditProps) {
    super(props);
    this.onUnload = this.onUnload.bind(this);
    const { unassociated } = this.props;
    this.state = {
      videoPage: 0,
      sortedVideoIds: [],
      videoItemStates: {},
      sorting: { direction: SortingDirection.Down, type: SortingType.Date },
      originalVideos: {},
      videoEdits: { edits: {} },
      allSelected: false,
      editDirty: false,
      associationsToShow: SourceObjectType.Video,
      unassociated,
      newIdCounter: 0,
      search: '',
      saved: false,
      resetSelectedInNextUpdate: false
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
  }

  componentDidUpdate(prevProps: VideoManagementEditProps) {
    if (prevProps !== this.props) {
      this.update();
    }
  }

  private handleDrop(targetVideoId: string, item: SourceProps) {
    const { sourceObject, originId } = item;
    const { videoItemStates } = this.state;

    const targetVideo = videoItemStates[targetVideoId];
    const targetSources = targetVideo.sources;

    if (!originId) {
      this.setState(
        update(this.state, {
          videoItemStates: {
            [targetVideoId]: {
              sources: {
                $set: addSourceObject(targetSources, sourceObject)
              }
            }
          },
          videoEdits: {
            $apply: (videoEdits: VideoEdits) => onlyAddSource(targetVideo, sourceObject, videoEdits)
          },
          editDirty: { $set: true },
          unassociated: {
            [sourceObject.type]: {
              $apply: (sourceObjects: SourceObject[]) => trimSourceObject(sourceObjects, sourceObject)
            }
          }
        }),
      );
      return;
    }

    if (originId === targetVideoId) {
      return;
    }

    const originalVideo = videoItemStates[originId];
    const originSources = originalVideo.sources;

    this.setState(
      update(this.state, {
        videoItemStates: {
          [originId]: {
            sources: {
              $set: removeSourceObject(originSources, sourceObject)
            }
          },
          [targetVideoId]: {
            sources: {
              $set: addSourceObject(targetSources, sourceObject)
            }
          }
        },
        videoEdits: {
          $apply: (videoEdits: VideoEdits) => moveSource(originalVideo, targetVideo, sourceObject, videoEdits)
        },
        editDirty: { $set: true },
      }),
    );
  }

  private handleDropOnUnassociated(item: SourceProps) {
    const { sourceObject, originId } = item;
    const { videoItemStates } = this.state;

    // If there is no video id, the drop was from unassociated => unassociated,
    // so we need to do nothing
    if (!originId) {
      return;
    }

    const originalVideo = videoItemStates[originId];
    const originSources = originalVideo.sources;

    this.setState(
      update(this.state, {
        videoItemStates: {
          [originId]: {
            sources: {
              $set: removeSourceObject(originSources, sourceObject)
            }
          }
        },
        videoEdits: {
          $apply: (videoEdits: VideoEdits) => onlyRemoveSource(originalVideo, sourceObject, videoEdits)
        },
        editDirty: { $set: true },
        unassociated: {
          [sourceObject.type]: { $push: [sourceObject] }
        }
      }),
    );
  }

  private handleDropOnNew(item: SourceProps) {
    const { sourceObject, originId } = item;
    const { videoItemStates, newIdCounter } = this.state;

    const newVideoId = `NEW-VIDEO-${newIdCounter}`;
    const [newVideo, newVideoEdit] = createNewVideo(newVideoId, sourceObject);

    if (!originId) {
      this.setState(
        update(this.state, {
          videoItemStates: {
            $merge: { [newVideoId]: newVideo }
          },
          originalVideos: {
            $merge: { [newVideoId]: newVideo as Video }
          },
          videoEdits: {
            edits: {
              $merge: { [newVideoId]: newVideoEdit }
            }
          },
          editDirty: { $set: true },
          unassociated: {
            [sourceObject.type]: {
              $apply: (sourceObjects: SourceObject[]) => trimSourceObject(sourceObjects, sourceObject)
            }
          },
          newIdCounter: { $apply: (x: number) => x + 1 },
          sortedVideoIds: (ids: string[]) => [newVideoId].concat(ids),
        }),
      );
      return;
    }

    const originalVideo = videoItemStates[originId];
    const originSources = originalVideo.sources;

    this.setState(
      update(this.state, {
        videoItemStates: {
          [originId]: {
            sources: {
              $set: removeSourceObject(originSources, sourceObject)
            }
          },
          [newVideoId]: { $set: newVideo }
        },
        originalVideos: {
          $merge: { [newVideoId]: newVideo as Video }
        },
        videoEdits: {
          $apply: (videoEdits: VideoEdits) => onlyRemoveSource(
            originalVideo,
            sourceObject,
            update(videoEdits, {
              edits: {
                $merge: { [newVideoId]: newVideoEdit }
              }
            })
          ),
        },
        editDirty: { $set: true },
        newIdCounter: { $apply: (x: number) => x + 1 },
        sortedVideoIds: (ids: string[]) => [newVideoId].concat(ids),
      }),
    );
  }
  private async update() {
    const { videos } = this.props;
    const { resetSelectedInNextUpdate } = this.state;
    const lastVideoItemStates = this.state.videoItemStates;
    const videoItemStatesArray = this.props.videos.map((video) => {
      const lastVideoState = lastVideoItemStates[video.id];
      const selected = !resetSelectedInNextUpdate && Boolean(lastVideoState && lastVideoState.selected);
      return {
        ...video,
        totalMetrics: [],
        metricsPerPersona: [],
        selected,
      } as VideoItemState;
    });
    const videoItemStates = arrayToObject(videoItemStatesArray, (v: VideoItemState) => v.id);
    const availableVideoIds = _.keys(videoItemStates);
    const originalVideos = arrayToObject(videos, (v: Video) => v.id);
    const inputVideosDidNotChange = isEqual(new Set(this.state.sortedVideoIds), new Set(availableVideoIds));
    if (inputVideosDidNotChange) {
      this.setState({
        videoItemStates,
        originalVideos,
        resetSelectedInNextUpdate: false
      });
    } else {
      this.setState((prevState) => {
        const sortedVideoIds = this.sortVideoIdsBy(prevState.sorting, true, videoItemStates) as string[];
        return {
          sortedVideoIds,
          videoItemStates,
          videoPage: 0,
          originalVideos,
          resetSelectedInNextUpdate: false
        };
      });
    }
  }

  private updateStateFromProps() {
    const { videos } = this.props;

    let videoItemStatesArray = videos.map((video) => {
      return {
        ...video,
        totalMetrics: [],
        metricsPerPersona: [],
        selected: false,
      } as VideoItemState;
    });
    let videoItemStates = arrayToObject(videoItemStatesArray, (v: VideoItemState) => v.id);
    this.setState((prevState) => {
      var sortedVideoIds: string[] = _.keys(videoItemStates);
      if (prevState && prevState.sortedVideoIds && prevState.sorting) {
        let nextSorted = this.sortVideoIdsBy(prevState.sorting, true, videoItemStates);
        if (nextSorted) { (sortedVideoIds = nextSorted); }
      }

      return {
        videoPage: prevState && prevState.videoPage,
        sortedVideoIds,
        videoItemStates,
        sorting: prevState.sorting,
        allSelected: prevState && prevState.allSelected,
        videoEdits: { edits: {} },
        editDirty: false,
      };
    });
  }

  private applySort(sorting: Sorting) {
    const s = sorting.type;
    const d = sorting.direction;
    this.setState((prevState) => {
      let sorting = { type: s, direction: d };
      let sortedVideoIds = this.sortVideoIdsBy(sorting, true) || prevState.sortedVideoIds;
      return { sorting, sortedVideoIds };
    }
    );
  }

  private sortVideoIdsBy(sorting: Sorting, force?: boolean, newVideoItemStates?: VideoItemStateDictionary): string[] {
    let { videoItemStates, sortedVideoIds } = this.state;

    return sortVideoIdsBy(
      sorting,
      sortedVideoIds,
      newVideoItemStates || videoItemStates,
      false,
      undefined,
      force
    ) || sortedVideoIds;
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

  private paginationSortTagsRef = React.createRef<HTMLDivElement>();

  public render() {
    const {
      metaTags,
      updateVideoData,
      requestData,
      archiveMode,
      setArchiveMode,
      canEdit,
      filterHeight
    } = this.props;
    const {
      sortedVideoIds,
      videoItemStates,
      videoPage,
      originalVideos,
      videoEdits,
      sorting,
      editDirty,
      allSelected,
      associationsToShow,
      unassociated,
      search,
      saved,
    } = this.state;
    let availableSortedVideoIds = getPageVideoIds(sortedVideoIds, videoItemStates, videoPage);

    const updateButtons = (
      <CancelAndSaveButtons
        show={canEdit}
        enabled={editDirty || saved}
        saved={saved}
        cancel={() => this.updateStateFromProps()}
        update={() => {
          if (this.state.unassociated[SourceObjectType.Video].length > 0) {
            alert('You should not have unassociated videos! Associate them before saving.');
            return;
          }
          updateVideoData(
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
          requestData();
        }}
      />
    );

    let unassociatedFiltered = unassociated[associationsToShow]
      .sort((x, y) => x.title.localeCompare(y.title));
    if (search.length > 0) {
      unassociatedFiltered = unassociatedFiltered
        .filter(x => x.title.toLowerCase().search(search.toLowerCase()) >= 0);
    }

    let videoOperationsTop = this.paginationSortTagsRef.current &&
      (this.paginationSortTagsRef.current.clientHeight + filterHeight) || 0;

    return (
      <div className="container">
        <Prompt
          when={this.state.editDirty}
          message={confirmationMessage}
        />
        <Title
          title="Video Management"
        />
        <div className="page-body">
          <div className="admin-video-container">
            <div className="pagination-sort-tags" style={{ top: filterHeight }} ref={this.paginationSortTagsRef}>
              <Pagination
                videoPage={videoPage}
                sortedVideoIds={sortedVideoIds}
                videoItemStates={videoItemStates}
                updateVideoPage={(videoPage: number) => this.setState({ videoPage })}
              />
              <div style={{ fontWeight: 300, marginRight: 10 }}>
                Sort by
            </div>
              <OrderBy
                sorting={sorting}
                applySort={(sorting: Sorting) => this.applySort(sorting)}
              />
              <div className="placeholder" />
              {this.props.canEdit &&
                <MetricSelectors
                  metaTags={metaTags}
                  videoItemStates={videoItemStates}
                  submitValue={(type: string, value: string) => { this.updateTagToSelectedVideos(type, value); }}
                />
              }
            </div>

            <div className="video-operations" style={{ top: videoOperationsTop }}>
              {this.props.canEdit &&
                <React.Fragment>
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
                  <Operation
                    callback={() => { this.mergeSelectedVideos(); }}
                    icon="clone"
                    tooltip="Merge"
                  />
                  <Operation
                    callback={() => this.setArchive(true)}
                    icon="archive"
                    tooltip="Archive"
                  />
                  <Operation
                    callback={() => this.setArchive(false)}
                    icon="inbox"
                    tooltip="Unarchive"
                  />
                </React.Fragment>
              }
              <div className="placeholder" />
              <div className="archive-selector">
                <div className="wrap">
                  <Select.default
                    value={archiveMode}
                    options={archiveSelectOptions}
                    clearable={false}
                    optionRenderer={(optionValue: Option<ArchiveMode>) => (
                      <div className="metric-option" style={{ fontSize: '12px' }}>
                        {optionValue.label}
                      </div>
                    )}
                    style={{ fontSize: '12px', margin: 2, padding: 0, height: 20 }}
                    // tslint:disable-next-line:no-any
                    onChange={(newValue: any) => {
                      if (newValue !== null) {
                        setArchiveMode(newValue);
                      }
                    }}
                    simpleValue={true}
                  />
                </div>
              </div>
              {updateButtons}
            </div>
            {this.props.canEdit &&
              <NewVideo
                associationsToShow={associationsToShow}
                onDrop={(item) => this.handleDropOnNew(item)}
              />
            }
            <div className="video">
              <div className="list-container" style={{ width: '100%' }}>
                <div className="video-list">
                  <div className="list-container">
                    {availableSortedVideoIds.map((videoId, index) => (
                      <div key={videoId}>
                        <div className="video-container">
                          <VideoItem
                            canEdit={this.props.canEdit}
                            key={videoId}
                            original={originalVideos[videoId]}
                            editMode={true}
                            videoEdits={videoEdits}
                            state={videoItemStates[videoId]}
                            updateVideoItemState={
                              (updatedVideoItem: VideoItemState, videoEdits: VideoEdits) => {
                                this.setState((prevState) => {
                                  return update(prevState, {
                                    editDirty: { $set: true },
                                    videoItemStates: {
                                      [videoId]: { $set: updatedVideoItem }
                                    },
                                    videoEdits: { $set: videoEdits },
                                  });
                                });
                              }
                            }
                            resetForceSelected={() => this.setState({ allSelected: false })}
                            toggleSelected={() => this.toggleSelected(videoId, null)}
                            showAssociations={true}
                            associationsToShow={associationsToShow}
                            onDrop={
                              (sourceProps: SourceProps) =>
                                this.handleDrop(videoId, sourceProps)
                            }
                            showPlaylistsInViewMode={false}
                          />
                        </div>
                        <div className="separator" />
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className="unassociated">
            <div className="unassoc-title" style={{ top: filterHeight }}>Unassociated</div>
            <div className="show-line wrap" style={{ top: filterHeight + 20 + 40 }}>
              <div className="message">Show source </div>
              <Select.default
                value={associationsToShow}
                className="select"
                options={[
                  { value: SourceObjectType.Video, label: 'Videos' },
                  { value: SourceObjectType.Campaign, label: 'Campaigns' },
                ]}
                clearable={false}
                searchable={false}
                optionRenderer={(optionValue: Select.Option<SourceObjectType>) => (
                  <div className="metric-option" style={{ fontSize: '12px' }}>
                    {optionValue.label}
                  </div>
                )}
                style={{ fontSize: '12px', margin: 2, padding: 0, height: 20 }}
                // tslint:disable-next-line:no-any
                onChange={(newValue: any) => {
                  if (newValue !== null) {
                    this.setState({ associationsToShow: newValue });
                  }
                }}
                simpleValue={true}
              />
            </div>
            <div className="search" style={{ top: filterHeight + 20 + 40 + 80 }}>
              <form>
                <div className="text-container">
                  <input
                    type="text"
                    onKeyPress={(evt) => {
                      if (evt.key === 'Enter') {
                        this.setState({ search: evt.currentTarget.value });
                        evt.preventDefault();
                      }
                    }}
                  />
                </div>
                <button
                  type="submit"
                  value="Submit"
                  style={{ marginRight: 10 }}
                  onClick={(e) => e.preventDefault()}
                >
                  <FontAwesomeIcon icon="search" className="f-icon" />
                </button>
              </form>
            </div>
            <Unassociations
              canEdit={this.props.canEdit}
              associationsToShow={associationsToShow}
              unassociated={unassociatedFiltered}
              onDrop={(item: SourceProps) => this.handleDropOnUnassociated(item)}
            />
          </div>
        </div>
      </div>
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
          return update(prevState, {
            editDirty: (editDirty: boolean) => editDirty || _.keys(updatedVideoObject).length > 0,
            videoItemStates: { $merge: updatedVideoObject },
            videoEdits: { $set: videoEdits }
          });
        });
      });
  }

}

export interface UpdateButtonProps {
  func: () => void;
  className: string;
  message: string;
  style?: React.CSSProperties;
}

const UpdateButton = (props: UpdateButtonProps) => {
  const { func, message, className, style } = props;
  return <a className={className} style={style} onClick={func}>{message}</a>;
};

export interface CancelAndSaveButtonsProps {
  show: boolean;
  enabled: boolean;
  saved: boolean;
  cancel: () => void;
  update: () => void;
}

export interface CancelAndSaveButtonsState {
  saving: boolean;
}

export class CancelAndSaveButtons extends React.Component<CancelAndSaveButtonsProps, CancelAndSaveButtonsState> {
  constructor(props: CancelAndSaveButtonsProps) {
    super(props);
    this.state = {
      saving: false
    };
  }
  render() {
    const { show, cancel, update, saved, enabled } = this.props;
    const { saving } = this.state;
    if (!show) {
      return null;
    }
    return (
      <React.Fragment>
        <UpdateButton func={cancel} className="operation fee-button button cancel" message="Cancel" />
        <UpdateButton
          func={() => {
            if (enabled) {
              this.setState({ saving: true });
              update();
              this.setState({ saving: false });
            }
          }}
          className="operation fee-button button update"
          message={saving ? 'Saving' : (saved ? 'Saved' : 'Save')}
          style={
            (!enabled && { backgroundColor: 'grey', pointerEvents: 'none' })
            || (saving && { backgroundColor: '#28ABE2' })
            || (saved && { backgroundColor: 'green' })
            || {}}
        />
      </React.Fragment>
    );
  }
}

export interface OperationProps {
  callback: () => void;
  icon: IconProp;
  tooltip: string;
}
export interface OperationState {
  hovered: boolean;
}

export class Operation extends React.Component<OperationProps, OperationState> {
  constructor(props: OperationProps) {
    super(props);
    this.state = {
      hovered: false
    };
  }
  render() {
    const { callback, icon, tooltip } = this.props;
    const { hovered } = this.state;
    return (
      <button
        onClick={callback}
        className="operation fee-button"
        title={tooltip}
        onMouseEnter={() => this.setState({ hovered: true })}
        onMouseLeave={() => this.setState({ hovered: false })}
      >
        <FontAwesomeIcon
          icon={icon}
          style={{
            color: hovered ? 'white' : '#59595C',
          }}
        />
      </button>
    );
  }
}

export function removeSourceObject(sources: Source[], sourceObject: SourceObject): Source[] {
  const { sourceName } = sourceObject;
  const updatedSource = sources.map(source => {
    if (source.sourceName === sourceName) {
      const newSourceObjects =
        source
          .sourceObjects
          .filter(so => !(so.type === sourceObject.type && so.sourceId === sourceObject.sourceId));
      const newVideoCount =
        sourceObject.type === SourceObjectType.Video
          ? source.videosCount - 1
          : source.videosCount;
      return { ...source, sourceObjects: newSourceObjects, videosCount: newVideoCount };
    } else {
      return source;
    }
  });
  return updatedSource;
}

export function addSourceObject(sources: Source[], sourceObject: SourceObject): Source[] {
  let sourceToUpdate = sources.find(source => source.sourceName === sourceObject.sourceName);
  if (!sourceToUpdate) {
    const videosCount = sourceObject.type === SourceObjectType.Video ? 1 : 0;
    sourceToUpdate = { sourceName: sourceObject.sourceName, videosCount, sourceObjects: [sourceObject] } as Source;
  } else {
    const videosCount =
      sourceObject.type === SourceObjectType.Video
        ? sourceToUpdate.videosCount + 1
        : sourceToUpdate.videosCount;
    sourceToUpdate = {
      ...sourceToUpdate,
      videosCount,
      sourceObjects:
        [sourceObject]
          .concat(sourceToUpdate.sourceObjects)
          .sort((s1, s2) => compareString(s1.sourceId, s2.sourceId))
    };
  }
  const updatedSources = sources
    .filter(source => !(source.sourceName === sourceObject.sourceName))
    .concat([sourceToUpdate]).sort((s1, s2) => compareString(s1.sourceName, s2.sourceName));
  return updatedSources;
}

export interface UnassociationsProps {
  associationsToShow: SourceObjectType;
  unassociated: SourceObject[];
  canDrop?: boolean;
  isOver?: boolean;
  connectDropTarget?: ConnectDropTarget;
  onDrop: (item: SourceProps) => void;
  canEdit: boolean;
  top?: number;
}

const unassociationAsTarget = {
  drop(props: UnassociationsProps, monitor: DropTargetMonitor) {
    props.onDrop(monitor.getItem());
  },
};

@DropTarget(
  (props: UnassociationsProps) => SourceObjectType[props.associationsToShow],
  unassociationAsTarget,
  (connect, monitor) => ({
    canDrop: monitor.canDrop(),
    connectDropTarget: connect.dropTarget(),
    isOver: monitor.isOver(),
  }),
)
export class Unassociations extends React.Component<UnassociationsProps> {
  constructor(props: UnassociationsProps) {
    super(props);
  }

  render() {
    const {
      isOver,
      canDrop,
      connectDropTarget,
      unassociated,
    } = this.props;

    const isActive = isOver && canDrop;
    let backgroundColor = 'white';
    if (isActive) {
      backgroundColor = '#28ABE2';
    }

    const unassociatedSources: JSX.Element[] =
      unassociated
        .map((sourceObject) => {
          return (
            <SourceComponent
              canEdit={this.props.canEdit}
              key={sourceObject.type + ',' + sourceObject.sourceId}
              originId={''}
              sourceObject={sourceObject}
            />
          );
        });

    let style: React.CSSProperties = { backgroundColor };
    if (this.props.top) {
      style.top = this.props.top + 70;
    }

    return (
      connectDropTarget &&
      connectDropTarget(
        <div className="unassociated-sources" style={style}>
          {unassociatedSources}
        </div>
      )
    );
  }
}

const newVideoAsTarget = {
  drop(props: NewVideoProps, monitor: DropTargetMonitor) {
    props.onDrop(monitor.getItem());
  },
  canDrop: (props: NewVideoProps) => {
    return props.associationsToShow === SourceObjectType.Video;
  },
};

export interface NewVideoProps {
  associationsToShow: SourceObjectType;
  canDrop?: boolean;
  isOver?: boolean;
  connectDropTarget?: ConnectDropTarget;
  onDrop: (item: SourceProps) => void;
}
@DropTarget(
  (props: NewVideoProps) => SourceObjectType[props.associationsToShow],
  newVideoAsTarget,
  (connect, monitor) => ({
    canDrop: monitor.canDrop(),
    connectDropTarget: connect.dropTarget(),
    isOver: monitor.isOver(),
  }),
)
export class NewVideo extends React.Component<NewVideoProps> {

  render() {
    const {
      connectDropTarget,
      isOver,
      canDrop,
      associationsToShow
    } = this.props;

    const isActive = isOver && canDrop;

    let backgroundColor = 'white';
    if (isActive) {
      backgroundColor = '#28ABE2';
    }

    if (associationsToShow !== SourceObjectType.Video) {
      return (
        <div
          className="new-video"
          style={{ backgroundColor }}
        >
          <span>Switch to Video Associations before creating a new video</span>
        </div>
      );
    }
    return (
      connectDropTarget &&
      connectDropTarget(
        <div
          className="new-video"
          style={{ backgroundColor }}
          title="Drag and drop source video to create new video"
        >
          <FontAwesomeIcon
            icon="plus"
            style={{ fontSize: '24px' }}
          />
          <span>New Video</span>
          <span>Drag and drop to add</span>
        </div>
      )
    );
  }
}

function trimSourceObject(sourceObjects: SourceObject[], sourceObject: SourceObject): SourceObject[] {
  return sourceObjects.filter(
    x => !(
      x.sourceId === sourceObject.sourceId
      && x.sourceName === sourceObject.sourceName
      && x.type === sourceObject.type
    )
  );
}

export function getSelectedVideos(sortedIds: string[], states: VideoItemStateDictionary): VideoItemState[] {
  return (
    _.chain(sortedIds)
      .pick((id: string) => states[id].selected)
      .map((id: string) => states[id])
      .value()
  );
}
