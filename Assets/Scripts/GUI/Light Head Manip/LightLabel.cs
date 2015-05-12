using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightLabel : MonoBehaviour {
    public Transform target;
    public Text label;
    public Image background, secondImage, selectionImage;

    private LightHead lh;

    public static CameraControl cam;
    private StyleNode lastStyle;

    void Start() {
        selectionImage.gameObject.SetActive(false);
        selectionImage.transform.rotation = Quaternion.identity;
        if(lh == null) lh = target.GetComponent<LightHead>();
        if(lh.isSmall) {
            ((RectTransform)transform).sizeDelta = new Vector2(65, 48);
        }
        Refresh();
    }

    public void Refresh() {
        if(lh.lhd.style != null) {
            label.text = "(" + lh.Bit + ")\n" + lh.lhd.style.name + " " + lh.lhd.optic.name;
            Color clr = lh.lhd.style.color;
            background.color = clr;
            if(clr.r + clr.g < clr.b) {
                label.color = Color.white;
            } else {
                label.color = Color.black;
            }
            if(lh.lhd.style.isDualColor) {
                clr = lh.lhd.style.color2;
            }
            secondImage.color = clr;
        } else {
            label.text = "(" + lh.Bit + ")\nEmpty";
            label.color = Color.white;
            background.color = new Color(0, 0, 0, 0.45f);
            secondImage.color = new Color(0, 0, 0, 0.45f);
        }

        lastStyle = lh.lhd.style;
    }

    void Update() {
        if(CameraControl.funcBeingTested != Function.NONE) return;
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        else {
            if(target != null) {
                transform.position = target.position;
                transform.rotation = target.rotation;

                if(!target.gameObject.activeInHierarchy) {
                    gameObject.SetActive(false);
                }
            }
        }

        if(lh == null) lh = target.GetComponent<LightHead>();
        if(lh.lhd.style != lastStyle) {
            Refresh();
        }

        if(lh.Selected) {
            selectionImage.gameObject.SetActive(true);
            selectionImage.transform.Rotate(new Vector3(0, 0, 20f) * Time.deltaTime);
        } else {
            selectionImage.gameObject.SetActive(false);
            selectionImage.transform.rotation = Quaternion.identity;
        }
    }
}
