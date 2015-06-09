﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

public class FuncEnable : MonoBehaviour {
    private static FuncEnable clr1, clr2;
    private FunctionEditPane pane;
    private Image checkmark;
    private Text label;
    private Button button;
    public bool IsColor2 = false;

    void Start() {
        button = GetComponent<Button>();
        label = transform.FindChild("Label").GetComponent<Text>();
        checkmark = transform.FindChild("Checkmark").GetComponent<Image>();
        pane = transform.parent.parent.GetComponent<FunctionEditPane>();
        if(IsColor2) clr2 = this;
        else clr1 = this;
    }

    public void Retest() {
        NbtCompound patts = BarManager.inst.patts;
        bool enabled = false, disabled = false, selectable = false;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            string cmpdName = BarManager.GetFnString(alpha.transform, pane.currFunc);
            if(cmpdName == null) {
                Debug.LogWarning(pane.currFunc.ToString() + " has no similar setting in the data bytes.");
                return;
            }
            short en = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("e" + (alpha.transform.position.z < 0 ? "r" : "f") + (IsColor2 ? "2" : "1")).ShortValue;

            bool thisEnabled = ((en & (0x1 << alpha.Bit)) > 0);

            enabled |= thisEnabled;
            disabled |= !thisEnabled;

            switch(pane.currFunc) {
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
                case AdvFunction.ALLEY:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.STEADY) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.CRUISE:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.CRUISE) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.EMITTER:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.EMITTER) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.T13:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.CAL_STEADY) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.STT_AND_TAIL:
                    selectable |= alpha.lhd.funcs.Contains(BasicFunction.STT) && (!IsColor2 || (alpha.lhd.optic != null && alpha.lhd.optic.dual));
                    break;
                case AdvFunction.TRAFFIC:
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
                label.text = "Color " + " Partly Enabled";
            } else {
                label.text = "Color " + (IsColor2 ? "2" : "1") + (enabled ? " Enabled" : " Disabled");
            }
        }
    }

    public void Clicked() {
        NbtCompound patts = BarManager.inst.patts;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            string cmpdName = BarManager.GetFnString(alpha.transform, pane.currFunc);
            if(cmpdName == null) {
                Debug.LogWarning(pane.currFunc.ToString() + " has no similar setting in the data bytes.");
                return;
            }
            NbtShort en = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("e" + (alpha.transform.position.z < 0 ? "r" : "f") + (IsColor2 ? "2" : "1"));

            if(checkmark.enabled) {
                en.DisableBit(alpha.Bit);
            } else {
                en.EnableBit(alpha.Bit);
            }
        }

        clr1.Retest();
        clr2.Retest();
    }
}
