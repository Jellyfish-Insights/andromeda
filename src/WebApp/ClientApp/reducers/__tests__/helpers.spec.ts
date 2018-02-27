import {
  Video,
} from '../../types';
import { getFirstPublishDate } from '../helpers';

const videos: Video[] = [
  {
    id: '1',
    updateDate: new Date(),
    archived: false,
    title: 'This is a Video Title 1',
    playlists: ['Playlist A'],
    tags: [],
    sources: [],
    thumbnailLink: 'https://via.placeholder.com/100x100',
    publishedAt: new Date('2016-04-07T00:00:00-03:00')
  }, {
    id: '2',
    updateDate: new Date(),
    title: 'This is a Video Title 2',
    archived: false,
    playlists: ['Playlist A'],
    tags: [],
    sources: [],
    thumbnailLink: 'https://via.placeholder.com/100x100',
    publishedAt: new Date('2015-03-23T00:00:00-03:00')
  },
  {
    id: '3',
    updateDate: new Date(),
    title: 'This is a Video Title 3',
    archived: false,
    playlists: ['Playlist A'],
    tags: [],
    sources: [],
    thumbnailLink: 'https://via.placeholder.com/100x100',
    publishedAt: new Date('2018-04-07T00:00:00-03:00')
  }
];

it('Test getting minimum date in a list of videos', () => {
  let expected = new Date('2015-03-23T00:00:00-03:00');
  let firstDate = getFirstPublishDate(videos);
  expect(firstDate).toEqual(expected);
});
