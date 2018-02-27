import { compareString } from '../reducers/helpers';
import { Metric, PersonaMetric, Video, VideoMetric, MetricInfo } from '../types';
import * as _ from 'lodash';
import { VideoItemState } from './VideoList';
import { Total } from '../store/Chart';

export enum SortingDirection {
  Up,
  Down,
  Disabled
}

export enum SortingType {
  Title,
  Metric,
  MetaTag,
  Date,
  Playlist
}

export interface Sorting {
  direction: SortingDirection;
  type: SortingType;
  persona?: string;
  metric?: MetricInfo;
}

export interface VideoItemStateDictionary {
  [videoId: string]: VideoItemState;
}
export interface VideoDictionary {
  [videoId: string]: Video;
}

export function getMetricValue(
  video: VideoMetric | VideoItemState, persona: string, metric: MetricInfo): number {
  if (!persona || persona === Total) {
    return getMetricValueContent(video, metric);
  } else if (persona) {
    return getMetricValueMarketing(video, persona, metric);
  }
  return 0;
}

export function getMetricValueContent(video: VideoMetric | VideoItemState, metric: MetricInfo): number {
  var value = 0;
  let totals = video.totalMetrics;
  if (totals) {
    let totalMetric = totals.find(x => x.type === metric.type && x.controllerType === metric.pageType);
    if (totalMetric) {
      value = totalMetric.value;
    }
  }
  return value;
}

export function getMetricValueMarketing(
  video: VideoMetric | VideoItemState, persona: string, metric: MetricInfo): number {
  if (persona === Total) {
    return getMetricValueContent(video, metric);
  }
  if (video.metricsPerPersona) {
    const personaMetric = video.metricsPerPersona.find(
      personaMetric => personaMetric.persona === persona
    ) as PersonaMetric;
    if (personaMetric) {
      const metricValue = personaMetric.metrics.find(
        metricValue => metricValue.type === metric.type && metricValue.controllerType === metric.pageType
      ) as Metric;
      return metricValue && metricValue.value;
    }
  }
  return 0;
}

export function sortVideoIdsBy(
  sorting: Sorting,
  currentSortedVideoIds: string[],
  videoItemStates: VideoItemStateDictionary,
  editMode: boolean,
  activeMetaTag: string | undefined,
  force?: boolean
): string[] | undefined {

  const availableVideoIds = _.keys(videoItemStates);
  const inputVideosDidNotChange =
    _.intersection(currentSortedVideoIds, availableVideoIds).length === availableVideoIds.length &&
    availableVideoIds.length === currentSortedVideoIds.length;
  const lastSortedVideoIds = inputVideosDidNotChange ? currentSortedVideoIds : availableVideoIds;
  const { type, direction, persona, metric } = sorting;

  if (!editMode || force) {
    var sortedVideoIds = lastSortedVideoIds;
    if (direction !== SortingDirection.Disabled) {
      switch (type) {

        case SortingType.MetaTag:
          sortedVideoIds = lastSortedVideoIds.sort((videoId1: string, videoId2: string) => {
            let tag1 = videoItemStates[videoId1].tags.find(x => x.type.toLowerCase() === activeMetaTag);
            let tag2 = videoItemStates[videoId2].tags.find(x => x.type.toLowerCase() === activeMetaTag);
            let topic1: string = tag1 ? tag1.value : '';
            let topic2: string = tag2 ? tag2.value : '';
            var returnValue = compareString(topic1, topic2);
            if (returnValue === 0) {
              returnValue = compareString(videoId1, videoId2);
            }
            return returnValue * (direction === SortingDirection.Up ? 1 : -1);
          });
          break;

        case SortingType.Metric:
          let videoIdsToSort: string[] = lastSortedVideoIds;
          let notSortedIds: string[] = [];
          // In the case of sorting a metric of type Cost per... or Total Cost, we want to filter
          // videos with non-zero metric, showing them at the end
          if (metric.type.toLowerCase().includes('cost')) {
            [videoIdsToSort, notSortedIds] = partitionVideosWithoutMetrics(videoItemStates, persona, metric);
          }
          sortedVideoIds = videoIdsToSort.sort((videoId1, videoId2) => {
            const video1 = videoItemStates[videoId1];
            const video2 = videoItemStates[videoId2];
            const metric1 = getMetricValue(video1, persona, metric);
            const metric2 = getMetricValue(video2, persona, metric);
            let returnValue = 0;
            if (metric1 < metric2) { returnValue = -1; }
            if (metric1 > metric2) { returnValue = +1; }
            if (returnValue === 0) {
              returnValue = compareString(videoId1, videoId2);
            }
            return returnValue * (direction === SortingDirection.Up ? 1 : -1);
          });
          sortedVideoIds = sortedVideoIds.concat(notSortedIds);
          break;

        case SortingType.Title:
          sortedVideoIds = lastSortedVideoIds.sort((videoId1: string, videoId2: string) => {
            let title1 = videoItemStates[videoId1].title;
            let title2 = videoItemStates[videoId2].title;
            var returnValue = compareString(title1, title2);
            if (returnValue === 0) {
              returnValue = compareString(videoId1, videoId2);
            }
            return returnValue * (direction === SortingDirection.Up ? 1 : -1);
          });
          break;

        case SortingType.Date:
          sortedVideoIds = lastSortedVideoIds.sort((videoId1: string, videoId2: string) => {
            let date1 = videoItemStates[videoId1].publishedAt;
            let date2 = videoItemStates[videoId2].publishedAt;
            var returnValue = date1 < date2 ? -1 : (date1 > date2 ? +1 : 0);
            if (returnValue === 0) {
              returnValue = compareString(videoId1, videoId2);
            }
            return returnValue * (direction === SortingDirection.Up ? 1 : -1);
          });
          break;

        case SortingType.Playlist: {
          // As a video may have none, one or more playlists, this sorting method partitions between
          // the videos with playlists and no playlists and then joins the playlists on the videos that have
          // them and sorts accordingly
          [videoIdsToSort, notSortedIds] = partitionVideosWithoutPlaylists(videoItemStates);
          let videoToPlaylists: { [id: string]: string } =
            _.mapValues(
              videoItemStates,
              videoItemState => videoItemState.playlists.sort().join(',')
            );

          sortedVideoIds = videoIdsToSort.sort((videoId1: string, videoId2: string) => {
            let playlists1 = videoToPlaylists[videoId1];
            let playlists2 = videoToPlaylists[videoId2];
            var returnValue = compareString(playlists1, playlists2);
            if (returnValue === 0) {
              returnValue = compareString(videoId1, videoId2);
            }
            return returnValue * (direction === SortingDirection.Up ? 1 : -1);
          });
          sortedVideoIds = sortedVideoIds.concat(notSortedIds);
          break;
        }

        default:
          break;
      }
    }
    return sortedVideoIds;
  }
}

// Output: [ videosWithNonZeroMetric, videosWithZeroedMetric ]
function partitionVideosWithoutMetrics(
  videoItemStates: VideoItemStateDictionary,
  persona: string,
  metric: MetricInfo
): [string[], string[]] {
  let ids = _.keys(videoItemStates);
  return _.partition(
    ids,
    (id: string): boolean => getMetricValue(videoItemStates[id], persona, metric) !== 0
  );
}

// Output: [ videosWithNonZeroMetric, videosWithZeroedMetric ]
function partitionVideosWithoutPlaylists(
  videoItemStates: VideoItemStateDictionary
): [string[], string[]] {
  let ids = _.keys(videoItemStates);
  return _.partition(
    ids,
    (id: string): boolean => videoItemStates[id].playlists.length !== 0
  );
}
