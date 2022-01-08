uid					:=	$(shell id -u)

data_lake_port		:=	5433
new_data_lake_port	:=	55433

vnc_port			:= 	5900
new_vnc_port		:= 	55900

new_sleep_interval	:=	1800

test_data_lake_df 	:= 	"test_data_lake.Dockerfile"
docker_compose_test	:= 	"docker-compose.test.yml"
appsettings			:=	"src/jobs/appsettings.json"

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

	sed -r "s/$(data_lake_port)/$(new_data_lake_port)/g" data_lake.Dockerfile > $(test_data_lake_df)
	sed "s/data_lake.Dockerfile/$(test_data_lake_df)/" docker-compose.dev.yml > $(docker_compose_test)

	sed -ri "s/scraper_dev:/scraper_test:/" $(docker_compose_test)
	sed -ri "s/$(data_lake_port)/$(new_data_lake_port)/g" $(docker_compose_test)
	sed -ri "s/$(vnc_port):$(vnc_port)/$(new_vnc_port):$(vnc_port)/g" $(docker_compose_test)
	sed -ri 's/(sleep_interval:)[ ]+"[0-9]+"/\1 "$(new_sleep_interval)"/' $(docker_compose_test)

	sed -ri "s/$(data_lake_port)/$(new_data_lake_port)/g" $(appsettings)

	docker-compose -f $(docker_compose_test) build --build-arg APP_UID=$(uid)
	docker-compose -f $(docker_compose_test) up -d
	
	echo "Undoing changes"
	rm $(test_data_lake_df) $(docker_compose_test)
	sed -ri "s/$(new_data_lake_port)/$(data_lake_port)/g" $(appsettings)

	