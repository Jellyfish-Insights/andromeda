import * as React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as $ from 'jquery';
import * as Moment from 'moment';
import DayPicker from 'react-day-picker';
import { DateRanges, getDateRange, DateRange, DATE_RANGE_ARRAY } from '../store/DateRange';
import { ExportToExcelButton, ExportToExcelProps } from './ExportToExcelButton';
import { formatYMDSlash, DATE_FORMAT, addSepsToDateString } from '../reducers/helpers';

export interface SimpleDateRange {
  from: Date | undefined;
  to: Date | undefined;
  enteredTo: Date | undefined;
}

export interface DateRangeState {
  selection: DateRange;
  internal: SimpleDateRange;
  fromInput: string;
  toInput: string;
}

type DateRangeProps = {
  onSelect: (dateRange: DateRanges, from: Date, to: Date, onlyRange: boolean) => void;
  rangeType: DateRanges;
  from: Date;
  to: Date;
  firstDate: Date; // the mininum publishing date of the set of videos
  metricTitle: string;
  getSpreadsheetData: ExportToExcelProps;
};

export class DateRangePicker extends React.Component<DateRangeProps, DateRangeState> {

  constructor(props: DateRangeProps) {
    super(props);
    this.handleDayClick = this.handleDayClick.bind(this);
    this.handleDayMouseEnter = this.handleDayMouseEnter.bind(this);
    this.handleResetClick = this.handleResetClick.bind(this);

    let range = getDateRange(
      () => Moment().clone(),
      props.rangeType,
      props.from,
      props.to,
      props.firstDate
    );

    this.state = {
      selection: range,
      internal: {
        from: range.from,
        to: range.to,
        enteredTo: range.to
      },
      fromInput: formatYMDSlash(range.from),
      toInput: formatYMDSlash(range.to)
    };
  }

  componentDidMount() {
    // This line is actually required for the dropdown to work
    // tslint:disable-next-line:no-unused-expression
    new Foundation.Dropdown($('.date-dropdown'), {
      closeOnClick: true
    });
  }

  componentDidUpdate(prevProps: DateRangeProps) {
    if (prevProps.rangeType === this.props.rangeType
      && this.props.rangeType === DateRanges.Lifetime
      && prevProps.firstDate.getTime() !== this.props.firstDate.getTime()) {
      this.props.onSelect(DateRanges.Lifetime, this.props.firstDate, prevProps.to, true);
      this.setState((prevState: DateRangeState) => {
        return {
          internal: {
            ...prevState.internal,
            from: this.props.firstDate,
          }
        };
      });
    }
  }

  private isSelectingFirstDay(from: Date | undefined, to: Date | undefined, day: Date) {
    const isBeforeFirstDay = from && Moment(day).isBefore(from);
    const isRangeSelected = from && to;
    return !from || isBeforeFirstDay || isRangeSelected;
  }

  private submitInputDatesOnEnter(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key !== 'Enter') {
      return;
    }
    event.preventDefault();
    event.stopPropagation();

    this.submitInputDates();
  }

  private submitInputDates() {
    const { firstDate } = this.props;
    let { fromInput, toInput } = this.state;
    if (Moment(fromInput, DATE_FORMAT, true).isValid() && Moment(toInput, DATE_FORMAT, true).isValid()) {
      if (Moment(fromInput, DATE_FORMAT).isAfter(Moment(toInput, DATE_FORMAT))) {
        alert(`From (${fromInput}) should be a date before To (${toInput}). Reverting date to previous.`);
        this.setState((prevState) => {
          return {
            fromInput: formatYMDSlash(prevState.selection.from),
            toInput: formatYMDSlash(prevState.selection.to),
          };
        });
      }
      let from = Moment(fromInput, DATE_FORMAT).toDate();
      let to = Moment(toInput, DATE_FORMAT).toDate();
      let range = getDateRange(() => Moment().clone(), DateRanges.Custom, from, to, firstDate);
      this.props.onSelect(DateRanges.Custom, from, to, false);
      this.setState({
        selection: range,
        internal: {
          from: from,
          to: to,
          enteredTo: to
        }
      });
    } else {
      alert('Your input is invalid. It should follow the format ' + DATE_FORMAT);
      this.setState((prevState) => {
        return {
          fromInput: formatYMDSlash(prevState.selection.from),
          toInput: formatYMDSlash(prevState.selection.to),
        };
      });
    }
  }

  private handleDayClick(day: Date) {
    let { from, to } = this.state.internal;
    if (this.isSelectingFirstDay(from, to, day)) {
      this.setState((prevState: DateRangeState) => {
        return {
          ...prevState,
          selection: {
            ...prevState.selection,
            type: DateRanges.Custom
          },
          internal: {
            from: day,
            to: undefined,
            enteredTo: undefined
          }
        };
      });
    } else {
      this.setState((prevState: DateRangeState) => {
        this.props.onSelect(DateRanges.Custom, prevState.internal.from as Date, day, false);
        return {
          ...prevState,
          selection: {
            ...prevState.selection,
            from: prevState.internal.from as Date,
            to: day
          },
          internal: {
            ...prevState.internal,
            to: day,
            enteredTo: day
          },
          fromInput: formatYMDSlash(prevState.internal.from as Date),
          toInput: formatYMDSlash(day),
        };
      });
    }
  }

  private handleDayMouseEnter(day: Date) {
    if (this.state.selection.type !== DateRanges.Custom) {
      return;
    }
    let { from, to } = this.state.internal;
    if (!this.isSelectingFirstDay(from, to, day)) {
      this.setState((prevState) => {
        return {
          ...prevState,
          internal: {
            ...prevState.internal,
            enteredTo: day
          },
        };
      });
    }
  }

  private handleResetClick() {
    this.setState((prevState) => {
      return {
        ...prevState,
        internal: {
          from: undefined,
          to: undefined,
          enteredTo: undefined
        }
      };
    });
  }

  private getSelectedDays(dateRange: SimpleDateRange) {
    let { from, to, enteredTo } = dateRange;
    if (from && !to) {
      return from;
    }
    if (from && enteredTo) {
      return [from, { from, to: enteredTo }];
    }
    return undefined;
  }

  public render() {
    const { metricTitle, onSelect, firstDate } = this.props;
    let { from, to, enteredTo } = this.state.internal;
    let { fromInput, toInput } = this.state;
    let modifiers = { start: from, end: enteredTo };
    let disabledDays = from ? { before: from } : undefined;
    let selectedDays = this.getSelectedDays(this.state.internal);
    let custom = this.state.selection.type === DateRanges.Custom;

    let m = () => Moment().clone();
    let getRange = (type: DateRanges) => getDateRange(m, type, from, to, firstDate);
    let rangeArray = DATE_RANGE_ARRAY.map(getRange);

    let buttons: DateRanges[] = [
      DateRanges.Last_7_Days,
      DateRanges.Last_14_Days,
      DateRanges.Last_30_Days
    ];

    let message = (
      <div>
        {!from && !to && 'Please select the first day.'}
        {from && !to && 'Please select the last day.'}
        <div className="date-element">
          <label>From</label>
          <input
            value={fromInput}
            style={Moment(fromInput, DATE_FORMAT, true).isValid() ? {} : { borderColor: 'red' }}
            onChange={(e) => {
              this.setState({ fromInput: addSepsToDateString(e.target.value) });
            }}
            onKeyDown={(e) => this.submitInputDatesOnEnter(e)}
            onBlur={(_) => this.submitInputDates()}
            pattern="[0-9]{4}/[0-9]{2}/[0-9]{2}"
          />
        </div>
        <div className="date-element">
          <label>To</label>
          <input
            value={toInput}
            style={Moment(toInput, DATE_FORMAT, true).isValid() ? {} : { borderColor: 'red' }}
            onChange={(e) => {
              this.setState({ toInput: addSepsToDateString(e.target.value) });
            }}
            onKeyDown={(e) => this.submitInputDatesOnEnter(e)}
            onBlur={(_) => this.submitInputDates()}
          />
        </div>
        {custom ? (from &&
          to && (
            <div>
              <button
                className="link"
                onClick={this.handleResetClick}
              >
                Reset
              </button>
            </div>
          )) : ''}
      </div>);

    let titleStyle: React.CSSProperties = {
      color: 'black',
      fontSize: 22,
      fontWeight: 500,
      marginLeft: 10,
      display: 'flex',
      justifyContent: 'center',
      alignContent: 'center',
      flexDirection: 'column'
    };

    return (
      <div className="date-picker-container">
        <div className="date-picker">
          <div style={titleStyle}>
            {metricTitle}
          </div>
          <div className="placeholder" />
          <div className="dropdown-container">
            <a className="button dropdown-toggle" data-toggle="date-dropdown">
              {this.state.selection.type}<FontAwesomeIcon className="icon-right" icon="calendar-alt" />
            </a>
            <div id="date-dropdown" className="date-dropdown" data-dropdown={true} data-auto-focus={true}>
              <ul className="date-menu">
                {
                  rangeArray.map((range, index) => {
                    return <li key={index}>
                      <a
                        className={this.state.selection.type === range.type ? 'active button' : 'button'}
                        onClick={() => this.updateType(range.type)}
                      >
                        {range.type}
                      </a>
                    </li>;
                  })
                }
              </ul>
              <div className={'range-selector' + (custom ? '' : ' disabled')}>
                <DayPicker
                  className="Range"
                  numberOfMonths={1}
                  month={this.state.selection.from}
                  fromMonth={custom ? undefined : from}
                  toMonth={custom ? undefined : to}
                  selectedDays={selectedDays}
                  disabledDays={disabledDays}
                  modifiers={modifiers}
                  onDayClick={this.handleDayClick}
                  onDayMouseEnter={this.handleDayMouseEnter}
                />
                {message}
              </div>
            </div>
          </div>
          <div className="select-pre-defined-buttons">
            {
              buttons.map((x: string, index: number) => {
                let range = rangeArray.find(y => y.type === x) as DateRange;
                let internal: SimpleDateRange = { from: range.from, to: range.to, enteredTo: range.to };
                let className = this.state.selection.type === range.type ? 'active' : '';
                return (
                  <button
                    key={index}
                    onClick={() => {
                      onSelect(range.type, range.from, range.to, false);
                      this.setState(() => {
                        return {
                          selection: range,
                          internal: internal,
                          fromInput: formatYMDSlash(range.from),
                          toInput: formatYMDSlash(range.to),
                        };
                      });
                    }
                    }
                    className={className}
                  >
                    {range.type}
                  </button>
                );
              })
            }
          </div>

          <ExportToExcelButton
            getData={this.props.getSpreadsheetData.getData}
            fileName={this.props.getSpreadsheetData.fileName}
          />

        </div>
      </div>
    );
  }

  private updateType(value: string) {

    let { firstDate } = this.props;
    let { from, to } = this.state.selection;
    let range = getDateRange(() => Moment().clone(), value as DateRanges, from, to, firstDate);

    if (range) {
      this.props.onSelect(range.type, range.from, range.to, false);
      this.setState({
        selection: range,
        fromInput: formatYMDSlash(range.from),
        toInput: formatYMDSlash(range.to),
        internal: {
          from: range.from,
          to: range.to,
          enteredTo: range.to
        }
      });
    }
  }
}
