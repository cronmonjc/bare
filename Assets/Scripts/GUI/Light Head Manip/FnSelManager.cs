using UnityEngine;
using System.Collections;

public class FnSelManager : MonoBehaviour {
    public static FnSelManager inst;
    private PatternFunc[] funcs;
    private CameraControl cam;

    private BarManager bm;

    void Start() {
        inst = this;
        funcs = transform.GetComponentsInChildren<PatternFunc>(true);
        foreach(PatternFunc fn in funcs) {
            fn.fsl = this;
        }
        cam = FindObjectOfType<CameraControl>();
        bm = FindObjectOfType<BarManager>();
    }

    public void Refresh() {
        foreach(PatternFunc fn in funcs) {
            fn.GetComponent<Animator>().SetTrigger("Normal");
            fn.gameObject.SetActive(false);
        }

        foreach(LightHead lh in FindObjectsOfType<LightHead>()) {
            if(!lh.Selected || !lh.gameObject.activeInHierarchy) continue;

            foreach(Function f in lh.CapableFunctions) {
                string name = "Nothing";
                if(lh.IsUsingFunction(f)) {
                    if(lh.lhd.style.isDualColor) {
                        if(lh.DualL.patterns.ContainsKey(f)) {
                            if(name == "Nothing") {
                                name = lh.DualL.patterns[f].name;
                            } else {
                                if(name != "-- Multiple --" && name != lh.DualL.patterns[f].name) {
                                    name = "-- Multiple --";
                                }
                            }
                        }
                        if(lh.DualR.patterns.ContainsKey(f)) {
                            if(name == "Nothing") {
                                name = lh.DualR.patterns[f].name;
                            } else {
                                if(name != "-- Multiple --" && name != lh.DualR.patterns[f].name) {
                                    name = "-- Multiple --";
                                }
                            }
                        }
                    } else {
                        if(lh.Single.patterns.ContainsKey(f)) {
                            if(name == "Nothing") {
                                name = lh.Single.patterns[f].name;
                            } else {
                                if(name != "-- Multiple --" && name != lh.Single.patterns[f].name) {
                                    name = "-- Multiple --";
                                }
                            }
                        }
                    }
                }

                foreach(PatternFunc fn in funcs) {
                    if(fn.fn == f) {
                        fn.DispPattern = name;
                        fn.gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }

    public void OnSelect(Function f) {
        foreach(PatternFunc fn in funcs) {
            fn.GetComponent<Animator>().ResetTrigger(fn.fn == f ? "Normal" : "Chosen");
            fn.GetComponent<Animator>().SetTrigger(fn.fn == f ? "Chosen" : "Normal");
        }
    }
}
