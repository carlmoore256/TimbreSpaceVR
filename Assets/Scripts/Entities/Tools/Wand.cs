using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Wand : TsvrTool {

    public Transform wandTipAnchor;
    public Transform wandTipTarget;
    public Transform wandTip;
    public Transform wandBase;
    public Animator pushButton;
    private int numLineSegments;    
    private Vector3[] linePositions;

    public Image distanceIndicator;
    public Image radiusIndicator;

    private TwistLockAction wandDistanceTwistAction;
    private TwistLockAction wandSizeTwistAction;

    public GameObject lineObjectPrefab;
    private GameObject lineObject;
    private LineRenderer wandLine;
    private float wandLineElasticity;

    public ActionBasedController Controller { get; set; }
    public Transform ToolGameObject { get; protected set; }

    // void Start() {
    //     ToolType = TsvrToolType.PlayWand;
    //     toolController = transform.parent.GetComponent<ToolController>();
    // }

    public WandTipCollider WandTipCollider { get; protected set; }

    public void OnEnable() {
        ToolType = TsvrToolType.PlayWand; // find out a better way to do this, maybe tools aren't structured perfectly...
        lineObject = Instantiate(lineObjectPrefab);
        wandLine = lineObject.GetComponent<LineRenderer>();
        numLineSegments = TsvrApplication.Settings.wandLineSegments;
        wandLineElasticity = TsvrApplication.Settings.wandLineElasticity;
        wandLine.positionCount = numLineSegments;

        WandTipCollider = wandTip.gameObject.AddComponent<WandTipCollider>();
        
        SubscribeActions();
        if (animations != null) {
            Debug.Log("Playing equip animation");
            animations.Play("EquipPlayWand");
            // animations['EquipPlayWand'].wrapMode = WrapMode.Once;
            // animations.Play("Base.EquipPlayWand", -1, 0f);
        }
    }

    void Update() {
        wandLine.SetPositions(CalculateLinePositions(numLineSegments));
    }

    public void OnDisable() {
        if (animations != null) {
            Debug.Log("Playing unequip animation");
            animations.Play("UnequipPlayWand");
        }
        UnsubscribeActions();
        Destroy(lineObject);
    }

    // ============================================================
    // Subscribe Tool Actions to Control Actions (e.g. twist lock)
    // ============================================================
    public void SubscribeActions() {
        // ControllerActions controllerActions = transform.parent.GetComponent<ControllerActions>();

        // changes the distance of the wand
        wandDistanceTwistAction = new TwistLockAction(
            -180f, 180f, 
            TsvrApplication.Settings.wandMinDist, TsvrApplication.Settings.wandMaxDist,
            1f,
            TsvrApplication.Settings.wandDistIncrement,// <- eventually set these from global parameters
            ControllerActions.twistLock.action,
            ControllerActions.rotationAction.action,
            ChangeWandDistance,
            ControllerActions.Hand == ControllerHand.Left
        );
        // changes the size of the wand
        wandSizeTwistAction = new TwistLockAction(
            -180f, 180f,
            TsvrApplication.Settings.wandMinRadius, TsvrApplication.Settings.wandMaxRadius,
            0.5f,
            TsvrApplication.Settings.wandDistIncrement,
            ControllerActions.toolOptionButton.action, // <- eventually set this from global parameters
            ControllerActions.rotationAction.action,
            ChangeWandSize,
            ControllerActions.Hand == ControllerHand.Left
        );


        ControllerActions.triggerValue.action.performed += TriggerTest;
    }

    void TriggerTest(InputAction.CallbackContext context) {
        Debug.Log("TriggerTest" + context.ReadValue<float>());
        if (context.ReadValue<float>() < 0.001f) return;
        foreach(Collider collider in WandTipCollider.ColliderBuffer) {
            if (collider.tag == "grain") {
                collider.gameObject.GetComponent<Grain>().PlayGrain();
            }
        }
    }

    public void UnsubscribeActions() {
        wandDistanceTwistAction.UnsubscribeActions();
        wandSizeTwistAction.UnsubscribeActions();

        ControllerActions.triggerValue.action.performed -= TriggerTest;
    }

    public void Spawn(ActionBasedController controller) {
    }

    public override void Destroy() {
        UnsubscribeActions();
        // Controller.modelPrefab = null; <- make a custom class to deal with this instead
        // TsvrApplication.AudioManager.PlayInterfaceSound(Prefab)
    }

    // public virtual void WandActivatedCallback() {

    // }

    private Vector3[] CalculateLinePositions(int numPositions) {
        Vector3[] positions = new Vector3[numPositions];
        for (int i = 0; i < numPositions; i++) {
            float t = (float)i / (float)(numPositions);
            // give the wand an elastic feel
            Vector3 tipTargetPosition = Vector3.Lerp(
                wandTip.position, wandTipTarget.position, t * wandLineElasticity);

            positions[numPositions-i-1] = Vector3.Lerp(wandBase.position, tipTargetPosition, 1-t);
        }
        return positions;
    }

    private void ChangeWandDistance(float value) {
        Vector3 newPos = wandBase.position;
        newPos += (wandBase.up * value);
        wandTipAnchor.position = newPos;
        distanceIndicator.fillAmount = (
            value - TsvrApplication.Settings.wandMinDist) / 
            (TsvrApplication.Settings.wandMaxDist - TsvrApplication.Settings.wandMinDist
        );


    }

    private void ChangeWandSize(float value) {
        Vector3 newScale = new Vector3(value, value, value);
        if (newScale.magnitude > TsvrApplication.Settings.wandMaxRadius) {
            newScale = newScale.normalized * TsvrApplication.Settings.wandMaxRadius;
        } else if (newScale.magnitude < TsvrApplication.Settings.wandMinRadius) {
            newScale = newScale.normalized * TsvrApplication.Settings.wandMinRadius;
        }

        // Reduce the max size of the wand when it's close to the tool
        // float proximityScalar = 1f;
        // float outerDistance = Vector3.Distance(wandTip.position, wandBase.position);
        // if (outerDistance - value < 0) {
        //     proximityScalar *= 1/(outerDistance - value);
        // }

        wandTip.transform.localScale = newScale;
        wandLine.endWidth = (value * 0.5f);
        radiusIndicator.fillAmount = (
            value - TsvrApplication.Settings.wandMinRadius) / 
            (TsvrApplication.Settings.wandMaxRadius - TsvrApplication.Settings.wandMinRadius
        );
    }
}