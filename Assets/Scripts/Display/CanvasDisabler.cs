using UnityEngine;
using System.Collections;

/// <summary>
/// Small static class that will disable the Canvas if needed to allow PDF generation to ignore the main UI Canvas.
/// </summary>
public class CanvasDisabler : MonoBehaviour {
    /// <summary>
    /// The Canvas that's being toggled on and off.  
    /// </summary>
    private static Canvas c;

    /// <summary>
    /// Should the main UI be visible?
    /// </summary>
    public static bool CanvasEnabled {
        get { return c.enabled; }
        set { c.enabled = value; }
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        c = GetComponent<Canvas>();
    }
}
