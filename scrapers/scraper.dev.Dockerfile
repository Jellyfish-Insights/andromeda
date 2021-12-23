# FROM ubuntu:20.04
FROM debian:11

# For avoiding prompts
ENV DEBIAN_FRONTEND=noninteractive

# Get repository sources
RUN apt-get update \
	&& apt-get install -y \
		# Basic tools we will need for everything else
		bash procps wget python3-pip \
		# For running the GUI
		xvfb fluxbox wmctrl \
		# For debugging the GUI
		x11vnc \
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

# For development and debugging -- will be excluded in production Dockerfile
RUN apt-get install -y less vim curl

# Copy our app and required binaries for Undetected Chrome Driver
RUN mkdir -p /opt/undetected-tiktok/ \
	&& mkdir -p /opt/browsermob-proxy-2.1.4/

COPY src/ /opt/undetected-tiktok/
COPY browsermob-proxy-2.1.4/ /opt/browsermob-proxy-2.1.4/

# Copy starting script
COPY bootstrap.sh /

# Add regular user, give them ownership of binaries
RUN useradd apps \
    && mkdir -p /home/apps \
    && chown -v -R apps:apps /home/apps \
	&& usermod -aG sudo apps \
	&& chown -R apps:apps /opt/undetected-tiktok/

# Make regular user able to write log files
RUN chmod a+w /var/log/

CMD [ "/bootstrap.sh" ]