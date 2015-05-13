using UnityEngine;
using System.Collections;

public class FnSelManager : MonoBehaviour {
    public static FnSelManager inst;
    public PattSelect ps;
    private PatternFunc[] funcs;

    void Awake() {
        inst = this;
    }

    public void Refresh() {
        LightBlock[] blocks = FindObjectsOfType<LightBlock>();
        ps.f = AdvFunction.NONE;
        ps.gameObject.SetActive(false);
        if(funcs == null) {
            funcs = transform.GetComponentsInChildren<PatternFunc>(true);
            foreach(PatternFunc fn in funcs) {
                fn.fsl = this;
            }
        }
        foreach(PatternFunc fn in funcs) {
            if(fn.gameObject.activeInHierarchy) {
                fn.GetComponent<Animator>().ResetTrigger("Chosen");
                fn.GetComponent<Animator>().SetTrigger("Normal");
            }
            fn.gameObject.SetActive(false);

            string name = "";

            foreach(LightBlock lb in blocks) {
                if(!lb.gameObject.activeInHierarchy || !lb.Selected) continue;
                LightHead lh = null;
                for(Transform t = lb.transform; lh == null && t != null; t = t.parent) {
                    lh = t.GetComponent<LightHead>();
                }
                if(lh == null) {
                    Debug.LogError("lolnope - " + lb.GetPath() + " can't find a LightHead.", lb);
                    ErrorText.inst.DispError(lb.GetPath() + " can't find a LightHead.");
                    continue;
                }
                if(lh.CapableFunctions.Contains(fn.fn)) {
                    fn.gameObject.SetActive(true);

                    if(lh.IsUsingFunction(fn.fn)) {
                        Pattern p = lh.GetPattern(fn.fn, false);
                        if(p != null) {
                            if(name == "") {
                                name = p.name;
                            } else {
                                if(name != "<i>-- Multiple Values --</i>" && name != p.name) {
                                    name = "<i>-- Multiple Values --</i>";
                                }
                            }
                        } else {
                            if(name != "") {
                                name = "<i>-- Multiple Values --</i>";
                            }
                        }
                        p = lh.GetPattern(fn.fn, true);
                        if(p != null) {
                            if(name == "") {
                                name = p.name;
                            } else {
                                if(name != "<i>-- Multiple Values --</i>" && name != p.name) {
                                    name = "<i>-- Multiple Values --</i>";
                                }
                            }
                        } else {
                            if(name != "") {
                                name = "<i>-- Multiple Values --</i>";
                            }
                        }
                    } else {
                        if(name != "") {
                            name = "<i>-- Multiple Values --</i>";
                        }
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

    public void RefreshLabels() {
        LightBlock[] blocks = FindObjectsOfType<LightBlock>();
        foreach(PatternFunc fn in funcs) {
            string name = "";

            foreach(LightBlock lb in blocks) {
                if(!lb.gameObject.activeInHierarchy || !lb.Selected) continue;
                LightHead lh = null;
                for(Transform t = lb.transform; lh == null && t != null; t = t.parent) {
                    lh = t.GetComponent<LightHead>();
                }
                if(lh == null) {
                    Debug.LogError("lolnope - " + lb.GetPath() + " can't find a LightHead.", lb);
                    ErrorText.inst.DispError(lb.GetPath() + " can't find a LightHead.");
                    continue;
                }

                if(lh.IsUsingFunction(fn.fn)) {
                    Pattern p = lh.GetPattern(fn.fn, false);
                    if(p != null) {
                        if(name == "") {
                            name = p.name;
                        } else {
                            if(name != "<i>-- Multiple Values --</i>" && name != p.name) {
                                name = "<i>-- Multiple Values --</i>";
                            }
                        }
                    } else {
                        if(name != "") {
                            name = "<i>-- Multiple Values --</i>";
                        }
                    }
                    p = lh.GetPattern(fn.fn, true);
                    if(p != null) {
                        if(name == "") {
                            name = p.name;
                        } else {
                            if(name != "<i>-- Multiple Values --</i>" && name != p.name) {
                                name = "<i>-- Multiple Values --</i>";
                            }
                        }
                    } else {
                        if(name != "") {
                            name = "<i>-- Multiple Values --</i>";
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

    public void OnSelect(AdvFunction f) {
        foreach(PatternFunc fn in funcs) {
            fn.GetComponent<Animator>().ResetTrigger(fn.fn == f ? "Normal" : "Chosen");
            fn.GetComponent<Animator>().SetTrigger(fn.fn == f ? "Chosen" : "Normal");
        }
        ps.f = f;
        ps.gameObject.SetActive(true);
        ps.Refresh();
    }
}
