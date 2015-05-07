﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using fNbt;

public class LightHead : MonoBehaviour {
    public LightBlock Single, DualL, DualR;

    public Location loc;

    private static CameraControl cam;
    private static SelBoxCollider sbc;

    private Color transparent;

    public bool isSmall;
    public LightHeadDefinition lhd;

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
        if(!CapableFunctions.Contains(f) || lhd.style == null) return false;
        NbtCompound patts = FindObjectOfType<BarManager>().patts;

        string cmpdName = BarManager.GetFnString(transform, f);
        if(cmpdName == null) {
            Debug.LogWarning("lolnope - " + f.ToString() + " has no similar setting in the data bytes.  Ask James.");
            return false;
        }
        NbtCompound func = patts.Get<NbtCompound>(cmpdName);

        short en = func.Get<NbtShort>("en" + (transform.position.z < 0 ? "r" : "f") + "1").ShortValue;

        if(lhd.style.isDualColor) {
            en = (short)(en | func.Get<NbtShort>("en" + (transform.position.z < 0 ? "r" : "f") + "2").ShortValue);
        }
        string path = transform.GetPath();
        byte bit = 16;
        if(transform.position.x < 0) {
            if(path.Contains("MidSection")) {
                // Center L, bit 5
                bit = 5;
            } else if(path.Contains("Corner")) {
                // Corner L, bit 0
                bit = 0;
            } else if(path.Contains("Outboard")) {
                if(loc == Location.FAR_REAR) {
                    // Outboard Far L, bit 1
                    bit = 1;
                } else {
                    // Outboard Near L, bit 2
                    bit = 2;
                }
            } else if(path.Contains("Alley")) {
                // Left Alley, bit 12
                bit = 12;
            }
        } else {
            if(path.Contains("MidSection")) {
                // Center R, bit 6
                bit = 6;
            } else if(path.Contains("Corner")) {
                // Corner L, bit 11
                bit = 11;
            } else if(path.Contains("Outboard")) {
                if(loc == Location.FAR_REAR) {
                    // Outboard Far R, bit 2
                    bit = 10;
                } else {
                    // Outboard Near R, bit 2
                    bit = 9;
                }
            } else if(path.Contains("Alley")) {
                // Right Alley, bit 12
                bit = 13;
            }
        }

        if(bit < 16) {
            return ((en & (0x1 << bit)) > 0);
        }

        return false;
    }

    public Pattern GetPattern(Function f, bool clr2 = false) {
        if(lhd.style == null) return null;
        if(LightDict.inst.steadyBurn.Contains(f)) {
            return LightDict.stdy;
        }
        NbtCompound patts = FindObjectOfType<BarManager>().patts;

        string cmpdName = BarManager.GetFnString(transform, f);
        if(cmpdName == null) {
            Debug.LogWarning("lolnope - " + f.ToString() + " has no similar setting in the data bytes.  Ask James.");
            ErrorText.inst.DispError(f.ToString() + " has no similar setting in the data bytes.  Ask James.");
            return null;
        }
        NbtCompound patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat" + (clr2 ? "2" : "1"));

        string tagname = transform.position.z < 0 ? "r" : "f";
        string path = transform.GetPath();

        if(path.Contains("Corner")) {
            tagname = tagname + "cor";
        } else if(path.Contains("Inboard")) {
            tagname = tagname + "inb";
        } else if(path.Contains("Outboard")) {
            if(loc == Location.FAR_REAR)
                tagname = tagname + "far";
            else
                tagname = tagname + "oub";
        } else if(path.Contains("MidSection")) {
            tagname = tagname + "cen";
        }

        short patID = patCmpd.Get<NbtShort>(tagname).Value;
        foreach(Pattern p in LightDict.inst.flashPatts) {
            if(p.id == patID) {
                return p;
            }
        }
        foreach(Pattern p in LightDict.inst.warnPatts) {
            if(p.id == patID) {
                return p;
            }
        }
        return null;

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
