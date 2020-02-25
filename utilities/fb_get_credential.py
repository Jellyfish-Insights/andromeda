#!/usr/local/bin/env python3
'''
    This script will help the process on get credentials
    to fetch data from Andromeda
'''
import requests, json, os

FB_URL = 'https://graph.facebook.com'
VERSION = 'v5.0'

with open('fb_client_secret.json') as json_file:
    data = json.load(json_file)
    APP_ID = data["APP_ID"]
    APP_SECRET = data["APP_SECRET"]
    USER_ACCESS_TOKEN = data["USER_ACCESS_TOKEN"] 

def create_credential_folders():
    try:
        os.mkdir('./credentials/')
        os.mkdir('./credentials/adwords')
        os.mkdir('./credentials/facebook')
        os.mkdir('./credentials/facebook/adaccount')
        os.mkdir('./credentials/facebook/page')
        os.mkdir('./credentials/instagram')
        os.mkdir('./credentials/youtube')
    except:
        pass
    print('Created credential structure.')

def request_data(url):
    response = requests.get(url)
    if response.status_code != 200:
        print('Request failed')
        print(json.loads(response._content.decode('utf8').replace("'", '"')))
        exit(1)

    return json.loads(response._content.decode('utf8').replace("'", '"'))

def save_on_json(path, data, platform, name):
    with open(path, 'w') as outfile:
        print(f'Saving long-time credentials for {platform} - {name}.')
        json.dump(data, outfile)

def long_lived_user_token():
    path = './credentials/facebook/user_credentials.json'
    if os.path.isfile(path):
        print('Loading long lived user token.')
        with open(path) as json_file:
            return json.load(json_file)

    print('Requesting long lived user token.')
    url = f'{FB_URL}/{VERSION}/oauth/access_token?' \
            + 'grant_type=fb_exchange_token&'       \
            + f'client_id={APP_ID}&'                \
            + f'client_secret={APP_SECRET}&'        \
            + f'fb_exchange_token={USER_ACCESS_TOKEN}'

    access_token = request_data(url)
    with open(path, 'w') as outfile:
        print("Saving long-time user credentials.")
        json.dump(access_token, outfile)

    return access_token

def long_lived_page_token(access_token):
    return request_data(f'{FB_URL}/{VERSION}/me/accounts?access_token={access_token}')

def change_token(page):
    # On Andromeda we use "token" but Facebook API return "access_token"
    page["token"] = page["access_token"]
    return page

def long_lived_instagram_token(page):
    url = f'{FB_URL}/{VERSION}/{page["id"]}?fields=instagram_business_account&access_token={page["access_token"]}'
    instagram = request_data(url)
    if "instagram_business_account" in instagram:
        instagram_data = {
            "token" : page["access_token"],
            "id" : instagram["instagram_business_account"]["id"],
            "source_page" : page["name"]
        }
        save_on_json(f'./credentials/instagram/{instagram_data["id"]}_credentials.json', instagram_data, 'instagram', instagram_data["source_page"])

def main():
    create_credential_folders()
    user_access_token = long_lived_user_token()
    pages_access_token = long_lived_page_token(user_access_token["access_token"])

    for page in pages_access_token["data"]:
        page = change_token(page)
        save_on_json(f'./credentials/facebook/page/{page["id"]}_credentials.json', page, "page", page["name"])
        long_lived_instagram_token(page)
        
    print('Done.')

if __name__ == '__main__': 
    main()