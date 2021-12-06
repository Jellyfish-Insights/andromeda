import time
import pyautogui
import webbrowser
import os
import matplotlib.pyplot as plt

class YouTubeLogin:

    def __init__(self, email, password, profile_directory):
        self.email = email
        self.password = password
        self.profile_directory = profile_directory
        self.target_url = 'https://www.youtube.com/'
        self.img_path = f"resources{os.path.sep}img{os.path.sep}"

    def get_browser_instance(self):
        mozilla = webbrowser.Mozilla()
        mozilla.name = 'firefox'
        mozilla.remote_args = [
            '-profile',
            self.profile_directory,
            '-height',
            '1080',
            '-width',
            '1920'
            '-new-instance',
            '-no-remote',
            '-url',
            '%s',
        ]
        return mozilla

    def sign_in_button(self):
        sign_in = pyautogui.locateCenterOnScreen(f"{self.img_path}sign_in_button.png")
        pyautogui.moveTo(sign_in)
        time.sleep(2)
        pyautogui.click()

    def send_email(self):
        time.sleep(2)
        pyautogui.write(self.email)
        next = pyautogui.locateCenterOnScreen(f"{self.img_path}next_blue_button.png")
        pyautogui.moveTo(next)
        time.sleep(2)
        pyautogui.click()

    def send_password(self):
        time.sleep(2)
        pyautogui.write(self.password)
        next = pyautogui.locateCenterOnScreen(f"{self.img_path}next_blue_button.png")
        pyautogui.moveTo(next)
        time.sleep(2)
        pyautogui.click()

    def login(self):
            mozilla = self.get_browser_instance()
            mozilla.open(self.target_url)
            time.sleep(2)

            # find the login button
            self.sign_in_button()

            # send the email and click on NEXT
            self.send_email()

            # send the password and click on NEXT
            self.send_password()

def main():

    profile_directory = f"{os.getcwd()}{os.path.sep}profiles{os.path.sep}YouTube"
    youtube = YouTubeLogin(
        email='email_account_here',
        password='password_here',
        profile_directory=profile_directory
    )
    youtube.login()

if __name__ == '__main__':
    main()
