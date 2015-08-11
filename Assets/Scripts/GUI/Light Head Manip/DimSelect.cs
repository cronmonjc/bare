using UnityEngine;
using System.Collections;

using fNbt;

/// <summary>
/// UI Component.  Currently Unused.  Sits on a Slider Component and simply modifies the dim percentage value in the pattern bytes.
/// </summary>
public class DimSelect : MonoBehaviour {
    /// <summary>
    /// The minimum percentage the value can be
    /// </summary>
    [Range(0, 100)]
    public short minimumPercentage = 15;
    /// <summary>
    /// The maximum percentage the value can be
    /// </summary>
    [Range(0, 100)]
    public short maximumPercentage = 25;

    /// <summary>
    /// The reference to the label Text Component
    /// </summary>
    public UnityEngine.UI.Text label;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        UnityEngine.UI.Slider s = GetComponentInChildren<UnityEngine.UI.Slider>();
        s.minValue = minimumPercentage;
        s.maxValue = maximumPercentage;
        s.wholeNumbers = true;
        s.value = maximumPercentage;
        label.text = maximumPercentage + "%";
        CurrVal = maximumPercentage;
    }

    /// <summary>
    /// Gets or sets the current percentage value.
    /// </summary>
    public short CurrVal {
        get {
            return FindObjectOfType<BarManager>().patts.Get<NbtCompound>("dim").Get<NbtShort>("dimp").ShortValue;
        }
        set {
            FindObjectOfType<BarManager>().patts.Get<NbtCompound>("dim").Get<NbtShort>("dimp").Value = value;
        }
    }

    /// <summary>
    /// Changes the value.  Called by sliding the slider.
    /// </summary>
    public void ChangeValue(float to) {
        CurrVal = (short)Mathf.Clamp(to, minimumPercentage, maximumPercentage);
        label.text = CurrVal + "%";
        BarManager.moddedBar = true;
    }

    /// <summary>
    /// Sets the value by code.  Also forces the slider to a certain position.
    /// </summary>
    public void SetValue(short to) {
        CurrVal = (short)Mathf.Clamp(to, minimumPercentage, maximumPercentage);
        label.text = CurrVal + "%";

        GetComponentInChildren<UnityEngine.UI.Slider>().value = to;
        BarManager.moddedBar = true;
    }
}
