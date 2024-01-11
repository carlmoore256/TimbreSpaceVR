using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class WandPlay : Wand {
    public override TsvrToolType ToolType { get => TsvrToolType.PlayWand; }
    private Transform wandTipAnchor;
    private Transform wandTipTarget;
    private Transform wandTip;
    private Transform wandBase;
    private Animator pushButton;
    private int numLineSegments;
    private Vector3[] linePositions;
    private TwistLockAction wandDistanceTwistAction;
    private TwistLockAction wandSizeTwistAction;
    private GameObject lineObject;
    private LineRenderer wandLine;
    private float wandLineElasticity;
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

        // ActiveCollisions = wandTip.gameObject.AddComponent<WandTipCollider>();
        

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
        
        if (animations != null) {
            Debug.Log("Playing equip animation");
            animations.Play("EquipPlayWand");
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
    }

    float triggerValue = 0f;
    void Update() {
        if (isTrigPressed) {
            triggerValue = ControllerActions.triggerValue.action.ReadValue<float>();
            // ActiveCollisions.PlayCollidedGrains(triggerValue * triggerValue);
        }
        wandLine.SetPositions(CalculateLinePositions());
    }

    float currentGain = 0f;
    private void GrainPlayAction(GameObject grainObject) {
        grainObject.GetComponent<GrainOld>().PlayGrain(currentGain);
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
}