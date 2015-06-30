using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

public class FuncPhase : MonoBehaviour {
    public static FuncPhase ph1, ph2;
    private Text label;
    private Button button;
    public bool IsColor2 = false;
    public bool CurrentlyB = false;

    void Start() {
        button = GetComponent<Button>();
        label = transform.FindChild("Label").GetComponent<Text>();
        if(IsColor2) ph2 = this;
        else ph1 = this;
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
            if(!patts.Get<NbtCompound>(cmpdName).Contains("pf1")) {
                button.interactable = false;
                return;
            }

            Pattern patt = alpha.GetPattern(FunctionEditPane.currFunc, IsColor2);

            bool thisSelectable = (alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && patt != null && (patt is FlashPatt || patt is SingleFlashRefPattern || patt is DoubleFlashRefPattern) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual)));

            if(thisSelectable) {
                short ph = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("p" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1")).ShortValue;

                bool thisEnabled = ((ph & (0x1 << alpha.Bit)) > 0);

                enabled |= thisEnabled;
                disabled |= !thisEnabled;

                selectable = true;
            }
        }

        button.interactable = selectable;

        if(!selectable) {
            CurrentlyB = false;
            label.text = "Cannot Phase Color " + (IsColor2 ? "2" : "1");
        } else {
            CurrentlyB = !disabled;
            if(enabled && disabled) {
                label.text = "Color " + (IsColor2 ? "2" : "1") + " Mixed Phase";
            } else {
                label.text = "Color " + (IsColor2 ? "2" : "1") + (enabled ? " Phase B" : " Phase A");
            }
        }
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
                default:
                    break;
            }

            Pattern patt = alpha.GetPattern(FunctionEditPane.currFunc, IsColor2);

            trigger &= (alpha.lhd.funcs.Contains(BasicFunction.FLASHING) && patt != null && (patt is FlashPatt || patt is SingleFlashRefPattern || patt is DoubleFlashRefPattern) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual)));

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

        ph1.Retest();
        ph2.Retest();

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }
        BarManager.moddedBar = true;
        if(patts.Contains("prog")) patts.Remove("prog");
    }

}
