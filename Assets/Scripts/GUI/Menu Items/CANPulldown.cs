using UnityEngine;
using System.Collections;

public class CANPulldown : PulldownItem {
    protected override bool IsSelected() {
        return number == 0 ? BarManager.useCAN : !BarManager.useCAN;
    }

    public override void Clicked() {
        BarManager.useCAN = (number == 0);
        BarManager.moddedBar = true;
        if(BarManager.inst.patts.Contains("prog")) BarManager.inst.patts.Remove("prog");
    }
}
