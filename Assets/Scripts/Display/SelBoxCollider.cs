using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelBoxCollider : MonoBehaviour {
    public List<LightHead> SelectedHead;
    public List<BarSegment> SelectedLens;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        SelectedHead = new List<LightHead>();
        SelectedLens = new List<BarSegment>();
    }

    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing
        RectTransform rt = (RectTransform)transform;
        BoxCollider c = GetComponent<BoxCollider>();

        Rect r = rt.rect;
        c.size = new Vector3(r.width, r.height, 0.3f);
        c.center = new Vector3(r.width * 0.5f, r.height * -0.5f, 0.1f);
    }

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