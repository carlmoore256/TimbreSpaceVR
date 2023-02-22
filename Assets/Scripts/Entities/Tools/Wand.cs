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

[RequireComponent(typeof(FlexibleLine))]
public class Wand : TsvrTool {   
    public WandType wandType; // <= select WandType in inspector
    public override TsvrToolType ToolType { get { 
        return (TsvrToolType) TsvrToolType.Parse(typeof(TsvrToolType), wandType.ToString()); 
    } }

    [SerializeField] private Transform wandTipAnchor;
    [SerializeField] private Transform wandTipTarget;
    [SerializeField] private Transform wandTip;
    [SerializeField] private Transform wandBase;
    [SerializeField] private Animator pushButton;

    [SerializeField] private Image distanceIndicator;
    [SerializeField] private Image radiusIndicator;

    private Vector3[] linePositions;
    private TwistLockAction wandDistanceTwistAction;
    private TwistLockAction wandSizeTwistAction;
    public WandTipCollider WandTipCollider { get; protected set; }
    private FlexibleLine wandLine;

    public void OnEnable() {
        wandLine = GetComponent<FlexibleLine>();
        wandLine.enabled = true;
        wandLine.Initialize(wandBase, wandTip, wandTipTarget);
        WandTipCollider = wandTip.gameObject.AddComponent<WandTipCollider>();
 
        inputActionHandlers.Add(
            ControllerActions.TriggerValue, 
            new InputActionValueHandler(ControllerActions.TriggerValue, this)
        );

        inputActionHandlers[ControllerActions.TriggerValue].AddObserver(
            InputActionValueHandler.ActionType.Value,
            (value) => {
                // Debug.Log("Trigger value: " + value + " | Num colliders hit: " + WandTipCollider.CollisionQueueStats());
                WandTipCollider.PlayCollidedGrains(value * value);
        });

        ControllerActions.toolAxis2D.action.performed += ChangeWandAxis2D;

        TwistLockOptions distanceOptions = new TwistLockOptions(
            minAngle: -180f, maxAngle: 180f, 
            minValue: TsvrApplication.Settings.WandMinDist, maxValue: TsvrApplication.Settings.WandMaxDist,
            initValue: 1f, incrementAmount: TsvrApplication.Settings.WandDistIncrement,
            reverse: ControllerActions.Hand == ControllerHand.Right
        );

        TwistLockOptions sizeOptions = new TwistLockOptions(
            minAngle: -180f, maxAngle: 180f, 
            minValue: TsvrApplication.Settings.WandMinRadius, maxValue: TsvrApplication.Settings.WandMaxRadius,
            initValue: 0.5f, incrementAmount: TsvrApplication.Settings.WandSizeIncrement,
            reverse: ControllerActions.Hand == ControllerHand.Left
        );

        // changes the distance of the wand
        wandDistanceTwistAction = new TwistLockAction(distanceOptions, ControllerActions.twistLock.action, ControllerActions.rotationAction.action, ChangeWandDistance);

        // changes the size of the wand
        wandSizeTwistAction = new TwistLockAction(sizeOptions, ControllerActions.toolOption.action, ControllerActions.rotationAction.action, ChangeWandSize);

        if (animations != null) {
            TsvrApplication.DebugLogger.Log("Playing equip animation");
            animations.Play("EquipPlayWand");
            // animations['EquipPlayWand'].wrapMode = WrapMode.Once;
            // animations.Play("Base.EquipPlayWand", -1, 0f);
        }
    }


    public void OnDisable() {
        if (animations != null) {
            TsvrApplication.DebugLogger.Log("Playing unequip animation");
            animations.Play("UnequipPlayWand");
        }
        // UnsubscribeActions();
        wandDistanceTwistAction.UnsubscribeActions();
        wandSizeTwistAction.UnsubscribeActions();  
        // Destroy(wandLine);
        wandLine.enabled = false;
        ControllerActions.toolAxis2D.action.performed -= ChangeWandAxis2D;
    }

    protected virtual void WandUpdate() {}

    protected virtual void ChangeWandAxis2D(InputAction.CallbackContext context) {
        // change wand distance based on vector 2 y, and size based on vector 2 x
        Vector2 value = context.ReadValue<Vector2>();
        ChangeWandDistance(value.y);
        ChangeWandSize(value.x);
    }

    protected virtual void ChangeWandDistance(float value) {
        Vector3 newPos = wandBase.position;
        newPos += (wandBase.up * value);
        wandTipAnchor.position = newPos;
        distanceIndicator.fillAmount = (
            value - TsvrApplication.Settings.WandMinDist) / 
            (TsvrApplication.Settings.WandMaxDist - TsvrApplication.Settings.WandMinDist
        );
    }


    protected virtual void ChangeWandSize(float value) {
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
        // wandLine.endWidth = (value * 0.5f);
        // wandLine.
        radiusIndicator.fillAmount = (
            value - TsvrApplication.Settings.WandMinRadius) / 
            (TsvrApplication.Settings.WandMaxRadius - TsvrApplication.Settings.WandMinRadius
        );

        wandLine.SetEndWidth(value * 0.5f);
    }
}




    // ============================================================
    // Subscribe Tool Actions to Control Actions (e.g. twist lock)
    // ============================================================
    // protected override void SubscribeActions() {
    //     // changes the distance of the wand
    //     wandDistanceTwistAction = new TwistLockAction(
    //         -180f, 180f, 
    //         TsvrApplication.Settings.WandMinDist.value, TsvrApplication.Settings.WandMaxDist.value,
    //         1f,
    //         TsvrApplication.Settings.WandDistIncrement.value,// <- eventually set these from global parameters
    //         ControllerActions.twistLock.action,
    //         ControllerActions.rotationAction.action,
    //         ChangeWandDistance,
    //         ControllerActions.Hand == ControllerHand.Right
    //     );
    //     // changes the size of the wand
    //     wandSizeTwistAction = new TwistLockAction(
    //         -180f, 180f,
    //         TsvrApplication.Settings.WandMinRadius.value, TsvrApplication.Settings.WandMaxRadius.value,
    //         0.5f,
    //         TsvrApplication.Settings.WandSizeIncrement.value,
    //         ControllerActions.toolOptionButton.action, // <- eventually set this from global parameters
    //         ControllerActions.rotationAction.action,
    //         ChangeWandSize,
    //         ControllerActions.Hand == ControllerHand.Left
    //     );
    
    //     ControllerActions.trigger.action.started += OnTriggerPress;
    //     ControllerActions.trigger.action.canceled += OnTriggerRelease;

    //     // ControllerActions.triggerValue.action.performed += OnTriggerPress;
    //     // ControllerActions.triggerValue.action.canceled += OnTriggerRelease;
    //     // ControllerActions.triggerValue.action.performed += TriggerPressEvent;
    // }

    // protected override void UnsubscribeActions() {
    //     wandDistanceTwistAction.UnsubscribeActions();
    //     wandSizeTwistAction.UnsubscribeActions();        
    //     ControllerActions.trigger.action.started -= OnTriggerPress;
    //     ControllerActions.trigger.action.canceled -= OnTriggerRelease;
    // }


// void Update() {
//     wandLine.SetPositions(CalculateLinePositions());
// }

// if (isTrigPressed) {
//     // currentGain = ControllerActions.triggerValue.action.ReadValue<float>();
//     // WandTipCollider.RunActionOnColliders(GrainPlayAction);
//     float gain = ControllerActions.triggerValue.action.ReadValue<float>();
//     WandTipCollider.PlayCollidedGrains(gain * gain);
// }
// void Update() {
//     if (isTrigPressed) {
//         // currentGain = ControllerActions.triggerValue.action.ReadValue<float>();
//         // WandTipCollider.RunActionOnColliders(GrainPlayAction);
//         float gain = ControllerActions.triggerValue.action.ReadValue<float>();
//         WandTipCollider.PlayCollidedGrains(gain * gain);
//     }
//     wandLine.SetPositions(CalculateLinePositions());
// }


// private Vector3[] CalculateLinePositions() {
//     for (int i = 0; i < wandSettings.numSegments; i++) {
//         float t = (float)i / (float)(wandSettings.numSegments);
//         Vector3 tipTargetPosition = wandTip.position;
//         if (wandSettings.isElastic) // give the wand an elastic feel
//             tipTargetPosition = Vector3.Lerp(wandTip.position, wandTipTarget.position, t * wandSettings.elasticity);
//         linePositions[wandSettings.numSegments-i-1] = Vector3.Lerp(wandBase.position, tipTargetPosition, 1-t);
//     }
//     return linePositions;
// }