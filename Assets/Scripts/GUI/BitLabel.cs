using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BitLabel : MonoBehaviour {
    public Transform target;
    public Text label;

    private LightHead lh;

    public static CameraControl cam;

    void Update() {
        if(CameraControl.funcBeingTested != Function.NONE) return;
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        else {
            transform.localRotation = Quaternion.identity;
            if(target != null) {
                transform.position = target.position;
                transform.localPosition = transform.localPosition + new Vector3(0, 0, -0.75f);

                if(!target.gameObject.activeInHierarchy) {
                    gameObject.SetActive(false);
                }
            }
        }

        if(lh == null) lh = target.GetComponent<LightHead>();
        else {
            label.text = lh.bits[FindObjectOfType<BarManager>().BarSize] + "";
        }
    }
}
