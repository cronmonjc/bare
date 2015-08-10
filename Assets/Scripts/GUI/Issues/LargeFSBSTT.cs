using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component, Issue.  Checks if a head is large and is set up for either Steady Burn or Stop Tail Turn.
/// </summary>
public class LargeFSBSTT : IssueChecker {
    /// <summary>
    /// Is this instance checking for Steady Burn?  If not, it's checking for Stop Tail Turn
    /// </summary>
    public bool IsSteadyBurn;

    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead lh = BarManager.inst.allHeads[h];
            if(!lh.gameObject.activeInHierarchy) continue;
            if(lh.isSmall) continue;
            if(lh.loc == Location.FRONT_CORNER || lh.loc == Location.REAR_CORNER) continue;
            for(byte i = 0; i < lh.lhd.funcs.Count; i++)
                if((IsSteadyBurn && lh.lhd.funcs[i] == BasicFunction.STEADY) || (!IsSteadyBurn && lh.lhd.funcs[i] == BasicFunction.STT)) {
                    return true;
                }
        }

        return false;
    }

    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return "This bar is using long optics to handle the " + (IsSteadyBurn ? "Steady Burn" : "Stop/Tail/Turn") + " function.  It is suggested to use a short optic for that function for better directionality."; }
    }
}
