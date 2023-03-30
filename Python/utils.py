import json
import os
import definitions
import datetime
import mimetypes

def save_json(data, path, sort_keys=False, indent=4):
    with open(path, "w") as f:
        json.dump(data, f, indent=indent, sort_keys=sort_keys)


def load_json(path):
    with open(path, "r") as f:
        return json.load(f)

def get_file_size(file):
    return os.stat(file).st_size

def auto_title(file):
    return os.path.basename(file).split(".")[0].replace("_", " ").replace("-", " ").title()

def save_temp_data(data, folder, name):
    if not os.path.exists(definitions.DATA_PATH):
        os.makedirs(definitions.DATA_PATH)
    path = os.path.join(definitions.DATA_PATH, folder)
    if not os.path.exists(path):
        os.makedirs(path)
    save_json(data, os.path.join(path, name))


# return a unix timestamp
def datestamp():
    return int(datetime.datetime.now().timestamp())

def get_mimetype(file):
    return mimetypes.guess_type(file)[0]

def print_pretty(data):
    print(json.dumps(data, indent=4))