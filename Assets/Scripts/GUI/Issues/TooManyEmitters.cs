using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component, Issue.  Checks that only one Emitter is used, if any.
/// </summary>
public class TooManyEmitters : IssueChecker {
    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        byte count = 0;

        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead alpha = BarManager.inst.allHeads[h];
            if(alpha.gameObject.activeInHierarchy) {
                for(byte i = 0; i < alpha.lhd.funcs.Count; i++) {
                    if(alpha.lhd.funcs[i] == BasicFunction.EMITTER) count++;
                    if(count == 2) return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return "This bar is using more than one Emitter head on it.  The extras should be removed."; }
    }
}
