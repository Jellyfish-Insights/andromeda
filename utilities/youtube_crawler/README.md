# YouTube Studio Crawler

## Instructions

1. Install the dependencies using pip3

        pip3 install -r requirements.txt

2. Create the required directories: 

        mkdir drivers profiles logs

3. Download the gecko driver: 

        cd drivers
        wget https://github.com/mozilla/geckodriver/releases/download/v0.30.0/geckodriver-v0.30.0-linux32.tar.gz
        tar -xf geckodriver-v0.30.0-linux32.tar.gz
        rm geckodriver-v0.30.0-linux32.tar.gz

4. Run the Python script to create the firefox profile (from the root folder):

        python3 src/firefox_profile.py

5. Open firefox and access `about:profiles`. After that, use the button to launch Firefox using the new YouTube profile.

7. Open the `src/login.py` script and update the credentials (inside the main function).

        youtube = YouTubeLogin(
                email='email_account_here',
                password='password_here',
                profile_directory=profile_directory
        )

8. Run the Python script to login to the desired Google Account. Notice that google will ask you to confirm the login from your smartphone:

        python3 src/login.py

9. Open the `src/youtube.py` script and update the video list at line 139, inside the `main` function. These ids should come from your youtube account.

        videos = ['your_video_id', 'another_video_id' ]    

10. Run the Python script to get the video statistics:

        python3 src/youtube.py

11. See the resulting json files inside the `logs` folder.

