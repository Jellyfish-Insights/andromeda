# Log file
UC_LOG_FILE = "uc.log"

# Options for our ChromeDriver
PROFILE_DIR = "chrome_profile"

CHROMEDRIVER_OPTIONS = [
	# Hopefully, using the settings below will disable other popups that could
	# disturb complete browser automation
	"--no-first-run",
	"--no-service-autorun",
	"--password-store=basic",
	"--disable-notifications",

	# For ignoring "untrusted proxy" errors
	"--ignore-ssl-errors=yes",
	"--ignore-certificate-errors",

	# For disabling /dev/shm usage (Docker Containers don't allocate a lot of
	# memory for that)
	"--disable-dev-shm-usage",

	# For disabling cache
	"--disk-cache-size=0",
]

CHROMEDRIVER_SERVICEARGS = ["--verbose", f"--log-path={UC_LOG_FILE}"]

# An exception is triggered and driver exits if page takes longer than that
# to load (given in seconds)
PAGE_LOAD_TIMEOUT = 30