using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WordyWarn : MonoBehaviour {
    public Image icon;
    public Text text;
    public InputField notes;

    public void Test() {
        int breaks = notes.text.Split('\n').Length;
        int len = notes.text.Length;
        if(breaks > 1) {
            len += (breaks - 1) * 150;
        }
        if(len > 1500) {
            icon.enabled = text.enabled = true;
        } else {
            icon.enabled = text.enabled = false;
        }
    }
}
