import { reducer, ChartDataState, ChartDataItemKey, sameKey } from '../Chart';
import * as Moment from 'moment';

let unloadedState: ChartDataState = {
  downloadedChartData: [],
  summaryValues: {},
  hideSummary: false,
  firstSmallChart: { content: 0, marketing: 0, dashboard: 0 },
  colorStacks: {},
  nextElement: 0,
  activeMetrics: {}
};
let viewsKey: ChartDataItemKey = {
  metric: 'views',
  type: 'persona',
  startDate: Moment('20180401', 'YYYYMMDD').toDate(),
  endDate: Moment('20180411', 'YYYYMMDD').toDate(),
  filters: '',
  pageType: 'marketing'
};
let topicKey: ChartDataItemKey = {
  metric: 'Topic 01',
  type: 'topic',
  startDate: Moment('20180401', 'YYYYMMDD').toDate(),
  endDate: Moment('20180411', 'YYYYMMDD').toDate(),
  filters: '',
  pageType: 'content'
};

it('State does not change in case of failure', () => {
  let newState = reducer(unloadedState, {
    type: 'RECEIVE_CHART_DATA_FAILURE',
    key: viewsKey,
    data: [],
    filters: '',
    metrics: [viewsKey.metric]
  });
  expect(newState).toEqual(unloadedState);
});

it('Set active metric for marketing page', () => {
  let nextState: ChartDataState = {
    ...unloadedState,
    activeMetrics: {
      marketing: viewsKey
    }
  };
  let newState = reducer(unloadedState, {
    type: 'SET_ACTIVE_METRIC',
    key: viewsKey,
    page: 'marketing',
    data: []
  });
  expect(newState).toEqual(nextState);
});

it('Set active metric for content page', () => {
  let initialState: ChartDataState = {
    ...unloadedState,
    activeMetrics: {
      marketing: viewsKey
    }
  };
  let nextState: ChartDataState = {
    ...initialState,
    activeMetrics: {
      marketing: viewsKey,
      content: topicKey
    }
  };
  let newState = reducer(initialState, {
    type: 'SET_ACTIVE_METRIC',
    key: topicKey,
    page: 'content',
    data: []
  });
  expect(newState).toEqual(nextState);
});
it('Key comparison with same dates', () => {
  let comparisonKey = {
    ...viewsKey,
    startDate: Moment('20180401', 'YYYYMMDD').toDate(),
    endDate: Moment('20180411', 'YYYYMMDD').toDate()
  };
  expect(sameKey(comparisonKey, viewsKey)).toBeTruthy();
});

it('Key comparison with different dates', () => {
  let comparisonKey = {
    ...viewsKey,
    startDate: Moment('20180311', 'YYYYMMDD').toDate(),
    endDate: Moment('20180411', 'YYYYMMDD').toDate()
  };
  expect(sameKey(comparisonKey, viewsKey)).toBeFalsy();
});

it('Toggle Summary', () => {
  let newState = reducer(unloadedState, { type: 'TOGGLE_SUMMARY' });
  expect(newState.hideSummary).toBeTruthy();
  let newestState = reducer(newState, { type: 'TOGGLE_SUMMARY' });
  expect(newestState.hideSummary).toBeFalsy();
});
