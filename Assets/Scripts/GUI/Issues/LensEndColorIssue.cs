using UnityEngine;
using System.Collections;

/// <summary>
/// GUI Item, Issue.  Checks that no end lens is colored and over a Cool White Alley.
/// </summary>
public class LensEndColorIssue : IssueChecker {
    public override bool DoCheck() {
        foreach(BarSegment seg in BarManager.inst.allSegs) {
            if(seg.Visible && seg.lens != null && seg.IsEnd && seg.lens.color != Color.white) {
                foreach(LightHead head in seg.AffectedLights) {
                    if(head.hasRealHead && head.loc == Location.ALLEY && head.lhd.style.partSuffix.Equals("C", System.StringComparison.CurrentCultureIgnoreCase)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public override string pdfText {
        get { return "This bar uses a colored lens overtop a Cool White alley head."; }
    }
}
