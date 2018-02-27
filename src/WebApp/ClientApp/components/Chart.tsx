import * as React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as Moment from 'moment';
import { MetricInfo, ChartType, Tag } from '../types';
import {
  ChartDataItem,
  ChartDataItemKey,
  sameKey,
  ControllerType,
  Total,
  ChartObjectValues,
  TypeToValueToFilterItem
} from '../store/Chart';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  Bar,
  BarChart,
  Label,
  ReferenceLine
} from 'recharts';
import {
  FilterItemWithColor,
  SelectorType,
  Aggregated,
  DemographicsChartData,
  SetFirstSmallChartAction,
  SmallChartState,
  ChartObject
} from '../store/Chart';
import { FilterItem } from '../store/Filter';
import { formatPercentage, formatAndAddUnit } from '../reducers/helpers';
import { SelectType } from './SelectType';
import { SmallChart } from './SmallChart';
import CustomTooltip from './CustomTooltip';
import { Loading } from './Loading';
import { Summary } from './Summary';
import { SelectValue } from './SelectValue';
import update from 'immutability-helper';
import * as _ from 'underscore';
import { inRange, isEqual } from 'lodash';

export interface ChartProps {
  page: ControllerType | 'dashboard';
  requestChartData: (pageType: ControllerType, key: ChartDataItemKey, metrics: string[]) => void;
  setActiveMetric: (page: string, key: ChartDataItemKey) => void;
  setSummaryValue: (
    valueType: string,
    value: string,
    active: boolean,
    metaTags: { [index: string]: Tag[] },
    doNotCreate?: boolean
  ) => void;
  availableMetricsInfo: MetricInfo[];
  activeMetric: ChartDataItemKey;
  toggleSummary: () => void;
  setFirstSmallChart: (newValue: SmallChartState) => SetFirstSmallChartAction;
  filters: FilterItem[];
  metaTags: { [type: string]: Tag[] };
  hideSummary: boolean;
  hideToggleSummary?: boolean;
  downloadedChartData: ChartDataItem[];
  summaryValues: TypeToValueToFilterItem;
  firstSmallChart: SmallChartState;
  onlyGrandTotal?: boolean;
  hideLegend?: boolean;
  smallChartsToShow?: number;
  hideDemographicsAlternatives?: boolean;
  hideArrows?: boolean;
}

export interface ChartState {
  loading: boolean;
  countUpdates: number;
  data: Array<ChartObject>;
  groupedData: DemographicsChartData;
  total: ChartObjectValues;
  lastKey?: ChartDataItemKey;
  showAggregatedChartForGroupData: boolean;
  smallCharts: JSX.Element[];
  byGenderOnly: boolean;
  activeDataKey: string;
  tooltipActive: boolean;
}

export function getChartComponent(c: ChartType) {
  var ChartComponent: typeof LineChart | typeof BarChart = LineChart;
  switch (c) {
    case ChartType.LINE:
      ChartComponent = LineChart;
      break;
    case ChartType.BAR:
      ChartComponent = BarChart;
      break;
  }
  return ChartComponent;
}
export function getLineComponent(c: ChartType) {
  var LineComponent: typeof Line | typeof Bar = Line;
  switch (c) {
    case ChartType.LINE:
      LineComponent = Line;
      break;
    case ChartType.BAR:
      LineComponent = Bar;
      break;
  }
  return LineComponent;
}

export class Chart extends React.Component<ChartProps, ChartState> {

  constructor(props: ChartProps) {
    super(props);
    this.state = {
      loading: true,
      countUpdates: 0,
      showAggregatedChartForGroupData: false,
      smallCharts: [],
      byGenderOnly: false,
      activeDataKey: '',
      tooltipActive: false,
      data: [],
      groupedData: { data: {}, dataByGender: [] },
      total: {},
    };
  }

  componentDidMount() {
    this.tick();
    this.updateFirstSmallChart();

  }

  private updateFirstSmallChart() {
    let key = this.props.activeMetric;
    let page = this.props.page;

    let nextMetricIndex = this.props.availableMetricsInfo
      .findIndex(m => m.type === key.metric && m.pageType === key.pageType);

    let smallChartsToShow = this.props.smallChartsToShow || 4;
    let start: number = this.props.firstSmallChart[page];
    let end: number = this.props.firstSmallChart[page] + smallChartsToShow;

    if (nextMetricIndex !== -1 && !inRange(nextMetricIndex, start, end)) {
      this.props.setFirstSmallChart({ ...this.props.firstSmallChart, [page]: nextMetricIndex });
    }
  }

  componentDidUpdate(prevProps: ChartProps) {
    if (prevProps.downloadedChartData !== this.props.downloadedChartData ||
      prevProps.smallChartsToShow !== this.props.smallChartsToShow) {
      this.updateSmallCharts();
    }

    this.tick();

    if (!isEqual(prevProps.activeMetric, this.props.activeMetric)) {
      this.updateFirstSmallChart();
    }

  }

  getKeyForMetric(metricInfo: MetricInfo): ChartDataItemKey {
    let { activeMetric, page } = this.props;
    const requestKey: ChartDataItemKey = update(activeMetric, {
      metric: { $set: metricInfo.type },
      pageType: { $set: metricInfo.pageType as ControllerType },
      type: (type: string) => page !== 'dashboard' ? type : (metricInfo.pageType === 'marketing' ? 'persona' : 'length')
    });
    return requestKey;
  }

  getSmallCharts(): JSX.Element[] {
    let { availableMetricsInfo, smallChartsToShow } = this.props;
    let smallCharts = (availableMetricsInfo
      .sort((x, y) => x.typeId - y.typeId)
      .map((metricInfo, index) => {
        let metricKey = this.getKeyForMetric(metricInfo);
        let dataItem = this.props.downloadedChartData.find(x => sameKey(x.key, metricKey));
        let activeMetric = this.props.activeMetric as ChartDataItemKey;
        if (!dataItem) { return null; }

        if (dataItem.dataByGroup) {
          let data = dataItem.groupedData.data[Aggregated];
          if (!dataItem.loading && data.length === 0) {
            return null;
          }
        } else if (dataItem.data.length === 0) {
          return null;
        }

        const isActiveMetric: boolean = (
          metricInfo.type === activeMetric.metric
          && metricInfo.pageType === activeMetric.pageType
        );
        return (
          <SmallChart
            key={index}
            {...this.props}
            chartType={metricInfo.chartType}
            metricInfo={metricInfo}
            dataItem={dataItem}
            activeMetric={activeMetric}
            index={metricInfo.typeId}
            isActiveMetric={isActiveMetric}
            metricKeyToSet={metricKey}
            smallChartsToShow={smallChartsToShow}
          />
        );
      }).filter(x => x !== null) as JSX.Element[]);
    return smallCharts;
  }

  private async tick() {
    const {
      downloadedChartData,
      summaryValues,
      setSummaryValue,
      availableMetricsInfo,
      requestChartData,
      metaTags
    } = this.props;

    var activeMetric = this.props.activeMetric;
    if (!activeMetric) {
      return;
    }

    let chartData: ChartDataItem = downloadedChartData.find(
      x => sameKey(x.key, activeMetric)
    ) as ChartDataItem;

    if (!chartData) {
      // Segregate data by pageType
      const pageTypes: string[] = _.uniq(availableMetricsInfo.map(m => m.pageType));
      // Do one call of requestChartData for each page type
      pageTypes.forEach(pageType => {
        // For time series, call in batch
        const pageTypeMetricsInfo: MetricInfo[] = availableMetricsInfo
          .filter(m => m.pageType === pageType)
          .filter(m => m.type.toLowerCase().search('demographic') < 0);
        const metrics: string[] = pageTypeMetricsInfo.map(x => x.type);
        requestChartData(
          pageType as ControllerType,
          activeMetric.pageType === pageType ? activeMetric : this.getKeyForMetric(pageTypeMetricsInfo[0]),
          metrics
        );

        // For demographic metrics, call one by one
        const demographicMetrics: MetricInfo[] = availableMetricsInfo
          .filter(m => m.pageType === pageType)
          .filter(m => m.type.toLowerCase().search('demographics') >= 0);
        demographicMetrics.forEach(metric => {
          requestChartData(pageType as ControllerType, this.getKeyForMetric(metric), [metric.type]);
        });
      });
      this.setState({ loading: true });
    } else if (chartData.data.length > 0 || _.keys(chartData.groupedData.data).length > 0) {
      if (this.state.lastKey && !this.state.loading) {
        if (sameKey(this.state.lastKey, chartData.key)) { return; }
      }
      // Initially populate marketing personas and meta tags, including Grand Total
      if (chartData.data[0]) {
        const activeValues = summaryValues[activeMetric.type];
        const currentCategories: string[] = _.keys(chartData.total);
        const activeValueKeys: string[] = (activeValues && _.keys(activeValues)) || [];
        const newCategories = _.difference(currentCategories, activeValueKeys);
        newCategories.forEach(
          (x) => setSummaryValue(activeMetric.type, x, x === Total, metaTags, true)
        );
      }

      // Make sure to only call that function after the above code that sets colors,
      // otherwise SmallCharts will have colors that differ from Grand Total
      let smallCharts = this.getSmallCharts();
      this.setState((prevState) => {
        return {
          ...this.state,
          loading: false,
          data: chartData.data,
          groupedData: chartData.groupedData,
          total: chartData.total,
          countUpdates: prevState.countUpdates + 1,
          lastKey: chartData.key,
          smallCharts,
        };
      });

    }
  }

  updateSmallCharts() {
    let smallCharts = this.getSmallCharts();
    this.setState({
      smallCharts,
    });
  }

  private GetSummary(total: ChartObjectValues) {
    const {
      page,
      setSummaryValue,
      availableMetricsInfo,
      metaTags,
      activeMetric,
      hideSummary
    } = this.props;

    if (hideSummary) { return; }
    let isMarketingPage = page === 'marketing';
    let type: SelectorType = activeMetric.type;
    let activeData = this.getChartCurrentSummaryValues();
    let heightOfSummary = isMarketingPage ? '580px' : '530px';
    const set = (value: string, active: boolean) => setSummaryValue(type, value, active, metaTags);
    let metricInfo = availableMetricsInfo.find(x => x.type === activeMetric.metric) as MetricInfo;
    return (
      <div className="summary" style={{ maxHeight: 'inherit' }}>
        <div className="summary-top">
          {!isMarketingPage &&
            <SelectType
              currentType={type}
              handleOnChange={(value: SelectorType) => {
                this.props.setActiveMetric(page, { ...activeMetric, type: value });
              }}
              filters={this.props.filters}
            />
          }
          <SelectValue
            type={type}
            filters={activeData}
            handleOnChange={set}
          />
        </div>
        <div className="metric-status" style={{ marginTop: '20px', height: heightOfSummary, overflowY: 'auto' }}>
          <Summary
            items={activeData}
            setSummaryItem={set}
            total={total}
            highlightLine={(line: string) => this.highlightLine(line)}
            dehighlightLine={() => this.dehighlightLine()}
            metricInfo={metricInfo}
          />
        </div>
      </div>
    );
  }

  private GetMetricSelectors() {
    let charts = this.state.smallCharts;
    const { page, firstSmallChart, hideArrows } = this.props;
    let first = firstSmallChart[page];
    var chartsToShow: JSX.Element[] = [];
    let smallChartsToShow = this.props.smallChartsToShow || 4;
    if (charts.length <= smallChartsToShow) {
      chartsToShow = charts;
    } else {
      for (var i = 0; i < smallChartsToShow; i++) {
        chartsToShow.push(charts[(first + i) % charts.length]);
      }
    }

    let setFirstSmallChart = (n: number) => this.props.setFirstSmallChart({ ...firstSmallChart, [page]: n });
    let previous = (first - 1) < 0 ? charts.length - 1 : first - 1;
    let next = (first + 1) >= charts.length ? 0 : first + 1;
    return (
      <div className="metric-selector">
        {!hideArrows && (<span>
          <button onClick={() => setFirstSmallChart(previous)}>
            <FontAwesomeIcon icon={'angle-left'} />
          </button>
        </span>
        )}
        {chartsToShow}
        {!hideArrows && (<span>
          <button onClick={() => setFirstSmallChart(next)}>
            <FontAwesomeIcon icon={'angle-right'} />
          </button>
        </span>
        )}
      </div>
    );
  }

  private formatChartData(dateString: string) {
    return Moment(dateString).format('MMM D');
  }

  public render() {
    if (this.state.loading) { return <Loading externalHeight={600} height={100} width={100} />; }
    let { groupedData } = this.state;
    if (groupedData && _.keys(groupedData.data).length > 0) {
      return this.renderGroupedData();
    }
    return this.renderTimeSeries();
  }

  public renderGroupedData() {

    const { byGenderOnly } = this.state;
    const {
      page,
      hideSummary,
      hideToggleSummary,
      hideLegend,
      hideDemographicsAlternatives
    } = this.props;

    let isMarketingPage = page === 'marketing';
    if (isMarketingPage) { return '<div>Type of data is not supposed by marketing page.</div>'; }
    let data = this.state.groupedData as DemographicsChartData;
    const activeSummaryValues = this.getActiveSummaryValues();

    let lines = activeSummaryValues
      .map((x: FilterItemWithColor, index: number) => {
        return (
          <Bar
            key={index}
            dataKey={x.value}
            stroke={x.color}
            fillOpacity={1}
            fill={x.color}
            isAnimationActive={false}
          />
        );
      }
      ) as JSX.Element[];

    let metricSelectors = this.GetMetricSelectors();

    // Notice that the width of the ResponsiveContainer was set to 99% due to a bug when resizing that made
    // the space between toggle button and the end of the div shrink
    let groups = _.keys(data.data);
    var ChartComponent = BarChart;
    let showAggregated = this.state.showAggregatedChartForGroupData || byGenderOnly;
    // groups.length - 1 is used because groups.length includes the grand total, which is not presented together
    let height = showAggregated ? 600 : (600 / (groups.length - 1));
    let containerHeight = 600;
    let groupsToShow = groups.filter(x => showAggregated ? x === Aggregated : x !== Aggregated);
    const maxValue = this.calculateMaxValueOfDemographics(groupsToShow, data);
    return (
      <div className="chart-container">
        <div className="chart">
          <div className={'active-chart' + (hideSummary ? ' super' : ' normal')}>
            {!hideDemographicsAlternatives && (
              <div className="title">
                <label style={{ marginLeft: 10 }}>
                  <input
                    type="checkbox"
                    checked={this.state.showAggregatedChartForGroupData}
                    onClick={() =>
                      this.setState((prevState) => {
                        return {
                          showAggregatedChartForGroupData: !prevState.showAggregatedChartForGroupData,
                          byGenderOnly: false
                        };
                      })
                    }
                  />By Age Group Only
                </label>
                <label style={{ marginLeft: 10 }}>
                  <input
                    type="checkbox"
                    checked={byGenderOnly}
                    onClick={() =>
                      this.setState((prevState) => {
                        return {
                          byGenderOnly: !prevState.byGenderOnly,
                          showAggregatedChartForGroupData: false
                        };
                      })
                    }
                  />By Gender Only
                </label>
              </div>
            )}
            <div className={this.props.hideSummary ? 'super' : 'normal'}>
              <ResponsiveContainer width="99%" height={containerHeight}>
                <div>
                  {!byGenderOnly &&
                    groupsToShow.map((group, index) =>
                      <ResponsiveContainer width="99%" height={height} key={index}>
                        {
                          (() => {
                            return (
                              <ChartComponent
                                width={1000}
                                height={height}
                                data={data.data[group]}
                                margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
                              >
                                <XAxis
                                  dataKey="ageGroup"
                                  hide={!(index === groupsToShow.length - 1)}
                                />
                                <YAxis
                                  width={120}
                                  type="number"
                                  domain={[0, maxValue]}
                                  tickFormatter={(percentage: number) => formatPercentage(percentage)}
                                >
                                  {!showAggregated && <Label value={group} offset={0} position="insideLeft" />}
                                </YAxis>
                                <CartesianGrid strokeDasharray="3 3" />
                                <Tooltip
                                  labelFormatter={x => x}
                                  formatter={(value) => formatPercentage(value as number)}
                                />
                                {index === 0 && !hideLegend && <Legend verticalAlign="top" />}
                                {lines}
                              </ChartComponent>
                            );
                          }
                          )()
                        }
                      </ResponsiveContainer>
                    )
                  }
                  {byGenderOnly &&
                    <ResponsiveContainer width="99%" height={height}>
                      {
                        (() => {
                          return (
                            <ChartComponent
                              width={1000}
                              height={height}
                              data={data.dataByGender}
                              margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
                            >
                              <XAxis
                                dataKey="ageGroup"
                              />
                              <YAxis
                                tickFormatter={(percentage: number) => formatPercentage(percentage)}
                              />
                              <CartesianGrid strokeDasharray="3 3" />
                              <Tooltip
                                labelFormatter={x => x}
                                formatter={(value) => formatPercentage((value as number))}
                              />
                              {!hideLegend && <Legend verticalAlign="top" />}
                              {lines}
                            </ChartComponent>
                          );
                        }
                        )()
                      }
                    </ResponsiveContainer>
                  }
                </div>
              </ResponsiveContainer>
              {!hideToggleSummary && <div className="toggle-summary">
                <button onClick={() => this.props.toggleSummary()}>
                  <FontAwesomeIcon icon={this.props.hideSummary ? 'angle-left' : 'angle-right'} />
                </button>
              </div>}
            </div>
          </div>
          {this.GetSummary(this.state.total)}
        </div>
        {metricSelectors}
      </div>
    );
  }

  calculateMaxValueOfDemographics(groupsToShow: string[], data: DemographicsChartData): number {
    let maxValue = 0;
    let activeData = this.getChartCurrentSummaryValues();
    const { byGenderOnly } = this.state;
    let showAggregated = this.state.showAggregatedChartForGroupData || byGenderOnly;
    const activeKeys = activeData.filter(x => x.active).map((x: FilterItemWithColor) => x.value);
    if (!showAggregated) {
      groupsToShow.forEach(group => {
        data.data[group].forEach(dataGroup => {
          activeKeys.forEach((x: string) => {
            const value = dataGroup[x] as number;
            if (value > maxValue) { maxValue = value; }
          });
        });
      });
    } else {
      maxValue = 1;
    }
    maxValue = Math.ceil(maxValue * 10) / 10;
    return maxValue;
  }
  highlightLine(line: string): void {
    this.setState({ activeDataKey: line, tooltipActive: true });
  }

  dehighlightLine(): void {
    this.setState({ activeDataKey: '', tooltipActive: false });
  }

  getIntersectionDates(data: Array<ChartObject>): Array<string> {
    return data.map((x: ChartObject) => x.date as string).filter(x => Moment(x).dayOfYear() === 1);
  }

  private getChartCurrentSummaryValues(): FilterItemWithColor[] {
    const { summaryValues, activeMetric } = this.props;

    const activeDataValueToFilterItem = summaryValues[activeMetric.type];
    if (!activeDataValueToFilterItem) { return []; }
    const keys = _.keys(activeDataValueToFilterItem);
    return keys.map(key => activeDataValueToFilterItem[key]);
  }

  private getActiveSummaryValues(): FilterItemWithColor[] {
    const { onlyGrandTotal } = this.props;
    return this.getChartCurrentSummaryValues()
      .filter(x => (!onlyGrandTotal && x.active) || (onlyGrandTotal && x.value === Total));
  }

  public renderTimeSeries() {
    const { activeDataKey } = this.state;
    const data = this.state.data as ChartObject[];
    let {
      availableMetricsInfo,
      activeMetric,
      hideToggleSummary,
      hideLegend
    } = this.props;
    let metricInfo = availableMetricsInfo.find(x => x.type === activeMetric.metric) as MetricInfo;
    let chartType = availableMetricsInfo.find(a => a.type === activeMetric.metric)!.chartType || ChartType.LINE;
    // Persona A, Persona B, Topic 0...
    let activeSummaryValues = this.getActiveSummaryValues();

    let lines = activeSummaryValues
      .map((x: FilterItemWithColor, index: number) => {
        let key = `values.${x.value}`;
        switch (chartType) {
          case ChartType.LINE:
            return (
              <Line
                key={index}
                type="linear"
                dataKey={key}
                name={x.value}
                stroke={x.color}
                strokeWidth={activeDataKey === x.value ? 7 : 3}
                dot={false}
                isAnimationActive={false}
                onMouseOver={() => this.highlightLine(x.value)}
                onMouseLeave={() => this.dehighlightLine()}
              />
            );
          case ChartType.BAR:
            return (
              <Bar
                key={index}
                dataKey={key}
                name={x.value}
                stroke={x.color}
                fillOpacity={1}
                fill={x.color}
                isAnimationActive={false}
              />
            );
        }
      }
      ) as JSX.Element[];

    let metricSelectors = this.GetMetricSelectors();

    // Notice that the width of the ResponsiveContainer was set to 99% due to a bug when resizing that made
    // the space between toggle button and the end of the div shrink
    let height = 600;
    var ChartComponent = getChartComponent(chartType);

    // Get Date Intersections
    let intersectionDates = this.getIntersectionDates(data);
    let offsetInKey = lines.length;
    let referenceLines = intersectionDates.map((x, index) => (
      <ReferenceLine key={index + offsetInKey} x={x} stroke={0}>
        <Label value={Moment(x).format('YYYY')} position="bottom" offset={30} />
      </ReferenceLine>
    ));
    return (
      <div className="chart-container">
        <div className="chart">
          <div className={'active-chart' + (this.props.hideSummary ? ' super' : ' normal')}>
            <div className={this.props.hideSummary ? 'super' : 'normal'}>
              <ResponsiveContainer width="99%" height={height}>
                {
                  (() => {
                    return (
                      <ChartComponent
                        width={500}
                        height={height}
                        data={data}
                        margin={{ top: 5, right: 30, bottom: 25 }}
                      >
                        <XAxis
                          dataKey="date"
                          tickFormatter={(date: string) => this.formatChartData(date)}
                        />
                        <YAxis
                          width={120}
                          tickFormatter={(value) => formatAndAddUnit(
                            metricInfo,
                            value,
                            {
                              normalizeBigNumber: true,
                              hourFormat: 'auto'
                            }
                          )
                          }
                        />
                        <CartesianGrid strokeDasharray="3 3" />
                        <Tooltip
                          labelFormatter={x => Moment(x).format('dddd, MMM Do, YYYY')}
                          formatter={(value) => formatAndAddUnit(
                            metricInfo,
                            value as number,
                            {
                              normalizeBigNumber: false,
                              hourFormat: 'HMS'
                            }
                          )}
                          content={<CustomTooltip activeDataKey={activeDataKey} />}
                          animationDuration={300}
                        />
                        {!hideLegend && <Legend verticalAlign="top" />}
                        {lines}
                        {referenceLines}
                      </ChartComponent>
                    );
                  }
                  )()
                }
              </ResponsiveContainer>
              {!hideToggleSummary && <div className="toggle-summary">
                <button onClick={() => this.props.toggleSummary()}>
                  <FontAwesomeIcon icon={this.props.hideSummary ? 'angle-left' : 'angle-right'} />
                </button>
              </div>}
            </div>
          </div>
          {this.GetSummary(this.state.total)}
        </div>
        {metricSelectors}
      </div>
    );
  }
}
