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
import * as DateRangeStore from '../store/DateRange';
import { DateRanges } from '../store/DateRange';
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
  ConvertContentVideoListDataToExcel,
  GetMetaTagSheet,
  TrimMetrics
} from './ExcelExport';
import {
  SortingDirection,
  SortingType,
  Sorting,
} from './VideoSort';

type ContentProps = YearPageProps;

class ContentInternal extends React.Component<ContentProps> {
  constructor(props: ContentProps) {
    super(props);
    this.props.select(ComponentIds.Content);
  }

  componentDidMount() {
    let { from, to, filters } = this.props;
    this.props.setActivePage('content');
    this.props.requestData('content', stringfyFilters(filters), from, to, false);
    this.props.requestFilterData('content', false);
    this.props.select(ComponentIds.Content);
  }

  canUserEdit() {
    return [YearApPermissionLevels.Admin, YearApPermissionLevels.Editor].includes(this.props.userrole);
  }

  private filterRef = React.createRef<HTMLDivElement>();

  public render() {
    if (!this.props.logged) { return <Redirect to="/account/login/content" />; }

    let { videos, from, to, filters, requestData, metaTags, activeMetrics, updateVideoData } = this.props;
    const activeMetric = activeMetrics.content;
    let availableContentMetrics = this.props.availableMetricsInfo.filter(x => x.pageType === 'content');
    const stringfiedFilters = stringfyFilters(filters);
    const metricTitle = (activeMetric && activeMetric.metric) || '';
    let activeChartItems = GetActiveChartItems(this.props.downloadedChartData, this.props);
    const firstDate = getFirstPublishDate(videos);
    if (!activeMetric) {
      return <div>Loading...</div>;
    }

    const activeMetricInfo = availableContentMetrics.find(
      x => x.type === activeMetric.metric && x.pageType === activeMetric.pageType
    );

    const sorting: Sorting = {
      type: SortingType.Metric,
      direction: activeMetricInfo.unit === '$' ? SortingDirection.Up : SortingDirection.Down,
      metric: activeMetricInfo
    };

    const videosObj = arrayToObject(videos, (v: Video) => v.id);

    const currentDataItem = this.props.downloadedChartData.find(x => sameKey(x.key, activeMetric));
    if (currentDataItem && currentDataItem.failed) {
      return (
        <div className="loading">
          {'An error occurred when requesting video data. Please try to reload.'}
        </div>
      );
    }

    return (
      <div className="page-container">
        <Filter
          videos={videos}
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
              this.props.requestData('content', stringfiedFilters, newFrom, newTo, false);
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
                ConvertContentVideoListDataToExcel(
                  'content',
                  TrimMetrics(videosObj, this.props.content),
                  videosObj,
                  availableContentMetrics,
                  from,
                  to,
                  stringfiedFilters,
                  metaTags
                ),
                GetMetaTagSheet(metaTags)
              ];
            },
            fileName: GetExcelFileName('content')
          }}
        />

        {this.props.videos.length !== 0 && <Chart
          page={'content'}
          downloadedChartData={this.props.downloadedChartData}
          requestChartData={this.props.requestChartData}
          setActiveMetric={this.props.setActiveMetric}
          availableMetricsInfo={availableContentMetrics}
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

        {(availableContentMetrics.length > 0 && videos.length > 0) ?
          <VideoList
            canEdit={this.canUserEdit()}
            hideEditButton={!this.canUserEdit()}
            activeMetric={activeMetric}
            page={'content'}
            availableMetricsInfo={availableContentMetrics}
            personas={[]}
            videoMetrics={addControllerTypeToMetrics(this.props.content, 'content')}
            videos={this.props.videos}
            metaTags={this.props.metaTags}
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

const Content = connect(
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
)(ContentInternal);

export default Content;
