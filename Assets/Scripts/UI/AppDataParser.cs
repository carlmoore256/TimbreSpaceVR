using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// load resources from json, parse and fill a menu
public static class AppDataParser {

    public static TsvrSamplePackMetadata[] GetInstalledSamplePacks(string path) {
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null) {
            return null;
        }
        TsvrSamplePackMetadata[] samplePacks = JsonHelper.FromJson<TsvrSamplePackMetadata>(JsonHelper.fixJson(textAsset.ToString()));
        return samplePacks;
    }

    public static TsvrSamplePack GetSamplePack(string id) {
        string resourceDir = "SamplePacks/" + id;
        TextAsset samplePackTextAsset = Resources.Load<TextAsset>(resourceDir + "/pack");
        if (samplePackTextAsset == null) {
            return null;
        }
        TsvrSamplePack samplePack = JsonUtility.FromJson<TsvrSamplePack>(samplePackTextAsset.ToString());
        return samplePack;
    }

    public static TsvrSamplePack GetSamplePack(TsvrSamplePackMetadata metadata) {
        return GetSamplePack(metadata.id);
    }

    public static void ParseSamplePacks(string path, MonoBehaviour targetMenu) {
        var samplePacks = GetInstalledSamplePacks(path);

        foreach(TsvrSamplePackMetadata metadata in samplePacks) {
            string resourceDir = "SamplePacks/" + metadata.id;
            try {
                TextAsset samplePackTextAsset = Resources.Load<TextAsset>(resourceDir + "/pack");
                TsvrSamplePack samplePack = JsonUtility.FromJson<TsvrSamplePack>(samplePackTextAsset.ToString());
                Debug.Log("SAMPLE PACK | Title: " + samplePack.metadata.title + " | ID: " + samplePack.metadata.id + " | Creator: " + samplePack.metadata.creator + " | Num Samples: " + samplePack.metadata.numSamples);
                foreach(TsvrSample sample in samplePack.samples) {
                    Debug.Log("SAMPLE | File: " + sample.file + " | Title: " + sample.title + " | Bytes: " + sample.bytes + " | Duration: " + sample.duration + " | Channels: " + sample.channels + " | Max DBFS: " + sample.maxDBFS);
                }
            } catch {
                Debug.Log($"ERROR: Sample pack {metadata.title} is missing or corrupted");
            }
        }

        // SelectableListItem<SamplePackMetadata>[] selectableListItems = new SelectableListItem<SamplePackMetadata>[samplePacks.Length];
        // for (int i = 0; i < samplePackInfos.Length; i++) {
        //     selectableListItems[i] = new SelectableListItem(samplePackInfos[i].title, samplePackInfos[i].id);
        // }
        // return selectableListItems;
    }
}
