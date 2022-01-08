FROM debian:11 AS scraper_prod

# For avoiding prompts
ENV DEBIAN_FRONTEND=noninteractive

# Get repository sources
RUN apt-get update \
	&& apt-get install -y \
		# Basic tools we will need for everything else
		bash unzip procps wget python3-pip \
		# For running the GUI
		xvfb fluxbox wmctrl \
		# We will need these for Chrome + Undetected Chrome
		default-jre 

# Install Chrome
RUN echo 'deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main' \
		> /etc/apt/sources.list.d/google-chrome.list \
	&& wget -O- https://dl.google.com/linux/linux_signing_key.pub 2> /dev/null \
		| gpg --dearmor > /etc/apt/trusted.gpg.d/google.gpg \
	&& apt-get update \
	&& apt-get install -y google-chrome-stable

# Install necessary Python libraries
COPY python_requirements.txt /tmp/
RUN python3 -m pip install -r /tmp/python_requirements.txt \
	&& rm /tmp/python_requirements.txt

# Install BrowserMobProxy binaries
RUN wget https://github.com/lightbody/browsermob-proxy/releases/download/browsermob-proxy-2.1.4/browsermob-proxy-2.1.4-bin.zip \
		-O browsermob.zip 2> /dev/null \
	&& unzip browsermob.zip -d /opt/ \
	&& rm browsermob.zip

RUN mkdir -p /opt/scraper/
COPY src/ /opt/scraper/

COPY bootstrap.sh /

# Add regular user, give them ownership of binaries
ARG APP_UID=${APP_UID:-1000}
ENV APP_UID=${APP_UID}
RUN useradd app -u ${APP_UID} \
    && mkdir -p /home/app \
    && chown -v -R app:app /home/app \
	&& mkdir -p /var/log/scraper/ \
	&& chown -R app:app /var/log/ \
	&& chown -R app:app /opt/

# Make it more lightweight by removing tests
RUN rm -r /opt/scraper/tests

CMD [ "/bootstrap.sh" ]

FROM scraper_prod AS scraper_dev

# Put tests back
COPY src/tests/ /opt/scraper/tests

# Install debugging features
RUN apt-get install -y make x11vnc less vim curl sudo htop
RUN usermod -aG sudo app && \
	echo "123456\n123456" | passwd app

# Install test data
RUN cd /opt/scraper/ && \
	python3 -m tests.test_tiktok_most_followed

CMD [ "/bootstrap.sh" ]