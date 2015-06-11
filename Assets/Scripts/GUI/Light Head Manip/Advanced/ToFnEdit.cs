using UnityEngine;
using System.Collections;

public class ToFnEdit : MonoBehaviour {
    public AdvFunction myFunc;

    public void Clicked() {
        FunctionEditPane.currFunc = myFunc;

        LightLabel.showPatt = (myFunc != AdvFunction.NONE);

        foreach(LightLabel ll in FindObjectsOfType<LightLabel>()) {
            ll.Refresh();
        }

        if(FuncEnable.clr1 != null) FuncEnable.clr1.Retest();
        if(FuncEnable.clr2 != null) FuncEnable.clr2.Retest();
    }
}
