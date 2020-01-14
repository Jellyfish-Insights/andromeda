#!/bin/bash

set -ex

dotnet run -- migrate --data-lake

dotnet run -- init-facebook-lake
