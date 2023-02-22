using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamplePackBrowser : TsvrTool
{
    public override TsvrToolType ToolType { get => TsvrToolType.SamplePackBrowser; }
    public ScrollableListUI scrollableListUI;
    public string manifestResourcePath = "SamplePacks/sample-packs";
    
    void Start()
    {
        DisplayAllSamplePacks(manifestResourcePath);
    }

    void DisplayAllSamplePacks(string _manifestResourcePath) {
        scrollableListUI.ClearItems();
        scrollableListUI.SetHeader("Sample Packs", "Select a pack to view its contents");
        SamplePackMetadata[] samplePacks = AppDataParser.GetInstalledSamplePacks(_manifestResourcePath);
        foreach(SamplePackMetadata samplePack in samplePacks) {

            scrollableListUI.AddItem(samplePack, (item, content) => {
                SamplePackMetadata samplePackMetadata = (SamplePackMetadata)item;
                content.header.text = samplePackMetadata.title;
                content.subheader.text = samplePackMetadata.creator;
            }, (item) => {
                Debug.Log("Selected sample pack: " + ((SamplePackMetadata)item).title);
                DisplaySamplePack((SamplePackMetadata)item);
            });
        }
    }
    
    void DisplaySamplePack(SamplePackMetadata metadata) {
        // get the samplePack item from resources
        TsvrSamplePack samplePack = AppDataParser.GetSamplePack(metadata.id);
        scrollableListUI.ClearItems();
        scrollableListUI.SetHeader(metadata.title, "Select a sample to create a grain cloud");
        scrollableListUI.AddItem("Back", () => {
            DisplayAllSamplePacks(manifestResourcePath);
        });
        foreach(TsvrSample sample in samplePack.samples) {
            scrollableListUI.AddItem(sample, (item, content) => {
                TsvrSample sampleMetadata = (TsvrSample)item;
                content.header.text = sampleMetadata.title;
                var stereo = sampleMetadata.channels == 2 ? "Stereo" : "Mono";
                content.subheader.text = $"{sampleMetadata.duration} Seconds | {stereo}";
            }, (item) => {
                TsvrSample tsvrSample = (TsvrSample)item;
                Debug.Log("Selected sample: " + tsvrSample.title);
            });
        }
    }
}
