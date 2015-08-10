using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component, Issue.  Checks for a 3-size light bar using a Large-Six Traffic Director option.
/// </summary>
public class BadSizeTD : IssueChecker {

    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        return BarManager.inst.td == TDOption.LG_SIX && BarManager.inst.BarSize == 3;
    }

    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return "This bar utilizes the Six Long Traffic Director option, and is 58 inches long.  Please be aware the center section is not populated for the sake of symmetry.  If this is not wanted, please use either the Seven Long option or a different size bar."; }
    }
}
