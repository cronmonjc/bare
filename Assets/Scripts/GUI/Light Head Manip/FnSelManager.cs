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
            fn.GetComponent<Animator>().ResetTrigger("Chosen");
            fn.GetComponent<Animator>().SetTrigger("Normal");
            fn.gameObject.SetActive(false);

            string name = "";

            foreach(LightHead lh in FindObjectsOfType<LightHead>()) {
                if(!lh.CapableFunctions.Contains(fn.fn) || !lh.Selected || !lh.gameObject.activeInHierarchy) continue;

                if(lh.IsUsingFunction(fn.fn)) {
                    if(lh.lhd.style.isDualColor) {
                        if(lh.DualL.patterns.ContainsKey(fn.fn)) {
                            if(name == "") {
                                name = lh.DualL.patterns[fn.fn].name;
                            } else {
                                if(name != "<i>-- Multiple Values --</i>" && name != lh.DualL.patterns[fn.fn].name) {
                                    name = "<i>-- Multiple Values --</i>";
                                }
                            }
                        }
                        if(lh.DualR.patterns.ContainsKey(fn.fn)) {
                            if(name == "") {
                                name = lh.DualR.patterns[fn.fn].name;
                            } else {
                                if(name != "<i>-- Multiple Values --</i>" && name != lh.DualR.patterns[fn.fn].name) {
                                    name = "<i>-- Multiple Values --</i>";
                                }
                            }
                        }
                    } else {
                        if(lh.Single.patterns.ContainsKey(fn.fn)) {
                            if(name == "") {
                                name = lh.Single.patterns[fn.fn].name;
                            } else {
                                if(name != "<i>-- Multiple Values --</i>" && name != lh.Single.patterns[fn.fn].name) {
                                    name = "<i>-- Multiple Values --</i>";
                                }
                            }
                        }
                    }
                } else {
                    if(name != "") {
                        name = "<i>-- Multiple Values --</i>";
                    }
                }
            }

            if(name == "") {
                fn.DispPattern = "<i>Nothing</i>";
            } else {
                fn.DispPattern = name;
            }
            fn.gameObject.SetActive(true);
        }


    }

    public void RefreshLabels() {
        foreach(PatternFunc fn in funcs) {
            string name = "";

            foreach(LightHead lh in FindObjectsOfType<LightHead>()) {
                if(!lh.CapableFunctions.Contains(fn.fn) || !lh.Selected || !lh.gameObject.activeInHierarchy) continue;

                if(lh.IsUsingFunction(fn.fn)) {
                    if(lh.lhd.style.isDualColor) {
                        if(lh.DualL.patterns.ContainsKey(fn.fn)) {
                            if(name == "") {
                                name = lh.DualL.patterns[fn.fn].name;
                            } else {
                                if(name != "<i>-- Multiple Values --</i>" && name != lh.DualL.patterns[fn.fn].name) {
                                    name = "<i>-- Multiple Values --</i>";
                                }
                            }
                        }
                        if(lh.DualR.patterns.ContainsKey(fn.fn)) {
                            if(name == "") {
                                name = lh.DualR.patterns[fn.fn].name;
                            } else {
                                if(name != "<i>-- Multiple Values --</i>" && name != lh.DualR.patterns[fn.fn].name) {
                                    name = "<i>-- Multiple Values --</i>";
                                }
                            }
                        }
                    } else {
                        if(lh.Single.patterns.ContainsKey(fn.fn)) {
                            if(name == "") {
                                name = lh.Single.patterns[fn.fn].name;
                            } else {
                                if(name != "<i>-- Multiple Values --</i>" && name != lh.Single.patterns[fn.fn].name) {
                                    name = "<i>-- Multiple Values --</i>";
                                }
                            }
                        }
                    }
                } else {
                    if(name != "") {
                        name = "<i>-- Multiple Values --</i>";
                    }
                }
            }

            if(name == "") {
                fn.DispPattern = "<i>Nothing</i>";
            } else {
                fn.DispPattern = name;
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
