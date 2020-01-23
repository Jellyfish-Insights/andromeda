# Jellyfish: A Social Media data lake

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](code_of_conduct.md)
[![license](https://img.shields.io/badge/license-Apache%202-blue)](License.txt)

# Introduction

## What is this repo?

This repo contains the main code of the "Jellyfish".

## What is the Jellyfish

It is an OS application that will collect analytics data from YouTube, Google Ads
and Facebook and generate reports about it.

The idea on making Jellyfish possible came by [FEE](https://fee.org/about)

# Building and Running

The code for all of them is located in the ```src``` directory. The instructions
below assume you are there.

## System Requirements

You need:
  - [.NET Core SDK 2.1.300](https://dotnet.microsoft.com/download/dotnet-core/2.1)
  - [PostgreSQL 10](https://www.postgresql.org/)
  - [Docker](#Running-with-Docker)
  - [Docker-compose](#Running-with-Docker)

## System Bootstrap

You'll need to setup a few things:
  - [Run the database with docker](#Running-with-Docker)
  - [Create initial migration](#Building-the-system)
  - [Place the credential files](#Place-the-credential-files)

### Note for Windows

   You need to install [Git for Windows](https://git-scm.com/download/win) All of the commands in this
   manual need to run over "Git Bash", not over "cmd.exe" or "powershell".

### Build back-end

Do:
```shell
  dotnet clean
  dotnet build
```

### Place the credential files

The following credential files are needed:
  - By the AdWords library:
    - ```./Jobs.Fetcher.AdWords/App.config```

  - By the ConsoleApp project:
    - The "YouTube" credentials
      - ```./ConsoleApp/credentials/client_secret.json```
      - ```./ConsoleApp/credentials/Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json```
    - The "Facebook" credentials
      - ```./ConsoleApp/credentials/addaccount_credentials.json```
      - ```./ConsoleApp/credentials/page_credentials.json```

## Running with Docker

   See [this](https://docs.docker.com/install/linux/docker-ce/ubuntu/) and [this](https://github.com/docker/compose/releases) for instructions on how to install Docker and
   Docker-compose.

   For Windows, the installation instructions are [here](https://docs.docker.com/docker-for-windows/install/). Docker for Windows includes docker-compose.

   Export the ```DOCKER_USER``` variable to ensure docker uses the same
   user as the host. In Windows, ```$USER``` isn't defined, so you'll need
   to substitute it by your username:
   ```shell
     export DOCKER_USER=$(id -u $USER)
   ```

   Now, you need to log in as your GitLab user in the GitLab registry:
   ```shell
     docker login registry.gitlab.com
   ```

## Running Manually

### Setup PostgreSQL (Linux)

  We'll need two database servers, so we recommend to just use the
  docker container in the docker compose file:

  ```shell
    docker-compose -f docker-compose.daemons.yml up -d data_lake analytics_platform
  ```

  After that you need to add an entry to ```/etc/hosts``` as the
  following:

  ```
    127.0.0.1 data_lake
    127.0.0.1 analytics_platform
  ```

### Setup PostgreSQL (Windows)

  Install [PostegresSQL](https://www.postgresql.org/download/windows/), and set the password of user ```postgres``` to ```dbpassword```.

  After that you need to add an entry to
  ```C:\Windows\System32\Drivers\etc\hosts``` as the following:
  ```
    127.0.0.1 data_lake
    127.0.0.1 analytics_platform
  ```

  Finally, modify all ```appsettings.json``` files, removing the ```Port=5433```
  entry from the connection strings, and changing the user to ```postgres```.

### Adding data to the development databases

  Since the system is already running in production, we suggest loading
  a dump of the production databases.

### Building the system

  Assuming that you just did the [System Bootstrap](#system-bootstrap),
  you'll need to apply the migrations:
  ```shell
    cd ConsoleApp
    ./migrate.sh
  ```

### Running the system
  
  For running all the jobs, you'll need to do:
  ```shell
    cd ConsoleApp
    dotnet run -- jobs
  ```

  It's also possible to run an specific job with the following command:
  ```shell
    dotnet run -- jobs -j Jobs.Fetcher.Facebook.page.posts
  ```

  One type of Job:

  ```shell
    dotnet run -- jobs -t Fetcher -s Facebook
  ```

  Or see a list of jobs:

  ```shell
    dotnet run -- jobs -l
  ```

# Developing

When developing, make sure you install the git pre-commit hook. For more
details, see the ```hooks/``` directory.
