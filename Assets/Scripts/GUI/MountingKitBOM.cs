using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Displays BOM information on the Mounting Kit in use.
/// </summary>
public class MountingKitBOM : MonoBehaviour {
    /// <summary>
    /// The reference to the description Text Component.
    /// </summary>
    private Text desc;
    /// <summary>
    /// The reference to the price Text Component.
    /// </summary>
    private Text price;
    /// <summary>
    /// The previous Mounting Kit used.
    /// </summary>
    private int prev = -1;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(desc == null) {
            desc = transform.Find("Desc").GetComponent<Text>();
            price = transform.Find("Price").GetComponent<Text>();
        }

        if(prev != BarManager.mountingKit) {
            if(BarManager.mountingKit == 0) {
                gameObject.SetActive(false);
                return;
            } else {
                MountingKitOption opt = LightDict.inst.mountKits[BarManager.mountingKit - 1];
                price.text = string.Format("${0:F2}", opt.price * 0.01f);
                desc.text = opt.name;
            }
            BarManager.moddedBar = true;
            prev = BarManager.mountingKit;
        }

        if(price.gameObject.activeInHierarchy ^ CameraControl.ShowPricing) {
            price.gameObject.SetActive(CameraControl.ShowPricing);
        }
    }
}
