using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class SelectableListItemContent {
    public Image icon; // if null, image is not spawned
    public TextMeshProUGUI header;
    public TextMeshProUGUI subheader;
    public Transform rightContent; // can be anything, including null
}

public class SelectableListItem : MonoBehaviour
{
    public object item;
    public SelectableListItemContent content;
    public Color selectedColor = Color.red;
    private Color originalColor;
    private Image image;
    public bool selected = false;
    public Action onSubmit;

    private void Start() {
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    public void OnSelect() {
        selected = true;
        image.color = selectedColor;
    }

    public void OnDeselect() {
        selected = false;
        image.color = originalColor;
    }

    public void OnSubmit() {
        onSubmit?.Invoke();
    }

    public void SetItem(object item, Action<object, SelectableListItemContent> uiMapper, Action<object> onSubmit) {
        this.item = item;
        this.onSubmit = () => onSubmit(item);
        uiMapper(item, content);
    }

    public void SetItem(string header, Action onSubmit) {
        this.onSubmit = onSubmit;
        content.header.text = header;
        // content.subheader.text = "";
    }

    // public void SetContent(SelectableListItemContent content) {
    //     this.content.icon = content.icon;
    //     this.content.header = content.header;
    //     this.content.subheader = content.subheader;
    //     this.content.rightContent = content.rightContent;
    // }
}