using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

/// <summary>
/// UI Component.  Component on a Button that enables a certain color on a LightHead for a certain function.
/// </summary>
public class FuncEnable : MonoBehaviour {
    /// <summary>
    /// Static references to instances for specific colors
    /// </summary>
    public static FuncEnable clr1, clr2;
    /// <summary>
    /// Reference to the tip text GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject tip;
    /// <summary>
    /// Is the function currently enabled for the selected heads?
    /// </summary>
    private bool funcEnabled;
    /// <summary>
    /// The label Text Component
    /// </summary>
    private Text label;
    /// <summary>
    /// The Button Component
    /// </summary>
    private Button button;
    /// <summary>
    /// Is this Component managing color 2?  Set via Unity Inspector.
    /// </summary>
    public bool IsColor2 = false;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        button = GetComponent<Button>();
        label = transform.FindChild("Label").GetComponent<Text>();
        if(IsColor2) clr2 = this;
        else clr1 = this;
        Retest();
    }

    /// <summary>
    /// Retests this Component, checking if the selected heads are already enabled.
    /// </summary>
    public void Retest() {
        if(FunctionEditPane.currFunc == AdvFunction.NONE) return; // We aren't modifying a function, return now

        #region Setup
        NbtCompound patts = BarManager.inst.patts;
        bool enabled = false, disabled = false, selectable = false;
        string clrText = ""; 
        #endregion

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected || !alpha.hasRealHead) continue;

            #region Figure out if the head can be enabled
            bool thisSelectable = false;

            switch(FunctionEditPane.currFunc) {
                case AdvFunction.PRIO1:
                case AdvFunction.PRIO2:
                case AdvFunction.PRIO3:
                case AdvFunction.PRIO4:
                case AdvFunction.PRIO5:
                case AdvFunction.FTAKEDOWN:
                case AdvFunction.FALLEY:
                case AdvFunction.ICL:
                    thisSelectable |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.TAKEDOWN:
                case AdvFunction.ALLEY_LEFT:
                case AdvFunction.ALLEY_RIGHT:
                    thisSelectable |= alpha.lhd.funcs.Contains(BasicFunction.STEADY) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.TURN_LEFT:
                case AdvFunction.TURN_RIGHT:
                case AdvFunction.TAIL:
                    thisSelectable |= alpha.lhd.funcs.Contains(BasicFunction.STT) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.T13:
                    thisSelectable |= alpha.lhd.funcs.Contains(BasicFunction.CAL_STEADY) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.EMITTER:
                    thisSelectable |= alpha.lhd.funcs.Contains(BasicFunction.EMITTER) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.CRUISE:
                    thisSelectable |= alpha.lhd.funcs.Contains(BasicFunction.CRUISE) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.TRAFFIC_LEFT:
                case AdvFunction.TRAFFIC_RIGHT:
                    thisSelectable |= alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.DIM:
                    thisSelectable |= (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                default:
                    break;
            } 
            #endregion

            if(thisSelectable) {
                #region Test if head is enabled
                string cmpdName = BarManager.GetFnString(alpha.transform, FunctionEditPane.currFunc);
                if(cmpdName == null) {
                    Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                    return;
                }
                if(!patts.Get<NbtCompound>(cmpdName).Contains("e" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1"))) {
                    button.interactable = false;
                    return;
                }
                short en = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("e" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1")).ShortValue;

                bool thisEnabled = ((en & (0x1 << alpha.Bit)) > 0); 
                #endregion

                enabled |= thisEnabled;
                disabled |= !thisEnabled;
                
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
            label.text = "Cannot Enable " + clrText;
            tip.gameObject.SetActive(true);
        } else {
            funcEnabled = !disabled;
            #region Coloration of the button
            ColorBlock cb = button.colors;
            cb.highlightedColor = cb.normalColor = (!disabled ? new Color(0.8f, 1.0f, 0.8f, 1.0f) : (!enabled ? new Color(1.0f, 0.8f, 0.8f, 1.0f) : new Color(1.0f, 1.0f, 1.0f, 1.0f)));
            button.colors = cb;
            #endregion
            tip.gameObject.SetActive(false);

            if(enabled && disabled) {
                label.text = clrText + " Partly Enabled";
            } else {
                label.text = clrText + (enabled ? " Enabled" : " Disabled");
            }
        }
    }

    /// <summary>
    /// Enables the selected heads for this Component's color.
    /// </summary>
    public void Enable() {
        NbtCompound patts = BarManager.inst.patts;
        #region Enable all selected heads
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            if(alpha.GetCanEnable(FunctionEditPane.currFunc)) { // Test if we can enable
                string cmpdName = BarManager.GetFnString(alpha.transform, FunctionEditPane.currFunc);
                if(cmpdName == null) {
                    Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                    return;
                }
                NbtShort en = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("e" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1"));

                en.EnableBit(alpha.Bit);
            }
        } 
        #endregion

        #region Retest and Refresh
        clr1.Retest();
        clr2.Retest();

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        } 

        FunctionEditPane fep = transform.GetComponentInParent<FunctionEditPane>();
        switch(fep.state) {
            case FunctionEditPane.ShowState.FLASHING:
                foreach(FuncPattSelect fps in fep.flashing.GetComponentsInChildren<FuncPattSelect>(true)) {
                    fps.Refresh();
                }
                break;
            case FunctionEditPane.ShowState.TRAFFIC:
                foreach(FuncPattSelect fps in fep.traffic.GetComponentsInChildren<FuncPattSelect>(true)) {
                    fps.Refresh();
                }
                break;
            default:
                break;
        }
        #endregion

        BarManager.moddedBar = true;
        if(patts.Contains("prog")) patts.Remove("prog"); // Remove default program tag, no longer applies
    }

    /// <summary>
    /// Called when the user clicks the Button this Component is on.
    /// </summary>
    public void Clicked() {
        NbtCompound patts = BarManager.inst.patts;
        #region Enable/disable all selected heads
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            if(alpha.GetCanEnable(FunctionEditPane.currFunc)) {
                string cmpdName = BarManager.GetFnString(alpha.transform, FunctionEditPane.currFunc);
                if(cmpdName == null) {
                    Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                    return;
                }
                NbtShort en = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("e" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1"));

                if(funcEnabled) {
                    en.DisableBit(alpha.Bit);
                } else {
                    en.EnableBit(alpha.Bit);
                }
            }
        } 
        #endregion

        #region Retest and Refresh
        clr1.Retest();
        clr2.Retest();

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        } 
        #endregion

        BarManager.moddedBar = true;
        if(patts.Contains("prog")) patts.Remove("prog"); // Remove default program tag, no longer applies
    }
}
