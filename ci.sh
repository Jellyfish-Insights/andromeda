#!/bin/bash

set -ex

# link the hostnames data_lake and analytics_platform to the localhost address
sudo /bin/bash -c 'echo -e "127.0.0.1 data_lake" >> /etc/hosts'
sudo /bin/bash -c 'echo -e "127.0.0.1 analytics_platform" >> /etc/hosts'

# check versions
dotnet --info
docker --version
docker-compose --version

# enter source directory
cd src

# up databases
docker-compose -f docker-compose.test.yml up -d

# build all projects
dotnet build ./src.sln -c Release

# run c# tests
cd Test
dotnet test --verbosity normal
