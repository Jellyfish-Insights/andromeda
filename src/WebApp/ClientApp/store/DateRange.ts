import { Action, Reducer } from 'redux';
import * as Moment from 'moment';

export enum DateRanges {
  Last_7_Days = 'Last 7 days',
  Last_14_Days = 'Last 14 days',
  Last_30_Days = 'Last 30 days',
  Last_60_Days = 'Last 60 days',
  This_Week = 'This week',
  Last_Week = 'Last week',
  This_Month = 'This month',
  Last_Month = 'Last month',
  Year_To_Date = 'Year to Date',
  Last_Year = 'Last Year',
  Custom = 'Custom',
  Lifetime = 'Lifetime',
}

export interface DateRange {
  type: DateRanges;
  from: Date;
  to: Date;
}

export const DATE_RANGE_ARRAY = [
  DateRanges.Last_7_Days,
  DateRanges.Last_14_Days,
  DateRanges.Last_30_Days,
  DateRanges.Last_60_Days,
  DateRanges.This_Week,
  DateRanges.Last_Week,
  DateRanges.This_Month,
  DateRanges.Last_Month,
  DateRanges.Year_To_Date,
  DateRanges.Last_Year,
  DateRanges.Lifetime,
  DateRanges.Custom,
];

function assertUnreachable(x: never): never {
  throw new Error('Non-exhaustive switch statement');
}

export const getDateRange = (
  m: () => Moment.Moment,
  type: DateRanges,
  fromDate: Date,
  toDate: Date,
  firstDateD: Date,
): DateRange => {
  let from = Moment(fromDate);
  let to = Moment(toDate);
  let firstDate = Moment(firstDateD);
  switch (type) {
    case DateRanges.Last_7_Days:
      return {
        type: DateRanges.Last_7_Days,
        from: m().subtract(7, 'd').endOf('d').toDate(),
        to: m().subtract(1, 'd').toDate()
      };

    case DateRanges.Last_14_Days:
      return {
        type: DateRanges.Last_14_Days,
        from: m().subtract(14, 'd').endOf('d').toDate(),
        to: m().subtract(1, 'd').toDate()
      };

    case DateRanges.Last_30_Days:
      return {
        type: DateRanges.Last_30_Days,
        from: m().subtract(30, 'd').endOf('d').toDate(),
        to: m().subtract(1, 'd').toDate()
      };

    case DateRanges.Last_60_Days:
      return {
        type: DateRanges.Last_60_Days,
        from: m().subtract(60, 'd').endOf('d').toDate(),
        to: m().subtract(1, 'd').toDate()
      };

    case DateRanges.This_Week:
      return {
        type: DateRanges.This_Week,
        from: m().startOf('w').toDate(),
        to: m().subtract(1, 'd').toDate()
      };

    case DateRanges.Last_Week:
      return {
        type: DateRanges.Last_Week,
        from: m().subtract(1, 'w').startOf('w').toDate(),
        to: m().subtract(1, 'w').endOf('w').toDate()
      };

    case DateRanges.This_Month:
      return {
        type: DateRanges.This_Month,
        from: m().startOf('M').toDate(),
        to: m().subtract(1, 'd').toDate()
      };

    case DateRanges.Last_Month:
      return {
        type: DateRanges.Last_Month,
        from: m().subtract(1, 'M').startOf('M').toDate(),
        to: m().subtract(1, 'M').endOf('M').toDate()
      };

    case DateRanges.Year_To_Date:
      return {
        type: DateRanges.Year_To_Date,
        from: Moment(m().year().toString() + '-01-01').toDate(),
        to: m().subtract(1, 'd').endOf('d').toDate()
      };

    case DateRanges.Last_Year:
      return {
        type: DateRanges.Last_Year,
        from: m().subtract(1, 'y').endOf('d').toDate(),
        to: m().subtract(1, 'd').endOf('d').toDate()
      };

    case DateRanges.Lifetime:
      return {
        type: DateRanges.Lifetime,
        from: firstDate.toDate(),
        to: m().endOf('d').toDate()
      };

    case DateRanges.Custom:
      return {
        type: DateRanges.Custom,
        from: from.toDate(),
        to: to.toDate()
      };
  }

  return assertUnreachable(type);
};
export interface DateRangeState {
  rangeType: DateRanges;
  from: Date;
  to: Date;
}

interface SetDateRangeAction {
  type: 'SET_DATE_RANGE';
  rangeType: DateRanges;
  from: Date;
  to: Date;
}

type KnownAction = SetDateRangeAction;

export const actionCreators = {
  setDateRange: (rangeType: string, from: Date, to: Date) => <SetDateRangeAction> {
    type: 'SET_DATE_RANGE', from, to, rangeType
  },
};

const unloadedState: DateRangeState = {
  rangeType: DateRanges.Last_14_Days,
  from: Moment().subtract(14, 'd').toDate(),
  to: Moment().subtract(1, 'd').toDate()
};

export const reducer: Reducer<DateRangeState> = (state: DateRangeState, incomingAction: Action): DateRangeState => {
  const action = incomingAction as KnownAction;
  switch (action.type) {
    case 'SET_DATE_RANGE':
      return { rangeType: action.rangeType, from: action.from, to: action.to };
  }

  return state || unloadedState;
};
