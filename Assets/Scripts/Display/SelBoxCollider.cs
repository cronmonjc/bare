using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component that is attached to the selection box, recording the collision with light heads for selection
/// </summary>
public class SelBoxCollider : MonoBehaviour {
    /// <summary>
    /// A list of heads that have collided
    /// </summary>
    public List<LightHead> SelectedHead;
    /// <summary>
    /// A list of lenses that have collided
    /// </summary>
    public List<BarSegment> SelectedLens;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        SelectedHead = new List<LightHead>();
        SelectedLens = new List<BarSegment>();
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing
        RectTransform rt = (RectTransform)transform;
        BoxCollider c = GetComponent<BoxCollider>();

        Rect r = rt.rect;
        c.size = new Vector3(r.width, r.height, 0.3f);
        c.center = new Vector3(r.width * 0.5f, r.height * -0.5f, 0.1f);
    }

    /// <summary>
    /// Called when this GameObject begins colliding with another
    /// </summary>
    /// <param name="coll">The Collision object that contains information on how the two objects collided</param>
    void OnCollisionEnter(Collision coll) {
        LightHead lh = coll.gameObject.GetComponent<LightHead>();
        if(lh != null) {
            if(!SelectedHead.Contains(lh)) {
                SelectedHead.Add(lh);
            }
        } else {
            BarSegment seg = coll.gameObject.GetComponent<BarSegment>();
            if(seg != null) {
                if(!SelectedLens.Contains(seg)) {
                    SelectedLens.Add(seg);
                }
            }
        }
    }

    /// <summary>
    /// Called when this GameObject stops colliding with another
    /// </summary>
    /// <param name="coll">The Collision object that contains information on how the two objects collided</param>
    void OnCollisionExit(Collision coll) {
        LightHead lh = coll.gameObject.GetComponent<LightHead>();
        if(lh != null) {
            if(SelectedHead.Contains(lh)) {
                SelectedHead.Remove(lh);
            }
        } else {
            BarSegment seg = coll.gameObject.GetComponent<BarSegment>();
            if(seg != null) {
                if(SelectedLens.Contains(seg)) {
                    SelectedLens.Remove(seg);
                }
            }
        }
    }
}