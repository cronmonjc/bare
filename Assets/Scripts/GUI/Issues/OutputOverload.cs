using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Component, Issue.  Checks if a single output on the bar is overloaded on current output.
/// </summary>
public class OutputOverload : IssueChecker {
    /// <summary>
    /// The maximum allowable current per output, in whole milliamps
    /// </summary>
    public uint maxAllowablePerOutput = 2500;

    /// <summary>
    /// The map of current used for each output
    /// </summary>
    private Dictionary<byte, uint> map;

    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return ""; }
    }

    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
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
