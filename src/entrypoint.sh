#!/bin/bash

set -ex

export RELEASE=out

cd ConsoleApp

./migrate.sh

./build.sh

# API secrets
cp -R credentials $RELEASE

# Database secrets
cp appsettings.json $RELEASE/

# Create directory for facebook cache
mkdir -p $RELEASE/cache

# Execute
: ${SLEEP_TIME:=300}

cd $RELEASE

set +e

while true
do
    dotnet ConsoleApp.dll jobs "$@"

    sleep $SLEEP_TIME
done
