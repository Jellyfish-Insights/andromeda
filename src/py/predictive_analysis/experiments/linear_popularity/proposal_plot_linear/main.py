"""

Sets up the early to late log popularity experiment that shows that
mentioned on the FEE YEAR AP: PREDICTIVE ANALYSIS PROPOSAL.

"""

from sklearn import linear_model

from year_ap_predictive import predictive_framework

from data_sources.linear_log_popularity_data_source import EarlyToLatePopularityDataSource
from report_writer import plot_and_show

CONFIG_FILE = '../config.json'

data_source = EarlyToLatePopularityDataSource(3, 7, CONFIG_FILE)
model = linear_model.LinearRegression

data_source.open_connection()
(group_data, group_models, group_scores) = predictive_framework.run(data_source, model)
data_source.close_connection()

plot_and_show(
    group_data,
    group_models,
    group_scores,
    "View count at 3th day",
    "View count at 7th day",
)
