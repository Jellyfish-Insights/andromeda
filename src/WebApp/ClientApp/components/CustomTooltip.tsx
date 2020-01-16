import * as React from 'react';
import { TooltipProps } from 'recharts';

export interface TooltipPropsWithExtras extends TooltipProps {
  activeDataKey: string;
}

class CustomTooltip extends React.Component<TooltipPropsWithExtras, {}> {

  renderContent() {
    const { payload, separator, formatter, itemStyle, itemSorter, activeDataKey } = this.props;

    if (payload && payload.length) {
      const listStyle = { padding: 0, margin: 0 };
      const items = payload.sort(itemSorter)
        .map((entry, i) => {
          const finalItemStyle = {
            display: 'block',
            paddingTop: 4,
            paddingBottom: 4,
            color: entry.color || '#000',
            ...itemStyle,
            ...((activeDataKey === entry.name ? { fontWeight: 700 } : {}) as object),
          };
          const finalFormatter = entry.formatter || formatter;

          return (
            <li className="recharts-tooltip-item" key={`tooltip-item-${i}`} style={finalItemStyle}>
              <span className="recharts-tooltip-item-name">{entry.name}</span>
              <span className="recharts-tooltip-item-separator">{separator}</span>
              <span className="recharts-tooltip-item-value">
                {finalFormatter ? finalFormatter(entry.value, entry.name, entry, i) : entry.value}
              </span>
              <span className="recharts-tooltip-item-unit">{entry.unit || ''}</span>
            </li>
          );
        });

      return <ul className="recharts-tooltip-item-list" style={listStyle}>{items}</ul>;
    }

    return null;
  }

  render() {
    const { labelStyle, label, labelFormatter, wrapperStyle } = this.props;
    const finalStyle: React.CSSProperties = {
      margin: 0,
      padding: 10,
      backgroundColor: '#fff',
      border: '1px solid #ccc',
      whiteSpace: 'nowrap',
      ...wrapperStyle,
    };
    const finalLabelStyle = {
      margin: 0,
      ...labelStyle,
    };
    let finalLabel: React.ReactText | React.ReactNode = label || '';

    if (label && labelFormatter) { finalLabel = labelFormatter(label); }

    return (
      <div className="recharts-default-tooltip" style={finalStyle}>
        <p className="recharts-tooltip-label" style={finalLabelStyle}>{finalLabel}</p>
        {this.renderContent()}
      </div>
    );
  }
}

export default CustomTooltip;
