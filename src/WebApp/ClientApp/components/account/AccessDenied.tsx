import '../../scss/account/login';
import * as React from 'react';
import { Redirect } from 'react-router';
import { FeeLogo } from '../../assets/img/FeeLogo';

export interface AccessDeniedState {
  timerId: number;
  countDown: number;
}
class AccessDenied extends React.Component<{}, AccessDeniedState> {

  constructor(props: AccessDeniedState) {
    super(props);
    this.state = { countDown: 5, timerId: 0 };
  }

  componentDidMount() {
    let interval: number = setInterval(() => this.tick(), 1000) as number;
    this.setState({ timerId: interval });
  }

  tick() {
    this.setState((prevState) => { return { countDown: prevState.countDown - 1 }; });
  }

  componentWillUnmount() {
    clearInterval(this.state.timerId);
  }

  public render() {
    const { countDown } = this.state;
    if (countDown <= 0) { return <Redirect to="/account/login/dashboard" />; }
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
        <div className="form-container grid-x">
          <h2 className="cell small-offset-3"><a href="/account/login/dashboard">Login</a></h2>
          <div className="cell small-offset-3 large-9 message-border"><span style={{ color: 'red' }}>Access denied.</span> You'll be sent back to the login page in {countDown} seconds. If you don't get redirected please <a href="/account/login/dashboard">click here</a>.</div>
        </div>
      </div>
    );
  }
}
export default AccessDenied;
