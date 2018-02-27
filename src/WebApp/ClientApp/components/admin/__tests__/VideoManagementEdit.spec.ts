import { addSourceObject, removeSourceObject } from '../VideoManagementEdit';
import { SourceObject, SourceObjectType, Source } from '../../../types';
import { compareString } from '../../../reducers/helpers';

export function getSource(id: number, sourceName: string, type: SourceObjectType): SourceObject {
  return {
    sourceId: 'Y' + id,
    sourceName,
    title: 'Title ' + id,
    thumbnailLink: 'ThumbnailLink ' + id,
    type,
    links: [],
    lengthInSeconds: type === SourceObjectType.Video ? 200 : undefined,
    publishedAt: new Date('2015-03-23T00:00:00-03:00'),
    publishedStatus: true
  };
}

const youtubeSources: SourceObject[] = [
  getSource(1, 'youtube', SourceObjectType.Video),
  getSource(2, 'youtube', SourceObjectType.Video),
  getSource(3, 'youtube', SourceObjectType.Video),
  getSource(4, 'youtube', SourceObjectType.Campaign),
];

const facebookSources: SourceObject[] = [
  getSource(1, 'facebook', SourceObjectType.Video),
  getSource(2, 'facebook', SourceObjectType.Video),
  getSource(3, 'facebook', SourceObjectType.Video),
  getSource(4, 'facebook', SourceObjectType.Campaign),
  getSource(5, 'facebook', SourceObjectType.Campaign),
];
const sources: Source[] = [
  {
    sourceName: 'facebook',
    videosCount: facebookSources.filter(s => s.type === SourceObjectType.Video).length,
    sourceObjects: facebookSources
  },
  {
    sourceName: 'youtube',
    videosCount: youtubeSources.filter(s => s.type === SourceObjectType.Video).length,
    sourceObjects: youtubeSources
  }
];

it('Add youtube video source object', () => {
  const toAdd = getSource(10, 'youtube', SourceObjectType.Video);
  const expectedResult = [
    {
      sourceName: 'facebook',
      videosCount: facebookSources.filter(s => s.type === SourceObjectType.Video).length,
      sourceObjects: facebookSources
    },
    {
      sourceName: 'youtube',
      videosCount: (youtubeSources.concat([toAdd])).filter(s => s.type === SourceObjectType.Video).length,
      sourceObjects: youtubeSources.concat([toAdd]).sort((s1, s2) => compareString(s1.sourceId, s2.sourceId))
    }
  ];

  let result = addSourceObject(sources, toAdd);

  expect(result).toEqual(expectedResult);
});

it('Add facebook video source object', () => {
  const toAdd = getSource(10, 'facebook', SourceObjectType.Video);
  const expectedResult = [
    {
      sourceName: 'facebook',
      videosCount: facebookSources.concat([toAdd]).filter(s => s.type === SourceObjectType.Video).length,
      sourceObjects: facebookSources.concat([toAdd]).sort((s1, s2) => compareString(s1.sourceId, s2.sourceId))
    },
    {
      sourceName: 'youtube',
      videosCount: youtubeSources.filter(s => s.type === SourceObjectType.Video).length,
      sourceObjects: youtubeSources
    }
  ];

  let result = addSourceObject(sources, toAdd);

  expect(result).toEqual(expectedResult);
});

it('Add facebook video source object when initial sources does not have facebook', () => {
  const toAdd = getSource(10, 'facebook', SourceObjectType.Video);
  const initialSources = [
    {
      sourceName: 'youtube',
      videosCount: youtubeSources.filter(s => s.type === SourceObjectType.Video).length,
      sourceObjects: youtubeSources
    }
  ];

  const expectedResult = [
    {
      sourceName: 'facebook',
      videosCount: 1,
      sourceObjects: [toAdd]
    },
    {
      sourceName: 'youtube',
      videosCount: youtubeSources.filter(s => s.type === SourceObjectType.Video).length,
      sourceObjects: youtubeSources
    }
  ];

  let result = addSourceObject(initialSources, toAdd);

  expect(result).toEqual(expectedResult);
});

it('Add youtube video source object when initial sources does not have youtube', () => {
  const toAdd = getSource(10, 'youtube', SourceObjectType.Video);
  const initialSources = [
    {
      sourceName: 'facebook',
      videosCount: facebookSources.filter(s => s.type === SourceObjectType.Video).length,
      sourceObjects: facebookSources
    }
  ];

  const expectedResult = [
    {
      sourceName: 'facebook',
      videosCount: facebookSources.filter(s => s.type === SourceObjectType.Video).length,
      sourceObjects: facebookSources
    },
    {
      sourceName: 'youtube',
      videosCount: 1,
      sourceObjects: [toAdd]
    },
  ];

  let result = addSourceObject(initialSources, toAdd);

  expect(result).toEqual(expectedResult);
});

it('Remove video source object', () => {
  const toRemove = youtubeSources.find(s => s.sourceId === 'Y3');
  const expectedResult = [
    {
      sourceName: 'facebook',
      videosCount: facebookSources.filter(s => s.type === SourceObjectType.Video).length,
      sourceObjects: facebookSources
    },
    {
      sourceName: 'youtube',
      videosCount: youtubeSources.filter(s => s.type === SourceObjectType.Video && s.sourceId !== 'Y3').length,
      sourceObjects:
        youtubeSources
          .filter(s => s.sourceId !== 'Y3')
          .sort((s1, s2) => compareString(s1.sourceId, s2.sourceId))
    }
  ];

  let result = removeSourceObject(sources, toRemove);

  expect(result).toEqual(expectedResult);
});
