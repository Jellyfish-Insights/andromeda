uid		:=	$(shell id -u)

schedule:
	cd src && python3 -m scripts.scheduler --dry_run

dev:
	docker-compose -f docker-compose.dev.yml build --build-arg APP_UID=$(uid) \
		&& docker-compose -f docker-compose.dev.yml up -d

prod:
	docker-compose -f docker-compose.prod.yml build --build-arg APP_UID=$(uid) \
		&& docker-compose -f docker-compose.prod.yml up -d

stop:
	docker-compose -f docker-compose.dev.yml -f docker-compose.prod.yml down
	