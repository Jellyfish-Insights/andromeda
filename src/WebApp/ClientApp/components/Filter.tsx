import * as React from 'react';
import { Tag, Video } from '../types';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { FilterItem } from '../store/Filter';
import { FilterSelectors } from './FilterSelectors';
import { SetFilterAction } from '../store/Filter';

export interface FilterProps {
  platforms: string[];
  tags: Tag[];
  personas: string[];
  playlists: string[];
  showSelector: boolean;
  setFilter: (tagValue: string, tagType: string, active: boolean) => SetFilterAction;
  filters: FilterItem[];
  submitSearchBarValue: (value: string) => void;
  videos: Video[];
  filterRef: React.RefObject<HTMLDivElement>;
}

export class Filter extends React.Component<FilterProps, {}> {
  constructor(props: FilterProps) {
    super(props);
  }

  public render() {
    const { videos, filters, setFilter, submitSearchBarValue, filterRef } = this.props;

    const activeFilters = filters.filter(x => x.active).length;

    return (
      <div className="filter-super-container" id="scroll-position" ref={filterRef}>
        <div className="filter-container">
          {activeFilters > 0 &&
            <div className="filter">
              <div className="active-filters">
                {
                  filters
                    .filter(x => x.active)
                    .map((x, index) => {
                      return (
                        <button
                          key={index}
                          className={x.type === 'search' ? 'search' : ''}
                          onClick={(e) => {
                            setFilter(x.type, x.value, false);
                            e.stopPropagation();
                          }
                          }
                        >
                          {x.value}<FontAwesomeIcon icon="times" className="f-icon" />
                        </button>
                      );
                    })
                }
              </div>
            </div>
          }
          <div className="filter">
            <div className="filter-title">Filters</div>
            <FilterSelectors
              videos={videos}
              filters={filters}
              setFilter={setFilter}
              submitSearchBarValue={submitSearchBarValue}
            />
          </div>
        </div>
      </div>
    );
  }
}
