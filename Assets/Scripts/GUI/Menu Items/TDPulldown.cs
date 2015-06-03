using UnityEngine;
using System.Collections;

public class TDPulldown : PulldownItem {
    protected override bool IsSelected() {
        return BarManager.inst.td == (TDOption)number;
    }

    public override void Clicked() {
        
    }
}
