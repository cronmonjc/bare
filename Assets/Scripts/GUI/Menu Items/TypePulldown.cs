using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Manages options for cable types / counts.
/// </summary>
public class TypePulldown : PulldownItem {
    /// <summary>
    /// Determines whether this Component's option is selected.
    /// </summary>
    /// <returns>
    /// True if it's selected
    /// </returns>
    protected override bool IsSelected() {
        return BarManager.cableType == number;
    }

    /// <summary>
    /// Called when the user clicks on the Button Component on this GameObject.
    /// </summary>
    public override void Clicked() {
        BarManager.cableType = number;
        BarManager.moddedBar = true;

        BOMCables bomc = FindObjectOfType<BOMCables>();
        if(bomc != null)
            bomc.Refresh();
    }
}
