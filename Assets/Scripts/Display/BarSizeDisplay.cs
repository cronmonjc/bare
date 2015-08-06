using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Shows the bar size with optional prefix / suffix text.
/// </summary>
public class BarSizeDisplay : MonoBehaviour {
    /// <summary>
    /// Text to put before the bar model number.  Set via the Unity Inspector.
    /// </summary>
    public string Prefix;
    /// <summary>
    /// Text to put after the bar model number.  Set via the Unity Inspector.
    /// </summary>
    public string Suffix;
    /// <summary>
    /// The Text Component of the price display.  May be null if none exists.  Set via the Unity Inspector.
    /// </summary>
    public UnityEngine.UI.Text priceText;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        SetSize();
    }

    public void SetSize() {
        GetComponent<UnityEngine.UI.Text>().text = Prefix + BarManager.inst.BarModel + Suffix;
        if(priceText != null) priceText.text = "$" + (BarManager.inst.BarPrice * 0.01f).ToString("F2");
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(priceText != null && (priceText.enabled ^ CameraControl.ShowPricing)) {
            priceText.enabled = CameraControl.ShowPricing;
        }
    }
}
