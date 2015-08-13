using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI Component.  Unused.  Allows for the changing between two color sets of a button programmatically and easily on the same GameObject.
/// </summary>
public class ChangeColors : MonoBehaviour {

    /// <summary>
    /// The "chosen idle" color.  Set via Unity Inspector.
    /// </summary>
    public Color chosenIdle;
    /// <summary>
    /// The "chosen highlighted" color.  Set via Unity Inspector.
    /// </summary>
    public Color chosenHighlighted;
    /// <summary>
    /// The "unchosen idle" color.  Set via Unity Inspector.
    /// </summary>
    public Color unchosenIdle;
    /// <summary>
    /// The "unchosen highlighted" color.  Set via Unity Inspector.
    /// </summary>
    public Color unchosenHighlighted;

    /// <summary>
    /// Switches this Component Button's colors.
    /// </summary>
    /// <param name="curr">Whether or not to show the "chosen" colors</param>
    public void Switch(bool curr) {
        Button b = GetComponent<Button>();
        ColorBlock cb = b.colors;
        if(curr) {
            cb.normalColor = chosenIdle;
            cb.highlightedColor = chosenHighlighted;
        } else {
            cb.normalColor = unchosenIdle;
            cb.highlightedColor = unchosenHighlighted;
        }
        b.colors = cb;
    }

}
