using UnityEngine;
using System.Collections;

public class ToFnEdit : MonoBehaviour {
    public AdvFunction myFunc;

    public bool IsNext, IsPrev;

    public void Clicked() {
        if(IsNext) {
            if(FunctionEditPane.currFunc == AdvFunction.EMITTER) {
                FunctionEditPane.currFunc = AdvFunction.TAKEDOWN;
            } else {
                FunctionEditPane.currFunc = (AdvFunction)((int)FunctionEditPane.currFunc << 1);
                if(FunctionEditPane.currFunc == AdvFunction.PATTERN) {
                    FunctionEditPane.currFunc = (AdvFunction)((int)FunctionEditPane.currFunc << 1);
                }
            }
        } else if(IsPrev) {
            if(FunctionEditPane.currFunc == AdvFunction.TAKEDOWN) {
                FunctionEditPane.currFunc = AdvFunction.EMITTER;
            } else {
                FunctionEditPane.currFunc = (AdvFunction)((int)FunctionEditPane.currFunc >> 1);
                if(FunctionEditPane.currFunc == AdvFunction.PATTERN) {
                    FunctionEditPane.currFunc = (AdvFunction)((int)FunctionEditPane.currFunc >> 1);
                }
            }
        } else {
            FunctionEditPane.currFunc = myFunc;
        }

        LightLabel.showPatt = (FunctionEditPane.currFunc != AdvFunction.NONE);

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.hasRealHead) continue;
            if(LightLabel.showPatt) alpha.PrefetchPatterns(FunctionEditPane.currFunc);
            alpha.myLabel.Refresh();
        }

        if(FuncEnable.clr1 != null) FuncEnable.clr1.Retest();
        if(FuncEnable.clr2 != null) FuncEnable.clr2.Retest();
    }
}
