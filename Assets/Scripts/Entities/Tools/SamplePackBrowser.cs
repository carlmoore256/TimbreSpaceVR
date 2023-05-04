using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SamplePackBrowser : TsvrTool
{
    public override TsvrToolType ToolType { get => TsvrToolType.SamplePackBrowser; }
    public ScrollableListUI scrollableListUI;
    private string manifestResourcePath = "SamplePacks/packs";
    
    void Start()
    {
        DisplayAllSamplePacks(manifestResourcePath);
    }

    void OnEnable() {
        ControllerActions.AddListener(ControllerActions.toolAxis2D, OnAxis2D, InputActionPhase.Performed);
        ControllerActions.AddListener(ControllerActions.uiSelect, OnSubmit, InputActionPhase.Performed);
    }

    void OnDisable() {
        ControllerActions.RemoveListener(ControllerActions.toolAxis2D, OnAxis2D, InputActionPhase.Performed);
        ControllerActions.RemoveListener(ControllerActions.uiSelect, OnSubmit, InputActionPhase.Performed);
    }

    void OnAxis2D(InputAction.CallbackContext context) {
        Vector2 value = context.ReadValue<Vector2>();
        scrollableListUI.ScrollValue(value.y);
    }

    void OnSubmit(InputAction.CallbackContext context) {
        scrollableListUI.OnSubmit();
    }

    void DisplayAllSamplePacks(string _manifestResourcePath) {
        scrollableListUI.ClearItems();
        scrollableListUI.SetHeader("Sample Packs", "Select a pack to view its contents");
        TsvrSamplePackMetadata[] samplePacks = AppDataParser.GetInstalledSamplePacks(_manifestResourcePath);
        if (samplePacks == null) {
            Debug.LogError("Could not load sample packs from " + _manifestResourcePath);
            return;
        }
        foreach(TsvrSamplePackMetadata samplePack in samplePacks) {

            scrollableListUI.AddItem(samplePack, (item, content) => {
                TsvrSamplePackMetadata samplePackMetadata = (TsvrSamplePackMetadata)item;
                content.header.text = samplePackMetadata.title;
                content.subheader.text = samplePackMetadata.creator;
            }, (item) => {
                Debug.Log("Selected sample pack: " + ((TsvrSamplePackMetadata)item).title);
                DisplaySamplePack((TsvrSamplePackMetadata)item);
            });
        }
    }
    
    void DisplaySamplePack(TsvrSamplePackMetadata metadata) {
        // get the samplePack item from resources
        TsvrSamplePack samplePack = AppDataParser.GetSamplePack(metadata.id);
        scrollableListUI.ClearItems();
        scrollableListUI.SetHeader(metadata.title, "Select a sample to create a grain cloud");
        scrollableListUI.AddItem("Back", () => {
            DisplayAllSamplePacks(manifestResourcePath);
        });
        foreach(TsvrAudioSample sample in samplePack.samples) {
            scrollableListUI.AddItem(
                sample, 
                (item, content) => {
                    TsvrAudioSample sampleMetadata = (TsvrAudioSample)item;
                    content.header.text = sampleMetadata.title;
                    var stereo = sampleMetadata.channels == 2 ? "Stereo" : "Mono";
                    content.subheader.text = $"{sampleMetadata.duration} Seconds | {stereo}";
                }, 
                (item) => {
                    TsvrAudioSample tsvrSample = (TsvrAudioSample)item;
                    ToolController.ChangeTool(TsvrToolType.ModelInspector, (tool) => {
                        ModelInspector modelMultitool = (ModelInspector)tool;
                        Vector3 modelPosition = transform.position + transform.forward * 1.5f;
                        Quaternion modelRotation = Quaternion.LookRotation(transform.forward, transform.up);
                        // make model rotation only on horizontal axis
                        modelRotation = Quaternion.Euler(0, modelRotation.eulerAngles.y, 0);
                        Debug.Log("METADATA: " + metadata);
                        // GrainModelOld newModel = GrainModelOld.SpawnFromSample(metadata, tsvrSample, modelPosition, modelRotation);
                        // Debug.Log("Just created new model, " + newModel);
                        // modelMultitool.SetSelectedModel(newModel);
                    });
                }
            );
        }
    }
}
