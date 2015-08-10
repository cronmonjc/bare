using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI Component, File Browser.  Component that handles an individual File.
/// </summary>
public class FileItem : Selectable, IPointerClickHandler {
    /// <summary>
    /// Is this item a Directory?
    /// </summary>
    public bool IsDir = false;
    /// <summary>
    /// Is this item being renamed?
    /// </summary>
    public bool IsRenaming = false;
    /// <summary>
    /// The selected file item instance
    /// </summary>
    public static FileItem SelectedFile;
    /// <summary>
    /// Is the user currently clicking?
    /// </summary>
    public static bool Clicking = false;

    /// <summary>
    /// The file's path
    /// </summary>
    public string myPath;

    /// <summary>
    /// The icon on the item.  Set via Unity Inspector.
    /// </summary>
    public Image icon;
    /// <summary>
    /// The label on the item.  Set via Unity Inspector.
    /// </summary>
    public InputField label;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    protected override void Start() {
        base.Start();

        label.onEndEdit.AddListener(delegate(string to) {
            EndRename(to);
        });

        label.enabled = false;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(myPath.Length > 0 && label.text.Length == 0) {
            string[] pathParts = myPath.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
            label.textComponent.text = label.text = pathParts[pathParts.Length - 1];
        }
        label.textComponent.fontSize = Mathf.RoundToInt(6f + (((RectTransform)transform).sizeDelta.x / 48f) * 4f);
    }

    /// <summary>
    /// Refreshes the label.
    /// </summary>
    public void RefreshLabel() {
        label.textComponent.text = label.text;
    }

    /// <summary>
    /// Called by Unity when the user clicks on the GameObject containing this Component
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerClick(PointerEventData eventData) {
        if(eventData.clickCount > 1) { // Clicked more than once (ie, twice)
            FileBrowser fb = FindObjectOfType<FileBrowser>();
            if(IsDir) {
                fb.Navigate(myPath); // Navigate if it's a path
            } else {
                fb.ActOnFile(this); // Act if it's a file
            }
        } else {
            ColorBlock cb;

            if(SelectedFile != null) {
                cb = SelectedFile.colors;
                cb.normalColor = new Color(1f, 1f, 1f, 0f);
                cb.highlightedColor = new Color(1f, 1f, 1f, 0.5f);
                SelectedFile.colors = cb; // De-color the currently selected file
            }

            SelectedFile = this; // Make this the currently selected file
            cb = colors;
            cb.normalColor = new Color(1f, 0.75f, 0f, 0.5f);
            cb.highlightedColor = new Color(1f, 0.75f, 0f, 0.5f);
            colors = cb; // Color it up

            Clicking = true;
            if(!IsDir)
                FindObjectOfType<FileField>().SetText(this); // Set the text
            Clicking = false;
        }
    }

    /// <summary>
    /// Starts the renaming process.
    /// </summary>
    public void StartRename() {
        label.text = label.textComponent.text;

        label.interactable = true;
        label.enabled = true;
        label.ActivateInputField();

        IsRenaming = true;
    }

    /// <summary>
    /// Ends the renaming process.
    /// </summary>
    /// <param name="to">What the file was renamed to.</param>
    public void EndRename(string to) {
        label.interactable = false;
        label.enabled = false;
        if(!label.wasCanceled) {
            string newpath = string.Join("\\", FindObjectOfType<FileBrowser>().currDir.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries)) + "\\" + to;
            if(IsDir) {
                Directory.Move(myPath, newpath);
            } else {
                File.Move(myPath, newpath);
            }
            myPath = newpath;
        }

        IsRenaming = false;
    }
}
