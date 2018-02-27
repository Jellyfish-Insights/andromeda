import * as React from 'react';
import { Route, Redirect } from 'react-router-dom';
import { Layout } from './components/Layout';
import Dashboard from './components/Dashboard';
import Content from './components/Content';
import Marketing from './components/Marketing';
import Webviews from './components/Webviews';
import Login from './components/account/Login';
import AccessDenied from './components/account/AccessDenied';
import PersonaManagement from './components/admin/PersonaManagement';
import VideoManagement from './components/admin/VideoManagement';
import TagManagement from './components/admin/TagManagement';
import UserManagement from './components/admin/UserManagement';

export const enum ComponentIds {
  Dashboard = 0,
  Content = 1,
  Marketing = 2,
  PersonaManagement = 3,
  VideoManagement = 4,
  TagManagement = 5,
  UserManagement = 6,
  Webviews = 7
}

// For every component that needs to render the nav bar and footer
const LayoutRoute = (params: any) => {
  // dbinding here instead of directly in the parameters prevents some TS errors
  let { component: Component, ...rest } = params;
  return (
    <Route
      {...rest}
      render={props => (
        <Layout {...props}>
          <Component {...props} />
        </Layout>
      )}
    />
  );
};

export const routes = (
  <React.Fragment>
    <Route
      exact={true}
      path="/"
      render={() => (
        <Redirect to="/dashboard" />
      )}
    />
    <LayoutRoute path="/dashboard" component={Dashboard} />
    <LayoutRoute path="/content" component={Content} />
    <LayoutRoute path="/audiences" component={Marketing} />
    <LayoutRoute path="/webviews" component={Webviews} />
    <LayoutRoute path="/admin/user" component={UserManagement} />
    <LayoutRoute path="/admin/tag" component={TagManagement} />
    <LayoutRoute path="/admin/persona" component={PersonaManagement} />
    <LayoutRoute path="/admin/video" component={VideoManagement} />
    <Route path="/account/login/:next" component={Login} />
    <Route path="/account/denied" component={AccessDenied} />
  </React.Fragment>
);
