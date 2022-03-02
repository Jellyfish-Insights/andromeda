FROM debian:11 AS scraper_prod
ENV SCRAPER_ENV=PRODUCTION

# For avoiding prompts
ENV DEBIAN_FRONTEND=noninteractive

# For handling host UID != 1000
ARG APP_UID=${APP_UID:-1000}
ENV APP_UID=${APP_UID}

# Create paths scraper will use
RUN mkdir -p /opt/scraper/ \
	&& mkdir -p /home/app \
	&& mkdir -p /var/log/scraper \
# Create user and give them permission to these paths
	&& useradd app -u ${APP_UID} --no-log-init \
	&& chown -R app:app /opt/scraper \
    && chown -v -R app:app /home/app \
	&& chown -R app:app /var/log/scraper

WORKDIR /opt/scraper
COPY --chown=app:app ["python_requirements.txt", "./"]

RUN apt-get update \
	&& apt-get upgrade -y \
	&& apt-get install -y \
		# Basic tools we will need for everything else
		gpg \
		procps \
		# necessary to include python3 by itself - because we will remove pip
		python3 \
		python3-pip \
		unzip \
		wget \
		# For running the GUI
		fluxbox \
		wmctrl \
		xvfb \
		# We will need this for Chrome + Undetected Chrome
		default-jre
# Install Chrome
RUN echo 'deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main' \
		> /etc/apt/sources.list.d/google-chrome.list \
	&& wget -O- https://dl.google.com/linux/linux_signing_key.pub 2> /dev/null \
		| gpg --dearmor > /etc/apt/trusted.gpg.d/google.gpg \
	&& apt-get update \
	&& apt-get install -y google-chrome-beta
# Install necessary Python libraries
RUN python3 -m pip install --no-cache-dir -r ./python_requirements.txt \
	&& rm ./python_requirements.txt
# Install BrowserMobProxy binaries
RUN wget https://github.com/lightbody/browsermob-proxy/releases/download/browsermob-proxy-2.1.4/browsermob-proxy-2.1.4-bin.zip \
		-O browsermob.zip 2> /dev/null \
	&& unzip browsermob.zip -d /opt/ \
	&& chown -R app:app /opt/browsermob-proxy-2.1.4 \
	&& rm browsermob.zip
# Make it as lightweight as possible
RUN	rm -rf tests/*
# wget cannot be removed, as chrome depends on it
RUN apt-get remove -y \
		gpg \
		perl \
		python3-pip \
		unzip \
	&& apt-get autoremove -y \
	&& apt-get clean \
	&& rm -rf /var/lib/apt/lists/*

COPY --chown=app:app ["src/", "bootstrap.sh", "appsettings.json", "./"]

CMD [ "./bootstrap.sh" ]

FROM scraper_prod AS scraper_dev
ENV SCRAPER_ENV=DEVELOPMENT

# Put tests back
COPY --chown=app:app ["src/tests/", "./tests"]

# Install debugging features
RUN apt-get update \
	&& apt-get install -y \
		curl \
		htop \
		less \
		make \
		sudo \
		vim \
		x11vnc \
	&& apt-get autoremove -y \
	&& apt-get clean \
	&& usermod -aG sudo app \
	&& echo "123456\n123456" \
		| passwd app

CMD [ "./bootstrap.sh" ]

