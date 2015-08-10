using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component, Issue.  Checks that only two Stop Tail Turns are used, if any.
/// </summary>
public class STTCount : IssueChecker {

    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return "This bar does not utilize the exactly two Stop/Tail/Turn lights we recommend."; }
    }

    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        byte count = 0;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.hasRealHead) continue;
            for(byte i = 0; i < alpha.lhd.funcs.Count; i++) {
                if(alpha.lhd.funcs[i] == BasicFunction.STT) {
                    count++;
                }
            }
        }
        return count != 0 && count != 2;
    }
}
