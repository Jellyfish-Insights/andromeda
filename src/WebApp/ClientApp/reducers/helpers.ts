import { VideoMetric, Video, MetricInfo, Metric, PersonaMetric } from '../types';
import * as _ from 'underscore';
import { compose } from '@typed/compose';
import { Tag } from '../types';
import * as Moment from 'moment';
import { FilterItem, sortFilters } from '../store/Filter';
import update from 'immutability-helper';

export const DATE_FORMAT = 'YYYY/MM/DD';

// This code might help when aggregating results by time
const getPersonaArrays = (v: VideoMetric[]) => v.map(x => x.metricsPerPersona.map(y => y.persona));
export const getPersonas = (v: VideoMetric[]): string[] => compose(
  ((x: string[]): string[] => _.sortBy(x)),
  _.flatten,
  getPersonaArrays)(v).filter(function(item, pos, ary) {
    return !pos || item !== ary[pos - 1];
  });

const areTagsDifferent = (x: Tag, y: Tag): boolean => (x.type !== y.type) || (x.value !== y.value);
const getTagsInternal = (v: Video[]) => v.map(x => x.tags);
export const getTags = (v: Video[]): Tag[] => compose(
  ((x: Tag[]): Tag[] => _.sortBy(x, t => t.value)),
  ((x: Tag[]): Tag[] => _.sortBy(x, t => t.type)),
  _.flatten,
  getTagsInternal)(v).filter(function(item, pos, ary) {
    return !pos || areTagsDifferent(item, ary[pos - 1]);
  });

export function arrayToObject<T>(v: T[], f: Function) {
  var o: { [id: string]: T } = {};
  for (var i = 0; i < v.length; i++) {
    o[f(v[i])] = v[i];
  }
  return o;
}

export function capitalizeFirst(s: string): string {
  return s[0].toUpperCase() + s.substring(1);
}

export function compareString(s1: string, s2: string): number {
  var returnValue = 0;
  if (s1 < s2 || s1 === null) { returnValue = -1; }
  if (s1 > s2 || s2 === null) { returnValue = +1; }
  return returnValue;
}

function formatWithSuffix(valueNumber: number, threshold: number, suffix: string): string {
  let normalizedNumber = valueNumber / threshold;
  let hasDot = !Number.isInteger(normalizedNumber);
  let numberWithMaybeDotAndCommas = numberWithCommas(normalizedNumber.toFixed(hasDot ? 1 : 0));
  let metricValue = numberWithMaybeDotAndCommas + suffix;
  return metricValue;
}
// normalizeBigNumber means adding a K, M or G suffix depending on how big it is
export function formatNumber(valueNumber: number, normalizeBigNumber: boolean): string {
  if (valueNumber === null) { return '0'; }
  if (Math.abs(valueNumber) < 1 && valueNumber !== 0) {
    // converting to number and back to string will remove trailing zeroes
    return Number(formatSmallNumber(valueNumber)).toString();
  }
  return numberWithCommas(normalizeBigNumber ? formatBigNumber(valueNumber) : roundMinimal(valueNumber));
}

export function roundMinimal(value: number) {
  if (value % 1 !== 0) {
    return value.toFixed(value < 10 ? 1 : 0);
  } else {
    return value.toFixed(0);
  }

}

export function formatSmallNumber(value: number) {
  if (Math.abs(value) > 0.01) { return value.toFixed(3); }
  return value.toFixed(5);
}

export function formatBigNumber(value: number) {
  if (Math.abs(value) < 1000) { return roundMinimal(value); }
  value = value / 1000;
  if (Math.abs(value) < 1000) { return roundMinimal(value) + 'K'; }
  value = value / 1000;
  if (Math.abs(value) < 1000) { return roundMinimal(value) + 'M'; }
  value = value / 1000;
  return roundMinimal(value) + 'G';
}

export function formatTimeHMS(
  time: number,
  formatInfo: FormatInfo): string {
  const hours = Math.floor(time / 3600);
  let remainingTime = time - hours * 3600;
  const minutes = Math.floor(remainingTime / 60);
  const seconds = Math.round(remainingTime - minutes * 60);

  switch (formatInfo.hourFormat) {

    case 'HMS': {
      if (hours === 0) {
        if (minutes === 0) {
          return `${seconds}s`;
        }
        return `${minutes}min ${seconds}s`;
      }
      return `${numberWithCommas(hours)}h ${minutes}min ${seconds}s`;
    }

    case 'auto':
      if (time < 60) { return time.toFixed(0) + ' s'; }
      time = time / 60;
      if (time < 60) { return time.toFixed(0) + ' min'; }
      time = time / 60;
      return (formatInfo.normalizeBigNumber ? formatBigNumber(time) : time) + ' h';

    case 'seconds':
      return `${numberWithCommas(time)} s`;
  }
}

export const numberWithCommas = (x: number | string) => {
  var parts = x.toString().split('.');
  parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  return parts.join('.');
};

export function formatYMD(date: Date) {
  return Moment(date).format('YYYYMMDD');
}

export function formatYMDDashed(date: Date) {
  return Moment(date).format('YYYY-MM-DD');
}

export function formatYMDSlash(date: Date) {
  return Moment(date).format(DATE_FORMAT);
}

export function addSepsToDateString(date: string) {
  let dateWithSeps = date.replace(/[^0-9]/g, '');
  if (dateWithSeps.length > 4) {
    dateWithSeps = dateWithSeps.slice(0, 4) + '/' + dateWithSeps.slice(4);
  }
  if (dateWithSeps.length > 7) {
    dateWithSeps = dateWithSeps.slice(0, 7) + '/' + dateWithSeps.slice(7);
  }
  return dateWithSeps;
}

function stringFill1(x: string, n: number) {
  var s = '';
  while (s.length < n) {
    s += x;
  }
  return s;
}
// log10 is the log10 of the maximum number that will be given
export function prependZeros(log10: number, num: number): string {
  const digits = log10 + 1;
  return (stringFill1('0', log10) + num).slice(-digits);
}

export function stringfyFilters(filters: FilterItem[]) {
  return JSON
    .stringify(
      sortFilters(filters)
        .map(filter => { return { type: filter.type, value: filter.value }; })
    );
}
export function formatPercentage(percentage: number) {
  return formatNumber(percentage * 100, false) + '%';
}

export function expandedUnit(unit: string) {
  switch (unit) {
    case 's':
      return 'time';
    case '$':
      return 'dollar';
    default:
      return '';
  }

}
export function formatMetricUnit(metric: MetricInfo) {
  return metric.type + (metric.unit ? ' [' + expandedUnit(metric.unit) + ']' : '');
}

export interface FormatInfo {
  normalizeBigNumber: boolean;
  // auto means only hours, minutes or seconds depending on situation
  hourFormat: 'HMS' | 'auto' | 'seconds';
}

export function formatAndAddUnit(
  metricInfo: MetricInfo,
  value: number,
  formatInfo: FormatInfo,
): string {

  if (metricInfo.unit === 's') {
    return formatTimeHMS(value, formatInfo);
  } else {
    const formattedNumber = formatNumber(value, formatInfo.normalizeBigNumber);
    return metricInfo.unitSide === 'left'
      ? `${metricInfo.unit}${formattedNumber}`
      : `${formattedNumber}${metricInfo.unit}`;
  }
}

export function addControllerTypeToMetrics(metrics: VideoMetric[], controllerType: string): VideoMetric[] {
  const addControllerType = (totalMetrics: Metric[]): Metric[] => {
    return totalMetrics.map((m: Metric) => {
      return {
        ...m,
        controllerType
      };
    });
  };
  return metrics.map(c => update(c, {
    totalMetrics: addControllerType,
    metricsPerPersona: (metrics: PersonaMetric[]) => metrics && metrics.map(
      (p: PersonaMetric) => {
        return update(p, {
          metrics: addControllerType
        });
      })
  }));
}

export function computeSmallCharts(numberOfMetrics: number, width: number, minWidthPerChart: number) {
  if (width / numberOfMetrics >= minWidthPerChart) {
    return numberOfMetrics;
  }
  const alternativeNumber = Math.floor(width / minWidthPerChart);
  return alternativeNumber > 0 ? alternativeNumber : 1;
}

export function getFirstPublishDate(videos: Video[]): Date {
  if (videos.length === 0) {
    return new Date();
  }
  return _.min(videos.map(video => new Date(video.publishedAt)));
}
