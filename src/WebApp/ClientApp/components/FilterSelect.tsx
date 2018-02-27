import * as React from 'react';
import * as Select from 'react-select';
import { FilterItem } from '../store/Filter';
import { SelectOptionType } from '../types/common';

export interface FilterSelectProps {
  type: string;
  filters: FilterItem[];
  handleOnChange: (tagValue: SelectOptionType) => void;
}

const GetInactiveForType = (type: string, filters: FilterItem[]) => {
  return filters
    .filter((x: FilterItem) => x.type === type && !x.active)
    .map((x: FilterItem): SelectOptionType => {
      return { value: x.value, label: x.value };
    });
};

export const FilterSelect: React.SFC<FilterSelectProps> = (props: FilterSelectProps) => {
  let options = GetInactiveForType(props.type, props.filters);

  return (
    <div className="filter-selector-item">
      <span>{props.type.charAt(0).toUpperCase() + props.type.slice(1)}</span>
      <Select.default
        options={options}
        placeholder={'Select your ' + props.type}
        onChange={(value) => props.handleOnChange(value as SelectOptionType)}
      />
    </div>
  );
};
