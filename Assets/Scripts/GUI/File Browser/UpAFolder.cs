using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component, File Browser.  Small Component that has a single function - when clicked, have the File Browser navigate up a folder when possible.
/// </summary>
public class UpAFolder : MonoBehaviour {
    /// <summary>
    /// Called when the user clicks on the Button this Component is on
    /// </summary>
    public void Clicked() {
        FileBrowser fb = FindObjectOfType<FileBrowser>();

        string[] parts = fb.currDir.Split(new char[] {'/', '\\'}, System.StringSplitOptions.RemoveEmptyEntries);

        if(parts.Length > 1)
            fb.Navigate(string.Join("\\", parts, 0, parts.Length - 1));
    }
}
