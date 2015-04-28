using UnityEngine;
using System.Collections;

public class FnSelManager : MonoBehaviour {
    private PatternFunc[] funcs;
    private CameraControl cam;

    void Start() {
        funcs = transform.GetComponentsInChildren<PatternFunc>(true);
        foreach(PatternFunc fn in funcs) {
            fn.fsl = this;
        }
        cam = FindObjectOfType<CameraControl>();
    }

    public void OnSelect(Function f) {

    }
}
