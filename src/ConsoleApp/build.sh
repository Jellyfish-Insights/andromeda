#!/bin/bash

set -ex

dotnet publish -c Release -o $RELEASE
