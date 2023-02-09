using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class WandPlay : Wand {

    // select in inspector
    // public override WandType wandType;

    // allows us to convert wandType to TsvrToolType
    // public new TsvrToolType ToolType { get { return (TsvrToolType) TsvrToolType.Parse(typeof(TsvrToolType), wandType.ToString()); } }

    public override TsvrToolType ToolType { get => TsvrToolType.PlayWand; }

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
        

        lineObject = Instantiate(TsvrApplication.Config.flexibleLinePrefab);
        wandLine = lineObject.GetComponent<LineRenderer>();
        numLineSegments = TsvrApplication.Settings.WandLineSegments.value;
        wandLineElasticity = TsvrApplication.Settings.WandLineElasticity.value;
        isElasticWand = TsvrApplication.Settings.EnableElasticWand.value;

        wandLine.positionCount = numLineSegments;
        linePositions = new Vector3[numLineSegments];
        wandLine.SetPositions(CalculateLinePositions());
        if (!isElasticWand) {
            Destroy(wandTip.GetComponent<SpringJoint>());
            wandTip.GetComponent<Rigidbody>().isKinematic = true;
            lineObject.transform.parent = transform;
            wandLine.useWorldSpace = true;
        }
        
        // SubscribeActions();
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
    
    public void UnsubscribeActions() {
        wandDistanceTwistAction.UnsubscribeActions();
        wandSizeTwistAction.UnsubscribeActions();
        
        // ControllerActions.trigger.action.started -= OnTriggerPress;
        // ControllerActions.trigger.action.canceled -= OnTriggerRelease;
        // ControllerActions.triggerValue.action.performed -= TriggerPressEvent;
    }

    void Update() {
        if (isTrigPressed) {
            // currentGain = ControllerActions.triggerValue.action.ReadValue<float>();
            // WandTipCollider.RunActionOnColliders(GrainPlayAction);
            float gain = ControllerActions.triggerValue.action.ReadValue<float>();
            WandTipCollider.PlayCollidedGrains(gain * gain);
        }
        wandLine.SetPositions(CalculateLinePositions());
    }

    float currentGain = 0f;
    private void GrainPlayAction(GameObject grainObject) {
        grainObject.GetComponent<Grain>().PlayGrain(currentGain);
    }


    private Vector3[] CalculateLinePositions() {
        // Vector3[] positions = new Vector3[numLineSegments];
        for (int i = 0; i < numLineSegments; i++) {
            float t = (float)i / (float)(numLineSegments);

            Vector3 tipTargetPosition = wandTip.position;
            if (isElasticWand) // give the wand an elastic feel
                tipTargetPosition = Vector3.Lerp(wandTip.position, wandTipTarget.position, t * wandLineElasticity);

            linePositions[numLineSegments-i-1] = Vector3.Lerp(wandBase.position, tipTargetPosition, 1-t);
        }
        return linePositions;
    }

    private void ChangeWandDistance(float value) {
        Vector3 newPos = wandBase.position;
        newPos += (wandBase.up * value);
        wandTipAnchor.position = newPos;
        distanceIndicator.fillAmount = (
            value - TsvrApplication.Settings.WandMinDist.value) / 
            (TsvrApplication.Settings.WandMaxDist.value - TsvrApplication.Settings.WandMinDist.value
        );
    }

    private void ChangeWandSize(float value) {
        Vector3 newScale = new Vector3(value, value, value);
        if (newScale.magnitude > TsvrApplication.Settings.WandMaxRadius.value) {
            newScale = newScale.normalized * TsvrApplication.Settings.WandMaxRadius.value;
        } else if (newScale.magnitude < TsvrApplication.Settings.WandMinRadius.value) {
            newScale = newScale.normalized * TsvrApplication.Settings.WandMinRadius.value;
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
            value - TsvrApplication.Settings.WandMinRadius.value) / 
            (TsvrApplication.Settings.WandMaxRadius.value - TsvrApplication.Settings.WandMinRadius.value
        );
    }
}