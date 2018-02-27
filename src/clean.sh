#!/bin/bash

git clean -xdn \
    | sed -e 's/^Would remove //' \
    | grep -v 'credentials' \
    | grep -v 'Jobs.Fetcher.AdWords\/App.config' \
    | grep -v 'Facebook\/cache' \
    | xargs rm -rf
