import * as React from 'react';
import { connect } from 'react-redux';
import { Redirect } from 'react-router';
import {
  stringfyFilters,
  addControllerTypeToMetrics,
  getFirstPublishDate,
  arrayToObject
} from '../reducers/helpers';
import { ComponentIds } from '../routes';
import { ApplicationState } from '../store';
import * as ChartStore from '../store/Chart';
import { sameKey } from '../store/Chart';
import * as MarketingDataStore from '../store/Data';
import { DateRanges } from '../store/DateRange';
import * as DateRangeStore from '../store/DateRange';
import * as FilterStore from '../store/Filter';
import * as LoginDataStore from '../store/LoginData';
import * as NavMenuStore from '../store/NavMenu';
import { YearApPermissionLevels, Video } from '../types';
import { Chart } from './Chart';
import { DateRangePicker } from './DateRangePicker';
import { Filter } from './Filter';
import { VideoList } from './VideoList';
import { YearPageProps, GetActiveChartItems, GetExcelFileName } from './AbstractPage';
import {
  ConvertChartDataToExcel,
  ConvertMarketingVideoListDataToExcel,
  GetMetaTagSheet,
  TrimMetrics
} from './ExcelExport';
import * as _ from 'underscore';
import {
  SortingDirection,
  SortingType,
  Sorting,
} from './VideoSort';

type MarketingProps = YearPageProps;

type MarketingState = {
  activeMetric: string;
  activeSources: { [key: string]: boolean };
  // tag type -> tag value -> active?
  filters: { [key: string]: { [key: string]: boolean } };
};

export class MarketingInternal extends React.Component<
  MarketingProps,
  MarketingState
  > {

  constructor(props: MarketingProps) {
    super(props);
    this.props.select(ComponentIds.Marketing);
    this.state = {
      activeMetric: '',
      activeSources: {},
      filters: {},
    };
  }

  componentDidMount() {
    let { from, to, filters } = this.props;
    this.props.setActivePage('marketing');
    this.props.requestData('marketing', stringfyFilters(filters), from, to, false);
    this.props.requestFilterData('marketing', false);
    this.props.select(ComponentIds.Marketing);
  }

  canUserEdit() {
    return [YearApPermissionLevels.Admin, YearApPermissionLevels.Editor].includes(this.props.userrole);
  }

  private filterRef = React.createRef<HTMLDivElement>();

  public render() {
    if (!this.props.logged) { return <Redirect to="/account/login/marketing" />; }
    let availableMarketingMetrics = this.props.availableMetricsInfo.filter(x => x.pageType === 'marketing');
    const {
      from,
      to,
      summaryValues,
      filters,
      videos,
      updateVideoData,
      requestData,
      metaTags,
      activeMetrics
    } = this.props;
    const activeMetric = activeMetrics.marketing;
    const metricTitle = (activeMetric && activeMetric.metric) || '';
    const personaFilterItems = summaryValues.persona
      ? _.keys(summaryValues.persona)
        .map(key => summaryValues.persona[key])
        .filter(persona => persona.active)
      : [];
    const firstDate = getFirstPublishDate(videos);

    const stringfiedFilters = stringfyFilters(filters);
    let activeChartItems = GetActiveChartItems(this.props.downloadedChartData, this.props);
    if (!activeMetric) {
      return <div>Loading...</div>;
    }
    const currentDataItem = this.props.downloadedChartData.find(x => sameKey(x.key, activeMetric));
    if (currentDataItem && currentDataItem.failed) {
      return (
        <div className="loading">
          {'An error occurred when requesting video data. Please try to reload.'}
        </div>
      );
    }

    const activeMetricInfo = availableMarketingMetrics.find(
      x => x.type === activeMetric.metric && x.pageType === activeMetric.pageType
    );

    const sorting: Sorting = {
      type: SortingType.Metric,
      direction: activeMetricInfo.unit === '$' ? SortingDirection.Up : SortingDirection.Down,
      metric: activeMetricInfo
    };

    const videosObj = arrayToObject(videos, (v: Video) => v.id);

    return (
      <div className="page-container">
        <Filter
          videos={this.props.videos}
          platforms={this.props.platforms}
          tags={this.props.tags}
          playlists={this.props.playlists}
          personas={this.props.personas}
          showSelector={this.props.showSelector}
          setFilter={
            (tagValue: string, tagType: string, active: boolean) => this.props.setFilter(tagValue, tagType, active)
          }
          filters={this.props.filters}
          submitSearchBarValue={this.props.submitSearchBarValue}
          filterRef={this.filterRef}
        />

        <DateRangePicker
          onSelect={
            (dateRange: DateRanges, newFrom: Date, newTo: Date, onlyRange: boolean) => {
              // Not only set the date range, but also request new VideoList data
              this.props.setDateRange(dateRange, newFrom, newTo);
              if (onlyRange) { return; }
              this.props.requestData('marketing', stringfiedFilters, newFrom, newTo, false);
            }
          }
          rangeType={this.props.rangeType}
          from={this.props.from}
          to={this.props.to}
          firstDate={firstDate}
          metricTitle={metricTitle}
          getSpreadsheetData={{
            getData: () => {
              return [
                ...ConvertChartDataToExcel(activeChartItems),
                ConvertMarketingVideoListDataToExcel(
                  'marketing',
                  TrimMetrics(videosObj, this.props.marketing),
                  videosObj,
                  availableMarketingMetrics,
                  from,
                  to,
                  stringfiedFilters,
                  metaTags
                ),
                GetMetaTagSheet(metaTags)
              ];
            },
            fileName: GetExcelFileName('marketing')
          }}

        />
        {this.props.videos.length !== 0 && <Chart
          page={'marketing'}
          downloadedChartData={this.props.downloadedChartData}
          requestChartData={this.props.requestChartData}
          setActiveMetric={this.props.setActiveMetric}
          availableMetricsInfo={availableMarketingMetrics}
          summaryValues={this.props.summaryValues}
          activeMetric={activeMetric}
          hideSummary={this.props.hideSummary}
          setSummaryValue={this.props.setSummaryValue}
          toggleSummary={this.props.toggleSummary}
          firstSmallChart={this.props.firstSmallChart}
          setFirstSmallChart={this.props.setFirstSmallChart}
          filters={this.props.filters}
          metaTags={metaTags}
        />}

        {(availableMarketingMetrics.length > 0 && videos.length > 0 && personaFilterItems.length > 0) ?
          <VideoList
            canEdit={this.canUserEdit()}
            hideEditButton={!this.canUserEdit()}
            activeMetric={activeMetric}
            page={'marketing'}
            availableMetricsInfo={availableMarketingMetrics}
            personas={personaFilterItems}
            videoMetrics={addControllerTypeToMetrics(this.props.marketing, 'marketing')}
            videos={this.props.videos}
            metaTags={metaTags}
            updateVideoData={updateVideoData}
            requestData={() => requestData('content', stringfiedFilters, from, to, false)}
            sorting={sorting}
            showPlaylistsInViewMode={true}
            setActiveMetric={this.props.setActiveMetric}
            filterHeight={this.filterRef.current && this.filterRef.current.clientHeight || 0}
          />
          :
          <div className="loading">
            {'No videos were returned for the current filters and date range.'}
          </div>
        }
      </div>
    );
  }
}

const Marketing = connect(
  (state: ApplicationState) => ({
    ...state.navMenu,
    ...state.data,
    ...state.filter,
    ...state.loginData,
    ...state.dateRange,
    ...state.chart
  }),
  {
    ...NavMenuStore.actionCreators,
    ...MarketingDataStore.actionCreators,
    ...FilterStore.actionCreators,
    ...DateRangeStore.actionCreators,
    ...LoginDataStore.actionCreators,
    ...ChartStore.actionCreators
  }
)(MarketingInternal);

export default Marketing;
