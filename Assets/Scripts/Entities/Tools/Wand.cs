using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;

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

    [SerializeField] protected Image distanceIndicator;
    [SerializeField] protected Image radiusIndicator;

    private Vector3[] linePositions;
    private TwistLockAction wandDistanceTwistLock;
    private TwistLockAction wandRadiusTwistLock;
    private FlexibleLine wandLine;

    private FloatInputActionHandler triggerValueHandler;
    private FloatInputActionHandler gripValueHandler;

    private float distanceCurrent = 1f;
    private float radiusCurrent = 1f;

    public void OnEnable() {
        wandLine = GetComponent<FlexibleLine>();
        wandLine.enabled = true;
        wandLine.Initialize(wandBase, wandTip, wandTipTarget);

        triggerValueHandler = new FloatInputActionHandler(ControllerActions.TriggerValue, this);

        triggerValueHandler.AddObserver(FloatInputActionHandler.ActionType.Value, (value) => {
            Collider[] hitColliders = Physics.OverlapSphere(wandTip.transform.position, wandTip.transform.localScale.x * 0.51f, 1<<7);
            CollectionHelpers.Shuffle<Collider>(hitColliders);
            foreach (Collider hitCollider in hitColliders) {
                // hitCollider.GetComponent<GrainOld>().PlayGrain(value * value);
                hitCollider.GetComponent<Grain>().Activate(value * value, Grain.ActivationAction.Play);
            }
        });

        ControllerActions.AddListener(ControllerActions.toolAxis2D, ChangeWandAxis2D, InputActionPhase.Performed);

        wandDistanceTwistLock = new TwistLockAction(
            new TwistLockOptions(minAngle: -180f, maxAngle: 180f, reverse: ControllerActions.Hand == ControllerHand.Left),
            ControllerActions, 
            ControllerActions.twistLock, 
            (value) => {
                float newDistance = distanceCurrent + (value * TsvrApplication.Settings.WandDistIncrement);
                newDistance = Mathf.Clamp(newDistance, TsvrApplication.Settings.WandMinDist, TsvrApplication.Settings.WandMaxDist);
                ChangeWandDistance(newDistance);
            },
            (value) => {
                float newDistance = distanceCurrent + (value * TsvrApplication.Settings.WandDistIncrement);
                distanceCurrent = Mathf.Clamp(newDistance, TsvrApplication.Settings.WandMinDist, TsvrApplication.Settings.WandMaxDist);
            }
        );

        wandRadiusTwistLock = new TwistLockAction(
            new TwistLockOptions(minAngle: -180f, maxAngle: 180f, reverse: ControllerActions.Hand == ControllerHand.Right),
            ControllerActions, 
            ControllerActions.toolOption, 
            (value) => {
                float newRadius = radiusCurrent + (value * TsvrApplication.Settings.WandSizeIncrement);
                newRadius = Mathf.Clamp(newRadius, TsvrApplication.Settings.WandMinRadius, TsvrApplication.Settings.WandMaxRadius);
                ChangeWandSize(newRadius);
            },
            (value) => {
                float newRadius = radiusCurrent + (value * TsvrApplication.Settings.WandSizeIncrement);
                radiusCurrent = Mathf.Clamp(newRadius, TsvrApplication.Settings.WandMinRadius, TsvrApplication.Settings.WandMaxRadius);
            }
        );

        if (animations != null) {
            DebugLogger.Log("Playing equip animation");
            animations.Play("EquipPlayWand");
        }
    }


    public void OnDisable() {
        if (animations != null) {
            DebugLogger.Log("Playing unequip animation");
            animations.Play("UnequipPlayWand");
        }
        wandDistanceTwistLock.UnsubscribeActions();
        wandRadiusTwistLock.UnsubscribeActions();  
        // Destroy(wandLine);
        wandLine.enabled = false;
        ControllerActions.RemoveListener(ControllerActions.toolAxis2D, ChangeWandAxis2D, InputActionPhase.Performed);

        triggerValueHandler.Disable();
    }

    protected virtual void WandUpdate() {}

    protected virtual void ChangeWandAxis2D(InputAction.CallbackContext context) {
        // change wand distance based on vector 2 y, and size based on vector 2 x
        Vector2 value = context.ReadValue<Vector2>();
        distanceCurrent = Mathf.Clamp(distanceCurrent + (value.y * TsvrApplication.Settings.WandDistIncrement * 0.1f), TsvrApplication.Settings.WandMinDist, TsvrApplication.Settings.WandMaxDist);
        radiusCurrent = Mathf.Clamp(radiusCurrent + (value.x * TsvrApplication.Settings.WandSizeIncrement * 0.1f), TsvrApplication.Settings.WandMinRadius, TsvrApplication.Settings.WandMaxRadius);
        ChangeWandDistance(distanceCurrent);
        ChangeWandSize(radiusCurrent);
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
        // if (newScale.magnitude > TsvrApplication.Settings.WandMaxRadius) {
        //     newScale = newScale.normalized * TsvrApplication.Settings.WandMaxRadius;
        // } else if (newScale.magnitude < TsvrApplication.Settings.WandMinRadius) {
        //     newScale = newScale.normalized * TsvrApplication.Settings.WandMinRadius;
        // }

        // Reduce the max size of the wand when it's close to the tool
        // float proximityScalar = 1f;
        // float outerDistance = Vector3.Distance(wandTip.position, wandBase.position);
        // if (outerDistance - value < 0) {
        //     proximityScalar *= 1/(outerDistance - value);
        // }

        wandTip.transform.localScale = newScale;
        radiusIndicator.fillAmount = (
            value - TsvrApplication.Settings.WandMinRadius) / 
            (TsvrApplication.Settings.WandMaxRadius - TsvrApplication.Settings.WandMinRadius
        );

        wandLine.SetEndWidth(value * 0.5f);
    }
}