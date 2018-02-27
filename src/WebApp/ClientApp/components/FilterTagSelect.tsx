import * as React from 'react';
import * as Autosuggest from 'react-autosuggest';
import { FilterItem } from '../store/Filter';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { capitalizeFirst } from '../reducers/helpers';

export interface TagWithValues {
  type: string;
  values: FilterItem[];
}

export interface FilterTagSelectProps {
  tagWithValues: TagWithValues[];
  submitValue: (tagType: string, tagValue: string) => void;
  showIfActive?: boolean;
  doNotShowLabelIcon?: boolean;
  activePlaceholder: string;
  inactivePlaceholder: string;
  creatable?: boolean;
  highlightFirstSuggestion?: boolean;
  disabled: boolean;
}

export interface FilterTagSelectState {
  current: string;
  suggestions: TagWithValues[];
}

function renderSuggestion(suggestion: FilterItem, showIfActive: boolean) {
  if (showIfActive) {
    const { active } = suggestion;
    return <span>{suggestion.value}{active && <FontAwesomeIcon icon="check-circle" />}</span>;
  }
  return <span>{suggestion.value}</span>;
}

function escapeRegexCharacters(str: string) {
  return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function getSectionSuggestions(filter: TagWithValues): FilterItem[] {
  return filter.values;
}

export class FilterTagSelect extends React.Component<FilterTagSelectProps, FilterTagSelectState> {
  constructor(props: FilterTagSelectProps) {
    super(props);

    this.state = {
      current: '',
      suggestions: [],
    };

  }

  // Returns sections
  private getSuggestions(value: string): TagWithValues[] {
    const escapedValue = escapeRegexCharacters(value.trim());
    const { tagWithValues } = this.props;
    if (escapedValue === '') {
      return tagWithValues;
    }
    const regex = new RegExp(escapedValue, 'i');

    let suggestions = tagWithValues
      .map(filter => {
        let values = filter.values.filter(tag => regex.test(tag.value));
        // Add generic tag as suggestions if non existing

        if (filter.type.toLowerCase() === 'generic') {
          if (values.filter(v => v.value.toLowerCase() === value).length === 0) {
            let genericSuggestion = { type: filter.type, value: value.toLowerCase(), active: false } as FilterItem;
            values.push(genericSuggestion);
          }
        }
        return {
          type: filter.type,
          values
        };
      })
      .filter(filter => filter.values.length > 0);

    return suggestions;
  }

  private renderSectionTitle(filter: TagWithValues) {
    return (
      <strong>{capitalizeFirst(filter.type)}</strong>
    );
  }

  handleChange(event: any) {
    this.setState({
      current: event.target.value
    });
  }

  submit(filter: Autosuggest.SuggestionSelectedEventData<any>) {
    const suggestedFilter: FilterItem = filter.suggestion;
    this.props.submitValue(suggestedFilter.type, suggestedFilter.value);
    this.setState({ current: '' });
  }

  handleKeyPress = (event: any) => {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.setState({
        suggestions: this.getSuggestions('')
      });
    }
  }

  getSuggestionValue(suggestion: FilterItem) {
    return suggestion.value;
  }

  onSuggestionsFetchRequested(value: string) {
    this.setState({
      suggestions: this.getSuggestions(value)
    });
  }

  onSuggestionsClearRequested() {
    this.setState({
      suggestions: this.getSuggestions('')
    });
  }

  public render() {

    const {
      showIfActive,
      doNotShowLabelIcon,
      activePlaceholder,
      inactivePlaceholder,
      disabled,
      highlightFirstSuggestion,
    } = this.props;
    const { suggestions } = this.state;
    const placeholder: string = disabled ? inactivePlaceholder : activePlaceholder;

    const inputProps: Autosuggest.InputProps<any> = {
      type: 'text',
      value: this.state.current,
      onKeyPress: (e) => this.handleKeyPress(e),
      onChange: (e) => this.handleChange(e),
      placeholder,
    };

    return (
      <div
        className="filter-selector-item"
        style={disabled ? { pointerEvents: 'none', cursor: 'not-allowed' } : {}}
      >
        <form>
          <Autosuggest
            multiSection={true}
            shouldRenderSuggestions={() => true}
            suggestions={suggestions}
            onSuggestionsFetchRequested={(value) => this.onSuggestionsFetchRequested(value.value)}
            onSuggestionsClearRequested={() => this.onSuggestionsClearRequested()}
            getSuggestionValue={this.getSuggestionValue}
            renderSuggestion={(suggestion: FilterItem) => renderSuggestion(suggestion, showIfActive || false)}
            renderSectionTitle={(title) => this.renderSectionTitle(title)}
            getSectionSuggestions={getSectionSuggestions}
            inputProps={inputProps}
            highlightFirstSuggestion={highlightFirstSuggestion}
            focusInputOnSuggestionClick={false}
            onSuggestionSelected={
              (e, data) => {
                e.preventDefault();
                this.submit(data);
              }
            }
          />
          <button
            type="submit"
            value="Submit"
            style={{ marginRight: 10 }}
            onClick={(e) => e.preventDefault()}
          >
            {!doNotShowLabelIcon && <FontAwesomeIcon icon="tag" className="f-icon" />}
          </button>
        </form>
      </div>
    );
  }
}
