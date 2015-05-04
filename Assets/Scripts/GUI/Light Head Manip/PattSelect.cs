using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PattSelect : MonoBehaviour {
    public Function f = Function.NONE;
    public RectTransform menu;
    public GameObject prefab;
    private CameraControl cam;

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

        GameObject newbie = GameObject.Instantiate<GameObject>(prefab);
        newbie.transform.SetParent(menu, false);
        newbie.transform.localScale = Vector3.one;
        PattSelectElement newpse = newbie.GetComponent<PattSelectElement>();
        newpse.selID = -1;


        if(LightDict.inst.steadyBurn.Contains(f)) {
        }
    }

    public void Select(PattSelectElement pattSelectElement) {
        throw new System.NotImplementedException();
    }
}
