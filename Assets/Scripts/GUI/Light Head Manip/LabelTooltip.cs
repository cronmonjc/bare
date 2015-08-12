using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Gives an increased view of the text on the labels.
/// </summary>
public class LabelTooltip : MonoBehaviour {
    /// <summary>
    /// The reference to the label GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject labelGO;
    /// <summary>
    /// The Text Component itself.  Set via Unity Inspector.
    /// </summary>
    public Text text;

    /// <summary>
    /// Shows the tooltip with the specified message.
    /// </summary>
    /// <param name="msg">The message.</param>
    public void Show(string msg) {
        text.text = msg;
        labelGO.SetActive(true);
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void Hide() {
        labelGO.SetActive(false);
    }
}
