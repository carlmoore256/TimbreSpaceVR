using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class FileListItem : MonoBehaviour 
{
    public TextMeshProUGUI filepath;
    public TextMeshProUGUI size;
    public bool Selected { get; private set; }
    public FileInfo file;
    // private Image background;
    private Color originalColor;

    void Start() {
        // background = GetComponent<Image>();
        originalColor = GetComponent<Image>().color;
    }

    public void SetFile(AddressableResourceInfo file) {
        this.file = new FileInfo(file.key);
        filepath.text = file.title;
        size.text = (file.bytes * 0.000001f).ToString("F2") + " MB";
    }

    public void SetFile(FileInfo file) {
        this.file = file;
        filepath.text = file.Name;
        size.text = (file.Length * 0.000001f).ToString("F2") + " MB";
    }

    public void Select() {
        GetComponent<Image>().color = Color.green;
        Selected = true;
    }

    public void Deselect() {
        GetComponent<Image>().color = originalColor;
        Selected = false;
    }

    public void SetHeight(float height) {
        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
    }
}