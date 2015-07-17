using UnityEngine;
using System.Collections;

public class MountingKitPulldown : PulldownItem {
    public MountingKitOption opt;

    protected override bool IsSelected() {
        return BarManager.mountingKit == opt;
    }

    public override void Clicked() {
        BarManager.mountingKit = opt;
    }
}
