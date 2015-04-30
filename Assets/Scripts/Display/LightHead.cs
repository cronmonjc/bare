using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightHead : MonoBehaviour {
    public LightBlock Single, DualL, DualR;

    public Location loc;

    private static CameraControl cam;
    private static SelBoxCollider sbc;

    private Color transparent;

    public bool isSmall;
    public LightHeadDefinition lhd;
    public bool leftSelect, rightSelect;

    public Dictionary<Function, Pattern> patterns;

    public List<Function> CapableFunctions {
        get { return LightDict.inst.capableFunctions[loc]; }
    }

    private LightLabel myLabel;

    public Light[] myLights;

    public bool Selected {
        get {
            return Single.Selected || DualL.Selected || DualR.Selected;
        }
        set {
            if(lhd.style == null || !lhd.style.isDualColor) {
                Single.Selected = value;
                DualL.Selected = false;
                DualR.Selected = false;
            } else if(lhd.style != null && lhd.style.isDualColor) {
                Single.Selected = false;
                DualL.Selected = value;
                DualR.Selected = value;
            }
        }
    }

    void Awake() {
        lhd = new LightHeadDefinition();

        patterns = new Dictionary<Function, Pattern>();
    }

    void Start() {
        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
        }

        GameObject go = GameObject.Instantiate(cam.LabelPrefab) as GameObject;
        myLabel = go.GetComponent<LightLabel>();
        myLabel.target = transform;
        myLabel.transform.SetParent(cam.LabelParent);

        myLights = GetComponentsInChildren<Light>(true);

        transparent = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        Single.gameObject.SetActive(true);
    }

    void Update() {
        if(CameraControl.funcBeingTested != Function.NONE) {
            if(myLabel.gameObject.activeInHierarchy) {
                myLabel.gameObject.SetActive(false);
            }
            return;
        }

        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
        }
        if(sbc == null) {
            sbc = cam.SelBox.GetComponent<SelBoxCollider>();
        }

        if(!myLabel.gameObject.activeInHierarchy) {
            myLabel.gameObject.SetActive(true);
        }
    }

    public bool IsUsingFunction(Function f) {
        return lhd.style != null && (lhd.style.isDualColor ? (DualL.patterns.ContainsKey(f) || DualR.patterns.ContainsKey(f)) : (Single.patterns.ContainsKey(f)));
    }

    public void CopyPatterns(LightHead dest) {
        dest.DualL.patterns.Clear();
        dest.DualR.patterns.Clear();
        dest.Single.patterns.Clear();

        foreach(Function f in CapableFunctions) {
            if(IsUsingFunction(f) && dest.CapableFunctions.Contains(f)) {
                if(DualL.patterns.ContainsKey(f)) {
                    dest.DualL.patterns[f] = DualL.patterns[f];
                }
                if(DualR.patterns.ContainsKey(f)) {
                    dest.DualR.patterns[f] = DualR.patterns[f];
                }
                if(Single.patterns.ContainsKey(f)) {
                    dest.Single.patterns[f] = Single.patterns[f];
                }
            }
        }
    }

    public void SetOptic(OpticNode newOptic) {
        if(newOptic == null) SetOptic("");
        else SetOptic(newOptic.name);
    }

    public void SetOptic(string newOptic, bool doDefault = true) {
        if(newOptic.Length > 0) {
            lhd.optic = LightDict.inst.FetchOptic(loc, newOptic);
            if(doDefault) {
                Function highFunction = Function.LEVEL1;
                foreach(Function f in CapableFunctions) {
                    if(!IsUsingFunction(f)) continue;
                    switch(f) {
                        case Function.ALLEY:
                        case Function.TAKEDOWN:
                            highFunction = f;
                            break;
                        case Function.TRAFFIC:
                            if(highFunction != Function.ALLEY || highFunction != Function.TAKEDOWN) {
                                highFunction = f;
                            }
                            break;
                        case Function.STT_AND_TAIL:
                            if(highFunction != Function.ALLEY || highFunction != Function.TAKEDOWN || highFunction != Function.TRAFFIC) {
                                highFunction = f;
                            }
                            break;
                        case Function.ICL:
                            if(highFunction != Function.ALLEY || highFunction != Function.TAKEDOWN || highFunction != Function.TRAFFIC || highFunction != Function.STT_AND_TAIL) {
                                highFunction = f;
                            }
                            break;
                        case Function.T13:
                            if(highFunction != Function.ALLEY || highFunction != Function.TAKEDOWN || highFunction != Function.TRAFFIC || highFunction != Function.STT_AND_TAIL || highFunction != Function.ICL) {
                                highFunction = f;
                            }
                            break;
                        default: break;
                    }
                }
                List<StyleNode> styles = new List<StyleNode>(lhd.optic.styles.Values);
                StyleNode styleToSet = null;
                switch(highFunction) {
                    case Function.T13:
                        foreach(StyleNode alpha in styles) {
                            if(alpha.partSuffix.Equals("r", System.StringComparison.CurrentCultureIgnoreCase) || alpha.partSuffix.Equals("rc", System.StringComparison.CurrentCultureIgnoreCase)) {
                                styleToSet = alpha;
                                break;
                            }
                        }
                        break;
                    case Function.TRAFFIC:
                        foreach(StyleNode alpha in styles) {
                            if(alpha.partSuffix.Equals("a", System.StringComparison.CurrentCultureIgnoreCase) || alpha.partSuffix.Equals("ar", System.StringComparison.CurrentCultureIgnoreCase)) {
                                styleToSet = alpha;
                                break;
                            }
                        }
                        break;
                    case Function.STT_AND_TAIL:
                        foreach(StyleNode alpha in styles) {
                            if(alpha.partSuffix.Equals("r", System.StringComparison.CurrentCultureIgnoreCase) || alpha.partSuffix.Equals("ar", System.StringComparison.CurrentCultureIgnoreCase)) {
                                styleToSet = alpha;
                                break;
                            }
                        }
                        break;
                    case Function.ALLEY:
                    case Function.TAKEDOWN:
                    case Function.ICL:
                        foreach(StyleNode alpha in styles) {
                            if(alpha.partSuffix.Equals("c", System.StringComparison.CurrentCultureIgnoreCase) || alpha.partSuffix.Equals("cc", System.StringComparison.CurrentCultureIgnoreCase)) {
                                styleToSet = alpha;
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
                SetStyle(styleToSet);
            } else {
                SetStyle(null);
            }
            if(new List<StyleNode>(lhd.optic.styles.Values)[0].isDualColor) {
                DualL.gameObject.SetActive(true);
                DualR.gameObject.SetActive(true);
                Single.gameObject.SetActive(false);
                Single.Selected = false;
                DualL.Selected = true;
                DualR.Selected = true;
            } else {
                DualL.gameObject.SetActive(false);
                DualR.gameObject.SetActive(false);
                Single.gameObject.SetActive(true);
                Single.Selected = true;
                DualL.Selected = false;
                DualR.Selected = false;
            }

        } else {
            lhd.optic = null;
            SetStyle(null);
            DualL.gameObject.SetActive(false);
            DualR.gameObject.SetActive(false);
            Single.gameObject.SetActive(true);
            Single.Selected = true;
            DualL.Selected = false;
            DualR.Selected = false;
        }


    }

    public void SetStyle(StyleNode newStyle) {
        if(newStyle == null) {
            lhd.style = null;
            Single.Color = transparent;
            DualL.Color = transparent;
            DualR.Color = transparent;
        } else {
            lhd.style = lhd.optic.styles[newStyle.name];
            Single.Color = lhd.style.color;
            DualL.Color = lhd.style.color;
            DualR.Color = lhd.style.color2;
        }
    }
}
