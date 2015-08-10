using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Checks to make sure that the length of the notes doesn't get too long.
/// </summary>
public class WordyWarn : MonoBehaviour {
    /// <summary>
    /// The icon indicating the notes are too long.  Set via Unity Inspector.
    /// </summary>
    public Image icon;
    /// <summary>
    /// The text indicating the notes are too long.  Set via Unity Inspector.
    /// </summary>
    public Text text;
    /// <summary>
    /// The notes field.  Set via Unity Inspector.
    /// </summary>
    public InputField notes;

    /// <summary>
    /// Tests if it's gotten too long.  Called every time input is recieved by the Notes field.
    /// </summary>
    public void Test() {
        int breaks = notes.text.Split('\n').Length;
        int len = notes.text.Length;
        if(breaks > 1) {
            len += (breaks - 1) * 150;
        }
        if(len > 1500) {
            icon.enabled = text.enabled = true;
        } else {
            icon.enabled = text.enabled = false;
        }
    }
}
