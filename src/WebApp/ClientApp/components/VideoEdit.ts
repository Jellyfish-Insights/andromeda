import { arrayToObject, capitalizeFirst } from '../reducers/helpers';
import { Tag, AddOrRemove, VideoEdits } from '../types';
import { addOrRemoveTag } from './Edit';
import { VideoItemState } from './VideoList';
import {
  VideoItemStateDictionary,
  VideoDictionary
} from './VideoSort';
import * as _ from 'underscore';

export function updateTagToSelectedVideos(
  type: string,
  value: string,
  videoItemStates: VideoItemStateDictionary,
  inVideoEdits: VideoEdits,
  originalVideos: VideoDictionary,
  updateVideosObject: (newObject: VideoItemStateDictionary, edits: VideoEdits) => void,
) {
  let tag = { type: capitalizeFirst(type), value: value.trim() } as Tag;
  let videoIds = _.keys(videoItemStates);
  let selectedVideos = videoIds.map(videoId => videoItemStates[videoId]).filter(video => video.selected);
  let videoEdits = { ...inVideoEdits };
  const tagIsPresentInAllVideos = selectedVideos
    .every(video =>
      video
        .tags
        .filter(t => t.type === tag.type)
        .map(t => t.value)
        .includes(tag.value)
    );
  var updatedVideoArray = selectedVideos.map(video => {

    const addOrRemoveOp = tagIsPresentInAllVideos ? AddOrRemove.Remove : AddOrRemove.Add;

    videoEdits = addOrRemoveTag(originalVideos[video.id], tag, addOrRemoveOp, videoEdits);
    const tagsWithPossibleRepeated = tagIsPresentInAllVideos
      ? video.tags.filter(x => !(x.type === tag.type && x.value === tag.value))
      : ([] as Tag[]).concat(video.tags).concat(tag);

    // Limit one meta tag value per type per video and allow multiple generic tags
    const tags = tagsWithPossibleRepeated.filter(t => {
      return (
        t.type.toLowerCase() !== type.toLowerCase()
        || t.value.toLowerCase() === value.toLowerCase()
        || t.type.toLowerCase() === 'generic'
      );
    });
    return { ...video, tags };
  });
  let updatedVideosObject: VideoItemStateDictionary = arrayToObject(updatedVideoArray, (v: VideoItemState) => v.id);
  updateVideosObject(updatedVideosObject, videoEdits);
}
