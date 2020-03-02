#!/bin/bash

set -ex

FOLDER="credentials"
FILES=("appsettings.json")

all_run_files_exists () {
    if [ ! -e "$FOLDER" ]; then
        echo "$FOLDER does not exist!"
        echo "Check if you bound the correct host folder with all the necessary files on the docker-compose file!"
        # 1 = false/the process failed
        return 1
    fi
    cd $FOLDER
    for FILE in $FILES; do
        if [ ! -f "$FILE" ]; then
            echo "Could not find the file: $FILE"
            echo "Check if the bound host folder contain this file!"
            # 1 = false
            return 1
        fi
    done
    cd ..
    # 0 = true/success
    return 0
}

if all_run_files_exists; then
    echo "Copying configuration files"
    cp credentials/appsettings.json .
    # Andromeda expects Adwords credentials to be in the same folder
    # as the binary, therefore, instead of copying this file inside the container
    # we create a symbolic link
    if [ -f "/app/release/credentials/adwords/App.config" ];then
        ln -s  /app/release/credentials/adwords/App.config Andromeda.ConsoleApp.dll.config
    else
        echo "Failed to link files! Could not find Adwords credentials: credentials/adwords/App.config"
    fi

    echo "Migrating data"
    dotnet Andromeda.ConsoleApp.dll migrate --data-lake
    dotnet Andromeda.ConsoleApp.dll migrate --facebook-lake

    if [ -z "$FETCH_SLEEP_TIME" ] ; then
        SLEEP_TIME = 3600 #14400 #seconds
    fi
    echo "My sleep time is: ${FETCH_SLEEP_TIME} seconds"

    while [ -f Andromeda.ConsoleApp.dll ]
    do
        echo "Fetching Data"
        dotnet Andromeda.ConsoleApp.dll fetcher
        echo "Sleeping"
        sleep $FETCH_SLEEP_TIME
    done
fi