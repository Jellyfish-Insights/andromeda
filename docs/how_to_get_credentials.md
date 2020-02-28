# How to get social media credentials

## Introduction

This document intends to explain how to get the social media credentials in
order to run Andromeda.

Keep in mind that just one credential is already enough to make the project run.

## Credentials folder structure

To run Andromeda, all the credentials should be placed in a specific folder
structure explained [here](./credential_folder_structure.md).

## Adwords

The official [Adwords API documentation for
.NET](https://developers.google.com/adwords/api/docs/guides/first-api-call#set_up_oauth2_authentication)
explains how to get the Adwords credentials.

Once you have your `App.config` ready (the documentation will explain to you how
to do it), copy and paste the file into the credentials folder.

## Facebook Pages, Facebook Marketing and Instagram Credentials

You can get credentials to Facebook Pages, Facebook Marketing and Instagram
Credentials in two ways: using the Andromeda python scripts or following the
instructions below. We recommend to use the python scripts, since it cuts a lot
of steps and make the process of getting the Facebook credentials easier.

You can check how to get the Facebook Pages, Facebook Marketing and Instagram
Credentials by running the python scripts [here](./run_credentials_script#Facebook/Instagram-Credentials).

### Getting Facebook credentials manually

#### Creating a Facebook APP
The first step to get the tokens for the Facebook Page,
Marketing and Instagram is to create a Facebook App.

   - Go to https://developers.facebook.com/ and log-in with your Facebook account.
   - On the right side click in `My Apps -> Create App` and fill the field with your Project Name.

   Create a User token to access your Pages, Marketing and Instagram accounts

   - Click in `Tools -> Graph API Explorer`.
   - On the right side, Select your App on `Facebook App` and click in `Get Token -> Get User Access Token`.
   - Choose the permissions (It can be one application for Page, Marketing and Instagram or one application for each).
      - **Page**: manage_pages, pages_show_list, read_insigths
      - **Adaccount**: ads_management, ads_read, read_insigths
      - **Instagram**: instagram_basic, instagram_manage_comments, instagram_manage_insigths
   - Click on `Get Access Token` and select the pages/instagrams that you want to bound to your application.
   - In the fields `Access Token` will be your short-time User Token.

#### Convert short-time token to long-lived token

   The short-time token expires after 1h, for Andromeda we would like to have a long-lived token.

   - Click on the exclamation button aside for your token.
   - Click in `Open Access Token Tool`.
   - At the bottom of the Page, you will find the button `Extend Access Token`, click on it to generate your long-lived token.
   - Click on `Debug` and copy your long-lived token.

#### Get your Page token

   On the `Graph API Explorer`:
   - Replace your shor-time token for the long-lived and click on `Get Access Token`.
   - On `User or Page` select the page that you wants the token.
   - In the fields `Access Token` will be your long-lived Page Token.

   **Page Token** can be used to Fetch data from Page and Instagram, while the **User Token** can be Used to Fetch data from Adaccount and Instagram (If the application permission is set up just to Instagram).

#### Query data from your page

   Using this token you will be able to manually query your data from Facebook API.

   - Copy your long-lived token;
   - Go back to `Graph API Explorer`;
   - Fill the field `Access Token` with your token;
   - At the top of the you can write:
      - **Page**: `<page-id>/posts`
      - **Adaccount**: `<account-id>/ads`
      - **Instagram**: `<instagram-id>/media`
   - Then click on the `Submit` button.

## YouTube

You can get credentials to YouTube Credentials in two ways: using the Andromeda python scripts or following the
The Medium article [Fetching YouTube Data using
Andromeda](https://medium.com/@insightsjellyfish/fetching-youtube-data-using-andromeda-8f1b1240803c).
We recommend to use the python scripts, since it cuts a lot of steps and make
the process of getting the YouTube credentials easier.

You can check how to get the YouTube
Credentials by running the python scripts [here](./run_credentials_script#YouTube-Credentials).
