import * as React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as XLSX from 'xlsx';

export interface ExportToExcelState {
  hovered: boolean;
}

type CellValue = string | number | Date;

export type Sheet = {
  SheetName: string,
  Summary: CellValue[][],
  Header: string[],
  Data: CellValue[][]
};

export type ExportToExcelProps = {
  getData: () => Sheet[];
  fileName: string;
};

export class ExportToExcelButton extends React.Component<ExportToExcelProps, ExportToExcelState> {
  constructor(props: ExportToExcelProps) {
    super(props);
    this.downloadData = this.downloadData.bind(this);
    this.state = {
      hovered: false
    };
  }

  downloadData() {
    const wb = XLSX.utils.book_new();
    this.props.getData().forEach(sheet => {
      const sheetData = sheet.Summary.concat([[]]).concat([sheet.Header]).concat(sheet.Data);
      const ws = XLSX.utils.aoa_to_sheet(sheetData, { cellDates: true });
      XLSX.utils.book_append_sheet(wb, ws, sheet.SheetName);
    });
    /* Trigger Download with `writeFile` */
    XLSX.writeFile(wb, this.props.fileName, { compression: true });
  }

  render() {
    const { hovered } = this.state;
    return (
      <button
        className="export"
        onClick={this.downloadData}
        onMouseEnter={() => this.setState({ hovered: true })}
        onMouseLeave={() => this.setState({ hovered: false })}
      >
        <FontAwesomeIcon icon="download" className="f-icon" style={{ color: (hovered ? 'white' : '#59595C') }} />
      </button>
    );
  }
}
