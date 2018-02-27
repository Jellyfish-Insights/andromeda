#!/bin/bash

set -ex

if [ -n "$MOCK" ] && [ "$MOCK" = "true" ]
then
    ./entrypoint.mock.sh
else
    ./entrypoint.real.sh
fi
