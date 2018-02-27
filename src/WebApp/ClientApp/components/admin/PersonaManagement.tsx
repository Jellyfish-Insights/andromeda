import * as React from 'react';
import { prependZeros } from '../../reducers/helpers';
import { connect } from 'react-redux';
import { Redirect } from 'react-router';
import { RouteComponentProps } from 'react-router-dom';
import { ComponentIds } from '../../routes';
import { ApplicationState } from '../../store';
import * as LoginDataStore from '../../store/LoginData';
import * as NavMenuStore from '../../store/NavMenu';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Title } from '../Title';
import { SourceProps, SourceComponent, ScrollWhileDrag } from '../VideoItem';
import { Operation, Unassociations } from './VideoManagementEdit';
import {
  ArchiveMode,
  PersonaVersion,
  EditType,
  PersonaVersionEdits,
  SourceObject,
  SourceObjectType,
  YearApPermissionLevels
} from '../../types';
import { authenticatedFetch } from '../../ConnectionUtil';
import * as Select from 'react-select';
import { CancelAndSaveButtons } from './VideoManagementEdit';
import {
  ConnectDropTarget,
  DropTarget,
  DropTargetMonitor,
  DragDropContext
} from 'react-dnd';
import HTML5Backend from 'react-dnd-html5-backend';
import * as _ from 'underscore';

type PersonaManagementProps =
  typeof NavMenuStore.actionCreators &
  LoginDataStore.LoginDataState &
  typeof LoginDataStore.actionCreators &
  RouteComponentProps<{}>;

const archiveSelectOptions = [ArchiveMode.All, ArchiveMode.Archived, ArchiveMode.UnArchived]
  .map(value => { return { value, label: ArchiveMode[value] }; });

function normalizeEdits(inputEdits: PersonaVersionEdits): PersonaVersionEdits {
  let edits = { ...inputEdits };
  for (const k of _.keys(edits.edits)) {
    const ed = edits.edits[k];
    if (!(ed.archive !== null || ed.addedAdSets.length > 0 || ed.removedAdSets.length > 0)) {
      delete edits.edits[k];
    }
  }
  return edits;
}
function gatherAllAdsets(prev: Readonly<PersonaManagementState>): AdSets {
  const { personaList, unassociated } = prev;
  var newAdsets = {} as AdSets;
  [].concat(unassociated[SourceObjectType.AdSet])
    .concat(personaList.map(x => x.adSets).reduce((x, y) => x.concat(y), []))
    .map(x => newAdsets[x.sourceId] = x);
  return newAdsets;
}

function applyEdits(state: PersonaManagementState) {
  // This function operations needs to be completely imutable regarding the original state
  // Any changed object needs to be copied first
  const { personaList, personaEdits, unassociated, unassociatedEdit } = state;
  const adsets = gatherAllAdsets(state);
  const newPersonaList = [...personaList];
  const edits = personaEdits.edits;
  const unassoc = [...unassociated[SourceObjectType.AdSet]];

  for (var k of _.keys(edits)) {
    const personaIx = personaList.findIndex(x => x.id === k);
    const newPersona = { ...personaList[personaIx], };
    const newAdSets = [...newPersona.adSets];
    newPersona.adSets = newAdSets;
    const newEdit = edits[k];
    if (newEdit.archive !== null) {
      newPersona.archived = newEdit.archive;
    }
    if (newEdit.addedAdSets) {
      newEdit.addedAdSets.map(x => {
        const move = adsets[x];
        newAdSets.push(move);
      });
    }
    if (newEdit.removedAdSets) {
      newEdit.removedAdSets.map(x => {
        const move = adsets[x];
        const k = newAdSets.findIndex(y => y.sourceId === x);
        newAdSets.splice(k, 1);
      });
    }
    newPersonaList[personaIx] = newPersona;
  }
  unassociatedEdit.removed.map(x => {
    var k = unassoc.findIndex(y => y.sourceId === x);
    unassoc.splice(k, 1);
  });
  unassociatedEdit.added.map(x => {
    const move = adsets[x];
    unassoc.push(move);
  });
  return { personas: newPersonaList, adsets: { [SourceObjectType.AdSet]: unassoc } };
}
export type Unassociated = { [x: number]: SourceObject[] };
type AdSets = { [id: string]: SourceObject };
type PersonaManagementState = {
  archiveMode: ArchiveMode;
  itemSelection: string[],
  adsetPage: number,
  unassociated: Unassociated;
  unassociatedEdit: { added: string[], removed: string[] };
  personaList: PersonaVersion[];
  personaEdits: PersonaVersionEdits;
  search: string;
  saved: boolean;
};

@DragDropContext(HTML5Backend)
export class PersonaManagementInternal extends React.Component<
PersonaManagementProps,
PersonaManagementState
> {

  constructor(props: PersonaManagementProps) {
    super(props);
    this.props.select(ComponentIds.PersonaManagement);
    this.state = {
      archiveMode: ArchiveMode.UnArchived,
      personaEdits: { edits: {} },
      adsetPage: 0,
      itemSelection: [],
      unassociatedEdit: { added: [], removed: [] },
      personaList: [],
      unassociated: { [SourceObjectType.AdSet]: [] },
      search: '',
      saved: false,
    };
    this.state.unassociated[SourceObjectType.AdSet] = [];
  }

  canUserEdit() {
    return [YearApPermissionLevels.Admin, YearApPermissionLevels.Editor].includes(this.props.userrole);
  }

  componentWillMount() {
    ScrollWhileDrag()
    this.getPersonaList(this.state.archiveMode);
    this.getUnassociated();
  }

  private setArchiveMode(mode: ArchiveMode) {
    this.getPersonaList(mode);
  }

  private getPersonaList(archiveMode: ArchiveMode) {
    let videoFetchTask = authenticatedFetch(`api/Persona/GetPersonas/${archiveMode}`)
      .then(response => response.json() as Promise<Array<PersonaVersion>>)
      .then(data => {
        this.setState({
          archiveMode: archiveMode,
          personaList: data
        });
      });
  }

  private updatePage(increment: number) {
    this.setState(prev => ({ ...prev, adsetPage: prev.adsetPage + increment }));
  }

  private getUnassociated() {
    const type = SourceObjectType.AdSet;
    let videoFetchTask = authenticatedFetch(`api/Persona/GetUnAssociatedAdSets/`)
      .then(response => response.json() as Promise<Array<SourceObject>>)
      .then(data => {
        const unassociated: Unassociated = { [SourceObjectType.AdSet]: data };
        this.setState({ unassociated });
      });
  }

  private handleDrop(targetPersonaId: string, item: SourceProps) {
    if (targetPersonaId !== item.originId) {
      this.setState(prev => {
        const { sourceObject, originId } = item;
        const { personaList, personaEdits, unassociatedEdit } = prev;

        const targetPersona = personaList.find(x => x.id === targetPersonaId);
        const newEdit: PersonaVersionEdits = { ...personaEdits };
        const newUnassocEdit = unassociatedEdit;
        if (originId && originId !== '') {
          // Origin persona actions From persona to persona
          const originalPersona = personaList.find(x => x.id === targetPersonaId);
          var existingOrigin = newEdit.edits[originId];
          if (!existingOrigin) {
            existingOrigin = {
              updateDate: originalPersona.updateDate,
              flag: EditType.Update,
              addedAdSets: [], removedAdSets: []
            };
            newEdit.edits[originId] = existingOrigin;
            existingOrigin.removedAdSets.push(sourceObject.sourceId);
          } else {
            const created = existingOrigin.addedAdSets.findIndex(x => x === sourceObject.sourceId);
            if (created !== -1) {
              existingOrigin.addedAdSets.splice(created, 1);
            } else {
              existingOrigin.removedAdSets.push(sourceObject.sourceId);
            }
          }
        } else {
          // Unassociated action From unassoc to persona
          const created = newUnassocEdit.added.findIndex(x => x === sourceObject.sourceId);
          if (created !== -1) {
            newUnassocEdit.added.splice(created, 1);
          } else {
            newUnassocEdit.removed.push(sourceObject.sourceId);
          }
        }
        // Target persona actions
        var existingTarget = newEdit.edits[targetPersonaId];
        if (!existingTarget) {
          existingTarget = {
            updateDate: targetPersona.updateDate,
            flag: EditType.Update,
            addedAdSets: [],
            removedAdSets: []
          };
          newEdit.edits[targetPersonaId] = existingTarget;
          existingTarget.addedAdSets.push(sourceObject.sourceId);
        } else {
          const created = existingTarget.removedAdSets.findIndex(x => x === sourceObject.sourceId);
          if (created !== -1) {
            existingTarget.removedAdSets.splice(created, 1);
          } else {
            existingTarget.addedAdSets.push(sourceObject.sourceId);
          }
        }
        return { ...prev, personaEdits: normalizeEdits(newEdit), unassociatedEdit: newUnassocEdit };
      });
    }
  }

  handleArchive = (archive: boolean) => {
    this.setState(prev => {
      const { personaEdits, itemSelection, personaList } = prev;
      itemSelection.map(item => {
        const edit = personaEdits.edits[item];
        const originPersona = personaList.find(x => x.id === item);

        if (originPersona.archived !== archive) {
          if (edit) {
            edit.archive = archive;
          } else {
            personaEdits.edits[item] = {
              archive: archive,
              updateDate: originPersona.updateDate,
              flag: EditType.Update,
              addedAdSets: [],
              removedAdSets: []
            };
          }
        } else {
          if (edit) {
            delete edit.archive;
          }
        }

      }
      );
      return { ...prev, personaEdits: normalizeEdits(personaEdits) };
    });

  }
  handleSelectAll = (state: Boolean) => {
    this.setState(prev => {
      if (state) {
        return { ...prev, itemSelection: prev.personaList.map(x => x.id) };
      } else {
        return { ...prev, itemSelection: [] };
      }
    });

  }
  toggleSelected = (key: string, state: Boolean) => {
    this.setState(prev => {
      const { itemSelection } = prev;
      if (state) {
        itemSelection.push(key);
      } else {
        const ix = itemSelection.findIndex(x => x === key);
        itemSelection.splice(ix, 1);
      }
      return { ...prev, itemSelection: itemSelection };
    });
  }

  cancelEdit = () => {
    this.setState(prev => {
      const emptyEdits = { ...prev.personaEdits };
      emptyEdits.edits = {};
      const unassocEdit = { ...prev.unassociatedEdit };
      unassocEdit.added = [];
      unassocEdit.removed = [];
      return { ...prev, personaEdits: emptyEdits, unassociatedEdit: unassocEdit };
    });
  }

  editPersona = () => {
    authenticatedFetch(`api/Persona/EditPersonas`, {
      method: 'PUT',
      body: JSON.stringify(this.state.personaEdits),
      headers: {
        'Content-Type': 'application/json'
      }
    })
      .then(response => response.json() as Promise<PersonaVersionEdits>).then(result => {
        this.setState((prev) => {
          if (_.keys(result.edits).length !== 0) {
            alert('Some of your changes failed to be applied: ' + JSON.stringify(result));
            return { ...prev, unassociatedEdit: { added: [], removed: [] }, personaEdits: { edits: {} } };
          } else {
            const { personas, adsets } = applyEdits(prev);
            return {
              ...prev,
              personaList: personas,
              unassociated: adsets,
              unassociatedEdit: { added: [], removed: [] },
              personaEdits: { edits: {} },
              saved: true
            };
          }
        });
        setTimeout(
          () => this.setState({ saved: false }),
          3000
        );
        this.getPersonaList(this.state.archiveMode);
        this.getUnassociated();
      }).catch(x => {
        alert('Server failed executing your request, please check your connection: ' + JSON.stringify(x));
      }
      );
  }
  private handleDropOnUnassociated(item: SourceProps) {
    const { sourceObject, originId } = item;
    if (originId && originId !== '') {
      this.setState(prev => {
        const { personaList, personaEdits, unassociatedEdit } = prev;
        const newEdit = personaEdits;
        const newUnassocEdit = unassociatedEdit;

        // Origin persona actions
        const originPersona = personaList.find(x => x.id === originId);
        const originPersonaEdit = newEdit.edits[originId]
          || { updateDate: originPersona.updateDate, flag: EditType.Update, addedAdSets: [], removedAdSets: [] };
        const added = originPersonaEdit.addedAdSets;
        const removed = originPersonaEdit.removedAdSets;
        if (newEdit.edits[originId]) {
          const created = added.findIndex(x => x === sourceObject.sourceId);
          if (created !== -1) {
            added.splice(created, 1);
          } else {
            removed.push(sourceObject.sourceId);
          }
        } else {
          newEdit.edits[originId] = originPersonaEdit;
          removed.push(sourceObject.sourceId);
        }

        // Unasssociated adsets actions
        const created = newUnassocEdit.removed.findIndex(x => x === sourceObject.sourceId);
        if (created !== -1) {
          newUnassocEdit.removed.splice(created, 1);
        } else {
          newUnassocEdit.added.push(sourceObject.sourceId);
        }
        return { ...prev, personaEdits: normalizeEdits(newEdit), unassociatedEdit: newUnassocEdit };
      });
    }
  }

  public render() {
    if (!this.props.logged) { return <Redirect to="/account/login/admin|persona" />; }
    const canEdit = [YearApPermissionLevels.Admin, YearApPermissionLevels.Editor].includes(this.props.userrole);
    const { archiveMode, personaEdits, itemSelection, search, saved } = this.state;
    const associationsToShow = SourceObjectType.AdSet;
    const { personas, adsets } = applyEdits(this.state);
    const editDiry = _.keys(personaEdits.edits).length > 0;
    const PAGE_SIZE = 40 as number;
    const page = this.state.adsetPage;
    const updateButtons = (
      <CancelAndSaveButtons
        show={canEdit}
        enabled={editDiry || saved}
        saved={saved}
        cancel={() => { this.cancelEdit(); }}
        update={() => { this.editPersona(); }}
      />);
    let filteredUnassociatedAdSets = adsets[SourceObjectType.AdSet]
      .sort((x, y) => x.title.localeCompare(y.title));
    if (search.length > 0) {
      filteredUnassociatedAdSets = filteredUnassociatedAdSets
        .filter(x => x.title.toLowerCase().search(search.toLowerCase()) >= 0);
    }
    const adsetSize = filteredUnassociatedAdSets.length;

    const log10 = Math.trunc(Math.log10(adsetSize));
    const lastInPage = Math.min((page + 1) * PAGE_SIZE, adsetSize);
    const lastPage = Math.ceil(adsetSize / PAGE_SIZE - 1);
    const firstPage = page * PAGE_SIZE + 1;

    return (
      <div className="page-container persona-management">
        <Title
          title="Persona Management"
          extra={updateButtons}
        />
        <div className="persona-page-body">
          <div className="admin-persona-container">
            <div className="persona-operations" id="scroll-position">
              {this.canUserEdit() &&
                <React.Fragment>
                  <input
                    name="Selected"
                    type="checkbox"
                    className="select-all-checkbox"
                    checked={itemSelection.length === personas.length && itemSelection.length !== 0}
                    value=""
                    onChange={e => this.handleSelectAll(e.target.checked)}
                  />
                  <Operation callback={() => { this.handleArchive(true); }} icon="archive" tooltip="Archive" />
                  <Operation callback={() => { this.handleArchive(false); }} icon="inbox" tooltip="Unarchive" />
                </React.Fragment>
              }
              <div className="placeholder" />
              <div className="archive-selector">
                <div className="wrap">
                  <Select.default
                    value={archiveMode}
                    options={archiveSelectOptions}
                    clearable={false}
                    searchable={false}
                    optionRenderer={(optionValue: Select.Option<ArchiveMode>) => (
                      <div className="metric-option" style={{ fontSize: '12px' }}>
                        {optionValue.label}
                      </div>
                    )}
                    style={{ fontSize: '12px', margin: 2, padding: 0, height: 20 }}
                    // tslint:disable-next-line:no-any
                    onChange={(newValue: any) => {
                      if (newValue !== null) {
                        this.setArchiveMode(newValue);
                      }
                    }}
                    simpleValue={true}
                  />
                </div>
              </div>
            </div>
            <div className="placeholder" />
            <div className="persona-list">
              {personas.map(ix => {
                return <div key={ix.id} >
                  <div className="list-item video-data">
                    <div className="basic">
                      <div className="checkbox">
                        {this.canUserEdit() &&
                          <input
                            name="Selected"
                            type="checkbox"
                            checked={itemSelection.findIndex(x => x === ix.id) !== -1}
                            value=""
                            onChange={v => {
                              this.toggleSelected(ix.id, Boolean(v.target.checked));
                            }}
                          />
                        }
                      </div>
                      <span className="title">{ix.name} - {ix.version}</span>
                      {ix.archived && <div className="archived-message">Archived</div>}
                    </div>
                    <PersonaAssociations
                      canEdit={this.canUserEdit()}
                      associationsToShow={associationsToShow}
                      videoItemState={ix}
                      onDrop={x => this.handleDrop(ix.id, x)}
                    />
                  </div>
                  <div className="separator" />
                </div>;
              })}
            </div>
          </div>
          <div className="unassociated">
            <div className="unassoc-title">
              Unassociated AdSets
          </div>
            <div className="video-pagination">
              <a
                onClick={() => this.updatePage(-1)}
                style={page <= 0 ? { color: 'grey', pointerEvents: 'none' } : { cursor: 'pointer' }}
                className="left"
              >
                <FontAwesomeIcon icon="chevron-left" />
              </a>
              <span className="center">{prependZeros(log10, firstPage)} - {prependZeros(log10, lastInPage)}</span>
              <span className="center-right" style={{ marginLeft: 0, fontWeight: 300 }}>of {adsetSize}</span>
              <a
                onClick={() => this.updatePage(+1)}
                style={page >= lastPage ? { color: 'grey', pointerEvents: 'none' } : { cursor: 'pointer' }}
                className="right"
              >
                <FontAwesomeIcon icon="chevron-right" />
              </a>
            </div>

            <div className="search">
              <form>
                <div className="text-container">
                  <input
                    type="text"
                    onKeyPress={(evt) => {
                      if (evt.key === 'Enter') {
                        this.setState({
                          search: evt.currentTarget.value,
                          adsetPage: 0
                        });
                        evt.preventDefault();
                      }
                    }}
                  />
                </div>
                <button
                  type="submit"
                  value="Submit"
                  style={{ marginRight: 10 }}
                  onClick={(e) => e.preventDefault()}
                >
                  <FontAwesomeIcon icon="search" className="f-icon" />
                </button>
              </form>
            </div>
            <Unassociations
              canEdit={this.canUserEdit()}
              associationsToShow={associationsToShow}
              unassociated={filteredUnassociatedAdSets.slice(firstPage, lastInPage)}
              onDrop={(item: SourceProps) => this.handleDropOnUnassociated(item)}
            />
          </div>
        </div>
      </div>
    );
  }
}

export interface PersonaItemState extends PersonaVersion {

}
const videoTargetAssociation = {
  drop(props: PersonaAssociationsProps, monitor: DropTargetMonitor) {
    props.onDrop(monitor.getItem());
  },
};

export interface PersonaAssociationsProps {
  associationsToShow: SourceObjectType;
  videoItemState: PersonaVersion;
  canDrop?: boolean;
  isOver?: boolean;
  connectDropTarget?: ConnectDropTarget;
  onDrop: (item: SourceProps) => void;
  canEdit: boolean;
}
@DropTarget(
  (props: PersonaAssociationsProps) => SourceObjectType[props.associationsToShow],
  videoTargetAssociation,
  (connect, monitor) => ({
    canDrop: monitor.canDrop(),
    connectDropTarget: connect.dropTarget(),
    isOver: monitor.isOver(),
  }),
)
export class PersonaAssociations extends React.Component<PersonaAssociationsProps> {
  constructor(props: PersonaAssociationsProps) {
    super(props);
  }

  render() {
    const {
      videoItemState,
      isOver,
      canDrop,
      connectDropTarget,
    } = this.props;

    const isActive = isOver && canDrop;
    let backgroundColor = 'white';
    if (isActive) {
      backgroundColor = '#28ABE2';
    }
    const { adSets, id } = videoItemState;
    const associationsArrays: JSX.Element[] =
      adSets
        .sort((x, y) => x.title.localeCompare(y.title)).map((sourceObject) => {
          return (
            <SourceComponent
              key={sourceObject.sourceId}
              originId={id}
              sourceObject={sourceObject}
              canEdit={this.props.canEdit}
            />
          );
        });

    const associations: JSX.Element[] = associationsArrays;

    return (
      connectDropTarget &&
      connectDropTarget(
        <div className="associations" style={{ backgroundColor }}>
          {associations}
        </div>
      )
    );
  }
}

const PersonaManagement = connect(
  (state: ApplicationState) => ({
    ...state.loginData,
  }),
  {
    ...NavMenuStore.actionCreators,
    ...LoginDataStore.actionCreators,
  }
)(PersonaManagementInternal);

export default PersonaManagement;
