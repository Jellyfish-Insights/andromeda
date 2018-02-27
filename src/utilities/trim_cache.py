#!/usr/local/bin/env python3

'''
    Script to clean cache older than a week
    This script should run periodically using cron job.
'''

import os
import sys
import json
from datetime import date, timedelta
from glob import glob


def usage():
    print("usage:")
    print("python3 deleted_older_cache.py <path_to_facebook_cache>")
    exit()


def main():
    sys.argv.pop(0)
    if len(sys.argv) < 1:
        usage()

    path = sys.argv.pop(0)
    one_week_ago = str(date.today() - timedelta(days=7))
    removed = 0

    print("removing cache files older that %s" % one_week_ago)
    for file in glob("%s/*.json" % path):
        with open(file, 'r') as cache_file:
            data = json.load(cache_file)
            if data['fetch_time'][:10] < one_week_ago:
                print("Removing cache file %s" % file)
                os.remove(file)
                removed += 1
    print("done")
    print("removed %d cache files" % removed)


if __name__ == "__main__":
    main()
