using UnityEngine;
using System.Collections;

public class BadSizeTD : IssueChecker {

    public override bool DoCheck() {
        return BarManager.inst.td == TDOption.LG_SIX && BarManager.inst.BarSize == 3;
    }

    public override string pdfText {
        get { return "This bar utilizes the Six Long Traffic Director option, and is 58 inches long.  Please be aware the center section is not populated for the sake of symmetry.  If this is not wanted, please use either the Seven Long option or a different size bar."; }
    }
}
