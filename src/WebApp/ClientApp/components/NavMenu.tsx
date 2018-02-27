import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as React from 'react';
import { connect } from 'react-redux';
import { NavLink } from 'react-router-dom';
import { ApplicationState } from '../store';
import * as LoginDataStore from '../store/LoginData';
import * as NavMenuStore from '../store/NavMenu';
import { menuItems } from '../store/NavMenu';
import { FeeLogo } from '../assets/img/FeeLogo';
import * as $ from 'jquery';
import { ComponentIds } from '../routes';

let Logo = () => {
  return (
    <div className="logo">
      <NavLink exact={true} to={'/'} activeClassName="active">
        <FeeLogo />
      </NavLink>
    </div>
  );
};

export function isAdminPage(id: ComponentIds): boolean {
  return id === ComponentIds.PersonaManagement
    || id === ComponentIds.TagManagement
    || id === ComponentIds.UserManagement
    || id === ComponentIds.VideoManagement;
}

export function setTitle(title: string) {
  if (title !== '') {
    document.title = title + ' - YEAR Analytics Platform';
  }
}

interface MenuProps {
  readonly type: string;
  readonly message: string;
  readonly icon?: IconProp;
  readonly path?: string;
  onClick?: () => void;
  readonly inner?: JSX.Element;
  readonly dataToggle?: string;
  readonly style?: React.CSSProperties;
}

class MenuItem extends React.Component<MenuProps, {}> {
  public render() {
    let { type, message, icon, path, inner, dataToggle, style } = this.props;
    let fwIcon: JSX.Element | string;
    if (icon) {
      fwIcon = <FontAwesomeIcon icon={icon} className="f-icon" />;
    } else {
      fwIcon = '';
    }

    return (
      <div
        className={type + ' button dropdown-toggle'}
        onClick={this.props.onClick}
        data-toggle={dataToggle}
        style={style}
      >
        {path && <NavLink to={path} className="button" >
          <span>{message}</span>{fwIcon}
        </NavLink>}
        {!path && <span>{message}</span>}
        {!path && fwIcon}
        {inner}
      </div>
    );
  }
}

export const VerticalLine = () => <div className="vertical-line" />;

type NavMenuProps =
  NavMenuStore.NavMenuState
  & typeof NavMenuStore.actionCreators
  & LoginDataStore.LoginDataState
  & typeof LoginDataStore.actionCreators;

class NavMenuInternal extends React.Component<NavMenuProps, {}> {

  constructor(props: NavMenuProps) {
    super(props);
  }

  componentWillMount() {
    this.props.setLoggedAction();
  }

  componentDidMount() {
    // This line is actually required for the dropdown to work
    if ($('.admin-dropdown').length > 0) {
      // tslint:disable-next-line:no-unused-expression
      new Foundation.Dropdown($('.admin-dropdown'), { hover: true, hoverDelay: 0, hoverPane: true });
    }
  }

  componentDidUpdate() {
    var title = menuItems.filter(element => {
      return element.id === this.props.activeMenuItem;
    });
    if (title.length > 0) {
      document.title = (title[0].message) + ' - YEAR Analytics Platform';
    }
  }

  public render() {
    const { activeMenuItem } = this.props;
    const isAdminActive = isAdminPage(activeMenuItem);

    const adminOptionValues = [
      [ComponentIds.UserManagement, 'User Management', '/admin/user'],
      [ComponentIds.TagManagement, 'Tag Management', '/admin/tag'],
      [ComponentIds.PersonaManagement, 'Persona Management', '/admin/persona'],
      [ComponentIds.VideoManagement, 'Video Management', '/admin/video'],
    ];

    const adminOptions = (
      <div id="admin-dropdown" className="admin-dropdown" data-options="align:down">
        <ul className="admin-menu">
          {
            adminOptionValues.map((optionValue, index) => {
              const [adminType, adminString, path] = optionValue;
              const classString = 'button' + (activeMenuItem === adminType ? ' active' : '');
              if (activeMenuItem === adminType) {
                setTitle(adminString.toString());
              }

              return <NavLink to={path as string} className={classString} key={index}>
                {adminString}
              </NavLink>;
            })
          }
        </ul>
      </div>
    );

    return (
      <div className="nav-container">
        <div className="site-header">
          <div className="left-container">
            <Logo />
            <div className="project-title">
              year ap
            </div>
          </div>
          <VerticalLine />
          {
            this.props.logged ?
              <div className="menu main-menu">
                {menuItems.map((menuItem) =>
                  <MenuItem
                    type={'main-menu-item' + ((menuItem.id === activeMenuItem) ? ' active' : '')}
                    message={menuItem.message}
                    path={menuItem.path}
                    key={menuItem.id}
                  />
                )}
              </div> :
              <div />
          }
          <VerticalLine />
          {
            this.props.logged ?
              <div className="menu user-menu">
                <MenuItem type="user-menu-item" message={'Hello, ' + this.props.username} icon="user-circle" path="/" />
                <MenuItem
                  type={'user-menu-item' + (isAdminActive ? ' active' : '')}
                  message="Admin"
                  icon="cog"
                  path="/admin/user"
                  inner={adminOptions}
                  dataToggle="admin-dropdown"
                  style={isAdminActive ? { color: '#eb4032' } : {}}
                />
                <MenuItem
                  type="user-menu-item"
                  message="Logout"
                  icon="power-off"
                  onClick={() => this.props.doLogoutAction()}
                />
              </div> :
              <div />
          }
        </div>
      </div>
    );
  }
}

export const NavMenu = connect(
  (state: ApplicationState) => {
    return {
      ...state.navMenu,
      ...state.loginData
    };
  },
  {
    ...NavMenuStore.actionCreators,
    ...LoginDataStore.actionCreators
  }
)(NavMenuInternal);
