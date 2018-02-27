#!/bin/bash

rm -rf build
rm -rf dist
find . -name "*.egg-info" | xargs rm -rf
rm -rf .mypy_cache
