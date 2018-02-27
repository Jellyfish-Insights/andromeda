import * as React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as _ from 'underscore';
import { prependZeros } from '../reducers/helpers';
import * as Select from 'react-select';
import { Option } from 'react-select';
import {
  SortingDirection,
  SortingType,
  Sorting,
  VideoItemStateDictionary
} from './VideoSort';

const VIDEO_PAGE_SIZE = 20;

export interface PaginationProps {
  videoPage: number;
  sortedVideoIds: string[];
  videoItemStates: VideoItemStateDictionary;
  updateVideoPage: (videoPage: number) => void;
}

export const Pagination = (props: PaginationProps) => {
  // Regarding pagination behaviour, the following should be noticed:
  // - changing the filters should change the total amount of videos presented, so the
  // "of XXX" part will change
  // - if the user changes the sorting method, the numbers should be kept the same, but the order of
  // the videos should change, so he may see different videos for the page he is currently in
  const { videoPage, sortedVideoIds, videoItemStates, updateVideoPage } = props;
  const availableSortedVideoIdsBeforeSlice = _.intersection(sortedVideoIds, _.keys(videoItemStates));
  const numberOfVideos = availableSortedVideoIdsBeforeSlice.length;
  const log10 = Math.trunc(Math.log10(numberOfVideos));
  const lastVideoPage = Math.ceil(numberOfVideos / VIDEO_PAGE_SIZE - 1);
  const firstVideoInPage = videoPage * VIDEO_PAGE_SIZE + 1;
  let lastVideoInPage = (videoPage + 1) * VIDEO_PAGE_SIZE;
  if (lastVideoInPage > numberOfVideos) {
    lastVideoInPage = numberOfVideos;
  }
  return (
    <div className="video-pagination">
      <a
        onClick={() => videoPage <= 0 ? updateVideoPage(lastVideoPage) : updateVideoPage(videoPage - 1)}
        style={{ cursor: 'pointer' }}
      >
        <FontAwesomeIcon icon="chevron-left" />
      </a>
      <span>{prependZeros(log10, firstVideoInPage)} - {prependZeros(log10, lastVideoInPage)}</span>
      <span style={{ marginLeft: 0, fontWeight: 300 }}>of {numberOfVideos}</span>
      <a
        onClick={() => videoPage >= lastVideoPage ?
          updateVideoPage(0) : updateVideoPage(videoPage + 1)}
        style={{ cursor: 'pointer' }}
      >
        <FontAwesomeIcon icon="chevron-right" />
      </a>
    </div>
  );
};

export function getPageVideoIds(
  sortedVideoIds: string[],
  videoItemStates: VideoItemStateDictionary,
  videoPage: number
): string[] {
  const availableSortedVideoIdsBeforeSlice = _.intersection(sortedVideoIds, _.keys(videoItemStates));
  return availableSortedVideoIdsBeforeSlice
    .slice(videoPage * VIDEO_PAGE_SIZE, (videoPage + 1) * VIDEO_PAGE_SIZE);
}

export interface OrderByProps {
  sorting: Sorting;
  applySort: (sorting: Sorting) => void;
}
export const OrderBy = (props: OrderByProps) => {
  const { sorting, applySort } = props;
  const { type, direction } = sorting;
  const valueDict: { [index: string]: [SortingType, SortingDirection, string] } = {
    'ascending': [SortingType.Title, SortingDirection.Up, 'A...Z'],
    'descending': [SortingType.Title, SortingDirection.Down, 'Z...A'],
    'playlist_ascending': [SortingType.Playlist, SortingDirection.Up, 'Playlist A...Z'],
    'playlist_descending': [SortingType.Playlist, SortingDirection.Down, 'Playlist Z...A'],
    'oldest': [SortingType.Date, SortingDirection.Up, 'Oldest'],
    'newest': [SortingType.Date, SortingDirection.Down, 'Newest'],
    'other': [SortingType.Title, SortingDirection.Disabled, 'Other'],
  };
  const options = _.keys(valueDict).map((k: string) => ({ value: k, label: valueDict[k][2] }));
  const value = _.keys(valueDict).find((key) => {
    const [t, d, s] = valueDict[key];
    return t === type && d === direction;
  });

  const cssForCurrentActiveItem: React.CSSProperties = value && value !== 'other'
    ? { fontWeight: 700 }
    : { fontWeight: 400 };

  return (
    <div>
      <div className="wrap" style={{ width: '110px' }}>
        <Select.default
          value={value ? value : 'other'}
          options={options}
          clearable={false}
          optionRenderer={(optionValue: Option<string>) => (
            <div
              className="metric-option"
              style={{
                fontSize: '12px',
                ...(optionValue.value !== 'other' && optionValue.value === value
                  ? { fontWeight: 700 }
                  : { fontWeight: 400 }
                )
              }
              }
            >
              {optionValue.label}
            </div>
          )}
          style={{ fontSize: '12px', margin: 2, padding: 0, height: 20, ...cssForCurrentActiveItem }}
          onChange={(newValue: any) => {
            if (newValue !== null) {
              const [newType, newDirection, s] = valueDict[newValue];
              applySort({ type: newType, direction: newDirection } as Sorting);
            }
          }}
          simpleValue={true}
        />
      </div>
    </div>
  );
};
