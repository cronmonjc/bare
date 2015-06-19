using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

public class FuncEnable : MonoBehaviour {
    public static FuncEnable clr1, clr2;
    private Image checkmark;
    private Text label;
    private Button button;
    public bool IsColor2 = false;

    void Start() {
        button = GetComponent<Button>();
        label = transform.FindChild("Label").GetComponent<Text>();
        checkmark = transform.FindChild("Checkmark").GetComponent<Image>();
        if(IsColor2) clr2 = this;
        else clr1 = this;
        Retest();
    }

    public void Retest() {
        if(FunctionEditPane.currFunc == AdvFunction.NONE) return;

        NbtCompound patts = BarManager.inst.patts;
        bool enabled = false, disabled = false, selectable = false;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            bool thisSelectable = false;

            switch(FunctionEditPane.currFunc) {
                case AdvFunction.LEVEL1:
                case AdvFunction.LEVEL2:
                case AdvFunction.LEVEL3:
                case AdvFunction.LEVEL4:
                case AdvFunction.LEVEL5:
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

            if(thisSelectable) {
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

                enabled |= thisEnabled;
                disabled |= !thisEnabled;
                
                selectable = true;
            }
            
        }

        button.interactable = selectable;

        if(!selectable) {
            checkmark.enabled = false;
            label.text = "Cannot Enable Color " + (IsColor2 ? "2" : "1");
        } else {
            checkmark.enabled = !disabled;

            if(enabled && disabled) {
                label.text = "Color " + (IsColor2 ? "2" : "1") + " Partly Enabled";
            } else {
                label.text = "Color " + (IsColor2 ? "2" : "1") + (enabled ? " Enabled" : " Disabled");
            }
        }
    }

    public void Enable() {
        NbtCompound patts = BarManager.inst.patts;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            bool trigger = false;

            switch(FunctionEditPane.currFunc) {
                case AdvFunction.LEVEL1:
                case AdvFunction.LEVEL2:
                case AdvFunction.LEVEL3:
                case AdvFunction.LEVEL4:
                case AdvFunction.LEVEL5:
                case AdvFunction.FTAKEDOWN:
                case AdvFunction.FALLEY:
                case AdvFunction.ICL:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING);
                    break;
                case AdvFunction.TAKEDOWN:
                case AdvFunction.ALLEY_LEFT:
                case AdvFunction.ALLEY_RIGHT:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.STEADY);
                    break;
                case AdvFunction.TURN_LEFT:
                case AdvFunction.TURN_RIGHT:
                case AdvFunction.TAIL:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.STT);
                    break;
                case AdvFunction.T13:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.CAL_STEADY);
                    break;
                case AdvFunction.EMITTER:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.EMITTER);
                    break;
                case AdvFunction.CRUISE:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.CRUISE);
                    break;
                case AdvFunction.TRAFFIC_LEFT:
                case AdvFunction.TRAFFIC_RIGHT:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC);
                    break;
                case AdvFunction.DIM:
                    trigger |= true;
                    break;
                default:
                    break;
            }

            if(trigger) {
                string cmpdName = BarManager.GetFnString(alpha.transform, FunctionEditPane.currFunc);
                if(cmpdName == null) {
                    Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                    return;
                }
                NbtShort en = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("e" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1"));

                en.EnableBit(alpha.Bit);
            }
        }

        clr1.Retest();
        clr2.Retest();

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }
        BarManager.moddedBar = true;
        if(patts.Contains("prog")) patts.Remove("prog");
    }

    public void Clicked() {
        NbtCompound patts = BarManager.inst.patts;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            bool trigger = false;

            switch(FunctionEditPane.currFunc) {
                case AdvFunction.LEVEL1:
                case AdvFunction.LEVEL2:
                case AdvFunction.LEVEL3:
                case AdvFunction.LEVEL4:
                case AdvFunction.LEVEL5:
                case AdvFunction.FTAKEDOWN:
                case AdvFunction.FALLEY:
                case AdvFunction.ICL:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING);
                    break;
                case AdvFunction.TAKEDOWN:
                case AdvFunction.ALLEY_LEFT:
                case AdvFunction.ALLEY_RIGHT:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.STEADY);
                    break;
                case AdvFunction.TURN_LEFT:
                case AdvFunction.TURN_RIGHT:
                case AdvFunction.TAIL:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.STT);
                    break;
                case AdvFunction.T13:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.CAL_STEADY);
                    break;
                case AdvFunction.EMITTER:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.EMITTER);
                    break;
                case AdvFunction.CRUISE:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.CRUISE);
                    break;
                case AdvFunction.TRAFFIC_LEFT:
                case AdvFunction.TRAFFIC_RIGHT:
                    trigger |= alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC);
                    break;
                case AdvFunction.DIM:
                    trigger |= true;
                    break;
                default:
                    break;
            }

            if(trigger) {
                string cmpdName = BarManager.GetFnString(alpha.transform, FunctionEditPane.currFunc);
                if(cmpdName == null) {
                    Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                    return;
                }
                NbtShort en = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("e" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1"));

                if(checkmark.enabled) {
                    en.DisableBit(alpha.Bit);
                } else {
                    en.EnableBit(alpha.Bit);
                }
            }
        }

        clr1.Retest();
        clr2.Retest();

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }
        BarManager.moddedBar = true;
        if(patts.Contains("prog")) patts.Remove("prog");
    }
}
