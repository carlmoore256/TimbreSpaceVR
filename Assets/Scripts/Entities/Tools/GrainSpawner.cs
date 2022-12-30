using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrainSpawner : TsvrTool
{   
    public new TsvrToolType ToolType = TsvrToolType.GrainSpawner;
    public FileListUI fileListUI;

    // public InputActionMap ToolActions;


    public void OnEnable() {

        // var files = TsvrApplication.AudioManager.GetDefaultAudioFiles();
        fileListUI.SetDirectory(TsvrApplication.AudioManager.GetDefaultAudioFilePath(), "wav");
        
        if (animations != null) {
            Debug.Log("Playing equip animation");
            animations.Play("GrainSpawnerEquip");
        }

        SubscribeActions();
        // StartCoroutine(DebugSpawnDelayed());
    }

    public void OnDisable() {
        UnsubscribeActions();
        if (animations != null) {
            Debug.Log("Playing unequip animation");
            // animations.Play("UnequipPlayWand");
        }
    }

    void SubscribeActions() {
        ControllerActions.toolAxis2D.action.started += CycleSelection;
        ControllerActions.toolOptionButton.action.started += SpawnSelectedFile;
        // m_ControllerActions.SubscribeAction(m_ToolController.m_ControllerActions.grainSpawnerAction, OnGrainSpawnerAction);
    }

    void UnsubscribeActions() {
        ControllerActions.toolAxis2D.action.started -= CycleSelection;
        ControllerActions.toolOptionButton.action.started -= SpawnSelectedFile;
        // m_ControllerActions.UnsubscribeAction(m_ToolController.m_ControllerActions.grainSpawnerAction, OnGrainSpawnerAction);
    }

    void Execute(InputAction.CallbackContext ctx) {
        switch(ctx.action.name) {
            case "GrainSpawnerAction":
                Debug.Log("Grain spawner action");
                break;
            default:
                Debug.Log("Unknown action");
                break;
        }
        Debug.Log("Joystick select" + ctx.ReadValue<Vector2>());
    }

    void CycleSelection(InputAction.CallbackContext ctx) {
        Debug.Log("Joystick select" + ctx.ReadValue<Vector2>());
        Vector2 joystick = ctx.ReadValue<Vector2>();
        if (joystick.y > 0f) {
            Debug.Log("Joystick select");
            fileListUI.NextFile();
        } else if (joystick.y < 0f) {
            Debug.Log("Joystick select");
            fileListUI.PreviousFile();
        }
    }

    void SpawnSelectedFile(InputAction.CallbackContext ctx) {
        animations.Play("GrainSpawnerButton");
        Debug.Log("Pushed main button");
        GameObject newModel = Instantiate(
            TsvrApplication.Config.grainModel, 
            GameObject.Find("GrainParent").transform);
        GrainModel grainModel = newModel.GetComponent<GrainModel>();
        grainModel.Initialize(new Vector3(0,1,1), 0.05f, fileListUI.CurrentlySelectedFile.FullName);
        // selectedModel = gm;
    }

    IEnumerator DebugSpawnDelayed() {
        yield return new WaitForSeconds(2);
        Debug.Log("Spawning grain");
        GameObject newModel = Instantiate(
            TsvrApplication.Config.grainModel, 
            GameObject.Find("GrainParent").transform);
        GrainModel grainModel = newModel.GetComponent<GrainModel>();
        grainModel.Initialize(new Vector3(0,1,1), 0.05f, fileListUI.CurrentlySelectedFile.FullName);
        // grainModel.Initialize(new Vector3(0,1,1), 0.05f, "Assets/Resources/Audio/mgmt.wav");
        // selectedModel = gm;
    }
}
