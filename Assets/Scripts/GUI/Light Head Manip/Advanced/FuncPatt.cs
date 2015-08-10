using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Individual element for FuncPattSelect.
/// </summary>
public class FuncPatt : MonoBehaviour {
    /// <summary>
    /// The Text Component showing this Component's pattern name.  Set via Unity Inspector.
    /// </summary>
    public Text text;
    /// <summary>
    /// The Pattern this Component holds.  Set by FuncPattSelect.
    /// </summary>
    public Pattern patt;
    /// <summary>
    /// The toggle image.  Set via Unity Inspector.
    /// </summary>
    public Image i;
    /// <summary>
    /// The FuncPattSelect to phone home to when selected.  Set by FuncPattSelect.
    /// </summary>
    public FuncPattSelect fps;

    /// <summary>
    /// Refreshes this Component, testing whether all selected heads have its pattern
    /// </summary>
    public void Refresh() {
        bool anyEnabled = false;
        bool on = true;
        text.text = patt.name;

        if(!BarManager.inst.patts.Get<fNbt.NbtCompound>(BarManager.GetFnString(BarManager.inst.transform, FunctionEditPane.currFunc)).Contains(patt is TraffPatt ? "patt" : "pat1")) {
            return;
        }

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;
            anyEnabled |= alpha.GetIsEnabled(FunctionEditPane.currFunc, fps.IsColor2, true);
            on &= alpha.GetPattern(FunctionEditPane.currFunc, fps.IsColor2) == patt;
        }

        i.enabled = anyEnabled & on;
    }

    /// <summary>
    /// Called when a user clicks on the Button this Component is on
    /// </summary>
    public void Clicked() {
        fps.SetSelection(patt);
    }
}
