using UnityEngine;
using System.Collections;

public class LargeFSBSTT : IssueChecker {
    public bool IsSteadyBurn;

    public override bool DoCheck() {
        foreach(LightHead lh in BarManager.inst.allHeads) {
            if(!lh.gameObject.activeInHierarchy) continue;
            if(lh.isSmall) continue;
            if(lh.loc == Location.FRONT_CORNER || lh.loc == Location.REAR_CORNER) continue;
            for(byte i = 0; i < lh.lhd.funcs.Count; i++)
                if((IsSteadyBurn && lh.lhd.funcs[i] == BasicFunction.STEADY) || (!IsSteadyBurn && lh.lhd.funcs[i] == BasicFunction.STT)) {
                    return true;
                }
        }

        return false;
    }

    public override string pdfText {
        get { return "This bar is using long optics to handle the " + (IsSteadyBurn ? "Steady Burn" : "Stop/Tail/Turn") + " function.  It is suggested to use a short optic for that function for better directionality."; }
    }
}
