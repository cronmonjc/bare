using UnityEngine;
using System.Collections;

public class NonLightTD : IssueChecker {

    public override bool DoCheck() {
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.shouldBeTD && !alpha.hasRealHead) return true;
        }
        return false;
    }

    public override string pdfText {
        get { return "This bar is not using a light in a spot that has been designated for a Traffic Director light.  A light should be used in that spot, or the Traffic Director option should be changed to None."; }
    }
}
