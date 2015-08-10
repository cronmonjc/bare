using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component, Issue.  Checks if any head is using the Warm White style and not covered by an Amber lens.
/// </summary>
public class WarmWhiteIssue : IssueChecker {
    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return "This bar appears to use Warm White lights.  Cool Whites are suggested for better illumination, unless these heads are to be covered by an Amber lens."; }
    }

    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead alpha = BarManager.inst.allHeads[h];
            if(alpha.gameObject.activeInHierarchy && alpha.lhd.style != null && alpha.lhd.style.partSuffix.Contains("W")) {
                foreach(BarSegment seg in BarManager.inst.allSegs) {
                    if(seg.AffectedLights.Contains(alpha) && seg.lens.partSuffix != "A") {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
