#!/usr/bin/env bash

# Use this to debug the scheduler without having to set the container up 
# every time

# Exit if command returns non-zero status
set -e

scroll_limit=5
timeout=0
scraping_interval=60
db_conn_string="postgresql://brab:brickabode@localhost:5432"
keep_logs=
slow_mode=
navigator_name="YouTube"

log(){
	>&2 echo -e "[BOOTSTRAP] $1"
}

main(){
	cd "$(dirname "$0")"
	scheduler_script="scheduler.py"
	scraping_instructions="to_scrape.sh"
	sleep_time=10

	# This needs to go unquoted in the command
	unquoted="$keep_logs $slow_mode"

	while true ; do
		log "Running scheduler script..."
		python3 "$scheduler_script" \
			--scroll_limit "$scroll_limit" \
			--timeout "$timeout" \
			--scraping_interval "$scraping_interval" \
			--db_conn_string "$db_conn_string" \
			$unquoted \
			"$navigator_name"

		log "Running scraper on instructions set by scheduler..."
		bash "$scraping_instructions"

		log "Sleeping for $sleep_time seconds before restarting loop..."
		sleep $sleep_time
	done
}

main