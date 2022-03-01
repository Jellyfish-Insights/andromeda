#!/usr/bin/env bash

# How can I run a local instance for development?

# Start a new postgres container:
# docker run -it --rm -p 5433:5432 -e POSTGRES_PASSWORD=dbpassword -e POSTGRES_USER=fee -e POSTGRES_DB=data_lake postgres

# And use these same settings in the "appsettings.json" file

set -ex

cd "$(dirname "$0")"

# Remove all containers & images (Fluxbox bug)
docker container ls -aq | xargs -r docker rm -f
docker images -aq | xargs -r docker rmi -f

# Let's determine how much memory the host system has
total_ram=$(awk '/MemTotal/{print $2}' '/proc/meminfo')
echo "Total RAM available in the system: $total_ram KB"

# If running in a system with less than 4 GB RAM, development is enforced
if [ "$SCRAPER_ENV" == "DEVELOPMENT" -o "$total_ram" -lt 4000000 ] ; then
    echo "We will use development build for the scraper"
    target="scraper_dev"
else
    echo "We will use production build for the scraper"
    target="scraper_prod"
fi

docker build \
    -t scraper \
    --build-arg APP_UID=$(id -u) \
    --target "$target" .

docker run --rm \
    --mount type=bind,source="$(pwd)/extracted_data",target="/opt/scraper/extracted_data" \
    --mount type=bind,source="$(pwd)/credentials",target="/opt/scraper/credentials" \
    --mount type=bind,source="$(pwd)/logs",target="/opt/scraper/logs" \
    -u app \
    -e sleep_interval=600 \
    -e random_order="" \
    -e SCRAPER_ENV="$SCRAPER_ENV" \
    --network host \
    --privileged \
    scraper

sleep_time=$(( 60 * 60 * 4 ))
echo "Now we will fall asleep for 4 hours"
sleep "$sleep_time"
