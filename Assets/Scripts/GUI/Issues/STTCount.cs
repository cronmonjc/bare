using UnityEngine;
using System.Collections;

public class STTCount : IssueChecker {

    public override string pdfText {
        get { return "This bar does not utilize the exactly two Stop/Tail/Turn lights we recommend."; }
    }

    public override bool DoCheck() {
        byte count = 0;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            for(byte i = 0; i < alpha.lhd.funcs.Count; i++) {
                if(alpha.lhd.funcs[i] == BasicFunction.STT) {
                    count++;
                }
            }
        }
        return count != 0 && count != 2;
    }
}
