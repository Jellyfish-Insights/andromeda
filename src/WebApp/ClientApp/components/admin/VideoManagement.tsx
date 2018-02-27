import * as React from 'react';
import { connect } from 'react-redux';
import { Redirect } from 'react-router';
import { RouteComponentProps } from 'react-router-dom';
import { ComponentIds } from '../../routes';
import { ApplicationState } from '../../store';
import * as MarketingDataStore from '../../store/Data';
import * as DateRangeStore from '../../store/DateRange';
import * as FilterStore from '../../store/Filter';
import * as LoginDataStore from '../../store/LoginData';
import * as NavMenuStore from '../../store/NavMenu';
import { Filter } from '../Filter';
import { stringfyFilters } from '../../reducers/helpers';
import { VideoManagementEdit } from './VideoManagementEdit';
import { SourceObject, SourceObjectType, YearApPermissionLevels } from '../../types';
import { authenticatedFetch } from '../../ConnectionUtil';
import { ScrollWhileDrag } from '../VideoItem';
import * as _ from 'underscore';

type VideoManagementProps =
  MarketingDataStore.DataState &
  typeof MarketingDataStore.actionCreators &
  NavMenuStore.NavMenuState &
  typeof NavMenuStore.actionCreators &
  FilterStore.FilterState &
  typeof FilterStore.actionCreators &
  DateRangeStore.DateRangeState &
  typeof DateRangeStore.actionCreators &
  LoginDataStore.LoginDataState &
  typeof LoginDataStore.actionCreators &
  RouteComponentProps<{}>;

export type Unassociated = { [x: number]: SourceObject[] };
type VideoManagementState = {
  unassociated: Unassociated;
};

export class VideoManagementInternal extends React.Component<
  VideoManagementProps,
  VideoManagementState
  > {

  constructor(props: VideoManagementProps) {
    super(props);
    let { from, to, filters } = this.props;
    this.props.select(ComponentIds.VideoManagement);
    this.props.requestFilterData('content', true);
    this.props.requestData('content', stringfyFilters(filters), from, to, true);
    this.state = {
      unassociated: {},
    };
  }

  componentDidMount() {
    ScrollWhileDrag();
    if (!this.props.logged) {
      return;
    }
    this.getUnassociated(SourceObjectType.Video);
    this.getUnassociated(SourceObjectType.Campaign);
    this.getUnassociated(SourceObjectType.AdSet);
  }

  private getUnassociated(type: SourceObjectType) {
    let videoFetchTask = authenticatedFetch(
      `api/MarketingData/GetUnassociatedSources/${type}`,
      {
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      }
    )
      .then(response => response.json() as Promise<Array<SourceObject>>)
      .then(data => {
        this.setState((prevState) => {
          const prevUnassociated = prevState.unassociated;
          return {
            unassociated: { ...prevUnassociated, [type]: data }
          };
        });
      });
  }

  private filterRef = React.createRef<HTMLDivElement>();

  public render() {
    if (!this.props.logged) { return <Redirect to="/account/login/admin|video" />; }

    const {
      videos,
      metaTags,
      requestData,
      filters,
      from,
      to,
      setVideoManagementArchiveMode,
      videoManagementArchiveMode,
      updateVideoData
    } = this.props;
    const { unassociated } = this.state;
    const stringfiedFilters = stringfyFilters(filters);

    const numberOfUnassociatedTypes = _.keys(SourceObjectType).length / 2;
    if (_.keys(unassociated).length !== numberOfUnassociatedTypes) {
      return <div>Loading...</div>;
    }

    return (
      <div className="page-container video-management">
        <Filter
          videos={this.props.videos}
          platforms={this.props.platforms}
          tags={this.props.tags}
          playlists={this.props.playlists}
          personas={this.props.personas}
          showSelector={this.props.showSelector}
          setFilter={
            (tagValue: string, tagType: string, active: boolean) => this.props.setFilter(tagValue, tagType, active)
          }
          filters={this.props.filters}
          submitSearchBarValue={this.props.submitSearchBarValue}
          filterRef={this.filterRef}
        />

        <VideoManagementEdit
          canEdit={[YearApPermissionLevels.Admin, YearApPermissionLevels.Editor].includes(this.props.userrole)}
          videos={videos}
          metaTags={metaTags}
          updateVideoData={updateVideoData}
          requestData={() => requestData('content', stringfiedFilters, from, to, true)}
          unassociated={unassociated}
          setArchiveMode={setVideoManagementArchiveMode}
          archiveMode={videoManagementArchiveMode}
          filterHeight={this.filterRef.current && this.filterRef.current.clientHeight || 0}
        />
      </div>
    );
  }
}

const VideoManagement = connect(
  (state: ApplicationState) => ({
    ...state.navMenu,
    ...state.data,
    ...state.filter,
    ...state.loginData,
    ...state.dateRange,
  }),
  {
    ...NavMenuStore.actionCreators,
    ...MarketingDataStore.actionCreators,
    ...FilterStore.actionCreators,
    ...DateRangeStore.actionCreators,
    ...LoginDataStore.actionCreators,
  }
)(VideoManagementInternal);

export default VideoManagement;
