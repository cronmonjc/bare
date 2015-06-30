using UnityEngine;
using System.Collections;

public class OddTD : IssueChecker {

    public override bool DoCheck() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy) {
                for(byte i = 0; i < alpha.lhd.funcs.Count; i++) {
                    if(alpha.lhd.funcs[i] == BasicFunction.TRAFFIC && !alpha.shouldBeTD) return true;
                }
            }
        }
        return false;
    }

    public override string pdfText {
        get { return "This bar is using Traffic Director heads, however they seem to differ from the presets that are available.  It is suggested that one of the presets available from the Bar Menu are used for the best symmetry and functionality."; }
    }
}
