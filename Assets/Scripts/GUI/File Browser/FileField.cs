using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// UI Component, File Browser.  Component that sits on the file Input Field, carefully watching for when it should get the File Browser to act.
/// </summary>
public class FileField : MonoBehaviour {
    private bool focusedLastFrame = false;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) { // If the user pressed either return...
            if(focusedLastFrame) { // ...and the file field was focused a frame ago...
                FindObjectOfType<FileBrowser>().ActOnFile(GetComponent<InputField>().textComponent.text, false); // ...get the browser to act on the current text.
                // Could not use "On End Edit" on the Input Field because that also fires when it simply loses focus because user clicked elsewhere
            }
        }
        focusedLastFrame = GetComponent<InputField>().isFocused;
    }

    /// <summary>
    /// Sets the text to the name of a selected File Item.  Called when the user clicks on the item.
    /// </summary>
    /// <param name="fi">The selected File Item.</param>
    public void SetText(FileItem fi) {
        string[] pathParts = fi.myPath.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
        GetComponent<InputField>().text = pathParts[pathParts.Length - 1];
    }
}
