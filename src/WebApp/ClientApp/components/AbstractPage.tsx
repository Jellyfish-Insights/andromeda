import { RouteComponentProps } from 'react-router-dom';
import * as ChartStore from '../store/Chart';
import * as MarketingDataStore from '../store/Data';
import * as DateRangeStore from '../store/DateRange';
import * as FilterStore from '../store/Filter';
import * as LoginDataStore from '../store/LoginData';
import * as NavMenuStore from '../store/NavMenu';
import { stringfyFilters } from '../reducers/helpers';
import { Page } from '../store/Chart';

export type YearPageProps =
  MarketingDataStore.DataState &
  typeof MarketingDataStore.actionCreators &
  NavMenuStore.NavMenuState &
  typeof NavMenuStore.actionCreators &
  FilterStore.FilterState &
  typeof FilterStore.actionCreators &
  DateRangeStore.DateRangeState &
  typeof DateRangeStore.actionCreators &
  ChartStore.ChartDataState &
  typeof ChartStore.actionCreators &
  LoginDataStore.LoginDataState &
  typeof LoginDataStore.actionCreators &
  RouteComponentProps<{}>;

/**
 * This function is used to select only the data currently being shown to the
 * user out of the full cached data.
 */
export function GetActiveChartItems(
  downloadedChartData: ChartStore.ChartDataItem[],
  pageProps: YearPageProps) {
  return downloadedChartData.filter(
    chart => chart.key.pageType === pageProps.activePage
      && chart.key.startDate.getTime() === pageProps.from.getTime()
      && chart.key.endDate.getTime() === pageProps.to.getTime()
      && chart.key.filters === stringfyFilters(pageProps.filters)
      && chart.key.type === pageProps.activeMetrics[pageProps.activePage].type);
}

export function GetExcelFileName(pageType: Page) {
  return `year-${pageType}-${(new Date()).toISOString().split('.')[0].replace(/:/g, '-')}.xlsx`;
}

export function updateFailure() {
  alert(
    'There was a failure while saving your data. ' +
    'The data may have been updated by another user, ' +
    'or maybe you do not have permission to edit it'
  );
}
