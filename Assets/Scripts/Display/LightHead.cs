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
    public SizeOptionControl soc;


    public bool basicPhaseA {
        get {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return false;
            }
            bool rtn = true;
            foreach(AdvFunction f in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                rtn &= !GetPhaseB(f, false);
            }
            return rtn;
        }
        set {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return;
            }
            foreach(AdvFunction f in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(Bit < 5, f));

                if(!patt.Contains("p" + (isRear ? "r" : "f") + "1"))
                    return;
                else
                    if(value)
                        patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + "1").DisableBit(Bit);
                    else
                        patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + "1").EnableBit(Bit);
            }
        }
    }
    public bool basicPhaseA2 {
        get {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return false;
            }
            bool rtn = true;
            foreach(AdvFunction f in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                rtn &= !GetPhaseB(f, true);
            }
            return rtn;
        }
        set {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return;
            }
            foreach(AdvFunction f in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(Bit < 5, f));

                if(!patt.Contains("p" + (isRear ? "r" : "f") + "2"))
                    return;
                else
                    if(value)
                        patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + "2").DisableBit(Bit);
                    else
                        patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + "2").EnableBit(Bit);
            }
        }
    }

    public bool basicPhaseB {
        get {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return false;
            }
            bool rtn = true;
            foreach(AdvFunction f in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                rtn &= GetPhaseB(f, false);
            }
            return rtn;
        }
        set {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return;
            }
            foreach(AdvFunction f in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(Bit < 5, f));

                if(!patt.Contains("p" + (isRear ? "r" : "f") + "1"))
                    return;
                else
                    if(value)
                        patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + "1").EnableBit(Bit);
                    else
                        patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + "1").DisableBit(Bit);
            }
        }
    }
    public bool basicPhaseB2 {
        get {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return false;
            }
            bool rtn = true;
            foreach(AdvFunction f in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                rtn &= GetPhaseB(f, true);
            }
            return rtn;
        }
        set {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return;
            }
            foreach(AdvFunction f in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(Bit < 5, f));

                if(!patt.Contains("p" + (isRear ? "r" : "f") + "2"))
                    return;
                else
                    if(value)
                        patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + "2").EnableBit(Bit);
                    else
                        patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + "2").DisableBit(Bit);
            }
        }
    }

    private bool m_knowsIsRear = false;
    private bool m_isRear = false;

    public bool isRear {
        get {
            if(!m_knowsIsRear) {
                m_isRear = transform.position.y < 0;
                m_knowsIsRear = true;
            }
            return m_isRear;
        }
    }

    public bool hasRealHead {
        get {
            return lhd.style != null && !lhd.optic.name.Equals("Block Off", System.StringComparison.CurrentCultureIgnoreCase);
        }
    }

    public Dictionary<AdvFunction, Pattern> pattDict1, pattDict2;

    public bool GetCanEnable(AdvFunction fn) {
        switch(fn) {
            case AdvFunction.LEVEL1:
            case AdvFunction.LEVEL2:
            case AdvFunction.LEVEL3:
            case AdvFunction.LEVEL4:
            case AdvFunction.LEVEL5:
            case AdvFunction.FTAKEDOWN:
            case AdvFunction.FALLEY:
            case AdvFunction.ICL:
                return lhd.funcs.Contains(BasicFunction.FLASHING);
            case AdvFunction.TAKEDOWN:
            case AdvFunction.ALLEY_LEFT:
            case AdvFunction.ALLEY_RIGHT:
                return lhd.funcs.Contains(BasicFunction.STEADY);
            case AdvFunction.TURN_LEFT:
            case AdvFunction.TURN_RIGHT:
            case AdvFunction.TAIL:
                return lhd.funcs.Contains(BasicFunction.STT);
            case AdvFunction.T13:
                return lhd.funcs.Contains(BasicFunction.CAL_STEADY);
            case AdvFunction.EMITTER:
                return lhd.funcs.Contains(BasicFunction.EMITTER);
            case AdvFunction.CRUISE:
                return lhd.funcs.Contains(BasicFunction.CRUISE);
            case AdvFunction.TRAFFIC_LEFT:
            case AdvFunction.TRAFFIC_RIGHT:
                return lhd.funcs.Contains(BasicFunction.TRAFFIC);
            case AdvFunction.DIM:
                return true;
            default:
                return false;
        }
    }

    public bool GetIsEnabled(AdvFunction fn, bool clr2 = false) {
        NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(Bit < 5, fn));

        if(!patt.Contains("e" + (isRear ? "r" : "f") + (clr2 ? "2" : "1")))
            return false;
        else
            return (patt.Get<NbtShort>("e" + (isRear ? "r" : "f") + (clr2 ? "2" : "1")).ShortValue & (0x1 << Bit)) > 0;
    }

    public bool GetPhaseB(AdvFunction fn, bool clr2 = false) {
        NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(Bit < 5, fn));

        if(!patt.Contains("p" + (isRear ? "r" : "f") + (clr2 ? "2" : "1")))
            return false;
        else
            return (patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + (clr2 ? "2" : "1")).ShortValue & (0x1 << Bit)) > 0;
    }

    public List<BasicFunction> CapableBasicFunctions {
        get {
            List<BasicFunction> rtn = new List<BasicFunction>(new BasicFunction[] { BasicFunction.FLASHING, BasicFunction.STEADY });

            switch(loc) {
                case Location.ALLEY:
                    break;
                case Location.FRONT:
                    rtn.Add(BasicFunction.CAL_STEADY);
                    if(!isSmall) rtn.Add(BasicFunction.EMITTER);
                    break;
                case Location.FRONT_CORNER:
                case Location.REAR_CORNER:
                    rtn.Add(BasicFunction.CRUISE);
                    break;
                case Location.REAR:
                    rtn.Add(BasicFunction.TRAFFIC);
                    break;
                case Location.FAR_REAR:
                    rtn.Add(BasicFunction.STT);
                    break;
                default:
                    return new List<BasicFunction>();
            }

            rtn.Add(BasicFunction.BLOCK_OFF);
            return rtn;
        }
    }

    public bool FarWire = false;

    [System.NonSerialized]
    public LightLabel myLabel;

    [System.NonSerialized]
    public Light[] myLights;

    public byte myBit = 255;

    [System.NonSerialized]
    public bool shouldBeTD;

    public byte Bit {
        get {
            return myBit;
        }
    }

    public bool Selected {
        get {
            if(cam == null) {
                cam = FindObjectOfType<CameraControl>();
            }
            return cam.OnlyCamSelectedHead.Contains(this);
        }
        set {
            if(value && !Selected) {
                cam.OnlyCamSelectedHead.Add(this);
            } else if(!value && Selected) {
                cam.OnlyCamSelectedHead.Remove(this);
            }
        }
    }

    void Awake() {
        lhd = new LightHeadDefinition();
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

        for(Transform t = transform; soc == null && t != null; t = t.parent) {
            soc = t.GetComponent<SizeOptionControl>();
        }

        if(isRear) { // Dummy if to get the head to recognize it's rearness.

        }
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
    //    if(!CapableAdvFunctions.Contains(f) || !hasRealHead) return false;
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

    public void PrefetchPatterns() {
        if(pattDict1 == null)
            pattDict1 = new Dictionary<AdvFunction, Pattern>();
        else
            pattDict1.Clear();
        if(pattDict2 == null)
            pattDict2 = new Dictionary<AdvFunction, Pattern>();
        else
            pattDict2.Clear();

        foreach(int i in new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 8192, 16384, 32768, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000 }) {
            pattDict1[(AdvFunction)i] = GetPattern((AdvFunction)i, false, true);
            pattDict2[(AdvFunction)i] = GetPattern((AdvFunction)i, true, true);
        }
    }

    public Pattern GetPattern(AdvFunction f, bool clr2 = false, bool forceFetch = false) {
        if(!forceFetch) {
            if(!clr2 && pattDict1 != null) return pattDict1.ContainsKey(f) ? pattDict1[f] : null;
            if(clr2 && pattDict2 != null) return pattDict2.ContainsKey(f) ? pattDict2[f] : null;
        }
        if(!hasRealHead) return null;
        if(LightDict.inst.steadyBurn.Contains(f)) {
            return LightDict.stdy;
        }
        NbtCompound patts = BarManager.inst.patts;

        string cmpdName = BarManager.GetFnString(transform, f);
        if(cmpdName == null) {
            Debug.LogWarning("lolnope - " + f.ToString() + " has no similar setting in the data bytes.");
            return null;
        }
        if(f == AdvFunction.TRAFFIC_LEFT || f == AdvFunction.TRAFFIC_RIGHT) {
            short patID = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("patt").Value;
            foreach(Pattern p in LightDict.inst.tdPatts) {
                if(p.id == patID) {
                    return p;
                }
            }
        } else {
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
        }
        return null;
    }

    public void AddBasicFunction(BasicFunction func, bool doDefault = true) {
        if(((func == BasicFunction.TRAFFIC && shouldBeTD) || CapableBasicFunctions.Contains(func)) && !lhd.funcs.Contains(func)) {
            lhd.funcs.Add(func);
            if(doDefault) RefreshBasicFuncDefault();
            TestSingleDual();
        }
        switch(func) {
            case BasicFunction.STT:
                NbtCompound taiCmpd = BarManager.inst.patts.Get<NbtCompound>((Bit < 5 ? "l" : "r") + "tai");
                taiCmpd.Get<NbtShort>("er1").EnableBit(Bit);
                taiCmpd.Get<NbtShort>("er2").EnableBit(Bit);
                break;
            case BasicFunction.CRUISE:
                NbtCompound cruCmpd = BarManager.inst.patts.Get<NbtCompound>("cru");
                cruCmpd.Get<NbtShort>("e" + (isRear ? "r" : "f") + "1").EnableBit(Bit);
                cruCmpd.Get<NbtShort>("e" + (isRear ? "r" : "f") + "2").EnableBit(Bit);
                break;
            default:
                break;
        }
        BarManager.moddedBar = true;
    }

    public void RemoveBasicFunction(BasicFunction func) {
        if(lhd.funcs.Contains(func)) {
            lhd.funcs.Remove(func);
            if(func == BasicFunction.TRAFFIC && shouldBeTD) {
                BarManager.inst.td = TDOption.NONE;
                foreach(LightHead alpha in BarManager.inst.allHeads) {
                    alpha.shouldBeTD = false;
                }
                BarManager.inst.StartCoroutine(BarManager.inst.RefreshBits());
            }
            RefreshBasicFuncDefault();
            TestSingleDual();
        }
        BarManager.moddedBar = true;
    }

    public void TestSingleDual() {
        useSingle = useDual = false;
        switch(lhd.funcs.Count) {
            case 1:
                useSingle = true;
                switch(lhd.funcs[0]) {
                    case BasicFunction.CAL_STEADY:
                    case BasicFunction.TRAFFIC:
                    case BasicFunction.FLASHING:
                        if(lhd.funcs[0] == BasicFunction.FLASHING) {
                            useDual = true;
                        }
                        return;
                    default:
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
                        useSingle = true;
                        useDual = (test == BasicFunction.FLASHING);
                    }
                    return;
                } else {
                    useDual = true;
                    if(lhd.funcs.Contains(BasicFunction.FLASHING) && lhd.funcs.Contains(BasicFunction.STEADY)) {
                        useSingle = true;
                    }
                }
                return;
            case 3:
                useDual = true;
                if(lhd.funcs.Contains(BasicFunction.CRUISE)) useSingle = true;
                return;
            default:
                return;
        }

    }

    public void RefreshBasicFuncDefault() {
        switch(lhd.funcs.Count) {
            case 1:
                switch(lhd.funcs[0]) {
                    case BasicFunction.BLOCK_OFF:
                        SetOptic("Block Off");
                        return;
                    case BasicFunction.EMITTER:
                        SetOptic("Emitter");
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
                        return;
                    default:
                        SetOptic("");
                        return;
                }
            case 2:
                if(lhd.funcs.Contains(BasicFunction.EMITTER) | lhd.funcs.Contains(BasicFunction.BLOCK_OFF)) {
                    lhd.funcs.RemoveAt(0);
                    RefreshBasicFuncDefault();
                } else if(lhd.funcs.Contains(BasicFunction.CRUISE)) {
                    BasicFunction test = BasicFunction.NULL;
                    if(lhd.funcs[0] == BasicFunction.CRUISE) {
                        test = lhd.funcs[1];
                    } else {
                        test = lhd.funcs[0];
                    }

                    if(test == BasicFunction.FLASHING || test == BasicFunction.STEADY) {
                        SetOptic("Lineum", test);
                        return;
                    } else {
                        SetOptic("");
                        return;
                    }
                } else {
                    if(lhd.funcs.Contains(BasicFunction.FLASHING) && lhd.funcs.Contains(BasicFunction.STEADY)) {
                        SetOptic(isSmall ? "Starburst" : "Lineum", BasicFunction.STEADY);
                    } else {
                        SetOptic("Dual " + (isSmall ? "Small " : "") + "Lineum", BasicFunction.STEADY);
                    }
                }
                return;
            case 3:
                if(lhd.funcs[2] == BasicFunction.EMITTER | lhd.funcs[2] == BasicFunction.BLOCK_OFF) {
                    lhd.funcs.RemoveRange(0, 2);
                    RefreshBasicFuncDefault();
                } else {
                    SetOptic("Dual " + (isSmall ? "Small " : "") + "Lineum", BasicFunction.STEADY);
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
        if(newOptic == null) {
            SetOptic("");
            try {
                cam.fs.Refresh();
            } catch(System.Exception) { }
        } else SetOptic(newOptic.name);
    }

    public void SetOptic(string newOptic, BasicFunction fn = BasicFunction.NULL, bool doDefault = true) {
        if(newOptic.Length > 0) {
            lhd.optic = LightDict.inst.FetchOptic(loc, newOptic);
            if(doDefault && lhd.optic != null) {
                if(lhd.optic.name == "Block Off") {
                    SetStyle("No Logo");
                } else {
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
            BarManager.moddedBar = true;
        } else {
            lhd.funcs.Clear();
            lhd.optic = null;
            SetStyle("");
            BarManager.moddedBar = true;
        }


    }

    public void SetStyle(string newStyle) {
        if(lhd.optic != null && newStyle.Length > 0) {
            lhd.style = lhd.optic.styles[newStyle];
        } else {
            lhd.style = null;
        }
        BarManager.inst.StartCoroutine(BarManager.inst.RefreshBits());
        BarManager.moddedBar = true;
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
        Gizmos.color = Color.white;
    }

    public string PartNumber {
        get {
            return lhd.optic.partNumber + lhd.style.partSuffix;
        }
    }
}
