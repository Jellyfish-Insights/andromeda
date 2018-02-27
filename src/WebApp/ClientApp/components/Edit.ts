import {
  VideoEdit,
  VideoEdits,
  Tag,
  AddOrRemove,
  EditType,
  Video,
  SourceObject,
  SourceObjectType,
  Source
} from '../types';
import { VideoItemState } from './VideoList';
import * as _ from 'underscore';
import update from 'immutability-helper';
import { VideoDictionary } from './VideoSort';
import { compareString } from '../reducers/helpers';
import { sortTags } from './VideoItem';
// This is a module to help dealing with video editing by the user

export function sanitizeTag(tag: string, lowerCase: boolean): string {
  return (lowerCase ? tag.toLowerCase() : tag).replace(/ +(?= )/g, '').trim();

}
function isUpdateEmpty(videoEdit: VideoEdit, strict?: boolean) {
  // if we have 0 keys or 2 (flag, date) and nothing else... the VideoEdit has nothing actually
  if (strict) {
    return _.keys(videoEdit).length === 0;
  }
  return _.keys(videoEdit).length <= 2;
}
export const editTitle = function(video: Video, title: string, videoEdits: VideoEdits) {
  let videoEdit: VideoEdit = { ...videoEdits.edits[video.id] };
  // In the case of editting a tag in a video that is not in videoEdits, this means we are updating a video
  // if the video were new or were going to be deleted, it should have been already here
  if (isUpdateEmpty(videoEdit, true)) {
    videoEdit.flag = EditType.Update;
    videoEdit.updateDate = video.updateDate;
  }
  videoEdit.title = title;
  return { edits: { ...videoEdits.edits, [video.id]: videoEdit } };
};

export const addOrRemoveTag = function(
  originalVideo: Video,
  tag: Tag,
  op: AddOrRemove,
  videoEdits: VideoEdits): VideoEdits {
  let videoEdit: VideoEdit = { ...videoEdits.edits[originalVideo.id] };
  // In the case of editting a tag in a video that is not in videoEdits, this means we are updating a video
  // if the video were new or were going to be deleted, it should have been already here
  if (isUpdateEmpty(videoEdit, true)) {
    videoEdit.flag = EditType.Update;
    videoEdit.updateDate = originalVideo.updateDate;
  }

  if (tag.type.toLowerCase() !== 'generic') {
    if (!videoEdit.metaTags) {
      videoEdit.metaTags = {};
    }
    const videoTag = originalVideo.tags.filter(x => x.type.toLowerCase() === tag.type.toLowerCase()).map(x => x.value);
    const tagExistsInOriginal = _.contains(videoTag, tag.value);
    const tagExistsInVideoEdit = (
      videoEdit.metaTags && videoEdit.metaTags[tag.type] === tag.value
    );
    const originalHasTagOfSameType = videoTag.length > 0;

    if (op === AddOrRemove.Remove) {
      // If operation is removal, does not check for value
      if (originalHasTagOfSameType) {
        videoEdit.metaTags[tag.type] = null;
      } else if (
        (videoEdit.flag === EditType.New && videoEdit.metaTags[tag.type]) || tagExistsInVideoEdit
      ) {
        delete videoEdit.metaTags[tag.type];
      }
    } else {
      if (videoEdit.metaTags[tag.type] === null && tagExistsInOriginal) {
        delete videoEdit.metaTags[tag.type];
      } else if (!tagExistsInOriginal || (videoEdit.metaTags[tag.type] && videoEdit.metaTags[tag.type] !== tag.value)) {
        videoEdit.metaTags[tag.type] = tag.value;
      }
    }

    if (_.keys(videoEdit.metaTags).length === 0) {
      delete videoEdit.metaTags;
    }
  } else {
    const videoGeneric = originalVideo.tags.filter(x => x.type.toLowerCase() === 'generic').map(x => sanitizeTag(x.value, true));
    const sanitizedValue = sanitizeTag(tag.value, true);
    const tagExistsInOriginal = _.contains(videoGeneric, sanitizedValue);

    if (!videoEdit.addedGenericTags) {
      videoEdit.addedGenericTags = [];
    }

    if (!videoEdit.removedGenericTags) {
      videoEdit.removedGenericTags = [];
    }

    if (op === AddOrRemove.Remove) {
      if (_.contains(videoEdit.addedGenericTags, sanitizedValue)) {
        videoEdit.addedGenericTags = videoEdit.addedGenericTags.filter(x => x !== sanitizedValue);
      } else if (tagExistsInOriginal) {
        videoEdit.removedGenericTags = [...videoEdit.removedGenericTags, sanitizedValue];
      }
    } else {
      if (_.contains(videoEdit.removedGenericTags, sanitizedValue)) {
        videoEdit.removedGenericTags = videoEdit.removedGenericTags.filter(x => x !== sanitizedValue);
      } else if (!tagExistsInOriginal) {
        videoEdit.addedGenericTags = [...videoEdit.addedGenericTags, sanitizedValue];
      }
    }

    if (videoEdit.addedGenericTags.length === 0) {
      delete videoEdit.addedGenericTags;
    }

    if (videoEdit.removedGenericTags.length === 0) {
      delete videoEdit.removedGenericTags;
    }
  }

  // If only one key, that's the "flag" key, so nothing was updated in the end
  if (videoEdit.flag && videoEdit.updateDate && isUpdateEmpty(videoEdit)) {
    let edits = { edits: { ...videoEdits.edits } };
    delete edits.edits[originalVideo.id];
    return edits;
  }
  return { edits: { ...videoEdits.edits, [originalVideo.id]: videoEdit } };
};

function initializeUpdate(video: Video, videoEdits: VideoEdits) {
  if (!videoEdits.edits[video.id]) {
    return update(videoEdits, {
      edits: {
        [video.id]: {
          $set: {
            flag: EditType.Update,
            updateDate: video.updateDate,
            addedCampaigns: [],
            removedCampaigns: [],
            addedVideos: [],
            removedVideos: [],
          }
        }
      }
    });
  }
  return update(videoEdits, {
    edits: {
      [video.id]: {
        flag: (f?: EditType) => f || EditType.Update,
        updateDate: (u?: Date) => u || video.updateDate,
        addedCampaigns: (a?: string[]) => a || [],
        removedCampaigns: (a?: string[]) => a || [],
        addedVideos: (a?: string[]) => a || [],
        removedVideos: (a?: string[]) => a || [],
      }
    }
  });
}

function removeSource(sourceObject: SourceObject, videoEdit: VideoEdit): VideoEdit {
  const { sourceId, type } = sourceObject;
  switch (type) {
    case SourceObjectType.Video: {
      const added = videoEdit.addedVideos;
      if (added && _.contains(added, sourceId)) {
        return update(videoEdit, {
          addedVideos: {
            $apply: (addedVideos: string[]) => addedVideos.filter((id: string) => id !== sourceId)
          }
        });
      }
      return update(videoEdit, {
        removedVideos: {
          $push: [sourceId]
        }
      });
    }

    case SourceObjectType.Campaign: {
      const added = videoEdit.addedCampaigns;
      if (added && _.contains(added, sourceId)) {
        return update(videoEdit, {
          addedCampaigns: {
            $apply: (addedCampaigns: string[]) => addedCampaigns.filter((id: string) => id !== sourceId)
          }
        });
      }
      return update(videoEdit, {
        removedCampaigns: {
          $push: [sourceId]
        }
      });
    }

    case SourceObjectType.AdSet: {
      throw 'Type AdSet is not supported when removing source video/campaign';
    }
  }
}

function addSource(sourceObject: SourceObject, videoEdit: VideoEdit): VideoEdit {
  const { sourceId, type } = sourceObject;
  const newVideo = videoEdit.flag === EditType.New;
  switch (type) {
    case SourceObjectType.Video: {
      const removed = videoEdit.removedVideos;
      if (removed && _.contains(removed, sourceId)) {
        return update(videoEdit, {
          removedVideos: {
            $apply: (removedVideos: string[]) => removedVideos.filter((id: string) => id !== sourceId)
          },
          flag: { $set: newVideo ? EditType.New : EditType.Update }
        });
      }
      return update(videoEdit, {
        addedVideos: {
          $push: [sourceId]
        },
        flag: { $set: newVideo ? EditType.New : EditType.Update }
      });
    }
    case SourceObjectType.Campaign: {
      const removed = videoEdit.removedCampaigns;
      if (removed && _.contains(removed, sourceId)) {
        return update(videoEdit, {
          removedCampaigns: {
            $apply: (removedCampaigns: string[]) => removedCampaigns.filter((id: string) => id !== sourceId)
          }
        });
      }
      return update(videoEdit, {
        addedCampaigns: {
          $push: [sourceId]
        }
      });
    }
    case SourceObjectType.AdSet: {
      throw 'Type AdSet is not supported when adding source video/campaign';
    }

  }

}

function getSourceArrayFromVideo(video: Video, sourceObjectType: SourceObjectType): SourceObject[] {
  return _.flatten(video.sources.map(x => x.sourceObjects))
    .filter((x: SourceObject) => x.type === sourceObjectType);
}

function cleanArrays(constVideoEdit: VideoEdit, video: Video, op: AddOrRemove): VideoEdit {
  // tslint:disable-next-line:no-any
  let videoEdit = _.omit(constVideoEdit, (value: any) => {
    return _.isArray(value) && value.length === 0;
  });
  const {
    removedVideos,
  } = videoEdit;

  // In the case that we have removed all videos, mark as Delete operation
  if (removedVideos) {
    const sourcesVideosIdsBeforeEdit =
      _.flatten(video.sources.map(x => x.sourceObjects))
        .filter((x: SourceObject) => x.type === SourceObjectType.Video);
    if (op === AddOrRemove.Remove && sourcesVideosIdsBeforeEdit.length === 1) {
      return update(videoEdit, {
        flag: { $set: EditType.Delete }
      });
    }
  }

  return videoEdit;
}

export const moveSource = function(
  sourceVideo: Video,
  targetVideo: Video,
  sourceObject: SourceObject,
  videoEdits: VideoEdits
): VideoEdits {
  const sourceVideoId = sourceVideo.id;
  const targetVideoId = targetVideo.id;
  let currentVideoEdits = initializeUpdate(sourceVideo, videoEdits);
  currentVideoEdits = initializeUpdate(targetVideo, currentVideoEdits);

  currentVideoEdits = update(currentVideoEdits, {
    edits: {
      [sourceVideoId]: {
        $apply: (videoEdit: VideoEdit) => {
          return cleanArrays(removeSource(sourceObject, videoEdit), sourceVideo, AddOrRemove.Remove);
        }
      },
      [targetVideoId]: {
        $apply: (videoEdit: VideoEdit) => {
          return cleanArrays(addSource(sourceObject, videoEdit), targetVideo, AddOrRemove.Add);
        }
      }
    }
  });
  const purgeIds =
    [sourceVideoId, targetVideoId]
      .filter(id => isUpdateEmpty(currentVideoEdits.edits[id]));

  return update(currentVideoEdits, { edits: { $unset: purgeIds } });
};

export const onlyRemoveSource = function(
  sourceVideo: Video,
  sourceObject: SourceObject,
  videoEdits: VideoEdits
): VideoEdits {
  const sourceVideoId = sourceVideo.id;
  let currentVideoEdits = initializeUpdate(sourceVideo, videoEdits);

  currentVideoEdits = update(currentVideoEdits, {
    edits: {
      [sourceVideoId]: {
        $apply: (videoEdit: VideoEdit) => {
          return cleanArrays(removeSource(sourceObject, videoEdit), sourceVideo, AddOrRemove.Remove);
        }
      },
    }
  });
  const purgeIds =
    [sourceVideoId]
      .filter(id => isUpdateEmpty(currentVideoEdits.edits[id]));

  return update(currentVideoEdits, { edits: { $unset: purgeIds } });
};

export const onlyAddSource = function(
  targetVideo: Video,
  sourceObject: SourceObject,
  videoEdits: VideoEdits
): VideoEdits {
  const targetVideoId = targetVideo.id;
  let currentVideoEdits = initializeUpdate(targetVideo, videoEdits);

  currentVideoEdits = update(currentVideoEdits, {
    edits: {
      [targetVideoId]: {
        $apply: (videoEdit: VideoEdit) => {
          return cleanArrays(addSource(sourceObject, videoEdit), targetVideo, AddOrRemove.Add);
        }
      }
    }
  });
  const purgeIds =
    [targetVideoId]
      .filter(id => isUpdateEmpty(currentVideoEdits.edits[id]));

  return update(currentVideoEdits, { edits: { $unset: purgeIds } });
};

export function createNewVideo(id: string, sourceObject: SourceObject): [VideoItemState, VideoEdit] {
  const { title, thumbnailLink, publishedAt, sourceName, sourceId } = sourceObject;
  const updateDate = new Date();
  const newVideo: VideoItemState = {
    id,
    title,
    archived: false,
    updateDate,
    playlists: [],
    tags: [],
    sources: [{ sourceName, videosCount: 1, sourceObjects: [sourceObject] }],
    thumbnailLink,
    publishedAt,
    totalMetrics: [],
    metricsPerPersona: [],
    selected: false,
    new: true
  };
  const newVideoEdits: VideoEdit = {
    title,
    archive: false,
    updateDate,
    addedVideos: [sourceId],
    flag: EditType.New,
  };
  return [newVideo, newVideoEdits];
}

export function setArchive(videos: VideoDictionary, archive: boolean, videoEdits: VideoEdits): VideoEdits {
  const videoIds = _.keys(videos);
  const defaultObjects = _.object(
    videoIds,
    videoIds.map((id) => {
      return { archive, updateDate: videos[id].updateDate, flag: EditType.Update } as VideoEdit;
    })
  );
  return update(videoEdits, {
    edits: (data: { [index: string]: VideoEdit }) => {
      return (
        _.defaults(
          _.mapObject(
            data,
            (videoEdit: VideoEdit, key: string) =>
              _.contains(videoIds, key)
                ? _.chain(videoEdit)
                  .omit(['archive'])
                  .defaults({ archive, updateDate: videos[key].updateDate, flag: EditType.Update })
                  .value()
                : videoEdit
          ),
          defaultObjects
        )
      );
    }
  });
}

export function addSourceObjectsToSource(sourceObjects: SourceObject[], sources: Source[]) {
  const sourceNames = _.chain(sourceObjects)
    .map(so => so.sourceName)
    .concat(sources.map(x => x.sourceName))
    .uniq()
    .value();
  const newSources: Source[] = [];
  sourceNames.forEach(sourceName => {
    const currentSource = sources.find(x => x.sourceName === sourceName);
    const currentSourceObject = sourceObjects.filter(so => so.sourceName === sourceName);
    const newVideosNumber = currentSourceObject.filter(so => so.type === SourceObjectType.Video).length;
    if (currentSource) {
      newSources.push(update(currentSource, {
        sourceObjects: { $push: currentSourceObject },
        videosCount: (count: number) => count + newVideosNumber
      }));
    } else {
      newSources.push({
        sourceName,
        sourceObjects: currentSourceObject,
        videosCount: newVideosNumber
      });
    }
  });
  return newSources
    .sort((x, y) => compareString(x.sourceName, y.sourceName));
}

export function mergeVideos(videos: Video[], videoEdits: VideoEdits): [Video, VideoEdits] {
  let videoToMerge = videos[0];
  const deletedVideos = _.tail(videos);
  let newSourceObjects: SourceObject[] = [];
  const deletionEdits: { [id: string]: VideoEdit } = _.object(
    deletedVideos.map(dv => dv.id),
    deletedVideos.map((video: Video): VideoEdit => {
      const currentCampaignSources = getSourceArrayFromVideo(video, SourceObjectType.Campaign);
      const currentVideoSources = getSourceArrayFromVideo(video, SourceObjectType.Video);
      newSourceObjects = newSourceObjects.concat(currentCampaignSources).concat(currentVideoSources);
      const currentVideoEdit = videoEdits.edits[video.id];
      return {
        updateDate: video.updateDate,
        removedCampaigns: currentCampaignSources
          .map(c => c.sourceId)
          .concat((currentVideoEdit && currentVideoEdit.removedCampaigns) || []),
        removedVideos: currentVideoSources
          .map(v => v.sourceId)
          .concat((currentVideoEdit && currentVideoEdit.removedVideos) || []),
        flag: EditType.Delete
      };
    })
  );
  const isVideoToMergeNew = Boolean(
    videoEdits.edits[videoToMerge.id] && videoEdits.edits[videoToMerge.id].flag === EditType.New
  );
  let alreadyAddedCampaigns: string[] = [];
  let alreadyAddedVideos: string[] = [];
  let videoToEditPreviousEdit = videoEdits.edits[videoToMerge.id];
  if (videoToEditPreviousEdit) {
    alreadyAddedCampaigns = videoToEditPreviousEdit.addedCampaigns || [];
    alreadyAddedVideos = videoToEditPreviousEdit.addedVideos || [];
  }
  const mergeEdit: { [id: string]: VideoEdit } = {
    [videoToMerge.id]: {
      ...(videoEdits.edits[videoToMerge.id] || {}),
      updateDate: videoToMerge.updateDate,
      addedCampaigns: _.chain(deletedVideos)
        .map(dv => getSourceArrayFromVideo(dv, SourceObjectType.Campaign))
        .flatten()
        .map(c => c.sourceId)
        .concat(alreadyAddedCampaigns)
        .value(),
      addedVideos: _.chain(deletedVideos)
        .map(dv => getSourceArrayFromVideo(dv, SourceObjectType.Video))
        .flatten()
        .map(c => c.sourceId)
        .concat(alreadyAddedVideos)
        .value(),
      archive: false,
      flag: isVideoToMergeNew ? EditType.New : EditType.Update
    }
  };

  const mergedVideo = update(videoToMerge, {
    archived: { $set: false },
    sources: (sources: Source[]) => addSourceObjectsToSource(
      newSourceObjects,
      sources
    )
  });

  return [
    mergedVideo,
    {
      edits: {
        ...videoEdits.edits,
        ...deletionEdits,
        ...mergeEdit
      }
    }
  ];

}

export interface UpdateTag extends Tag {
  update?: 'add' | 'remove';
}

export function getUpdateTags(originalTags: Tag[], videoEdit: VideoEdit): UpdateTag[] {
  let newTags: UpdateTag[] = [...originalTags];
  if (!videoEdit) {
    return sortTags(newTags);
  }
  const addedGenericTags = videoEdit.addedGenericTags || [];
  const removedGenericTags = videoEdit.removedGenericTags || [];
  const metaTagsDict: { [index: string]: string | null } = videoEdit.metaTags || {};
  newTags = newTags.map((x: UpdateTag): UpdateTag =>
    (x.type.toLowerCase() === 'generic' && removedGenericTags.includes(x.value))
      ? { ...x, update: 'remove' }
      : x
  );
  const newGeneric: Tag[] = (addedGenericTags && addedGenericTags.map((x: string): UpdateTag => {
    return { type: 'Generic', value: x, updateDate: new Date(), update: 'add' };
  })) || [];
  newTags = newTags.concat(newGeneric);
  const metaTags = _.keys(metaTagsDict);
  metaTags.forEach(metaTag => {
    newTags = newTags.map((x: UpdateTag): UpdateTag => !(x.type === metaTag) ? x : { ...x, update: 'remove' });
    if (metaTagsDict[metaTag]) {
      newTags = newTags.concat([{
        type: metaTag,
        value: metaTagsDict[metaTag],
        updateDate: new Date(),
        update: 'add'
      }]);
    }
  });
  return sortTags(newTags);
}
