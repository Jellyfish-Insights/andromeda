#!/bin/bash

set -ex

export RELEASE=out

./build.sh

# Execute
cd $RELEASE

exec dotnet WebApp.dll
