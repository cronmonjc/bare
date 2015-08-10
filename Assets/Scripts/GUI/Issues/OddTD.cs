using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component, Issue.  Checks if any heads are set up Traffic Director when they shouldn't be, or aren't set up when they should.
/// </summary>
public class OddTD : IssueChecker {

    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        bool foundTraff = false;
        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead alpha = BarManager.inst.allHeads[h];
            if(alpha.gameObject.activeInHierarchy) {
                foundTraff = false;
                for(byte i = 0; i < alpha.lhd.funcs.Count; i++) {
                    foundTraff |= alpha.lhd.funcs[i] == BasicFunction.TRAFFIC;
                }
                if(foundTraff ^ alpha.shouldBeTD) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return "This bar is using Traffic Director heads, however they seem to differ from the presets that are available.  It is suggested that one of the presets available from the Bar Menu are used for the best symmetry and functionality."; }
    }
}
