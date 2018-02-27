import * as React from 'react';
import { ResponsiveContainer, XAxis, YAxis, ComposedChart, Bar, Area } from 'recharts';
import { formatAndAddUnit } from '../reducers/helpers';
import {
  ChartDataItemKey,
  Total,
  Aggregated,
  ChartDataItem,
  ControllerType
} from '../store/Chart';
import { ChartType, MetricInfo } from '../types';
import * as ReactLoading from 'react-loading';

const PADDING = 10;

interface SmallChartProps {
  page: ControllerType | 'dashboard';
  metricInfo: MetricInfo;
  activeMetric: ChartDataItemKey;
  dataItem: ChartDataItem;
  index: number;
  chartType: ChartType;
  isActiveMetric: boolean;
  metricKeyToSet: ChartDataItemKey;
  smallChartsToShow?: number;
  setActiveMetric: (page: string, key: ChartDataItemKey) => void;
}

interface SmallChartState {
  hover: boolean;
}

export class SmallChart extends React.Component<SmallChartProps, SmallChartState> {

  constructor(props: SmallChartProps) {
    super(props);
    this.state = { hover: false };
  }

  public render() {
    const {
      metricInfo,
      index,
      dataItem,
      page,
      setActiveMetric,
      chartType,
      isActiveMetric,
      metricKeyToSet,
      smallChartsToShow
    } = this.props;
    const { hover } = this.state;
    let data: Array<object> = dataItem && dataItem.data;
    let yAxisKey = `values.${Total}`;
    let xAxisKey = 'date';
    if (dataItem && dataItem.dataByGroup) {
      data = dataItem.groupedData.data[Aggregated];
      xAxisKey = 'ageGroup';
      yAxisKey = Total;
    }
    if (!data || data.length === 0) {
      return (
        <div
          className={'small-chart-item items-' + (smallChartsToShow || 4)}
          key={index}
          style={{ marginLeft: 'auto', marginRight: 'auto', paddingTop: 15, paddingBottom: 15, pointerEvents: 'none' }}
        >
          <div style={{ display: 'block', margin: 'auto', width: 100, height: 100 }}>
            <ReactLoading.default type={'spin'} color={'#eb4032'} height={100} width={100} />
          </div>
        </div>
      );
    }
    const valueNumber: number = dataItem ? dataItem.total[Total] : 0.0;
    const valueWithUnit: string = formatAndAddUnit(
      metricInfo,
      valueNumber,
      {
        normalizeBigNumber: true,
        hourFormat: 'auto'
      });

    const onClick = () => setActiveMetric(page, metricKeyToSet);

    const color = (hover || isActiveMetric) ? 'white' : 'black';
    let backgroundColor = 'white';
    if (isActiveMetric) {
      backgroundColor = '#eb4032';
    } else if (hover) {
      backgroundColor = '#fa7676';
    }
    const style: React.CSSProperties = {
      color,
      backgroundColor,
      paddingBottom: PADDING,
      paddingTop: PADDING,
      cursor: isActiveMetric ? 'inherit' : 'pointer',
    };

    const areaColor = (hover || isActiveMetric) ? 'white' : 'rgba(189, 189, 190, 0.8)';
    return (
      <div
        className={'small-chart-item items-' + (smallChartsToShow || 4)}
        key={index}
        onClick={() => onClick()}
        onMouseEnter={() => this.setState({ hover: true })}
        onMouseLeave={() => this.setState({ hover: false })}
        style={style}
      >
        <ResponsiveContainer width="100%" height={100}>
          {
            (() => {
              const LineComponent = chartType === ChartType.BAR ? Bar : Area;
              const fill = areaColor;
              return (
                <ComposedChart
                  data={data}
                  margin={{ top: 5, right: 0, left: 0, bottom: 15 }}
                >
                  <XAxis hide={true} tick={false} dataKey={xAxisKey} />
                  <YAxis hide={true} tick={false} />
                  <LineComponent
                    dataKey={yAxisKey}
                    type={'linear'}
                    stroke={color}
                    fillOpacity={1}
                    fill={fill}
                    dot={false}
                    isAnimationActive={false}
                  />
                </ComposedChart>
              );
            })()
          }
        </ResponsiveContainer>
        <div style={style}>
          <div className="metric-type">{metricInfo.type}</div>
          <div className="metric-value">{valueWithUnit}</div>
        </div>
      </div>
    );
  }
}
