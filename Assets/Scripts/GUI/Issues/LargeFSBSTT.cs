using UnityEngine;
using System.Collections;

public class LargeFSBSTT : IssueChecker {

    public override bool DoCheck() {
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(!lh.gameObject.activeInHierarchy) continue;
            if(lh.isSmall) continue;
            if(lh.lhd.funcs.Contains(BasicFunction.STEADY) || lh.lhd.funcs.Contains(BasicFunction.STT)) {
                return true;
            }
        }

        return false;
    }

    public override string pdfText {
        get { return "This bar is using long optics to handle the Steady Burn or Stop/Tail/Turn functions.  It is suggested to use a short optic for these functions for better directionality."; }
    }
}
