from google_auth_oauthlib.flow import InstalledAppFlow
from datetime import datetime
from shutil import copyfile
import json, os, glob

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

def get_credentials():
    flow = InstalledAppFlow.from_client_secrets_file(
        'client_secret.json',
        scopes = ['https://www.googleapis.com/auth/youtube.readonly',
                'https://www.googleapis.com/auth/yt-analytics-monetary.readonly',
                'https://www.googleapis.com/auth/yt-analytics.readonly' 
        ]
    )

    credentials = flow.run_local_server(
        host='localhost',
        port=8080, 
        authorization_prompt_message='Please visit this URL: {url}', 
        success_message='The auth flow is complete; you may close this window.',
        open_browser=True
    )
    credentials.expiry
    return credentials

def save_credential(credential, path):
    data = {
        "access_token": f'{credential.token}',
        "token_type": "Bearer",
        "expires_in": 3600,
        "refresh_token": f'{credential.refresh_token}',
        "scope": "https://www.googleapis.com/auth/yt-analytics-monetary.readonly https://www.googleapis.com/auth/youtube.readonly",
        "Issued": f'{datetime.now().strftime("%Y-%m-%dT%H:%M:%SZ")}',
        "IssuedUtc": f'{datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%SZ")}'
    }
    file_name = 'Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json'
    try:
        os.mkdir(path)
    except:
        pass
    with open(f'{path}/{file_name}' , 'w') as outfile:
        print('Saving youtube credentials.')
        json.dump(data, outfile)
        

def main():
    create_credential_folders()
    channel_number = 1
    while True:
        path = f'./credentials/youtube'
        while os.path.isdir(f'{path}/channel_{channel_number}'):
            channel_number += 1
        
        credentials = get_credentials()
        save_credential(credentials, f'{path}/channel_{channel_number}')
        op = input("\nDo you want add a new channel? (y/N) ")

        if op != 'y':
            break
    copyfile("./client_secret.json", f'{path}/client_secret.json')
    print('Done.')

if __name__ == '__main__': 
    main()