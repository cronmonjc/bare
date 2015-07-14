using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;

public class FileBrowser : MonoBehaviour {
    public InputField fileField;

    public enum State {
        OPEN, SAVE
    }

    [NonSerialized]
    public State BrowserState;
    [NonSerialized]
    public string[] drives;
    [NonSerialized]
    public string savingFile = "";
    [NonSerialized]
    public string currFile = "";
    [NonSerialized]
    public string currDir = "";

    public string fileFieldText {
        get { return fileField.text; }
        set { fileField.text = value; }
    }

    private FileListing fl;

    public bool IsOpen {
        get { return gameObject.activeInHierarchy; }
    }

    [Space(20f)]
    [Header("Callbacks")]
    public FileEvent OnSave;
    public FileEvent OnOpen;

    void Awake() {
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
            transform.Find("MainArea/FileArea/FileField/Act/Text").GetComponent<Text>().text = "Save";

            if(fl == null) fl = transform.Find("MainArea/FileArea/FileListing").GetComponent<FileListing>();
            fl.Refresh();

            transform.Find("MainArea/DirHide/Button").GetComponent<DirHide>().DirShown = false;
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
            transform.Find("MainArea/FileArea/FileField/Act/Text").GetComponent<Text>().text = "Open";

            if(fl == null) fl = transform.Find("MainArea/FileArea/FileListing/FileListing").GetComponent<FileListing>();
            fl.Refresh();

            transform.Find("MainArea/DirHide/Button").GetComponent<DirHide>().DirShown = false;
        }
    }

    public void Navigate(string str) {
        str = string.Join("\\", str.Split(new char[] {'/', '\\'}, System.StringSplitOptions.RemoveEmptyEntries )) + "\\";
        if(Directory.Exists(str)) {
            currDir = str;
        }
        transform.Find("MainArea/DirectoryTree").GetComponent<DirectoryTree>().Refresh();
        if(fl == null) fl = transform.Find("MainArea/FileArea/FileListing/FileListing").GetComponent<FileListing>();
        fl.Refresh();
    }

    public void NewFolder() {
        string cleanCurrDir = string.Join("\\", currDir.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries));
        if(!Directory.Exists(cleanCurrDir + "/New Folder")) {
            Directory.CreateDirectory(cleanCurrDir + "/New Folder");
        }
        transform.Find("MainArea/DirectoryTree").GetComponent<DirectoryTree>().Refresh();
        if(fl == null) fl = transform.Find("MainArea/FileArea/FileListing/FileListing").GetComponent<FileListing>();
        fl.Refresh();
    }

    public void RenameFile() {
        if(FileItem.SelectedFile != null) {
            FileItem.SelectedFile.StartRename();
        }
    }

    public void DeleteFile() {
        if(FileItem.SelectedFile != null) {
            Transform deleteconfirm = transform.Find("DeleteConfirm");
            deleteconfirm.gameObject.SetActive(true);
            deleteconfirm.Find("head/pathlabel").GetComponent<Text>().text = FileItem.SelectedFile.myPath;
        }
    }

    public void SeriouslyDeleteFile() {
        if(FileItem.SelectedFile != null) {
            if(FileItem.SelectedFile.IsDir) {
                Directory.Delete(FileItem.SelectedFile.myPath, true);
            } else {
                File.Delete(FileItem.SelectedFile.myPath);
            }
            GameObject.Destroy(FileItem.SelectedFile.gameObject);
            FileItem.SelectedFile = null;
        }
    }

    public void DeselectFile() {
        if(!FileItem.Clicking && FileItem.SelectedFile != null) {
            ColorBlock cb = FileItem.SelectedFile.colors;
            cb.normalColor = cb.highlightedColor = new Color(1f, 1f, 1f, 0f);
            FileItem.SelectedFile.colors = cb;
            FileItem.SelectedFile = null;
        }
    }

    public void InvokeSave(bool force) {
        if(!force && File.Exists(currFile)) {
            Transform overconfirm = transform.Find("OverwriteConfirm");
            overconfirm.gameObject.SetActive(true);
            overconfirm.Find("head/pathlabel").GetComponent<Text>().text = savingFile;
        } else {
            currFile = savingFile;
            OnSave.Invoke(currFile);
            gameObject.SetActive(false);
        }
    }

    public void CancelSave() {
        savingFile = "";
    }

    public void ActOnFile(string name, bool isFullPath = false) {
        if(!isFullPath) {
            name = string.Join("\\", currDir.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries)) + "/" + name;
        }

        if(BrowserState == State.OPEN) {
            currFile = name;
            OnOpen.Invoke(name);
            gameObject.SetActive(false);
        } else { // State.SAVE
            savingFile = name;
            InvokeSave(false);
        }
    }

    public void ActOnFile(FileItem fileItem) {
        if(fileItem == null) ActOnFile(GetComponentInChildren<FileField>().GetComponent<InputField>().textComponent.text, false);
        else ActOnFile(fileItem.myPath, true);
    }

    public void ActOnFile() {
        ActOnFile(FileItem.SelectedFile);
    }
}

[System.Serializable]
public class FileEvent : UnityEvent<string> { }
