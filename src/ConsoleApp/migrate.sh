#!/bin/bash

set -ex

dotnet run -- migrate --data-lake

dotnet run -- migrate --application

dotnet run -- init-facebook-lake
