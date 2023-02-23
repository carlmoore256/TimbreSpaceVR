using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SamplePackBrowser : TsvrTool
{
    public override TsvrToolType ToolType { get => TsvrToolType.SamplePackBrowser; }
    public ScrollableListUI scrollableListUI;
    public string manifestResourcePath = "SamplePacks/sample-packs";
    
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
        foreach(TsvrSample sample in samplePack.samples) {
            scrollableListUI.AddItem(sample, (item, content) => {
                TsvrSample sampleMetadata = (TsvrSample)item;
                content.header.text = sampleMetadata.title;
                var stereo = sampleMetadata.channels == 2 ? "Stereo" : "Mono";
                content.subheader.text = $"{sampleMetadata.duration} Seconds | {stereo}";
            }, (item) => {
                TsvrSample tsvrSample = (TsvrSample)item;
                Debug.Log("Selected sample: " + tsvrSample.title);
                ToolController.ChangeTool(TsvrToolType.ModelMultitool, (tool) => {
                    ModelMultitool modelMultitool = (ModelMultitool)tool;
                    Vector3 modelPosition = transform.position + transform.forward * 0.5f;
                    Quaternion modelRotation = Quaternion.LookRotation(transform.forward, transform.up);
                    // make model rotation only on horizontal axis
                    modelRotation = Quaternion.Euler(0, modelRotation.eulerAngles.y, 0);
                    GrainModel newModel = GrainModel.SpawnFromSample(metadata, tsvrSample, modelPosition, modelRotation);
                    modelMultitool.SetCurrentModel(newModel);
                });

            });
        }
    }
}
