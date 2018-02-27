import * as ChartStore from '../store/Chart';
import { MetricInfo, VideoMetric, Video, Metric, Tag } from '../types';
import { Sheet } from './ExportToExcelButton';
import * as _ from 'underscore';
import * as Moment from 'moment';
import { arrayToObject } from '../reducers/helpers';

type DemographicEntry = { [x: string]: string | number };
type GenderKey = 'F' | 'M' | 'U';
type ExcelCellValue = string | number | Date;
type ExcelRow = ExcelCellValue[];

function UnionSets<T>(arrayOfSets: Set<T>[]): Set<T> {
  return arrayOfSets.reduce((acc, cur) => new Set([...Array.from(acc), ...Array.from(cur)]), new Set());
}

function UnionArrayOfArrays<T>(arrayOfArrays: T[][]): T[] {
  return arrayOfArrays.reduce((acc, cur) => [...acc, ...cur], [] as T[]);
}

/**
 * Demographics data can been seen as a list of (gender, age group, video category, value).
 * This function returns the list of all values of video category.
 *
 * For example, if the video is being categorized by length, the array returned may be:
 * ['1min', '3min', '5min+']
 */
function GetVideoCategoriesOfDemographicsData(chartData: ChartStore.ChartDataItem): string[] {
  const getValueColumnNames = (row: DemographicEntry) => _.keys(row).filter(k => k !== 'ageGroup');
  const genders = _.keys(chartData.groupedData.data).filter(k => k !== 'Aggregated');
  const getValueColumnNamesForGender = (gender: string) => UnionSets(
    chartData.groupedData.data[gender].map(row => new Set(getValueColumnNames(row))));
  return Array.from(UnionSets(genders.map(getValueColumnNamesForGender)));
}

function ExcelFromGroupedChartData(chartData: ChartStore.ChartDataItem) {
  const videoCategories = GetVideoCategoriesOfDemographicsData(chartData);
  const genders = _.keys(chartData.groupedData.data).filter(k => k !== 'Aggregated') as GenderKey[];

  const getKeyColumns = (gender: GenderKey, row: DemographicEntry) => [gender, row.ageGroup];
  const getValueColumns = (row: DemographicEntry) => videoCategories.map(k => row[k] || 0);

  const getRowFromDemographicEntry = (gender: GenderKey, row: DemographicEntry) => [
    ...getKeyColumns(gender, row),
    ...getValueColumns(row)];

  const getRowsForGender = (gender: GenderKey) => chartData.groupedData.data[gender].map(
    row => getRowFromDemographicEntry(gender, row));

  return UnionArrayOfArrays(genders.map(getRowsForGender));
}

function ExcelFromUngroupedChartData(chartData: ChartStore.ChartDataItem) {
  const getKeyColumn = (row: ChartStore.ChartObject) => [
    Moment(row.date, 'YYYYMMDD').toDate() as ExcelCellValue];

  const valueColumnHeaders = _.keys(chartData.total);
  const getValueColumns = (row: ChartStore.ChartObject) => valueColumnHeaders.map(k => row.values[k] as ExcelCellValue);
  const getRow = (row: ChartStore.ChartObject) => [...getKeyColumn(row), ...getValueColumns(row)];

  const getTotalRow = (v: ChartStore.ChartDataItem) => [[
    ('Total' as ExcelCellValue),
    ...valueColumnHeaders.map(k => v.total[k])]];

  return [...chartData.data.map(getRow), ...getTotalRow(chartData)];
}

function GetExcelSummaryForChart(chartKey: ChartStore.ChartDataItemKey): ExcelRow[] {
  return [
    ['page', chartKey.pageType],
    ['generated at', new Date()],
    ['metric', chartKey.metric],
    ['start date', chartKey.startDate],
    ['end date', chartKey.endDate],
    ['filters', chartKey.filters],
    ['aggregation by', chartKey.type],
  ];
}

function GetExcelSummaryForVideoList(pageName: string, from: Date, to: Date, filter: string) {
  return [
    ['page', pageName],
    ['generated at', new Date()],
    ['start date', from],
    ['end date', to],
    ['filters', filter],
  ];
}

function ExcelFromChartItem(chartData: ChartStore.ChartDataItem): Sheet {
  const data = (chartData.dataByGroup) ? ExcelFromGroupedChartData(chartData) : ExcelFromUngroupedChartData(chartData);
  const header = (chartData.dataByGroup) ? (
    ['gender',
      'age group',
      ...GetVideoCategoriesOfDemographicsData(chartData)]
  ) : (
      ['date', ..._.keys(chartData.total)]);
  return {
    SheetName: chartData.key.metric,
    Summary: GetExcelSummaryForChart(chartData.key),
    Header: header,
    Data: data
  };
}

export function ConvertChartDataToExcel(downloadedChartData: ChartStore.ChartDataItem[]): Sheet[] {
  return downloadedChartData.map(ExcelFromChartItem);
}

function GetMetaTagGenerator(
  videos: { [id: string]: Video },
  metaTags: { [type: string]: Tag[] }) {
  const metaTagTypes = _.keys(metaTags).sort();
  const getVideoTags = (videoMetrics: VideoMetric): string[] => {
    return metaTagTypes.map(
      metaTagType => videos[videoMetrics.id].tags
        .filter(x => x.type === metaTagType)
        .map(x => x.value)
        .join(','));
  };

  return { metaTagTypes, getVideoTags };
}

function GetPlaylistsGenerator(videos: { [id: string]: Video }) {
  const numberOfPlaylists: number = _.max(_.mapObject(videos, v => v.playlists.length));

  const playlists =
    numberOfPlaylists >= 2
      ? Array.from({ length: numberOfPlaylists }, (v, i) => i + 1).map(x => 'Playlist ' + x)
      : ['Playlist'];

  const getVideoPlaylists = (video: Video): string[] => {
    return playlists.map((playlist, i) => video.playlists[i] || '');
  };

  return { playlists, getVideoPlaylists };
}

export function TrimMetrics(videos: { [id: string]: Video }, videoMetrics: VideoMetric[]): VideoMetric[] {
  let ids = new Set(_.keys(videos));
  return videoMetrics.filter(v => ids.has(v.id));
}

export function ConvertContentVideoListDataToExcel(
  pageName: string,
  videoMetrics: VideoMetric[],
  videos: { [id: string]: Video },
  availableMetrics: MetricInfo[],
  from: Date,
  to: Date,
  filters: string,
  metaTags: { [type: string]: Tag[] }): Sheet {

  if (videoMetrics.length === 0) {
    alert('Metrics were not fully downloaded yet.');
    return;
  }
  const getVideoMetric = (videoMetrics: VideoMetric, metric: MetricInfo): number => {
    if (!videoMetrics || !videoMetrics.totalMetrics) {
      return 0;
    }
    const foundMetric = videoMetrics.totalMetrics.find(videoMetric => videoMetric.type === metric.type);
    return (foundMetric && foundMetric.value) || 0;
  };

  const getVideoValues = (videoMetrics: VideoMetric) => {
    return availableMetrics.map(metric => getVideoMetric(videoMetrics, metric));
  };

  const { metaTagTypes, getVideoTags } = GetMetaTagGenerator(videos, metaTags);
  const { playlists, getVideoPlaylists } = GetPlaylistsGenerator(videos);

  return {
    SheetName: 'Video List',
    Summary: GetExcelSummaryForVideoList(pageName, from, to, filters),
    Header: [
      'Title',
      ...metaTagTypes,
      ...playlists,
      ...availableMetrics.map(x => x.type)
    ],
    Data: videoMetrics.map(metric => [
      getVideoKey(videos, metric),
      ...getVideoTags(metric),
      ...getVideoPlaylists(videos[metric.id]),
      ...(getVideoValues(metric))
    ])
  };
}

function getVideoKey(videos: { [id: string]: Video }, videoMetric: VideoMetric): string {
  return videos[videoMetric.id].title;
}

export function ConvertMarketingVideoListDataToExcel(
  pageName: string,
  videoMetrics: VideoMetric[],
  videos: { [id: string]: Video },
  availableMetrics: MetricInfo[],
  from: Date,
  to: Date,
  filters: string,
  metaTags: { [type: string]: Tag[] }): Sheet {

  if (videoMetrics.length === 0) {
    alert('Metrics were not fully downloaded yet.');
    return;
  }

  const personasWithMetrics = Array.from(
    UnionSets(videoMetrics.map(v => new Set(v.metricsPerPersona.map(p => p.persona))))
  ).sort();

  const getVideoMetric = (metric: MetricInfo, videoMetrics?: Metric[]) => {
    const entry = videoMetrics.find(videoMetric => videoMetric.type === metric.type);
    if (!videoMetrics || !entry) {
      return 0;
    }
    return entry.value;
  };

  const getMetricOfPersona = (videoMetrics: VideoMetric, personaName: string) => {
    return videoMetrics.metricsPerPersona.find(x => x.persona === personaName);
  };

  const getVideoValues = (videoMetrics: VideoMetric) => {
    return _.chain(
      personasWithMetrics
    ).map(
      personaName => getMetricOfPersona(videoMetrics, personaName)
    ).map(
      p => availableMetrics.map(
        metric => p ? getVideoMetric(metric, p.metrics) : 0)
    ).flatten().value();
  };

  const { metaTagTypes, getVideoTags } = GetMetaTagGenerator(videos, metaTags);
  const { playlists, getVideoPlaylists } = GetPlaylistsGenerator(videos);

  return {
    SheetName: 'Video List',
    Summary: GetExcelSummaryForVideoList(pageName, from, to, filters),
    Header: [
      'Title',
      ...metaTagTypes,
      ...playlists,
      ...(_.chain(
        personasWithMetrics
      ).map(
        personaName => availableMetrics.map(
          metric => `${personaName}-${metric.type}`
        )).flatten().value())
    ],
    Data: videoMetrics.map(
      metric => [
        getVideoKey(videos, metric),
        ...getVideoTags(metric),
        ...getVideoPlaylists(videos[metric.id]),
        ...(getVideoValues(metric))]
    )
  };
}

export function GetMetaTagSheet(
  tagCollection: { [type: string]: Tag[] }) {
  const tagTypeCollection = _.keys(tagCollection).sort();

  return {
    SheetName: 'Meta Tags',
    Summary: [['generated at', new Date()]],
    Header: [
      'Tag Type',
      'Tag Values',
    ],
    Data: tagTypeCollection.map(tagType => [tagType, ...tagCollection[tagType].map(tag => tag.value)])
  };
}
