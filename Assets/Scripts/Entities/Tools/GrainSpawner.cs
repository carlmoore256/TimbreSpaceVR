using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrainSpawner : TsvrTool
{   
    public void OnEnable() {
        ToolType = TsvrToolType.GrainSpawner; // find out a better way to do this, maybe tools aren't structured perfectly...

        if (animations != null) {
            Debug.Log("Playing equip animation");
            animations.Play("GrainSpawnerEquip");
        }

        SubscribeActions();
    }

    public void OnDisable() {
        UnsubscribeActions();
        if (animations != null) {
            Debug.Log("Playing unequip animation");
            // animations.Play("UnequipPlayWand");
        }
    }

    void SubscribeActions() {
        ControllerActions.toolOptionButton.action.started += PushMainButton;
        // m_ControllerActions.SubscribeAction(m_ToolController.m_ControllerActions.grainSpawnerAction, OnGrainSpawnerAction);
    }

    void UnsubscribeActions() {
        ControllerActions.toolOptionButton.action.started -= PushMainButton;
        // m_ControllerActions.UnsubscribeAction(m_ToolController.m_ControllerActions.grainSpawnerAction, OnGrainSpawnerAction);
    }

    void PushMainButton(InputAction.CallbackContext ctx) {
        animations.Play("GrainSpawnerButton");
        Debug.Log("Pushed main button");
        GameObject newModel = Instantiate(TsvrApplication.Config.grainModel, GameObject.Find("GrainParent").transform);
        GrainModel gm = newModel.AddComponent<GrainModel>();
        gm.Initialize(new Vector3(0,0,1), "Assets/Resources/Audio/mgmt.wav");
        // selectedModel = gm;

    }

    void Update() {
    }
}
