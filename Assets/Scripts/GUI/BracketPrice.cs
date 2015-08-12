using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Displays the static price for a mounting bracket when needed.
/// </summary>
public class BracketPrice : MonoBehaviour {
    /// <summary>
    /// The reference to the Text Component to display the price on
    /// </summary>
    private UnityEngine.UI.Text text;

    /// <summary>
    /// The price text.
    /// </summary>
    public uint Price {
        set {
            text.text = "$" + (value * 0.01f).ToString("F2");
        }
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        text = GetComponent<UnityEngine.UI.Text>();
        Price = LightDict.inst.bracketPrice;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(text.enabled ^ CameraControl.ShowPricing) {
            text.enabled = CameraControl.ShowPricing;
        }
    }
}
