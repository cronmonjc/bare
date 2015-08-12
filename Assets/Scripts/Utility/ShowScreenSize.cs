using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Debugging.  Displays the current size of the screen.
/// </summary>
public class ShowScreenSize : MonoBehaviour {
    /// <summary>
    /// The text Component to display the screen size on.
    /// </summary>
    private Text t;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(t == null) t = GetComponent<Text>();

        t.text = "Debug: " + Screen.width + "w*" + Screen.height + "h";
    }
}
