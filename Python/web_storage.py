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


def upload_file_to_aws(file_path, bucket_name=env['AWS_BUCKET_NAME'], object_key=None):
    import boto3
    # Create an S3 client
    s3 = boto3.client(
        's3',
        aws_access_key_id=env['AWS_ACCESS_KEY_ID'],
        aws_secret_access_key=env['AWS_SECRET_ACCESS_KEY'])

    # Use the filename as the object key if not provided
    if object_key is None:
        object_key = os.path.basename(file_path)

    # Upload the file
    with open(file_path, 'rb') as file:
        response = s3.upload_fileobj(file, bucket_name, object_key)
        # , ExtraArgs={'ACL': 'public-read'}

    # If successful, return the object key and URL
    if response is None:
        url = f'https://{bucket_name}.s3.amazonaws.com/{object_key}'
        return {'object_key': object_key, 'url': url}
    else:
        print(f'Error uploading to AWS S3: {response}')
        return None
    
def upload_json_to_aws(data, bucket_name=env['AWS_BUCKET_NAME'], object_key=None):
    # create a temp json file
    import tempfile
    import json
    with tempfile.NamedTemporaryFile(mode='w+b', suffix='.json', delete=False) as temp:
        json_data = json.dumps(data).encode('utf-8')
        temp.write(json_data)
        temp.flush()
        temp.close() # close the file to release the file lock
        return upload_file_to_aws(temp.name, bucket_name, object_key)
    
def aws_web_provider(file_path):
    return upload_file_to_aws(file_path, env['AWS_BUCKET_NAME'], None)['url']
