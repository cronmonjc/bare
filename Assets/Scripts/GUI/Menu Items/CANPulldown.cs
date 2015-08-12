using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Manages options for CAN.
/// </summary>
public class CANPulldown : PulldownItem {
    /// <summary>
    /// Determines whether this Component's option is selected.
    /// </summary>
    /// <returns>
    /// True if it's selected
    /// </returns>
    protected override bool IsSelected() {
        return number == 0 ? BarManager.useCAN : !BarManager.useCAN;
    }

    /// <summary>
    /// Called when the user clicks on the Button Component on this GameObject.
    /// </summary>
    public override void Clicked() {
        BarManager.useCAN = (number == 0);
        BarManager.moddedBar = true;
        if(BarManager.inst.patts.Contains("prog")) BarManager.inst.patts.Remove("prog");

        BOMCables bomc = FindObjectOfType<BOMCables>();
        if(bomc != null)
            bomc.Refresh();
    }
}
