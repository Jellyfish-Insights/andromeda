"""
This module sets up the pipe-line that runs during an execution of the predictive analysis component.

The pipe line includes:
- Extracting data from data sources
- Group the data according to a samples's label
- Training a machine learning model on the data (one per sample group)
- Evaluating the trained model (one per sample group)
- Writing a report summarizing results of the experiment

User applications should interact with this module by calling the method 'run'.
"""

from itertools import groupby
from typing import (
    Any,
    Dict,
    Iterator,
    List,
    Tuple
)

from sklearn.model_selection import cross_validate
import numpy as np

from year_ap_predictive.library_types import (
    DataSet,
    GroupDataSet,
    GroupModels,
    GroupName,
    GroupScores,
    Label,
    ModelClass,
    SampleDataPoint,
    SampleFeature,
    SampleValue,
)
from year_ap_predictive.data_source import DataSource
from year_ap_predictive.scorers import negative_mean_relative_square_error


# Auxiliary Functions


def label_getter(dataPoint: SampleDataPoint) -> Label:
    return dataPoint.label


def feature_getter(dataPoint: SampleDataPoint) -> SampleFeature:
    return dataPoint.feature


def value_getter(dataPoint: SampleDataPoint) -> SampleValue:
    return dataPoint.value

# Data Manipulation Functions


def materialize_data(data_source: DataSource) -> List[SampleDataPoint]:
    samples_ids = data_source.list_sample_ids()
    return [SampleDataPoint(v, data_source.get_class_of_sample(v), data_source.get_feature_vector(v), data_source.get_value_vector(v)) for v in samples_ids]


def filter_data(data_source: DataSource, materialized_data: List[SampleDataPoint]) -> List[SampleDataPoint]:
    return [x for x in materialized_data if data_source.should_consider_sample_id(x)]


def group_raw_data_set(raw_data_set: List[SampleDataPoint]) -> Iterator[Tuple[Label, Iterator[SampleDataPoint]]]:
    filtered_data = sorted(raw_data_set, key=label_getter)
    return groupby(filtered_data, key=label_getter)


def filter_out_small_groups(group_data_set: GroupDataSet, threshold=10) -> GroupDataSet:
    return {k: v for k, v in group_data_set.items() if v.size > threshold}


def build_grouped_data_set(raw_data_set: List[SampleDataPoint]) -> GroupDataSet:
    grouped_data = group_raw_data_set(raw_data_set)
    group_data_set = {}
    for (group_key, samples) in grouped_data:
        materialized_samples = list(samples)
        group_data_set[group_key] = DataSet(
            list(map(feature_getter, materialized_samples)),
            list(map(value_getter, materialized_samples)),
            len(materialized_samples)
        )
    return group_data_set

# Pipes of the pipe-line


def extract_data(data_source: DataSource) -> GroupDataSet:
    materialized_data = materialize_data(data_source)
    filtered_data = filter_data(data_source, materialized_data)
    group_data_set = build_grouped_data_set(filtered_data)
    return filter_out_small_groups(group_data_set)


def to_numpy_vector(v):
    return np.array(v, np.float64)


def to_numpy_2d_matrix(v):
    return np.array(v, np.float64)


def learn_models(grouped_data: GroupDataSet, model_class: ModelClass) -> Dict[GroupName, Any]:
    models = {}

    for group_key, data in grouped_data.items():
        model = model_class()
        X = to_numpy_2d_matrix(data.featureMatrix)
        Y = to_numpy_2d_matrix(data.valueMatrix)
        model.fit(X, Y)
        models[group_key] = model

    return models


def evaluate_models(grouped_data: GroupDataSet, model_class: ModelClass) -> GroupScores:
    scores = {}
    scoring_metrics = {
        'r2': 'r2',
        'rse': negative_mean_relative_square_error
    }
    folds = 4

    for group_key, data in grouped_data.items():
        model = model_class()
        score = cross_validate(model, data.featureMatrix, data.valueMatrix, scoring=scoring_metrics, cv=folds)
        scores[group_key] = score

    return scores


# Pipe-line setup


def run(data_source: DataSource, model_class: ModelClass) -> Tuple[GroupDataSet, GroupModels, GroupScores]:
    """
    Extracts grouped data out of data source, learns and evaluates one model on each group
    """
    grouped_data = extract_data(data_source)
    models = learn_models(grouped_data, model_class)
    scores = evaluate_models(grouped_data, model_class)
    return (grouped_data, models, scores)
