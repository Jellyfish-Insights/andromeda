from sys import stderr, stdin
import subprocess
from tkinter.constants import NW
import numpy as np
import time

import pyautogui
import json
from selenium import webdriver
from selenium.common.exceptions import InvalidArgumentException
from selenium.webdriver.common.desired_capabilities import DesiredCapabilities

from tkinter import Tk

import os

class FirefoxDriverFactory:
    def __init__(self, geckodriver, profile_path):
        self.geckodriver = geckodriver
        self.profile_path = profile_path

    def build(self):
        options = webdriver.FirefoxOptions()
        options.add_argument('--start-maximized')
        options.add_argument('--disable-web-security')
        options.add_argument('--allow-running-insecure-content')
        options.add_argument(self.profile_path)
        options.add_argument("--disable-blink-features=AutomationControlled")

        profile = webdriver.FirefoxProfile(self.profile_path)
        profile.set_preference("dom.webdriver.enabled", False)
        profile.set_preference('useAutomationExtension', False)
        profile.set_preference('devtools.jsonview.enabled', False)
        profile.update_preferences()
        desired = DesiredCapabilities.FIREFOX

        driver = webdriver.Firefox(
            options=options,
            firefox_profile=profile,
            desired_capabilities=desired,
            executable_path=self.geckodriver
        )

        driver.set_window_position(0, 0)
        driver.set_window_size(1920, 1080)

        return driver

class YouTubeStudioExtractor:
    def __init__(self, driver, raw_request, credentials):
        self.img_path = f"resources{os.path.sep}img{os.path.sep}"
        self.driver : webdriver.Firefox = driver
        self.raw_request = raw_request
        self.email = credentials.get('email')
        self.password = credentials.get('password')

    def extract_clipboard(self):
        tk = Tk()
        return tk.clipboard_get()

    def save_content(self, content : str, video_id : str):
        if 0 == len(content):
            raise InvalidArgumentException(msg='The content is empty')
        with open(file=self.raw_request, mode='w') as f:
            template = '{{ video }}'
            f.write(f"{content.replace(video_id, template)}\n")

    def start(self, video):

        # waiter = WebDriverWait(driver=self.driver, timeout=3)
        self.driver.get('about:blank')
        pyautogui.sleep(2)
        pyautogui.press('f12')
        pyautogui.sleep(2)

        self.driver.get(f"https://studio.youtube.com/video/{video}/analytics/tab-reach_viewers/period-lifetime")
        pyautogui.sleep(6)

        # expand menu
        get_screen = pyautogui.locateCenterOnScreen(f"{self.img_path}get_screen_gray.png")
        pyautogui.moveTo(get_screen)
        pyautogui.sleep(1)
        pyautogui.rightClick()

        # open submenu
        copy_menu = pyautogui.locateCenterOnScreen(f"{self.img_path}copy_request_white_menu.png")
        pyautogui.moveTo(copy_menu)
        pyautogui.sleep(2)
        pyautogui.click()

        # confirm curl
        copy_curl = pyautogui.locateCenterOnScreen(f"{self.img_path}copy_as_curl_white.png")
        pyautogui.moveTo(copy_curl)
        pyautogui.sleep(2)
        pyautogui.click()

        print("================================================ B")
        self.save_content(content=self.extract_clipboard(), video_id=video)
        self.driver.close()

class YouTubeStudioCrawler:
    def __init__(self, raw_request):
        self.raw_request = raw_request

    def start(self, videos):

        with open(file=self.raw_request, mode='r') as f:
            line = f.read()
            statistics = []
            for video in videos:
                new_line = line.replace('{{ video }}', video)
                with open(file='tmp.sh', mode='w') as h:
                    h.write('curl ' + new_line + '\n')
                with open(file=f"logs{os.path.sep}{video}.json", mode='w') as g:
                    initial = time.time()
                    result = subprocess.check_output(['/bin/bash', 'tmp.sh'])
                    now = time.time()
                    statistics.append(now - initial)
                    if result:
                        g.write(f"{json.dumps(json.loads(result), indent=2)}")
            print('====================================================================')
            print(np.average(np.array(statistics)))
        os.remove(path='tmp.sh')

def main():
    fact = FirefoxDriverFactory(
        geckodriver=f"{os.getcwd()}{os.path.sep}drivers{os.path.sep}geckodriver",
        profile_path=f"{os.getcwd()}{os.path.sep}profiles{os.path.sep}YouTube"
    )

    raw_request = 'raw_request.txt'

    # TODO get the encryptied versions of the email and password from the database,
    # TODO decrypt them and use here
    extractor = YouTubeStudioExtractor(driver=fact.build(), raw_request=raw_request, credentials={ 
        'email': 'your_email_account',
        'password': 'the_account_password'
    })

    extractor.start(video='EcyTxEO0qKI')

    # TODO get the video list from the database
    videos = [
        'Gkuecw-AA2Q',
        'EcyTxEO0qKI',
        'HkK49oM-BMM',
        'ApuyMcjOue8',
        '2Ban5vAyn3g',
        'KEx9oh11Hoc',
        'DeQUo6So7p4',
        'GOW4cX4BUpY',
        'TkxvhRepldo',
        'rt0-lW0nuug',
        'k7-E7o93q34'
    ]

    crawler = YouTubeStudioCrawler(raw_request=raw_request)
    crawler.start(videos=videos)

if __name__ == "__main__":
    main()
