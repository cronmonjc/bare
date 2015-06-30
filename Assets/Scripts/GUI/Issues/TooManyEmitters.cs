using UnityEngine;
using System.Collections;

public class TooManyEmitters : IssueChecker {
    private int count;

    public override bool DoCheck() {
        count = 0;

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy) {
                for(byte i = 0; i < alpha.lhd.funcs.Count; i++) {
                    if(alpha.lhd.funcs[i] == BasicFunction.EMITTER) count++;
                    if(count == 2) return true;
                }
            }
        }
        return false;
    }

    public override string pdfText {
        get { return "This bar is using more than one Emitter head on it.  The extras should be removed."; }
    }
}
