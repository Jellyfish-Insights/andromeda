import abc
from typing import List

from year_ap_predictive.library_types import (
    Label,
    SampleDataPoint,
    SampleFeature,
    SampleIdentifier,
    SampleValue,
)


class DataSource(abc.ABC):
    """
    This is the interface that must be implemented by data sources of the predictive analysis module.
    """

    @abc.abstractmethod
    def list_sample_ids(self) -> List[SampleIdentifier]:
        pass

    @abc.abstractmethod
    def get_feature_vector(self, sample_id: SampleIdentifier) -> SampleFeature:
        pass

    @abc.abstractmethod
    def get_value_vector(self, sample_id: SampleIdentifier) -> SampleValue:
        pass

    @abc.abstractmethod
    def get_class_of_sample(self, sample_id: SampleIdentifier) -> Label:
        pass

    @abc.abstractmethod
    def should_consider_sample_id(self, dataPoint: SampleDataPoint) -> bool:
        pass
