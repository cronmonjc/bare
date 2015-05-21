using UnityEngine;
using System.Collections;

public class CanvasDisabler : MonoBehaviour {
    private static Canvas c;

    public static bool CanvasEnabled {
        get { return c.enabled; }
        set { c.enabled = value; }
    }

    void Start() {
        c = GetComponent<Canvas>();
    }
}
