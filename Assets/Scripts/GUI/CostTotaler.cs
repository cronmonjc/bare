using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI Component.  Totals the sale price of the components.
/// </summary>
public class CostTotaler : MonoBehaviour {
    /// <summary>
    /// The reference to the total object GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject TotalObject;
    /// <summary>
    /// The text displaying the total sale price.  Set via Unity Inspector.
    /// </summary>
    public Text TotalText;

    /// <summary>
    /// The reference to a BOMCables object.  Set via Unity Inspector.
    /// </summary>
    public BOMCables Cables;

    /// <summary>
    /// The total sale price
    /// </summary>
    public uint total;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(CameraControl.ShowPricing) {
            total = LightDict.inst.bracketPrice;  // Get static gutter mount kit price over with

            if(BarManager.mountingKit != 0)
                total += LightDict.inst.mountKits[BarManager.mountingKit - 1].price;  // Add mounting kit price if using one

            total += BarManager.inst.BarPrice;  // Add base bar cost

            total += Cables.totalCost; // Add costs for all cables and circuit

            foreach(LightHead alpha in BarManager.inst.allHeads) {  // Add all heads' prices
                if(!alpha.gameObject.activeInHierarchy) continue;
                if(alpha.lhd.style == null) continue;

                total += alpha.lhd.optic.cost;
            }

            foreach(BarSegment seg in BarManager.inst.allSegs) {  // Then all lenses
                if(!seg.gameObject.activeInHierarchy) continue;

                total += seg.lens.cost;
            }

            TotalText.text = "$" + (total * 0.01f).ToString("F2");

            TotalObject.SetActive(true);
        } else {
            total = 0;
            TotalObject.SetActive(false);
        }
    }
}
