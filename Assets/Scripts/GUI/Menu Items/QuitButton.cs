using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Quits the application when clicked.
/// </summary>
public class QuitButton : MonoBehaviour {
    /// <summary>
    /// Called when the user clicks the Button Component on this GameObject.
    /// </summary>
    public void Quit() {
        Application.Quit();
    }
}
