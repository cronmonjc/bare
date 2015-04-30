using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelBoxCollider : MonoBehaviour {

    public List<LightBlock> Selected;

    void Start() {
        Selected = new List<LightBlock>();
    }

    void Update() {
        if(CameraControl.funcBeingTested != Function.NONE) return;
        RectTransform rt = (RectTransform)transform;
        BoxCollider c = GetComponent<BoxCollider>();

        Rect r = rt.rect;
        c.size = new Vector3(r.width, r.height, 0.3f);
        c.center = new Vector3(r.width * 0.5f, r.height * -0.5f, 0.1f);
    }

    void OnCollisionEnter(Collision coll) {
        LightBlock lh = coll.gameObject.GetComponent<LightBlock>();
        if(lh != null) {
            Selected.Add(lh);
        }
    }

    void OnCollisionExit(Collision coll) {
        LightBlock lh = coll.gameObject.GetComponent<LightBlock>();
        if(lh != null && Selected.Contains(lh)) {
            Selected.Remove(lh);
        }
    }

}
