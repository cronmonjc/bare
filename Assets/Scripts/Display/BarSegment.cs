using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BarSegment : MonoBehaviour {
    private static CameraControl cam;

    public Lens lens;

    [System.NonSerialized]
    public LightHead[] affected;

    public GameObject labelPrefab;
    public LensLabel myLabel;

    public bool Visible {
        get { return IsEnd || VisibleOn[BarManager.inst.BarSize]; }
    }

    public string LensDescrip {
        get {
            if(lens == null) return "No Lens";
            else return "Clear Coated " + lens.name + (IsEnd ? " End" : " Center") + " Lens";
        }
    }

    public string LensPart {
        get {
            if(lens == null) return "";
            else return (IsEnd ? Lens.lgPrefix : Lens.smPrefix) + lens.partSuffix;
        }
    }

    public bool IsEnd;
    public bool[] VisibleOn = new bool[] { false, false, false, false, true };
    public float[] XPosOn = new float[] { 0f, 0f, 0f, 0f, 0f };

    // Update is called once per frame
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;
        if(cam == null) cam = FindObjectOfType<CameraControl>();

        foreach(Transform alpha in transform) {
            alpha.gameObject.SetActive(IsEnd || VisibleOn[BarManager.inst.BarSize]);
        }

        transform.localPosition = new Vector3(XPosOn[BarManager.inst.BarSize], 0f, 0f);

        if(myLabel == null) {
            GameObject newbie = GameObject.Instantiate<GameObject>(labelPrefab);
            myLabel = newbie.GetComponent<LensLabel>();
            myLabel.target = transform;
            myLabel.transform.SetParent(cam.LabelParent);
            myLabel.transform.localScale = Vector3.one;
        }
        myLabel.gameObject.SetActive(IsEnd || VisibleOn[BarManager.inst.BarSize]);
    }

    public List<LightHead> AffectedLights {
        get {
            if(affected == null || affected.Length == 0) {
                affected = GetComponentsInChildren<LightHead>(true);
            }
            List<LightHead> rtn = new List<LightHead>();
            foreach(LightHead head in affected) {
                if(head.gameObject.activeInHierarchy) rtn.Add(head);
            }
            return rtn;
        }
    }
}
