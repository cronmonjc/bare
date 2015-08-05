using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A bar segment.  Contains lens information.
/// </summary>
public class BarSegment : MonoBehaviour {
    /// <summary>
    /// Static reference to the CameraControl Component
    /// </summary>
    private static CameraControl cam;

    /// <summary>
    /// This Segment's lens information
    /// </summary>
    public Lens lens;

    /// <summary>
    /// A list of heads that this lens needs to watch for
    /// </summary>
    [System.NonSerialized]
    private LightHead[] affected;
    /// <summary>
    /// A list of heads that is active and that this lens needs to watch for
    /// </summary>
    [System.NonSerialized]
    private List<LightHead> liveAffected;

    /// <summary>
    /// A reference to the lens label prefab
    /// </summary>
    public GameObject labelPrefab;
    /// <summary>
    /// This segment's label
    /// </summary>
    public LensLabel myLabel;

    /// <summary>
    /// Should this bar segment be visible?
    /// </summary>
    public bool Visible {
        get { return IsEnd || VisibleOn[BarManager.inst.BarSize]; }
    }

    /// <summary>
    /// Get the description for this segment's lens
    /// </summary>
    public string LensDescrip {
        get {
            if(lens == null) return "No Lens";
            else return "Clear Coated " + lens.name + (IsEnd ? " End" : " Center") + " Lens";
        }
    }

    /// <summary>
    /// Get the part number for this segment's lens
    /// </summary>
    public string LensPart {
        get {
            if(lens == null) return "";
            else return (IsEnd ? Lens.lgPrefix : Lens.smPrefix) + lens.partSuffix;
        }
    }

    /// <summary>
    /// Is this segment on the end?  Set via the Unity Inspector.
    /// </summary>
    public bool IsEnd;
    /// <summary>
    /// If it's not on the end, on which of the five bar sizes is this segment visible?  Set via the Unity Inspector.
    /// </summary>
    public bool[] VisibleOn = new bool[] { false, false, false, false, true };
    /// <summary>
    /// Where is this segment positioned on the X axis for each of the five bar sizes?  Set via the Unity Inspector.
    /// </summary>
    public float[] XPosOn = new float[] { 0f, 0f, 0f, 0f, 0f };

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing
        if(cam == null) cam = FindObjectOfType<CameraControl>();

        foreach(Transform alpha in transform) { // Show or hide every child when needed
            alpha.gameObject.SetActive(IsEnd || VisibleOn[BarManager.inst.BarSize]);
        }

        transform.localPosition = new Vector3(XPosOn[BarManager.inst.BarSize], 0f, 0f); // Move the segment around

        if(myLabel == null) { // If the label doesn't exist, make one
            GameObject newbie = GameObject.Instantiate<GameObject>(labelPrefab);
            myLabel = newbie.GetComponent<LensLabel>();
            myLabel.target = transform;
            myLabel.transform.SetParent(cam.LabelParent);
            myLabel.transform.localScale = Vector3.one;
        }
        myLabel.gameObject.SetActive(IsEnd || VisibleOn[BarManager.inst.BarSize]); // Show or hide the label when needed
        if(affected == null || affected.Length == 0) { // If we don't know what heads we can affect, get 'em
            affected = GetComponentsInChildren<LightHead>(true);
        }
        if(liveAffected == null) { // Create or clear the liveAffected list
            liveAffected = new List<LightHead>();
        } else {
            liveAffected.Clear();
        }
        for(byte h = 0; h < affected.Length; h++) {
            if(affected[h].gameObject.activeInHierarchy) liveAffected.Add(affected[h]); // Only add to liveAffected if the head's active
        }
    }

    /// <summary>
    /// Get a list of LightHeads this BarSegment covers
    /// </summary>
    public List<LightHead> AffectedLights {
        get {
            return (liveAffected == null ? new List<LightHead>() : liveAffected);
        }
    }
}
