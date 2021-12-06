import os
import pyautogui
import subprocess

class FirefoxProfile:
    def __init__(self, profile_path):
        self.profile_path = profile_path
        self.img_path = f"resources{os.path.sep}img{os.path.sep}"

    def delete_old_profile_folder(self):
        subprocess.run(['rm', '-rf', self.profile_path])

    def create_new_profile_folder(self):
        os.mkdir(path=self.profile_path)

    def run_profile_manager(self):
        self.firefox_process = subprocess.Popen(
            args=['firefox', '-P'],
            start_new_session=True
        )
        pyautogui.sleep(5)

    def remove_old_profile(self):
        youtube = pyautogui.locateCenterOnScreen(f"{self.img_path}profile_name.png")
        if youtube is not None:
            pyautogui.moveTo(youtube)
            pyautogui.click()
            pyautogui.sleep(1)
            delete_profile = pyautogui.locateCenterOnScreen(f"{self.img_path}delete_profile_white_button.png")
            pyautogui.moveTo(delete_profile)
            pyautogui.click()
            pyautogui.sleep(1)
            delete_files = pyautogui.locateCenterOnScreen(f"{self.img_path}delete_files_white_button.png")
            pyautogui.moveTo(delete_files)
            pyautogui.click()
            pyautogui.sleep(3)

    def create_new_profile(self):
        sign_in = pyautogui.locateCenterOnScreen(f"{self.img_path}create_new_profile.png")
        pyautogui.moveTo(sign_in)
        pyautogui.click()
        pyautogui.sleep(1)

        next = pyautogui.locateCenterOnScreen(f"{self.img_path}next_white_button.png")
        pyautogui.moveTo(next)
        pyautogui.click()
        pyautogui.sleep(1)

        pyautogui.write('YouTube')
        pyautogui.sleep(2)

        choose = pyautogui.locateCenterOnScreen(f"{self.img_path}choose_folder_button.png")
        pyautogui.moveTo(choose)
        pyautogui.click()
        pyautogui.sleep(1)

        pyautogui.keyDown('ctrl')
        pyautogui.press('l')
        pyautogui.keyUp('ctrl')
        pyautogui.sleep(1)

        pyautogui.write(self.profile_path)
        pyautogui.sleep(2)
        pyautogui.press('enter')
        pyautogui.sleep(1)

        finish = pyautogui.locateCenterOnScreen(f"{self.img_path}finish_white_button.png")
        pyautogui.sleep(1)
        pyautogui.moveTo(finish)
        pyautogui.click()
        pyautogui.sleep(1)

        pyautogui.hotkey('alt', 'f4')

    def create(self):
        self.delete_old_profile_folder()
        self.create_new_profile_folder()
        self.run_profile_manager()
        self.remove_old_profile()
        self.create_new_profile()

def main():
    profile = FirefoxProfile(profile_path=f"{os.getcwd()}{os.path.sep}profiles{os.path.sep}YouTube")
    profile.create()

if __name__ == '__main__':
    main()
