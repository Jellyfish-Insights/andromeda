import {
  createStore,
  applyMiddleware,
  compose,
  combineReducers,
  GenericStoreEnhancer,
  Store,
  StoreEnhancerStoreCreator,
  ReducersMapObject,
  Reducer
} from 'redux';
import thunk from 'redux-thunk';
import { routerReducer, routerMiddleware } from 'react-router-redux';
import * as StoreModule from './store';
import { ApplicationState, reducers } from './store';
import { History } from 'history';
import { ChartDataItemKey } from './store/Chart';
import { MetricInfo, ArchiveMode } from './types';
import { requestDataFunc } from './store/Data';
import { stringfyFilters } from './reducers/helpers';
import { ComponentIds } from './routes';
import * as _ from 'lodash';

export default function configureStore(history: History, initialState?: ApplicationState) {
  // Build middleware. These are functions that can process the actions before they reach the store.
  // tslint:disable-next-line:no-any
  const windowIfDefined = typeof window === 'undefined' ? null : window as any;
  // If devTools is installed, connect to it
  const devToolsExtension =
    windowIfDefined
    && windowIfDefined.__REDUX_DEVTOOLS_EXTENSION__ as () => GenericStoreEnhancer;

  const createStoreWithMiddleware = compose<
    (r: Reducer<StoreModule.ApplicationState>, s?: ApplicationState) => Store<ApplicationState>
    >(
      applyMiddleware(thunk, routerMiddleware(history)),
      devToolsExtension ? devToolsExtension() : <S>(next: StoreEnhancerStoreCreator<S>) => next
    )(createStore);

  // Combine all reducers and instantiate the app-wide store instance
  const allReducers = buildRootReducer(reducers);
  const store = createStoreWithMiddleware(allReducers, initialState) as Store<ApplicationState>;

  // When the date range state is updated, update the Chart active key for marketing and content
  function handleIntervalChange() {
    var from: Date;
    var to: Date;
    const listener = () => {
      let previousFrom = from;
      let previousTo = to;
      from = store.getState().dateRange.from;
      to = store.getState().dateRange.to;
      if (from !== previousFrom || to !== previousTo) {
        let chart = store.getState().chart;
        let activeMetrics = [chart.activeMetrics.marketing, chart.activeMetrics.content];
        let newActiveMetrics = activeMetrics.map(x => x ? { ...x, startDate: from, endDate: to } : x);
        if (newActiveMetrics[0]) {
          store.dispatch({ type: 'SET_ACTIVE_METRIC', page: 'marketing', key: newActiveMetrics[0] });
        }
        if (newActiveMetrics[1]) {
          store.dispatch({ type: 'SET_ACTIVE_METRIC', page: 'content', key: newActiveMetrics[1] });
        }

        // It is not necessary to request metrics in this function, because there is a callback on the date range picker
        // That already gets the metrics updated
      }
    };
    return listener;
  }

  // To set the active marketing chart for the first time, need to first get the available metric types
  // from the server
  function handleMetricUpdate() {
    var currentMetrics: MetricInfo[] = [];
    const listener = () => {
      let previousMetrics = currentMetrics;
      currentMetrics = store.getState().filter.availableMetricsInfo;
      if (currentMetrics.length !== 0 && previousMetrics.length !== currentMetrics.length) {
        // let metric = currentMetrics[0];
        let contentMetric = currentMetrics.filter(x => x.pageType === 'content')[0];
        let marketingMetric = currentMetrics.filter(x => x.pageType === 'marketing')[0];
        let from = store.getState().dateRange.from;
        let to = store.getState().dateRange.to;
        let filters = stringfyFilters(store.getState().filter.filters);
        if (marketingMetric) {
          let marketingMetricKey: ChartDataItemKey = {
            metric: marketingMetric.type,
            type: 'persona',
            startDate: from,
            endDate: to,
            filters,
            pageType: 'marketing'
          };
          store.dispatch({ type: 'SET_ACTIVE_METRIC', page: 'marketing', key: marketingMetricKey });
        }
        if (contentMetric) {
          let contentMetricKey: ChartDataItemKey = {
            metric: contentMetric.type,
            type: 'length',
            startDate: from,
            endDate: to,
            filters,
            pageType: 'content'
          };
          store.dispatch({ type: 'SET_ACTIVE_METRIC', page: 'content', key: contentMetricKey });
        }
      }
    };

    return listener;
  }

  function handleFilterUpdate() {
    var filters: string;
    var archiveMode: ArchiveMode;
    const listener = () => {
      let previousFilter: string = filters;
      let previousArchiveMode: ArchiveMode = archiveMode;
      // Filters here have the structure {type, value}, without the `active`
      // because they are always sent as active to the controllers. The sorting
      // is just for us to keep the property of not downloading data that was
      // already downloaded. If the filters are [{type: 'a', value: 'b'}, {type:
      // 'b', value: 'a'}], it does not matter the order that they were added
      // when making a key based on them
      const activeMenuItem = store.getState().navMenu.activeMenuItem;
      if (activeMenuItem !== ComponentIds.Content
        && activeMenuItem !== ComponentIds.Marketing
        && activeMenuItem !== ComponentIds.VideoManagement) {
        // Metrics are only needed on content and marketing pages
        return;
      }

      filters = stringfyFilters(store.getState().filter.filters);

      if (previousFilter && filters !== previousFilter) {
        let oldMarketingMetric = store.getState().chart.activeMetrics.marketing;
        let oldContentMetric = store.getState().chart.activeMetrics.content;
        if (oldMarketingMetric) {
          let marketingMetricKey: ChartDataItemKey = { ...oldMarketingMetric, filters };
          store.dispatch({ type: 'SET_ACTIVE_METRIC', page: 'marketing', key: marketingMetricKey });
        }
        if (oldContentMetric) {
          let contentMetricKey: ChartDataItemKey = { ...oldContentMetric, filters };
          store.dispatch({ type: 'SET_ACTIVE_METRIC', page: 'content', key: contentMetricKey });
        }
        let { from, to } = store.getState().dateRange;

        let platformFilters = store.getState().filter.filters
          .filter(x => x.active === true && x.type === 'platform');
        let platformFilterStrings = platformFilters.map(x => x.value);

        let parsedPreviousFilter: { type: string, value: string }[] = JSON.parse(previousFilter);
        let previousPlatformFilters = parsedPreviousFilter.filter(x => x.type === 'platform').map(x => x.value);

        let shouldGetMetrics: boolean =
          _.difference(previousPlatformFilters, platformFilterStrings).length > 0
          || _.difference(platformFilterStrings, previousPlatformFilters).length > 0;

        if (shouldGetMetrics) {
          // Fetch new data only if platform filter is changed
          requestDataFunc('content', filters, from, to, false, false, store.dispatch, store.getState);
          requestDataFunc('marketing', filters, from, to, false, false, store.dispatch, store.getState);
        } else {
          // Unless a platform filter changed, this function is not getting metrics, just getting the updated VideoList
          // Because of that, there is no need to call data for both pages
          // Also, both the marketing and content controllers will return the same set of videos
          requestDataFunc('content', filters, from, to, true, false, store.dispatch, store.getState);
        }
      }

      archiveMode = store.getState().data.videoManagementArchiveMode;
      if (previousArchiveMode && previousArchiveMode !== archiveMode) {
        let { from, to } = store.getState().dateRange;
        requestDataFunc('content', filters, from, to, true, false, store.dispatch, store.getState);
      }
    };
    return listener;
  }

  store.subscribe(handleIntervalChange());
  store.subscribe(handleMetricUpdate());
  store.subscribe(handleFilterUpdate());

  // Enable Webpack hot module replacement for reducers
  if (module.hot) {
    module.hot.accept('./store', () => {
      const nextRootReducer = require<typeof StoreModule>('./store');
      store.replaceReducer(buildRootReducer(nextRootReducer.reducers));
    });
  }

  return store;
}

function buildRootReducer(allReducers: ReducersMapObject) {
  return combineReducers<ApplicationState>(Object.assign({}, allReducers, { routing: routerReducer }));
}
