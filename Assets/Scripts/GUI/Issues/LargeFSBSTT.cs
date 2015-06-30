using UnityEngine;
using System.Collections;

public class LargeFSBSTT : IssueChecker {

    public override bool DoCheck() {
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(!lh.gameObject.activeInHierarchy) continue;
            if(lh.isSmall) continue;
            for(byte i = 0; i < lh.lhd.funcs.Count; i++)
                if(lh.lhd.funcs[i] == BasicFunction.STEADY || lh.lhd.funcs[i] == BasicFunction.STT) {
                    return true;
                }
        }

        return false;
    }

    public override string pdfText {
        get { return "This bar is using long optics to handle the Steady Burn or Stop/Tail/Turn functions.  It is suggested to use a short optic for these functions for better directionality."; }
    }
}
