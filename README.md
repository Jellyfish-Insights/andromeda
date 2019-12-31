#+TITLE: FEE YEAR-AP: A Social Media Analytics Platform

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](code-of-conduct.md)

* Introduction

** What is this repo?

This repo contains the main code of the "FEE YEAR Analytics Platform".

** What is the FEE YEAR-AP

It is an application that will collect analytics data from YouTube, Google Ads
and Facebook and generate reports about it, so that FEE can improve their
reach on the population and spread their ideas on liberty.

* Building and Running

The AP is composed of the following subsystems:
    - Console App
    - Web App

The code for all of them is located in the ~src/~ directory. The instructions
below assume you are there.

** System Requirements

You need:
    - .NET Core SDK 2.1.300
    - PostgreSQL 10
    - node v9.11.1
    - npm 5.6.0

** System Bootstrap
:PROPERTIES:
:CUSTOM_ID: system-bootstrap
:END:

You'll need to setup a few things:
    - Install front-end dependencies;
    - Create initial migration;
    - Place the credential files.

*** Note for Windows

   You need to install [[https://git-scm.com/download/win][Git for Windows]]. All of the commands in this
   manual need to run over "Git Bash", not over "cmd.exe" or "powershell".

   You also need to run the following commands from an admin shell to be able
   to install the npm dependencies.
   #+BEGIN_SRC shell
     npm install --global --production windows-build-tools
     npm install --global node-gyp
   #+END_SRC
   This will allow you to do ~npm install~.

*** Install front-end dependencies

Do:
#+BEGIN_SRC shell
  cd WebApp
  npm install
#+END_SRC

*** Build back-end

Do:
#+BEGIN_SRC shell
  dotnet clean
  dotnet build
#+END_SRC

*** Place the credential files

The following credential files are needed:
    - By the AdWords library:
        - ~./Jobs.Fetcher.AdWords/App.config~

    - By the ConsoleApp project:
        - The "YouTube" credentials
            - ~./ConsoleApp/credentials/client_secret.json~
            - ~./ConsoleApp/credentials/Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json~
        - The "Facebook" credentials
            - ~./ConsoleApp/credentials/addaccount_credentials.json~
            - ~./ConsoleApp/credentials/page_credentials.json~

** Running with Docker

   See [[https://docs.docker.com/install/linux/docker-ce/ubuntu/][this]] and [[https://github.com/docker/compose/releases][this]] for instructions on how to install Docker and
   Docker-compose.

   For Windows, the installation instructions are [[https://docs.docker.com/docker-for-windows/install/][here]]. Docker for
   Windows includes docker-compose.

   Export the ~DOCKER_USER~ variable to ensure docker uses the same
   user as the host. In Windows, ~$USER~ isn't defined, so you'll need
   to substitute it by your username:
   #+BEGIN_SRC shell
     export DOCKER_USER=$(id -u $USER)
   #+END_SRC

   Now, you need to log in as your GitLab user in the GitLab registry:
   #+BEGIN_SRC shell
     docker login registry.gitlab.com
   #+END_SRC

*** WebApp

   Do:
   #+BEGIN_SRC shell
     docker-compose -f docker-compose.real.yml up
   #+END_SRC

   This will make the web application available at [[https://localhost/]].

*** ConsoleApp

   Do:
   #+BEGIN_SRC shell
     docker-compose -f docker-compose.daemons.yml up
   #+END_SRC

   This will execute all the jobs. For more information on these jobs check
   [[./src/README.org#jobs][its documentation]].

** Running Manually

*** Setup PostgreSQL (Linux)

    We'll need two database servers, so we recommend to just use the
    docker container in the docker compose file:
    #+BEGIN_SRC shell
      docker-compose -f docker-compose.daemons.yml up -d data_lake analytics_platform
    #+END_SRC

    After that you need to add an entry to ~/etc/hosts~ as the
    following:
    #+BEGIN_QUOTE
      127.0.0.1 data_lake
      127.0.0.1 analytics_platform
    #+END_QUOTE

*** Setup PostgreSQL (Windows)

    Install [[https://www.postgresql.org/download/windows/][PostgreSQL]], and set the password of user ~postgres~
    to ~dbpassword~.

    After that you need to add an entry to
    ~C:\Windows\System32\Drivers\etc\hosts~ as the following:
    #+BEGIN_QUOTE
      127.0.0.1 data_lake
      127.0.0.1 analytics_platform
    #+END_QUOTE

    Finally, modify all ~appsettings.json~ files, removing the ~Port=5433~
    entry from the connection strings, and changing the user to ~postgres~.

*** Adding data to the development databases

    Since the system is already running in production, we suggest loading
    a dump of the production databases.

*** Building the system

    Assuming that you just did the [[#system-bootstrap][system bootstrap]],
    you'll need to apply the migrations:
    #+BEGIN_SRC shell
      cd ConsoleApp
      ./migrate.sh
    #+END_SRC

*** Running the system
    :PROPERTIES:
    :CUSTOM_ID: run-system
    :END:

    To execute the ~WebApp~, got into its directory and use the ~dotnet run~
    command.  When executing the ~WebApp~, the web system will be available
    at [[http://localhost:5000]].

    For running the jobs, you'll need to do:
    #+BEGIN_SRC shell
      cd ConsoleApp
      dotnet run -- jobs
    #+END_SRC

*** Creating a user

    Make sure you set your email as the "DefaultUserEmail" in
    ~WebApp/appsettings.json~. Restart Web App and you'll become
    an admin of the system.

    To invite new users, navigate to the User Management page.

* Developing

When developing, make sure you install the git pre-commit hook. For more
details, see the ~hooks/~ directory.
