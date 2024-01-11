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
        items = new List<SelectableListItem>();
    }

    void OnDisable() {
        items.Clear();
    }

    public void AddItem(object item, Action<object, SelectableListItemContent> uiMapper, Action<object> onSubmit) {
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
        // if (items == null) items = new List<SelectableListItem>();
        items.Add(item);
        item.transform.SetParent(listContainer);
        item.transform.localScale = Vector3.one;
    }

    public void AddItem(SelectableListItemContent content) {
        // if (items == null) items = new List<SelectableListItem>();
        GameObject listItem = Instantiate(itemPrefab, listContainer);
        SelectableListItem selectableListItem = listItem.GetComponent<SelectableListItem>();
        selectableListItem.content = content;
        items.Add(selectableListItem);
        selectableListItem.transform.SetParent(listContainer);
        // selectableListItem.transform.localScale = Vector3.one;
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

    public void SetHeader(string headerText, string subheaderText = "") {
        header.text = headerText;
        subheader.text = subheaderText;
    }

    /// <summary>
    /// Supply with a float value, neg to scroll up, pos to scroll down
    /// <summary>
    public void ScrollValue(float value) {
        if (items == null) return;
        // int index = items.Count - (int)(value * items.Count);
        float scrollValue = scrollbar.value + value * Time.deltaTime;
        scrollbar.value = Mathf.Clamp01(scrollbar.value + value * Time.deltaTime);
        int index = items.Count - (int)(scrollValue * items.Count);
        ScrollToItem(index);
    }

    public void OnSubmit() {
        if (items == null) return;
        if (items.Count == 0) return;
        items[_selectedIndex].OnSubmit();
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



}