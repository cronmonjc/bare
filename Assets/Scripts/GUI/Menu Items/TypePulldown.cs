using UnityEngine;
using System.Collections;

public class TypePulldown : PulldownItem {
    protected override bool IsSelected() {
        return BarManager.cableType == number;
    }

    public override void Clicked() {
        BarManager.cableType = number;
    }
}
