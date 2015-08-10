using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Component, Issue.  Checks to make sure that there isn't a Cool White head under an Amber lens.
/// </summary>
public class AmberOverCoolWhiteIssue : IssueChecker {
    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        for(byte s = 0; s < BarManager.inst.allSegs.Count; s++) {
            BarSegment seg = BarManager.inst.allSegs[s];
            if(seg.lens != null && seg.lens.partSuffix.Equals("a", System.StringComparison.CurrentCultureIgnoreCase)) {
                for(byte h = 0; h < seg.AffectedLights.Count; h++) {
                    LightHead head = seg.AffectedLights[h];
                    if(head.gameObject.activeInHierarchy && head.hasRealHead) {
                        if(head.lhd.style.partSuffix.Contains("C")) {
                            return true;
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
        get { return "This bar has at least one Cool White light head underneath an Amber lens.  A Warm White head is suggested to improve the brightness through the lens."; }
    }
}
