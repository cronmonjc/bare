using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Component, Issue.  Checks that no lens mismatches with any head it's over.
/// </summary>
public class LensColorIssue : IssueChecker {
    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        for(byte s = 0; s < BarManager.inst.allSegs.Count; s++) {
            BarSegment seg = BarManager.inst.allSegs[s];
            if(seg.Visible && seg.lens != null) {
                for(byte h = 0; h < seg.AffectedLights.Count; h++) {
                    LightHead head = seg.AffectedLights[h];
                    if(head.hasRealHead) {
                        if(!seg.lens.Test(head.lhd.style.color)) {
                            return true;
                        }
                        if(seg.lens.color != Color.white) {
                            if(head.lhd.style.isDualColor) {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return "This bar has lenses whose colors confict with the colors of the light heads underneath them."; }
    }
}
