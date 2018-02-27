#!/bin/bash

set -ex

# check versions
dotnet --info
node --version
npm --version
tsfmt --version
uncrustify --version

# check for commited password
test $(grep 'Password=' $(find . -name 'appsettings.json') | grep -v 'Password=dbpassword' | wc -l) -eq 0

# enter source directory
cd src

# check C# style
uncrustify -c ../uncrustify.cfg --check $(find . -name '*.cs' | grep -v "Migrations")

# enter WebApp directory
cd WebApp

# check TS(X) style
tsfmt --verify $(find . -name '*.ts' -o -name '*.tsx' | grep -v 'types.*\.ts$')

# install front-end dependencies
npm install

# run front-end tests
npm test

# go back to source directory
cd ..

# build all projects
dotnet build

# fix connection strings
./use-ci-conn-string

# apply migrations
cd ConsoleApp
./migrate.sh

# run C# tests
cd ../Test
dotnet test --verbosity normal --no-restore --no-build
