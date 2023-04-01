using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


[Serializable]
public class AddressableResourceInfo {
    public string key;
    public string file;
    public string title;
    public int bytes;
}



public class FileListUI : MonoBehaviour
{
    public GameObject fileItemPrefab;
    public Transform screenMask;
    
    public FileInfo CurrentlySelectedFile { get { 
        if(fileListItems.Count > 0) return fileListItems[fileIndex].file;
        else return null;
    } }

    public bool isBrowsingBuiltInFiles = true;

    public string FilePath { get; private set; }
    private int fileIndex = 0;
    private List<FileListItem> fileListItems = new List<FileListItem>();

    // make a SamplePackBrowser class that inherits from a generic base called ItemList or something
    public static TsvrSamplePackMetadata[] ListSamplePacks() {
        TextAsset textAsset = Resources.Load<TextAsset>("SamplePacks/sample-packs");
        return JsonHelper.FromJsonArray<TsvrSamplePackMetadata>(JsonHelper.FixJson(textAsset.ToString()));
    }

    public static AddressableResourceInfo[] ListBuiltinFiles() {
        TextAsset textAsset = Resources.Load<TextAsset>("Data/builtin-sounds");
        return JsonHelper.FromJsonArray<AddressableResourceInfo>(JsonHelper.FixJson(textAsset.ToString()));
    }

    public void SetToBuiltinFiles() {
        isBrowsingBuiltInFiles = true;
        AddressableResourceInfo[] files = ListBuiltinFiles();
        UpdateFileList(files);
    }

    public void SetDirectory(string path, string extension = null) {
        isBrowsingBuiltInFiles = false;
        Debug.Log("SetDirectory: " + path);
        FileInfo[] files = GetDirectoryFiles(path, extension);
        if (files.Length == 0) {
            Debug.LogError("No files found in " + path);
            return;
        }
        // foreach(FileInfo file in files) Debug.Log(file.Name);
        FilePath = path;
        UpdateFileList(files);
    }

    private FileInfo[] GetDirectoryFiles(string path, string extension = null) {
        DirectoryInfo dir = new DirectoryInfo(path);
        string ext = extension == null ? "*.*" : "*." + extension;
        FileInfo[] info = dir.GetFiles(ext);
        return info;
    }

    private void UpdateFileList(AddressableResourceInfo[] files) {
        if (files.Length == 0) {
            Debug.LogError("Length of files is 0!");
            return;
        };
        foreach (FileListItem fileListItem in fileListItems)
            Destroy(fileListItem.gameObject);
           
        fileListItems.Clear();
        foreach (AddressableResourceInfo file in files) {
            GameObject item = Instantiate(fileItemPrefab, screenMask);
            FileListItem fileItem = item.GetComponent<FileListItem>();
            fileItem.SetFile(file);
            fileListItems.Add(fileItem);
        }
        fileIndex = 0;
        fileListItems[fileIndex].Select();
    }

    private void UpdateFileList(FileInfo[] files) {
        Debug.Log("FILE LIST ITEMS" + files.Length);
        if (files.Length == 0) {
            Debug.LogError("Length of files is 0!");
            return;
        };
        foreach (FileListItem fileListItem in fileListItems)
            Destroy(fileListItem.gameObject);
            
        fileListItems.Clear();
        foreach (FileInfo file in files) {
            GameObject item = Instantiate(fileItemPrefab, screenMask);
            FileListItem fileItem = item.GetComponent<FileListItem>();
            fileItem.SetFile(file);
            fileListItems.Add(fileItem);
        }
        fileIndex = 0;
        fileListItems[fileIndex].Select();
    }

    private void UpdateItemHeights() {
        for (int i = 0; i < fileListItems.Count; i++) {
            if (i < fileIndex - 1)
                fileListItems[i].SetHeight(0);
            else if (i == fileIndex)
                fileListItems[i].SetHeight(58);
            else if (i == fileIndex + 1)
                fileListItems[i].SetHeight(58);
        }
    }

    public FileInfo NextFile() {
        if (fileListItems.Count == 0) return null;
        foreach(FileListItem item in fileListItems) item.Deselect();
        fileIndex = (fileIndex + 1) % fileListItems.Count;
        fileListItems[fileIndex].Select();
        UpdateItemHeights();
        return fileListItems[fileIndex].file;
    }

    public FileInfo PreviousFile() {
        if (fileListItems.Count == 0) return null;
        foreach(FileListItem item in fileListItems) item.Deselect();
        fileIndex = (fileIndex - 1 + fileListItems.Count) % fileListItems.Count;
        fileListItems[fileIndex].Select();
        UpdateItemHeights();
        return fileListItems[fileIndex].file;
    }
}



// public FileInfo CurrentlySelectedFile { get {
//     foreach(FileListItem item in fileListItems) {
//         if (item.Selected) {
//             return item.file;
//         }
//     }
//     return null;
// } }