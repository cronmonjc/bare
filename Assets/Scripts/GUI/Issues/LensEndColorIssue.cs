﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Component, Issue.  Checks that no end lens is colored and over a Cool White Alley.
/// </summary>
public class LensEndColorIssue : IssueChecker {
    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        for(byte s = 0; s < BarManager.inst.allSegs.Count; s++) {
            BarSegment seg = BarManager.inst.allSegs[s];
            if(seg.Visible && seg.lens != null && seg.IsEnd && seg.lens.color != Color.white) {
                for(byte h = 0; h < seg.AffectedLights.Count; h++) {
                    LightHead head = seg.AffectedLights[h];
                    if(head.hasRealHead && head.loc == Location.ALLEY && head.lhd.style.partSuffix.Equals("C", System.StringComparison.CurrentCultureIgnoreCase)) {
                        return true;
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
        get { return "This bar uses a colored lens overtop a Cool White alley head.  This will cause the alley to shine with the color of the lens, not white."; }
    }
}
