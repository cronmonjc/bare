using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BitLabel : MonoBehaviour {
    public Transform target;
    public Text label;

    private LightHead lh;

    public static CameraControl cam;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        else {
            transform.localRotation = Quaternion.identity;
            if(target != null) {
                transform.position = target.position;

                if(!target.gameObject.activeInHierarchy) {
                    gameObject.SetActive(false);
                }
            }
        }

        if(lh == null) lh = target.GetComponent<LightHead>();
        else {
            label.text = lh.Bit + "";
        }
    }
}
