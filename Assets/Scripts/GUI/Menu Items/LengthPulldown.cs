using UnityEngine;
using System.Collections;

public class LengthPulldown : PulldownItem {
    protected override bool IsSelected() {
        return BarManager.cableLength == number;
    }

    public override void Clicked() {
        BarManager.cableLength = number;
        BarManager.moddedBar = true;
    }
}
