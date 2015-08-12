using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using fNbt;
using System.Text;

/// <summary>
/// UI Component.  Displays information about the lenses.
/// </summary>
public class LensLabel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    /// <summary>
    /// The target of this label
    /// </summary>
    public Transform target;
    /// <summary>
    /// The label Text Component, used to display information
    /// </summary>
    public Text label;
    /// <summary>
    /// The background Image Component, used for coloration
    /// </summary>
    public Image background;
    /// <summary>
    /// The selection Image Component, used to indicate selection
    /// </summary>
    public Image selectionImage;

    /// <summary>
    /// The reference to the BarSegment on the target
    /// </summary>
    private BarSegment seg;

    /// <summary>
    /// The reference to the CameraControl, to make figuring out selection easier
    /// </summary>
    public static CameraControl cam;
    /// <summary>
    /// The reference to the tooltip
    /// </summary>
    public static LabelTooltip tooltip;
    /// <summary>
    /// The last lens assigned to the BarSegment, for automated refreshing
    /// </summary>
    private Lens lastLens;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        selectionImage.gameObject.SetActive(false);
        selectionImage.transform.rotation = Quaternion.identity;
        if(seg == null) seg = target.GetComponent<BarSegment>();
        Refresh();
    }

    /// <summary>
    /// Refreshes this Component.
    /// </summary>
    public void Refresh() {
        if(LightLabel.showParts) { // If LightLabels are showing parts, so should LensLabels
            if(seg.lens != null) {
                label.text = seg.LensPart;
                label.color = Color.black;
                background.color = Color.white;
            }
        } else if(LightLabel.showJustBit || LightLabel.showWire || LightLabel.showPatt) { // LightLabels showing light-only information, so hide lenses
            label.text = "";
            background.color = new Color(0f, 0f, 0f, 0f);
        } else { // Show description
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

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        else {
            #region Move label into position
            if(target != null) {
                transform.position = target.position;
                transform.rotation = target.rotation;

                if(!target.gameObject.activeInHierarchy) {
                    gameObject.SetActive(false);
                }
            } 
            #endregion
        }

        #region Refresh label
        if(seg == null) seg = target.GetComponent<BarSegment>();
        if(seg.lens != lastLens) {
            Refresh();
        } 
        #endregion

        #region Selection Image
        if(cam.SelectedLens.Contains(seg)) {
            selectionImage.gameObject.SetActive(true);
            selectionImage.transform.Rotate(new Vector3(0, 0, 20f) * Time.deltaTime);
        } else {
            selectionImage.gameObject.SetActive(false);
            selectionImage.transform.rotation = Quaternion.identity;
        } 
        #endregion
    }

    /// <summary>
    /// Called when the user moves a mouse onto an object.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerEnter(PointerEventData eventData) {
        if(tooltip == null) tooltip = FindObjectOfType<LabelTooltip>();
        if(label.text.Length > 0)
            tooltip.Show(label.text);
    }

    /// <summary>
    /// Called when the user moves a mouse off an object.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerExit(PointerEventData eventData) {
        tooltip.Hide();
    }
}
