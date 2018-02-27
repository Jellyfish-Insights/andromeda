import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as React from 'react';
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import { connect } from 'react-redux';
import { Redirect } from 'react-router';
import { RouteComponentProps } from 'react-router-dom';
import { ComponentIds } from '../../routes';
import { ApplicationState } from '../../store';
import { sanitizeTag } from '../Edit';
import { Title } from '../Title';
import { CancelAndSaveButtons } from './VideoManagementEdit';
import * as LoginDataStore from '../../store/LoginData';
import * as NavMenuStore from '../../store/NavMenu';
import { Tag, TagEdit, TagEdits, EditType, YearApPermissionLevels } from '../../types';
import { Prompt } from 'react-router';
import { authenticatedFetch } from '../../ConnectionUtil';
import * as _ from 'underscore';

type TagManagementProps =
  NavMenuStore.NavMenuState &
  typeof NavMenuStore.actionCreators &
  LoginDataStore.LoginDataState &
  typeof LoginDataStore.actionCreators &
  RouteComponentProps<{}>;

type TagList = { [type: string]: { [id: string]: Tag } };

export interface TagManagementState {
  newTagId: number;
  tagTypes: string[];
  edits: TagEdits;
  tagList: TagList;
  saved: boolean;
}

const editStyle = (edit: TagEdit): React.CSSProperties => {
  if (!edit) {
    return {
      backgroundColor: 'rgb(135, 135, 135)',
      color: 'white'
    };
  } else {
    switch (edit.flag) {
      case EditType.Delete:
        return {
          backgroundColor: 'white',
          textDecoration: 'line-through',
          textDecorationColor: 'black',
        };
      case EditType.New:
        return {
          backgroundColor: 'rgb(82, 170, 221)',
          color: 'white'
        };
      case EditType.Update:
        return {
          backgroundColor: 'rgb(234, 234, 234)',
          color: 'rgb(135, 135, 135)'
        };
      default:

    }
  }

};
const applyEdits = (state: TagManagementState) => {
  const tags = { ...(state.tagList[state.edits.type] || {}) };
  const eds = state.edits.edits;
  _.keys(eds).map(k => {
    const ed = eds[k];
    switch (ed.flag) {
      case EditType.Delete:
        // Do nothing in the delete case, we strike the entry instead of deleting
        break;
      case EditType.New:
        if (ed.name !== null && ed.name !== undefined) {
          tags[k] = { value: ed.name, type: state.edits.type, updateDate: new Date() };
        }
        break;
      case EditType.Update:
        const v = tags[k];
        if (v && eds[k].name) {
          tags[k] = { ...v, value: eds[k].name || '' };
        }
        break;
    }
  });
  return tags;

};
const loseChangesMessage = 'There are unsaved changes, are you sure?';

let registeredShortcuts: Array<(evt: KeyboardEvent) => boolean> = [];

export class TagManagementInternal extends React.Component<
  TagManagementProps,
  TagManagementState
  > {

  constructor(props: TagManagementProps) {
    super(props);
    this.state = {
      newTagId: 0,
      tagTypes: [],
      tagList: {},
      edits: { type: 'Generic', edits: {} },
      saved: false
    };
    this.props.select(ComponentIds.TagManagement);
  }

  canUserEdit() {
    return [YearApPermissionLevels.Admin, YearApPermissionLevels.Editor].includes(this.props.userrole);
  }

  handleTagCreate = () => {
    this.setState(s => {
      const newEdits = { ...s.edits };
      const newId = s.newTagId + 1;
      const newTagLabel = '';
      newEdits.edits['0' + newId.toFixed(0)] = { updateDate: new Date(), name: newTagLabel, flag: EditType.New };
      return { ...s, newTagId: newId, edits: newEdits };
    }
    );
  }
  handleTagDelete = (ix: string, x: Tag) => {
    this.setState(s => {
      const newEdits = { ...s.edits };
      if (s.edits.edits[ix] && s.edits.edits[ix].flag === EditType.New) {
        delete newEdits.edits[ix];
      } else {
        newEdits.edits[ix] = { updateDate: x.updateDate, flag: EditType.Delete };
      }
      return { ...s, edits: newEdits };
    }
    );
  }
  handleTagEdit = (ix: string, x: Tag, e: string, final: boolean) => {
    this.setState(s => {
      const newEdits = { ...s.edits };
      const exist = newEdits.edits[ix];
      const cleanTag = final ? sanitizeTag(e, x.type === 'Generic') : e;
      const original = s.tagList[x.type][ix];
      if (newEdits.edits[ix]) {
        if (exist.flag === EditType.Update) {
          if (original.value !== cleanTag) {
            exist.name = cleanTag;
          } else {
            delete newEdits.edits[ix];
          }
        } else {
          exist.name = cleanTag;
        }
      } else {
        if (original.value !== cleanTag) {
          newEdits.edits[ix] = { updateDate: x.updateDate, name: cleanTag, flag: EditType.Update };
        }
      }
      return { ...s, edits: newEdits };
    }
    );
  }
  handleChangeType = (x: string) => {
    this.setState(prev => {
      if (prev.edits.type === x) {
        return prev;
      }
      if (_.keys(prev.edits.edits).length > 0) {
        if (!confirm(loseChangesMessage)) {
          return prev;
        }
      }
      return { ...prev, newTagId: 0, edits: { type: x, edits: {} } };
    });
  }

  cancelEdit = () => {
    this.setState(prev => {
      const emptyEdits = { ...prev.edits };
      emptyEdits.edits = {};
      return { ...prev, edits: emptyEdits };
    });
  }

  editTags = () => {
    authenticatedFetch(`api/Tag/EditMetaTags`, {
      method: 'PUT',
      body: JSON.stringify(this.state.edits),
      headers: {
        'Content-Type': 'application/json'
      }
    })
      .then(response => response.json() as Promise<TagEdits>).then(result => {
        this.setState((prev) => {
          const success = _.keys(result.edits).length === 0;
          if (_.keys(result.edits).length !== 0) {
            alert('Some of your changes failed to be applied: ' + JSON.stringify(result));
          }
          return { ...prev, tagList: prev.tagList, edits: { type: result.type, edits: {} }, saved: success };
        });
        setTimeout(
          () => this.setState({ saved: false }),
          3000
        );
        this.fetchData();
      }).catch(x => {
        alert('Server failed executing your request, please check your connection: ' + JSON.stringify(x));
      }
      );
  }
  fetchData() {
    authenticatedFetch(`api/Tag/GetMetaTags`)
      .then(response => response.json() as Promise<TagList>).then(result => {
        this.setState({ tagTypes: _.keys(result), tagList: result });
      });
  }
  isDirty() {
    return _.keys(this.state.edits.edits).length !== 0;

  }
  onUnload(event: BeforeUnloadEvent) { // the method that will be used for both add and remove event
    if (this.isDirty()) {
      event.returnValue = loseChangesMessage;
    }
  }
  componentWillMount() {
    window.addEventListener('beforeunload', this.onUnload.bind(this));
    document.addEventListener('keydown', (evt) => this.handleKeyDown(evt));
  }

  public componentWillUnmount() {
    window.removeEventListener('beforeunload', this.onUnload.bind(this));
    registeredShortcuts = [];
  }

  handleKeyDown(evt: KeyboardEvent) {
    for (let i = 0; i < registeredShortcuts.length; i++) {
      const r = registeredShortcuts[i];
      if (r(evt)) {
        evt.preventDefault();
        break;
      }
    }
  }

  componentDidMount() {
    this.fetchData();
    const newTag = (evt: KeyboardEvent) => {
      if (evt.key !== ',' ||
        ![YearApPermissionLevels.Admin, YearApPermissionLevels.Editor].includes(this.props.userrole)) {
        return false;
      }
      this.handleTagCreate();
      return true;
    };

    registeredShortcuts.push(newTag);
  }

  public render() {
    if (!this.props.logged) { return <Redirect to="/account/login/admin|tag" />; }
    const { saved } = this.state;
    const options = this.state.tagTypes.sort();
    const allTags = this.state.tagList;
    const editedTags = applyEdits(this.state);
    const { edits } = this.state;
    const editDirty = _.keys(edits.edits).length !== 0;
    const canEdit = [YearApPermissionLevels.Admin, YearApPermissionLevels.Editor].includes(this.props.userrole);
    const updateButtons = (
      <CancelAndSaveButtons
        show={canEdit}
        enabled={editDirty || saved}
        saved={saved}
        cancel={() => this.cancelEdit()}
        update={() => {
          this.editTags();
        }}
      />
    );
    return (

      <div className="react-tabs grid-x page-container">

        <Prompt
          when={this.isDirty()}
          message={loseChangesMessage}
        />

        <div className="admin-tab cell large-8 large-offset-2">
          <Title
            title="Tag Management"
            extra={updateButtons}
          />
        </div>

        <div className="admin-tab cell large-8 large-offset-2">
          <Tabs
            selectedIndex={options.findIndex(x => x === edits.type)}
            onSelect={(value) => this.handleChangeType(options[value])}
          >
            <TabList>
              {
                options.map((ix) => {
                  return <Tab key={ix}>
                    {ix}
                  </Tab>;
                })
              }
            </TabList>

            {options.map((ix) => {
              const currentTags = (ix === edits.type) ? editedTags : allTags[ix];
              return <TabPanel key={ix}>
                <div key={ix} className="grid-x admin-tags">
                  {this.canUserEdit() &&
                    <div className="large-4">
                      <div
                        style={{
                          backgroundColor: 'white',
                          border: '1px solid black',
                        }}
                        className="tag"
                      >
                        <a
                          style={{ 'width': '100%', 'textAlign': 'center' }}
                          onClick={() => this.handleTagCreate()}
                        >
                          <FontAwesomeIcon icon="plus" style={{ color: 'black' }} />
                        </a>
                      </div>
                    </div>
                  }
                  {
                    _.keys(currentTags).reverse().map((ix) => {
                      const tag = currentTags[ix];
                      const inactive = edits.edits[ix] ? edits.edits[ix].flag === EditType.Delete : false;
                      let inputStyle = editStyle(edits.edits[ix]);
                      if (tag.type.toLowerCase() === 'generic') {
                        inputStyle = { ...inputStyle, textTransform: 'lowercase' };
                      }
                      return (
                        <div
                          className="large-4"
                          key={ix}
                        >
                          <div
                            style={{
                              ...editStyle(edits.edits[ix]),
                              ...(inactive ? { border: '1px solid black' } : { border: '1px solid white' })
                            }}

                            className="tag"
                          >
                            <input
                              readOnly={!this.canUserEdit() || inactive}
                              type="text"
                              autoFocus={edits.edits[ix] ? edits.edits[ix].flag === EditType.New : false}
                              value={tag.value}
                              onBlur={x => this.handleTagEdit(ix, tag, x.currentTarget.value, true)}
                              onChange={x => this.handleTagEdit(ix, tag, x.currentTarget.value, false)}
                              onKeyPress={ev => {
                                if (ev.key === 'Enter') {
                                  this.editTags();
                                }
                              }}
                              style={inputStyle}
                            />
                            {this.canUserEdit() &&
                              <a
                                type={tag.type}
                                hidden={inactive}
                                style={editStyle(edits.edits[ix])}
                                onClick={() => this.handleTagDelete(ix, tag)}
                              >
                                <FontAwesomeIcon
                                  icon="times"
                                  style={editStyle(edits.edits[ix])}
                                />
                              </a>
                            }
                          </div >
                        </div >
                      );
                    })
                  }
                </div>
              </TabPanel>;
            })
            }
          </Tabs>
        </div>
      </div>

    );
  }
}

const TagManagement = connect(
  (state: ApplicationState) => ({
    ...state.navMenu,
    ...state.loginData,
  }),
  {
    ...NavMenuStore.actionCreators,
    ...LoginDataStore.actionCreators,
  }
)(TagManagementInternal);

export default TagManagement;
