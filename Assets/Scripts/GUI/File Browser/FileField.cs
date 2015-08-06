using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class FileField : MonoBehaviour {
    private bool focusedLastFrame = false;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            if(focusedLastFrame) {
                FindObjectOfType<FileBrowser>().ActOnFile(GetComponent<InputField>().textComponent.text, false);
            }
        }
        focusedLastFrame = GetComponent<InputField>().isFocused;
    }

    public void SetText(FileItem fi) {
        string[] pathParts = fi.myPath.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
        GetComponent<InputField>().text = pathParts[pathParts.Length - 1];
    }
}
