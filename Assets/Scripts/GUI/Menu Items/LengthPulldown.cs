using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Manages options for cable length.
/// </summary>
public class LengthPulldown : PulldownItem {
    /// <summary>
    /// Determines whether this Component's option is selected.
    /// </summary>
    /// <returns>
    /// True if it's selected
    /// </returns>
    protected override bool IsSelected() {
        return BarManager.cableLength == number;
    }

    /// <summary>
    /// Called when the user clicks on the Button Component on this GameObject.
    /// </summary>
    public override void Clicked() {
        BarManager.cableLength = number;
        BarManager.moddedBar = true;

        BOMCables bomc = FindObjectOfType<BOMCables>();
        if(bomc != null)
            bomc.Refresh();
    }
}
