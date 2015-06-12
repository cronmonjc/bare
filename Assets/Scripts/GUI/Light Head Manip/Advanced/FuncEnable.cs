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

            switch(FunctionEditPane.currFunc) {
                case AdvFunction.LEVEL1:
                case AdvFunction.LEVEL2:
                case AdvFunction.LEVEL3:
                case AdvFunction.LEVEL4:
                case AdvFunction.LEVEL5:
                case AdvFunction.FTAKEDOWN:
                case AdvFunction.FALLEY:
                case AdvFunction.ICL:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.TAKEDOWN:
                case AdvFunction.ALLEY_LEFT:
                case AdvFunction.ALLEY_RIGHT:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.STEADY) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.TURN_LEFT:
                case AdvFunction.TURN_RIGHT:
                case AdvFunction.TAIL:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.STT) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.T13:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.CAL_STEADY) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.EMITTER:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.EMITTER) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.CRUISE:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.CRUISE) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.TRAFFIC_LEFT:
                case AdvFunction.TRAFFIC_RIGHT:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.DIM:
                    selectable |= (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                default:
                    break;
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

    public void Clicked() {
        NbtCompound patts = BarManager.inst.patts;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

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

        clr1.Retest();
        clr2.Retest();

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }
    }
}
