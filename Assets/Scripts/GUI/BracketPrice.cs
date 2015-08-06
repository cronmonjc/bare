using UnityEngine;
using System.Collections;

public class BracketPrice : MonoBehaviour {
    private UnityEngine.UI.Text text;

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
