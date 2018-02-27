from itertools import groupby
from operator import itemgetter
from tabulate import tabulate


def group_by(data, key_getter):
    return groupby(sorted(data, key=key_getter), key=key_getter)


def get_cel(dict_data_group, i, j, default='-'):
    cell = dict_data_group.get((i, j), None)
    if cell is None:
        return default
    return "%.2f +- %.2f" % (cell)


def build_dict_data_group(data_group, key_getter, value_getter):
    return {key_getter(row): value_getter(row) for row in data_group}


def build_tabular_data(data_group, row_keys, col_keys, key_getter, error_getter):
    dict_data_group = build_dict_data_group(data_group, key_getter, error_getter)
    return [[i] + [get_cel(dict_data_group, i, j) for j in col_keys] for i in row_keys]


def write_report(experiments):
    for group_key, data_group in group_by(experiments, itemgetter(2)):
        data_group = list(data_group)
        row_keys = sorted(list(set([row[0] for row in data_group])))
        col_keys = sorted(list(set([row[1] for row in data_group])))

        key_getter = itemgetter(0, 1)
        r2_getter = itemgetter(3, 4)
        rse_getter = itemgetter(5, 6)

        r2_tabular_data = build_tabular_data(data_group, row_keys, col_keys, key_getter, r2_getter)
        rse_tabular_data = build_tabular_data(data_group, row_keys, col_keys, key_getter, rse_getter)

        print("## %s" % group_key)
        print("### R2 Score")
        print(tabulate(r2_tabular_data, col_keys, tablefmt="simple"))
        print("### Negative Relative Square Error")
        print(tabulate(rse_tabular_data, col_keys, tablefmt="simple"))
