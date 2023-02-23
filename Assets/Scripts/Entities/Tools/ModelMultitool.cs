using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ModelMultitool : TsvrTool
{
    public override TsvrToolType ToolType { get => TsvrToolType.ModelMultitool; }
    public ScrollableListUI scrollableListUI;
    public Transform lineStart;
    private LineRenderer lineRenderer;
    public LayerMask grainLayerMask;
    private GrainModel selectedModel;
    private RaycastHit raycastHit;

    private float modelScale;
    private float modelDist;
    private Quaternion modelRotationTarget;

    enum ModelMultitoolState {
        Inspecting,
        Positioning,
        Unassigned
    }

    private ModelMultitoolState state = ModelMultitoolState.Inspecting;

    public void OnEnable() {
        ControllerActions.AddListener(ControllerActions.toolAxis2D, OnAxis2D, InputActionPhase.Performed);
        ControllerActions.AddListener(ControllerActions.uiSelect, OnSubmit, InputActionPhase.Performed);
        lineRenderer = RuntimeManager.SpawnWorldSpaceLine();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, lineStart.position);
    }

    public void OnDisable() {
        ControllerActions.RemoveListener(ControllerActions.toolAxis2D, OnAxis2D, InputActionPhase.Performed);
        ControllerActions.RemoveListener(ControllerActions.uiSelect, OnSubmit, InputActionPhase.Performed);
        Destroy(lineRenderer.gameObject);
    }


    void Update()
    {
        switch(state) {
            case ModelMultitoolState.Inspecting:
                if (selectedModel != null) {
                    lineRenderer.SetPosition(1, raycastHit.point);
                    selectedModel.Inspect();
                }
                break;
            case ModelMultitoolState.Positioning:
                if (selectedModel != null) {
                    selectedModel.coroutineManager.MoveTo(
                        ToolController.transform.position + ToolController.transform.forward * modelDist, 0.5f);
                    selectedModel.coroutineManager.ScaleTo(
                        new Vector3(modelScale, modelScale, modelScale), 0.5f);
                    selectedModel.coroutineManager.RotateTo(
                        modelRotationTarget, 0.5f);
                }
                break;
            case ModelMultitoolState.Unassigned:
                RaycastTarget();
                break;
        }
    }

    public void SetCurrentModel(GrainModel model) {
        selectedModel = model;
        state = ModelMultitoolState.Positioning;
        modelDist = Vector3.Distance(ToolController.transform.position, model.transform.position);
        modelScale = model.transform.localScale.x;
        modelRotationTarget = model.transform.rotation;
    }

    private void InspectCurrentModel() {
        if (selectedModel == null) return;
        selectedModel.Inspect();
        lineRenderer.SetPosition(0, lineStart.position);
        lineRenderer.SetPosition(1, selectedModel.transform.position);
    }

    private void RaycastTarget() {
        lineRenderer.SetPosition(0, lineStart.position);
        if (Physics.Raycast(transform.position, transform.forward, out raycastHit, Mathf.Infinity, grainLayerMask))
        {
            GameObject hitObject = raycastHit.collider.gameObject;
            GrainModel grainModel = hitObject.GetComponent<GrainModel>();
            if (grainModel != null)
            {
                selectedModel = grainModel;
                selectedModel.Inspect();
                lineRenderer.SetPosition(1, raycastHit.point);
                Debug.Log("Hit grain model: " + grainModel.name);
                return;
            }
            lineRenderer.SetPosition(1, lineStart.forward * 100f);
            selectedModel = null;
        }
    }

    private void OnAxis2D(InputAction.CallbackContext context) {
        Vector2 value = context.ReadValue<Vector2>();
        switch(state) {
            case ModelMultitoolState.Inspecting:
                scrollableListUI.ScrollValue(value.y);
                break;
            case ModelMultitoolState.Positioning:
                if (selectedModel != null) {
                    modelDist += value.y * 0.1f;
                    modelRotationTarget *= Quaternion.Euler(Vector3.up * value.x * 10f);
                    // modelScale += value.y * 0.1f;
                    // selectedModel.transform.Rotate(Vector3.up, value.x * 10f);
                    // selectedModel.transform.position += transform.forward * value.y * 0.1f;
                }
                break;
        }
    }

    private void OnSubmit(InputAction.CallbackContext context) {
        switch(state) {
            case ModelMultitoolState.Inspecting:
                scrollableListUI.OnSubmit();
                break;
            case ModelMultitoolState.Positioning:
                if (selectedModel != null) {
                    selectedModel.ChangeState(GrainModelState.Placed);
                    state = ModelMultitoolState.Inspecting;
                } else {
                    state = ModelMultitoolState.Unassigned;
                }
                break;
            case ModelMultitoolState.Unassigned:
                if (selectedModel != null) {
                    state = ModelMultitoolState.Inspecting;   
                }
                break;
        }
    }
}
