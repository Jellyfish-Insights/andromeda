import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as React from 'react';
import { connect } from 'react-redux';
import { Redirect, Prompt } from 'react-router';
import { authenticatedFetch } from '../../ConnectionUtil';
import { RouteComponentProps } from 'react-router-dom';
import { ComponentIds } from '../../routes';
import { ApplicationState } from '../../store';
import * as LoginDataStore from '../../store/LoginData';
import * as NavMenuStore from '../../store/NavMenu';
import * as _ from 'underscore';
import {
  AllAccountsInfo,
  YearApPermissionLevels,
  AccountEdit,
  EditType
} from '../../types';
import { Loading } from '../Loading';

type UserManagementProps =
  NavMenuStore.NavMenuState &
  typeof NavMenuStore.actionCreators &
  LoginDataStore.LoginDataState &
  typeof LoginDataStore.actionCreators &
  RouteComponentProps<{}>;

type Validity = 'valid' | 'invalid' | 'empty';

type FrontUserEntry = {
  email: string;
  role: YearApPermissionLevels;
  lastLogIn?: Date;
  registrationDate?: Date;
  // `lastUpdate` is used for implementing optimistic locking.
  // Updates only succeed if `lastUpdate` matches the database version of the entry being updated.
  // Do not modify the value returned from server, otherwise updates will fail.
  lastUpdate?: Date;
};

type UserManagementState = {
  newUserForm: { email: string };
  formValid: Validity;
  disablePage: boolean;
  usersList?: FrontUserEntry[];
  usersListOriginal?: FrontUserEntry[];
  errorMessage?: string;
};

const confirmationMessage = 'There are unsaved changes, are you sure?';

function ConvertUtcTimeStringToLocal(date: string | Date): Date {
  if (!date) {
    return null;
  }
  return new Date(date + 'Z');
}

export class PersonaManagementInternal extends React.Component<
  UserManagementProps,
  UserManagementState
  > {

  constructor(props: UserManagementProps) {
    super(props);
    this.onUnload = this.onUnload.bind(this);
    this.state = {
      newUserForm: { email: '' },
      formValid: 'empty',
      disablePage: true,
    };
    this.props.select(ComponentIds.UserManagement);
  }

  onUnload(event: BeforeUnloadEvent) {
    if (!this.disableButtons()) {
      event.returnValue = confirmationMessage;
    }
  }

  componentDidMount() {
    window.addEventListener('beforeunload', this.onUnload);
    this.props.setLoggedAction();
    this.fetchUsers();
  }

  componentWillUnmount() {
    window.removeEventListener('beforeunload', this.onUnload);
  }

  fetchUsers() {
    return authenticatedFetch('/api/Account/ListAllAccountRoles')
      .then(res => res.json() as Promise<AllAccountsInfo>)
      .then((res: AllAccountsInfo) => this.setState(prevState => {
        let usersList: FrontUserEntry[] = _.keys(res.accounts).map(k => {
          // Dates in the server are stored without timezone information,
          // and when this information is used in new Date(), it is considered to be the
          // local timezone; as the server only uses UTC data, we can safely ignore the
          // added timezone part, which is what is being done below
          const lastLogin = ConvertUtcTimeStringToLocal(res.accounts[k].lastLogIn);
          const registrationDate = ConvertUtcTimeStringToLocal(res.accounts[k].registrationDate);
          return {
            email: k,
            role: res.accounts[k].role,
            lastLogIn: lastLogin,
            lastUpdate: res.accounts[k].lastUpdate,
            registrationDate: registrationDate,
          };
        });
        return {
          ...prevState,
          usersList: [...usersList].sort((x, y) => this.orderEmails(x, y)),
          usersListOriginal: [...usersList].sort((x, y) => this.orderEmails(x, y)),
          disablePage: false,
        };
      }));
  }

  orderEmails(x: FrontUserEntry, y: FrontUserEntry) {
    if (x.email === y.email) {
      return 0;
    } else if (x.email < y.email) {
      return -1;
    }
    return 1;
  }

  isUserAdmin() {
    return this.props.userrole === YearApPermissionLevels.Admin;
  }

  disableButtons() {
    return (this.state.usersList.length === this.state.usersListOriginal.length &&
      this.state.usersList.every(
        (v, i) => v.email === this.state.usersListOriginal[i].email &&
          v.role === this.state.usersListOriginal[i].role));
  }

  public render() {
    if (!this.props.logged) { return <Redirect to="/account/login/admin|user" />; }
    if (this.state.errorMessage) {
      return (
        <div className="loading">
          {this.state.errorMessage}
        </div>
      );
    }
    if (!this.state.usersList || !this.state.usersListOriginal || this.state.disablePage) {
      return <Loading externalHeight={600} height={100} width={100} />;
    }
    // the following line is the recommended idiom to get the keys of a enum:
    // https://github.com/Microsoft/TypeScript/issues/17198
    const permissionKeys = _.keys(YearApPermissionLevels)
      .filter((x) => typeof YearApPermissionLevels[x as any] === 'number');
    const permissionOptions = permissionKeys.map(k => (
      <option key={k} value={YearApPermissionLevels[k as any]}>{k}</option>
    ));

    const completeUserList = _.uniq(
      this.state.usersList.concat(this.state.usersListOriginal),
      x => x.email
    ).sort((x, y) => this.orderEmails(x, y));

    const userListComponent = completeUserList.map((user) => {
      const original = this.state.usersListOriginal.find(x => x.email === user.email);
      const current = this.state.usersList.find(x => x.email === user.email);
      const changed = current && original && current.role !== original.role ?
        'user-changed' : '';

      const dateOptions: Intl.DateTimeFormatOptions = {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      };
      return (
        <tr key={user.email} className={'user-row' + (!current ? ' strike' : '')}>
          <td className={!original ? 'user-changed' : ''}>{user.email}</td>
          <td>
            {user.registrationDate ? user.registrationDate.toLocaleDateString('en-US', dateOptions) : ''}
          </td>
          <td>{user.lastLogIn ? user.lastLogIn.toLocaleDateString('en-US', dateOptions) : ''}</td>
          <td>
            <select
              className={changed}
              value={user.role}
              onChange={evt => this.handleChangePermission(evt, user.email)}
              disabled={!this.isUserAdmin()}
            >
              {permissionOptions}
            </select>
          </td>
          <td>
            {this.isUserAdmin() &&
              <a onClick={() => this.handleRemove(user.email)}>
                <FontAwesomeIcon icon="times" className="f-icon" />
              </a>
            }
          </td>

        </tr>
      );
    });

    const disableButtons = this.disableButtons();

    return (
      <div className="page-container grid-x">
        <Prompt when={!this.disableButtons()} message={confirmationMessage} />
        <h1 className="cell large-offset-2 large-8 header">User management</h1>
        <div className="cell large-offset-2 large-8 content">
          {this.isUserAdmin() &&
            <form className="user-form grid-x grid-margin-x" onSubmit={evt => this.handleAddUser(evt)}>
              <input
                className={'cell large-4' + (this.state.formValid === 'invalid' ? ' invalid' : '')}
                placeholder="Type an email of the new member"
                type="email"
                name="Email"
                value={this.state.newUserForm.email}
                onChange={evt => this.handleEmailInput(evt)}
              />
              <input
                className="cell large-1 button update"
                value="+"
                type="submit"
                disabled={this.state.formValid === 'invalid' ||
                  this.state.formValid === 'empty'}
              />
              <input
                className="cell large-offset-3 large-2 button cancel"
                value="Cancel"
                type="button"
                disabled={disableButtons}
                onClick={() => this.handleCancel()}
              />
              <input
                className="cell large-2 button update"
                value="Save changes"
                disabled={disableButtons}
                onClick={() => this.handleSave()}
                type="button"
              />
            </form>
          }
          <table>
            <thead>
              <tr>
                <th>Email</th>
                <th>Invitation Date</th>
                <th>Last Login Date</th>
                <th>Access Level</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {userListComponent}
            </tbody>
          </table>
        </div>
      </div>
    );
  }

  // Handling adding a new user on form submit because pressing enter to add
  // the user to the list is more intuitive than pressing enter to save changes
  handleAddUser(evt: React.FormEvent<HTMLFormElement>) {
    evt.preventDefault();
    if (this.state.formValid === 'valid') {
      this.setState(prevState => ({
        ...prevState,
        newUserForm: { email: '' },
        formValid: 'empty',
        usersList: prevState.usersList.concat([{
          ...prevState.newUserForm,
          role: YearApPermissionLevels.ReadOnly
        }]).sort((x, y) => this.orderEmails(x, y)),
      }));
    }
  }

  handleCancel() {
    this.setState(prevState => ({
      ...prevState,
      usersList: [...prevState.usersListOriginal],
    }));
  }

  handleSave() {
    this.setState({ disablePage: true });
    let requestBody: AccountEdit = {
      edits: {}
    };
    // Checking new users or permission changes
    this.state.usersList.forEach(user => {
      let existing = this.state.usersListOriginal.find(u => u.email === user.email);
      if (!existing) {
        requestBody.edits[user.email] = {
          flag: EditType.New,
          permission: user.role,
          versionStamp: new Date(),
        };
      } else if (existing.role !== user.role) {
        requestBody.edits[user.email] = {
          flag: EditType.Update,
          permission: user.role,
          versionStamp: existing.lastUpdate,
        };
      }
    });
    // Checking user removals
    this.state.usersListOriginal.forEach(user => {
      if (!this.state.usersList.some(u => u.email === user.email)) {
        requestBody.edits[user.email] = {
          flag: EditType.Delete,
          permission: user.role,
          versionStamp: user.lastUpdate,
        };
      }
    });
    authenticatedFetch('/api/Account/EditAccount', {
      method: 'POST',
      body: JSON.stringify(requestBody),
      headers: {
        'Content-Type': 'application/json',
      }
    })
      .then(() => this.fetchUsers())
      .catch(x => {
        this.setState({
          errorMessage: 'Server failed executing your request, please check your connection: ' + JSON.stringify(x)
        });
      });
  }

  handleChangePermission(evt: React.ChangeEvent<HTMLSelectElement>, email: string) {
    const index = this.state.usersList.findIndex(x => x.email === email);
    // I need to parseInt because evt.target.value comes as a string
    // But I want a number.
    // I also need to save it to a variable because setState(prevState => ...)
    // cannot capture evt.target.value
    const value = parseInt(evt.target.value, 10);
    this.setState(prevState => ({
      ...prevState,
      usersList: Object.assign(
        [...prevState.usersList],
        {
          [index]: {
            ...prevState.usersList[index],
            role: value
          }
        }
      )
    }));
  }

  handleRemove(email: string) {
    const index = this.state.usersList.findIndex(x => x.email === email);
    if (index >= 0) {
      this.setState(prevState => ({
        ...prevState,
        usersList: [
          ...prevState.usersList.slice(0, index),
          ...prevState.usersList.slice(index + 1)
        ]
      }));
    } else {
      this.setState(prevState => {
        const original = prevState.usersListOriginal.find(x => x.email === email);
        return {
          ...prevState,
          usersList: prevState.usersList.concat([{
            ...original
          }]).sort((x, y) => this.orderEmails(x, y))
        };
      });
    }
  }

  handleEmailInput(evt: React.ChangeEvent<HTMLInputElement>) {
    let validity: Validity = 'valid';
    if (evt.target.value === '') {
      validity = 'empty';
    } else if (!evt.target.validity.valid ||
      this.state.usersList.some(x => x.email === evt.target.value) ||
      this.state.usersListOriginal.some(x => x.email === evt.target.value)) {
      validity = 'invalid';
    }
    this.setState({
      formValid: validity,
      newUserForm: { email: evt.target.value }
    });
  }
}

const UserManagement = connect(
  (state: ApplicationState) => ({
    ...state.navMenu,
    ...state.loginData,
  }),
  {
    ...NavMenuStore.actionCreators,
    ...LoginDataStore.actionCreators,
  }
)(PersonaManagementInternal);

export default UserManagement;
