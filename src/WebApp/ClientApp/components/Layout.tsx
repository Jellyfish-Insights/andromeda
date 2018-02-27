import * as React from 'react';
import { connect } from 'react-redux';
import * as LoginDataStore from '../store/LoginData';
import { ApplicationState } from '../store';
import { NavMenu } from './NavMenu';
import fontawesome from '@fortawesome/fontawesome';
import brands from '@fortawesome/fontawesome-free-brands';
import pack, { IconDefinition } from '@fortawesome/fontawesome-free-solid';
import { faUser } from '@fortawesome/fontawesome-free-regular';
import { FeeLogo } from '../assets/img/FeeLogo';
import { RouteComponentProps } from 'react-router-dom';

export type LayoutProps =
  LoginDataStore.LoginDataState &
  typeof LoginDataStore.actionCreators &
  RouteComponentProps<{}>;

export class InternalLayout extends React.Component<LayoutProps, {}> {

  componentDidMount() {
    this.props.setLoggedAction();
  }

  public render() {
    if (this.props.logged === null) {
      return null;
    }
    fontawesome.library.add(brands, pack, faUser as IconDefinition);
    return (
      <div>
        <div>
          <NavMenu />
        </div>
        <div>
          {this.props.children}
        </div>
        <footer>
          <p><FeeLogo /></p>
          <p>Foundation for Economic Education © 2018 All Rights Reserved</p>
        </footer>
      </div>
    );
  }
}

export const Layout = connect(
  (state: ApplicationState) => ({
    ...state.loginData,
  }),
  {
    ...LoginDataStore.actionCreators,
  }
)(InternalLayout);
