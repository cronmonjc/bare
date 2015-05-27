using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System;

public class FileBrowser : MonoBehaviour {
    public enum State {
        OPEN, SAVE
    }

    [NonSerialized]
    public State BrowserState;
    [NonSerialized]
    public string[] drives;
    [NonSerialized]
    public string currFile = "";
    [NonSerialized]
    public string currDir = "";

    public bool IsOpen {
        get { return gameObject.activeInHierarchy; }
    }

    [Space(20f)]
    [Header("Callbacks")]
    public FileEvent OnSave;
    public FileEvent OnOpen;

    void Awake() {
        drives = Directory.GetLogicalDrives();

        if(Application.isWebPlayer) {
            Navigate(Application.persistentDataPath);
        } else {
            Navigate(Directory.GetCurrentDirectory());
        }
    }

    public void BeginSaveAs() {
        if(!gameObject.activeInHierarchy) {
            BrowserState = State.SAVE;
            gameObject.SetActive(true);
            transform.FindChild("FileField").FindChild("Act").FindChild("Text").GetComponent<UnityEngine.UI.Text>().text = "Save";
        }
    }

    public void BeginSave() {
        if(!gameObject.activeInHierarchy) {
            if(currFile.Length > 0) {
                OnSave.Invoke(currFile);
            } else {
                BeginSaveAs();
            }
        }
    }

    public void BeginOpen() {
        if(!gameObject.activeInHierarchy) {
            BrowserState = State.OPEN;
            gameObject.SetActive(true);
            transform.FindChild("FileField").FindChild("Act").FindChild("Text").GetComponent<UnityEngine.UI.Text>().text = "Open";
        }
    }

    public void Navigate(string str) {
        if(Directory.Exists(str)) {
            currDir = str;
        }
        FindObjectOfType<DirectoryTree>().Refresh();
        FindObjectOfType<FileListing>().Refresh();
    }

    public void NewFolder() {
        string cleanCurrDir = string.Join("/", currDir.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries));
        if(!Directory.Exists(cleanCurrDir + "/New Folder")) {
            Directory.CreateDirectory(cleanCurrDir + "/New Folder");
        }
        FindObjectOfType<DirectoryTree>().Refresh();
        FindObjectOfType<FileListing>().Refresh();
    }

    public void RenameFile() {
        if(FileItem.SelectedFile != null) {
            FileItem.SelectedFile.StartRename();
        }
    }

    public void DeleteFile() {
        if(FileItem.SelectedFile != null) {
            Transform deleteconfirm = transform.FindChild("DeleteConfirm");
            deleteconfirm.gameObject.SetActive(true);
            deleteconfirm.FindChild("head").FindChild("pathlabel").GetComponent<UnityEngine.UI.Text>().text = FileItem.SelectedFile.myPath;
        }
    }

    public void SeriouslyDeleteFile() {
        if(FileItem.SelectedFile != null) {
            if(FileItem.SelectedFile.IsDir) {
                Directory.Delete(FileItem.SelectedFile.myPath, true);
            } else {
                File.Delete(FileItem.SelectedFile.myPath);
            }
            GameObject.DestroyImmediate(FileItem.SelectedFile.gameObject);
            FileItem.SelectedFile = null;
        }
    }

    public void ActOnFile(string name, bool isFullPath = false) {
        if(!isFullPath) {
            name = string.Join("/", currDir.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries)) + "/" + name;
        }

        currFile = name;

        if(BrowserState == State.OPEN) {
            OnOpen.Invoke(name);
        } else { // State.SAVE
            OnSave.Invoke(name);
        }

        gameObject.SetActive(false);
    }

    public void ActOnFile(FileItem fileItem) {
        if(fileItem == null) ActOnFile(GetComponentInChildren<FileField>().GetComponent<UnityEngine.UI.InputField>().textComponent.text, false);
        else ActOnFile(fileItem.myPath, true);
    }

    public void ActOnFile() {
        ActOnFile(FileItem.SelectedFile);
    }
}

[System.Serializable]
public class FileEvent : UnityEvent<string> { }
