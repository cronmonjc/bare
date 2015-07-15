using UnityEngine;
using System.Collections;

public class BarSizeDisplay : MonoBehaviour {
    public string Prefix, Suffix;
    public UnityEngine.UI.Text priceText;

    public void SetSize() {
        GetComponent<UnityEngine.UI.Text>().text = Prefix + BarManager.inst.BarModel + Suffix;
        if(priceText != null) priceText.text = "$" + (BarManager.inst.BarPrice * 0.01f).ToString("F2");
    }
}
