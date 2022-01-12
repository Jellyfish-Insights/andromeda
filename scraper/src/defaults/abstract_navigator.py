# CONSTANTS FOR COLLECTING DATA

# I am not sure we need to capture headers, but still haven't tried to
# remove this option
HAR_OPTIONS = {
	'captureHeaders': True,
	'captureContent': True
}

# CONSTANTS FOR NAVIGATION

WAIT_RANDOM_FACTOR = 0.15

MIN_AMOUNT_OF_SCROLLING = 200
SHORT_PAUSE_LENGTH = 1.0
LONG_PAUSE_LENGTH = 5.0
LONG_PAUSE_PROBABILITY = 0.10
SLOW_MODE_MULTIPLIER = 2.0

WAIT_UNTIL_TIMEOUT = 10.0
POLL_FREQUENCY = 0.5

SLOW_TYPE_SLEEP_INTERVAL = 0.15

MOVE_AROUND_MOVE_MOUSE_TIMES = 5
MOVE_AROUND_VISIT_LINK_PROB = 0.05

# For performing a sequence of actions as a single block (avoids overhead)
BATCH_ACTION_SIZE = 10