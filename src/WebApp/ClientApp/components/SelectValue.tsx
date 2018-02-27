import * as React from 'react';
import {
  FilterItemWithColor,
  SelectorType
} from '../store/Chart';
import { SelectOptionType } from '../types/common';
import * as Select from 'react-select';

export interface SelectValueProps {
  type: SelectorType;
  filters: FilterItemWithColor[];
  handleOnChange: (value: string, active: boolean) => void;
}

export class SelectValue extends React.Component<SelectValueProps, {}> {

  private GetInactiveForType(type: string, filters: FilterItemWithColor[]): SelectOptionType[] {
    return filters
      .filter((x: FilterItemWithColor) => x.type === type && !x.active)
      .map((x: FilterItemWithColor): SelectOptionType => {
        return { value: x.value, label: x.value };
      });
  }

  public render() {
    let { type, filters, handleOnChange } = this.props;
    let options = this.GetInactiveForType(type, filters);

    return (
      <div className="select wrap">
        <Select.default
          options={options}
          placeholder={'Select your ' + type}
          onChange={(value) => handleOnChange((value as SelectOptionType).value, true)}
        />
      </div>
    );
  }
}
