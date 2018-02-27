"""
This module defines functions that may be used to write reports based on the result
of the predictive analsysis pipe-line.

User applications should use the factory method 'get_writer' in order to access
the available report writers.
"""

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


def plot_and_show(grouped_data: GroupDataSet, models: GroupModels, scores: GroupScores) -> None:
    """
    Plots the real data and the learned curve. Assumes both feature and value vector are univariate.
    """
    n_groups = len(models.keys())
    _, ax = plt.subplots(n_groups, 1, sharex='col')

    for i, p in enumerate(models.items()):
        group_key, model = p
        X = to_numpy_2d_matrix(grouped_data[group_key].featureMatrix).reshape(-1, 1)
        Y = to_numpy_2d_matrix(grouped_data[group_key].valueMatrix).reshape(-1, 1)

        test_x = np.linspace(X.min(), X.max()).reshape(-1, 1)
        test_y = model.predict(test_x)

        X = np.power(10, X)
        Y = np.power(10, Y)

        test_x = np.power(10, test_x)
        test_y = np.power(10, test_y)

        ax[i].loglog(X, Y, 'bo', label='sample data points')
        ax[i].loglog(test_x, test_y, 'r-', label='learned line')
        ax[i].loglog(test_x, test_x, 'k--', label='y=x')
        ax[i].legend(loc="upper right")
        ax[i].grid(True)
        ax[i].set_title(group_key)
        ax[i].set_xlabel('Log of View Count at 7th day')
        ax[i].set_ylabel('Log of View Count at 30th day')

    plt.show()
