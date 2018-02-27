#!/bin/bash

set -ex

npm install

ASPNETCORE_ENVIRONMENT=Development dotnet run -c MockNoAuth
