using UnityEngine;
using System.Collections;

public class BarSizeDisplay : MonoBehaviour {
    public string Prefix, Suffix;
    public UnityEngine.UI.Text priceText;

    void Start() {
        SetSize();
    }

    public void SetSize() {
        GetComponent<UnityEngine.UI.Text>().text = Prefix + BarManager.inst.BarModel + Suffix;
        if(priceText != null) priceText.text = "$" + (BarManager.inst.BarPrice * 0.01f).ToString("F2");
    }

    void Update() {
        if(priceText != null && (priceText.enabled ^ CameraControl.ShowPricing)) {
            priceText.enabled = CameraControl.ShowPricing;
        }
    }
}
