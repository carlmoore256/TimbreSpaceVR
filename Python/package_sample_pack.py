import glob
import os 
import json
import argparse
from pydub import AudioSegment
import datetime
import shutil
from utils import save_json, load_json

DEFAULT_CREATOR = "Carl Moore"
SAMPLE_PACKS_RESOURCE_PATH = "samplepacks"
PACK_METADATA_PATH = "metadata.json"
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

def get_audio_info(file):
    audio_file = AudioSegment.from_wav(file)
    return {
        "duration": round(audio_file.duration_seconds, 4),
        "channels": audio_file.channels,
        # "maxDBFS": round(audio_file.max_dBFS, 4),
}


def get_sample_info(file, pack_title=None):
    print(f'Getting info for {file}...')
    file_bytes = os.stat(file).st_size
    audio_info = get_audio_info(file)
    info = {
        "file": os.path.basename(file),
        "title": os.path.basename(file).split(".")[0].replace("_", " ").replace("-", " ").title(),
        "bytes": file_bytes,
        "duration": audio_info["duration"],
        "channels": audio_info["channels"],
        # "maxDBFS": audio_info["maxDBFS"]
    }    
    if pack_title is not None:
        filetype = "." + os.path.basename(file).split(".")[1]
        info["resource"] =  f"{SAMPLE_PACKS_RESOURCE_PATH}/{pack_title}/{os.path.basename(file).replace(filetype, '')}"
    info = {**info, "parameters" : DEFAULT_PARAMETERS}
    return info

def save_json(data, path):
    with open(path, "w") as f:
        json.dump(data, f, indent=4)


def load_json(path):
    with open(path, "r") as f:
        return json.load(f)
    

def update_sample_pack_info():
    packs = glob.glob(SAMPLE_PACKS_PATH + "/*")
    packs = [p for p in packs if os.path.isdir(p)]
    packs = [load_json(os.path.join(p, PACK_METADATA_PATH)) for p in packs]
    packs = sorted(packs, key=lambda k: k['metadata']['title'])
    info = [p['metadata'] for p in packs]
    save_json(info, SAMPLE_PACKS_INFO)
    print(f"Updated sample pack info: {SAMPLE_PACKS_INFO}")


def create_sample_pack(path, title=None, creator=DEFAULT_CREATOR, description="", overwrite=False):
    if title is None:
        title = os.path.basename(path).replace("_", " ").replace("-", " ").title()
    files = glob.glob(path + "/*.wav")
    samples = [get_sample_info(f, title) for f in files]
    samples = [s for s in samples if s["bytes"] > 0 and s["duration"] > 0]
    print(f'Sample Pack: {title} | Found {len(samples)} samples in {path}')
    samples = sorted(samples, key=lambda k: k['title'])
    pack = {
        "metadata" : {
            "title": title,
            "id": title.replace(" ", "-").lower(),
            "creator": creator,
            "date": datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "numSamples": len(samples),
        },
        "samples": samples,
    }
    outdir = os.path.join(SAMPLE_PACKS_PATH, pack["metadata"]["id"])
    if os.path.exists(outdir) and not overwrite:
        print("Sample pack already exists. Use --overwrite to overwrite.")
        return
    if os.path.exists(outdir) and overwrite:
        print("Overwriting existing sample pack...")
    if not os.path.exists(outdir):
        print("Creating new sample pack...")
        os.mkdir(outdir)
    for sample in samples:
        src = os.path.join(path, sample["file"])
        dst = os.path.join(outdir, sample["file"])
        if os.path.exists(dst):
            print(f"File {dst} already exists, continuing")
        else:
            print(f"Copying {src} => {dst}")
            shutil.copyfile(src, dst)
    save_json(pack, os.path.join(outdir, PACK_METADATA_PATH))
    update_sample_pack_info()


if __name__ == "__main__":

    parser = argparse.ArgumentParser()
    parser.add_argument("--path", help="Path to the sample pack folder")
    parser.add_argument("--title", help="Title of the sample pack", default=None)
    parser.add_argument("--creator", help="Creator of the sample pack", default=DEFAULT_CREATOR)
    parser.add_argument("--description", help="Description of the sample pack", default="")
    parser.add_argument("--overwrite", help="Overwrite existing sample pack", action="store_true")
    args = parser.parse_args()

    create_sample_pack(args.path, args.title, args.creator, args.description, args.overwrite)

   