import psycopg2

from year_ap_predictive.data_source import DataSource
from year_ap_predictive.config_util import read_conn_params


class AnalyticsPlatformDataSource(DataSource):

    def __init__(self, config_file):
        self.config_file = config_file
        self.conn = None

    def open_connection(self):
        if self.conn is None:
            self.conn = psycopg2.connect(**read_conn_params("BusinessDatabase", self.config_file))

    def close_connection(self):

        if self.conn is not None:
            self.conn.close()
            self.conn = None

    def new_cursor(self):
        return self.conn.cursor()

    def simple_execute(self, cmd, params=None):
        if params is None:
            params = ()
        cursor = self.new_cursor()
        cursor.execute(cmd, params)
        return cursor.fetchall()
