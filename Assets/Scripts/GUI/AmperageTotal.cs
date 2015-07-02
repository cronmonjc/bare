using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AmperageTotal : MonoBehaviour {
    public Text text;

    void Update() {
        uint totalAmp = 0;
        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.hasRealHead) {
                totalAmp += alpha.lhd.optic.amperage;
            }
        }

        text.text = "Total Current Draw: " + (totalAmp * 0.001).ToString("F3") + "A Max / " + (totalAmp * 0.0005).ToString("F3") + "A Avg";
    }
}
