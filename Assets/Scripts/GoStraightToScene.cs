using UnityEngine;
using System.Collections;

/// <summary>
/// Small Component that forces the Application to move to another Scene immediately.  Used for the Splash Screen.
/// </summary>
public class GoStraightToScene : MonoBehaviour {
    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        Application.LoadLevel(1);
    }
}
