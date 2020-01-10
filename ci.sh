#!/bin/bash

set -ex

# link the hostnames data_lake and analytics_platform to the localhost address
sudo /bin/bash -c 'echo -e "127.0.0.1 data_lake" >> /etc/hosts'
sudo /bin/bash -c 'echo -e "127.0.0.1 analytics_platform" >> /etc/hosts'

# Installing uncrustify
RUN git clone https://github.com/uncrustify/uncrustify.git \
    && cd uncrustify && git checkout "uncrustify-0.67" \
    && mkdir build && cd build \
    && cmake .. && make && make install \
    && cd ../../ && rm -rf uncrustify

# check versions
dotnet --info
node --version
npm --version
docker --version
docker-compose --version
uncrustify --version

# Checking if the code is formated
uncrustify -c ../uncrustify.cfg --check $(find . -name '*.cs' | grep -v "Migrations")

# enter source directory
cd src

# up databases
docker-compose -f docker-compose.test.yml up -d

# install the correct node version
sudo npm install n
sudo n 9.11.1

# enter WebApp directory
cd WebApp
npm install
npm test

cd ..
# build all projects
dotnet build ./ap.sln -c Release

# run c# tests
cd Test
dotnet test --verbosity normal
