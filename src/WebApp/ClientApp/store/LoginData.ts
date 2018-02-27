import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from '.';
import { AuthStateInfo, YearApPermissionLevels } from '../types';
import { authenticatedFetch } from '../ConnectionUtil';

type AuthViewModel = AuthStateInfo & {
  succeeded: boolean;
};

export interface LoginDataState {
  isLoading: boolean;
  logged: boolean | null;
  username: string;
  userrole: YearApPermissionLevels;
}

interface ReceiveLoginDataAction {
  type: 'RECEIVE_LOGIN_DATA';
  data: AuthViewModel;
}

interface SendLogoutAction {
  type: 'SEND_LOGOUT';
}

type KnownAction = ReceiveLoginDataAction | SendLogoutAction;

export const actionCreators = {
  doLogoutAction: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    let fetchTask = authenticatedFetch('api/Account/Logout', {
      method: 'POST',
    }).then(response => {
      dispatch({ type: 'SEND_LOGOUT' });
    });
  },
  setLoggedAction: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    let fetchTask = authenticatedFetch('api/Account/AuthState')
      .then(response => {
        if (response.status === 401) {
          dispatch({ type: 'SEND_LOGOUT' });
        } else {
          response.json().then((info: AuthStateInfo) => {
            let match = info.name.match(/^(.+)@.+$/);
            let username = match == null ? info.name : match[1];
            dispatch({
              type: 'RECEIVE_LOGIN_DATA', data: {
                ...info,
                succeeded: response.ok,
                name: username,
              }
            });
          });
        }
      });
  }
};

const unloadedState: LoginDataState = {
  isLoading: false,
  logged: null,
  username: '',
  userrole: YearApPermissionLevels.ReadOnly,
};

export const reducer: Reducer<LoginDataState> = (state: LoginDataState, incomingAction: Action) => {
  const action = incomingAction as KnownAction;
  switch (action.type) {
    case 'RECEIVE_LOGIN_DATA':
      return { ...state, logged: action.data.succeeded, username: action.data.name, userrole: action.data.role };
    case 'SEND_LOGOUT':
      return { ...state, logged: false, username: '', userrole: null };
  }

  return state || unloadedState;
};
