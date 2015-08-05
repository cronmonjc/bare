using UnityEngine;
using System.Collections;

public class CanvasDisabler : MonoBehaviour {
    private static Canvas c;

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
