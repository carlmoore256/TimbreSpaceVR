using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
// ============================---------===================================
// ============================-M-E-N-U-===================================
// ============================---------===================================

public class Menu : TsvrTool
{
    public new TsvrToolType ToolType = TsvrToolType.Menu;

    public Menu() {}
    public Menu(ActionBasedController controller) {
        Controller = controller;
        Spawn(controller);
    }

    // public TsvrToolType ToolType { get { return TsvrToolType.Menu; } }
    public ActionBasedController Controller { get; set; }
    
    [SerializeField]
    public GameObject Prefab { get; }

    public GameObject GameObject { get; protected set; }
    // private GameObject _prefab;
    // public GameObject Prefab { get {
    //     if (_prefab == null) {
    //         _prefab = Resources.Load("Prefabs/TSVR_MenuBasic") as GameObject;
    //     }
    //     return _prefab;
    // } }


    void OnEnable() {
        SubscribeActions();
    }

    public void SubscribeActions() {}

    public void UnsubscribeActions() {}

    public void Spawn(ActionBasedController controller) {
        controller.modelPrefab = Prefab.transform;
        Controller = controller;
    }

    public void Destroy() {
        UnsubscribeActions();
        Controller.modelPrefab = null;
    }
}