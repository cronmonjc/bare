using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI Component.  Holds information on certain options that can be selected by the user.
/// </summary>
[RequireComponent(typeof(Button))]
public abstract class PulldownItem : MonoBehaviour {
    /// <summary>
    /// The index of the PulldownItem amongst its peers.  Set via Unity Inspector.
    /// </summary>
    public int number;
    /// <summary>
    /// The colors the background takes on when the item is selected.  Set via Unity Inspector.
    /// </summary>
    public ColorBlock selected = new ColorBlock() { colorMultiplier = 1f, normalColor = new Color32(255, 192, 0, 255), highlightedColor = new Color32(255, 223, 127, 255), pressedColor = new Color32(255, 192, 0, 255), disabledColor = new Color32(255, 144, 144, 255) };
    /// <summary>
    /// The colors the background takes on when the item is not selected.  Set via Unity Inspector.
    /// </summary>
    public ColorBlock unselected = new ColorBlock() { colorMultiplier = 1f, normalColor = new Color32(255, 255, 255, 255), highlightedColor = new Color32(245, 245, 245, 255), pressedColor = new Color32(200, 200, 200, 255), disabledColor = new Color32(255, 144, 144, 255) };
    /// <summary>
    /// The Button Component reference
    /// </summary>
    protected Button b;
    /// <summary>
    /// Cached information, used to test if the selection status has changed
    /// </summary>
    protected bool prev = false;

    /// <summary>
    /// Determines whether this Component's option is selected.
    /// </summary>
    /// <returns>True if it's selected</returns>
    protected abstract bool IsSelected();
    /// <summary>
    /// Called when the user clicks on the Button Component on this GameObject.
    /// </summary>
    public abstract void Clicked();

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(b == null) b = GetComponent<Button>();
        bool curr = IsSelected();
        if(prev ^ curr) {
            b.colors = curr ? selected : unselected;
            prev = curr;
        }
    }
}
