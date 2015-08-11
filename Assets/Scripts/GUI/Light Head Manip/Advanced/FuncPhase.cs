using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

/// <summary>
/// UI Component.  Component on a Button that applies a certain phase on a LightHead for a certain flashing function.
/// </summary>
public class FuncPhase : MonoBehaviour {
    /// <summary>
    /// Static references to instances for specific colors
    /// </summary>
    public static FuncPhase ph1, ph2;
    /// <summary>
    /// The label Text Component
    /// </summary>
    private Text label;
    /// <summary>
    /// The Button Component
    /// </summary>
    private Button button;
    /// <summary>
    /// Is this Component managing color 2?
    /// </summary>
    public bool IsColor2 = false;
    /// <summary>
    /// Is the phase currently B for the selected heads?
    /// </summary>
    public bool CurrentlyB = false;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        button = GetComponent<Button>();
        label = transform.FindChild("Label").GetComponent<Text>();
        if(IsColor2) ph2 = this;
        else ph1 = this;
        Retest();
    }

    /// <summary>
    /// Retests this Component, checking the current phase of the selected heads.
    /// </summary>
    public void Retest() {
        if(FunctionEditPane.currFunc == AdvFunction.NONE) return; // We aren't modifying a function, return now

        #region Setup
        NbtCompound patts = BarManager.inst.patts;
        bool enabled = false, disabled = false, selectable = false;
        string clrText = ""; 
        #endregion

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            string cmpdName = BarManager.GetFnString(alpha.transform, FunctionEditPane.currFunc);
            if(cmpdName == null) {
                Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                return;
            }
            if(!patts.Get<NbtCompound>(cmpdName).Contains("pf1")) { // Cannot phase, return now
                selectable = false;
                break;
            }

            Pattern patt = alpha.GetPattern(FunctionEditPane.currFunc, IsColor2);

            bool thisSelectable = (alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && patt != null && (patt is FlashPatt || patt is SingleFlashRefPattern || patt is DoubleFlashRefPattern) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual)));

            if(thisSelectable) {
                short ph = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("p" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1")).ShortValue;
                // Test if head is phase B
                bool thisPhased = ((ph & (0x1 << alpha.Bit)) > 0);

                enabled |= thisPhased;
                disabled |= !thisPhased;

                selectable = true;
                string[] clrs = alpha.lhd.style.name.Split('/'); // Fetch color names

                if(clrText.Length > 0) {
                    if(!clrText.StartsWith("Color")) // Have a proper color name currently
                        if(!clrText.Equals(clrs[(clrs.Length > 1 && IsColor2) ? 1 : 0])) // Colors don't match, go generic
                            clrText = "Color " + (IsColor2 ? "2" : "1");
                } else {
                    clrText = clrs[(clrs.Length > 1 && IsColor2) ? 1 : 0]; // Apply color name
                }
            }
        }

        button.interactable = selectable;

        if(!selectable) {
            CurrentlyB = false;
            label.text = "Cannot Phase " + clrText;
        } else {
            CurrentlyB = !disabled;
            if(enabled && disabled) {
                label.text = clrText + " Mixed Phase";
            } else {
                label.text = clrText + (enabled ? " Phase B" : " Phase A");
            }
        }
    }

    /// <summary>
    /// Called when the user clicks the Button this Component is on.
    /// </summary>
    public void Clicked() {
        NbtCompound patts = BarManager.inst.patts;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            #region Test if we can Phase these heads
            bool trigger = false;

            #region Test if function is Flashing
            switch(FunctionEditPane.currFunc) {
                case AdvFunction.PRIO1:
                case AdvFunction.PRIO2:
                case AdvFunction.PRIO3:
                case AdvFunction.PRIO4:
                case AdvFunction.PRIO5:
                case AdvFunction.FTAKEDOWN:
                case AdvFunction.FALLEY:
                case AdvFunction.ICL:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING);
                    break;
                default:
                    break;
            } 
            #endregion

            Pattern patt = alpha.GetPattern(FunctionEditPane.currFunc, IsColor2);

            // Test if pattern is Phase-able
            trigger &= (alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && patt != null && (patt is FlashPatt || patt is SingleFlashRefPattern || patt is DoubleFlashRefPattern) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual))); 
            #endregion

            if(trigger) {
                string cmpdName = BarManager.GetFnString(alpha.transform, FunctionEditPane.currFunc);
                if(cmpdName == null) {
                    Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                    return;
                }
                NbtShort ph = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("p" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1"));

                if(CurrentlyB) {
                    ph.DisableBit(alpha.Bit);
                } else {
                    ph.EnableBit(alpha.Bit);
                }
            }
        }

        #region Retest and Refresh
        ph1.Retest();
        ph2.Retest();

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        } 
        #endregion
        BarManager.moddedBar = true;
        if(patts.Contains("prog")) patts.Remove("prog"); // Remove default program tag, no longer applies
    }

}
