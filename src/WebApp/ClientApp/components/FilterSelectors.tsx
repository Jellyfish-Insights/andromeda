import * as React from 'react';
import { FilterItem } from '../store/Filter';
import { SearchBar } from './SearchBar';
import * as _ from 'underscore';
import { Video } from '../types';
import { FilterTagSelect } from './FilterTagSelect';
import { TagWithValues } from './FilterTagSelect';

interface FilterSelectorsProps {
  filters: FilterItem[];
  setFilter: (type: string, value: string, active: boolean) => void;
  submitSearchBarValue: (value: string) => void;
  videos: Video[];
}

export class FilterSelectors extends React.Component<FilterSelectorsProps, {}> {

  public render() {
    const { filters, submitSearchBarValue, videos, setFilter } = this.props;

    if (filters.length === 0) { return null; }

    const componentClasses = ['filter-selector', 'wrap'];
    return (
      <div className={componentClasses.join(' ')}>
        <SearchBar submitValue={submitSearchBarValue} videos={videos} />
        <FilterTagSelect
          tagWithValues={convertFiltersToTaggedDictionary(filters)}
          submitValue={(type, value) => setFilter(type, value, true)}
          activePlaceholder="Tag and Playlist search"
          inactivePlaceholder=""
          disabled={false}
        />
      </div>
    );
  }
}

function convertFiltersToTaggedDictionary(filters: FilterItem[]): TagWithValues[] {
  const types = _.uniq(filters.map(f => f.type)).filter(x => x.toLowerCase() !== 'search');
  const tagWithValues = types.map(
    type => {
      return {
        type,
        values: filters.filter(f => f.type === type && !f.active),
      };
    }
  ).filter(tagWithValue => tagWithValue.values.length > 0);
  return tagWithValues;
}
