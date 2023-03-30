import requests
from dotenv import dotenv_values
import os
import json

env = dotenv_values(".env")

# pinata metadata looks like this: '{"name": "MyFile", "keyvalues": {"company": "Pinata"}}'
PINATA_FILE_URL = "https://api.pinata.cloud/pinning/pinFileToIPFS"
PINATA_JSON_URL = "https://api.pinata.cloud/pinning/pinJSONToIPFS"

PINATA_OPTIONS = {"cidVersion": 1}

IPFS_GATEWAYS = {
    'ipfs.io': 'https://ipfs.io/ipfs/',
    'pinata': 'https://gateway.pinata.cloud/ipfs/'
}

def ipfs_cid_to_url(cid, gateway='ipfs.io'):
    return f'{IPFS_GATEWAYS[gateway]}{cid}'

def upload_file_to_pinata(file_path, filename=None, jwt=env['PINATA_JWT'], pinata_metadata=None):
    payload = {'pinataOptions': json.dumps(PINATA_OPTIONS)}
    if pinata_metadata is not None:
        payload['pinataMetadata'] = json.dumps(pinata_metadata)
    elif filename is not None:
        payload['pinataMetadata'] = json.dumps({'name': filename})
    # payload = json.dumps(payload)

    if filename is None:
        filename = os.path.basename(file_path)
    files = [
        ('file', (filename, open(file_path, 'rb'), 'application/octet-stream'))
    ]
    headers = {
        'Authorization': f'Bearer {jwt}'
    }
    response = requests.post(
        PINATA_FILE_URL, data=payload, files=files, headers=headers)
    if response.status_code == 200:
        response_dict = json.loads(response.text)
        response_dict['url'] = ipfs_cid_to_url(response_dict['IpfsHash'])
        return response_dict
    print(f'Error uploading to pinata: {response.text}')
    return None


def upload_json_to_pinata(data, jwt=env['PINATA_JWT'], pinata_metadata=None):
    payload = {
        'pinataOptions': PINATA_OPTIONS,
        'pinataContent': json.dumps(data)
    }

    if pinata_metadata is not None:
        payload['pinataMetadata'] = pinata_metadata
    payload = json.dumps(payload)

    print(payload)
    headers = {
        'Content-Type': 'application/json',
        'Authorization': f'Bearer {jwt}'
    }

    response = requests.post(PINATA_JSON_URL, data=payload, headers=headers)
    if response.status_code == 200:
        response_dict = json.loads(response.text)
        response_dict['url'] = ipfs_cid_to_url(response_dict['IpfsHash'])
        return response_dict
    print(f'Error uploading to pinata: {response.text}')
    return None

# helper to plug into create_web_resource
def pinata_web_provider(file_path):
    return upload_file_to_pinata(file_path, None, env['PINATA_JWT'], None)['url']