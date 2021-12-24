#!/usr/bin/env bash

cd "$(dirname "$0")"
scraping_instructions="to_scrape.sh"

while true; do
	echo "Running scheduler"
	python3 scheduler.py \
		--scraping_interval 60 \
		--timeout 45 \
		"<ACCOUNT_NAME>" \
		"TikTok"

	if [ -f "$scraping_instructions" ] ; then
		echo "Starting scraping"
		bash "$scraping_instructions"
	fi

	sleep 5
done