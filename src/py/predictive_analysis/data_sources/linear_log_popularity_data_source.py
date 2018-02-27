"""
This module defines the sources of data used in the predictive analysis library.

User applicatios should import the factory method `get_data_source` in order to
instantiate the classes defined on this module.
"""

import numpy as np
from typing import List

from year_ap_predictive.library_types import SampleDataPoint

from data_sources.analytics_platform_data_source import AnalyticsPlatformDataSource

# disable numpy warnings
np.warnings.filterwarnings('ignore')

# Helper functions


def project_column(rows, column_index):
    return [row[column_index] for row in rows]


def to_float64_np_array(rows):
    return np.asarray(rows, dtype=np.float64)


def get_log10_column(raw, column_index: int):
    return np.log10(to_float64_np_array(project_column(raw, column_index)))


def check_all_log_values_are_valid(feature_vector: List[float], value_vector: List[float]) -> bool:
    return not np.any(np.isneginf(np.concatenate((feature_vector, value_vector))))


# Data Sources


class EarlyToLatePopularityDataSource(AnalyticsPlatformDataSource):
    """
    Source of data for Szabo and Huberman's Linear Log Model on prediction of late video popularity.

    Due to the log transformation required by this algorithm, samples with
    popularity count equals to zero are rejected by this data source.
    """
    popularity_at_age_sql = """
        SELECT
            COALESCE(SUM(svm."ViewCount"), 0.0)
        FROM
            "SourceVideos" AS sv
                LEFT JOIN
            "SourceVideoMetrics" AS svm on sv."Id" = svm."VideoId"
        WHERE
            sv."Id" = %s
            AND svm."EventDate" < sv."PublishedAt" + INTERVAL '%s days'
            AND svm."EventDate" >= sv."PublishedAt"
    """

    total_view_count_column_index = 0

    def __init__(self, early_age: int, late_age: int, config_file: str) -> None:
        """
        Keyword arguments:
        early_age -- number of days that define early popularity
        late_age -- number of days that define late popularity
        """
        super().__init__(config_file)
        self.set_params(early_age, late_age)

    def set_params(self, early_age: int, late_age: int):
        self.early_age = early_age
        self.late_age = late_age

    def list_sample_ids(self):
        """
        list of source video ids
        """
        cmd = """
            SELECT
                "Id"
            FROM
                "SourceVideos"
        """
        raw = self.simple_execute(cmd)
        return [str(r[0]) for r in raw]

    def get_feature_vector(self, sample_id):
        """
        The only component is the early popularity count
        """
        raw = self.simple_execute(self.popularity_at_age_sql, (sample_id, self.early_age))
        return get_log10_column(raw, self.total_view_count_column_index)

    def get_value_vector(self, sample_id):
        """
        The only component is the late popularity count
        """
        raw = self.simple_execute(self.popularity_at_age_sql, (sample_id, self.late_age))
        return get_log10_column(raw, self.total_view_count_column_index)

    def get_class_of_sample(self, sample_id):
        """
        The platform of the source video
        """
        cmd = """
            SELECT
                "Platform"
            FROM
                "SourceVideos"
            WHERE
                "Id" = %s
        """
        raw = self.simple_execute(cmd, (sample_id,))
        return raw[0][0]

    def should_consider_sample_id(self, dataPoint: SampleDataPoint):
        """
        Rejects data sources with 0's on any of the entries of both the feature
        and value vector

        Why? This data source takes the log of the view count at the k-th
        day. The logarithm of 0 is not defined and thus this entries must be
        removed.

        From a business point of view, it can be argued that no prediction
        can be done uppon a video that was not yet watched.
        """
        return check_all_log_values_are_valid(dataPoint.feature, dataPoint.value)


class MetaTagAwareEarlyToLatePopularityDataSource(EarlyToLatePopularityDataSource):
    def __init__(self, early_age: int, late_age: int, config_file: str, tag_types: List[str]) -> None:
        super().__init__(early_age, late_age, config_file)
        self.tag_types = tag_types

    def get_class_of_sample(self, sample_id):
        """
        The platform of the source video
        """
        cmd = """
            SELECT
                sv."Platform",
                amt."Tag"
            FROM
                "SourceVideos" AS sv
                    JOIN
                "ApplicationVideoSourceVideos" AS avsv ON sv."Id" = avsv."SourceVideoId"
                    JOIN
                "ApplicationVideos" AS av ON avsv."ApplicationVideoId" = av."Id"
                    JOIN
                "ApplicationVideoApplicationMetaTags" AS avamt ON av."Id" = avamt."VideoId"
                    JOIN
                "ApplicationMetaTagsTypes" AS amtt ON amtt."Id" = avamt."TypeId"
                    JOIN
                "ApplicationMetaTags" AS amt ON amt."Id" = avamt."TagId"
            WHERE
                sv."Id" = %s
                    AND
                amtt."Type" IN %s
        """
        raw = self.simple_execute(cmd, (sample_id, self.tag_types))
        groups = [raw[0][0]] + [row[1] for row in raw]
        return "-".join(groups)


class MetaTagExclusiveEarlyToLatePopularityDataSource(EarlyToLatePopularityDataSource):
    def __init__(self, early_age: int, late_age: int, config_file: str, tag_types: List[str]) -> None:
        super().__init__(early_age, late_age, config_file)
        self.tag_types = tag_types

    def get_class_of_sample(self, sample_id):
        """
        The platform of the source video
        """
        cmd = """
            SELECT
                amt."Tag"
            FROM
                "SourceVideos" AS sv
                    JOIN
                "ApplicationVideoSourceVideos" AS avsv ON sv."Id" = avsv."SourceVideoId"
                    JOIN
                "ApplicationVideos" AS av ON avsv."ApplicationVideoId" = av."Id"
                    JOIN
                "ApplicationVideoApplicationMetaTags" AS avamt ON av."Id" = avamt."VideoId"
                    JOIN
                "ApplicationMetaTagsTypes" AS amtt ON amtt."Id" = avamt."TypeId"
                    JOIN
                "ApplicationMetaTags" AS amt ON amt."Id" = avamt."TagId"
            WHERE
                sv."Id" = %s
                    AND
                amtt."Type" IN %s
        """
        raw = self.simple_execute(cmd, (sample_id, self.tag_types))
        groups = [row[0] for row in raw]
        return "-".join(groups)
