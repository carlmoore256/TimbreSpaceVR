import hashlib
import json

SHORT_HASH = 16
USE_SHORT_HASH = True

def hash_file(file_path, short=USE_SHORT_HASH):
    with open(file_path, 'rb') as f:
        sha256 = hashlib.sha256()
        while True:
            data = f.read(65536)  # Read in 64KB blocks
            if not data:
                break
            sha256.update(data)
        hash = sha256.hexdigest()
        if short:
            return hash[:SHORT_HASH]
        return hash

def hash_string(text, short=USE_SHORT_HASH):
    sha256 = hashlib.sha256()
    sha256.update(text.encode('utf-8'))
    hash = sha256.hexdigest()
    if short:
        return hash[:SHORT_HASH]
    return hash

def hash_dict(data, short=USE_SHORT_HASH):
    sha256 = hashlib.sha256()
    sha256.update(json.dumps(data, sort_keys=True).encode('utf-8'))
    hash = sha256.hexdigest()
    if short:
        return hash[:SHORT_HASH]
    return hash

def hash_list(data, short=USE_SHORT_HASH):
    sha256 = hashlib.sha256()
    sha256.update(json.dumps(data, sort_keys=True).encode('utf-8'))
    hash = sha256.hexdigest()
    if short:
        return hash[:SHORT_HASH]
    return hash