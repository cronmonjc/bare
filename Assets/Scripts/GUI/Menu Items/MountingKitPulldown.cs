using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Manages options for mounting kits.
/// </summary>
public class MountingKitPulldown : PulldownItem {
    /// <summary>
    /// Determines whether this Component's option is selected.
    /// </summary>
    /// <returns>
    /// True if it's selected
    /// </returns>
    protected override bool IsSelected() {
        return BarManager.mountingKit == number;
    }

    /// <summary>
    /// Called when the user clicks on the Button Component on this GameObject.
    /// </summary>
    public override void Clicked() {
        BarManager.mountingKit = number;
    }
}
