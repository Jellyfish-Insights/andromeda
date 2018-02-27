"""
This module defines the types used thorough the predictve analysis component.

Whenever data is to be shared across modules of the predictive analysis
library, the types using the inter-module communication should be declared
here.

"""
from typing import (
    Any,
    Callable,
    Dict,
    List,
    NamedTuple
)


# Identifier to a data point
SampleIdentifier = str

# Features attributed to a data point.
SampleFeature = List[float]

# The value attributed to a data point. This is often a univariate vector.
SampleValue = List[float]

# The class attributed to a data point. This class is used to bucket related data points.
Label = str

FeatureMatrix = List[SampleFeature]
ValueMatrix = List[SampleValue]


class SampleDataPoint(NamedTuple):
    identifier: SampleIdentifier
    label: Label
    feature: SampleFeature
    value: SampleValue


class DataSet(NamedTuple):
    featureMatrix: FeatureMatrix
    valueMatrix: ValueMatrix
    size: int


GroupName = str
ScorerName = str
Model = Any
GroupDataSet = Dict[GroupName, DataSet]
GroupModels = Dict[GroupName, Model]
GroupScores = Dict[GroupName, Dict[ScorerName, float]]

ReportWriter = Callable[[GroupDataSet, GroupModels, GroupScores], None]
ModelClass = Any
