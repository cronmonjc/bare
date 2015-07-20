using UnityEngine;
using System.Collections;

public class MountingKitPulldown : PulldownItem {
    protected override bool IsSelected() {
        return BarManager.mountingKit == number;
    }

    public override void Clicked() {
        BarManager.mountingKit = number;
    }
}
