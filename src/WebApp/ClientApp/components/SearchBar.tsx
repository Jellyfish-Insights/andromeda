import * as React from 'react';
import { Video } from '../types';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as Autosuggest from 'react-autosuggest';
import * as Fuse from 'fuse.js';

export interface SearchBarFilterProps {
  videos: Video[];
  submitValue: (value: string) => void;
}

export interface SearchBarState {
  value: string;
  suggestions: VideoWithMatches[];
  matcher: Fuse;
}

export interface VideoWithMatches {
  item: Video;
  matches: Array<{ indices: Array<Array<number>>, value: string, key: string, arrayIndex: number }>;
}

function indexInRange(index: number, indices: number[][]): boolean {
  for (var i = 0; i < indices.length; i++) {
    let range = indices[i];
    let [b, e] = range;
    if (b <= index && index <= e) {
      return true;
    }
  }
  return false;
}

function renderSuggestion(suggestion: VideoWithMatches, info: { query: string, isHighlighted: boolean }) {
  let indices: Array<Array<number>> = [];
  if (info.query.length > 0) {
    const lowerCaseTitle = suggestion.item.title.toLowerCase();
    const queryWords = info.query.split(' ').map(x => x.toLowerCase());
    queryWords.forEach(queryWord => {
      let i = -1;
      while ((i = lowerCaseTitle.indexOf(queryWord, i + 1)) >= 0) {
        indices.push([i, i + queryWord.length - 1]);
      }
    });
  }
  const titleCharacters = suggestion.item.title.split('');
  const rendered = titleCharacters
    .map((character, index) => {
      const style: React.CSSProperties = indexInRange(index, indices) ? { fontWeight: 'bold' } : {};
      return <span key={index} style={style}>{character}</span>;
    });
  return (
    <span>
      {rendered}
    </span>
  );
}

let fuseOptions: Fuse.FuseOptions = {
  shouldSort: true,
  tokenize: true,
  includeMatches: true,
  threshold: 0.7,
  location: 0,
  distance: 100,
  maxPatternLength: 32,
  minMatchCharLength: 1,
  keys: ['title']
};
export class SearchBar extends React.Component<SearchBarFilterProps, SearchBarState> {
  constructor(props: SearchBarFilterProps) {
    super(props);
    let { videos } = props;
    this.state = {
      value: '',
      suggestions: [],
      matcher: new Fuse(videos, fuseOptions)
    };
  }

  componentDidUpdate(prevProps: SearchBarFilterProps) {
    let lastVideos = prevProps.videos;
    let { videos } = this.props;
    if (videos.length > lastVideos.length) {
      this.setState({ matcher: new Fuse(videos, fuseOptions) });
    }
  }

  handleChange(event: any) {
    this.setState({ value: event.target.value });
  }

  submit(value: string) {
    this.props.submitValue(value);
    this.setState({ value: '' });
  }

  handleSubmit = (event: any) => {
    this.submit(this.state.value);
    event.preventDefault();
  }

  handleKeyPress = (event: any) => {
    if (event.key === 'Enter') {
      this.handleSubmit(event);
    }
  }

  // Teach Autosuggest how to calculate suggestions for any given input value.
  getSuggestions(value: string): VideoWithMatches[] {
    const inputValue = value.trim().toLowerCase();
    const inputLength = inputValue.length;
    const { matcher } = this.state;
    let suggestions: VideoWithMatches[] = [];
    if (inputLength === 0) {
      let { videos } = this.props;
      videos = videos.map((v: Video): Video => {
        return { ...v, publishedAt: new Date(v.publishedAt) };
      });

      suggestions = videos
        .filter(v => v.title !== null)
        .sort((v1, v2) => {
          let date1 = v1.publishedAt;
          let date2 = v2.publishedAt;
          let returnValue = date1 < date2 ? -1 : (date1 > date2 ? +1 : 0);
          return -returnValue;
        })
        .slice(0, 10)
        .map((v: Video): VideoWithMatches => {
          return { item: v, matches: [] };
        });
    } else {
      suggestions = matcher.search(value);
    }
    // limit the number of suggestions by slicing
    return suggestions.slice(0, 10);
  }

  getSuggestionValue(suggestion: VideoWithMatches) {
    return suggestion.item.title;
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

    let { suggestions } = this.state;

    let inputProps: Autosuggest.InputProps<VideoWithMatches> = {
      type: 'text',
      value: this.state.value,
      onKeyPress: (e) => this.handleKeyPress(e),
      onChange: (e) => this.handleChange(e),
      placeholder: 'Title search',
    };

    return (
      <div className="search-bar">
        <div className="search-bar-container">
          <form>
            <Autosuggest
              suggestions={suggestions}
              shouldRenderSuggestions={() => true}
              onSuggestionsFetchRequested={(value) => this.onSuggestionsFetchRequested(value.value)}
              onSuggestionsClearRequested={() => this.onSuggestionsClearRequested()}
              getSuggestionValue={this.getSuggestionValue}
              renderSuggestion={renderSuggestion}
              inputProps={inputProps}
              onSuggestionSelected={(_, data) => this.submit(data.suggestionValue)}
            />
            <button
              type="submit"
              value="Submit"
              onClick={(e) => this.handleSubmit(e)}
            >
              <FontAwesomeIcon icon="search" className="f-icon" />
            </button>
          </form>
        </div>
      </div>
    );
  }
}
