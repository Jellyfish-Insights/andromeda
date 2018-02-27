import { ComponentIds } from '../routes';
import { Reducer, Action } from 'redux';

export let menuItems: MenuItemData[] = [
  { id: ComponentIds.Dashboard, message: 'Dashboard', path: '/dashboard' },
  { id: ComponentIds.Content, message: 'Content', path: '/content' },
  { id: ComponentIds.Marketing, message: 'Audiences', path: '/audiences' },
  { id: ComponentIds.Webviews, message: 'Webviews', path: '/webviews' }
];
export interface MenuItemData {
  id: ComponentIds;
  message: string;
  path: string;
}

// Component State
export interface NavMenuState {
  activeMenuItem: ComponentIds;
}

// Action Creators
interface SelectAction { type: 'SELECT_ACTION'; payload: number; }
export type NavMenuAction = SelectAction;

export const actionCreators = {
  select: (nextMenuItem: number) => <SelectAction> { type: 'SELECT_ACTION', payload: nextMenuItem },
};

// Action types

// Reducer
export const reducer: Reducer<NavMenuState> = (state: NavMenuState, incomingAction: Action) => {
  const action = incomingAction as NavMenuAction;
  switch (action.type) {
    case 'SELECT_ACTION':
      return { activeMenuItem: action.payload };
  }
  return state || { activeMenuItem: 0 };
};
