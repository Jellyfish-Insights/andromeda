import * as React from "react";
import * as Select from "react-select";
import * as _ from "underscore";
import { capitalizeFirst } from "../reducers/helpers";
import { SelectorType } from "../store/Chart";
import { FilterItem } from "../store/Filter";
import { SelectOptionType } from "../types/common";

interface SelectTypeProps {
  currentType: SelectorType;
  handleOnChange: Function;
  filters: FilterItem[];
}

interface SelectTypeState {
  options: Array<SelectOptionType>;
}

export class SelectType extends React.Component<
  SelectTypeProps,
  SelectTypeState
  > {
  constructor(props: SelectTypeProps) {
    super(props);
    const { filters } = props;
    const lowerCaseFilters = filters.map((f: FilterItem) =>
      f.type.toLowerCase()
    );
    let tagTypes = _.uniq(lowerCaseFilters)
      .filter(x => x !== "playlist")
      .filter(x => x !== "platform");
    let options = tagTypes.map((filter: string) => {
      return {
        value: filter,
        label: capitalizeFirst(filter)
      };
    });
    this.state = {
      options
    };
  }

  public render() {
    let { currentType, handleOnChange } = this.props;

    return (
      <div className="select wrap">
        <Select.default
          options={this.state.options}
          placeholder={"Select your tag type"}
          simpleValue={true}
          clearable={false}
          value={currentType}
          onChange={value => handleOnChange(value)}
        />
      </div>
    );
  }
}
