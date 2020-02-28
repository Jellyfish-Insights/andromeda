# How to fetch data from multiple YouTube and Facebook accounts

This document explains how to set up Andromeda to fetch data from
multiple Facebook accounts and/or YouTube channels.

## Multiple Facebook/Instagram accounts

The process for setting up Andromeda to fetch data from multiple Facebook accounts is very simple. Just get the Facebook Page, Facebook Marketing or Instagram credentials for every account that you want to pull data and place them the respective folders:

- `credentials/facebook/page`
- `credentials/facebook/adaccount`
- `credentials/instagram`

Also, every credential file has to follow this name convention: `<some_name>_credentials.json`.

For instance, if we want to fetch data from three Facebook Pages, our `credentials/facebook/page` folder will be like this:

```
credentials/facebook/page
|
└──100679434865960_credentials.json
|
└──100239433455999_credentials.json
|
└──100434960865679_credentials.json
```

You can get this `<name>_credentials.json` files by running Andromeda python scripts for every Facebook account that you want to fetch data from. The process of getting the Facebook credentials using the python scripts can be seen [here](./run_credentials_script.md#Facebook/Instagram-Credentials)

The process is almost the same for adding multiple Adaccount and Instagram accounts, what will change is the folder where you will place the credentials files.

## Multiple YouTube channels

The process for setting up Andromeda to fetch data from multiple YouTube channels is also very simple. Just add the credentials file for each YouTube channel that you want to pull data inside separated folders the `credentials/youtube` directory.

For instance, if we want to pull data from three different YouTube channel our `credentials/youtube` folder will be like this:

```
credentials/youtube
|
└──channel_1
|    Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json
|
└──channel_2
|    Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json
|
└──channel_3
     Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json
```

You can get the `Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json` files by running Andromeda python scripts for every YouTube account that you want to fetch data from. The process of getting the YouTube channel credentials using the python scripts can be seen [here](./run_credentials_script.md#YouTube-Credentials)

Another alternative, if you are not running Andromeda on the containers or in a server is to use the Andromeda CLI.

### Getting YouTube credentials using Andromeda CLI

Open a terminal on the root directory of your andromeda folder, then execute the following commands:

```bash
cd Andromeda.ConsoleApp/credentials/youtube
mkdir channel_1
mkdir channel_2
```

In sequence run the YouTube Fetchers

```bash
cd ../..
dotnet run -- fetcher -s YouTube
```

For each folder that you created, the Andromeda will open the browser window and will ask for you to make login on the Google account that you want to get the credentials.

This process will save your credentials into the created folders (e.g. channel_1, channel_2).
