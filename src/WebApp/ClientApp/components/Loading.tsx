import * as React from 'react';
import * as ReactLoading from 'react-loading';

export interface LoadingProps {
  externalHeight: number;
  height: number;
  width: number;
}

export const Loading = (props: LoadingProps) => {
  let { externalHeight, height, width } = props;
  return (
    <div className="chart-container">
      <div className="chart">
        <div className="chart-loading" style={{ height: externalHeight }}>
          <ReactLoading.default type={'spin'} color={'#eb4032'} height={height} width={width} />
        </div>
      </div>
    </div>
  );
};
