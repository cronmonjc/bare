using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

public class FuncPhase : MonoBehaviour {
    public static FuncPhase ph1, ph2;
    private Image checkmark;
    private Text label;
    private Button button;
    public bool IsColor2 = false;

    void Start() {
        button = GetComponent<Button>();
        label = transform.FindChild("Label").GetComponent<Text>();
        checkmark = transform.FindChild("Checkmark").GetComponent<Image>();
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
            short ph = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("p" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1")).ShortValue;

            bool thisEnabled = ((ph & (0x1 << alpha.Bit)) > 0);

            enabled |= thisEnabled;
            disabled |= !thisEnabled;

            Pattern patt = alpha.GetPattern(FunctionEditPane.currFunc, IsColor2);

            if(patt != null && patt is FlashPatt) {
                selectable |= !IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual);
            }
        }

        button.interactable = selectable;

        if(!selectable) {
            checkmark.enabled = false;
            label.text = "Cannot Phase Color " + (IsColor2 ? "2" : "1");
        } else {
            checkmark.enabled = !disabled;

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

            string cmpdName = BarManager.GetFnString(alpha.transform, FunctionEditPane.currFunc);
            if(cmpdName == null) {
                Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                return;
            }
            NbtShort ph = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("p" + (alpha.transform.position.y < 0 ? "r" : "f") + (IsColor2 ? "2" : "1"));

            if(checkmark.enabled) {
                ph.DisableBit(alpha.Bit);
            } else {
                ph.EnableBit(alpha.Bit);
            }
        }

        ph1.Retest();
        ph2.Retest();

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }
    }

}
