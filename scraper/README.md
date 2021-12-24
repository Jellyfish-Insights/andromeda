## What is this?
This sets up a service which scrapes data from social media periodically.
The accounts to be scraped are fetched from a containerized database
Later, the scraped data can be retrieved from the same database.
This was designed as an API for the [Andromeda project](https://github.com/Jellyfish-Insights/andromeda).

![](docs/img/data-screenshot.png)

## How to run the program?

Build the Docker images and then run it with
```
docker-compose -f <FILE> build && docker-compose -f <FILE> up
```

We have both a development and a production version pre-configured.

## Environment variables
File `docker-compose.yml` accepts a number of parameters, which you can tune to
better suit the tool to your needs. These parameters mirror the CLI options for
the program, and further documentation can be found there. If you do not know
what to use, leave the options as they are. Removing the environment variables from 
`docker-compose.yml` will break the program.

```
usage: scraper.py [-h] [--scroll_limit SCROLL_LIMIT] [--timeout TIMEOUT]
                  [--scraping_interval SCRAPING_INTERVAL]
                  [--db_conn_string DB_CONN_STRING]
                  [--logging {0,10,20,30,40,50}] [--keep_logs]
                  [--slow_mode] [--quiet]
                  account_name {TikTok,YouTube}
```

## Debugging the GUI

In addition to these arguments, we have one extra which can be used to restrict access
to the graphic terminal in development mode:
- `VNC_SERVER_PASSWORD` is optional unless you are using Mac. If you are on
Ubuntu and leave this empty, then the VNC will simply be accessible without
a password.

The Docker command will open a container whose port 5900 is connected to port 5900 of the host. If you want to check out what is going on inside the container, you can connect to the GUI with a client such as `vncviewer`, like so: `vncviewer localhost:5900`.

After a bunch of stuff related to GUI is printed, you should see something like that:

![](docs/img/scraper.png)