"""
This module provides utility functions to access the YEAR AP database.
"""

import json

SETTINGS_FILE = 'config.json'


def read_json_file(fn: str):
    with open(fn, 'r') as f:
        return json.load(f)


def conn_string_to_conn_param(conn_string: str) -> dict:
    return {y[0].lower(): y[1] for y in map(lambda x: x.split("="), conn_string.split(";"))}


def read_conn_params(database_name: str, settings_filename=None) -> dict:
    if settings_filename is None:
        settings_filename = SETTINGS_FILE
    app_settings = read_json_file(settings_filename)
    conn_string = app_settings["ConnectionStrings"][database_name]
    return conn_string_to_conn_param(conn_string)
