import * as React from 'react';
import { connect } from 'react-redux';
import { Redirect } from 'react-router';
import { YearPageProps } from './AbstractPage';
import { ComponentIds } from '../routes';
import { ApplicationState } from '../store';
import * as ChartStore from '../store/Chart';
import * as MarketingDataStore from '../store/Data';
import * as DateRangeStore from '../store/DateRange';
import * as FilterStore from '../store/Filter';
import * as LoginDataStore from '../store/LoginData';
import * as NavMenuStore from '../store/NavMenu';
import { YearApPermissionLevels } from '../types';

type WebviewsProps = YearPageProps;

export class WebviewsInternal extends React.Component<WebviewsProps, {}> {
  constructor(props: WebviewsProps) {
    super(props);
    this.props.select(ComponentIds.Webviews);
  }

  canUserEdit() {
    return [YearApPermissionLevels.Admin, YearApPermissionLevels.Editor].includes(this.props.userrole);
  }

  public render() {
    if (!this.props.logged) { return <Redirect to="/account/login/webviews" />; }
    return (
      <iframe
        src="https://dash.parsely.com/to/?token=<YOUR TOKEN TO PARSELY HERE>"
        width="100%"
        height="2000"
        frameBorder="0"
        scrolling="no"
      />
    );
  }
}

const Webviews = connect(
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
)(WebviewsInternal);

export default Webviews;
