using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using fNbt;

public class LightHead : MonoBehaviour {
    public Location loc;

    private static CameraControl cam;
    private static SelBoxCollider sbc;

    public bool isSmall;
    public LightHeadDefinition lhd;

    [System.NonSerialized]
    public bool useSingle;
    [System.NonSerialized]
    public bool useDual;

    [System.NonSerialized]
    public bool basicPhaseB = false;
    [System.NonSerialized]
    public bool basicPhaseB2 = false;

    public Dictionary<AdvFunction, Pattern> patterns;

    public List<BasicFunction> CapableBasicFunctions {
        get {
            switch(loc) {
                case Location.ALLEY:
                    return new List<BasicFunction>(new BasicFunction[] { BasicFunction.FLASHING, BasicFunction.STEADY });
                case Location.FRONT:
                    if(isSmall)
                        return new List<BasicFunction>(new BasicFunction[] { BasicFunction.FLASHING, BasicFunction.STEADY, BasicFunction.CAL_STEADY });
                    else
                        return new List<BasicFunction>(new BasicFunction[] { BasicFunction.FLASHING, BasicFunction.STEADY, BasicFunction.EMITTER, BasicFunction.CAL_STEADY });
                case Location.FRONT_CORNER:
                case Location.REAR_CORNER:
                    return new List<BasicFunction>(new BasicFunction[] { BasicFunction.FLASHING, BasicFunction.STEADY, BasicFunction.CRUISE });
                case Location.REAR:
                    return new List<BasicFunction>(new BasicFunction[] { BasicFunction.FLASHING, BasicFunction.STEADY, BasicFunction.TRAFFIC });
                case Location.FAR_REAR:
                    return new List<BasicFunction>(new BasicFunction[] { BasicFunction.FLASHING, BasicFunction.STEADY, BasicFunction.STT });
                default:
                    return new List<BasicFunction>();
            }
        }
    }

    public bool[] FarWire = { false, false, false, false, false };

    [System.NonSerialized]
    public LightLabel myLabel;

    [System.NonSerialized]
    public Light[] myLights;

    public byte[] bits = new byte[5];
    private TDBitChanger tbc;

    [System.NonSerialized]
    public bool shouldBeTD;

    public byte Bit {
        get {
            if(tbc != null)
                return tbc.Bit;
            else
                return bits[BarManager.inst.BarSize];
        }
    }

    public bool Selected {
        get {
            if(cam == null) {
                cam = FindObjectOfType<CameraControl>();
            }
            return cam.OnlyCamSelected.Contains(this);
        }
        set {
            if(value && !Selected) {
                cam.OnlyCamSelected.Add(this);
            } else if(!value && Selected) {
                cam.OnlyCamSelected.Remove(this);
            }
        }
    }

    void Awake() {
        lhd = new LightHeadDefinition();

        patterns = new Dictionary<AdvFunction, Pattern>();
    }

    void Start() {
        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
        }

        myLabel = GameObject.Instantiate<GameObject>(cam.LabelPrefab).GetComponent<LightLabel>();
        myLabel.target = transform;
        myLabel.transform.SetParent(cam.LabelParent);
        myLabel.transform.localScale = Vector3.one;
        myLabel.DispError = false;

        myLights = GetComponentsInChildren<Light>(true);

        tbc = GetComponent<TDBitChanger>();
    }

    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) {
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

    //public bool IsUsingFunction(AdvFunction f) {
    //    if(!CapableAdvFunctions.Contains(f) || lhd.style == null) return false;
    //    NbtCompound patts = FindObjectOfType<BarManager>().patts;

    //    string cmpdName = BarManager.GetFnString(transform, f);
    //    if(cmpdName == null) {
    //        Debug.LogWarning("lolnope - " + f.ToString() + " has no similar setting in the data bytes.  Ask James.");
    //        return false;
    //    }
    //    NbtCompound func = patts.Get<NbtCompound>(cmpdName);

    //    short en = func.Get<NbtShort>("e" + (transform.position.z > 0 ? "f" : "r") + "1").ShortValue;

    //    if(lhd.style.isDualColor) {
    //        en = (short)(en | func.Get<NbtShort>("e" + (transform.position.z > 0 ? "f" : "r") + "2").ShortValue);
    //    }

    //    return ((en & (0x1 << Bit)) > 0);
    //}

    public Pattern GetPattern(AdvFunction f, bool clr2 = false) {
        if(lhd.style == null) return null;
        if(LightDict.inst.steadyBurn.Contains(f)) {
            return LightDict.stdy;
        }
        NbtCompound patts = BarManager.inst.patts;

        string cmpdName = BarManager.GetFnString(transform, f);
        if(cmpdName == null) {
            Debug.LogWarning("lolnope - " + f.ToString() + " has no similar setting in the data bytes.");
            return null;
        }
        NbtCompound patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat" + (clr2 ? "2" : "1"));

        string tagname = transform.position.y < 0 ? "r" : "f";
        string path = transform.GetPath();

        if(path.Contains("C") || path.Contains("A")) {
            tagname = tagname + "cor";
        } else if(path.Contains("I")) {
            tagname = tagname + "inb";
        } else if(path.Contains("O")) {
            if(loc == Location.FAR_REAR)
                tagname = tagname + "far";
            else
                tagname = tagname + "oub";
        } else if(path.Contains("N") || path.Split('/')[2].EndsWith("F")) {
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

    public void AddBasicFunction(BasicFunction func, bool doDefault = true) {
        if(((func == BasicFunction.TRAFFIC && shouldBeTD) || CapableBasicFunctions.Contains(func)) && !lhd.funcs.Contains(func)) {
            lhd.funcs.Add(func);
            if(doDefault) RefreshBasicFuncDefault();
        }
    }

    public void RemoveBasicFunction(BasicFunction func) {
        if(lhd.funcs.Contains(func)) {
            lhd.funcs.Remove(func);
            if(func == BasicFunction.TRAFFIC && shouldBeTD) {
                BarManager.inst.td = TDOption.NONE;
                foreach(LightHead alpha in BarManager.inst.allHeads) {
                    alpha.shouldBeTD = false;
                }
            }
            RefreshBasicFuncDefault();
        }
    }

    public void RefreshBasicFuncDefault() {
        useSingle = useDual = false;
        switch(lhd.funcs.Count) {
            case 1:
                useSingle = true;
                switch(lhd.funcs[0]) {
                    case BasicFunction.EMITTER:
                        SetOptic("Emitter");
                        useSingle = false;
                        return;
                    case BasicFunction.STT:
                    case BasicFunction.STEADY:
                        if(isSmall) {
                            SetOptic("Starburst", lhd.funcs[0]);
                        } else {
                            SetOptic("Lineum", lhd.funcs[0]);
                        }
                        return;
                    case BasicFunction.CAL_STEADY:
                    case BasicFunction.TRAFFIC:
                    case BasicFunction.FLASHING:
                        if(isSmall) {
                            SetOptic("Small Lineum", lhd.funcs[0]);
                        } else {
                            SetOptic("Lineum", lhd.funcs[0]);
                        }
                        if(lhd.funcs[0] == BasicFunction.FLASHING) {
                            useDual = true;
                        }
                        return;
                    default:
                        SetOptic("");
                        return;
                }
            case 2:
                if(lhd.funcs.Contains(BasicFunction.CRUISE)) {
                    BasicFunction test = BasicFunction.NULL;
                    if(lhd.funcs[0] == BasicFunction.CRUISE) {
                        test = lhd.funcs[1];
                    } else {
                        test = lhd.funcs[0];
                    }

                    if(test == BasicFunction.FLASHING || test == BasicFunction.STEADY) {
                        SetOptic("Lineum", test);
                        useSingle = true;
                        return;
                    } else {
                        SetOptic("");
                        return;
                    }
                } else if(lhd.funcs.Contains(BasicFunction.EMITTER)) {
                    lhd.funcs.RemoveAt(0);
                    RefreshBasicFuncDefault();
                } else {
                    useDual = true;
                    if(lhd.funcs.Contains(BasicFunction.FLASHING) && lhd.funcs.Contains(BasicFunction.STEADY)) {
                        useSingle = true;
                        SetOptic(isSmall ? "Starburst" : "Lineum", BasicFunction.STEADY);
                    } else {
                        SetOptic("Dual " + (isSmall ? "Small " : "") + "Lineum", BasicFunction.STEADY);
                    }
                }
                return;
            case 3:
                if(lhd.funcs[2] == BasicFunction.EMITTER) {
                    lhd.funcs.RemoveRange(0, 2);
                    RefreshBasicFuncDefault();
                } else {
                    SetOptic("Dual " + (isSmall ? "Small " : "") + "Lineum", BasicFunction.STEADY);
                    useDual = true;
                    if(lhd.funcs.Contains(BasicFunction.CRUISE)) useSingle = true;
                }
                return;
            case 4:
                lhd.funcs.RemoveRange(0, 3);
                RefreshBasicFuncDefault();
                return;
            default:
                SetOptic("");
                return;
        }
    }

    public void SetOptic(OpticNode newOptic) {
        if(newOptic == null) SetOptic("");
        else SetOptic(newOptic.name);
    }

    public void SetOptic(string newOptic, BasicFunction fn = BasicFunction.NULL, bool doDefault = true) {

        if(newOptic.Length > 0) {
            lhd.optic = LightDict.inst.FetchOptic(loc, newOptic);
            if(doDefault && lhd.optic != null) {
                List<StyleNode> styles = new List<StyleNode>(lhd.optic.styles.Values);

                foreach(StyleNode alpha in new List<StyleNode>(styles)) {
                    if(!StyleSelect.IsRecommended(this, alpha)) {
                        styles.Remove(alpha);
                    }
                }

                if(styles.Count == 1) {
                    SetStyle(styles[0]);
                } else {
                    SetStyle("");
                }

                //AdvFunction highFunction = AdvFunction.LEVEL1;
                //foreach(AdvFunction f in CapableFunctions) {
                //    if(!IsUsingFunction(f)) continue;
                //    switch(f) {
                //        case AdvFunction.ALLEY:
                //        case AdvFunction.TAKEDOWN:
                //            highFunction = f;
                //            break;
                //        case AdvFunction.TRAFFIC:
                //            if(highFunction != AdvFunction.ALLEY || highFunction != AdvFunction.TAKEDOWN) {
                //                highFunction = f;
                //            }
                //            break;
                //        case AdvFunction.STT_AND_TAIL:
                //            if(highFunction != AdvFunction.ALLEY || highFunction != AdvFunction.TAKEDOWN || highFunction != AdvFunction.TRAFFIC) {
                //                highFunction = f;
                //            }
                //            break;
                //        case AdvFunction.ICL:
                //            if(highFunction != AdvFunction.ALLEY || highFunction != AdvFunction.TAKEDOWN || highFunction != AdvFunction.TRAFFIC || highFunction != AdvFunction.STT_AND_TAIL) {
                //                highFunction = f;
                //            }
                //            break;
                //        case AdvFunction.T13:
                //            if(highFunction != AdvFunction.ALLEY || highFunction != AdvFunction.TAKEDOWN || highFunction != AdvFunction.TRAFFIC || highFunction != AdvFunction.STT_AND_TAIL || highFunction != AdvFunction.ICL) {
                //                highFunction = f;
                //            }
                //            break;
                //        default: break;
                //    }
                //}

                //switch(highFunction) {
                //    case AdvFunction.T13:
                //        foreach(StyleNode alpha in styles) {
                //            if(alpha.partSuffix.Equals("r", System.StringComparison.CurrentCultureIgnoreCase) || alpha.partSuffix.Equals("rc", System.StringComparison.CurrentCultureIgnoreCase)) {
                //                styleToSet = alpha;
                //                break;
                //            }
                //        }
                //        break;
                //    case AdvFunction.TRAFFIC:
                //        foreach(StyleNode alpha in styles) {
                //            if(alpha.partSuffix.Equals("a", System.StringComparison.CurrentCultureIgnoreCase) || alpha.partSuffix.Equals("ar", System.StringComparison.CurrentCultureIgnoreCase)) {
                //                styleToSet = alpha;
                //                break;
                //            }
                //        }
                //        break;
                //    case AdvFunction.STT_AND_TAIL:
                //        foreach(StyleNode alpha in styles) {
                //            if(alpha.partSuffix.Equals("r", System.StringComparison.CurrentCultureIgnoreCase) || alpha.partSuffix.Equals("ar", System.StringComparison.CurrentCultureIgnoreCase)) {
                //                styleToSet = alpha;
                //                break;
                //            }
                //        }
                //        break;
                //    case AdvFunction.ALLEY:
                //    case AdvFunction.TAKEDOWN:
                //    case AdvFunction.ICL:
                //        foreach(StyleNode alpha in styles) {
                //            if(alpha.partSuffix.Equals("c", System.StringComparison.CurrentCultureIgnoreCase) || alpha.partSuffix.Equals("cc", System.StringComparison.CurrentCultureIgnoreCase)) {
                //                styleToSet = alpha;
                //                break;
                //            }
                //        }
                //        break;
                //    default:
                //        break;
                //}
            } else {
                SetStyle("");
            }

        } else {
            lhd.optic = null;
            SetStyle("");
        }


    }

    public void SetStyle(string newStyle) {
        if(lhd.optic != null && newStyle.Length > 0) {
            lhd.style = lhd.optic.styles[newStyle];
        } else {
            lhd.style = null;
        }
    }

    public void SetStyle(StyleNode newStyle) {
        if(newStyle == null || lhd.optic == null) {
            SetStyle("");
        } else {
            SetStyle(newStyle.name);
        }
    }
    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "Head" + (isSmall ? "Sm" : "Lg") + ".png", true);
        if(shouldBeTD) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
        if(basicPhaseB) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.98f);
        }
        Gizmos.color = Color.white;
    }

    public string PartNumber {
        get {
            return lhd.optic.partNumber + lhd.style.partSuffix;
        }
    }
}
