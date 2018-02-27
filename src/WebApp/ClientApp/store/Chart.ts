import { Action, Reducer } from 'redux';
import { AppThunkAction } from '.';
import { DemographicData, DemographicDataItem, Tag } from '../types';
import * as _ from 'underscore';
import { FilterItem } from './Filter';
import { capitalizeFirst, formatYMD } from '../reducers/helpers';
import update from 'immutability-helper';
import { authenticatedFetch } from '../ConnectionUtil';

export type ControllerType = 'content' | 'marketing';
export type Page = ControllerType | 'dashboard';
export const Aggregated = 'Aggregated';
export const GenderAggregated = 'GenderAggregated';

export const Total = 'Grand Total';
interface Color {
  color?: string;
  order?: number;
}

export type FilterItemWithColor = FilterItem & Color;
export type ValueToFilterItem = { [value: string]: FilterItemWithColor };
export type TypeToValueToFilterItem = { [type: string]: ValueToFilterItem };

export interface TimeSeriesChartData {
  startDate: string;
  endDate: string;
  metric: string;
  chartObjectArray: Array<ChartObject>;
  totalPerGroup: ChartObjectValues;
}

export type SelectorType = 'persona' | 'topic' | 'medium' | 'tone' | 'length';
export interface ChartDataItemKey {
  metric: string; // "Views", "Likes",...
  type: SelectorType; // "persona", "topic",...
  startDate: Date;
  endDate: Date;
  filters: string; // JSON.stringfy(FilterItem[])
  pageType: ControllerType;
}

export interface ChartDataItem {
  key: ChartDataItemKey;
  dataByGroup: boolean; // if true, dataByGroup should be used and multiple charts are drawn
  data: Array<ChartObject>;
  groupedData: DemographicsChartData;
  total: { [x: string]: number };
  loading: boolean;
  failed: boolean;
}

export interface SmallChartState {
  content: number;
  marketing: number;
  dashboard: number;
}

export interface DemographicsChartData {
  data: { [x: string]: Array<{ [x: string]: number | string }> };
  dataByGender: Array<{ [x: string]: number | string }>;
}

// Each time a chart needs to be generated, it will look in the cache with
// the given key; if not found, then an action 'RequestChartDataAction' should be submitted to the server
export interface ChartDataState {
  downloadedChartData: ChartDataItem[];
  activeMetrics: { [page: string]: ChartDataItemKey };
  summaryValues: TypeToValueToFilterItem;
  hideSummary: boolean;
  // Small charts are stored in an array;
  // this number indicates which one will be presented on the left of the caroussel-like component
  firstSmallChart: SmallChartState;
  activePage?: Page;
  colorStacks: { [valueType: string]: string[] };
  nextElement: number;
}

export const Palette: Array<string> = [
  '#561F37',
  '#E8AB23',
  '#04994A',
  '#086FBC',
  '#A60DCC',

  '#841C26',
  '#CEAF19',
  '#08A569',
  '#0446AF',
  '#C10CB9',

  '#D11111',
  '#ABAF0E',
  '#05AA8B',
  '#0E337C',
  '#D10086',

  '#EF6637',
  '#7D9B10',
  '#14B5C1',
  '#4A00A0',
  '#C1084F',

  '#F28E3d',
  '#4C8E0C',
  '#28ABE2',
  '#7304BA',
  '#AD1F2D'
];

interface RequestChartDataAction {
  type: 'REQUEST_CHART_DATA';
  key: ChartDataItemKey;
  dataByGroup: boolean;
  metrics: string[];
}

interface ReceiveChartDataSuccessAction {
  type: 'RECEIVE_CHART_DATA_SUCCESS';
  key: ChartDataItemKey;
  data?: TimeSeriesChartData[];
  groupedData?: DemographicData;
}

interface ReceiveChartDataFailureAction {
  type: 'RECEIVE_CHART_DATA_FAILURE';
  key: ChartDataItemKey;
  metrics: string[];
}

interface SetActiveMetricAction {
  type: 'SET_ACTIVE_METRIC';
  page: string;
  key: ChartDataItemKey;
}

interface SetSummaryValueAction {
  type: 'SET_SUMMARY_VALUE';
  valueType: string; // persona, topic, length, ...
  value: string; // persona A, topic 1, ...
  active: boolean;
  ignoreIfExists?: boolean; // default is false
  metaTags: { [type: string]: Tag[] };
}

interface ToggleSummaryAction {
  type: 'TOGGLE_SUMMARY';
}

export interface SetFirstSmallChartAction {
  type: 'SET_FIRST_SMALL_CHART';
  newValue: SmallChartState;
}

interface SetActivePageAction {
  type: 'SET_ACTIVE_PAGE';
  newValue: ControllerType;
}
type KnownAction = RequestChartDataAction
  | ReceiveChartDataSuccessAction
  | ReceiveChartDataFailureAction
  | SetActiveMetricAction
  | SetSummaryValueAction
  | ToggleSummaryAction
  | SetFirstSmallChartAction
  | SetActivePageAction;

export const sameKey = (key1: ChartDataItemKey, key2: ChartDataItemKey): boolean => {
  return (
    (key1.type === key2.type)
    && (key1.startDate.getTime() === key2.startDate.getTime())
    && (key1.endDate.getTime() === key2.endDate.getTime())
    && (key1.metric === key2.metric)
    && (key1.filters === key2.filters)
    && (key1.pageType === key2.pageType)
  );
};

export type ChartObjectValues = { [value: string]: number };

export interface ChartObject {
  date: string;
  values: ChartObjectValues;
}

function createChartObjectDemographic(demographicData: DemographicData): DemographicsChartData {
  let output: DemographicsChartData = { data: {}, dataByGender: [] };
  let genders = demographicData.groups;
  var tags: string[] = [];
  demographicData.values.forEach((demographicDataItem: DemographicDataItem) => {
    tags.push(demographicDataItem.groupName);
  });
  var ageGroups: string[] = [];
  demographicData.values.forEach((demographicDataItem: DemographicDataItem) => {
    ageGroups = _.uniq(ageGroups.concat(_.keys(demographicDataItem.values)));
  });

  genders.forEach(gender => {
    output.data[gender] = [];
    ageGroups.forEach(ageGroup => {
      var obj: { [x: string]: number | string } = { ageGroup };
      tags.forEach(tag => {
        let group = demographicData.values.find(x => x.groupName === tag);
        obj[tag] = group ? group.values[ageGroup][gender] : 0.0;
      });
      output.data[gender].push(obj);
    });
    // This is to present data to show by gender only, stripping away age groups
    var currentObj: { [x: string]: number | string } = { ageGroup: gender };
    output.data[gender].forEach(obj => {
      _.keys(obj).filter(x => x !== 'ageGroup').map((group: string) => {
        currentObj[group] = currentObj[group]
          ? (currentObj[group] as number) + (obj[group] as number)
          : (obj[group] as number);
      });
    });
    output.dataByGender.push(currentObj);
  });

  // Remove age groups if there is no data for them
  output.dataByGender.forEach(obj => {
    if (obj[Total] === 0) {
      genders = genders.filter(x => x !== obj.ageGroup);
      delete output.data[obj.ageGroup];
    }
  });
  output.dataByGender = output.dataByGender.filter(x => genders.includes(x.ageGroup as string));

  // Super Total
  output.data[Aggregated] = [];
  ageGroups.forEach(ageGroup => {
    var obj: { [x: string]: number | string } = { ageGroup };
    tags.forEach(tag => {
      let group = demographicData.values.find(x => x.groupName === tag);
      if (group) {
        obj[tag] = genders
          .map(gender => (group as DemographicDataItem).values[ageGroup][gender])
          .reduce((acc, x) => acc + x, 0);
      } else {
        obj[tag] = 0.0;
      }
    });
    output.data[Aggregated].push(obj);
  });
  return output;
}
export const actionCreators = {
  requestChartData: (pageType: ControllerType, key: ChartDataItemKey, metrics: string[]): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      // In case data was already downloaded, do not download it again
      let cache: ChartDataState = getState().chart;
      let cachedItem = cache.downloadedChartData.find(x => sameKey(x.key, key));
      if (cachedItem && !cachedItem.loading) {
        return;
      }

      let controller = capitalizeFirst(pageType);

      // Make request
      const from: string = formatYMD(key.startDate);
      const to: string = formatYMD(key.endDate);
      const lowerCaseDemographicTypes: string[] = ['demographics view count', 'demographics view time'];
      const encodedFilters = encodeURIComponent(key.filters);
      const encodedMetrics = encodeURIComponent(JSON.stringify(metrics));
      const args = `${encodedMetrics}/${key.type}/${from}/${to}/${encodedFilters}`;
      if (lowerCaseDemographicTypes.findIndex(x => x === key.metric.toLowerCase()) < 0) {
        authenticatedFetch(`api/${controller}Data/GetChartData/${args}`, {
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          }
        })
          .then(
            response => response.json() as Promise<TimeSeriesChartData[]>
          ).then(result => {
            dispatch({ type: 'RECEIVE_CHART_DATA_SUCCESS', key, data: result });
          }).catch(ex => {
            dispatch({ type: 'RECEIVE_CHART_DATA_FAILURE', key, metrics });
          });

        dispatch({ type: 'REQUEST_CHART_DATA', key, dataByGroup: false, metrics });
      } else {
        authenticatedFetch(`api/${controller}Data/GetDemographicsChartData/${args}`, {
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          }
        })
          .then(response => response.json() as Promise<{
            metric: string,
            startDate: string,
            endDate: string,
            data: DemographicData,
            filters: { type: string, value: string }
          }[]>
          ).then(result => {
            dispatch({ type: 'RECEIVE_CHART_DATA_SUCCESS', key, groupedData: result[0].data });
          }).catch(ex => {
            console.log(ex);
            dispatch({ type: 'RECEIVE_CHART_DATA_FAILURE', key, metrics: [key.metric] });
          });

        dispatch({ type: 'REQUEST_CHART_DATA', key, dataByGroup: true, metrics: [key.metric] });
      }
    },

  setActiveMetric:
    (page: string, key: ChartDataItemKey) =>
      <SetActiveMetricAction> { type: 'SET_ACTIVE_METRIC', page, key },

  setSummaryValue: (
    valueType: string,
    value: string,
    active: boolean,
    metaTags: { [index: string]: Tag[] },
    ignoreIfExists?: boolean
  ) =>
    <SetSummaryValueAction> { type: 'SET_SUMMARY_VALUE', valueType, value, active, metaTags, ignoreIfExists },

  toggleSummary: () => <ToggleSummaryAction> { type: 'TOGGLE_SUMMARY' },

  setFirstSmallChart:
    (newValue: SmallChartState) =>
      <SetFirstSmallChartAction> { type: 'SET_FIRST_SMALL_CHART', newValue },

  setActivePage:
    (newValue: Page) =>
      <SetActivePageAction> { type: 'SET_ACTIVE_PAGE', newValue },
};

let unloadedState: ChartDataState = {
  downloadedChartData: [],
  summaryValues: {},
  hideSummary: false,
  firstSmallChart: { content: 0, marketing: 0, dashboard: 0 },
  colorStacks: {},
  nextElement: 0,
  activeMetrics: {},
};

export const reducer: Reducer<ChartDataState> = (state: ChartDataState, incomingAction: Action): ChartDataState => {
  const action = incomingAction as KnownAction;
  switch (action.type) {
    case 'REQUEST_CHART_DATA':
      return {
        ...state,
        downloadedChartData: state.downloadedChartData.concat(
          action.metrics.map((metric: string): ChartDataItem => {
            return {
              key: { ...action.key, metric },
              dataByGroup: action.dataByGroup,
              data: [],
              groupedData: { data: {}, dataByGender: [] },
              total: {},
              loading: true,
              failed: false
            };
          })
        )
      };
    case 'RECEIVE_CHART_DATA_SUCCESS':
      if (action.data) {
        let newDownloadedDataSuccess: ChartDataItem[] = [...state.downloadedChartData];
        action.data.forEach(timeSeriesData => {
          const chartData: Array<ChartObject> = timeSeriesData.chartObjectArray;
          const totalPerGroup: ChartObjectValues = timeSeriesData.totalPerGroup;
          let emptyGroupedData: DemographicsChartData = { data: {}, dataByGender: [] };
          const metric = timeSeriesData.metric;
          const key = { ...action.key, metric };
          newDownloadedDataSuccess = newDownloadedDataSuccess.map(
            x =>
              sameKey(x.key, key)
                ? {
                  key,
                  dataByGroup: false,
                  data: chartData,
                  groupedData: emptyGroupedData,
                  total: totalPerGroup,
                  loading: false,
                  failed: false
                }
                : x
          );
        });
        return { ...state, downloadedChartData: newDownloadedDataSuccess };
      }
      if (action.groupedData) {
        let data: DemographicData = action.groupedData;
        let chartData: DemographicsChartData = createChartObjectDemographic(data);
        let total: { [x: string]: number } = {};
        data.values.forEach(x => total[x.groupName] = x.total);
        let newDownloadedDataSuccess: ChartDataItem[] = state.downloadedChartData.map(
          x =>
            sameKey(x.key, action.key)
              ? {
                key: action.key,
                dataByGroup: true,
                data: [],
                groupedData: chartData,
                total,
                loading: false,
                failed: false
              }
              : x
        );
        return { ...state, downloadedChartData: newDownloadedDataSuccess };
      }
      return { ...state };
    case 'RECEIVE_CHART_DATA_FAILURE':
      // Remove item with key that was loading in case of failure
      let newDownloadedData: ChartDataItem[] = [...state.downloadedChartData];
      action.metrics.forEach(metric => {
        newDownloadedData = newDownloadedData.filter(x => sameKey(x.key, { ...action.key, metric }));
      });
      return { ...state, downloadedChartData: newDownloadedData };
    case 'SET_ACTIVE_METRIC':
      return update(state, {
        activeMetrics: { $merge: { [action.page]: action.key } }
      });
    case 'SET_SUMMARY_VALUE': {
      let summaryValues = { ...state.summaryValues };
      let colorStacks = { ...state.colorStacks };
      let nextElement = state.nextElement;
      const { valueType, value, active, ignoreIfExists, metaTags } = action;
      let serverColor: string;
      if (metaTags[capitalizeFirst(valueType)]) {
        let thisTag = metaTags[capitalizeFirst(valueType)].find(x => x.value === value);
        serverColor = thisTag && thisTag.color;
      }
      if (ignoreIfExists && summaryValues[valueType] && summaryValues[valueType][value]) { return { ...state }; }
      if (!summaryValues[valueType] && !serverColor) {
        summaryValues[valueType] = {};
        // Each valueType, which can be Persona, Topic, or another meta tag, has
        // a color stack
        colorStacks[valueType] = [...Palette].reverse();
      }
      let color = serverColor;
      if (!color) {
        if (active) {
          color = colorStacks[valueType].pop();
          nextElement++;
        } else if (summaryValues[valueType][value]) {
          let oldColor = summaryValues[valueType][value].color;
          if (oldColor) { colorStacks[valueType].push(oldColor); }
        }
      }
      summaryValues[valueType][value] = {
        type: valueType,
        value: value,
        active,
        color,
        order: nextElement
      };
      // Make sure that we have at least one active item
      if (howManyValuesAreActive(summaryValues[valueType]) === 0) {
        summaryValues[valueType][Total] = {
          type: valueType,
          value: Total,
          active: true,
          color: serverColor || colorStacks[valueType].pop(),
          order: nextElement,
        };
      }
      return { ...state, summaryValues, colorStacks, nextElement };
    }
    case 'TOGGLE_SUMMARY':
      return { ...state, hideSummary: !state.hideSummary };
    case 'SET_FIRST_SMALL_CHART':
      return { ...state, firstSmallChart: action.newValue };
    case 'SET_ACTIVE_PAGE':
      return { ...state, activePage: action.newValue };
    default:
      const exhaustiveCheck: never = action;
  }

  return state || unloadedState;
};

function howManyValuesAreActive(valueToFilterItem: ValueToFilterItem): number {
  const keys = _.keys(valueToFilterItem);
  return keys
    .map(key => valueToFilterItem[key])
    .filter(filterItem => filterItem.active)
    .length;
}
