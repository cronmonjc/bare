using UnityEngine;
using System.Collections;

using fNbt;

public class DimSelect : MonoBehaviour {
    [Range(0, 100)]
    public short minimumPercentage = 15, maximumPercentage = 25;

    public UnityEngine.UI.Text label;

    void Start() {
        UnityEngine.UI.Slider s = GetComponentInChildren<UnityEngine.UI.Slider>();
        s.minValue = minimumPercentage;
        s.maxValue = maximumPercentage;
        s.wholeNumbers = true;
        s.value = maximumPercentage;
        label.text = maximumPercentage + "%";
        CurrVal = maximumPercentage;
    }

    public short CurrVal {
        get {
            return FindObjectOfType<BarManager>().patts.Get<NbtCompound>("dim").Get<NbtShort>("dimp").ShortValue;
        }
        set {
            FindObjectOfType<BarManager>().patts.Get<NbtCompound>("dim").Get<NbtShort>("dimp").Value = value;
        }
    }

    public void ChangeValue(float to) {
        CurrVal = (short)Mathf.Clamp(to, minimumPercentage, maximumPercentage);
        label.text = CurrVal + "%";
        BarManager.moddedBar = true;
    }

    public void SetValue(short to) {
        CurrVal = (short)Mathf.Clamp(to, minimumPercentage, maximumPercentage);
        label.text = CurrVal + "%";

        GetComponentInChildren<UnityEngine.UI.Slider>().value = to;
        BarManager.moddedBar = true;
    }
}
