#!/bin/bash

set -ex

# link the hostnames data_lake and analytics_platform to the localhost address
sudo /bin/bash -c 'echo -e "127.0.0.1 data_lake" >> /etc/hosts'
sudo /bin/bash -c 'echo -e "127.0.0.1 analytics_platform" >> /etc/hosts'

sudo apt-get install build-essential

git clone https://github.com/uncrustify/uncrustify.git \
    && cd uncrustify && git checkout "uncrustify-0.67" \
    && mkdir build && cd build \
    && cmake .. && make && sudo make install \
    && cd ../../ && rm -rf uncrustify

# check versions
dotnet --info
docker --version
docker-compose --version
uncrustify --version

# enter source directory
cd src

#Uncrustify verification
uncrustify -c ../uncrustify.cfg --check $(find . -name '*.cs' | grep -v "Migrations")

#If check is failing run this command locally
#uncrustify -c ../uncrustify.cfg --no-backup $(find . -name '*.cs' | grep -v "Migrations")

# up databases
docker-compose -f docker-compose.test.yml up -d

# build all projects
dotnet build ./src.sln -c Release

# run c# tests
cd Test
dotnet test --verbosity normal
