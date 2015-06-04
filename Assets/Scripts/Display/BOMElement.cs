using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// GUI Item.  Displays the individual element in the BOM.
/// </summary>
public class BOMElement : MonoBehaviour {
    /// <summary>
    /// Is this element an "unconfigured" element?  That is, does it display the quantity of items that still need to be configured?
    /// </summary>
    public bool unconfigured;

    /// <summary>
    /// What type of element is this? <seealso cref="BOMControl.BOMType"/>
    /// </summary>
    public BOMControl.BOMType type;

    /// <summary>
    /// How many of this item are there?
    /// </summary>
    public int quantity;

    /// <summary>
    /// If this is a <see cref="BOMControl.BOMType.Lights"/> element, the description of the light head it's showing.
    /// </summary>
    public LightHead headToDescribe;

    /// <summary>
    /// If this is a <see cref="BOMControl.BOMType.FlasherBundles"/> element, the part number of the flasher bundle it's showing.
    /// </summary>
    public string bundleToDescribe;
    
    public Text qty, desc, amp, cost;
    
    void LateUpdate() {
        qty.text = quantity + "x"; // Always update the quantity.
        if(!unconfigured) {
            if(type == BOMControl.BOMType.Lights && headToDescribe != null && headToDescribe.lhd.style != null) {
                desc.text = headToDescribe.PartNumber + " -- " + (headToDescribe.lhd.optic.styles.Count > 1 ? headToDescribe.lhd.style.name + " " : "") + headToDescribe.lhd.optic.name;
                
                amp.text = (headToDescribe.lhd.optic.amperage * quantity * 0.001f).ToString("F2") + "A";
                if(CameraControl.ShowPricing) {
                    cost.text = "$" + (headToDescribe.lhd.optic.cost * quantity * 0.01f).ToString("F2");
                } else {
                    cost.text = "";
                }
            //} else if(type == BOMControl.BOMType.Lenses && lensToDescribe != null) {
            //    // This massive line displays the entire description of the lens.  Blame long names for the length of this line.
            //    desc.text = (lensToDescribe.side ? MainCameraControl.sideLensPart : MainCameraControl.midLensPart) +
            //        lensToDescribe.myLensOption.part + (MainCameraControl.clearCoat ? "-C" : "") + " -- " +
            //        lensToDescribe.myLensOption.name + (lensToDescribe.side ? " End" : " Mid") + " Lens" +
            //        (MainCameraControl.clearCoat ? " with Clear Coating" : "");
            //} else if(type == BOMControl.BOMType.FlasherBundles) {
            //    desc.text = bundleToDescribe + " -- " + cam.flasherBundles[bundleToDescribe].description;
            //    if(MainCameraControl.ShowPricing) {
            //        cost.text = "$" + (cam.flasherBundles[bundleToDescribe].cost * quantity * 0.01f).ToString("F2");
            //    } else {
            //        cost.text = "";
            //    }
            //    if(amp != null) { // Because I'm lazy and used the same prefab for both lights and flasher bundles...
            //        Destroy(amp.gameObject);  //...this handles getting rid of the Amperage display item.
            //        amp = null;
            //    }
            }
        }
    }
}