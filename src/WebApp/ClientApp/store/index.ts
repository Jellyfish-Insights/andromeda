import * as NavMenu from './NavMenu';
import * as Marketing from './Data';
import * as Filter from './Filter';
import * as DateRange from './DateRange';
import * as Chart from './Chart';
import * as LoginData from './LoginData';

// The top-level state object
export interface ApplicationState {
  navMenu: NavMenu.NavMenuState;
  data: Marketing.DataState;
  filter: Filter.FilterState;
  dateRange: DateRange.DateRangeState;
  chart: Chart.ChartDataState;
  loginData: LoginData.LoginDataState;
}

// Whenever an action is dispatched, Redux will update each top-level application state property using
// the reducer with the matching name. It's important that the names match exactly, and that the reducer
// acts on the corresponding ApplicationState property type.
export const reducers = {
  navMenu: NavMenu.reducer,
  data: Marketing.reducer,
  filter: Filter.reducer,
  dateRange: DateRange.reducer,
  chart: Chart.reducer,
  loginData: LoginData.reducer,
};

// This type can be used as a hint on action creators so that its 'dispatch' and 'getState' params are
// correctly typed to match your store.
export interface AppThunkAction<TAction> {
  (dispatch: (action: TAction) => void, getState: () => ApplicationState): void;
}
