using UnityEngine;
using System.Collections;

public class BracketPrice : MonoBehaviour {
    private UnityEngine.UI.Text text;

    public uint Price {
        set {
            text.text = "$" + (value * 0.01f).ToString("F2");
        }
    }

    void Start() {
        text = GetComponent<UnityEngine.UI.Text>();
        Price = LightDict.inst.bracketPrice;
    }

    void Update() {
        if(text.enabled ^ CameraControl.ShowPricing) {
            text.enabled = CameraControl.ShowPricing;
        }
    }
}
