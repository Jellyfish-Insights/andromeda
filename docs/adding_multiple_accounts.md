# How to fetch data from multiple YouTube and Facebook accounts

This document explains how to set up Andromeda to fetch data from
multiple YouTube channels and/or Facebook accounts.

### Multiple Facebook/Instagram accounts

Andromeda will be looking for files named as `<name>_credentials.json` into the folders:   
- `credentials/facebook/page`
- `credentials/facebook/adaccount`
- `credentials/instagram`

For instance, when we want to add a new Facebook Page the process will be:
- Get Facebook Page access_token an page_id. ([How to get Andromeda credentials](link))
- Create the file `<page_id>_credentials.json` into the folder `credentials/facebook/page`.
   - File structure `100679434865960_credentials.json`:
   ```json
   {
        "name": "Mock Page - Jellyfish",
        "id": "100679434865960",
        "token": "EAAOSHRq0hqABAItrHYDGmA8VT7aU******************************************************************"
    }
   ```
- Save the file

The same process works for the Adaccount and Instagram, what will change is the folder where will you 
create the JSON file and the process to take the credential.

### Multiple YouTube channels

For YouTube channel, the Andromeda will be looking for empty folders inside the `credentials/youtube`.
So, to add a new YouTube Channel the process will be:  

- Create the folder
```bash
cd Andromeda.ConsoleApp/redentials/youtube
mkdir channel_1
mkdir channel_2
```
- Run the Fetcher
```bash
dotnet run -- fetcher -s YouTube
```
The system will open the browser to make a login on your Google Account.
- Login your Google account for channel_1
For each folder that you created, the system will ask for a new login.
- Log-in your Google account for channel_2

This process will create save your credentials into the created folders (e.g. channel_1, channel_2) and
on the next time that you run the system won't be needed to make any login.
