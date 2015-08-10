using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI Component, File Browser.  The Component that manages the entire file browser.
/// </summary>
public class FileBrowser : MonoBehaviour {
    /// <summary>
    /// The file field.  Set via Unity Inspector.
    /// </summary>
    public InputField fileField;

    /// <summary>
    /// A value indicating whether the browser is opening or saving a file
    /// </summary>
    public enum State {
        /// <summary>
        /// Indicates the browser is opening
        /// </summary>
        OPEN,
        /// <summary>
        /// Indicates the browser is saving
        /// </summary>
        SAVE
    }

    /// <summary>
    /// The current state of the browser
    /// </summary>
    [NonSerialized]
    public State BrowserState;
    /// <summary>
    /// The listing of drives mounted
    /// </summary>
    [NonSerialized]
    public string[] drives;
    /// <summary>
    /// The file currently being saved
    /// </summary>
    [NonSerialized]
    public string savingFile = "";
    /// <summary>
    /// The file currently being worked on
    /// </summary>
    [NonSerialized]
    public string currFile = "";
    /// <summary>
    /// The current directory the browser's navigated to
    /// </summary>
    [NonSerialized]
    public string currDir = "";

    /// <summary>
    /// Gets or sets the file field text.
    /// </summary>
    public string fileFieldText {
        get { return fileField.text; }
        set { fileField.text = value; }
    }

    /// <summary>
    /// A reference to the file listing
    /// </summary>
    private FileListing fl;

    /// <summary>
    /// Value indicating whether this instance is open.
    /// </summary>
    public bool IsOpen {
        get { return gameObject.activeInHierarchy; }
    }

    /// <summary>
    /// The "On Save" callback.  Set via Unity Inspector.
    /// </summary>
    [Space(20f)]
    [Header("Callbacks")]
    public FileEvent OnSave;
    /// <summary>
    /// The "On Open" callback.  Set via Unity Inspector.
    /// </summary>
    public FileEvent OnOpen;

    /// <summary>
    /// Awake is called once, immediately as the object is created (typically at load time)
    /// </summary>
    void Awake() {
        if(Application.isWebPlayer) {
            Navigate(Application.persistentDataPath);
        } else {
            Navigate(Directory.GetCurrentDirectory());
        }
    }

    /// <summary>
    /// Begins the "Save As" operation
    /// </summary>
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

    /// <summary>
    /// Begins the "Save" operation.  Invokes "On Save" if the current file path is known, otherwise will open the browser to perform a "Save As".
    /// </summary>
    public void BeginSave() {
        if(!gameObject.activeInHierarchy) {
            if(currFile.Length > 0) {
                OnSave.Invoke(currFile);
            } else {
                BeginSaveAs();
            }
        }
    }

    /// <summary>
    /// Begins the "Open" operation
    /// </summary>
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

    /// <summary>
    /// Navigates to the specified path
    /// </summary>
    /// <param name="path">The path to nagivate to</param>
    public void Navigate(string path) {
        path = string.Join("\\", path.Split(new char[] {'/', '\\'}, System.StringSplitOptions.RemoveEmptyEntries )) + "\\"; // Clean up the path, just in case
        if(Directory.Exists(path)) { // If path exists
            currDir = path; // Go to it
        }
        transform.Find("MainArea/DirectoryTree").GetComponent<DirectoryTree>().Refresh(); // Refresh directory listing
        if(fl == null) fl = transform.Find("MainArea/FileArea/FileListing/FileListing").GetComponent<FileListing>(); // If we don't have a File Listing reference, get it
        fl.Refresh(); // Refresh file listing
    }

    /// <summary>
    /// Makes a new folder at the current directory
    /// </summary>
    public void NewFolder() {
        string cleanCurrDir = string.Join("\\", currDir.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries)); // Clean up the path, just in case
        if(!Directory.Exists(cleanCurrDir + "/New Folder")) { // If a "New Folder" folder doesn't exist
            Directory.CreateDirectory(cleanCurrDir + "/New Folder"); // Make it
        }
        transform.Find("MainArea/DirectoryTree").GetComponent<DirectoryTree>().Refresh(); // Refresh directory listing
        if(fl == null) fl = transform.Find("MainArea/FileArea/FileListing/FileListing").GetComponent<FileListing>(); // If we don't have a File Listing reference, get it
        fl.Refresh(); // Refresh file listing
    }

    /// <summary>
    /// Renames the selected file, if there is a selected file
    /// </summary>
    public void RenameFile() {
        if(FileItem.SelectedFile != null) {
            FileItem.SelectedFile.StartRename();
        }
    }

    /// <summary>
    /// Prepares deletion of the selected file, if there is a selected file
    /// </summary>
    public void DeleteFile() {
        if(FileItem.SelectedFile != null) {
            Transform deleteconfirm = transform.Find("DeleteConfirm");
            deleteconfirm.gameObject.SetActive(true);
            deleteconfirm.Find("head/pathlabel").GetComponent<Text>().text = FileItem.SelectedFile.myPath;
        }
    }

    /// <summary>
    /// Performs the actual deletion of the selected file
    /// </summary>
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

    /// <summary>
    /// Deselects the selected file, if there is a selected file
    /// </summary>
    public void DeselectFile() {
        if(!FileItem.Clicking && FileItem.SelectedFile != null) {
            ColorBlock cb = FileItem.SelectedFile.colors;
            cb.normalColor = cb.highlightedColor = new Color(1f, 1f, 1f, 0f);
            FileItem.SelectedFile.colors = cb;
            FileItem.SelectedFile = null;
        }
    }

    /// <summary>
    /// Invokes the Save callback, called when attempting to save
    /// </summary>
    /// <param name="force">Whether or not to overwrite an existing file, if one exists.  If false, will request confirmation from user.</param>
    public void InvokeSave(bool force) {
        if(!force && File.Exists(savingFile)) {
            Transform overconfirm = transform.Find("OverwriteConfirm");
            overconfirm.gameObject.SetActive(true);
            overconfirm.Find("head/pathlabel").GetComponent<Text>().text = savingFile;
        } else {
            currFile = savingFile;
            OnSave.Invoke(currFile);
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Cancels the Save
    /// </summary>
    public void CancelSave() {
        savingFile = "";
    }

    /// <summary>
    /// Acts on the provided filename.  The actual action taken will depend on the current browser state.
    /// </summary>
    /// <param name="name">The path to the file</param>
    /// <param name="isFullPath">If true, this is the full path to the file.  If false, this is not the full path, prepend the current directory first.</param>
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

    /// <summary>
    /// Acts on the provided file item.  The actual action taken will depend on the current browser state.
    /// </summary>
    /// <param name="fileItem">The file item to act on.  If null, will act on the file described in the File Field instead.</param>
    public void ActOnFile(FileItem fileItem) {
        if(fileItem == null) ActOnFile(GetComponentInChildren<FileField>().GetComponent<InputField>().textComponent.text, false);
        else ActOnFile(fileItem.myPath, true);
    }

    /// <summary>
    /// Acts on the selected file.  Also called by the Act button.
    /// </summary>
    public void ActOnFile() {
        ActOnFile(FileItem.SelectedFile);
    }
}

/// <summary>
/// Unity's equivalent of a delegate for the save/load callback.  Allows callbacks to be assigned via the Inspector.
/// </summary>
[System.Serializable]
public class FileEvent : UnityEvent<string> { }
