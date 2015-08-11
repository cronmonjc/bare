using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Calculates "basic phase" - ie a blind test of whether or not a head is using Phase A or B for all of the Flashing functions - and applies them too.
/// </summary>
public class BasicPhase : MonoBehaviour {
    /// <summary>
    /// Does this Component apply Phase B when clicked?  Set via Unity Inspector.
    /// </summary>
    public bool IsPhaseB;
    /// <summary>
    /// Does this Component manage color 2?  Set via Unity Inspector.
    /// </summary>
    public bool IsColor2;
    /// <summary>
    /// The checkmark Image Component reference.  Set via Unity Inspector.
    /// </summary>
    public Image Check;
    /// <summary>
    /// The Button Component reference.  Set via Unity Inspector.
    /// </summary>
    private Button b;

    /// <summary>
    /// Refreshes this Component.  Test all selected heads for phase and phasability.
    /// </summary>
    public void Refresh() {
        bool active = true;
        bool interact = false;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected) {
                if(IsColor2) active &= IsPhaseB ? alpha.basicPhaseB2 : alpha.basicPhaseA2;
                else active &= IsPhaseB ? alpha.basicPhaseB : alpha.basicPhaseA;
                interact |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
            }
        }
        Check.enabled = active & interact;
        if(b == null) b = GetComponent<Button>();
        b.interactable = interact;
    }

    /// <summary>
    /// Called when the user clicks the Button Component on the same GameObject.
    /// </summary>
    public void Clicked() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.Selected && alpha.lhd.funcs.Contains(BasicFunction.FLASHING)) {
                if(IsColor2) {
                    alpha.basicPhaseB2 = IsPhaseB;
                } else {
                    alpha.basicPhaseB = IsPhaseB;
                }
            }
        }
        foreach(BasicPhase alpha in FindObjectsOfType<BasicPhase>()) {
            alpha.Refresh();
        }
        BarManager.moddedBar = true;
    }
}
