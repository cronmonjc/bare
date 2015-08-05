using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class TitleText : MonoBehaviour {

    public static TitleText inst;

    private Text t;
    public string currFile;
    public string preset;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        inst = this;
    }

    void Update() {
        if(t == null) t = GetComponent<Text>();

        if(currFile.Length > 1) {
            t.text = currFile + (BarManager.moddedBar ? "**" : "");
        } else if(preset.Length > 1) {
            t.text = "New " + preset + (BarManager.moddedBar ? "**" : "");
        } else {
            t.text = "New Phaser Light Bar" + (BarManager.moddedBar ? "**" : "");
        }
    }
}
