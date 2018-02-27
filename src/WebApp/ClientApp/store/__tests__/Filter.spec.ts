import { reducer, FilterState } from '../Filter';

const unloadedState: FilterState = {
  platforms: [],
  tags: [],
  playlists: [],
  personas: [],
  isFilterLoading: false,
  filters: [],
  showSelector: false,
  availableMetricsInfo: [],
  availableDashboardMetricsInfo: [],
};

it('Set metric info', () => {
  let expectedState = {
    platforms: [],
    tags: [],
    playlists: [],
    personas: [],
    isFilterLoading: false,
    filters: [],
    showSelector: false,
    availableMetricsInfo: [{ type: 'a', unit: 'b', unitSide: 'left' }],
    availableDashboardMetricsInfo: [],
  } as FilterState;
  let reducedState = reducer(unloadedState, {
    type: 'SET_METRIC_INFO',
    metricInfo: [{ type: 'a', unit: 'b', unitSide: 'left' }],
  });
  expect(reducedState).toEqual(expectedState);
});
