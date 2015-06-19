using UnityEngine;
using System.Collections;

public class InputEdit : MonoBehaviour {
    public bool IsSecond = false;
    public void Clicked() {
        int val = FnDragTarget.inputMap.Value[GetComponentInParent<FnDragTarget>().key];

        if(val == 0xC00) {
            FunctionEditPane.currFunc = (IsSecond ? AdvFunction.FALLEY : AdvFunction.FTAKEDOWN);
        } else {
            FunctionEditPane.currFunc = (AdvFunction)val;
        }

        LightLabel.showPatt = true;

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }

        if(FuncEnable.clr1 != null) FuncEnable.clr1.Retest();
        if(FuncEnable.clr2 != null) FuncEnable.clr2.Retest();
        BarManager.moddedBar = true;
    }
}
