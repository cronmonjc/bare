using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Swaps the image on Traffic Director based on whether or not the option is selected.
/// </summary>
public class TDImageSwapper : MonoBehaviour {
    /// <summary>
    /// The idle sprite to use.  Set via Unity Inspector.
    /// </summary>
    public Sprite idle;
    /// <summary>
    /// The selected sprite to use.  Set via Unity Inspector.
    /// </summary>
    public Sprite selected;
    /// <summary>
    /// This Component's TDOption, used to test if the bar's option matches.  Set via Unity Inspector.
    /// </summary>
    public TDOption myOpt;
    /// <summary>
    /// The reference to the Image UI Component containing the sprite to swap.
    /// </summary>
    private UnityEngine.UI.Image img;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(img == null) img = GetComponent<UnityEngine.UI.Image>();

        img.sprite = (BarManager.inst.td == myOpt ? selected : idle);
    }
}
