using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OutputOverload : IssueChecker {
    public uint maxAllowablePerOutput = 2500;

    private Dictionary<byte, uint> map;

    public override string pdfText {
        get { return ""; }
    }

    public override bool DoCheck() {
        if(map == null) map = new Dictionary<byte, uint>();

        for(byte i = 0; i < 32; i++) {
            map[i] = 0;
        }

        LightHead alpha;
        byte thisBit = 0;
        for(byte i = 0; i < BarManager.inst.allHeads.Count; i++) {
            alpha = BarManager.inst.allHeads[i];
            if(!alpha.gameObject.activeInHierarchy) continue;
            if(!alpha.hasRealHead) continue;
            if(alpha.Bit == 255) continue;

            thisBit = (byte)(alpha.Bit + (alpha.isRear ? 16 : 0));
            map[thisBit] += alpha.lhd.optic.amperage;
            if(map[thisBit] > maxAllowablePerOutput) return true;
        }
        return false;
    }
}
