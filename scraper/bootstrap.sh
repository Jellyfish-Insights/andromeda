#!/usr/bin/env bash

# Credits for this file go to https://medium.com/dot-debug/running-chrome-in-a-docker-container-a55e7f4da4a8
# Based on: http://www.richud.com/wiki/Ubuntu_Fluxbox_GUI_with_x11vnc_and_Xvfb

# This script should NOT be run as root, as it is not advisable to run Chrome
# with root privileges.

main() {
    log_i "Starting xvfb virtual display..."
    launch_xvfb
    log_i "Starting window manager..."
    launch_window_manager

	if [ "$development" == "development" ] ; then
		log_i "Starting VNC server..."
		run_vnc_server
	fi

	############################################################################
	# Run our app
	############################################################################
	directory="/opt/scraper/"
	scheduler_script="scripts/scheduler.py"

	log_i "Sleeping for 10 seconds to allow Postgres to init"
	sleep 10

	cd "$directory"
	if [ -d chrome_profile ] ; then
		rm -rf chrome_profile
	fi

	# This needs to go unquoted in the command
	unquoted="$random_order"

	while true ; do
		log_i "Running scheduler script..."
		python3 -m scripts.scheduler \
			--sleep_interval "$sleep_interval" \
			$unquoted
		sleep 10
	done
}

launch_xvfb() {
    local xvfbLockFilePath="/tmp/.X1-lock"
    if [ -f "${xvfbLockFilePath}" ]
    then
        log_i "Removing xvfb lock file '${xvfbLockFilePath}'..."
        if ! rm -v "${xvfbLockFilePath}"
        then
            log_e "Failed to remove xvfb lock file"
            exit 1
        fi
    fi

    # Set defaults if the user did not specify envs.
    export DISPLAY=${XVFB_DISPLAY:-:1}
    local screen=${XVFB_SCREEN:-0}
    local resolution=${XVFB_RESOLUTION:-1280x960x24}
    local timeout=${XVFB_TIMEOUT:-5}

    # Start and wait for either Xvfb to be fully up or we hit the timeout.
    Xvfb ${DISPLAY} -screen ${screen} ${resolution} \
		>> /var/log/xvfb.log \
		2>> /var/log/xvfb.error &
    local loopCount=0
    until xdpyinfo -display ${DISPLAY} > /dev/null 2>&1
    do
        loopCount=$((loopCount+1))
        sleep 1
        if [ ${loopCount} -gt ${timeout} ]
        then
            log_e "xvfb failed to start"
            exit 1
        fi
    done
}

launch_window_manager() {
    local timeout=${XVFB_TIMEOUT:-5}

    # Start and wait for either fluxbox to be fully up or we hit the timeout.
    fluxbox \
		>> /var/log/fluxbox.log \
		2>> /var/log/fluxbox.error &
    local loopCount=0
    until wmctrl -m > /dev/null 2>&1
    do
        loopCount=$((loopCount+1))
        sleep 1
        if [ ${loopCount} -gt ${timeout} ]
        then
            log_e "fluxbox failed to start"
            exit 1
        fi
    done
}

run_vnc_server() {
    local passwordArgument='-nopw'

    if [ -n "${VNC_SERVER_PASSWORD}" ]
    then
        local passwordFilePath="${HOME}/.x11vnc.pass"
        if ! x11vnc -storepasswd "${VNC_SERVER_PASSWORD}" "${passwordFilePath}"
        then
            log_e "Failed to store x11vnc password"
            exit 1
        fi
        passwordArgument=-"-rfbauth ${passwordFilePath}"
        log_i "The VNC server will ask for a password"
    else
        log_w "The VNC server will NOT ask for a password"
    fi

    x11vnc -display ${DISPLAY} -forever ${passwordArgument} \
		>> /var/log/x11vnc.log \
		2>> /var/log/x11vnc.error &
	
}

log_i() {
    log "[INFO] ${@}"
}

log_w() {
    log "[WARN] ${@}"
}

log_e() {
    log "[ERROR] ${@}"
}

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] [BOOTSTRAP] ${@}"
}

control_c() {
    echo "Interrupt received... exiting... :("
    exit
}

trap control_c SIGINT SIGTERM SIGHUP

main

exit
