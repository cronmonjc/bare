using UnityEngine;
using System.Collections;

public class BadSizeTD : IssueChecker {
    public override bool DoCheck() {
        return BarManager.inst.td == TDOption.LG_SIX && BarManager.inst.BarSize == 3;
    }
}
