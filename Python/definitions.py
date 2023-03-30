DEFAULT_CREATOR = {
    "name" : "Carl Moore",
    "website" : "https://carlmoore.xyz"
}

DATA_PATH = "data/"

SAMPLE_PACKS_RESOURCE_PATH = "SamplePacks"
PACK_METADATA_PATH = "metadata.json"
UNITY_RESOURCES_PATH = "../Assets/Resources"
SAMPLE_PACKS_PATH = f"../Assets/Resources/{SAMPLE_PACKS_RESOURCE_PATH}"
SAMPLE_PACKS_INFO = f"{SAMPLE_PACKS_PATH}/packs.json"
DEFAULT_PARAMETERS = {
    "xFeature": "MFCC_0",
    "yFeature": "MFCC_1",
    "zFeature": "MFCC_2",
    "rFeature": "MFCC_3",
    "gFeature": "MFCC_4",
    "bFeature": "MFCC_5",
    "scaleFeature": "RMS",
    "windowSize": 8192,
    "hopSize": 8192,
    "scaleMult" : 0.01,
    "scaleExp" : 0.1,
    "useHSV" : False,
    "posAxisScale" : [1,1,1],
}

RESOURCE_LOCATION = {
    "package" : 0,
    "local" : 1,
    "appdata" : 2,
    "web" : 3,
}

RESOURCE_CATEGORIES = [
    "sample",
    "thumbnail",
    "samplepack",
    "settings",
]

RESOURCE_TYPES = [
    "audio/wav",
    "audio/mp3",
    "image/png",
    "image/jpeg",
    "image/gif",
    "application/json",
    "text/plain",
    "application/octet-stream",
]

EMPTY_SESSION = {
    "sequences" : []
}