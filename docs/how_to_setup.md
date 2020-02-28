# Introduction

This document has the intention of explaining how to set up the environment to
run and test Andromeda. Besides this document, you can consult the Medium article
on [Getting started with
Andromeda](https://medium.com/@insightsjellyfish/andromeda-storing-your-social-media-data-in-one-place-b91a6ab3d022)
for more details.

## Note for Windows

  You need to install [Git for Windows](https://git-scm.com/download/win). All of the commands in this
  manual need to run over "Git Bash", not over "cmd.exe" or "powershell".

## System Requirements

You need:
  - [.NET Core SDK 2.1.300](https://dotnet.microsoft.com/download/dotnet-core/2.1)
  - [PostgreSQL 10](https://www.postgresql.org/)
  - [Docker](#Running-with-Docker)
  - [Docker-compose](#Running-with-Docker)

## System Bootstrap

You'll need to setup a few things before running Andromeda:

  - [Run the Postgres database](#Running-with-Docker)
  - [Create initial migration](#Building-the-system)
  - [Place the credential files](#Place-the-credential-files)

## Place the credential files

If you want to fetch data from all the social media that Andromeda supports the
following credential files are needed:

  - By the Andromeda.ConsoleApp project:
    - The "AdWords" credentials:
      - ```./Andromeda.ConsoleApp/credentials/adwords/App.config```
    - The "YouTube" credentials
      - ```./Andromeda.ConsoleApp/credentials/youtube/client_secret.json```
      - ```./Andromeda.ConsoleApp/credentials/youtube/channel_1/Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json```
    - The "Facebook" credentials
      - ```./Andromeda.ConsoleApp/credentials/facebook/adaacount/user1_credentials.json```
      - ```./Andromeda.ConsoleApp/credentials/facebook/page/user1_credentials.json```
    - The "Instagram" credentials
      - ```./Andromeda.ConsoleApp/credentials/instagram/user1_credentials.json```

Check how to get these credentials on [How to get credentials
documentation](./how_to_get_credentials.md).

Andromeda also supports fetching data from multiple Facebook and YouTube
accounts, see the [Adding multiple accounts
documentation](./adding_multiple_accounts.md) for more details.

## Running Andromeda with Docker

You can see how to run Andromeda using the docker container on [How to build and
run the Andromeda container](./docker_container) document.

## Compiling and Running Andromeda

The following instructions will explain how to compile and run Andromeda and
are assuming you are in the ```andromeda``` directory that you cloned.

### Building Andromeda

To build Andromeda code
Do:
```shell
  dotnet clean
  dotnet build
```

### Setup PostgreSQL database (Linux)

  We'll need one database server, so we recommend to just use the
  docker container in the docker-compose-andromeda.yml file.

  On the root directory run:

  ```shell
    docker-compose -f docker-compose-andromeda.yml up -d data_lake
  ```

  After that you need to add an entry to ```/etc/hosts``` as the
  following:

  ```shell
    127.0.0.1 data_lake
  ```

### Setup PostgreSQL (Windows)

  Install [PostegresSQL](https://www.postgresql.org/download/windows/), and set the password of user ```postgres``` to ```dbpassword```.

  After that you need to add an entry to
  ```C:\Windows\System32\Drivers\etc\hosts``` as the following:
  ```
    127.0.0.1 data_lake
  ```

Finally, modify the ```appsettings.json``` file located on the folder
Andromeda.ConsoleApp by removing the ```Port=5433``` entry from the connection strings, and changing the user to ```postgres```.

**You can see more details of what are the ```appsettings.json``` files and how to
edit them on [Configuring the appsettings.json](./docker_container.md#configuring-the-appsettingsjson).**

### Initial Migration

  Assuming that you just did the [System Bootstrap](#system-bootstrap),
  you'll need to apply the migrations:

  ```shell
    cd Andromeda.ConsoleApp
    dotnet run migrate --data-lake
    dotnet run migrate --facebook-lake
  ```

### Running the system

  The following instructions show some of the ways to run Andromeda.
  **This topic assumes that you just did all the steps explained above and are on the Andromeda.ConsoleApp folder.**

  To fetch data from all social medias, run:

  ```shell
    dotnet run -- fetcher
  ```

  Or Fetch just from a specific Social media:

  ```shell
    dotnet run -- fetcher -s Facebook
  ```

  After fetching your data, you can export your data as CSV with the command:

  ```shell
    dotnet run export
  ```

You can see more details about how to export data using Andromeda on [How to
export data lake data as CSV or JSON](./export_csv_json.md) documentat.

  To see all the commands available and their description:

  ```shell
    dotnet run -- --help
  ```

  It's also possible to see the description of sub-commands:

  ```shell
    dotnet run -- fetcher --help
  ```
