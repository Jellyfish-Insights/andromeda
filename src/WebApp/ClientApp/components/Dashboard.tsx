import * as React from 'react';
import { RouteComponentProps } from 'react-router-dom';
import { connect } from 'react-redux';
import { Redirect } from 'react-router';
import * as NavMenuStore from '../store/NavMenu';
import { ApplicationState } from '../store';
import { ComponentIds } from '../routes';
import * as LoginDataStore from '../store/LoginData';
import {
  arrayToObject,
  stringfyFilters,
  addControllerTypeToMetrics,
  computeSmallCharts
} from '../reducers/helpers';
import * as MarketingDataStore from '../store/Data';
import * as DateRangeStore from '../store/DateRange';
import * as FilterStore from '../store/Filter';
import * as ChartStore from '../store/Chart';
import { ChartDataItemKey, ControllerType, sameKey } from '../store/Chart';
import { Chart } from './Chart';
import { VideoList } from './VideoList';
import { VideoMetric, MetricInfo } from '../types';
import * as Moment from 'moment';
import {
  SortingDirection,
  SortingType,
  Sorting,
} from './VideoSort';
import * as _ from 'underscore';

type DashboardProps =
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

interface DashboardState {
  startDate: Date;
  endDate: Date;
  initialMetricKey?: ChartDataItemKey;
  availableMetricsInfo: MetricInfo[];
  width: number;
  height: number;
}

function mergeVideoMetrics(content: VideoMetric[], marketing: VideoMetric[]): VideoMetric[] {
  const ids = _.uniq(content.map(m => m.id).concat(marketing.map(m => m.id)));
  const contentObject = arrayToObject(
    addControllerTypeToMetrics(content, 'content'),
    (v: VideoMetric) => v.id
  );
  const marketingObject = arrayToObject(
    addControllerTypeToMetrics(marketing, 'marketing'),
    (v: VideoMetric) => v.id
  );
  const mergedVideoMetrics: VideoMetric[] = ids.map(id => {
    return {
      id,
      totalMetrics: [
        ...((contentObject[id] && contentObject[id].totalMetrics) || []),
        ...((marketingObject[id] && marketingObject[id].totalMetrics) || [])
      ]
    };
  });
  return mergedVideoMetrics;
}

function createKey(info: MetricInfo, from: Date, to: Date): ChartDataItemKey {
  return {
    metric: info.type,
    type: info.pageType === 'marketing' ? 'persona' : 'length',
    startDate: from,
    endDate: to,
    filters: stringfyFilters([]),
    pageType: info.pageType as ControllerType
  };
}

class DashboardInternal extends React.Component<DashboardProps, DashboardState> {

  constructor(props: DashboardProps) {
    super(props);
    this.props.select(ComponentIds.Dashboard);
    this.state = {
      startDate: Moment().subtract(60, 'd').toDate(),
      endDate: Moment().subtract(1, 'd').toDate(),
      availableMetricsInfo: [],
      width: 0,
      height: 0,
    };
    this.updateWindowDimensions = this.updateWindowDimensions.bind(this);
  }

  updateWindowDimensions() {
    this.setState({ width: window.innerWidth, height: window.innerHeight });
  }

  componentDidMount() {
    const { startDate, endDate } = this.state;
    this.props.setActivePage('dashboard');
    this.props.requestData('content', stringfyFilters([]), startDate, endDate, false, true);
    this.props.requestData('marketing', stringfyFilters([]), startDate, endDate, false, true);
    this.props.requestFilterData('dashboard', false);
    this.props.select(ComponentIds.Dashboard);
    this.updateWindowDimensions();
    window.addEventListener('resize', this.updateWindowDimensions);
  }

  componentWillUnmount() {
    window.removeEventListener('resize', this.updateWindowDimensions);
  }

  componentDidUpdate() {
    const { availableMetricsInfo, startDate, endDate } = this.state;
    let { availableDashboardMetricsInfo } = this.props;
    if (
      availableMetricsInfo.length === 0
      && availableDashboardMetricsInfo
      && availableDashboardMetricsInfo.length !== 0
    ) {

      availableDashboardMetricsInfo.sort((x, y) => x.typeId - y.typeId);

      let initialMetric = createKey(
        availableDashboardMetricsInfo.find(x => x.type === 'Views') || availableDashboardMetricsInfo[0],
        startDate,
        endDate
      );
      this.props.setActiveMetric('dashboard', initialMetric);
      this.setState({
        availableMetricsInfo: availableDashboardMetricsInfo
      });
    }
  }

  public render() {
    if (!this.props.logged) {
      return <Redirect to="/account/login/dashboard" />;
    }
    const {
      videos,
      requestData,
      metaTags,
      activeMetrics,
      content,
      marketing,
    } = this.props;
    const {
      startDate,
      endDate,
      availableMetricsInfo,
      width
    } = this.state;

    if (availableMetricsInfo.length === 0) {
      return <div>Loading Metric Information...</div>;
    }
    if (videos.length === 0) {
      return <div>Loading Video List...</div>;
    }

    // In principle, removing the comments below have no impact
    // They were added because the staging database is not beign updated
    // and this makes the whole page display nothing
    // TODO Remove or uncomment the lines below
    /* if (content.length === 0 || marketing.length === 0) {
     *   return <div>Loading Metrics...</div>;
     * } */

    const stringfiedFilters = stringfyFilters([]);

    const activeMetric: ChartDataItemKey = {
      ...activeMetrics.dashboard,
      startDate,
      endDate,
      filters: stringfiedFilters
    };

    const videoMetrics: VideoMetric[] = mergeVideoMetrics(content, marketing);
    const activeMetricInfo = availableMetricsInfo.find(
      x => x.type === activeMetric.metric && x.pageType === activeMetric.pageType
    );

    const sorting: Sorting = {
      type: SortingType.Metric,
      direction: activeMetricInfo.unit === '$' ? SortingDirection.Up : SortingDirection.Down,
      metric: activeMetricInfo
    };

    if (!activeMetricInfo) {
      return <div>Loading 4...</div>;
    }

    const currentDataItem = this.props.downloadedChartData.find(x => sameKey(x.key, activeMetric));
    if (currentDataItem && currentDataItem.failed) {
      return (
        <div className="loading">
          {'An error occurred when requesting video data. Please try to reload.'}
        </div>
      );
    }

    const minWidthPerChart = 240;
    let smallChartsToShow = computeSmallCharts(availableMetricsInfo.length, width, minWidthPerChart);

    return (
      <div className="page-container">
        <div className="vspacing" />
        <Chart
          page={'dashboard'}
          downloadedChartData={this.props.downloadedChartData}
          requestChartData={this.props.requestChartData}
          setActiveMetric={this.props.setActiveMetric}
          availableMetricsInfo={availableMetricsInfo}
          summaryValues={this.props.summaryValues}
          activeMetric={activeMetric}
          setSummaryValue={this.props.setSummaryValue}
          toggleSummary={this.props.toggleSummary}
          firstSmallChart={this.props.firstSmallChart}
          setFirstSmallChart={this.props.setFirstSmallChart}
          filters={[]}
          metaTags={metaTags}
          hideSummary={true}
          hideToggleSummary={true}
          onlyGrandTotal={true}
          hideLegend={true}
          smallChartsToShow={smallChartsToShow}
          hideDemographicsAlternatives={true}
          hideArrows={smallChartsToShow >= availableMetricsInfo.length}
        />

        <VideoList
          canEdit={false}
          activeMetric={activeMetric}
          page={'dashboard'}
          availableMetricsInfo={availableMetricsInfo}
          personas={[]}
          videoMetrics={videoMetrics}
          videos={videos}
          metaTags={metaTags}
          updateVideoData={() => { return; }}
          requestData={() => requestData('content', stringfiedFilters, startDate, endDate, false)}
          hidePagination={true}
          hideSortBy={true}
          hideEditButton={true}
          hideViewModeMetricSelector={true}
          hideMetaTagColumn={true}
          sorting={sorting}
          limitOfVideosToShow={10}
          disableManualSorting={false}
          hideInactiveSortingArrows={false}
          showActiveMetricNameAndPageTypeInHeader={true}
          borderBetweenContentMarketing={true}
          dashboardHeader={true}
          showPlaylistsInViewMode={false}
          setActiveMetric={this.props.setActiveMetric}
          filterHeight={0}
        />
      </div>
    );
  }
}

const Dashboard = connect(
  (state: ApplicationState) => ({
    ...state.navMenu,
    ...state.data,
    ...state.filter,
    ...state.dateRange,
    ...state.loginData,
    ...state.chart
  }),
  {
    ...NavMenuStore.actionCreators,
    ...FilterStore.actionCreators,
    ...MarketingDataStore.actionCreators,
    ...DateRangeStore.actionCreators,
    ...LoginDataStore.actionCreators,
    ...ChartStore.actionCreators
  }
)(DashboardInternal);

export default Dashboard;
