using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;

public enum WandType
{
    PlayWand = TsvrToolType.PlayWand,
    EditWand = TsvrToolType.EditWand,
    ConstellationWand = TsvrToolType.ConstellationWand,
    MeasureWand = TsvrToolType.MeasureWand,
    PaintWand = TsvrToolType.PaintWand,
    SelectWand = TsvrToolType.SelectWand,
}

[RequireComponent(typeof(FlexibleLine))]
public class Wand : TsvrTool
{
    public WandType wandType; // <= select WandType in inspector
    public override TsvrToolType ToolType
    {
        get { return (TsvrToolType)TsvrToolType.Parse(typeof(TsvrToolType), wandType.ToString()); }
    }

    [SerializeField]
    private Transform wandTipAnchor;

    [SerializeField]
    private Transform wandTipTarget;

    [SerializeField]
    private Transform wandTip;

    [SerializeField]
    private Transform wandBase;

    [SerializeField]
    private Animator pushButton;

    [SerializeField]
    protected Image distanceIndicator;

    [SerializeField]
    protected Image radiusIndicator;

    private TwistLockAction _wandDistanceTwistLock;
    private TwistLockAction _wandRadiusTwistLock;
    private FlexibleLine _wandLine;

    private FloatInputActionHandler _triggerValueHandler;
    private FloatInputActionHandler _gripValueHandler;

    private float _distanceCurrent = 1f;
    private float _radiusCurrent = 1f;

    public void OnEnable()
    {
        _wandLine = GetComponent<FlexibleLine>();
        _wandLine.enabled = true;
        _wandLine.Initialize(wandBase, wandTip, wandTipTarget);

        _triggerValueHandler = new FloatInputActionHandler(ControllerActions.TriggerValue, this);

        _triggerValueHandler.AddObserver(
            FloatInputActionHandler.ActionType.Value,
            (value) =>
            {
                Collider[] hitColliders = Physics.OverlapSphere(
                    wandTip.transform.position,
                    wandTip.transform.localScale.x * 0.51f,
                    1 << 7
                );
                CollectionHelpers.Shuffle<Collider>(hitColliders);
                foreach (Collider hitCollider in hitColliders)
                {
                    // @optimize: make a static class that has reference to all grain colliders,
                    // and this can call that instead of doing a GetComponent
                    hitCollider
                        .GetComponent<Grain>()
                        .DoWandInteraction(
                            new WandInteraction()
                            {
                                ActionType = WandInteractionType.Play,
                                Value = value * value
                            }
                        );
                }
            }
        );

        ControllerActions.AddListener(
            ControllerActions.toolAxis2D,
            ChangeWandAxis2D,
            InputActionPhase.Performed
        );

        _wandDistanceTwistLock = new TwistLockAction(
            new TwistLockOptions(
                minAngle: -180f,
                maxAngle: 180f,
                reverse: ControllerActions.Hand == ControllerHand.Left
            ),
            ControllerActions,
            ControllerActions.twistLock,
            (value) =>
            {
                float newDistance =
                    _distanceCurrent + (value * TsvrApplication.Settings.WandDistIncrement);
                newDistance = Mathf.Clamp(
                    newDistance,
                    TsvrApplication.Settings.WandMinDist,
                    TsvrApplication.Settings.WandMaxDist
                );
                ChangeWandDistance(newDistance);
            },
            (value) =>
            {
                float newDistance =
                    _distanceCurrent + (value * TsvrApplication.Settings.WandDistIncrement);
                _distanceCurrent = Mathf.Clamp(
                    newDistance,
                    TsvrApplication.Settings.WandMinDist,
                    TsvrApplication.Settings.WandMaxDist
                );
            }
        );

        _wandRadiusTwistLock = new TwistLockAction(
            new TwistLockOptions(
                minAngle: -180f,
                maxAngle: 180f,
                reverse: ControllerActions.Hand == ControllerHand.Right
            ),
            ControllerActions,
            ControllerActions.toolOption,
            (value) =>
            {
                float newRadius =
                    _radiusCurrent + (value * TsvrApplication.Settings.WandSizeIncrement);
                newRadius = Mathf.Clamp(
                    newRadius,
                    TsvrApplication.Settings.WandMinRadius,
                    TsvrApplication.Settings.WandMaxRadius
                );
                ChangeWandSize(newRadius);
            },
            (value) =>
            {
                float newRadius =
                    _radiusCurrent + (value * TsvrApplication.Settings.WandSizeIncrement);
                _radiusCurrent = Mathf.Clamp(
                    newRadius,
                    TsvrApplication.Settings.WandMinRadius,
                    TsvrApplication.Settings.WandMaxRadius
                );
            }
        );

        if (animations != null)
        {
            DebugLogger.Log("Playing equip animation");
            animations.Play("EquipPlayWand");
        }
    }

    public void OnDisable()
    {
        if (animations != null)
        {
            DebugLogger.Log("Playing unequip animation");
            animations.Play("UnequipPlayWand");
        }
        _wandDistanceTwistLock.UnsubscribeActions();
        _wandRadiusTwistLock.UnsubscribeActions();
        // Destroy(wandLine);
        _wandLine.enabled = false;
        ControllerActions.RemoveListener(
            ControllerActions.toolAxis2D,
            ChangeWandAxis2D,
            InputActionPhase.Performed
        );

        _triggerValueHandler.Disable();
    }

    protected virtual void WandUpdate() { }

    protected virtual void ChangeWandAxis2D(InputAction.CallbackContext context)
    {
        // change wand distance based on vector 2 y, and size based on vector 2 x
        Vector2 value = context.ReadValue<Vector2>();
        _distanceCurrent = Mathf.Clamp(
            _distanceCurrent + (value.y * TsvrApplication.Settings.WandDistIncrement * 0.1f),
            TsvrApplication.Settings.WandMinDist,
            TsvrApplication.Settings.WandMaxDist
        );
        _radiusCurrent = Mathf.Clamp(
            _radiusCurrent + (value.x * TsvrApplication.Settings.WandSizeIncrement * 0.1f),
            TsvrApplication.Settings.WandMinRadius,
            TsvrApplication.Settings.WandMaxRadius
        );
        ChangeWandDistance(_distanceCurrent);
        ChangeWandSize(_radiusCurrent);
    }

    protected virtual void ChangeWandDistance(float value)
    {
        Vector3 newPos = wandBase.position;
        newPos += (wandBase.up * value);
        wandTipAnchor.position = newPos;
        distanceIndicator.fillAmount =
            (value - TsvrApplication.Settings.WandMinDist)
            / (TsvrApplication.Settings.WandMaxDist - TsvrApplication.Settings.WandMinDist);
    }

    protected virtual void ChangeWandSize(float value)
    {
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
        radiusIndicator.fillAmount =
            (value - TsvrApplication.Settings.WandMinRadius)
            / (TsvrApplication.Settings.WandMaxRadius - TsvrApplication.Settings.WandMinRadius);

        _wandLine.SetEndWidth(value * 0.5f);
    }
}
