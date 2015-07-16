using UnityEngine;
using System.Collections;

public class OddTD : IssueChecker {

    public override bool DoCheck() {
        bool foundTraff = false;
        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead alpha = BarManager.inst.allHeads[h];
            if(alpha.gameObject.activeInHierarchy) {
                foundTraff = false;
                for(byte i = 0; i < alpha.lhd.funcs.Count; i++) {
                    foundTraff |= alpha.lhd.funcs[i] == BasicFunction.TRAFFIC;
                }
                if(foundTraff ^ alpha.shouldBeTD) return true;
            }
        }
        return false;
    }

    public override string pdfText {
        get { return "This bar is using Traffic Director heads, however they seem to differ from the presets that are available.  It is suggested that one of the presets available from the Bar Menu are used for the best symmetry and functionality."; }
    }
}
