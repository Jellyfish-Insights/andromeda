# How to get social media credentials

# Introduction
This document intends to help with getting the Andromeda necessary credentials following step-by-step instructions.

Keep in mind that just one credential is already enough to make the project run.

# Social media
  We have in total 5 social media token required to Andromeda work well, their names can be seen below:
  - Facebook Graph API (works for Facebook pages and Instagram);
  - Facebook Marketing API;
  - YouTube API;
  - Adwords API;
  - Google Login API.

   And we also need to set up `apsettings.json`.

# Getting credentials step-by-step
  This section's purpose is to explain the steps to get the credentials tokens and put them in the right folder with the intention to make Andromeda run.

## Credentials structure
   The credentials are divided under the structure presented below. On the next sub-sections of this document, we will learn how to set up each one of those with the intention to make andromeda run well.

   ```
   andromeda
   │   appsettings.json
   │
   └───facebook
   │       addaccount_credentials.json
   │       page_credentials.json
   │
   └───youtube
   │       client_secret.json
   │       Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json
   │
   └───adwords
           App.config
   ```

## Facebook Graph API

   ### Create a Facebook App and User token

   Create a Facebook App is the first step to get the tokens for the Page, Marketing and Instagram.

   - Go to https://developers.facebook.com/ and log-in with your Facebook account.
   - On the right side click in `My Apps -> Create App` and fill the field with your Project Name.

   Create a User token to access your Pages, Marketing and Instagrams accounts

   - Click in `Tools -> Graph API Explorer`.
   - On the right side, Select your App on `Facebook App` and click in `Get Token -> Get User Access Token`.
   - Choose the permissions (It can be one application for Page, Marketing and Instagram or one application for each).
      - **Page**: manage_pages, pages_show_list, read_insigths
      - **Adaccount**: ads_management, ads_read, read_insigths
      - **Instagram**: instagram_basic, instagram_manage_comments, instagram_manage_insigths
   - Click on `Get Access Token` and select the pages/instagrams that you want to bound to your application.
   - In the fields `Access Token` will be your short-time User Token.

   ### Convert short-time token to long-lived token

   The short-time token expires after 1h, for Andromeda we would like to have a long-lived token.

   - Click on the exclamation button aside for your token.
   - Click in `Open Access Token Tool`.
   - At the bottom of the Page, you will find the button `Extend Access Token`, click on it to generate your long-lived token.
   - Click on `Debug` and copy your long-lived token.

   ### Get your Page token

   On the `Graph API Explorer`: 
   - Replace your shor-time token for the long-lived and click on `Get Access Token`.
   - On `User or Page` select the page that you wants the token.
   - In the fields `Access Token` will be your long-lived Page Token.

   **Page Token** can be used to Fetch data from Page and Instagram, while the **User Token** can be Used to Fetch data from Adaccount and Instagram (If the application permission is set up just to Instagram).

   ### Query data from your page

   Using this token you will be able to manually query your data from Facebook API.

   - Copy your long-lived token;
   - Go back to `Graph API Explorer`;
   - Fill the field `Access Token` with your token;
   - At the top of the you can write:
      - **Page**: `<page-id>/posts`
      - **Adaccount**: `<account-id>/ads`
      - **Instagram**: `<instagram-id>/media`
   - Then click on the `Submit` button.

   ### Instagram
   https://developers.facebook.com/docs/instagram-api/getting-started


## YouTube API

Here we will explain how to get the YouTube token for your Andromeda application. The YouTube token is needed to access your data from the YouTube platform.

   ### Getting client_id and client_secret

   The first step to get your YouTube token is to set-up your application with the google client_id and client_secret. Following we will summarize the process of getting this information.

   - Go to https://console.developers.google.com/ and log-in with your Gmail account;
   - Click on Select Project -> New Project and fill the fields Project Name and Organization as you want;
   - In the left side click on Library and enable the YouTube Data API and YouTube Analytics API;
   - Go back to the previous screen and click on Configure the consent screen -> external -> create;
   - Choose your application name and save;
   - In the left side click in credentials;
   - Click on Create Credentials -> OAuth client ID, select other, fill the name of your project and save;
   - Download the `client_secret.json` and past it on `andromeda/credentials/youtube`.

   ### Configuring appsetings.json for google authentication

   Our login to Aurelia is made by google account authentication. The user will make a login with his Gmail account and then access the platform. To use this system we need to configure our appsetings.json with the client_id and client_secret.

   - Go to `andromeda/credentials/youtube/client_secret.json` and copy the fields client_id and client_secret;
   - Go to `andromeda/appsetings.json` and replace the **<GOOGLE CLIENT ID>** to client_id and **<GOOGLE CLIENT SECRET>** to the client_secret that you copy.

   Another how to get OAuth client_secret:
   - Video explaining how to get this credential: https://www.youtube.com/watch?v=sGLEcsRg0IM, keep in mind that this video uses `web application` instead of `other` when creating OAuth token(at the 1:11);

## Adwords API

Here we will explain how to get the AdWords token for your Andromeda application. The AdWords token is needed to access your data from the AdWords platform. The following video tutorial explain how to get the credentials using Java and the documentation guides you to get it using .NET, the concept is the same so we high recommend to follow this video with the documentation. 

   - Video: https://www.youtube.com/watch?v=yaDlZMfYWkg
   - Documentation: https://developers.google.com/adwords/api/docs/guides/first-api-call#set_up_oauth2_authentication

Once you have your `App.config` ready (the documentation will explain to you how to do it), copy and paste the file into the credentials folder.

## Google Login API
   - Good resource -> https://github.com/googleapis/google-api-python-client/blob/master/docs/oauth-web.md
