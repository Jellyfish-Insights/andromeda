# How to run python script to get credentials

# Introduction
This document intends to help you with the python script to get credentials. The process to set-up credentials for Andromeda can be difficult, these scripts will help you to start to set-up your envoriment.

Keep in mind that you will need a inital set-up to use these scripts.

## Credentials structure

This script will create the following structure. As you can see, this will be the same structure of the {How to run the container}, so the credentials used here can be used to run Andromeda into your container.

```
credentials
│
└─ facebook
| |  user_credentials.json
| |
│ └── adaccount
| |    adaccount-id1_credentials.json
| |    adaccount-id2_credentials.json
│ └── page
│      page-id1_credentials.json
│      page-id2_credentials.json
|
└─ youtube
│ |  client_secret.json
| |
| └── channel_1
│ |    Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json
| |
| └── channel_2
│      Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json
|
└─ adwords
|
└─ instagram
    instagram-id1_credentials.json
```

## Python dependencies 

To use these scripts you need to have Python install on your computer and also the following libraries:

- https://pypi.org/project/requests/
- https://pypi.org/project/google-auth-oauthlib/

## Initial configuration

There are some configuration needed before you start. Following you can find the instructions to set-up your envoriment and get your credentials.

### Create a Facebook App and get Inital Tokens

To start to use this script you need to create a Facebook Application.
- Go to https://developers.facebook.com/ -> My Apps -> Create App
- Go to **Settings** -> **Basic** 
   - Copy your `App ID` and `App Secret` and paste on the file `utilities/fb_client_secrets.json` 

![Facebook App ID](../assets/doc-facebook-ap.gif)

```bash
vim fb_client_secrets.json
```
```json
{
    "APP_ID" : "<APP_ID>",
    "APP_SECRET" : "<APP_SECRET>",
    "USER_ACCESS_TOKEN" : "<USER_ACCESS_TOKEN>"
}
```
- Save and close the file.
- Go to **Tools** -> **Graph API Explorer**
   - Click on **Get Token** -> **Get User Access Token** and make your log-in.
   - Click on **Add a Permission** and select the fields:
      - ads_management
      - ads_read
      - business_management
      - manage_pages
      - pages_show_list
      - read_insights
      - instagram_basic
      - instagram_manage_insights
      - instagram_manage_comments

<div align="center">
    </br>
    <img src="../assets/doc-facebook-user.gif" width="50%" height="50%">
    </br></br>
</div>

- Click on **Get Access Token** and select the Instagram business account and Pages wanted.
    - Copy your `Access Token` and paste on the file `utilities/fb_client_secrets.json` 

<div align="center">
    </br>
    <img src="../assets/doc-facebook-user-token.gif" width="50%" height="50%">
    </br></br>
</div>

In the end of this process you would have something like:

- Folder structure
```
utilities
|  facebook_credentials.py
|  fb_client_secret.json
|  youtube_credentials.py
```
- `fb_client_secrets.json` file
```json
{
    "APP_ID" : "1303879906465623",
    "APP_SECRET" : "34c9a9317163c41bcc4cfea19195b9cf",
    "USER_ACCESS_TOKEN" : "EAASh3zKnl1cBALiuftI***************************************************"
}
```

### Running the Facebook script

After follow the previous steps you will be able to run the script.

```bash
cd utilities
python3 facebook_credentials.py
```
<div align="center">
    </br>
    <img src="../assets/doc-facebook-script.gif" width="75%" height="75%">
    </br></br>
</div>

This command will create all necessary crendentials to fetch your data from Facebook API.

- Folder structure
```
utilities
|  facebook_credentials.py
|  fb_client_secret.json
|  youtube_credentials.py
|
└─ credentials
  |
  └─ facebook
  | |  user_credentials.json
  | |
  │ └── adaccount
  | |    adaccount-id1_credentials.json
  | |    adaccount-id2_credentials.json
  │ └── page
  │      page-id1_credentials.json
  │      page-id2_credentials.json
  |
  └─ youtube
  |
  |
  └─ adwords
  |
  └─ instagram
      instagram-id1_credentials.json
```

This folder is ready to run on Andromeda container, you now need to copy and paste on the folder `andromeda-config`.

With these credentials you will be able to Fetch your **Facebook** data, following we will explain how to get your youtube credentials.

### Create a Google Project and get Inital Tokens

To start to use this script you need to create a client_secret.json from https://console.developers.google.com/.

This article have a brief explanation about getting the client_secret.json, we recommend that you follow the steps described there and come back here after have your token ready. 

- [Fetching YouTube Data using Andromeda](https://medium.com/@insightsjellyfish/fetching-youtube-data-using-andromeda-8f1b1240803c)

**Note. You don't need to run all the steps, once you have your client_secret.json file you can come back and run the script**

After getting your file, paste it into the `utilities` folder. At this moment the folder will be looking like:

```
utilities
|  facebook_credentials.py
|  fb_client_secret.json
|  youtube_credentials.py
|  client_secret.json
|
└─ credentials
  |
  └─ facebook
  | |  user_credentials.json
  | |
  │ └── adaccount
  | |    adaccount-id1_credentials.json
  | |    adaccount-id2_credentials.json
  │ └── page
  │      page-id1_credentials.json
  │      page-id2_credentials.json
  |
  └─ youtube
  |
  |
  └─ adwords
  |
  └─ instagram
      instagram-id1_credentials.json
```

### Running the Youtube script

After follow the previous steps you will be able to run the script. This script will open a screen for you make your login to the google account related to the Youtube Channel that you want to Fetch data.

```bash
cd utilities
python3 youtube_credentials.py
```
<div align="center">
    </br>
    <img src="../assets/doc-youtube-script.gif" width="75%" height="75%">
    </br></br>
</div>

### Final folder structure

```
utilities
|  facebook_credentials.py
|  fb_client_secret.json
|  youtube_credentials.py
|  client_secret.json
|
└─ credentials
  |
  └─ facebook
  | |  user_credentials.json
  | |
  │ └── adaccount
  | |    adaccount-id1_credentials.json
  | |    adaccount-id2_credentials.json
  │ └── page
  │      page-id1_credentials.json
  │      page-id2_credentials.json
  |
  └─ youtube
  |  client_secret.json
  |
  └── channel_1
  |    Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json
  └── channel_2
  |    Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json
  |
  └─ adwords
  |
  └─ instagram
      instagram-id1_credentials.json
```

Now you have the credentials for multiple Youtube Channels and also for multiple Facebook Pages, these credentials will be used to Fetch data when you use the **Andromeda**. Following the tutorial [How to run Andromeda in a docker container](https://github.com/Jellyfish-Insights/andromeda/blob/master/docs/docker_container.md) at some moment you will need to paste on the `andromeda-config`. We created this scripts to make your life easier and prepare the folders for you.