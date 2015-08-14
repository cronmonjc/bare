using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Applies a specific function to the FunctionEditPane.currFunc value when clicked.
/// </summary>
public class ToFnEdit : MonoBehaviour {
    /// <summary>
    /// The function to apply to FunctionEditPane.currFunc.  Set via Unity Inspector.
    /// </summary>
    public AdvFunction myFunc;

    /// <summary>
    /// Rather than applying a static function, does this Component simply cycle to the next function?  Set via Unity Inspector.
    /// </summary>
    public bool IsNext;
    /// <summary>
    /// Rather than applying a static function, does this Component simply cycle to the previous function?  Set via Unity Inspector.
    /// </summary>
    public bool IsPrev;

    /// <summary>
    /// Called when the user clicks the Button Component on this GameObject.
    /// </summary>
    public void Clicked() {
        if(IsNext) {
            if(FunctionEditPane.currFunc == AdvFunction.EMITTER) { // Loop around from Emitter to Takedown
                FunctionEditPane.currFunc = AdvFunction.TAKEDOWN;
            } else {
                FunctionEditPane.currFunc = (AdvFunction)((int)FunctionEditPane.currFunc << 1); // Shift one up
                if(FunctionEditPane.currFunc == AdvFunction.PATTERN) { // Never show Pattern.  If we're at it now, shift up again
                    FunctionEditPane.currFunc = (AdvFunction)((int)FunctionEditPane.currFunc << 1);
                }
            }
        } else if(IsPrev) {
            if(FunctionEditPane.currFunc == AdvFunction.TAKEDOWN) { // Loop around from Takedown to Emitter
                FunctionEditPane.currFunc = AdvFunction.EMITTER;
            } else {
                FunctionEditPane.currFunc = (AdvFunction)((int)FunctionEditPane.currFunc >> 1); // Shift one down
                if(FunctionEditPane.currFunc == AdvFunction.PATTERN) { // Never show Pattern.  If we're at it now, shift down again
                    FunctionEditPane.currFunc = (AdvFunction)((int)FunctionEditPane.currFunc >> 1);
                }
            }
        } else {
            FunctionEditPane.currFunc = myFunc; // Apply static function
        }

        LightLabel.showPatt = (FunctionEditPane.currFunc != AdvFunction.NONE); // If the current function is now AdvFunction.NONE, don't show patterns on labels.

        // LightInteractionPanel handles the switching of panes to and from the pattern editing pane.

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy) continue;
                if(alpha.hasRealHead && LightLabel.showPatt) alpha.PrefetchPatterns(FunctionEditPane.currFunc); // Get the heads to fetch the patterns it's using for the function
            alpha.myLabel.Refresh(); // Also refresh their labels
        }

        if(FuncEnable.clr1 != null) FuncEnable.clr1.Retest(); // Retest enable buttons
        if(FuncEnable.clr2 != null) FuncEnable.clr2.Retest();
    }
}
