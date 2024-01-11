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

    private InteractableObject _selectedObject;
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
        if (_selectedObject != null && !_selectedObject.PlacementState.Equals(PlacementState.Placed)) {
            Debug.Log($"Selected model not placed, destroying {_selectedObject}");
            Destroy(_selectedObject.gameObject);
        }
    }

    void Update()
    {
        var lineEndPosition = lineStart.position + lineStart.forward * 100f;
        if (_selectedObject != null) {
            lineEndPosition = _selectedObject.transform.position + _selectedObject.transform.forward * _selectedObject.transform.localScale.z;
        }
        
        switch(state) {
            case ModelMultitoolState.Inspecting:
                if (_selectedObject != null) {
                    UpdateLine(_selectedObject.transform.position, lineInspectColor);
                    _selectedObject.Inspect();
                }
                break;
            case ModelMultitoolState.Positioning:
                // if (lineRenderer.gameObject.activeSelf) {
                //     lineRenderer.gameObject.SetActive(false);
                // }
                UpdateLine(lineEndPosition, lineInspectColor);
                if (_selectedObject != null) {
                    _selectedObject.MoveTo(
                        new TransformSnapshot(
                            ToolController.transform.position + ToolController.transform.forward * modelDist,
                            modelRotationTarget,
                            new Vector3(modelScale, modelScale, modelScale)
                        ),
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
                if (_selectedObject != null) {
                    // _selectedObject.Reposition(
                    //     _selectedObject.transform.position,
                    //     _selectedObject.transform.rotation,
                    //     new Vector3(modelScale, modelScale, modelScale),
                    //     0.5f
                    // );
                }
                break;
            case ModelMultitoolState.Unassigned:
                RaycastTarget();
                break;
        }
    }

    private void DisplayMainMenu() {
        scrollableListUI.ClearItems();
        if (_selectedObject == null) return;
        // scrollableListUI.SetHeader("Grain Model Inspector", $"{selectedModel.name} | {selectedModel.}");
        scrollableListUI.SetHeader("Inspector", $"{_selectedObject.name}");
        
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
            if (_selectedObject != null) {
                Destroy(_selectedObject.gameObject);
                _selectedObject = null;
            }
        });

        scrollableListUI.AddItem(this, (item, content) => {
            content.header.text = "Cancel";
            content.subheader.text = "Cancel the current action";
        }, (item) => {
            _selectedObject = null;
            state = ModelMultitoolState.Unassigned;
        });
    }

    public void SetSelectedModel(InteractableObject model) {
        _selectedObject = model;
        state = ModelMultitoolState.Positioning;
        modelDist = Vector3.Distance(ToolController.transform.position, model.transform.position);
        modelScale = model.transform.localScale.x;
        modelRotationTarget = model.transform.rotation;
    }

    private void InspectCurrentModel() {
        if (_selectedObject == null) return;
        _selectedObject.Inspect();
        lineRenderer.SetPosition(0, lineStart.position);
        lineRenderer.SetPosition(1, _selectedObject.transform.position);
    }

    private void RaycastTarget() {
        if (Physics.Raycast(lineStart.position, lineStart.forward, out raycastHit, Mathf.Infinity, grainLayerMask))
        {
            GameObject hitObject = raycastHit.collider.gameObject;
            InteractableObject grainModel = hitObject.transform.parent.GetComponent<InteractableObject>();
            if (grainModel != null)
            {
                _selectedObject = grainModel;
                _selectedObject.Inspect();
                UpdateLine(raycastHit.point, lineUnassignedHoverColor);
                return;
            }
        } else {
            UpdateLine(lineStart.position + lineStart.forward * 10f, lineUnassignedColor);
            _selectedObject = null;
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
                if (_selectedObject != null) {
                    modelDist += value.y * 0.1f;
                    modelRotationTarget *= Quaternion.Euler(Vector3.up * value.x * 2f);
                    // modelScale += value.y * 0.1f;
                }
                break;
            case ModelMultitoolState.Resizing:
                if (_selectedObject != null) {
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
                // _selectedObject.Place();
                DisplayMainMenu();
                // if (selectedModel != null) {
                //     selectedModel.Place();
                // } else {
                //     state = ModelMultitoolState.Unassigned;
                // }
                break;
            case ModelMultitoolState.Resizing:
                state = ModelMultitoolState.Inspecting;
                // _selectedObject.Place();
                DisplayMainMenu();
                break;
            case ModelMultitoolState.Unassigned:
                if (_selectedObject != null) {
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
