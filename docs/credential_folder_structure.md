# Creating the folder structure

In order to run the Andromeda container, we need to create a folder containing at least one social media credential and the configuration file `appsettings.json` . The file `appsettings.json` is the file that contains additional information needed by andromeda to run, like database information and it needs to be in the root of the created directory. Each credential file needs to be in a folder with the name of the social media that it belongs to. The following steps demonstrate how to create the folder structure and how to place correctly the necessary files.
To create all the folders needed by Andromeda, open a terminal window and run the following command:

**Note:** All occurrences of `<ANDROMEDA_USER_FOLDER>` should be changed to the name of your user folder

``` bash
mkdir andromeda-config && cd andromeda-config && mkdir <ANDROMEDA_USER_FOLDER> && cd <ANDROMEDA_USER_FOLDER> && mkdir adwords facebook facebook/adaccount facebook/page instagram youtube && cd ../..
```

In sequence, copy the credential files from each social media to their respective directories inside the `andromeda-config` folder. For instance, if you have all the credentials from the social media that Andromeda supports and you are not using multi-accounts, your folder structure should be the following:

```
andromeda-config
└─appsettings.json
│
│
└─<ANDROMEDA_USER_FOLDER>
│  └─facebook
│  | └─user_credentials.json
│  | |
│  │ └──adaccount
│  | |  └─user1-adaccount_credentials.json
│  │ └──page
│  │    └─user1-page_credentials.json
│  |
│  └─youtube
│  | |
│  | └──channel1_name
│  │    └─Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json
│  │
│  └─adwords
│  | └─App.config
│  |
│  └─instagram
│    └─user1-instagram_credentials.json
└─client_secret.json
```

With Andromeda you can pull data from multiple facebook, instagram and YouTube
channels and store in the same data lake. You can see more details about how to
set up multiple accounts [here](adding_multiple_accounts.md).