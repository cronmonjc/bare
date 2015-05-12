using UnityEngine;
using System.Collections;

public class BarSegment : MonoBehaviour {
    private static BarManager man;

    public bool IsEnd;
    public bool[] VisibleOn = new bool[] {false, false, false, false, true};
    public float[] XPosOn = new float[] { 0f, 0f, 0f, 0f, 0f };

    // Update is called once per frame
    void Update() {
        if(CameraControl.funcBeingTested != Function.NONE) return;
        if(man == null) {
            man = FindObjectOfType<BarManager>();
        }

        foreach(Transform alpha in transform) {
            alpha.gameObject.SetActive(IsEnd || VisibleOn[man.BarSize]);
        }

        transform.localPosition = new Vector3(XPosOn[man.BarSize], 0f, 0f);
    }
}
