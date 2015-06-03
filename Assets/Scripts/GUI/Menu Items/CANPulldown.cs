using UnityEngine;
using System.Collections;

public class CANPulldown : PulldownItem {
    protected override bool IsSelected() {
        return number == 0 ? BarManager.useCAN : !BarManager.useCAN;
    }

    public override void Clicked() {
        BarManager.useCAN = (number == 0);
    }
}
