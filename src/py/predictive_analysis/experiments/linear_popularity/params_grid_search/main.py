from sklearn import linear_model

from year_ap_predictive import predictive_framework

from data_sources.linear_log_popularity_data_source import EarlyToLatePopularityDataSource
from report_writer import write_report

CONFIG_FILE = '../config.json'


def scores_statistics(scores):
    return (scores.mean(), 2 * scores.std())


data_source = EarlyToLatePopularityDataSource(7, 30, CONFIG_FILE)
model = linear_model.LinearRegression

data_source.open_connection()

early_ages = [1, 2, 3, 4, 5, 6, 7]
late_ages = [1, 2, 3, 4, 5, 6, 7, 14, 30]
experiments = []
for early in early_ages:
    for late in late_ages:
        if early >= late:
            continue
        print("running setup: early = %d, late = %d" % (early, late))
        data_source.set_params(early, late)
        (group_data, group_models, group_scores) = predictive_framework.run(data_source, model)

        for group_key, v in group_scores.items():
            r2_score = scores_statistics(v['test_r2'])
            rms_score = scores_statistics(v['test_rse'])
            experiments.append((early, late, group_key, r2_score[0], r2_score[1], rms_score[0], rms_score[1]))

data_source.close_connection()
write_report(experiments)
