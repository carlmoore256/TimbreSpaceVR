import glob
import os 
import json
import argparse
import datetime
import shutil

resources_path = "../Assets/Resources"
ignore_files = ["resources-map.json", ".DS_Store"]
ignore_ext = ["meta", "asset"]

# make a json map of all files in resources
def make_resources_map():
    resources_map = []
    for file in glob.glob(resources_path + "/**/*", recursive=True):
        if os.path.isdir(file):
            continue
        # if file in ignore_files:
        #     continue
        if file.split(".")[-1] in ignore_ext:
            continue
        # file_path = os.path.abspath(file).recplace("\\", "/")
        file_path = file.replace("\\", "/")
        file_name = file_path.replace(resources_path, "")
        if file_name.startswith("/"):
            file_name = file_name[1:]
        if file_name in ignore_files:
            continue
        resources_map.append({
            "file": file_name,
            "type": file_path.split(".")[-1],
            "bytes": os.stat(file_path).st_size,
            "created": datetime.datetime.fromtimestamp(os.path.getctime(file_path)).isoformat(),
            "modified": datetime.datetime.fromtimestamp(os.path.getmtime(file_path)).isoformat(),
        })
    with open(os.path.join(resources_path, "resources-map.json"), "w") as f:
            json.dump(resources_map, f, indent=4)


if __name__ == "__main__":
    make_resources_map()
