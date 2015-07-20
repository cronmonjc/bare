using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MountingKitBOM : MonoBehaviour {
    private Text desc, price;
    private int prev = -1;

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
