import '../../scss/account/login';
import * as React from 'react';
import { connect } from 'react-redux';
import { Redirect } from 'react-router';
import { RouteComponentProps } from 'react-router-dom';
import { ApplicationState } from '../../store';
import * as LoginDataStore from '../../store/LoginData';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import brands from '@fortawesome/fontawesome-free-brands';
import { FeeLogo } from '../../assets/img/FeeLogo';

// At runtime, Redux will merge together...
type LoginProps =
  LoginDataStore.LoginDataState               // ... state we've requested from the Redux store
  & { next: string }
  & typeof LoginDataStore.actionCreators      // ... plus action creators we've requested
  & RouteComponentProps<{}>;                  // ... plus incoming routing parameters

class LoginInternal extends React.Component<LoginProps, {}> {
  componentWillMount() {
    this.setState({});
    this.props.setLoggedAction();
  }

  public render() {
    if (this.props.logged) { return <Redirect to={this.props.next} />; }
    return (
      <div className="login-container">
        <div className="logo-container">
          <div className="logo-container-up">
            <FeeLogo />
          </div>
          <div className="logo-container-down">
            YEAR AP
          </div>
        </div>
        <div className="form-container">
          <form className="login-form" method="post" action="/api/Account/ExternalLogin">
            <div className="grid-x">
              <h2 className="cell small-offset-3">Login</h2>
              <h3 className="cell small-offset-3">You will be redirected to an external page.</h3>
              <div className="cell small-6 small-offset-3">
                <button
                  className="button social google-plus expanded"
                  type="submit"
                  value="Google"
                  name="provider"
                  title="Log in using your Google account"
                >
                  <FontAwesomeIcon icon={brands.faGooglePlusG} /> Login with Google
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>
    );
  }
}

const mapStateToProps = (state: ApplicationState, ownProps: any) => {
  return { next: '/' + ownProps.match.params.next.replace('|', '/'), ...state.loginData };
};

const mapDispatchToProps = LoginDataStore.actionCreators;

const Login = connect(
  mapStateToProps,    // Selects which state properties are merged into the component's props
  mapDispatchToProps  // Selects which action creators are merged into the component's props
)(LoginInternal);

export default Login;
