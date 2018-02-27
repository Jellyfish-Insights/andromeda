import * as React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { FilterItemWithColor } from '../store/Chart';
import { formatAndAddUnit } from '../reducers/helpers';
import { MetricInfo } from '../types';

export interface SummaryProps {
  items: FilterItemWithColor[];
  setSummaryItem: (value: string, active: boolean) => void;
  total: { [x: string]: number };
  highlightLine: (line: string) => void;
  dehighlightLine: () => void;
  metricInfo: MetricInfo;
}

export class Summary extends React.Component<SummaryProps, {}> {

  private summaryLine(x: FilterItemWithColor, index: number) {
    const { metricInfo, total, setSummaryItem } = this.props;
    const unset = (value: string): void => setSummaryItem(value, false);
    const valueExact = total[x.value] ? total[x.value] : 0;
    const metricValue = formatAndAddUnit(
      metricInfo,
      valueExact,
      {
        normalizeBigNumber: true,
        hourFormat: 'auto'
      });
    const tooltipMetricValue = formatAndAddUnit(
      metricInfo,
      valueExact,
      {
        normalizeBigNumber: false,
        hourFormat: 'HMS'
      });

    const style = {
      color: x.color,
    };
    return (
      <div
        className="line"
        key={index}
        onMouseOver={() => this.props.highlightLine(x.value)}
        onMouseLeave={() => this.props.dehighlightLine()}
      >
        <div className="item" style={style}>
          <FontAwesomeIcon icon={['far', 'user']} className="f-icon" />
          <div
            className="category"
            title={x.value}
          >
            {x.value}
          </div>
          <button onClick={() => unset(x.value)}>
            <FontAwesomeIcon style={style} icon="times" className="f-icon-close" />
          </button>
        </div>
        <div
          className="count"
          style={style}
          title={'Exact value: ' + tooltipMetricValue}
        >
          {metricValue}
        </div>
      </div>
    );
  }
  public render() {
    let items = this.props.items
      .filter(x => x.active)
      .sort((x, y) => {
        return (x.order || 0) - (y.order || 0);
      })
      .map((x, index) => this.summaryLine(x, index));

    return (
      <div className="summary-filters">
        {items}
      </div>
    );
  }
}
