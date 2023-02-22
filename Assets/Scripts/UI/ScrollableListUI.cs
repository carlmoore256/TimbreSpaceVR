using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using TMPro;

public class ScrollableListUI : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform canvasParent;
    public Transform listContainer;
    public Scrollbar scrollbar;
    public TextMeshProUGUI header;
    public TextMeshProUGUI subheader;
    private ControllerActions controllerActions;
    private List<SelectableListItem> items;
    private int _selectedIndex = 0;

    void OnEnable() {
        controllerActions = ObjectHelpers.RecurseParentsForComponent<ControllerActions>(gameObject);
        if (controllerActions == null) {
            Debug.LogError("ScrollableListUI: Could not find ControllerActions component in parent or parents.");
            return;
        }
        controllerActions.toolAxis2D.action.performed += OnScrollStart;
        controllerActions.uiSelect.action.performed += OnSubmit;
    }

    void OnDisable() {
        if (controllerActions != null) {
            controllerActions.toolAxis2D.action.performed -= OnScrollStart;
            controllerActions.uiSelect.action.performed -= OnSubmit;
        }

        items.Clear();
    }

    public void AddItem(object item, Action<object, ListItemContent> uiMapper, Action<object> onSubmit) {
        GameObject listItem = Instantiate(itemPrefab, listContainer);
        SelectableListItem selectableListItem = listItem.GetComponent<SelectableListItem>();
        selectableListItem.SetItem(item, uiMapper, onSubmit);
        AddItem(selectableListItem);
    }

    public void AddItem(string header, Action onSubmit) {
        GameObject listItem = Instantiate(itemPrefab, listContainer);
        SelectableListItem selectableListItem = listItem.GetComponent<SelectableListItem>();
        selectableListItem.SetItem("Back", onSubmit);
        AddItem(selectableListItem);
    }

    public void AddItem(SelectableListItem item) {
        if (items == null) items = new List<SelectableListItem>();
        items.Add(item);
        item.transform.SetParent(listContainer);
        item.transform.localScale = Vector3.one;
    }


    public void RemoveItem(SelectableListItem item) {
        if (items == null) return;
        items.Remove(item);
        Destroy(item.gameObject);
    }

    public void ClearItems() {
        if (items == null) return;
        foreach (SelectableListItem item in items) {
            Destroy(item.gameObject);
        }
        items.Clear();
    }

    public void SetHeader(string headerText, string subheaderText) {
        header.text = headerText;
        subheader.text = subheaderText;
    }

    private void OnScrollStart(InputAction.CallbackContext context) {
        if (items == null) return;
        Vector2 scroll = context.ReadValue<Vector2>();
        // float scrollScalar = 1/(float)items.Count;
        // scrollScalar *= 0.3f;
        // scroll.y = (scroll.y * (1+scrollScalar)) - scrollScalar;
        float scrollValue = scrollbar.value + scroll.y * Time.deltaTime;
        scrollbar.value = Mathf.Clamp01(scrollbar.value + scroll.y * Time.deltaTime);
        int index = items.Count - (int)(scrollValue * items.Count);
        ScrollToItem(index);
    }

    private void ScrollToItem(int index) {
        if (items == null) return;
        if (index < 0) index = 0;
        if (index >= items.Count) index = items.Count - 1;
        for (int i = 0; i < items.Count; i++) {
            if (i == index) items[i].OnSelect();
            else items[i].OnDeselect();
        }
        _selectedIndex = index;
        // float scrollbarValue = ((float)index / (float)items.Count);
        // scrollbar.value = scrollbarValue;
    }

    private void OnSubmit(InputAction.CallbackContext context) {
        if (items == null) return;
        if (items.Count == 0) return;
        items[_selectedIndex].OnSubmit();
    }

}