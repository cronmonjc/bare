using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using fNbt;

/// <summary>
/// Massive class that handles individual light heads.  Handles everything from definition storage to processing.
/// </summary>
/// <remarks>
/// LightHead is a Component that exists on every light head on the bar.  While it manages
/// information regarding the physical definition of the head (optic and style) it will also
/// cache information regarding patterns and enables, and handles default optics and styles
/// when adding functions.
/// </remarks>
public class LightHead : MonoBehaviour {
    /// <summary>
    /// The location this LightHead is residing.  Set via the Unity Inspector.
    /// </summary>
    public Location loc;

    /// <summary>
    /// A reference to the CameraControl Component
    /// </summary>
    private static CameraControl cam;

    /// <summary>
    /// Is this a small head?  Set via the Unity Inspector.
    /// </summary>
    public bool isSmall;
    /// <summary>
    /// The object containing information on the light head itself
    /// </summary>
    public LightHeadDefinition lhd;

    /// <summary>
    /// Do the functions on this head allow for use of single-color heads?
    /// </summary>
    [System.NonSerialized]
    public bool useSingle;
    /// <summary>
    /// Do the functions on this head allow for use of dual-color heads?
    /// </summary>
    [System.NonSerialized]
    public bool useDual;

    /// <summary>
    /// The SizeOptionControl that owns this head, if any
    /// </summary>
    [System.NonSerialized]
    public SizeOptionControl soc;

    /// <summary>
    /// This head's personal cache variable on its path
    /// </summary>
    private string m_path = "";
    /// <summary>
    /// This head's path
    /// </summary>
    public string Path {
        get {
            if(m_path.Length == 0) m_path = transform.GetPath(); // We don't know where we're living, spend a little time to get that now.
            return m_path; // Heads don't move in the hierarchy at all - fetch once and we never need to fetch again.
        }
    }

    /// <summary>
    /// Is Color 1 on this head using Phase A?
    /// </summary>
    public bool basicPhaseA {
        get {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return false;
            }
            bool rtn = true;
            foreach(AdvFunction f in LightDict.flashingFuncs) {
                rtn &= !GetPhaseB(f, false);
            }
            return rtn;
        }
        set {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return;
            }
            foreach(AdvFunction f in LightDict.flashingFuncs) {
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
    /// <summary>
    /// Is Color 2 on this head using Phase A?
    /// </summary>
    public bool basicPhaseA2 {
        get {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return false;
            }
            bool rtn = true;
            foreach(AdvFunction f in LightDict.flashingFuncs) {
                rtn &= !GetPhaseB(f, true);
            }
            return rtn;
        }
        set {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return;
            }
            foreach(AdvFunction f in LightDict.flashingFuncs) {
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

    /// <summary>
    /// Is Color 1 on this head using Phase B?
    /// </summary>
    public bool basicPhaseB {
        get {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return false;
            }
            bool rtn = true;
            foreach(AdvFunction f in LightDict.flashingFuncs) {
                rtn &= GetPhaseB(f, false);
            }
            return rtn;
        }
        set {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return;
            }
            foreach(AdvFunction f in LightDict.flashingFuncs) {
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
    /// <summary>
    /// Is Color 2 on this head using Phase B?
    /// </summary>
    public bool basicPhaseB2 {
        get {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return false;
            }
            bool rtn = true;
            foreach(AdvFunction f in LightDict.flashingFuncs) {
                rtn &= GetPhaseB(f, true);
            }
            return rtn;
        }
        set {
            if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                return;
            }
            foreach(AdvFunction f in LightDict.flashingFuncs) {
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

    /// <summary>
    /// Does this head know if it's a rear head?  (Can't simply check if m_isRear is null, bools aren't nullable)
    /// </summary>
    private bool m_knowsIsRear = false;
    /// <summary>
    /// This head's personal cache variable for isRear.  Important for PDF export (can't use UnityEngine.Transform in another thread).
    /// </summary>
    private bool m_isRear = false;

    /// <summary>
    /// Is this head in the rear of the bar?
    /// </summary>
    public bool isRear {
        get {
            if(!m_knowsIsRear) {
                m_isRear = transform.position.y < 0;
                m_knowsIsRear = true;
            }
            return m_isRear;
        }
    }

    /// <summary>
    /// This head's personal cache variable for hasRealHead.
    /// </summary>
    private bool m_hasRealHead = false;

    /// <summary>
    /// Does this head have a real optic in it? (defined, not block off)
    /// </summary>
    public bool hasRealHead {
        get {
            return m_hasRealHead;
        }
    }

    public bool isFar {
        get {
            return (isRear) && (Bit == 1 || Bit == 10);
        }
    }

    /// <summary>
    /// This head's cache variable for patterns, to prevent having to fetch it multiple times over
    /// </summary>
    public Dictionary<AdvFunction, Pattern> pattDict1, pattDict2;

    /// <summary>
    /// Get whether this head can be enabled given its current functions for a specific Advanced Function
    /// </summary>
    /// <param name="fn">The Advanced Function to test</param>
    /// <returns>True if this light head can be enabled given its setup</returns>
    public bool GetCanEnable(AdvFunction fn) {
        switch(fn) {
            case AdvFunction.PRIO1:
            case AdvFunction.PRIO2:
            case AdvFunction.PRIO3:
            case AdvFunction.PRIO4:
            case AdvFunction.PRIO5:
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

    /// <summary>
    /// This head's personal cache Dictionary storing whether or not it's enabled for each Advanced Function
    /// </summary>
    private Dictionary<AdvFunction, byte> cachedEnables;

    /// <summary>
    /// Get whether or not this head is enabled for a certain Advanced Function
    /// </summary>
    /// <param name="fn">The Advanced Function to test for</param>
    /// <param name="clr2">Are we checking color 2?</param>
    /// <param name="forceRefreshCache">Force the function to get information from the pattern bytes</param>
    /// <returns>True if the head is enabled for that function</returns>
    public bool GetIsEnabled(AdvFunction fn, bool clr2 = false, bool forceRefreshCache = false) {
        if(forceRefreshCache || !cachedEnables.ContainsKey(fn)) {
            NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(Bit < 5, fn));

            string tag = "e" + (isRear ? "r" : "f");
            if(patt.Contains(tag + "1")) {
                cachedEnables[fn] = (byte)(((patt.Get<NbtShort>(tag + "1").ShortValue & (0x1 << Bit)) > 0) ? 1 : 0);
                cachedEnables[fn] |= (byte)(((patt.Get<NbtShort>(tag + "2").ShortValue & (0x1 << Bit)) > 0) ? 2 : 0);
            } else {
                cachedEnables[fn] = 0;
            }

        }

        return (cachedEnables[fn] & (clr2 ? 2 : 1)) > 0;
    }

    /// <summary>
    /// Get whether or not this head is using Phase B for a certain Flashing Advanced Function
    /// </summary>
    /// <param name="fn">The Advanced Function to test for</param>
    /// <param name="clr2">Are we checking color 2?</param>
    /// <returns>True if the head is using Phase B for that function</returns>
    public bool GetPhaseB(AdvFunction fn, bool clr2 = false) {
        NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(Bit < 5, fn));

        if(!patt.Contains("p" + (isRear ? "r" : "f") + (clr2 ? "2" : "1")))
            return false;
        else
            return (patt.Get<NbtShort>("p" + (isRear ? "r" : "f") + (clr2 ? "2" : "1")).ShortValue & (0x1 << Bit)) > 0;
    }

    /// <summary>
    /// A list of all of the Basic Functions this head is capable of
    /// </summary>
    public List<BasicFunction> CapableBasicFunctions {
        get {
            List<BasicFunction> rtn = new List<BasicFunction>(new BasicFunction[] { BasicFunction.FLASHING, BasicFunction.STEADY });

            switch(loc) {
                case Location.ALLEY:
                case Location.FRONT_CORNER:
                case Location.REAR_CORNER:
                    break;
                case Location.FRONT:
                    rtn.Add(BasicFunction.CAL_STEADY);
                    if(!isSmall) rtn.Add(BasicFunction.EMITTER);
                    break;
                case Location.REAR:
                    rtn.Add(BasicFunction.STT);
                    rtn.Add(BasicFunction.TRAFFIC);
                    break;
                default:
                    return new List<BasicFunction>();
            }

            rtn.Add(BasicFunction.CRUISE);
            //rtn.Add(BasicFunction.BLOCK_OFF);
            return rtn;
        }
    }

    /// <summary>
    /// Is this head using the far wire if it's in the center?
    /// </summary>
    public bool FarWire = false;

    /// <summary>
    /// A reference to this head's light label
    /// </summary>
    [System.NonSerialized]
    public LightLabel myLabel;

    /// <summary>
    /// This head's bit.  A bit of 255 means it has none assigned.
    /// </summary>
    public byte myBit = 255;
    /// <summary>
    /// This head's bit, which will stay for the entire frame at most (in theory).  A bit of 255 means it has none assigned.
    /// </summary>
    public byte cacheBit = 255;

    /// <summary>
    /// Should this head be a Traffic Director head?
    /// </summary>
    [System.NonSerialized]
    public bool shouldBeTD;

    /// <summary>
    /// This head's bit.  A bit of 255 means it has none assigned.
    /// </summary>
    public byte Bit {
        get {
            return cacheBit;
        }
    }

    /// <summary>
    /// Does this head know if it's selected this frame?  (Can't simply check if m_selected is null, bools aren't nullable)
    /// </summary>
    private bool m_knowsSelectedThisFrame = false;
    /// <summary>
    /// This head's personal cache variable for Selected.
    /// </summary>
    private bool m_selected = false;

    /// <summary>
    /// Is this head currently selected?
    /// </summary>
    public bool Selected {
        get {
            if(!m_knowsSelectedThisFrame) {
                m_knowsSelectedThisFrame = true;
                if(cam == null) {
                    cam = FindObjectOfType<CameraControl>();
                }
                m_selected = cam.SelectedHead.Contains(this);
            }
            return m_selected;
        }
        set {
            if(value && !Selected) {
                cam.SelectedHead.Add(this);
            } else if(!value && Selected) {
                cam.SelectedHead.Remove(this);
            }
            m_knowsSelectedThisFrame = true;
            m_selected = value;
        }
    }

    /// <summary>
    /// Awake is called once, immediately as the object is created (typically at load time)
    /// </summary>
    void Awake() {
        lhd = new LightHeadDefinition();

        cachedEnables = new Dictionary<AdvFunction, byte>();
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
        }

        myLabel = GameObject.Instantiate<GameObject>(cam.LabelPrefab).GetComponent<LightLabel>();
        myLabel.target = transform;
        myLabel.transform.SetParent(cam.LabelParent);
        myLabel.transform.localScale = Vector3.one;
        myLabel.DispError = false;

        for(Transform t = transform; soc == null && t != null; t = t.parent) { // Look for a SizeOptionControl somewhere in its heritage
            soc = t.GetComponent<SizeOptionControl>();
        }

        if(isRear) { // Dummy if to get the head to recognize it's rearness.

        }
    }

    /// <summary>
    /// EarlyUpdate is called once each frame, before all Updates.
    /// </summary>
    void EarlyUpdate() {
        cacheBit = myBit;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) {  // Quick & dirty optimization - if we're previewing a function, do nothing
            return;
        }

        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
        }

        if(!myLabel.gameObject.activeInHierarchy) {
            myLabel.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// LateUpdate is called once each frame, after all Updates.
    /// </summary>
    void LateUpdate() {
        m_knowsSelectedThisFrame = false;
    }

    /// <summary>
    /// Gets the head to fetch ALL of the Patterns assigned to it
    /// </summary>
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

    /// <summary>
    /// Gets the head to fetch the Patterns assigned to it for a specific Advanced Function
    /// </summary>
    /// <param name="which">The Advanced Function to fetch for</param>
    public void PrefetchPatterns(AdvFunction which) {
        if(pattDict1 == null)
            pattDict1 = new Dictionary<AdvFunction, Pattern>();
        if(pattDict2 == null)
            pattDict2 = new Dictionary<AdvFunction, Pattern>();

        pattDict1[which] = GetPattern(which, false, true);
        pattDict2[which] = GetPattern(which, true, true);
    }

    /// <summary>
    /// Get the Pattern this head is assigned for a specific Advanced Function
    /// </summary>
    /// <param name="f">The Advanced Function to fetch for</param>
    /// <param name="clr2">Are we fetching for color 2?</param>
    /// <param name="forceFetch">Force the function to get information from the pattern bytes</param>
    /// <returns>The Pattern this head is using</returns>
    public Pattern GetPattern(AdvFunction f, bool clr2 = false, bool forceFetch = false) {
        if(!forceFetch) { // We aren't forced to fetch
            if(!clr2 && pattDict1 != null) return pattDict1.ContainsKey(f) ? pattDict1[f] : null; // If color 1, return the value in pattDict1
            if(clr2 && pattDict2 != null) return pattDict2.ContainsKey(f) ? pattDict2[f] : null; // If color 2, return the value in pattDict2
        }
        if(!hasRealHead) return null; // If this head doesn't have a real head, it doesn't have a Pattern to return
        if(LightDict.inst.steadyBurn.Contains(f)) { // If this is a Steady Burn Advanced Function, return the Steady Burn pattern
            return LightDict.stdy;
        }
        NbtCompound patts = BarManager.inst.patts; // Get the pattern bytes NbtCompound

        string cmpdName = BarManager.GetFnString(transform, f);
        if(cmpdName == null) {
            Debug.LogWarning("lolnope - " + f.ToString() + " has no similar setting in the data bytes.");
            return null;
        }
        if(f == AdvFunction.TRAFFIC_LEFT || f == AdvFunction.TRAFFIC_RIGHT) { // Looking for traffic director Pattern
            short patID = patts.Get<NbtCompound>(cmpdName).Get<NbtShort>("patt").Value; // Get the Pattern ID
            foreach(Pattern p in LightDict.inst.tdPatts) { // Look through every Pattern, return the one whose ID matches
                if(p.id == patID) {
                    return p;
                }
            }
        } else { // Looking for a flashing Pattern
            NbtCompound patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat" + (clr2 ? "2" : "1")); // Get the pertinent function NbtCompound

            string tagname = isRear ? "r" : "f";
            string path = Path;

            if(path.Contains("C") || path.Contains("A")) {
                tagname = tagname + "cor";
            } else if(path.Contains("I")) {
                tagname = tagname + "inb";
            } else if(path.Contains("O")) {
                if(isFar)
                    tagname = tagname + "far";
                else
                    tagname = tagname + "oub";
            } else if(path.Contains("N") || path.Split('/')[2].EndsWith("F")) {
                tagname = tagname + "cen";
            }

            short patID = patCmpd.Get<NbtShort>(tagname).Value; // Get the Pattern ID
            foreach(Pattern p in LightDict.inst.flashPatts) { // Look through every Pattern, return the one whose ID matches
                if(p.id == patID) {
                    return p;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Adds a Basic Function to this head.  Will process defaults unless not desired.
    /// </summary>
    /// <param name="func">The Basic Function to add</param>
    /// <param name="doDefault">Are we applying defaults?</param>
    public void AddBasicFunction(BasicFunction func, bool doDefault = true) {
        if(((func == BasicFunction.TRAFFIC && shouldBeTD) || CapableBasicFunctions.Contains(func)) && !lhd.funcs.Contains(func)) {  // If it's capable of adding the function and it doesn't have it yet...
            lhd.funcs.Add(func); // Add the function
            TestSingleDual(); // Check for ability to take single/dual heads

            // Refresh bits before modifying programming bytes
            StartCoroutine(RefreshBitsThenEnableBytes(func));

            if(doDefault) RefreshBasicFuncDefault(); // Apply defaults if desired
        }

        BarManager.moddedBar = true;
    }

    /// <summary>
    /// Coroutine.  Refresh LightHead bits then enable pattern bytes.
    /// </summary>
    /// <param name="func">The function that should be applied.</param>
    public IEnumerator RefreshBitsThenEnableBytes(BasicFunction func) {
        // Get the Bits refreshed
        yield return BarManager.inst.StartCoroutine(BarManager.inst.RefreshBitsIEnum());

        if(func == BasicFunction.EMITTER)
            foreach(string alpha in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim", "traf" }) {
                NbtCompound cmpd = BarManager.inst.patts.Get<NbtCompound>(alpha);
                string tag = "e" + (isRear ? "r" : "f");

                if(cmpd.Contains(tag + "1")) {
                    cmpd.Get<NbtShort>(tag + "1").DisableBit(Bit);
                    cmpd.Get<NbtShort>(tag + "2").DisableBit(Bit);
                }
            }

        // Enable the bytes
        switch(func) { // Automatically enable heads for certain functions
            case BasicFunction.STT:
                NbtCompound taiCmpd = BarManager.inst.patts.Get<NbtCompound>((Bit < 6 ? "l" : "r") + "tai");
                taiCmpd.Get<NbtShort>("er1").EnableBit(Bit);
                break;
            case BasicFunction.CRUISE:
                NbtCompound cruCmpd = BarManager.inst.patts.Get<NbtCompound>("cru");
                cruCmpd.Get<NbtShort>("e" + (isRear ? "r" : "f") + "1").EnableBit(Bit);
                break;
            case BasicFunction.EMITTER:
                NbtCompound emiCmpd = BarManager.inst.patts.Get<NbtCompound>("emi");
                emiCmpd.Get<NbtShort>("ef1").EnableBit(Bit);
                break;
            case BasicFunction.CAL_STEADY:
                NbtCompound calCmpd = BarManager.inst.patts.Get<NbtCompound>("cal");
                calCmpd.Get<NbtShort>("ef1").EnableBit(Bit);
                break;
            case BasicFunction.STEADY:
                NbtCompound cmpd = null;
                switch(loc) {
                    case Location.ALLEY: // Alley
                        cmpd = BarManager.inst.patts.Get<NbtCompound>((Bit == 12 ? "l" : "r") + "all");
                        break;
                    case Location.FRONT: // Takedown / Work Light
                    case Location.FRONT_CORNER:
                    case Location.REAR:
                    case Location.REAR_CORNER:
                    case Location.FAR_REAR:
                        cmpd = BarManager.inst.patts.Get<NbtCompound>("td");
                        break;
                    default:
                        break;
                }
                if(cmpd != null) {
                    cmpd.Get<NbtShort>("e" + (isRear ? "r" : "f") + (lhd.style.isDualColor ? "2" : "1")).EnableBit(Bit);
                }

                if(!lhd.funcs.Contains(BasicFunction.FLASHING)) {
                    foreach(AdvFunction f in LightDict.flashingFuncs) {
                        NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(Bit < 5, f));

                        patt.Get<NbtShort>("e" + (isRear ? "r" : "f") + "1").DisableBit(Bit);
                        patt.Get<NbtShort>("e" + (isRear ? "r" : "f") + "2").DisableBit(Bit);
                    }
                }
                break;
            default:
                break;
        }

        yield return null;
    }

    /// <summary>
    /// Removes a Basic Function from this head.  Will always process defaults.
    /// </summary>
    /// <param name="func">The Basic Function to remove</param>
    public void RemoveBasicFunction(BasicFunction func) {
        if(lhd.funcs.Contains(func)) {
            lhd.funcs.Remove(func);
            if(func == BasicFunction.TRAFFIC && shouldBeTD) {
                BarManager.inst.td = TDOption.NONE;
                foreach(LightHead alpha in BarManager.inst.allHeads) {
                    alpha.shouldBeTD = false;
                }
            }
            TestSingleDual();
            RefreshBasicFuncDefault();
        }

        if(gameObject.activeInHierarchy)
            StartCoroutine(RefreshBitsThenDisableBytes(func));

        BarManager.moddedBar = true;
    }

    /// <summary>
    /// Coroutine.  Refresh LightHead bits then disable pattern bytes.
    /// </summary>
    /// <param name="func">The function that should be removed.</param>
    public IEnumerator RefreshBitsThenDisableBytes(BasicFunction func) {
        byte theBit = Bit;

        // Get the Bits refreshed
        yield return BarManager.inst.StartCoroutine(BarManager.inst.RefreshBitsIEnum());

        // Disable the bytes
        switch(func) { // Automatically disable heads for certain functions
            case BasicFunction.STT:
                NbtCompound taiCmpd = BarManager.inst.patts.Get<NbtCompound>((theBit < 5 ? "l" : "r") + "tai");
                taiCmpd.Get<NbtShort>("er1").DisableBit(theBit);
                taiCmpd.Get<NbtShort>("er2").DisableBit(theBit);
                break;
            case BasicFunction.CRUISE:
                NbtCompound cruCmpd = BarManager.inst.patts.Get<NbtCompound>("cru");
                cruCmpd.Get<NbtShort>("e" + (isRear ? "r" : "f") + "1").DisableBit(theBit);
                cruCmpd.Get<NbtShort>("e" + (isRear ? "r" : "f") + "2").DisableBit(theBit);
                break;
            case BasicFunction.EMITTER:
                NbtCompound emiCmpd = BarManager.inst.patts.Get<NbtCompound>("emi");
                emiCmpd.Get<NbtShort>("ef1").DisableBit(theBit);
                emiCmpd.Get<NbtShort>("ef2").DisableBit(theBit);
                break;
            case BasicFunction.CAL_STEADY:
                NbtCompound calCmpd = BarManager.inst.patts.Get<NbtCompound>("cal");
                calCmpd.Get<NbtShort>("ef1").DisableBit(theBit);
                calCmpd.Get<NbtShort>("ef2").DisableBit(theBit);
                break;
            case BasicFunction.STEADY:
                NbtCompound cmpd = null;
                switch(loc) {
                    case Location.ALLEY: // Alley
                        cmpd = BarManager.inst.patts.Get<NbtCompound>((theBit == 12 ? "l" : "r") + "all");
                        break;
                    case Location.FRONT: // Takedown / Work Light
                    case Location.FRONT_CORNER:
                    case Location.REAR:
                    case Location.REAR_CORNER:
                    case Location.FAR_REAR:
                        cmpd = BarManager.inst.patts.Get<NbtCompound>("td");
                        break;
                    default:
                        break;
                }
                if(cmpd != null) {
                    cmpd.Get<NbtShort>("e" + (isRear ? "r" : "f") + "1").DisableBit(theBit);
                    cmpd.Get<NbtShort>("e" + (isRear ? "r" : "f") + "2").DisableBit(theBit);
                }
                break;
            case BasicFunction.FLASHING:
                if(lhd.funcs.Contains(BasicFunction.STEADY)) {
                    foreach(AdvFunction f in LightDict.flashingFuncs) {
                        NbtCompound patt = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(theBit < 5, f));

                        patt.Get<NbtShort>("e" + (isRear ? "r" : "f") + "1").DisableBit(theBit);
                        patt.Get<NbtShort>("e" + (isRear ? "r" : "f") + "2").DisableBit(theBit);
                    }
                }
                break;
            default:
                break;
        }

        yield return null;
    }

    /// <summary>
    /// Refreshes the useSingle and useDual variables on this head to match the Basic Functions on it
    /// </summary>
    public void TestSingleDual() {
        // If we have more than one Basic Function, and one of them is Emitter or Block Off, remove all but the last (aka most recent) one and start over.
        if(lhd.funcs.Count > 1 && (lhd.funcs.Contains(BasicFunction.EMITTER) || lhd.funcs.Contains(BasicFunction.BLOCK_OFF))) {
            List<BasicFunction> funcsToRemove = new List<BasicFunction>(lhd.funcs);
            funcsToRemove.RemoveAt(funcsToRemove.Count - 1);
            for(byte i = 0; i < funcsToRemove.Count; i++) {
                RemoveBasicFunction(funcsToRemove[i]); // Process removal of basic function
            }

            TestSingleDual();
            return;
        }

        if((useSingle || useDual) && Bit == 255) return; // Do nothing if this bit's currently not set but already has single or dual compatibility
        useSingle = useDual = false;
        switch(lhd.funcs.Count) {
            case 0: // No functions: single never, dual never
                break;
            case 1: // One function: single always, dual only on Flashing
                useSingle = true;
                switch(lhd.funcs[0]) {
                    case BasicFunction.FLASHING:
                        useDual = true;
                        break;
                    default:
                        break;
                }
                break;
            case 2: // Two functions
                byte funcs = 0x0;
                foreach(BasicFunction fn in lhd.funcs) // Figure out which two we have
                    funcs |= (byte)fn;
                switch(funcs) {
                    case 0x3: // FLASHING | STEADY
                    case 0x11: // FLASHING | CRUISE
                    case 0x41: // FLASHING | TRAFFIC
                        useSingle = useDual = true; // Single and Dual okay
                        break;
                    default:
                        useDual = true; // Dual only for any other combo
                        break;
                }
                break;
            case 3: // Three functions
                useDual = true; // Dual always
                if(lhd.funcs.Contains(BasicFunction.CRUISE)) useSingle = true; // Single okay too if functions contains Cruise - FLASHING | CRUISE | {STEADY or TRAFFIC}
                break;
            default: // Four plus functions
                useDual = true;
                break;
        }
        useDual &= !(isRear && (Bit < 2 || Bit > 9));
    }

    /// <summary>
    /// Set the optic and style of this head to match the default defined by this head's Basic Functions
    /// </summary>
    public void RefreshBasicFuncDefault() {
        // By this time, the functions should be cleared up, since TestSingleDual is always called before this

        switch(lhd.funcs.Count) {
            case 0:
                SetOptic(""); // No functions selected, clear out the head
                return;
            case 1:
                switch(lhd.funcs[0]) { // Only one function, apply optic and style fitting the function
                    case BasicFunction.BLOCK_OFF:
                        SetOptic("dont use");				// tempr remove of block off
                        return;
                    case BasicFunction.EMITTER:
                        SetOptic("Emitter");
                        return;
                    case BasicFunction.STT:
                    case BasicFunction.STEADY:
                        if(isSmall) {
                            SetOptic("Starburst");
                        } else {
                            SetOptic("Lineum");
                        }
                        return;
                    case BasicFunction.CAL_STEADY:
                    case BasicFunction.TRAFFIC:
                    case BasicFunction.FLASHING:
                    case BasicFunction.CRUISE:
                        if(isSmall) {
                            SetOptic("Small Lineum");
                        } else {
                            SetOptic("Lineum");
                        }
                        return;
                    default:
                        SetOptic("");
                        return;
                }
            case 2: // Two functions
                byte funcs = 0x0;
                foreach(BasicFunction fn in lhd.funcs) // Figure out which functions we've got
                    funcs |= (byte)fn;
                switch(funcs) {
                    case 0x3: // FLASHING | STEADY
                        if(isSmall) {
                            SetOptic("Starburst");
                        } else {
                            SetOptic("Lineum");
                        }
                        break;
                    case 0x11: // FLASHING | CRUISE
                    case 0x41: // FLASHING | TRAFFIC
                        if(isSmall) {
                            SetOptic("Small Lineum");
                        } else {
                            SetOptic("Lineum");
                        }
                        break;
                    default: // Any other valid combination
                        if(useDual) {
                            if(isSmall) {
                                SetOptic("Dual Small Lineum");
                            } else {
                                SetOptic("Dual Lineum");
                            }
                        }
                        break;
                }
                return;
            default: // Three plus functions
                if(useDual) // If it can dual
                    SetOptic("Dual " + (isSmall ? "Small " : "") + "Lineum"); // Use dual.  Don't care what combo of functions it's using.
                return;
        }
    }

    /// <summary>
    /// Set this head's optic using an OpticNode
    /// </summary>
    /// <param name="newOptic">The OpticNode to have this head use</param>
    public void SetOptic(OpticNode newOptic) {
        if(newOptic == null) {
            SetOptic("");
            try {
                cam.fs.Refresh();
            } catch(System.Exception) { }
        } else SetOptic(newOptic.name);
    }

    /// <summary>
    /// Set this head's optic by name
    /// </summary>
    /// <param name="newOptic">The name of the optic to have this head use</param>
    /// <param name="doDefault">Should the default style be set too?</param>
    public void SetOptic(string newOptic, bool doDefault = true) {
        if(newOptic.Length > 0) {
            lhd.optic = LightDict.inst.FetchOptic(loc, newOptic);
            if(doDefault && lhd.optic != null) { // If we're setting the optic (rather than removing one) and we're applying a default style...
                if(lhd.optic.name == "dont use") {// temp remove block off
                    SetStyle("No Logo");
                } else {
                    List<StyleNode> styles = new List<StyleNode>(lhd.optic.styles.Values);

                    foreach(StyleNode alpha in new List<StyleNode>(styles)) {
                        if(!StyleSelect.IsRecommended(this, alpha)) {
                            styles.Remove(alpha);
                        }
                    }

                    if(styles.Count == 1) { // Only apply a default style when one style is recommended
                        SetStyle(styles[0]);
                    } else {
                        SetStyle("");
                    }

                    if(lhd.funcs.Contains(BasicFunction.STEADY) && lhd.optic.dual) { // We're applying a dual-color optic on a head with Steady Burn enabled, apply default programming
                        NbtCompound pat;
                        if(loc == Location.ALLEY)
                            pat = BarManager.inst.patts.Get<NbtCompound>((Bit == 12 ? "l" : "r") + "all");
                        else
                            pat = BarManager.inst.patts.Get<NbtCompound>("td");

                        pat.Get<NbtShort>("e" + (isRear ? "r" : "f") + "1").DisableBit(Bit);
                        pat.Get<NbtShort>("e" + (isRear ? "r" : "f") + "2").EnableBit(Bit);
                    }
                }
            } else {
                SetStyle("");
            }
            //if(lhd.optic == null || !lhd.optic.dual) {
            //    string shortName = "e" + (isRear ? "r" : "f") + "2";
            //    foreach(string patt in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim", "traf" }) {
            //        NbtCompound cmpd = BarManager.inst.patts.Get<NbtCompound>(patt);
            //        if(cmpd.Contains(shortName)) {
            //            cmpd.Get<NbtShort>(shortName).DisableBit(Bit);
            //        }
            //    }
            //}
            BarManager.moddedBar = true;
        } else {
            lhd.optic = null;
            SetStyle("");
            BarManager.moddedBar = true;
        }
    }

    /// <summary>
    /// Set the style of the head by name
    /// </summary>
    /// <param name="newStyle">Name of the style to set</param>
    public void SetStyle(string newStyle) {
        if(lhd.optic != null && newStyle.Length > 0) {
            lhd.style = lhd.optic.styles[newStyle];
        } else {
            lhd.style = null;
        }
        m_hasRealHead = (lhd.style != null && !lhd.optic.name.Equals("dont use", System.StringComparison.CurrentCultureIgnoreCase));

        if(lhd.style != null && lhd.style.isDualColor && lhd.funcs.Contains(BasicFunction.CAL_STEADY)) {
            NbtCompound calCmpd = BarManager.inst.patts.Get<NbtCompound>("cal");

            if(newStyle.Equals("Amber/Red", System.StringComparison.CurrentCultureIgnoreCase)) {
                calCmpd.Get<NbtShort>("ef1").DisableBit(Bit);
                calCmpd.Get<NbtShort>("ef2").EnableBit(Bit);
            } else {
                calCmpd.Get<NbtShort>("ef1").EnableBit(Bit);
                calCmpd.Get<NbtShort>("ef2").DisableBit(Bit);
            }
        }

        BarManager.inst.StartCoroutine(BarManager.inst.RefreshBitsIEnum());
        BarManager.moddedBar = true;
    }

    /// <summary>
    /// Set the style of the head by a StyleNode
    /// </summary>
    /// <param name="newStyle">The StyleNode to set this head to</param>
    public void SetStyle(StyleNode newStyle) {
        if(newStyle == null || lhd.optic == null) {
            SetStyle("");
        } else {
            SetStyle(newStyle.name);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// EDITOR ONLY.  Tells Unity how to render this GameObject in the Scene View.
    /// </summary>
    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "Head" + (isSmall ? "Sm" : "Lg") + ".png", true);
        if(shouldBeTD) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
        if(UnityEditor.EditorApplication.isPlaying) {
            try {
                Gizmos.color = Color.red;
                if(basicPhaseB) Gizmos.DrawSphere(transform.position + new Vector3(0, 0.25f, 0), 0.25f);
                if(basicPhaseB2) Gizmos.DrawSphere(transform.position + new Vector3(0, -0.25f, 0), 0.25f);
            } catch(System.Exception) {

            }
        }
        Gizmos.color = Color.white;
    }
#endif

    /// <summary>
    /// The part number of the optic on this head (including style suffix)
    /// </summary>
    public string PartNumber {
        get {
            return lhd.optic.partNumber + lhd.style.partSuffix;
        }
    }
}
