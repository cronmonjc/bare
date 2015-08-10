using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component, Issue.  Checks if a Traffic Director head is not using a real light (aka is not defined or is using Block Off).
/// </summary>
public class NonLightTD : IssueChecker {

    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead alpha = BarManager.inst.allHeads[h];
            if(alpha.gameObject.activeInHierarchy && alpha.shouldBeTD && !alpha.hasRealHead) return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return "This bar is not using a light in a spot that has been designated for a Traffic Director light.  A light should be used in that spot, or the Traffic Director option should be changed to None."; }
    }
}
