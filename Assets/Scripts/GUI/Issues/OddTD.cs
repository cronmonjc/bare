using UnityEngine;
using System.Collections;

public class OddTD : IssueChecker {

    public override bool DoCheck() {
        RaycastHit[] hits = Physics.RaycastAll(new Vector3(-9, -1.25f, 0), new Vector3(1, 0, 0));

        foreach(RaycastHit hit in hits) {
            LightHead alpha = hit.transform.GetComponent<LightHead>();
            if(alpha != null && alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC) && !alpha.shouldBeTD) {
                return true;
            }
        }
        return false;
    }
}
