using UnityEngine;
using System.Collections;

public class WarmWhiteIssue : IssueChecker {
    public override string pdfText {
        get { return "This bar appears to use Warm White lights.  Cool Whites are suggested for better illumination, unless these heads are to be covered by an Amber lens."; }
    }

    public override bool DoCheck() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
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
