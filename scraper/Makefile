uid					:=	$(shell id -u)

data_lake_port		:=	5433
new_data_lake_port	:=	55433

data_lake_name		:=	data_lake
data_lake_new_name	:=	$(data_lake_name)_test

# App settings
data_lake_old_host	:=	Host=$(data_lake_name)
data_lake_new_host	:=	Host=$(data_lake_new_name)

vnc_port			:= 	5900
new_vnc_port		:= 	55900

new_sleep_interval	:=	3600

test_data_lake_df 	:= 	data_lake_test.Dockerfile
docker_compose_test	:= 	docker-compose.test.yml
appsettings			:=	src/jobs/appsettings.json
bkp_appsettings		:=	src/jobs/appsettings.json.bkp

schedule:
	cd src ; python3 -m scripts.scheduler

dev:
	docker-compose -f docker-compose.dev.yml build --build-arg APP_UID=$(uid)
	docker-compose -f docker-compose.dev.yml up -d

prod:
	docker-compose -f docker-compose.prod.yml build --build-arg APP_UID=$(uid)
	docker-compose -f docker-compose.prod.yml up -d

stop:
	docker-compose -f docker-compose.dev.yml -f docker-compose.prod.yml down

test:
	cd src ; python3 -m tests.test_tiktok_most_followed

	sed -r "s/$(data_lake_port)/$(new_data_lake_port)/g" "data_lake.Dockerfile" > "$(test_data_lake_df)"
	cp docker-compose.dev.yml "$(docker_compose_test)"

	sed -ri "s/scraper_dev:/scraper_test:/" "$(docker_compose_test)"
	sed -ri "s/$(data_lake_port)/$(new_data_lake_port)/g" "$(docker_compose_test)"
	sed -ri "s/$(vnc_port):$(vnc_port)/$(new_vnc_port):$(vnc_port)/g" "$(docker_compose_test)"
	sed -ri 's/(sleep_interval:)[ ]+"[0-9]+"/\1 "$(new_sleep_interval)"/' "$(docker_compose_test)"
	sed -ri 's/(random_order:)[ ]+".*"/\1 "--random_order"/' "$(docker_compose_test)"

	sed -ri 's/$(data_lake_name)/$(data_lake_new_name)/' "$(docker_compose_test)"

	cp "$(appsettings)" "$(bkp_appsettings)"
	sed -ri "s/$(data_lake_port)/$(new_data_lake_port)/g" "$(appsettings)"
	sed -ri "s/$(data_lake_old_host)/$(data_lake_new_host)/g" "$(appsettings)"

	docker-compose -f "$(docker_compose_test)" build --build-arg APP_UID=$(uid)
	docker-compose -f "$(docker_compose_test)" up -d
	
	echo "Undoing changes"
	rm "$(test_data_lake_df)" "$(docker_compose_test)"
	mv "$(bkp_appsettings)" "$(appsettings)"
	rm -rf src/jobs/test*
	