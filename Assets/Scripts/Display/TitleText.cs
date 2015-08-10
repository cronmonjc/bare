using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI Component.  Manages the title text at the top of the screen.
/// </summary>
[RequireComponent(typeof(Text))]
public class TitleText : MonoBehaviour {
    /// <summary>
    /// The single instance of this class
    /// </summary>
    public static TitleText inst;

    /// <summary>
    /// The Text Component reference
    /// </summary>
    private Text t;
    /// <summary>
    /// The current file
    /// </summary>
    public string currFile;
    /// <summary>
    /// The preset in use
    /// </summary>
    public string preset;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        inst = this;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(t == null) t = GetComponent<Text>(); // Get Text if we don't have one yet

        if(currFile.Length > 1) { // Have a filename
            t.text = currFile + (BarManager.moddedBar ? "**" : "");
        } else if(preset.Length > 1) { // No filename, but based on preset
            t.text = "New " + preset + (BarManager.moddedBar ? "**" : "");
        } else { // Based on blank bar
            t.text = "New Phaser Light Bar" + (BarManager.moddedBar ? "**" : "");
        }
    }
}
