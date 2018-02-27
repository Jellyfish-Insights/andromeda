import * as React from 'react';
import * as _ from 'underscore';
import {
  MetricInfo
} from '../types';
import * as ReactTooltip from 'react-tooltip';
import * as ReactMarkdown from 'react-markdown';

export interface MetricTooltipProps {
  id: string;
  prevId?: string;
  innerPresentation: JSX.Element;
  metricInfo: MetricInfo;
  className: string;
  afterShow(): void;
  afterHide(): void;
}

export const MetricTooltip = (props: MetricTooltipProps) => {
  const {
    id,
    prevId,
    innerPresentation,
    metricInfo,
    className,
    afterShow,
    afterHide,
  } = props;
  const markdownSource = metricInfo.markdownSource;

  const presentation = (
    <a
      data-tip={true}
      data-for={id}
      data-event="click focus"
      data-event-off="blur"
      style={{
        color: 'black',
        cursor: 'help'
      }}
      data-iscapture={false}
    >
      {innerPresentation}
    </a>
  );

  const tooltip = (
    <ReactTooltip
      id={id}
      className="metric-tooltip"
      effect="solid"
      place="bottom"
      type="light"
      border={true}
      event="focus"
      eventOff="blur"
      globalEventOff="click"
      getContent={() => {
        return <ReactMarkdown source={markdownSource} />;
      }
      }
      disable={prevId && prevId !== id}
      afterShow={afterShow}
      afterHide={afterHide}
      isCapture={false}
    />
  );

  return (
    <span className={className}>
      {presentation}
      {tooltip}
    </span>
  );
};
