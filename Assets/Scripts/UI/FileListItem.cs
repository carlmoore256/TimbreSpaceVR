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

    void Start() {
        // background = GetComponent<Image>();
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
        GetComponent<Image>().color = Color.white;
        Selected = false;
    }
}