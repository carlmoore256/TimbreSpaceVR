using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ModelMultitoolOptions {
    Reposition,
    Resize,
    Parameters,
    Info,
    Save,
    Delete,
    Cancel
}

public class ModelInspector : TsvrTool
{
    public override TsvrToolType ToolType { get => TsvrToolType.ModelInspector; }
    public ScrollableListUI scrollableListUI;
    public Transform lineStart;
    private LineRenderer lineRenderer;
    public LayerMask grainLayerMask;

    public Color lineInspectColor;
    public Color lineUnassignedColor;
    public Color lineUnassignedHoverColor;
    public Color linePositioningColor;

    private GrainModelOld selectedModel;
    private RaycastHit raycastHit;

    private float modelScale;
    private float modelDist;
    private Quaternion modelRotationTarget;


    enum ModelMultitoolState {
        Inspecting,
        Positioning,
        Resizing,
        Unassigned
    }

    private ModelMultitoolState state = ModelMultitoolState.Unassigned;

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
        if (selectedModel != null && !selectedModel.HasBeenPlaced) {
            Debug.Log($"Selected model not placed, destroying {selectedModel}");
            Destroy(selectedModel.gameObject);
        }
    }

    void Update()
    {
        var lineEndPosition = lineStart.position + lineStart.forward * 100f;
        if (selectedModel != null) {
            lineEndPosition = selectedModel.transform.position + selectedModel.transform.forward * selectedModel.transform.localScale.z;
        }
        
        switch(state) {
            case ModelMultitoolState.Inspecting:
                if (selectedModel != null) {
                    UpdateLine(selectedModel.transform.position, lineInspectColor);
                    selectedModel.Inspect();
                }
                break;
            case ModelMultitoolState.Positioning:
                // if (lineRenderer.gameObject.activeSelf) {
                //     lineRenderer.gameObject.SetActive(false);
                // }
                UpdateLine(lineEndPosition, lineInspectColor);
                if (selectedModel != null) {
                    selectedModel.Reposition(
                        ToolController.transform.position + ToolController.transform.forward * modelDist,
                        modelRotationTarget,
                        new Vector3(modelScale, modelScale, modelScale),
                        0.5f
                    );
                }
                break;
            case ModelMultitoolState.Resizing:
                // if (lineRenderer.gameObject.activeSelf) {
                //     lineRenderer.gameObject.SetActive(false);
                // }
                // make line end position stop at the size of the grainModel
                UpdateLine(lineEndPosition, lineInspectColor);
                if (selectedModel != null) {
                    selectedModel.Reposition(
                        selectedModel.transform.position,
                        selectedModel.transform.rotation,
                        new Vector3(modelScale, modelScale, modelScale),
                        0.5f
                    );
                }
                break;
            case ModelMultitoolState.Unassigned:
                RaycastTarget();
                break;
        }
    }

    private void DisplayMainMenu() {
        scrollableListUI.ClearItems();
        if (selectedModel == null) return;
        // scrollableListUI.SetHeader("Grain Model Inspector", $"{selectedModel.name} | {selectedModel.}");
        scrollableListUI.SetHeader("Inspector", $"{selectedModel.name}");
        
        scrollableListUI.AddItem(this, (item, content) => {
            content.header.text = "Reposition";
            content.subheader.text = "Reposition the selected model";
        }, (item) => {
            state = ModelMultitoolState.Positioning;
        });

        scrollableListUI.AddItem(this, (item, content) => {
            content.header.text = "Resize";
            content.subheader.text = "Resize the selected model";
        }, (item) => {
            state = ModelMultitoolState.Resizing;
        });

        scrollableListUI.AddItem(this, (item, content) => {
            content.header.text = "Parameters";
            content.subheader.text = "Edit the selected model's parameters";
        }, (item) => {
            Debug.Log("Implement parameters");
        });

        scrollableListUI.AddItem(this, (item, content) => {
            content.header.text = "Info";
            content.subheader.text = "View the selected model's info";
        }, (item) => {
            Debug.Log("Implement info");
        });

        scrollableListUI.AddItem(this, (item, content) => {
            content.header.text = "Save";
            content.subheader.text = "Save the selected model";
        }, (item) => {
            Debug.Log("Implement save");
        });

        scrollableListUI.AddItem(this, (item, content) => {
            content.header.text = "Delete";
            content.subheader.text = "Delete the selected model";
        }, (item) => {
            Debug.Log("Deleting Current Model!");
            if (selectedModel != null) {
                Destroy(selectedModel.gameObject);
                selectedModel = null;
            }
        });

        scrollableListUI.AddItem(this, (item, content) => {
            content.header.text = "Cancel";
            content.subheader.text = "Cancel the current action";
        }, (item) => {
            selectedModel = null;
            state = ModelMultitoolState.Unassigned;
        });
    }

    public void SetSelectedModel(GrainModelOld model) {
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
        if (Physics.Raycast(lineStart.position, lineStart.forward, out raycastHit, Mathf.Infinity, grainLayerMask))
        {
            GameObject hitObject = raycastHit.collider.gameObject;
            GrainModelOld grainModel = hitObject.transform.parent.GetComponent<GrainModelOld>();
            if (grainModel != null)
            {
                selectedModel = grainModel;
                selectedModel.Inspect();
                UpdateLine(raycastHit.point, lineUnassignedHoverColor);
                return;
            }
        } else {
            UpdateLine(lineStart.position + lineStart.forward * 10f, lineUnassignedColor);
            selectedModel = null;
        }
    }

    private void UpdateLine(Vector3 target, Color color = default) {
        if (lineRenderer.gameObject.activeSelf) {
            lineRenderer.gameObject.SetActive(true);
        }
        lineRenderer.SetPosition(0, lineStart.position);
        lineRenderer.SetPosition(1, target);
        if (color != default) {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
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
                    modelRotationTarget *= Quaternion.Euler(Vector3.up * value.x * 2f);
                    // modelScale += value.y * 0.1f;
                }
                break;
            case ModelMultitoolState.Resizing:
                if (selectedModel != null) {
                    modelScale += value.y * 0.1f;
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
                state = ModelMultitoolState.Inspecting;
                selectedModel.Place();
                DisplayMainMenu();
                // if (selectedModel != null) {
                //     selectedModel.Place();
                // } else {
                //     state = ModelMultitoolState.Unassigned;
                // }
                break;
            case ModelMultitoolState.Resizing:
                state = ModelMultitoolState.Inspecting;
                selectedModel.Place();
                DisplayMainMenu();
                break;
            case ModelMultitoolState.Unassigned:
                if (selectedModel != null) {
                    state = ModelMultitoolState.Inspecting;   
                    DisplayMainMenu();
                }
                break;
        }
    }

    // private void ChangeState(ModelMultitoolState newState) {
    //     switch(newState) {
            
    //     }
    //     state = newState;
    // }
}
