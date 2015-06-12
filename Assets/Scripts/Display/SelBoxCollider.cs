using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelBoxCollider : MonoBehaviour {

    public List<LightHead> Selected;

    void Start() {
        Selected = new List<LightHead>();
    }

    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;
        RectTransform rt = (RectTransform)transform;
        BoxCollider c = GetComponent<BoxCollider>();

        Rect r = rt.rect;
        c.size = new Vector3(r.width, r.height, 0.3f);
        c.center = new Vector3(r.width * 0.5f, r.height * -0.5f, 0.1f);
    }

    void OnCollisionEnter(Collision coll) {
        LightHead lh = coll.gameObject.GetComponent<LightHead>();
        if(lh != null && !Selected.Contains(lh)) {
            Selected.Add(lh);
        }
    }

    void OnCollisionExit(Collision coll) {
        LightHead lh = coll.gameObject.GetComponent<LightHead>();
        if(lh != null && Selected.Contains(lh)) {
            Selected.Remove(lh);
        }
    }

}
