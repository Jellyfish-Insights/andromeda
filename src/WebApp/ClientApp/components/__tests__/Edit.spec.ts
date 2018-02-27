import {
  Video,
  VideoEdit,
  VideoEdits,
  AddOrRemove,
  EditType,
  SourceObjectType,
  Source,
  Tag
} from '../../types';
import {
  editTitle,
  addOrRemoveTag,
  moveSource,
  onlyRemoveSource,
  onlyAddSource,
  createNewVideo,
  setArchive,
  mergeVideos,
  addSourceObjectsToSource,
  getUpdateTags,
  UpdateTag
} from '../Edit';
import { getSource } from '../admin/__tests__/VideoManagementEdit.spec';
import update from 'immutability-helper';
import * as _ from 'underscore';
import { compareString } from '../../reducers/helpers';
import { sortTags } from '../../components/VideoItem';

const videos: Video[] = [
  {
    id: '1',
    updateDate: new Date(),
    archived: false,
    title: 'This is a Video Title 1',
    playlists: ['Playlist A'],
    tags: [{
      type: 'Tone',
      updateDate: new Date(),
      value: 'Angry'
    }, {
      type: 'Topic',
      updateDate: new Date(),
      value: 'Topic 1'
    }, {
      type: 'Length',
      updateDate: new Date(),
      value: '16-44min'
    }],
    sources: [{
      sourceName: 'youtube',
      videosCount: 3,
      sourceObjects: [
        getSource(1, 'youtube', SourceObjectType.Video),
        getSource(2, 'youtube', SourceObjectType.Video),
        getSource(3, 'youtube', SourceObjectType.Video),
        getSource(4, 'youtube', SourceObjectType.Campaign),
      ]
    }, {
      sourceName: 'facebook',
      videosCount: 2,
      sourceObjects: [
        getSource(5, 'facebook', SourceObjectType.Video),
        getSource(6, 'facebook', SourceObjectType.Video),
        getSource(7, 'facebook', SourceObjectType.Campaign),
      ]
    }],
    thumbnailLink: 'https://via.placeholder.com/100x100',
    publishedAt: new Date('2015-03-23T00:00:00-03:00')
  }, {
    id: '2',
    updateDate: new Date(),
    title: 'This is a Video Title 2',
    archived: false,
    playlists: ['Playlist A'],
    tags: [{
      type: 'Tone',
      updateDate: new Date(),
      'value': 'Happy'
    }, {
      type: 'Topic',
      updateDate: new Date(),
      'value': 'Topic 1'
    }, {
      type: 'Medium',
      updateDate: new Date(),
      'value': 'Medium 2'
    }, {
      type: 'Length',
      updateDate: new Date(),
      'value': '45+min'
    }],
    sources: [{
      sourceName: 'facebook',
      videosCount: 3,
      sourceObjects: [
        getSource(8, 'facebook', SourceObjectType.Campaign),
        getSource(9, 'facebook', SourceObjectType.Video),
        getSource(10, 'facebook', SourceObjectType.Video),
        getSource(11, 'facebook', SourceObjectType.Video),
      ]
    }],
    thumbnailLink: 'https://via.placeholder.com/100x100',
    publishedAt: new Date('2016-04-07T00:00:00-03:00')
  },
  {
    id: '3',
    updateDate: new Date(),
    title: 'This is a Video Title 3',
    archived: false,
    playlists: ['Playlist A'],
    tags: [{
      type: 'Tone',
      updateDate: new Date(),
      'value': 'Happy'
    }, {
      type: 'Topic',
      updateDate: new Date(),
      'value': 'Topic 1'
    }, {
      type: 'Medium',
      updateDate: new Date(),
      'value': 'Medium 2'
    }, {
      type: 'Length',
      updateDate: new Date(),
      'value': '45+min'
    }],
    sources: [{
      sourceName: 'facebook',
      videosCount: 1,
      sourceObjects: [
        getSource(12, 'facebook', SourceObjectType.Video),
      ]
    }],
    thumbnailLink: 'https://via.placeholder.com/100x100',
    publishedAt: new Date('2016-04-07T00:00:00-03:00')
  }
];

it('Test adding a tag of an existing type', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        metaTags: {
          Tone: 'Happy'
        }
      }
    }
  };
  const finalState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Happy'
    },
    AddOrRemove.Add,
    initialState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Test adding adding a tag of an existing type and then removing it', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        metaTags: {
          Tone: null
        }
      }
    }
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Happy'
    },
    AddOrRemove.Add,
    initialState
  );
  const finalState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Happy'
    },
    AddOrRemove.Remove,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Test editing title and adding tag to multiple videos', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        title: 'BBB',
        metaTags: { Medium: 'Medium 1' }
      },
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        title: 'AAA',
        metaTags: {
          Medium: 'Medium 1'
        }
      }
    }
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Medium',
      updateDate: new Date(),
      value: 'Medium 1'
    },
    AddOrRemove.Add,
    initialState
  );
  const thirdState = addOrRemoveTag(
    videos[1],
    {
      type: 'Medium',
      updateDate: new Date(),
      value: 'Medium 1'
    },
    AddOrRemove.Add,
    secondState
  );
  const fourthState = editTitle(
    videos[0],
    'BBB',
    thirdState
  );
  const finalState = editTitle(
    videos[1],
    'AAA',
    fourthState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Try to remove a tag that does not exist in video', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        metaTags: {
          Medium: null
        }
      }
    }
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Medium',
      updateDate: new Date(),
      value: 'Medium 2'
    },
    AddOrRemove.Remove,
    initialState
  );
  const finalState = addOrRemoveTag(
    videos[1],
    {
      type: 'Medium',
      updateDate: new Date(),
      value: 'Medium 2'
    },
    AddOrRemove.Remove,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Try to remove a tag that does not exist in video and then add it to both videos', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        metaTags: {
          Medium: 'Medium 2'
        }
      },
    }
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Medium',
      updateDate: new Date(),
      value: 'Medium 2'
    },
    AddOrRemove.Remove,
    initialState
  );
  const thirdState = addOrRemoveTag(
    videos[1],
    {
      type: 'Medium',
      updateDate: new Date(),
      value: 'Medium 2'
    },
    AddOrRemove.Remove,
    secondState
  );
  const fourthState = addOrRemoveTag(
    videos[0],
    {
      type: 'Medium',
      updateDate: new Date(),
      value: 'Medium 2'
    },
    AddOrRemove.Add,
    thirdState
  );
  const finalState = addOrRemoveTag(
    videos[1],
    {
      type: 'Medium',
      updateDate: new Date(),
      value: 'Medium 2'
    },
    AddOrRemove.Add,
    fourthState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Try to assign a single tag value for videos with different values previously assigned for the tag type', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        metaTags: {
          Tone: 'Happy'
        }
      },
    }
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Angry'
    },
    AddOrRemove.Remove,
    initialState
  );
  const thirdState = addOrRemoveTag(
    videos[1],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Happy'
    },
    AddOrRemove.Remove,
    secondState
  );
  const fourthState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Happy'
    },
    AddOrRemove.Add,
    thirdState
  );
  const finalState = addOrRemoveTag(
    videos[1],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Happy'
    },
    AddOrRemove.Add,
    fourthState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Try to remove a tag that does not exist in video, but another value of same type exists', () => {
  const initialState: VideoEdits = { edits: {} };
  // Removal just check type, so if video has type, it will remove
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        metaTags: {
          Tone: null
        }
      }
    }
  };
  const finalState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Happy'
    },
    AddOrRemove.Remove,
    initialState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Add a tag that already exists', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
    }
  };
  const finalState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Angry'
    },
    AddOrRemove.Add,
    initialState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Remove a tag, add it and remove again', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        metaTags: {
          Tone: null
        }
      }
    }
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date()
      , value: 'Angry'
    },
    AddOrRemove.Remove,
    initialState
  );
  const thirdState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(), value: 'Angry'
    },
    AddOrRemove.Add,
    secondState
  );
  const finalState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(), value: 'Angry'
    },
    AddOrRemove.Remove,
    thirdState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Add meta tag with another value for exiting type multiple times', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        metaTags: {
          Tone: 'Happy'
        }
      }
    }
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(), value: 'Frustrated'
    },
    AddOrRemove.Add,
    initialState
  );
  const finalState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(), value: 'Happy'
    },
    AddOrRemove.Add,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Add and remove generic tag', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {}
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Generic',
      updateDate: new Date(), value: 'Something'
    },
    AddOrRemove.Add,
    initialState
  );
  const finalState = addOrRemoveTag(
    videos[0],
    {
      type: 'Generic',
      updateDate: new Date(), value: 'Something'
    },
    AddOrRemove.Remove,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Remove, add and then add another metatag', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        metaTags: {
          Tone: 'Frustrated'
        }
      }
    }
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(), value: 'Angry'
    },
    AddOrRemove.Remove,
    initialState
  );
  const thirdState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(), value: 'Happy'
    },
    AddOrRemove.Add,
    secondState
  );
  const finalState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(), value: 'Frustrated'
    },
    AddOrRemove.Add,
    thirdState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move source video from video 1 to 2', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(1, 'youtube', SourceObjectType.Video);
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove.sourceId]
      },
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        addedVideos: [sourceToMove.sourceId]
      }
    }
  };
  const finalState = moveSource(
    videos[0],
    videos[1],
    sourceToMove,
    initialState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move source video from video 1 to 2 and back to 1', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(1, 'youtube', SourceObjectType.Video);
  const expectedFinalState: VideoEdits = {
    edits: {}
  };
  const secondState = moveSource(
    videos[0],
    videos[1],
    sourceToMove,
    initialState
  );
  const finalState = moveSource(
    videos[1],
    videos[0],
    sourceToMove,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move source campaign from video 1 to 2', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(4, 'youtube', SourceObjectType.Campaign);
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        removedCampaigns: [sourceToMove.sourceId]
      },
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        addedCampaigns: [sourceToMove.sourceId]
      }
    }
  };
  const finalState = moveSource(
    videos[0],
    videos[1],
    sourceToMove,
    initialState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move source campaign from video 1 to 2 and back to 1', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(4, 'youtube', SourceObjectType.Campaign);
  const expectedFinalState: VideoEdits = {
    edits: {}
  };
  const secondState = moveSource(
    videos[0],
    videos[1],
    sourceToMove,
    initialState
  );
  const finalState = moveSource(
    videos[1],
    videos[0],
    sourceToMove,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move source video from video 1 to 2 and from 2 to 3', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(1, 'youtube', SourceObjectType.Video);
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove.sourceId]
      },
      '3': {
        updateDate: videos[2].updateDate,
        flag: EditType.Update,
        addedVideos: [sourceToMove.sourceId]
      }
    }
  };
  const secondState = moveSource(
    videos[0],
    videos[1],
    sourceToMove,
    initialState
  );
  const finalState = moveSource(
    videos[1],
    videos[2],
    sourceToMove,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move 2 source videos from video 1 and 2 to 3', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove1 = getSource(1, 'youtube', SourceObjectType.Video);
  const sourceToMove2 = getSource(9, 'facebook', SourceObjectType.Video);
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove1.sourceId]
      },
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove2.sourceId]
      },
      '3': {
        updateDate: videos[2].updateDate,
        flag: EditType.Update,
        addedVideos: [sourceToMove1.sourceId, sourceToMove2.sourceId]
      }
    }
  };
  const secondState = moveSource(
    videos[0],
    videos[2],
    sourceToMove1,
    initialState
  );
  const finalState = moveSource(
    videos[1],
    videos[2],
    sourceToMove2,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move source video from video 1 to unassociated and from unassociated to video 2 and then video 1', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(1, 'youtube', SourceObjectType.Video);
  const expectedFinalState: VideoEdits = {
    edits: {}
  };
  const secondState = onlyRemoveSource(
    videos[0],
    sourceToMove,
    initialState
  );
  const thirdState = onlyAddSource(
    videos[1],
    sourceToMove,
    secondState
  );
  const finalState = moveSource(
    videos[1],
    videos[0],
    sourceToMove,
    thirdState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move all source videos from a video to another', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(12, 'facebook', SourceObjectType.Video);
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        addedVideos: [sourceToMove.sourceId]
      },
      '3': {
        updateDate: videos[2].updateDate,
        flag: EditType.Delete,
        removedVideos: [sourceToMove.sourceId]
      }
    }
  };
  const finalState = moveSource(
    videos[2],
    videos[0],
    sourceToMove,
    initialState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move all source videos from a video to another and back again', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(12, 'facebook', SourceObjectType.Video);
  const expectedFinalState: VideoEdits = {
    edits: {}
  };
  const secondState = moveSource(
    videos[2],
    videos[0],
    sourceToMove,
    initialState
  );
  const finalState = moveSource(
    videos[0],
    videos[2],
    sourceToMove,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Move all source videos from a video to another and then move another to the empty one', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove1 = getSource(12, 'facebook', SourceObjectType.Video);
  const sourceToMove2 = getSource(11, 'facebook', SourceObjectType.Video);
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        addedVideos: [sourceToMove1.sourceId]
      },
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove2.sourceId]
      },
      '3': {
        updateDate: videos[2].updateDate,
        flag: EditType.Update,
        addedVideos: [sourceToMove2.sourceId],
        removedVideos: [sourceToMove1.sourceId]
      }
    }
  };
  const secondState = moveSource(
    videos[2],
    videos[0],
    sourceToMove1,
    initialState
  );
  const finalState = moveSource(
    videos[1],
    videos[2],
    sourceToMove2,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Create a new video, add a tag to it, check, remove it and check again', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(11, 'facebook', SourceObjectType.Video);
  const [newVideoState, newVideoEdit] = createNewVideo('NEW-VIDEO-1', sourceToMove);
  const expectedIntermediaryState: VideoEdits = {
    edits: {
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove.sourceId]
      },
      'NEW-VIDEO-1': {
        updateDate: newVideoState.updateDate,
        flag: EditType.New,
        addedVideos: [sourceToMove.sourceId],
        title: 'Title 11',
        archive: false,
        metaTags: {
          'Tone': 'Happy'
        },
      }
    }
  };
  const secondState = onlyRemoveSource(videos[1], sourceToMove, initialState);
  const thirdState = update(secondState, {
    edits: {
      $merge: {
        'NEW-VIDEO-1': newVideoEdit
      }
    }
  });
  const intermediaryState = addOrRemoveTag(
    newVideoState,
    {
      type: 'Tone',
      updateDate: new Date('2018-01-01'),
      value: 'Happy'
    },
    AddOrRemove.Add,
    thirdState
  );
  expect(intermediaryState).toEqual(expectedIntermediaryState);

  const expectedFinalState: VideoEdits = {
    edits: {
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove.sourceId]
      },
      'NEW-VIDEO-1': {
        updateDate: newVideoState.updateDate,
        flag: EditType.New,
        addedVideos: [sourceToMove.sourceId],
        title: 'Title 11',
        archive: false,
      }
    }
  };

  const finalState = addOrRemoveTag(
    newVideoState,
    {
      type: 'Tone',
      updateDate: new Date('2018-01-01'),
      value: 'Happy'
    },
    AddOrRemove.Remove,
    intermediaryState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Archive two videos', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        archive: true
      },
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        archive: true
      },
    }
  };
  const finalState = setArchive(
    _.object(['1', '2'], [videos[0], videos[1]]),
    true,
    initialState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Add tag to video 1 and archive videos 1 and 2', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        archive: true,
        metaTags: {
          'Tone': 'Happy'
        }
      },
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        archive: true
      },
    }
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Tone',
      updateDate: new Date(),
      value: 'Happy'
    },
    AddOrRemove.Add,
    initialState
  );
  const finalState = setArchive(
    _.object(['1', '2'], [videos[0], videos[1]]),
    true,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Create a new video and archive it and another video', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(11, 'facebook', SourceObjectType.Video);
  const [newVideoState, newVideoEdit] = createNewVideo('NEW-VIDEO-1', sourceToMove);
  const expectedIntermediaryState: VideoEdits = {
    edits: {
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove.sourceId]
      },
      'NEW-VIDEO-1': {
        updateDate: newVideoState.updateDate,
        flag: EditType.New,
        addedVideos: [sourceToMove.sourceId],
        title: 'Title 11',
        archive: false,
        metaTags: {
          'Tone': 'Happy'
        },
      }
    }
  };
  const secondState = onlyRemoveSource(videos[1], sourceToMove, initialState);
  const thirdState = update(secondState, {
    edits: {
      $merge: {
        'NEW-VIDEO-1': newVideoEdit
      }
    }
  });
  const intermediaryState = addOrRemoveTag(
    newVideoState,
    {
      type: 'Tone',
      updateDate: new Date('2018-01-01'),
      value: 'Happy'
    },
    AddOrRemove.Add,
    thirdState
  );
  expect(intermediaryState).toEqual(expectedIntermediaryState);

  const expectedFinalState: VideoEdits = {
    edits: {
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove.sourceId],
        archive: true
      },
      'NEW-VIDEO-1': {
        updateDate: newVideoState.updateDate,
        flag: EditType.New,
        addedVideos: [sourceToMove.sourceId],
        title: 'Title 11',
        archive: true,
        metaTags: {
          'Tone': 'Happy'
        }
      }
    }
  };

  const finalState = setArchive(
    _.object(['NEW-VIDEO-1', '2'], [newVideoState, videos[1]]),
    true,
    intermediaryState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Merge videos 2 and 1', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Delete,
        removedCampaigns: ['Y4', 'Y7'],
        removedVideos: ['Y1', 'Y2', 'Y3', 'Y5', 'Y6']
      },
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        archive: false,
        addedCampaigns: ['Y4', 'Y7'],
        addedVideos: ['Y1', 'Y2', 'Y3', 'Y5', 'Y6']
      },
    }
  };
  const [_, finalState] = mergeVideos(
    [videos[1], videos[0]],
    initialState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Merge videos 1 and 2', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {
      '1': {
        updateDate: videos[0].updateDate,
        flag: EditType.Update,
        archive: false,
        addedCampaigns: ['Y8'],
        addedVideos: ['Y9', 'Y10', 'Y11']
      },
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Delete,
        removedCampaigns: ['Y8'],
        removedVideos: ['Y9', 'Y10', 'Y11']
      },
    }
  };
  const [_, finalState] = mergeVideos(
    [videos[0], videos[1]],
    initialState
  );
  expect(finalState).toEqual(expectedFinalState);
});

it('Test adding source objects to a Source array', () => {
  const initialSources: Source[] = [{
    sourceName: 'youtube',
    videosCount: 3,
    sourceObjects: [
      getSource(1, 'youtube', SourceObjectType.Video),
      getSource(2, 'youtube', SourceObjectType.Video),
      getSource(3, 'youtube', SourceObjectType.Video),
      getSource(4, 'youtube', SourceObjectType.Campaign),
    ]
  }, {
    sourceName: 'facebook',
    videosCount: 2,
    sourceObjects: [
      getSource(5, 'facebook', SourceObjectType.Video),
      getSource(6, 'facebook', SourceObjectType.Video),
      getSource(7, 'facebook', SourceObjectType.Campaign),
    ]
  }]
    .sort((x, y) => compareString(x.sourceName, y.sourceName));

  const addSourceObjects = [
    getSource(8, 'facebook', SourceObjectType.Campaign),
    getSource(9, 'facebook', SourceObjectType.Video),
    getSource(10, 'facebook', SourceObjectType.Video),
    getSource(11, 'facebook', SourceObjectType.Video),
  ];

  const expectedEndSources: Source[] = [{
    sourceName: 'youtube',
    videosCount: 3,
    sourceObjects: [
      getSource(1, 'youtube', SourceObjectType.Video),
      getSource(2, 'youtube', SourceObjectType.Video),
      getSource(3, 'youtube', SourceObjectType.Video),
      getSource(4, 'youtube', SourceObjectType.Campaign),
    ]
  }, {
    sourceName: 'facebook',
    videosCount: 5,
    sourceObjects: [
      getSource(5, 'facebook', SourceObjectType.Video),
      getSource(6, 'facebook', SourceObjectType.Video),
      getSource(7, 'facebook', SourceObjectType.Campaign),
      getSource(8, 'facebook', SourceObjectType.Campaign),
      getSource(9, 'facebook', SourceObjectType.Video),
      getSource(10, 'facebook', SourceObjectType.Video),
      getSource(11, 'facebook', SourceObjectType.Video),
    ]
  }]
    .sort((x, y) => compareString(x.sourceName, y.sourceName));

  const endSources = addSourceObjectsToSource(addSourceObjects, initialSources);
  expect(endSources).toEqual(expectedEndSources);

});

it('Create a new video based on source from video 1 and merge it with video 1', () => {
  const initialState: VideoEdits = { edits: {} };
  const sourceToMove = getSource(11, 'facebook', SourceObjectType.Video);
  const [newVideoState, newVideoEdit] = createNewVideo('NEW-VIDEO-1', sourceToMove);
  const expectedIntermediaryState: VideoEdits = {
    edits: {
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Update,
        removedVideos: [sourceToMove.sourceId]
      },
      'NEW-VIDEO-1': {
        updateDate: newVideoState.updateDate,
        flag: EditType.New,
        addedVideos: [sourceToMove.sourceId],
        title: 'Title 11',
        archive: false,
        metaTags: {
          'Tone': 'Happy'
        },
      }
    }
  };
  const secondState = onlyRemoveSource(videos[1], sourceToMove, initialState);
  const thirdState = update(secondState, {
    edits: {
      $merge: {
        'NEW-VIDEO-1': newVideoEdit
      }
    }
  });
  const intermediaryState = addOrRemoveTag(
    newVideoState,
    {
      type: 'Tone',
      updateDate: new Date('2018-01-01'),
      value: 'Happy'
    },
    AddOrRemove.Add,
    thirdState
  );
  expect(intermediaryState).toEqual(expectedIntermediaryState);
  const [_, finalState] = mergeVideos(
    [
      newVideoState,
      {
        ...(videos[1]),
        sources: [{
          sourceName: 'facebook',
          videosCount: 2,
          sourceObjects: [
            getSource(8, 'facebook', SourceObjectType.Campaign),
            getSource(9, 'facebook', SourceObjectType.Video),
            getSource(10, 'facebook', SourceObjectType.Video),
          ]
        }]
      }
    ],
    intermediaryState
  );
  const expectedFinalState: VideoEdits = {
    edits: {
      '2': {
        updateDate: videos[1].updateDate,
        flag: EditType.Delete,
        removedVideos:
          videos[1]
            .sources[0]
            .sourceObjects
            .filter(x => x.type === SourceObjectType.Video)
            .map(x => x.sourceId)
        ,
        removedCampaigns:
          videos[1]
            .sources[0]
            .sourceObjects
            .filter(x => x.type === SourceObjectType.Campaign)
            .map(x => x.sourceId)
      },
      'NEW-VIDEO-1': {
        updateDate: newVideoState.updateDate,
        flag: EditType.New,
        addedVideos:
          videos[1]
            .sources[0]
            .sourceObjects
            .filter(x => x.type === SourceObjectType.Video)
            .map(x => x.sourceId)
        ,
        addedCampaigns:
          videos[1]
            .sources[0]
            .sourceObjects
            .filter(x => x.type === SourceObjectType.Campaign)
            .map(x => x.sourceId),
        title: 'Title 11',
        archive: false,
        metaTags: {
          'Tone': 'Happy'
        },
      }
    }
  };
  expect(finalState).toEqual(expectedFinalState);
});

it('Get tags with proper new/delete info for VideoItem', () => {
  const originalTags: Tag[] = [{
    type: 'Tone',
    updateDate: new Date(),
    value: 'Angry'
  }, {
    type: 'Topic',
    updateDate: new Date(),
    value: 'Topic 1'
  }, {
    type: 'Length',
    updateDate: new Date(),
    value: '16-44min'
  }, {
    type: 'Generic',
    updateDate: new Date(),
    value: 'kkk'
  }];
  const videoEdit: VideoEdit = {
    updateDate: new Date(),
    metaTags: { 'Tone': 'Happy' },
    addedGenericTags: ['ttt', 'ooo'],
    removedGenericTags: ['kkk'],
    flag: EditType.Update,
  };
  const finalState = getUpdateTags(originalTags, videoEdit)
    .map(x => { return { ...x, updateDate: new Date('2015-03-23T00:00:00-03:00') }; });
  const expectedFinalState: UpdateTag[] = sortTags([{
    type: 'Tone',
    updateDate: new Date('2015-03-23T00:00:00-03:00'),
    value: 'Angry',
    update: 'remove'
  }, {
    type: 'Tone',
    updateDate: new Date('2015-03-23T00:00:00-03:00'),
    value: 'Happy',
    update: 'add'
  }, {
    type: 'Topic',
    updateDate: new Date('2015-03-23T00:00:00-03:00'),
    value: 'Topic 1'
  }, {
    type: 'Length',
    updateDate: new Date('2015-03-23T00:00:00-03:00'),
    value: '16-44min'
  }, {
    type: 'Generic',
    updateDate: new Date('2015-03-23T00:00:00-03:00'),
    value: 'kkk',
    update: 'remove'
  }, {
    type: 'Generic',
    updateDate: new Date('2015-03-23T00:00:00-03:00'),
    value: 'ttt',
    update: 'add'
  }, {
    type: 'Generic',
    updateDate: new Date('2015-03-23T00:00:00-03:00'),
    value: 'ooo',
    update: 'add'
  }]);
  expect(finalState).toEqual(expectedFinalState);
});

it('Get tags with proper new/delete info for VideoItem when there are no edits', () => {
  const originalTags: Tag[] = [{
    type: 'Tone',
    updateDate: new Date(),
    value: 'Angry'
  }, {
    type: 'Topic',
    updateDate: new Date(),
    value: 'Topic 1'
  }, {
    type: 'Length',
    updateDate: new Date(),
    value: '16-44min'
  }, {
    type: 'Generic',
    updateDate: new Date(),
    value: 'kkk'
  }];
  let videoEdit: VideoEdit;
  const finalState = getUpdateTags(originalTags, videoEdit)
    .map(x => { return { ...x, updateDate: new Date('2015-03-23T00:00:00-03:00') }; });
  const expectedFinalState: UpdateTag[] = sortTags(originalTags)
    .map(x => { return { ...x, updateDate: new Date('2015-03-23T00:00:00-03:00') }; });
  expect(finalState).toEqual(expectedFinalState);
});

it('Add meta tag of new type to video and try to remove it', () => {
  const initialState: VideoEdits = { edits: {} };
  const expectedFinalState: VideoEdits = {
    edits: {}
  };
  const secondState = addOrRemoveTag(
    videos[0],
    {
      type: 'Target Audience',
      updateDate: new Date('2015-03-23T00:00:00-03:00'),
      value: 'Secular left'
    },
    AddOrRemove.Add,
    initialState
  );
  const finalState = addOrRemoveTag(
    videos[0],
    {
      type: 'Target Audience',
      updateDate: new Date('2015-03-23T00:00:00-03:00'),
      value: 'Secular left'
    },
    AddOrRemove.Remove,
    secondState
  );
  expect(finalState).toEqual(expectedFinalState);
});
