using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Totals the amperage in use by the bar.
/// </summary>
public class AmperageTotal : MonoBehaviour {
    /// <summary>
    /// The display Text Component.  Set via Unity Inspector.
    /// </summary>
    public Text text;
    /// <summary>
    /// The total current draw of the bar
    /// </summary>
    public uint totalAmp;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        totalAmp = 0;
        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead alpha = BarManager.inst.allHeads[h];
            if(alpha.gameObject.activeInHierarchy && alpha.hasRealHead) {
                totalAmp += alpha.lhd.optic.amperage;
            }
        }

        text.text = "Total Current Draw: " + (totalAmp * 0.001).ToString("F3") + "A Max / " + (totalAmp * 0.0005).ToString("F3") + "A Avg";
    }
}
