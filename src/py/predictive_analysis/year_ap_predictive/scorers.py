import numpy as np


def negative_mean_relative_square_error(estimator, log10X, log10y) -> float:
    """
    By convention, higher numbers are better, since this score is a loss function, that value is negated.
    """
    return -1 * ((np.power(10, estimator.predict(log10X)) / np.power(10, log10y) - 1) ** 2).mean()
