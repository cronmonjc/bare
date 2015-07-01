using UnityEngine;
using System.Collections;

/// <summary>
/// GUI Item, Issue.  Checks that no lens mismatches with any head it's over.
/// </summary>
public class LensColorIssue : IssueChecker {
    public override bool DoCheck() {
        foreach(BarSegment seg in BarManager.inst.allSegs) {
            if(seg.Visible && seg.lens != null) {
                foreach(LightHead head in seg.AffectedLights) {
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

    public override string pdfText {
        get { return "This bar has lenses whose colors confict with the colors of the light heads underneath them."; }
    }
}
