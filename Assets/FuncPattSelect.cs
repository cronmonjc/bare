using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FuncPattSelect : MonoBehaviour {
    public RectTransform menu;
    public GameObject optionPrefab;
    public CameraControl cam;
    public bool IsTD = false;
    public bool IsColor2 = false;

    public void Clear() {
        List<Transform> temp = new List<Transform>();
        foreach(Transform alpha in menu) {
            temp.Add(alpha);
        }
        foreach(Transform alpha in temp) {
            DestroyImmediate(alpha.gameObject);
        }
    }

    public void Refresh() {
        Clear();

        if(IsTD) {
        
        } else {
            foreach(FlashPatt alpha in LightDict.inst.flashPatts) {
                GameObject newbie = GameObject.Instantiate<GameObject>(optionPrefab);
                newbie.transform.SetParent(menu, false);
                newbie.transform.localScale = Vector3.one;
                FuncPatt patt = newbie.GetComponent<FuncPatt>();
                patt.fps = this;
                patt.patt = alpha;
            }
        }
    }

    public void SetSelection(Pattern p) {

    }
}
