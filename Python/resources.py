from utils import save_json, load_json, get_mimetype, get_file_size, auto_title, save_temp_data, datestamp
from hashing import hash_file
import definitions
from unity import copy_file_to_unity_resources, format_resource_path


def create_package_resource(
        local_file, 
        category, 
        resource_dir=definitions.SAMPLE_PACKS_RESOURCE_PATH,
        encapsulate_path=True,
        title=None, 
        type=None):
    
    if type is None:
        type = get_mimetype(local_file)
    assert type in definitions.RESOURCE_TYPES
    assert category in definitions.RESOURCE_CATEGORIES
    hash = hash_file(local_file)
    num_bytes = get_file_size(local_file)
    title = title if title is not None else auto_title(local_file)
    if encapsulate_path:
        resource_dir = f"{resource_dir}/{title}"
    unity_resource_path = copy_file_to_unity_resources(
        local_file, resource_dir
    )
    resource = {
        "type": type,
        "category": category,
        "location": "package",
        "uri": format_resource_path(unity_resource_path),
        "hash": hash,
        "bytes": num_bytes
    }
    save_temp_data(resource, "resources", f"{hash}_{datestamp()}.json")
    return resource


def create_web_resource(
        local_file, 
        category, 
        web_provider, 
        type=None):
    
    if type is None:
        type = get_mimetype(local_file)
        print(f'Autodetected MIME type: {type}')
    assert type in definitions.RESOURCE_TYPES
    assert category in definitions.RESOURCE_CATEGORIES
    hash = hash_file(local_file)
    num_bytes = get_file_size(local_file)
    url = web_provider(local_file)
    resource = {
        "type": type,
        "category": category,
        "location": "web",
        "uri": url,
        "hash": hash,
        "bytes": num_bytes
    }
    # save to temp data since resource upload might be cost incurring or may
    # not work a second time
    save_temp_data(resource, "resources", f"{hash}_{datestamp()}.json")
    return resource
