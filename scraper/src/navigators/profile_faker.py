"""Use this to visit a bunch of websites and get a more credible user profile"""
import random
from typing import Any, Dict
from scraper_middleware import ScraperMiddleWare
from defaults.profile_faker import NAVIGATOR_DEFAULT_OPTIONS
from logger import log

class ProfileFaker(ScraperMiddleWare):
	needs_authentication = True
	navigator_default_options: Dict[str, Any] = NAVIGATOR_DEFAULT_OPTIONS
	
	############################################################################
	# METHODS
	############################################################################
	def main(self):
		just_pass_by = set([
			"https://www.instagram.com/",
			"https://twitter.com/",
			"https://web.whatsapp.com/",
			"https://www.netflix.com/",
			"https://www.office.com/",
			"https://zoom.us/",
			"https://discord.com/"
		])
		stay_for_a_while = set([
			"https://www.yahoo.com/",
			"https://www.youtube.com/",
			"https://www.amazon.com",
			"https://www.ebay.com/",
			"https://www.reddit.com/",
			"https://www.nytimes.com/",
			"https://www.globo.com/",
			"https://en.wikipedia.org/wiki/Main_Page",
			"https://yandex.ru/",
			"https://9gag.com/",
			"https://www.twitch.tv/",
			"https://www.walmart.com/",
			"https://www.bbc.com/"
		])
		all_websites_random_order = list(just_pass_by | stay_for_a_while)
		random.shuffle(all_websites_random_order)
		all_websites = set(all_websites_random_order)
		
		log.info("In order to create a more credible browser fingerprint, we "
			"will be accessing some of the following websites:")
		log.info(all_websites)

		for website in all_websites:
			self.go(website)
			if website in just_pass_by:
				self.move_aimlessly(timeout=15)
			else:
				self.move_aimlessly(timeout=180)
			