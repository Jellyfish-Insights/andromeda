#!/usr/local/bin/env python3
'''
    This script will help the process on get credentials
    to fetch data from Andromeda
'''
import requests, json, os

# Import secret data
FB_URL = 'https://graph.facebook.com'
VERSION = 'v5.0'

with open('fb_client_secret.json') as json_file:
    data = json.load(json_file)
    APP_ID = data["APP_ID"]
    APP_SECRET = data["APP_SECRET"]
    USER_ACCESS_TOKEN = data["USER_ACCESS_TOKEN"] 

def request_data(url):
    response = requests.get(url)
    if response.status_code != 200:
        print('Request failed')
        print(json.loads(response._content.decode('utf8').replace("'", '"')))
        exit(1)

    return json.loads(response._content.decode('utf8').replace("'", '"'))

# Get long lived tokens
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
    url = f'{FB_URL}/{VERSION}/me/accounts?access_token={access_token}'
    response = requests.get(url)
    return json.loads(response._content.decode('utf8').replace("'", '"'))

def long_lived_instagram_token(pages_access_token):
    for page in pages_access_token:
        url = f'{FB_URL}/{VERSION}/{page["id"]}?fields=instagram_business_account&access_token={page["access_token"]}'
        response = requests.get(url)
        instagram_info = json.loads(response._content.decode('utf8').replace("'", '"'))
        if "instagram_business_account" in instagram_info:
            save_instagram_token(instagram_info["instagram_business_account"]["id"], page["name"], page["access_token"])

# Save tokens on JSON files
def save_pages_token(pages_access_token):
    for page in pages_access_token:
        with open(f'./credentials/facebook/page/{page["id"]}_credentials.json', 'w') as outfile:
            print(f'Saving long-time credentials for the page {page["name"]}.')
            json.dump(page, outfile)

def save_instagram_token(instagram_id, page, access_token):
    with open(f'./credentials/instagram/{instagram_id}_credentials.json', 'w') as outfile:
        print(f'Saving long-time credentials for the instagram business account related to {page}.')
        instagram_info = {
            "access_token" : access_token,
            "id" : instagram_id,
            "Source Page" : page
        }
        json.dump(instagram_info, outfile)

# Create folders structure
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

def main():
    create_credential_folders()
    user_access_token = long_lived_user_token()
    pages_access_token = long_lived_page_token(user_access_token["access_token"])
    save_pages_token(pages_access_token["data"])
    long_lived_instagram_token(pages_access_token["data"])
    print('Done.')

if __name__ == '__main__': 
    main() 
