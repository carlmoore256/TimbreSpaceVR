import argparse
from utils import save_json, load_json, get_file_size, auto_title, get_mimetype
from hashing import hash_file, hash_dict, hash_list
import definitions
from unity import resources_subfolder, copy_file_to_unity_resources, format_resource_path
from resources import create_package_resource, create_web_resource


# handles creating the sample package that can be distributed to TSVR
def create_graincloud_metadata(
        local_file,
        title=None, 
        description="",
        parameters=definitions.DEFAULT_PARAMETERS, 
        creator=definitions.DEFAULT_CREATOR,
        resources=[],
        session=definitions.EMPTY_SESSION,
        output_file=None
    ):
    # resources needs to be an array
    if isinstance(resources, dict):
        resources = [resources]
    assert isinstance(resources, list)
    assert filter(lambda x: x["category"] == "sample", resources) is not None
    sample = list(filter(lambda x: x["category"] == "sample", resources))[0]
    # the unique graincloud hash = hash(hash(sample), hash(parameters))
    hash = hash_list([sample["hash"], hash_dict(parameters)])
    metadata = {
        "title": title if title is not None else auto_title(local_file),
        "description" : description,
        "hash" : hash,
        "creator" : creator,
        "parameters" : parameters,
        "resources" : resources,
        "session" : session
    }
    if output_file is None:
        output_file = definitions.DATA_PATH + f"metadata/{metadata['hash']}.json"
    save_json(metadata, output_file)
    return metadata 

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--path", help="Path to the audio file containing the sample")
    parser.add_argument("--title", help="Title of the sample pack", default=None)
    parser.add_argument("--creator", help="Creator of the sample pack", default=definitions.DEFAULT_CREATOR)
    parser.add_argument("--description", help="Description of the sample pack", default="")

    args = parser.parse_args()

    resources = [
        create_package_resource(
            local_file=args.path, 
            category="sample",
            title=args.title, 
        )
    ]

    create_graincloud_metadata(
        args.path,
        title=args.title,
        description=args.description,
        creator={'name' : args.creator},
        resources=resources
    )