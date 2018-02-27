from sklearn import linear_model

from year_ap_predictive import predictive_framework

from report_writer import (
    plot_and_show,
    show_coeffs,
)


CONFIG_FILE = '../config.json'


def run(DataSource):
    early, late = 3, 7
    data_source = DataSource(early, late, CONFIG_FILE, ("Length",))
    model = linear_model.LinearRegression

    data_source.open_connection()
    (group_data, group_models, group_scores) = predictive_framework.run(data_source, model)
    data_source.close_connection()
    show_coeffs(group_data, group_models, group_scores, early, late)
    plot_and_show(group_data, group_models, group_scores, early, late)
