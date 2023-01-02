using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum WandType {
    PlayWand = TsvrToolType.PlayWand,
    EditWand = TsvrToolType.EditWand,
    ConstellationWand = TsvrToolType.ConstellationWand,
    MeasureWand = TsvrToolType.MeasureWand,
    PaintWand = TsvrToolType.PaintWand,
    SelectWand = TsvrToolType.SelectWand,
}

public class Wand : TsvrTool {

    // select in inspector
    public WandType wandType;

    // allows us to convert wandType to TsvrToolType
    public new TsvrToolType ToolType { get { return (TsvrToolType) TsvrToolType.Parse(typeof(TsvrToolType), wandType.ToString()); } }

    private Transform wandTipAnchor;
    private Transform wandTipTarget;
    private Transform wandTip;
    private Transform wandBase;
    private Animator pushButton;
    private int numLineSegments;    
    private Vector3[] linePositions;

    // public Canvas statusDisplay;
    public Image distanceIndicator;
    public Image radiusIndicator;

    private TwistLockAction wandDistanceTwistAction;
    private TwistLockAction wandSizeTwistAction;

    private GameObject lineObject;
    private LineRenderer wandLine;
    private float wandLineElasticity;

    public WandTipCollider WandTipCollider { get; protected set; }

    private bool isTrigPressed = false;
    private bool isElasticWand = false;

    public void OnEnable() {
        wandBase = transform.Find("Base");
        wandTipAnchor = transform.Find("TipAnchor");
        wandTipTarget = wandTipAnchor.Find("Target");
        wandTip = wandTipAnchor.Find("Tip");
        pushButton = transform.Find("PushButton").GetComponent<Animator>();

        // distanceIndicator = statusDisplay.gameObject.transform.Find("DistanceIndicator").GetComponent<Image>();
        // radiusIndicator = statusDisplay.gameObject.transform.Find("RadiusIndicator").GetComponent<Image>();

        WandTipCollider = wandTip.gameObject.AddComponent<WandTipCollider>();
        

        lineObject = Instantiate(TsvrApplication.Config.lineObjectPrefab);
        wandLine = lineObject.GetComponent<LineRenderer>();
        numLineSegments = TsvrApplication.Settings.WandLineSegments;
        wandLineElasticity = TsvrApplication.Settings.WandLineElasticity;
        isElasticWand = TsvrApplication.Settings.EnableElasticWand;

        wandLine.positionCount = numLineSegments;
        linePositions = new Vector3[numLineSegments];
        wandLine.SetPositions(CalculateLinePositions());
        if (!isElasticWand) {
            Destroy(wandTip.GetComponent<SpringJoint>());
            wandTip.GetComponent<Rigidbody>().isKinematic = true;
            lineObject.transform.parent = transform;
            wandLine.useWorldSpace = true;
        }
        
        SubscribeActions();
        if (animations != null) {
            Debug.Log("Playing equip animation");
            animations.Play("EquipPlayWand");
            // animations['EquipPlayWand'].wrapMode = WrapMode.Once;
            // animations.Play("Base.EquipPlayWand", -1, 0f);
        }
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
        // changes the distance of the wand
        wandDistanceTwistAction = new TwistLockAction(
            -180f, 180f, 
            TsvrApplication.Settings.WandMinDist, TsvrApplication.Settings.WandMaxDist,
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
            TsvrApplication.Settings.WandMinRadius, TsvrApplication.Settings.WandMaxRadius,
            0.5f,
            TsvrApplication.Settings.wandDistIncrement,
            ControllerActions.toolOptionButton.action, // <- eventually set this from global parameters
            ControllerActions.rotationAction.action,
            ChangeWandSize,
            ControllerActions.Hand == ControllerHand.Left
        );
    
        ControllerActions.trigger.action.started += OnTriggerPress;
        ControllerActions.trigger.action.canceled += OnTriggerRelease;

        // ControllerActions.triggerValue.action.performed += OnTriggerPress;
        // ControllerActions.triggerValue.action.canceled += OnTriggerRelease;
        // ControllerActions.triggerValue.action.performed += TriggerPressEvent;
    }

    public void UnsubscribeActions() {
        wandDistanceTwistAction.UnsubscribeActions();
        wandSizeTwistAction.UnsubscribeActions();
        
        ControllerActions.trigger.action.started -= OnTriggerPress;
        ControllerActions.trigger.action.canceled -= OnTriggerRelease;
        // ControllerActions.triggerValue.action.performed -= TriggerPressEvent;
    }

    void OnTriggerPress(InputAction.CallbackContext context) {
        Debug.Log("Trigger Pressed!");
        isTrigPressed = true;
    }

    void OnTriggerRelease(InputAction.CallbackContext context) {
        Debug.Log("Trigger Released!");
        isTrigPressed = false;
    }

    void TriggerPressEvent(InputAction.CallbackContext context) {
        if (context.ReadValue<float>() < 0.001f) return;
        foreach(Collider collider in WandTipCollider.ColliderBuffer) {
            collider.gameObject.GetComponent<Grain>().PlayGrain();
        }
    }

    public override void Destroy() {
        UnsubscribeActions();
        // TsvrApplication.AudioManager.PlayInterfaceSound(Prefab)
    }

    void Update() {
        if (isTrigPressed) {
            foreach(Collider collider in WandTipCollider.ColliderBuffer) {
                if (collider.tag == "grain") {
                    float gain = ControllerActions.triggerValue.action.ReadValue<float>();
                    collider.gameObject.GetComponent<Grain>().PlayGrain(gain);
                }
            }
        }
        if (isElasticWand)
            wandLine.SetPositions(CalculateLinePositions());
        else
            wandLine.SetPositions(CalculateLinePositionsStatic());
    }

    private Vector3[] CalculateLinePositionsStatic() {
        // Vector3[] positions = new Vector3[numPositions];
        for (int i = 0; i < numLineSegments; i++) {
            float t = (float)i / (float)(numLineSegments);
            linePositions[numLineSegments-i-1] = Vector3.Lerp(wandBase.position, wandTip.position, 1-t);
        }
        return linePositions;
    }

    private Vector3[] CalculateLinePositions() {
        // Vector3[] positions = new Vector3[numLineSegments];
        for (int i = 0; i < numLineSegments; i++) {
            float t = (float)i / (float)(numLineSegments);
            // give the wand an elastic feel
            Vector3 tipTargetPosition = Vector3.Lerp(
                wandTip.position, wandTipTarget.position, t * wandLineElasticity);
            linePositions[numLineSegments-i-1] = Vector3.Lerp(wandBase.position, tipTargetPosition, 1-t);
        }
        return linePositions;
    }

    private void ChangeWandDistance(float value) {
        Vector3 newPos = wandBase.position;
        newPos += (wandBase.up * value);
        wandTipAnchor.position = newPos;
        distanceIndicator.fillAmount = (
            value - TsvrApplication.Settings.WandMinDist) / 
            (TsvrApplication.Settings.WandMaxDist - TsvrApplication.Settings.WandMinDist
        );
    }

    private void ChangeWandSize(float value) {
        Vector3 newScale = new Vector3(value, value, value);
        if (newScale.magnitude > TsvrApplication.Settings.WandMaxRadius) {
            newScale = newScale.normalized * TsvrApplication.Settings.WandMaxRadius;
        } else if (newScale.magnitude < TsvrApplication.Settings.WandMinRadius) {
            newScale = newScale.normalized * TsvrApplication.Settings.WandMinRadius;
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
            value - TsvrApplication.Settings.WandMinRadius) / 
            (TsvrApplication.Settings.WandMaxRadius - TsvrApplication.Settings.WandMinRadius
        );
    }
}