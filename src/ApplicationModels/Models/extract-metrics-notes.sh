#!/bin/bash

metric_files="./SourceAdMetric.cs ./SourceVideoMetric.cs ./SourceDeltaEncodedVideoMetric.cs ./SourceVideoDemographicMetric.cs"

sed -n '/\/\*\*/{:a;n;/\*\//b;p;ba}' $metric_files \
    | sed -e 's/^\s\{8,9\}//' \
    | sed -e 's/\$+/#+/' > metrics-notes.org
