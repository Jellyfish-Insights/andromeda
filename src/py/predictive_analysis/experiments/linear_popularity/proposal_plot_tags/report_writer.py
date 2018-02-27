"""
This module defines functions that may be used to write reports based on the result
of the predictive analsysis pipe-line.

User applications should use the factory method 'get_writer' in order to access
the available report writers.
"""

import math

from tabulate import tabulate
import matplotlib.pyplot as plt
import numpy as np

from year_ap_predictive.library_types import (
    GroupDataSet,
    GroupModels,
    GroupScores
)

from year_ap_predictive.predictive_framework import (
    to_numpy_2d_matrix
)


def get_chart_dimensions(x: int):
    return (math.ceil(x / math.ceil(math.sqrt(x))), math.ceil(math.sqrt(x)))


def plot_and_show(grouped_data: GroupDataSet, models: GroupModels, scores: GroupScores, early: int, late: int) -> None:
    """
    Plots the real data and the learned curve. Assumes both feature and value vector are univariate.
    """
    n_groups = len(models.keys())
    n_rows, m_cols = get_chart_dimensions(n_groups)
    _, ax = plt.subplots(n_rows, m_cols, sharex='col', figsize=(20, 10))

    for i, p in enumerate(models.items()):
        row_index = math.floor(i / m_cols)
        col_index = i % m_cols

        group_key, model = p
        X = to_numpy_2d_matrix(grouped_data[group_key].featureMatrix).reshape(-1, 1)
        Y = to_numpy_2d_matrix(grouped_data[group_key].valueMatrix).reshape(-1, 1)

        test_x = np.linspace(X.min(), X.max()).reshape(-1, 1)
        test_y = model.predict(test_x)

        X = np.power(10, X)
        Y = np.power(10, Y)

        test_x = np.power(10, test_x)
        test_y = np.power(10, test_y)

        ax[row_index][col_index].loglog(X, Y, 'bo', label='sample data points')
        ax[row_index][col_index].loglog(test_x, test_y, 'r-', label='learned line')
        ax[row_index][col_index].loglog(test_x, test_x, 'k--', label='y=x')
        ax[row_index][col_index].legend(loc="upper left")
        ax[row_index][col_index].grid(True)
        ax[row_index][col_index].set_title(group_key)
        ax[row_index][col_index].set_xlabel('Log of View Count at %dth day' % (early))
        ax[row_index][col_index].set_ylabel('Log of View Count at %dth day' % (late))

    plt.show()


def show_coeffs(grouped_data: GroupDataSet, models: GroupModels, scores: GroupScores, early: int, late: int) -> None:
    coeffs = [[k, "%.2f" % v.intercept_[0], "%.2f" % v.coef_[0][0]] for k, v in models.items()]
    print("Coefficients of the linear regression per group")
    print(tabulate(coeffs, ["group", "intercept", "rate"], tablefmt="fancy_grid"))
