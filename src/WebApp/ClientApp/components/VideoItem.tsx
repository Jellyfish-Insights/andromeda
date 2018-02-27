import { IconProp } from '@fortawesome/fontawesome';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as React from 'react';
import * as Modal from 'react-modal';
import * as _ from 'underscore';
import {
  capitalizeFirst,
  compareString,
  formatYMDDashed
} from '../reducers/helpers';
import {
  Source,
  AddOrRemove,
  Video,
  Tag,
  VideoEdits,
  SourceObjectType,
  SourceLinkType,
  SourceObject,
  MetricInfo
} from '../types';
import { VideoItemState } from './VideoList';
import * as Moment from 'moment';
import { editTitle, addOrRemoveTag, UpdateTag, getUpdateTags } from './Edit';
import * as ReactTooltip from 'react-tooltip';
import * as ReactMarkdown from 'react-markdown';

import {
  ConnectDragSource,
  ConnectDragPreview,
  DragSource,
  DragSourceConnector,
  DragSourceMonitor,
  ConnectDropTarget,
  DropTarget,
  DropTargetMonitor,
} from 'react-dnd';

export const modalStyle = {
  content: {
    top: '50%',
    left: '50%',
    right: 'auto',
    bottom: 'auto',
    marginRight: '-50%',
    transform: 'translate(-50%, -50%)',
    width: '80%',
    maxHeight: '70%',
  },
  overlay: {
    zIndex: 5001,
  },
};

export function ScrollWhileDrag() {
  var animationId: any;
  document.addEventListener('dragover', function(e) {
    var divPosition = this.getElementById("scroll-position");
    var posMouseY = e.pageY;
    var posTopDiv = divPosition.offsetTop + divPosition.offsetHeight;
    var scrollSpreed = 100;

    if (posMouseY < posTopDiv) {
      if (animationId == null) {
        animationId = setInterval(start, scrollSpreed);
      }
    } else {
      clearInterval(animationId);
      animationId = null;
    }
    function start() {
      window.scroll({
        top: window.scrollY - (scrollSpreed + scrollSpreed * 0.50),
        behavior: 'smooth'
      });
    }
    document.addEventListener('dragend', function(e) {
      clearInterval(animationId);
      animationId = null;
    })
  })
}

export function filterSources(sources: Source[]): Source[] {
  return sources.reduce(
    (filtered, source) => {
      const sourceObjects = source.sourceObjects.filter(
        sourceObject => sourceObject.type !== SourceObjectType.Video || sourceObject.publishedStatus
      );
      const videosCount = sourceObjects.filter(sourceObject => sourceObject.type === SourceObjectType.Video).length;
      if (sourceObjects.length > 0) {
        filtered.push({ ...source, sourceObjects, videosCount });
      }
      return filtered;
    },
    []);
}

export function sortTags(tags: UpdateTag[]): UpdateTag[] {
  return tags
    .sort((x, y) => compareString(x.value, y.value))
    .sort((x, y) => {
      return compareString(x.type, y.type);
    }
    );
}
export interface VideoProps {
  key?: string;
  editMode: boolean;
  updateVideoItemState: (x: VideoItemState, videoEdits: VideoEdits) => void;
  videoEdits: VideoEdits;
  toggleSelected: () => void;
  resetForceSelected: () => void;
  state: VideoItemState;
  original: Video;
  showAssociations?: boolean;
  associationsToShow?: SourceObjectType;
  onDrop: (item: SourceProps) => void;
  canEdit: boolean;
  showPlaylistsInViewMode: boolean;
}

interface InternalState {
  modalOpen: { [x: string]: boolean };
}

export class VideoItem extends React.Component<VideoProps, InternalState> {
  constructor(props: VideoProps) {
    super(props);
    this.state = { modalOpen: {} };
  }

  public render() {
    const { editMode, showAssociations, associationsToShow, onDrop } = this.props;
    const videoItemState = this.props.state;
    return (
      <div className="video-data">
        <div className="basic">
          <div className="thumb-checkbox">
            <div>
              {this.props.canEdit && this.props.editMode &&
                <div className="checkbox">
                  <input
                    name="Selected"
                    type="checkbox"
                    checked={videoItemState.selected}
                    value=""
                    onChange={() => {
                      this.props.toggleSelected();
                      this.props.resetForceSelected();
                    }}
                  />
                </div>
              }
              <div className="thumb">
                <img src={this.props.state.thumbnailLink} />
              </div>
            </div>
          </div>
          {this.props.editMode ? this.renderEditMode() : this.renderViewMode()}
        </div>
        {editMode && showAssociations &&
          <VideoAssociations
            canEdit={this.props.canEdit}
            videoItemState={videoItemState}
            associationsToShow={associationsToShow}
            onDrop={(item: SourceProps) => onDrop(item)}
          />
        }
      </div>
    );
  }

  private renderViewMode() {
    const { showPlaylistsInViewMode } = this.props;
    const videoAndMetric = this.props.state;
    const { publishedAt, playlists } = this.props.state;
    const { modalOpen } = this.state;
    const videoTitle = videoAndMetric.title || 'Video has no title';
    if (!videoAndMetric.sources) {
      return <div className="loading">Loading...</div>;
    }
    const sources = videoAndMetric.sources;
    const videoId: string = 'VIDEO' + videoAndMetric.id;
    const presentation = (
      <a data-tip={true} data-for={videoId} data-event="click focus" style={{ color: 'rgb(89, 89, 92)' }}>
        <p className="title" title={videoTitle}>
          {videoTitle}
        </p>
      </a>
    );

    let tags = (
      <div className="medium-tone-topic">
        <span style={{ fontWeight: 700, float: 'left', paddingRight: '10px' }}>Tags: </span>
        {videoAndMetric.tags
          .map((tag, index) => {
            let style: React.CSSProperties = {
              backgroundColor: 'rgb(135, 135, 135)'
            };
            if (tag.type.toLowerCase() === 'generic') {
              style = { backgroundColor: '#fa7676' };
            }

            style = {
              ...style,
              float: 'left',
              display: 'inline',
            };

            return (
              <a
                className="tag"
                type={tag.type}
                key={index}
                style={style}
              >
                {tag.value}
              </a>
            );
          })}
      </div>
    );

    const tooltip = (
      <ReactTooltip
        id={videoId}
        className="video-component-tooltip"
        effect="solid"
        globalEventOff="click"
        offset={{ right: 200 }}
      >
        <div>
          <div>
            <span style={{ fontWeight: 700 }}>Title: </span>{videoTitle}
          </div>
          {tags}
        </div>
      </ReactTooltip>
    );
    return (
      <div className="info">
        <div className="title-container">
          <div className="title-sub-container">
            {presentation}
            {tooltip}
          </div>
        </div>
        {sources.map(
          (source, index) => renderSourceIcons(
            source,
            modalOpen,
            (modalOpen) => this.setState({ modalOpen }),
            index
          )
        )}
        {renderVideoPublicationDate(publishedAt)}
        {showPlaylistsInViewMode && <div className="sources-playlists">
          <span className="playlists edit">{playlists.sort().join(' | ')}</span>
        </div>}
      </div>
    );
  }

  private renderEditMode() {
    const { videoEdits, state, original, updateVideoItemState, canEdit } = this.props;
    const { title, sources, playlists, publishedAt, archived } = state;
    const { modalOpen } = this.state;
    const tags = getUpdateTags(original.tags, videoEdits.edits[original.id]);
    return (
      <div className="info expanded">
        <input
          name="title"
          placeholder="Video Title"
          disabled={!canEdit}
          onChange={evt => {
            updateVideoItemState(
              {
                ...state,
                title: evt.target.value,
              },
              editTitle(original, evt.target.value, videoEdits)
            );
          }
          }
          type="text"
          value={title || ''}
        />
        <div className="sources-playlists">
          {sources.map((source, index) => renderSourceIcons(
            source,
            modalOpen,
            (modalOpen) => this.setState({ modalOpen }),
            index)
          )}
          {renderVideoPublicationDate(publishedAt)}
          <span className="playlists">{playlists.sort().join(' | ')}</span>
        </div>
        {archived && <div className="archived-message">Archived</div>}
        <div className="medium-tone-topic">
          {tags
            .map((tag, index) => {
              const tagNotRemoved = !(tag.update && tag.update === 'remove');
              let style: React.CSSProperties = {
                backgroundColor: 'rgb(135, 135, 135)'
              };
              if (tag.type.toLowerCase() === 'generic') {
                style = { backgroundColor: '#fa7676' };
              }
              if (tag.update) {
                if (tag.update === 'add') {
                  style = {
                    backgroundColor: 'rgb(82, 170, 221)',
                    color: 'white'
                  };
                } else if (tag.update === 'remove') {
                  style = {
                    backgroundColor: 'white',
                    textDecoration: 'line-through',
                    textDecorationColor: 'black',
                    color: 'black',
                    border: '1px solid black',
                  };
                }
              }

              style = {
                ...style,
                float: 'left',
                display: 'inline',
              };

              return (
                <a
                  className="tag"
                  type={tag.type}
                  key={index}
                  onClick={() => {
                    if (this.props.canEdit && tagNotRemoved) {
                      handleTagDelete(
                        tag.type,
                        tag.updateDate,
                        tag.value,
                        videoEdits,
                        tags,
                        state,
                        original,
                        updateVideoItemState,
                      );
                    }
                  }}
                  style={style}
                >
                  {tag.value}
                  {this.props.canEdit && tagNotRemoved && <FontAwesomeIcon icon="times" />}
                </a>
              );
            })}
        </div>
      </div>
    );
  }
}

const videoTargetAssociation = {
  drop(props: VideoAssociationsProps, monitor: DropTargetMonitor) {
    props.onDrop(monitor.getItem());
  },
};

export interface VideoAssociationsProps {
  videoItemState: VideoItemState;
  associationsToShow: SourceObjectType;
  canDrop?: boolean;
  isOver?: boolean;
  connectDropTarget?: ConnectDropTarget;
  onDrop: (item: SourceProps) => void;
  canEdit: boolean;
}
@DropTarget(
  (props: VideoAssociationsProps) => SourceObjectType[props.associationsToShow],
  videoTargetAssociation,
  (connect, monitor) => ({
    canDrop: monitor.canDrop(),
    connectDropTarget: connect.dropTarget(),
    isOver: monitor.isOver(),
  }),
)
export class VideoAssociations extends React.Component<VideoAssociationsProps> {
  constructor(props: VideoAssociationsProps) {
    super(props);
  }

  render() {
    const {
      videoItemState,
      associationsToShow,
      isOver,
      canDrop,
      connectDropTarget,
    } = this.props;

    if (associationsToShow === undefined) {
      return null;
    }
    const isActive = isOver && canDrop;
    let backgroundColor = 'white';
    if (isActive) {
      backgroundColor = '#28ABE2';
    }
    const { sources, id } = videoItemState;
    let i = 0;
    const associationsArrays: JSX.Element[][] = sources.map((source) => {
      return source.sourceObjects
        .filter(sourceObject => sourceObject.type === associationsToShow)
        .map((sourceObject) => {
          i++;
          return (
            <SourceComponent
              key={i}
              originId={id}
              sourceObject={sourceObject}
              canEdit={this.props.canEdit}
            />
          );
        });
    });
    const associations: JSX.Element[] = [].concat.apply([], associationsArrays);

    return (
      connectDropTarget &&
      connectDropTarget(
        <div className="associations" style={{ backgroundColor }}>
          {associations}
        </div>
      )
    );
  }
}

export function handleTagDelete(
  type: string,
  updateDate: Date,
  value: string,
  videoEdits: VideoEdits,
  tags: Tag[],
  videoState: VideoItemState,
  originalVideo: Video,
  updateVideoItemState: (x: VideoItemState, videoEdits: VideoEdits) => void
) {
  let filteredTags = tags.filter(tag => !(tag.type === type && tag.value === value));
  updateVideoItemState(
    { ...videoState, tags: filteredTags },
    addOrRemoveTag(originalVideo, { type, updateDate, value }, AddOrRemove.Remove, videoEdits)
  );
}

export function renderVideoPublicationDate(publishedAt: Date) {
  const formattedDate = Moment(publishedAt).format('MMM Do, YYYY');
  const style: React.CSSProperties = {
    fontSize: 10,
  };
  return <span style={style}>{formattedDate}</span>;
}

export function renderSourceIcons(
  source: Source,
  modalOpenDict: { [x: string]: boolean },
  setModalOpen: (dict: { [sourceName: string]: boolean }) => void,
  index: number,
) {
  let modalOpen: boolean = modalOpenDict[source.sourceName];
  let sourceObjectCategories = _.uniq(
    source.sourceObjects.sort((x, y) => x.type - y.type).map(x => x.type)
  ).map(type => [type, SourceObjectType[type]]);
  let sourceName = capitalizeFirst(source.sourceName);
  if (source.sourceName === 'youtube') {
    sourceName = 'YouTube';
  }
  return (
    <span key={index}>
      <span onClick={() => setModalOpen({ [source.sourceName]: true })}><SourceIcon
        sourceName={source.sourceName}
      /></span><span style={{ marginRight: 10 }}>{source.videosCount}</span>
      <Modal
        isOpen={modalOpen}
        onRequestClose={() => setModalOpen({})}
        style={modalStyle}
        shouldCloseOnEsc={true}
        shouldFocusAfterRender={true}
      >
        <h2 style={{ textAlign: 'center' }}>{sourceName}</h2>
        {sourceObjectCategories.map(([id, category], index) => (
          <div key={index}>
            <h4>{category === 'Video' ? 'Video instance' : 'Ad campaign'}</h4>
            <div>
              {source
                .sourceObjects
                .filter(l => l.type === id)
                .sort((l1, l2) => {
                  const dateCompare = compareString(formatYMDDashed(l1.publishedAt), formatYMDDashed(l2.publishedAt));
                  if (dateCompare === 0) {
                    return compareString(l1.title, l2.title);
                  }
                  return dateCompare;
                })
                .map((l, subIndex) => {
                  const links = l.links.filter(x => x.type !== SourceLinkType.Content).map((sourceLink, id) => {
                    return (
                      <a
                        target="_blank"
                        href={sourceLink.link}
                        key={id}
                        className="link"
                      >{SourceLinkType[sourceLink.type] === 'Content' ? 'Link' : SourceLinkType[sourceLink.type]}
                      </a>
                    );
                  });
                  const contentData = l.links.find(x => x.type === SourceLinkType.Content);
                  const contentLink = contentData && contentData.link;
                  const videoTitle = l.title || 'Video has no title';
                  const formattedDate = formatYMDDashed(l.publishedAt);
                  // The server return the date 0001-01-01 sometimes, instead of null
                  const shouldShowDate = formattedDate.split('-')[0] !== '0001';
                  return (
                    <div className="link-container" key={subIndex} title={'Title:' + l.title}>
                      {shouldShowDate &&
                        <p className="date">
                          {formattedDate}
                        </p>
                      }
                      <p className="title">
                        <span
                          style={{
                            display: 'block',
                            maxWidth: '100%',
                            textOverflow: 'ellipsis',
                            overflow: 'hidden',
                            whiteSpace: 'nowrap'
                          }}
                        >
                          {contentLink
                            ? (
                              <a
                                target="_blank"
                                href={contentLink}
                                key={id}
                                className="link"
                              >
                                {videoTitle}
                              </a>
                            )
                            : videoTitle}
                          {(l.publishedStatus || category !== 'Video') ? '' : ' (unpublished)'}
                        </span>
                      </p>
                      <p>
                        {links}
                      </p>
                    </div>
                  );
                })
              }
            </div>
          </div>
        ))}
      </Modal>
    </span>
  );
}

export const boxSource = {
  beginDrag(props: SourceProps) {
    return props;
  },
  canDrag(props: SourceProps) {
    return props.canEdit;
  }
};

export interface SourceProps {
  originId?: string;
  sourceObject: SourceObject;
  connectDragSource?: ConnectDragSource;
  connectDragPreview?: ConnectDragPreview;
  isDragging?: boolean;
  isDropped?: boolean;
  canEdit: boolean;
}

@DragSource(
  (props: SourceProps) => SourceObjectType[props.sourceObject.type],
  boxSource,
  (connect: DragSourceConnector, monitor: DragSourceMonitor) => ({
    connectDragSource: connect.dragSource(),
    connectDragPreview: connect.dragPreview(),
    isDragging: monitor.isDragging()
  }),
)
export class SourceComponent extends React.Component<SourceProps> {
  constructor(props: SourceProps) {
    super(props);
  }

  render() {
    const { sourceObject, isDragging, connectDragSource, connectDragPreview } = this.props;
    const opacity = isDragging ? 0.4 : 1;
    const { sourceName, title, sourceId, thumbnailLink } = sourceObject;
    const contentData = sourceObject.links.find(x => x.type === SourceLinkType.Content);
    const contentLink = contentData && contentData.link;
    const innerPresentation = (
      <div className="source-component">
        <SourceIcon sourceName={sourceName} />
        <a className="title" href={contentLink}>
          {title}
        </a>
      </div>
    );
    const presentation = (
      <a data-tip={true} data-for={sourceId}>
        {innerPresentation}
      </a>
    );
    const tooltip = (
      <ReactTooltip id={sourceId} className="source-tooltip" effect="solid" >
        <div className="source-component-tooltip">
          <div className="thumb">
            <img src={thumbnailLink} />
          </div>
          <div className="container">
            <div className="title">
              {title}
            </div>
            <div className="info">
              <SourceIcon sourceName={sourceName} />
            </div>
          </div>
        </div>
      </ReactTooltip>
    );

    return (
      connectDragSource && connectDragPreview &&
      connectDragSource(
        <div className="association-container" style={{ opacity }}>
          {connectDragPreview(presentation, { captureDraggingState: true })}
          {!isDragging && tooltip}
        </div>
      )
    );
  }
}

export interface SourceIconProps {
  sourceName: string;
}

export const SourceIcon = (props: SourceIconProps) => {
  const { sourceName } = props;
  return (
    <FontAwesomeIcon
      fixedWidth={true}
      icon={['fab', sourceName] as IconProp}
      className={sourceName}
    />
  );
};
