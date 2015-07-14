using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FileItem : Selectable, IPointerClickHandler {
    public bool IsDir = false;
    public bool IsRenaming = false;
    public static FileItem SelectedFile;
    public static bool Clicking = false;

    public string myPath;

    public Image icon;
    public InputField label;

    protected override void Start() {
        base.Start();

        label.onEndEdit.AddListener(delegate(string to) {
            EndRename(to);
        });

        label.enabled = false;
    }

    void Update() {
        if(myPath.Length > 0 && label.text.Length == 0) {
            string[] pathParts = myPath.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
            label.textComponent.text = label.text = pathParts[pathParts.Length - 1];
        }
        label.textComponent.fontSize = Mathf.RoundToInt(6f + (((RectTransform)transform).sizeDelta.x / 48f) * 4f);
    }

    public void RefreshLabel() {
        label.textComponent.text = label.text;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(eventData.clickCount > 1) {
            FileBrowser fb = FindObjectOfType<FileBrowser>();
            if(IsDir) {
                fb.Navigate(myPath);
            } else {
                fb.ActOnFile(this);
            }
        } else {
            ColorBlock cb;

            if(SelectedFile != null) {
                cb = SelectedFile.colors;
                cb.normalColor = new Color(1f, 1f, 1f, 0f);
                cb.highlightedColor = new Color(1f, 1f, 1f, 0.5f);
                SelectedFile.colors = cb;
            }

            SelectedFile = this;
            cb = colors;
            cb.normalColor = new Color(1f, 0.75f, 0f, 0.5f);
            cb.highlightedColor = new Color(1f, 0.75f, 0f, 0.5f);
            colors = cb;

            Clicking = true;
            if(!IsDir)
                FindObjectOfType<FileField>().SetText(this);
            Clicking = false;
        }
    }

    public void StartRename() {
        label.text = label.textComponent.text;

        label.interactable = true;
        label.enabled = true;
        label.ActivateInputField();

        IsRenaming = true;
    }

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
