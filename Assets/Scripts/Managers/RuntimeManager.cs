using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class RuntimeManager : MonoBehaviour
{
    [SerializeField]
    public ScriptableObject masterManager;

    public ActionBasedController leftHandController;
    public ActionBasedController rightHandController;

    public ToolController LeftHandTool { get; set; }
    public ToolController RightHandTool { get; set; }
    
    // public Transform XRRig { set {
    //     if (value == null) {
    //         Debug.LogError("XRRig is null!");
    //         return;
    //     }
    //     rightHandController = value.Find("TrackingSpace/RightHandAnchor").GetComponent<ActionBasedController>();
    //     leftHandController = value.Find("TrackingSpace/LeftHandAnchor").GetComponent<ActionBasedController>();

    //     leftHandTool = new HandTool(leftHandController, TsvrToolType.Wand);
    //     rightHandTool = new HandTool(rightHandController, TsvrToolType.Wand);
    // } }

    void Awake() {
    }

    void Start()
    {
        // LeftHandTool = new ToolController(leftHandController, TsvrToolType.PlayWand);
        // RightHandTool = new ToolController(rightHandController, TsvrToolType.PlayWand);
        // Debug.Log(TsvrApplication.PlayerProperties.LeftHand.name);
        // leftHandTool = new HandTool(TsvrApplication.PlayerProperties.LeftHand.GetComponent<ActionBasedController>(), TsvrToolType.Wand);
        // rightHandTool = new HandTool(TsvrApplication.PlayerProperties.RightHand.GetComponent<ActionBasedController>(), TsvrToolType.Wand);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializeHands() {

    }
}
