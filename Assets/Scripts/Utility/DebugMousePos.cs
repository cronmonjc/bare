using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Debugging.  Displays the current mouse position on a Text Component.
/// </summary>
public class DebugMousePos : MonoBehaviour {
    /// <summary>
    /// The Text Component reference.
    /// </summary>
    private Text t;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(t == null) t = GetComponent<Text>();

        t.text = Input.mousePosition.ToString("F3");
    }
}
