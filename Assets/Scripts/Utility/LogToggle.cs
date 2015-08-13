using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Toggles whether or not to log input.
/// </summary>
public class LogToggle : MonoBehaviour {
    /// <summary>
    /// The reference to the Text UI Component.  Set via Unity Inspector.
    /// </summary>
    public UnityEngine.UI.Text t;

    /// <summary>
    /// Called when the user clicks the Button Component on this GameObject.
    /// </summary>
    public void Clicked() {
        ErrorLogging.logInput = !ErrorLogging.logInput;

        t.text = (ErrorLogging.logInput ? "" : "Not") + " Logging Input";
    }
}
