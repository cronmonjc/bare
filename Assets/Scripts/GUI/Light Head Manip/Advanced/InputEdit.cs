using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Sits on a Pencil Image that's a child of a FnDragTarget, and offers editing of whatever that FnDragTarget's function currently is
/// </summary>
public class InputEdit : MonoBehaviour {
    /// <summary>
    /// Whether or not this is the second Pencil for the FnDragTarget.  Useful when the parent FnDragTarget has the Flashing Alley and Flashing Pursuit functions on it.  Set via Unity Inspector.
    /// </summary>
    public bool IsSecond = false;
    /// <summary>
    /// Called when the user clicks on the GameObject containing this Component.
    /// </summary>
    public void Clicked() {
        // Find the value the parent FnDragTarget is referencing
        int val = FnDragTarget.inputMap.Value[GetComponentInParent<FnDragTarget>().key];

        if(val == 0xC00) { // If parent has Flashing Alley and Flashing Pursuit functions...
            FunctionEditPane.currFunc = (IsSecond ? AdvFunction.FALLEY : AdvFunction.FTAKEDOWN); // ...go to Flashing Alley if we're second, otherwise go to Flashing Pursuit
        } else {
            FunctionEditPane.currFunc = (AdvFunction)val; // Otherwise just go to the value
        }
        // LightInteractionPanel will move the application to show the Function editing pane.

        #region Have LightLabels refresh, showing pattern
        LightLabel.showPatt = true;

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.hasRealHead)
                if(LightLabel.showPatt) alpha.PrefetchPatterns(FunctionEditPane.currFunc); // Get the heads to fetch the patterns it's using for the function
            alpha.myLabel.Refresh(); // Also refresh their labels
        }
        #endregion

        if(FuncEnable.clr1 != null) FuncEnable.clr1.Retest();
        if(FuncEnable.clr2 != null) FuncEnable.clr2.Retest();
        BarManager.moddedBar = true;
        if(BarManager.inst.patts.Contains("prog")) BarManager.inst.patts.Remove("prog");
    }
}
