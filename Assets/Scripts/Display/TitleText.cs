using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class TitleText : MonoBehaviour {

    private Text t;
    public FileBrowser fb;
    public string preset;

    void Update() {
        if(t == null) t = GetComponent<Text>();

        if(fb.currFile.Length > 1) {
            t.text = fb.currFile + (BarManager.moddedBar ? "**" : "");
        } else if(preset.Length > 1) {
            t.text = "New " + preset + (BarManager.moddedBar ? "**" : "");
        } else {
            t.text = "New 1000-Series Light Bar" + (BarManager.moddedBar ? "**" : "");
        }
    }
}
