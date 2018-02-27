import * as React from 'react';

type TitleProps = {
  title: string;
  extra?: JSX.Element;
};

export class Title extends React.Component<TitleProps, {}> {

  constructor(props: TitleProps) {
    super(props);
  }

  public render() {
    const { title, extra } = this.props;

    let titleStyle: React.CSSProperties = {
      color: 'black',
      fontSize: 22,
      fontWeight: 500,
      marginLeft: 10,
      display: 'flex',
      justifyContent: 'center',
      alignContent: 'center',
      flexDirection: 'column'
    };
    return (
      <div className="date-picker-container">
        <div className="date-picker">
          <div style={titleStyle}>
            {title}
          </div>
          <div className="placeholder" />
          {extra && extra}
        </div>
      </div>
    );
  }
}
