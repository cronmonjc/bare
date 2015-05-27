using UnityEngine;
using System.Collections;

public class BarSizeDisplay : MonoBehaviour {
    public string Prefix, Suffix;
    public string[] Parts = new string[5];

    public void SetSize(float to) {
        GetComponent<UnityEngine.UI.Text>().text = Prefix + Parts[Mathf.RoundToInt(to)] + Suffix;
    }
}
