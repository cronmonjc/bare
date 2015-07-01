using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using fNbt;
using System.Text;

public class LensLabel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public Transform target;
    public Text label;
    public Image background, selectionImage;

    private BarSegment seg;

    public static CameraControl cam;
    public static LabelTooltip tooltip;
    private Lens lastLens;

    void Start() {
        selectionImage.gameObject.SetActive(false);
        selectionImage.transform.rotation = Quaternion.identity;
        if(seg == null) seg = target.GetComponent<BarSegment>();
        Refresh();
    }

    public void Refresh() {
        if(LightLabel.showParts) {
            if(seg.lens != null) {
                label.text = seg.LensPart;
                label.color = Color.black;
                background.color = Color.white;
            }
        } else if(LightLabel.showJustBit || LightLabel.showWire || LightLabel.showPatt) {
            label.text = "";
            background.color = new Color(0f, 0f, 0f, 0f);
        } else {
            label.text = seg.LensDescrip;
            if(seg.lens != null) {
                Color clr = seg.lens.color;
                background.color = clr;
                if(clr.r + clr.g < clr.b) {
                    label.color = Color.white;
                } else {
                    label.color = Color.black;
                }
            } else {
                label.color = Color.white;
                background.color = new Color(0, 0, 0, 0.45f);
            }
        }

        lastLens = seg.lens;
    }

    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        else {
            if(target != null) {
                transform.position = target.position;
                transform.rotation = target.rotation;

                if(!target.gameObject.activeInHierarchy) {
                    gameObject.SetActive(false);
                }
            }

            if(cam.SelectedLens.Contains(seg)) {
                selectionImage.gameObject.SetActive(true);
                selectionImage.transform.Rotate(new Vector3(0, 0, 20f) * Time.deltaTime);
            } else {
                selectionImage.gameObject.SetActive(false);
                selectionImage.transform.rotation = Quaternion.identity;
            }
        }

        if(seg == null) seg = target.GetComponent<BarSegment>();
        if(seg.lens != lastLens) {
            Refresh();
        }

        if(cam.SelectedLens.Contains(seg)) {
            selectionImage.gameObject.SetActive(true);
            selectionImage.transform.Rotate(new Vector3(0, 0, 20f) * Time.deltaTime);
        } else {
            selectionImage.gameObject.SetActive(false);
            selectionImage.transform.rotation = Quaternion.identity;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if(tooltip == null) tooltip = FindObjectOfType<LabelTooltip>();
        if(label.text.Length > 0)
            tooltip.Show(label.text);
    }

    public void OnPointerExit(PointerEventData eventData) {
        tooltip.Hide();
    }
}
