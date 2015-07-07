using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing.Layout;
using System;
using System.Text;

public class BarManager : MonoBehaviour {
    public static string DirRoot;

    public AdvFunction funcBeingTested = AdvFunction.NONE;

    [NonSerialized]
    public bool savePDF = false;

    [Range(0, 4)]
    public int BarSize = 3;
    public TDOption td;
    public static bool _useCAN = false;
    public static bool useCAN {
        get { return _useCAN; }
        set {
            _useCAN = value;
            GameObject.Find("UI/Canvas/FuncAssign/FunctionSelection/Panel/InputSelects/Hardwire").SetActive(!value);
            GameObject.Find("UI/Canvas/FuncAssign/FunctionSelection/Panel/InputSelects/CAN").SetActive(value);

            if(value) {
                FnDragTarget.inputMap.Value = new int[] { 0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800,
                                                      0x2000, 0x4000, 0x8000, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000 };
            } else {
                FnDragTarget.inputMap.Value = new int[] { 1, 2, 3072, 4, 8, 512, 256, 32, 16, 128, 64, 4096, 0, 0, 0, 0, 0, 0, 0, 0 };
            }
        }
    }
    public static int cableType = 0, cableLength = 0;

    public NbtCompound patts;

    public static BarManager inst;
    public List<LightHead> allHeads;
    public List<BarSegment> allSegs;
    public static LightHead[] headNumber;

    public LightHead first;

    public InputField custName, orderNum, notes;

    public FileBrowser fb;
    [NonSerialized]
    public string barFilePath;

    public Slider SizeSlider;

    public IssueChecker[] issues;

    public static bool RefreshingBits = false;

    public GameObject quitDialog;
    public static bool moddedBar = false;
    public static bool quitAfterSave = false;
    public static bool forceQuit = false;

    [Serializable]
    public class ProgressStuff {
        public Slider progressBar;
        public GameObject progressGO;
        public Text progressText;

        public bool Shown {
            set { progressGO.SetActive(value); }
        }

        public float Progress {
            set { progressBar.value = value; }
        }

        public string Text {
            set { progressText.text = value; }
        }
    }

    public ProgressStuff progressStuff;

    public string BarModel {
        get {
            switch(BarSize) {
                case 0:
                    return "1300";
                case 1:
                    return "1400";
                case 2:
                    return "1500";
                case 3:
                    return "1550";
                case 4:
                    return "1600";
                default:
                    return "????";
            }
        }
    }

    void Awake() {
        CreatePatts();

        allHeads = new List<LightHead>();
        inst = this;

        string[] parts = Application.dataPath.Split('/', '\\');
        DirRoot = string.Join("/", parts, 0, parts.Length - 1) + "/";
    }

    private void CreatePatts() {
        patts = new NbtCompound("pats");

        foreach(string alpha in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim", "traf" }) {
            patts.Add(new NbtCompound(alpha));
        }

        patts.Get<NbtCompound>("traf").AddRange(new NbtShort[] { new NbtShort("er1", 0), new NbtShort("er2", 0) });

        foreach(string alpha in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim" }) {
            patts.Get<NbtCompound>(alpha).AddRange(new NbtShort[] { new NbtShort("ef1", 0), new NbtShort("ef2", 0), new NbtShort("er1", 0), new NbtShort("er2", 0) });
        }

        patts.Get<NbtCompound>("dim").Add(new NbtShort("dimp", 15));

        foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw" }) {
            patts.Get<NbtCompound>(alpha).AddRange(new NbtShort[] { new NbtShort("pf1", 0), new NbtShort("pf2", 0), new NbtShort("pr1", 0), new NbtShort("pr2", 0) });
        }

        patts.Get<NbtCompound>("traf").Add(new NbtShort("patt", 0));

        foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl" }) {
            patts.Get<NbtCompound>(alpha).Add(new NbtCompound("pat1", new NbtTag[] { new NbtShort("fcen", 0), new NbtShort("finb", 0), new NbtShort("foub", 0), new NbtShort("ffar", 0), new NbtShort("fcor", 0),
                                                                                     new NbtShort("rcen", 0), new NbtShort("rinb", 0), new NbtShort("roub", 0), new NbtShort("rfar", 0), new NbtShort("rcor", 0) }));
            patts.Get<NbtCompound>(alpha).Add(new NbtCompound("pat2", new NbtTag[] { new NbtShort("fcen", 0), new NbtShort("finb", 0), new NbtShort("foub", 0), new NbtShort("ffar", 0), new NbtShort("fcor", 0),
                                                                                     new NbtShort("rcen", 0), new NbtShort("rinb", 0), new NbtShort("roub", 0), new NbtShort("rfar", 0), new NbtShort("rcor", 0) }));
        }

        FnDragTarget.inputMap = new NbtIntArray("map", new int[] { 1, 2, 3072, 4, 8, 512, 256, 32, 16, 128, 64, 4096, 0, 0, 0, 0, 0, 0, 0, 0 });
        patts.Add(FnDragTarget.inputMap);
    }

    public void SetCAN(bool to) {
        useCAN = to;
    }

    void Start() {
        allHeads.AddRange(transform.GetComponentsInChildren<LightHead>(true));
        allSegs.AddRange(transform.GetComponentsInChildren<BarSegment>(true));
        StartCoroutine(RefreshBits());
        progressStuff.Shown = false;



        foreach(Lens opt in LightDict.inst.lenses) {
            if(opt.partSuffix == "C") {
                foreach(BarSegment seg in transform.GetComponentsInChildren<BarSegment>(true)) {
                    seg.lens = opt;
                }
            }
        }
    }

    public static string GetFnString(bool left, AdvFunction f) {
        switch(f) {
            case AdvFunction.ALLEY_LEFT:
                return "lall";
            case AdvFunction.ALLEY_RIGHT:
                return "rall";
            case AdvFunction.CRUISE:
                return "cru";
            case AdvFunction.DIM:
                return "dim";
            case AdvFunction.EMITTER:
                return "emi";
            case AdvFunction.FALLEY:
                return "afl";
            case AdvFunction.FTAKEDOWN:
                return "tdp";
            case AdvFunction.ICL:
                return "icl";
            case AdvFunction.PRIO1:
                return "l1";
            case AdvFunction.PRIO2:
                return "l2";
            case AdvFunction.PRIO3:
                return "l3";
            case AdvFunction.PRIO4:
                return "l4";
            case AdvFunction.PRIO5:
                return "l5";
            case AdvFunction.TURN_LEFT:
                return "ltai";
            case AdvFunction.TURN_RIGHT:
                return "rtai";
            case AdvFunction.TAIL:
                if(left) {
                    return "ltai";
                } else {
                    return "rtai";
                }
            case AdvFunction.TAKEDOWN:
                return "td";
            case AdvFunction.TRAFFIC_LEFT:
            case AdvFunction.TRAFFIC_RIGHT:
                return "traf";
            case AdvFunction.T13:
                return "cal";
            default:
                return null;
        }

    }

    public static string GetFnString(Transform t, AdvFunction f) {
        return GetFnString(t.position.x < 0, f);
    }

    public static string GetWire(LightHead lh) {
        return GetWireColor1(lh) + (lh.lhd.style.isDualColor ? (" C & " + GetWireColor2(lh) + " W") : "");
    }

    public static string GetWireColor1(LightHead lh) {
        byte bit = lh.Bit;
        string rtn = "";

        if(lh.transform.position.y < 0) {
            if(bit > 5) {
                rtn = "RP-";
            } else {
                rtn = "RD-";
            }

            switch(bit) {
                case 5:
                case 6:
                    rtn = rtn + "1";
                    break;
                case 4:
                case 7:
                    rtn = rtn + "2";
                    break;
                case 3:
                case 8:
                    rtn = rtn + "3";
                    break;
                case 2:
                case 9:
                    rtn = rtn + "4";
                    break;
                case 1:
                case 10:
                    rtn = rtn + "5";
                    break;
                case 0:
                case 11:
                    rtn = rtn + "6";
                    break;
                default:
                    rtn = rtn + "?";
                    break;
            }
        } else {
            if(bit > 5 && bit != 12) {
                rtn = "FP-";
            } else {
                rtn = "FD-";
            }

            switch(bit) {
                case 5:
                case 6:
                    rtn = rtn + (lh.FarWire ? "2" : "1");
                    break;
                case 4:
                case 7:
                    rtn = rtn + "3";
                    break;
                case 1:
                case 10:
                    rtn = rtn + "4";
                    break;
                case 0:
                case 11:
                    rtn = rtn + "5";
                    break;
                case 12:
                case 13:
                    rtn = rtn + "6";
                    break;
                default:
                    rtn = rtn + "?";
                    break;
            }
        }
        return rtn;
    }

    public static string GetWireColor2(LightHead lh) {
        byte bit = lh.Bit;
        string rtn = "";

        if(lh.transform.position.y < 0) {
            if(bit > 5) {
                rtn = "RP-";
            } else {
                rtn = "RD-";
            }

            switch(bit) {
                case 5:
                case 6:
                    rtn = rtn + "12";
                    break;
                case 4:
                case 7:
                    rtn = rtn + "11";
                    break;
                case 3:
                case 8:
                    rtn = rtn + "10";
                    break;
                case 2:
                case 9:
                    rtn = rtn + "9";
                    break;
                default:
                    rtn = rtn + "?";
                    break;
            }
        } else {
            if(bit > 5 && bit != 12) {
                rtn = "FP-";
            } else {
                rtn = "FD-";
            }

            switch(bit) {
                case 5:
                case 6:
                    rtn = rtn + (lh.FarWire ? "11" : "12");
                    break;
                case 4:
                case 7:
                    rtn = rtn + "10";
                    break;
                case 1:
                case 10:
                    rtn = rtn + "9";
                    break;
                case 0:
                case 11:
                    rtn = rtn + "8";
                    break;
                case 12:
                case 13:
                    rtn = rtn + "7";
                    break;
                default:
                    rtn = rtn + "?";
                    break;
            }
        }
        return rtn;
    }

    public void SetBarSize(float to) {
        SetBarSize(Mathf.RoundToInt(to), true);
    }

    public void SetBarSize(int to, bool sliding = false) {
        if(to < 5 && to > -1) {
            if(!sliding) {
                SizeSlider.GetComponent<SliderSnap>().lastWholeVal = to;
                SizeSlider.value = to;
            }

            td = TDOption.NONE;

            foreach(LightHead lh in allHeads) {
                if(lh.transform.position.y < 0) {
                    lh.shouldBeTD = false;
                    lh.RemoveBasicFunction(BasicFunction.TRAFFIC);
                }
                if(lh.myLabel != null)
                    lh.myLabel.Refresh();
            }

            BarSize = to;
            foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) {
                soc.ShowLong = true;
            }
            FindObjectOfType<CameraControl>().OnlyCamSelectedHead.Clear();
        }
        StartCoroutine(RefreshBits());
    }

    public void SetTDOption(int to) {
        SetTDOption((TDOption)to);
    }

    public void SetTDOption(TDOption to) {
        StartCoroutine(SetTDOptionCoroutine(to));
    }

    public IEnumerator SetTDOptionCoroutine(TDOption to) {
        RaycastHit[] hits;
        foreach(LightHead lh in allHeads) {
            if(lh.transform.position.y < 0) {
                lh.shouldBeTD = false;
                lh.RemoveBasicFunction(BasicFunction.TRAFFIC);
            }
        }
        td = to;

        switch(td) {
            case TDOption.NONE:
                foreach(LightHead lh in allHeads) {
                    if(lh.transform.position.y < 0) {
                        lh.RemoveBasicFunction(BasicFunction.TRAFFIC);
                    }
                }
                break;
            case TDOption.LG_SEVEN:
            case TDOption.LG_EIGHT:
                yield return new WaitForEndOfFrame();
                foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) {
                    if(soc.transform.position.y < 0) soc.ShowLong = true;
                }
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                hits = Physics.RaycastAll(new Vector3(-8, -1.25f), new Vector3(1, 0));

                foreach(RaycastHit hit in hits) {
                    LightHead lh = hit.transform.GetComponent<LightHead>();
                    lh.lhd.funcs.Clear();
                    lh.shouldBeTD = true;
                    lh.AddBasicFunction(BasicFunction.TRAFFIC);
                }
                break;
            case TDOption.LG_SIX:
                yield return new WaitForEndOfFrame();
                foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) {
                    if(soc.transform.position.y < 0) soc.ShowLong = (soc.transform.position.x != 0);
                }
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                hits = Physics.RaycastAll(new Vector3(-8f, -1.25f), new Vector3(1, 0));

                foreach(RaycastHit hit in hits) {
                    if(BarSize == 4 && hit.transform.GetPath().Contains("RO")) continue;
                    LightHead lh = hit.transform.GetComponent<LightHead>();
                    if(lh.isSmall) continue;
                    lh.lhd.funcs.Clear();
                    lh.shouldBeTD = true;
                    lh.AddBasicFunction(BasicFunction.TRAFFIC);
                }
                break;
            case TDOption.SM_EIGHT:
            case TDOption.SM_SIX:
                foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) {
                    if(soc.transform.position.y < 0) soc.ShowLong = false;
                }
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                if(td == TDOption.SM_SIX)
                    hits = Physics.RaycastAll(new Vector3(-2.4f, -1.25f), new Vector3(1, 0), 4.4f);
                else //SM_EIGHT
                    hits = Physics.RaycastAll(new Vector3(-3.2f, -1.25f), new Vector3(1, 0), 6.0f);

                foreach(RaycastHit hit in hits) {
                    LightHead lh = hit.transform.GetComponent<LightHead>();
                    lh.lhd.funcs.Clear();
                    lh.shouldBeTD = true;
                    lh.AddBasicFunction(BasicFunction.TRAFFIC);
                }
                break;
        }
        yield return StartCoroutine(RefreshBits());

        foreach(LightLabel ll in GameObject.Find("BarCanvas/Labels").GetComponentsInChildren<LightLabel>(true)) {
            ll.Refresh();
        }

        patts.Get<NbtCompound>("traf").Get<NbtShort>("er1").Value = (short)(td == TDOption.NONE ? 0 : 1020);
        patts.Get<NbtCompound>("traf").Get<NbtShort>("patt").Value = 7;

        yield return null;
    }

    public IEnumerator RefreshBits() {
        RefreshingBits = true;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        foreach(LightHead alpha in allHeads) {
            if(!alpha.gameObject.activeInHierarchy) continue;
            alpha.myBit = 255;
            alpha.FarWire = false;
            if(alpha.loc == Location.FRONT_CORNER || alpha.loc == Location.REAR_CORNER) {
                if(alpha.transform.position.x < 0) {
                    alpha.myBit = 0;
                } else {
                    alpha.myBit = 11;
                }
            } else if(alpha.loc == Location.ALLEY) {
                if(alpha.transform.position.x < 0) {
                    alpha.myBit = 12;
                } else {
                    alpha.myBit = 13;
                }
            } else {
                if(BarSize > 1 && alpha.transform.position.y < 0) continue;
                string[] path = alpha.transform.GetPath().Split('/');

                switch(path[2]) {
                    case "DE":
                        switch(path[3]) {
                            case "FO": alpha.myBit = 1; break;
                            case "FI": alpha.myBit = 4; break;
                            case "RO": alpha.myBit = (byte)(alpha.isSmall ? ((path[5] == "L" ? 2 : 3) - BarSize) : 2); break;
                            case "RI": alpha.myBit = (byte)(alpha.isSmall ? ((path[5] == "L" ? 4 : 5) - BarSize) : 4); break;
                            default: break;
                        }
                        break;
                    case "DF":
                        switch(path[3]) {
                            #region /Bar/DF/F
                            case "F":
                                if(BarSize == 2) {
                                    if(alpha.isSmall) {
                                        alpha.FarWire = path[5] == "L";
                                        alpha.myBit = (byte)(alpha.FarWire ? 5 : 6);
                                    } else {
                                        alpha.myBit = 5;
                                        alpha.FarWire = true;
                                    }
                                } else {
                                    alpha.myBit = 5;
                                    alpha.FarWire = true;
                                }
                                break;
                            #endregion
                            default: break;
                        }
                        break;
                    case "DN":
                        switch(path[3]) {
                            #region /Bar/DN/F
                            case "F":
                                alpha.myBit = (byte)(alpha.transform.position.x > 0 ? 6 : 5);
                                alpha.FarWire = (BarSize == 1);
                                break;
                            #endregion
                            case "R": alpha.myBit = (byte)(path[path.Length - 1] == "L" ? 5 : 6); break;
                            default: break;
                        }
                        break;
                    case "PN":
                        switch(path[3]) {
                            case "F": alpha.myBit = 6; break;
                            default: break;
                        }
                        break;
                    case "PF":
                        switch(path[3]) {
                            #region /Bar/PF/F
                            case "F":
                                if(BarSize == 2) {
                                    if(alpha.isSmall) {
                                        alpha.FarWire = path[5] == "R";
                                        alpha.myBit = (byte)(alpha.FarWire ? 6 : 5);
                                    } else {
                                        alpha.myBit = 6;
                                        alpha.FarWire = true;
                                    }
                                } else {
                                    alpha.myBit = 6;
                                    alpha.FarWire = true;
                                }
                                break;
                            #endregion
                            default: break;
                        }
                        break;
                    case "PE":
                        switch(path[3]) {
                            case "FO": alpha.myBit = 10; break;
                            case "FI": alpha.myBit = 7; break;
                            case "RO": alpha.myBit = (byte)(alpha.isSmall ? ((path[5] == "L" ? 8 : 9) + BarSize) : 9); break;
                            case "RI": alpha.myBit = (byte)(alpha.isSmall ? ((path[5] == "L" ? 6 : 7) + BarSize) : 7); break;
                            default: break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        if(BarSize > 1) {
            List<LightHead> heads = new List<LightHead>(10);
            List<RaycastHit> test = new List<RaycastHit>(Physics.RaycastAll(new Vector3(0, -1.25f), new Vector3(-1f, 0)));
            RaycastHit far; LightHead farHead;
            switch(td) {
                case TDOption.NONE:
                    far = test[0];
                    foreach(RaycastHit alpha in test) {
                        if(far.transform != alpha.transform && far.distance < alpha.distance)
                            far = alpha;
                    }

                    farHead = far.transform.GetComponent<LightHead>();
                    if(!farHead.isSmall) {
                        farHead.myBit = 2;
                        test.Remove(far);
                    }
                    break;
                default:
                    break;
            }
            byte bit = 5;
            RaycastHit center;
            if(Physics.Raycast(new Vector3(0, 0), new Vector3(0, -1), out center)) {
                LightHead alpha = center.transform.GetComponent<LightHead>();
                if(alpha.hasRealHead) {
                    alpha.myBit = 5;
                    bit = 4;
                } else {
                    alpha.myBit = 255;
                }
            }
            bool cont = true;
            while(cont && test.Count > 0) {
                for(int i = 0; i < test.Count; i++) {
                    LightHead testHead = test[i].transform.GetComponent<LightHead>();
                    if(!testHead.hasRealHead) {
                        testHead.myBit = 255;
                        test.RemoveAt(i);
                        break;
                    }
                    if(i == test.Count - 1) {
                        cont = false;
                        break;
                    }
                }
            }
            while(test.Count > 0) {
                center = test[0];
                for(int i = 1; i < test.Count; i++) {
                    if(test[i].point.x > center.point.x) {
                        center = test[i];
                    }
                }
                test.Remove(center);
                heads.Add(center.transform.GetComponent<LightHead>());
            }
            for(int i = 0; i < heads.Count; i++) {
                if(!heads[i].hasRealHead) {
                    heads[i].myBit = 255;
                    continue;
                }
                if(heads[i].shouldBeTD) {
                    heads[i].myBit = bit--;
                } else if(bit == 1) {
                    heads[i].myBit = 1;
                } else {
                    if(td == TDOption.SM_SIX) {
                        heads[i].myBit = bit;
                    } else if(heads.Count - i > bit) {
                        if(heads[i].isSmall ^ heads[i + 1].isSmall) {
                            heads[i].myBit = bit--;
                        } else {
                            heads[i].myBit = bit;
                            heads[++i].myBit = bit--;
                        }
                    } else {
                        heads[i].myBit = bit--;
                    }
                }
            }
            if(bit == 1) {
                test = new List<RaycastHit>(Physics.RaycastAll(new Vector3(heads[heads.Count - 1].transform.position.x, -1.25f), new Vector3(-1f, 0)));
                if(test.Count > 0) {
                    foreach(RaycastHit hit in test) {
                        hit.transform.GetComponent<LightHead>().myBit = 1;
                    }
                }
            }

            heads.Clear();
            test.Clear();
            test.AddRange(Physics.RaycastAll(new Vector3(0, -1.25f), new Vector3(1f, 0)));
            switch(td) {
                case TDOption.NONE:
                    far = test[0];
                    foreach(RaycastHit alpha in test) {
                        if(far.transform != alpha.transform && far.distance < alpha.distance)
                            far = alpha;
                    }

                    farHead = far.transform.GetComponent<LightHead>();
                    if(!farHead.isSmall) {
                        farHead.myBit = 9;
                        test.Remove(far);
                    }
                    break;
                default:
                    break;
            }
            bit = 6;
            if(Physics.Raycast(new Vector3(0, 0), new Vector3(0, -1))) {
                bit = 7;
            }
            cont = true;
            while(cont && test.Count > 0) {
                for(int i = 0; i < test.Count; i++) {
                    LightHead testHead = test[i].transform.GetComponent<LightHead>();
                    if(!testHead.hasRealHead) {
                        testHead.myBit = 255;
                        test.RemoveAt(i);
                        break;
                    }
                    if(i == test.Count - 1) {
                        cont = false;
                        break;
                    }
                }
            }
            while(test.Count > 0) {
                center = test[0];
                for(int i = 1; i < test.Count; i++) {
                    if(test[i].point.x < center.point.x) {
                        center = test[i];
                    }
                }
                test.Remove(center);
                heads.Add(center.transform.GetComponent<LightHead>());
            }
            for(int i = 0; i < heads.Count; i++) {
                if(!heads[i].hasRealHead) {
                    heads[i].myBit = 255;
                    continue;
                }
                if(heads[i].shouldBeTD) {
                    heads[i].myBit = bit++;
                } else if(bit == 10) {
                    heads[i].myBit = 10;
                } else {
                    if(td == TDOption.SM_SIX) {
                        heads[i].myBit = bit;
                    } else if(heads.Count - i > (11 - bit)) {
                        if(heads[i].isSmall ^ heads[i + 1].isSmall) {
                            heads[i].myBit = bit++;
                        } else {
                            heads[i].myBit = bit;
                            heads[++i].myBit = bit++;
                        }
                    } else {
                        heads[i].myBit = bit++;
                    }
                }
            }
            if(bit == 10) {
                test = new List<RaycastHit>(Physics.RaycastAll(new Vector3(heads[heads.Count - 1].transform.position.x, -1.25f), new Vector3(1f, 0)));
                if(test.Count > 0) {
                    foreach(RaycastHit hit in test) {
                        hit.transform.GetComponent<LightHead>().myBit = 10;
                    }
                }
            }
            RefreshingBits = false;
        }




        yield return StartCoroutine(RefreshAllLabels());
        yield return null;
    }

    public void Save(string filename) {
        try {
            NbtCompound root = new NbtCompound("root");

            NbtCompound opts = new NbtCompound("opts");
            opts.Add(new NbtByte("size", (byte)BarSize));
            opts.Add(new NbtByte("tdop", (byte)td));
            opts.Add(new NbtByte("can", (byte)(useCAN ? 1 : 0)));
            opts.Add(new NbtByte("cabt", (byte)cableType));
            opts.Add(new NbtByte("cabl", (byte)cableLength));
            root.Add(opts);

            NbtCompound order = new NbtCompound("ordr");
            order.Add(new NbtString("name", custName.text));
            order.Add(new NbtString("num", orderNum.text));
            order.Add(new NbtString("note", notes.text));
            root.Add(order);

            NbtList lightList = new NbtList("lite");
            foreach(LightHead lh in allHeads) {
                if(!lh.gameObject.activeInHierarchy) continue;
                NbtCompound lightCmpd = new NbtCompound();
                lightCmpd.Add(new NbtString("path", lh.transform.GetPath()));
                if(lh.lhd.style != null) {
                    lightCmpd.Add(new NbtString("optc", lh.lhd.optic.partNumber));
                    lightCmpd.Add(new NbtString("styl", lh.lhd.style.name));
                }

                byte fn = 0;
                foreach(BasicFunction bfn in lh.lhd.funcs) {
                    fn |= (byte)bfn;
                }
                lightCmpd.Add(new NbtByte("func", fn));

                lightList.Add(lightCmpd);
            }
            root.Add(lightList);

            root.Add(patts.Clone());

            NbtList socList = new NbtList("soc");
            foreach(SizeOptionControl soc in transform.GetComponentsInChildren<SizeOptionControl>(true)) {
                NbtCompound socCmpd = new NbtCompound();
                socCmpd.Add(new NbtString("path", soc.transform.GetPath()));
                socCmpd.Add(new NbtByte("isLg", soc.ShowLong ? (byte)1 : (byte)0));
                socList.Add(socCmpd);
            }
            root.Add(socList);

            NbtList lensList = new NbtList("lens");
            foreach(BarSegment seg in allSegs) {
                NbtCompound segCmpd = new NbtCompound();
                segCmpd.Add(new NbtString("path", seg.transform.GetPath()));
                segCmpd.Add(new NbtString("part", seg.lens.partSuffix));
                lensList.Add(segCmpd);
            }
            root.Add(lensList);

            NbtFile file = new NbtFile(root);
            file.SaveToFile(filename + (!filename.EndsWith(".bar.nbt") ? ".bar.nbt" : ""), NbtCompression.None);

            moddedBar = false;

            if(quitAfterSave) { Application.Quit(); }

            if(savePDF) { StartCoroutine(SavePDF(filename)); }
        } catch(Exception ex) {
            ErrorText.inst.DispError("Problem saving: " + ex.Message);
            Debug.LogException(ex);
        }
    }

    public void Open(string filename) {
        StartCoroutine(OpenIEnum(filename));
    }

    public IEnumerator OpenIEnum(string filename) {
        Clear();

        NbtFile file = new NbtFile(filename);

        NbtCompound root = file.RootTag;

        NbtCompound opts = root.Get<NbtCompound>("opts");
        SetBarSize(opts["size"].IntValue, false);
        SetTDOption((TDOption)opts["tdop"].ByteValue);
        useCAN = opts["can"].ByteValue == 1;
        cableType = opts["cabt"].IntValue;
        cableLength = opts["cabl"].IntValue;

        yield return StartCoroutine(RefreshBits());

        NbtCompound order = root.Get<NbtCompound>("ordr");
        custName.text = order["name"].StringValue;
        orderNum.text = order["num"].StringValue;
        notes.text = order["note"].StringValue;

        NbtList lightList = (NbtList)root["lite"];
        NbtList socList = (NbtList)root["soc"];
        NbtList lensList = (NbtList)root["lens"];
        Dictionary<string, LightHead> lights = new Dictionary<string, LightHead>();
        Dictionary<string, SizeOptionControl> socs = new Dictionary<string, SizeOptionControl>();

        foreach(LightHead lh in allHeads) {
            lights[lh.transform.GetPath()] = lh;
        }
        foreach(SizeOptionControl soc in transform.GetComponentsInChildren<SizeOptionControl>(true)) {
            socs[soc.transform.GetPath()] = soc;
        }
        List<NbtTag> stts = new List<NbtTag>();
        foreach(NbtTag alpha in lightList) {
            NbtCompound lightCmpd = alpha as NbtCompound;
            LightHead lh = lights[lightCmpd["path"].StringValue];

            byte fn = lightCmpd["func"].ByteValue;
            lh.lhd.funcs.Clear();
            foreach(BasicFunction bfn in lh.CapableBasicFunctions) {
                if(((byte)bfn & fn) != 0) {
                    lh.lhd.funcs.Add(bfn);
                }
            }

            if(lh.lhd.funcs.Contains(BasicFunction.STT)) {
                stts.Add(alpha);
                continue;
            }

            if(lightCmpd.Contains("optc")) {
                LocationNode ln = LightDict.inst.FetchLocation(lh.loc);
                string partNum = lightCmpd["optc"].StringValue;

                foreach(OpticNode on in ln.optics.Values) {
                    if(on.partNumber == partNum) {
                        lh.SetOptic(on.name, BasicFunction.NULL, false);
                        lh.SetStyle(lightCmpd["styl"].StringValue);
                        break;
                    }
                }
            }

            lh.TestSingleDual();
        }

        patts = root.Get<NbtCompound>("pats");
        FnDragTarget.inputMap = patts.Get<NbtIntArray>("map");

        foreach(NbtTag alpha in socList) {
            NbtCompound socCmpd = alpha as NbtCompound;
            SizeOptionControl soc = socs[socCmpd["path"].StringValue];
            soc.ShowLong = (socCmpd["isLg"].ByteValue == 1);
        }

        yield return StartCoroutine(RefreshBits());

        if(stts.Count > 0) {
            foreach(NbtTag alpha in stts) {
                NbtCompound lightCmpd = alpha as NbtCompound;
                LightHead lh = lights[lightCmpd["path"].StringValue];

                if(lightCmpd.Contains("optc")) {
                    LocationNode ln = LightDict.inst.FetchLocation(lh.loc);
                    string partNum = lightCmpd["optc"].StringValue;

                    foreach(OpticNode on in ln.optics.Values) {
                        if(on.partNumber == partNum) {
                            lh.SetOptic(on.name, BasicFunction.NULL, false);
                            lh.SetStyle(lightCmpd["styl"].StringValue);
                            break;
                        }
                    }
                }

                lh.TestSingleDual();
            }

            yield return StartCoroutine(RefreshBits());
        }

        foreach(NbtTag alpha in lensList) {
            NbtCompound lensCmpd = alpha as NbtCompound;
            foreach(BarSegment seg in allSegs) {
                if(seg.transform.GetPath() == lensCmpd["path"].StringValue) {
                    foreach(Lens opt in LightDict.inst.lenses) {
                        if(opt.partSuffix == lensCmpd["part"].StringValue) {
                            seg.lens = opt;
                            break;
                        }
                    }
                    break;
                }
            }
        }

        FindObjectOfType<CameraControl>().RefreshOnSelect.Invoke();
        moddedBar = false;
    }

    public void StartPDF() {
        savePDF = true;
        barFilePath = fb.currFile;

        Directory.CreateDirectory(DirRoot + "Lightbar Drawings");
        fb.currFile = "";
        fb.Navigate(DirRoot + "Lightbar Drawings");
        Debug.Log(fb.currDir);
        fb.fileFieldText = custName.text + "_" + (System.Environment.MachineName) + "_" + DateTime.Now.ToString("yyMMddHHmmssf");
    }

    public void JustSavePDF() {
        Directory.CreateDirectory(DirRoot + "output");
        StartCoroutine(SavePDF(DirRoot + "output/" + (System.Environment.MachineName) + " Preview.pdf"));
    }

    public IEnumerator SavePDF(string filename) {
        progressStuff.Shown = false;
        progressStuff.Progress = 0;
        CameraControl.ShowWhole = true;
        CanvasDisabler.CanvasEnabled = false;

        Camera cam = FindObjectOfType<CameraControl>().GetComponent<Camera>();

        cam.transform.position = new Vector3(0f, 0f, -10f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Vector3 tl = Vector3.zero, br = Vector3.zero;
        foreach(ReferencePoint rp in FindObjectsOfType<ReferencePoint>()) {
            if(rp.gameObject.name == "tl") {
                tl = cam.WorldToScreenPoint(rp.transform.position);
            } else if(rp.gameObject.name == "br") {
                br = cam.WorldToScreenPoint(rp.transform.position);
            }
        }

        Rect capRect = new Rect(tl.x, br.y, br.x - tl.x, tl.y - br.y);

        yield return StartCoroutine(CapImages(capRect));

        progressStuff.Shown = true;
        progressStuff.Text = "Finished capturing images.";

        PDFExportJob pej = new PDFExportJob();

        pej.Start(filename + (!filename.EndsWith(".pdf") ? ".pdf" : ""));
        while(!pej.Update()) {
            yield return null;
        }

        yield return null;
    }

    public IEnumerator CapImages(Rect capRect) {
        RefreshCurrentHeads();

        Camera cam = FindObjectOfType<CameraControl>().GetComponent<Camera>();

        bool debugBit = LightLabel.showBit;
        LightLabel.showBit = false;
        LightLabel.showParts = false;
        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.DispError = false;
            alpha.Refresh(true);
        }
        foreach(LensLabel alpha in FindObjectsOfType<LensLabel>()) {
            alpha.Refresh();
        }
        Texture2D tex = new Texture2D(Mathf.RoundToInt(capRect.width), Mathf.RoundToInt(capRect.height));
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(capRect, 0, 0);
        tex.Apply();

        Directory.CreateDirectory("tempgen");
        using(FileStream imgOut = new FileStream("tempgen\\desc.png", FileMode.OpenOrCreate)) {
            byte[] imgbytes = tex.EncodeToPNG();
            imgOut.Write(imgbytes, 0, imgbytes.Length);
        }

        LightLabel.showJustBit = true;
        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh(true);
        }
        foreach(LensLabel alpha in FindObjectsOfType<LensLabel>()) {
            alpha.Refresh();
        }
        LightLabel.showJustBit = false;
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(capRect, 0, 0);
        tex.Apply();

        using(FileStream imgOut = new FileStream("tempgen\\bits.png", FileMode.OpenOrCreate)) {
            byte[] imgbytes = tex.EncodeToPNG();
            imgOut.Write(imgbytes, 0, imgbytes.Length);
        }

        LightLabel.showParts = true;
        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh(true);
        }
        foreach(LensLabel alpha in FindObjectsOfType<LensLabel>()) {
            alpha.Refresh();
        }
        LightLabel.showParts = false;
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(capRect, 0, 0);
        tex.Apply();

        using(FileStream imgOut = new FileStream("tempgen\\part.png", FileMode.OpenOrCreate)) {
            byte[] imgbytes = tex.EncodeToPNG();
            imgOut.Write(imgbytes, 0, imgbytes.Length);
        }

        LightLabel.showWire = true;
        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh(false);
        }
        foreach(LensLabel alpha in FindObjectsOfType<LensLabel>()) {
            alpha.Refresh();
        }
        foreach(SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>()) {
            sr.color = Color.white;
        }

        yield return new WaitForEndOfFrame();
        tex.ReadPixels(capRect, 0, 0);
        tex.Apply();

        using(FileStream imgOut = new FileStream("tempgen\\wire.png", FileMode.OpenOrCreate)) {
            byte[] imgbytes = tex.EncodeToPNG();
            imgOut.Write(imgbytes, 0, imgbytes.Length);
        }

        LightLabel.colorlessWire = true;
        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh(false);
        }
        foreach(LensLabel alpha in FindObjectsOfType<LensLabel>()) {
            alpha.Refresh();
        }
        foreach(SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>()) {
            sr.color = new Color32(116, 116, 116, 255);
        }
        LightLabel.colorlessWire = false;
        LightLabel.showWire = false;

        yield return new WaitForEndOfFrame();
        tex.ReadPixels(capRect, 0, 0);
        tex.Apply();

        using(FileStream imgOut = new FileStream("tempgen\\wireClrless.png", FileMode.OpenOrCreate)) {
            byte[] imgbytes = tex.EncodeToPNG();
            imgOut.Write(imgbytes, 0, imgbytes.Length);
        }

        LightLabel.showBit = debugBit;

        cam.orthographicSize = cam.GetComponent<CameraControl>().partialOrtho;

        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh();
        }
        foreach(LensLabel alpha in FindObjectsOfType<LensLabel>()) {
            alpha.Refresh();
        }

    }

    public void Clear() {
        foreach(LightHead lh in allHeads) {
            lh.lhd.funcs.Clear();
            lh.RefreshBasicFuncDefault();
            if(lh.myLabel != null) lh.myLabel.Refresh();
        }
        foreach(SizeOptionControl soc in transform.GetComponentsInChildren<SizeOptionControl>(true))
            soc.ShowLong = true;
        foreach(Lens opt in LightDict.inst.lenses) {
            if(opt.partSuffix == "C") {
                foreach(BarSegment seg in transform.GetComponentsInChildren<BarSegment>(true)) {
                    seg.lens = opt;
                }
            }
        }

        RefreshBits();

        CreatePatts();
    }

    public void RefreshCurrentHeads() {
        List<LightHead> headList = new List<LightHead>(50);

        RaycastHit info;
        if(Physics.Raycast(new Ray(first.transform.position, new Vector3(1, 0.5f)), out info)) {
            headList.Add(first);
            LightHead curr = info.transform.GetComponent<LightHead>();
            while(curr != first && headList.Count < 50) {
                headList.Add(curr);
                Ray ray;
                switch(curr.loc) {
                    case Location.FRONT_CORNER:
                        if(curr.transform.position.x < 0) {
                            ray = new Ray(curr.transform.position, new Vector3(1, 0));
                        } else {
                            ray = new Ray(curr.transform.position, new Vector3(1, -1));
                        }
                        break;
                    case Location.ALLEY:
                        if(curr.transform.position.x < 0) {
                            ray = new Ray(curr.transform.position, new Vector3(0, 1));
                        } else {
                            ray = new Ray(curr.transform.position, new Vector3(-0.5f, -1));
                        }
                        break;
                    case Location.REAR_CORNER:
                        if(curr.transform.position.x < 0) {
                            ray = new Ray(curr.transform.position, new Vector3(-1, 1));
                        } else {
                            ray = new Ray(curr.transform.position, new Vector3(-1, -0.5f));
                        }
                        break;
                    case Location.FAR_REAR:
                    case Location.REAR:
                        ray = new Ray(curr.transform.position, new Vector3(-1, 0));
                        break;
                    case Location.FRONT:
                    default:
                        ray = new Ray(curr.transform.position, new Vector3(1, 0));
                        break;
                }
                if(Physics.Raycast(ray, out info))
                    curr = info.transform.GetComponent<LightHead>();
                else
                    break;
            }
        }

        headNumber = headList.ToArray();
    }

    public void AutoPhase() {
        string AStyle = first.lhd.style != null ? first.lhd.style.name.Split('\\', '/')[0] : "";

        if(AStyle.Length == 0) {
            ErrorText.inst.DispError("Define the color that's using Phase A on the front driver's corner light, first.");
            return;
        }

        string[] alphaStyles = new string[2];
        foreach(LightHead alpha in allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.hasRealHead && alpha.lhd.funcs.Contains(BasicFunction.FLASHING)) {
                alphaStyles = alpha.lhd.style != null ? alpha.lhd.style.name.Split('\\', '/') : new string[2];
                alpha.basicPhaseA = (alphaStyles[0].Equals(AStyle, StringComparison.CurrentCultureIgnoreCase));
                if(alphaStyles.Length > 1) {
                    alpha.basicPhaseA2 = !alpha.basicPhaseA;
                }
            }
        }
    }

    public IEnumerator RefreshAllLabels() {
        yield return new WaitForEndOfFrame();

        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh();
        }

        yield return null;
    }

    public void BeginPreview() {
        funcBeingTested = FunctionEditPane.currFunc;
        FindObjectOfType<CameraControl>().OnlyCamSelectedHead.Clear();
        CameraControl.ShowWhole = true;
        CanvasDisabler.CanvasEnabled = false;
        PattTimer.inst.StartTimer();
        StartCoroutine(RefreshAllLabels());
    }

    public void EndPreview() {
        PattTimer.inst.StopTimer();
        CameraControl.ShowWhole = false;
        CanvasDisabler.CanvasEnabled = true;
        funcBeingTested = AdvFunction.NONE;
        StartCoroutine(RefreshAllLabels());
    }

    public void SaveAndQuit() {
        quitAfterSave = true;
        fb.BeginSave();
    }

    public void ForceQuit() {
        forceQuit = true;
        Application.Quit();
    }

    public void OnApplicationQuit() {
        if(funcBeingTested != AdvFunction.NONE) {
            Application.CancelQuit();
            EndPreview();
            return;
        }
        if(!forceQuit && moddedBar) {
            Application.CancelQuit();
            quitDialog.SetActive(true);
        }
    }

}

public class ThreadedJob {
    private bool m_IsDone = false;
    private object m_Handle = new object();
    private System.Threading.Thread m_Thread = null;
    public bool IsDone {
        get {
            bool tmp;
            lock(m_Handle) {
                tmp = m_IsDone;
            }
            return tmp;
        }
        set {
            lock(m_Handle) {
                m_IsDone = value;
            }
        }
    }

    public virtual void Start() {
        m_Thread = new System.Threading.Thread(Run);
        m_Thread.Start();
    }
    public virtual void Abort() {
        m_Thread.Abort();
    }

    protected virtual void ThreadFunction() { }

    protected virtual void OnFinished() { }

    public virtual bool Update() {
        if(IsDone) {
            OnFinished();
            return true;
        }
        return false;
    }
    private void Run() {
        ThreadFunction();
        IsDone = true;
    }
}

public class PDFExportJob : ThreadedJob {
    public string custName, orderNumber, notes;
    public string progressText = "...";
    public float progressPercentage = 0f;
    public bool failed = false;
    public NbtCompound patts;
    public bool useCAN = false;
    public string BarModel = "";
    public string filename = "";
    public Rect capRect;
    public List<String> issues;
    public BarManager.ProgressStuff progressStuff;
    public LightHead[] headNumber;
    public Dictionary<LightHead, string> color1Wire, color2Wire;
    public BOMCables bomcables;

    public void Start(string fname) {
        BarManager bm = BarManager.inst;
        custName = bm.custName.text;
        orderNumber = bm.orderNum.text;
        notes = bm.notes.text;
        failed = false;
        useCAN = BarManager.useCAN;
        patts = bm.patts.Clone() as NbtCompound;
        progressStuff = bm.progressStuff;
        BarModel = bm.BarModel;
        filename = fname;
        issues = new List<string>();
        foreach(IssueChecker issue in bm.issues) {
            if(issue.DoCheck()) {
                issues.Add(issue.pdfText);
            }
        }
        headNumber = BarManager.headNumber;
        color1Wire = new Dictionary<LightHead, string>();
        color2Wire = new Dictionary<LightHead, string>();
        bomcables = GameObject.FindObjectOfType<BOMCables>();
        if(bomcables == null)
            Debug.LogError("Couldn't find BOMCable Object");

        foreach(LightHead alpha in headNumber) {
            alpha.PrefetchPatterns();
            if(alpha.hasRealHead) {
                color1Wire[alpha] = BarManager.GetWireColor1(alpha);
                if(alpha.lhd.style.isDualColor) color2Wire[alpha] = BarManager.GetWireColor2(alpha);
            }
        }

        Camera cam = GameObject.FindObjectOfType<CameraControl>().GetComponent<Camera>();

        Vector3 tl = Vector3.zero, br = Vector3.zero;
        foreach(ReferencePoint rp in GameObject.FindObjectsOfType<ReferencePoint>()) {
            if(rp.gameObject.name == "tl") {
                tl = cam.WorldToScreenPoint(rp.transform.position);
            } else if(rp.gameObject.name == "br") {
                br = cam.WorldToScreenPoint(rp.transform.position);
            }
        }

        capRect = new Rect(tl.x, br.y, br.x - tl.x, tl.y - br.y);

        base.Start();
    }

    public override bool Update() {
        if(IsDone) {
            OnFinished();
            return true;
        } else {
            lock(progressStuff) {
                progressStuff.Shown = true;
                progressStuff.Progress = progressPercentage;
                progressStuff.Text = progressText;
            }
            return false;
        }
    }

    protected override void OnFinished() {
        if(failed) {
            ErrorText.inst.DispError("Problem saving the PDF.  Do you still have it open?");
        } else {
            Application.OpenURL("file://" + filename);
        }

        if(BarManager.inst.savePDF)
            BarManager.inst.fb.currFile = BarManager.inst.barFilePath;
        BarManager.inst.savePDF = false;

        CanvasDisabler.CanvasEnabled = true;
        CameraControl.ShowWhole = false;

        progressStuff.Shown = false;

    }

    protected override void ThreadFunction() {
        progressText = "Finished capturing images.";

        PdfDocument doc = new PdfDocument();
        doc.Info.Author = "Star Headlight and Lantern Co., Inc.";
        doc.Info.Creator = "1000 Lightbar Configurator";
        doc.Info.Title = "1000 Lightbar Configuration";
        lock(progressStuff) {
            progressText = "Publishing Page 1/5: Overview...";
            progressPercentage = 10;
        }
        OverviewPage(doc.AddPage(), capRect);
        lock(progressStuff) {
            progressText = "Publishing Page 2/5: BOM...";
            progressPercentage = 30;
        }
        PartsPage(doc.AddPage(), capRect);
        lock(progressStuff) {
            progressText = "Publishing Page 3/5: Wiring...";
            progressPercentage = 50;
        }
        WiringPage(doc.AddPage(), capRect);
        lock(progressStuff) {
            progressText = "Publishing Page 4/5: Programming...";
            progressPercentage = 80;
        }
        PatternPage(doc.AddPage(), capRect);
        lock(progressStuff) {
            progressText = "Publishing Page 5/5: Output Map...";
            progressPercentage = 90;
        }
        OutputMapPage(doc.AddPage(), capRect);
        lock(progressStuff) {
            progressText = "Saving...";
            progressPercentage = 99;
        }

        try {
            doc.Save(filename);
        } catch(IOException) {
            failed = true;
        } finally {
            doc.Close();
            doc.Dispose();
        }
    }

    public void OverviewPage(PdfPage p, Rect capRect) {
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont courier = new XFont("Courier New", new XUnit(12, XGraphicsUnit.Point).Inch);
        XFont courierSm = new XFont("Courier New", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliLg = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch);
        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliSmBold = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch, XFontStyle.Bold);

        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage descImg = XImage.FromFile("tempgen\\desc.png")) {
            gfx.DrawImage(descImg, 0.5, 1.3, capRect.width * scale, capRect.height * scale);
        }
        using(XImage tl = XImage.FromFile("pdfassets\\TopLeft.png")) {
            gfx.DrawImage(tl, 0.5, 0.5, 0.74, 0.9);
        }
        using(XImage tr = XImage.FromFile("pdfassets\\TopRight.png")) {
            gfx.DrawImage(tr, ((float)p.Width.Inch) - 2.45, 0.5, 1.95, 0.75);
        }

        progressPercentage = 20;

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Star 1000", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));
        tf.DrawString("Model " + BarModel, courier, XBrushes.Black, new XRect(0.5, 1.1, p.Width.Inch - 1.0, 1.0));

        tf.Alignment = XParagraphAlignment.Left;

        tf.DrawString("Light Head Type and Style", caliSmBold, XBrushes.Black, new XRect(1.4, 3.39, 2.0, 0.1));
        tf.DrawString("Amperage", caliSmBold, XBrushes.Black, new XRect(4.0, 3.39, 0.5, 0.1));
        if(CameraControl.ShowPricing)
            tf.DrawString("List Price", caliSmBold, XBrushes.Black, new XRect(5.5, 3.39, 0.5, 0.1));

        double top = 3.5;
        for(int i = 0; i < headNumber.Length; i++) {
            LightHead lh = headNumber[i];
            tf.DrawString("Position " + (i + 1).ToString("00"), courierSm, XBrushes.Black, new XRect(0.5, top + (i * 0.10), 1.2, 0.10));
            PrintHead(tf, caliSm, courierSm, top + (i * 0.10), lh);
        }
        top += headNumber.Length * 0.1;
        top += 0.1;
        top += 0.2;

        XPen border = new XPen(XColors.Black, 0.025);

        StringBuilder sb = new StringBuilder(1024);
        foreach(string issue in issues) {
            sb.AppendLine(issue);
        }
        if(sb.Length > 0) {
            tf.DrawString("Issues found:", caliSmBold, XBrushes.Black, new XRect(0.5, top, 1.2, 0.10));
            tf.DrawString("Sign Off:", caliLg, XBrushes.Black, new XRect(3.2, top - .1, 0.6, 0.2));
            gfx.DrawLine(border, 3.8, top + 0.1, 4.0, top - 0.1);
            gfx.DrawLine(border, 3.8, top - 0.1, 4.0, top + 0.1);
            gfx.DrawLine(border, 4.0, top + 0.1, 6.0, top + 0.1);
            tf.DrawString(sb.ToString(), caliSm, XBrushes.Black, new XRect(0.8, top + 0.10, p.Width.Inch - 1.3, 2.0));
        }

        top = p.Height.Inch - 2.5;
        gfx.DrawRectangle(border, XBrushes.White, new XRect(0.5, top, p.Width.Inch - 1.0, 2.0));
        gfx.DrawLine(border, 0.5, top + 0.5, p.Width.Inch - 0.5, top + 0.5);

        tf.DrawString("Customer", caliSm, XBrushes.DarkGray, new XRect(0.55, top + 0.01, 1.0, 0.15));
        tf.DrawString("Order Number / PO", caliSm, XBrushes.DarkGray, new XRect(4, top + 0.01, 1.5, 0.15));
        gfx.DrawLine(border, 3.95, top, 3.95, top + 0.5);
        tf.DrawString("Order Date", caliSm, XBrushes.DarkGray, new XRect(6.2, top + 0.01, 1.0, 0.15));
        gfx.DrawLine(border, 6.15, top, 6.15, top + 0.5);

        tf.DrawString(custName, caliLg, XBrushes.Black, new XRect(0.6, top + 0.2, 3.0, 0.2));
        tf.DrawString(orderNumber, courier, XBrushes.Black, new XRect(4.05, top + 0.2, 1.75, 0.2));
        tf.DrawString(System.DateTime.Now.ToString("MMM dd, yyyy"), courier, XBrushes.Black, new XRect(6.25, top + 0.2, 3.0, 0.2));

        tf.DrawString("Order Notes", caliSm, XBrushes.DarkGray, new XRect(0.55, top + 0.51, 1.0, 0.15));
        tf.DrawString(notes, caliSm, XBrushes.Black, new XRect(0.6, top + 0.61, p.Width.Inch - 1.2, 1.4));

        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, caliSm, XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
    }

    private static void PrintHead(XTextFormatter tf, XFont caliSm, XFont courierSm, double top, LightHead lh) {
        if(lh.lhd.style == null) {
            tf.DrawString(" -- ", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 0.5, 0.10));
        } else {
            tf.DrawString((lh.lhd.optic.styles.Count > 1 ? lh.lhd.style.name + " " : "") + lh.lhd.optic.name, caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            tf.DrawString((lh.lhd.optic.amperage * 0.001f).ToString("F3"), courierSm, XBrushes.Black, new XRect(4.0, top, 1.0, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (lh.lhd.optic.cost * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(5.5, top, 1.0, 0.10));
        }
    }

    public void PartsPage(PdfPage p, Rect capRect) {
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont courier = new XFont("Courier New", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliBold = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch, XFontStyle.Bold);
        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);

        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage partImg = XImage.FromFile("tempgen\\part.png")) {
            gfx.DrawImage(partImg, 0.5, 1.0, capRect.width * scale, capRect.height * scale);
        }

        progressPercentage = 40;

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Model " + BarModel, new XFont("Courier New", new XUnit(24, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.5, p.Width.Inch - 1.0, 1.0));
        tf.DrawString("Production Copy - Bill of Materials", caliBold, XBrushes.Black, new XRect(0.5, 0.8, p.Width.Inch - 1.0, 1.0));

        tf.DrawString("Quantity", caliBold, XBrushes.Black, new XRect(0.5, 3.3, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Component", caliBold, XBrushes.Black, new XRect(1.5, 3.3, 1.0, 0.2));
        tf.DrawString("Description", caliBold, XBrushes.Black, new XRect(3.0, 3.3, 4.0, 0.2));

        List<string> parts = new List<string>();
        Dictionary<string, int> counts = new Dictionary<string, int>();
        Dictionary<string, object> descs = new Dictionary<string, object>();
        foreach(LightHead lh in BarManager.headNumber) {
            if(lh.lhd.style != null) {
                string part = lh.PartNumber;
                if(counts.ContainsKey(part)) {
                    counts[part]++;
                } else {
                    counts[part] = 1;
                    descs[part] = lh;
                    parts.Add(part);
                }
            }
        }

        double top = 3.5;
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("S8070-454-" + ((bomcables.dualLCount + bomcables.dualRCount) > 0 ? "2" : "1"), courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
        tf.DrawString("Control Circuit - " + ((bomcables.dualLCount + bomcables.dualRCount) > 0 ? "Dual-Color Capable" : "Single-Color Only"), caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
        top += 0.25;
        foreach(string part in parts) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(counts[part] + "", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString((descs[part] as LightHead).PartNumber, courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString(((descs[part] as LightHead).lhd.optic.styles.Count > 1 ? (descs[part] as LightHead).lhd.style.name + " " : "") + (descs[part] as LightHead).lhd.optic.name, caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }

        progressPercentage = 45;

        top += 0.2;

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Quantity", caliBold, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Lenses", caliBold, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
        tf.DrawString("Description", caliBold, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));

        parts.Clear();
        counts.Clear();
        descs.Clear();
        foreach(BarSegment seg in BarManager.inst.allSegs) {
            if(seg.Visible && seg.lens != null) {
                string part = seg.LensPart;
                if(counts.ContainsKey(part)) {
                    counts[part]++;
                } else {
                    counts[part] = 1;
                    descs[part] = seg;
                    parts.Add(part);
                }
            }
        }

        top += 0.2;
        foreach(string part in parts) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(counts[part] + "", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString((descs[part] as BarSegment).LensPart, courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString((descs[part] as BarSegment).LensDescrip, caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }

        top += 0.2;

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Quantity", caliBold, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Cables", caliBold, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
        tf.DrawString("Description", caliBold, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));

        top += 0.2;
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("SWH-" + (BarManager.useCAN ? "CAN" : "1000BAR") + (BarManager.cableLength == 1 ? "25" : "17"), courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
        tf.DrawString("External Control Cable - " + (BarManager.cableLength == 1 ? "25" : "17") + "'", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
        top += 0.15;
        if(BarManager.cableType == 1 || BarManager.useCAN) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("S271-POWER10-" + (BarManager.cableLength == 1 ? "25" : "17"), courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("10 Gauge Power Cable - " + (BarManager.cableLength == 1 ? "25" : "17") + "'", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }
        if(bomcables.singleLCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + bomcables.singleLCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("SWH-1000-" + (BarManager.inst.BarSize > 2 ? "65" : "51") + "L", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Control Cable - Single Color, Left", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }
        if(bomcables.singleRCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + bomcables.singleRCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("SWH-1000-" + (BarManager.inst.BarSize > 2 ? "65" : "51") + "R", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Control Cable - Single Color, Right", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }
        if(bomcables.dualLCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + bomcables.dualLCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("SWH-1000-" + (BarManager.inst.BarSize > 2 ? "65" : "51") + "DL", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Control Cable - Dual Color, Left", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }
        if(bomcables.dualRCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + bomcables.dualRCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("SWH-1000-" + (BarManager.inst.BarSize > 2 ? "65" : "51") + "DR", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Control Cable - Dual Color, Right", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }


        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, caliSm, XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
    }

    public void WiringPage(PdfPage p, Rect capRect) {
        p.Orientation = PageOrientation.Landscape;

        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);

        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage wireImg = XImage.FromFile("tempgen\\wire.png")) {
            gfx.DrawImage(wireImg, 0.5, 1.2, capRect.width * scale, capRect.height * scale);
        }

        progressPercentage = 60;

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Wiring Diagram", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));

        XImage circuit = XImage.FromFile("pdfassets\\Circuit.png");
        scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (circuit.PixelWidth * 1.0f);
        gfx.DrawImage(circuit, 0.5, 3.75, circuit.PixelWidth * scale, circuit.PixelHeight * scale);

        progressPercentage = 70;

        tf.Alignment = XParagraphAlignment.Left;
        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, caliSm, XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
    }

    public void PatternPage(PdfPage p, Rect capRect) {
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliBold = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch, XFontStyle.Bold);

        XPen border = new XPen(XColors.Black, 0.025);

        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage wireImg = XImage.FromFile("tempgen\\wireClrless.png")) {
            gfx.DrawImage(wireImg, 0.5, 1.2, capRect.width * scale, capRect.height * scale);
        }

        progressPercentage = 90;

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Bar Programming", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));

        if(patts.Contains("prog")) {
            gfx.DrawRectangle(border, XBrushes.Yellow, new XRect(0.5, 3.25, 0.75, 0.7));
            tf.DrawString("Default\nProgram\n" + patts["prog"].ByteValue, caliBold, XBrushes.Black, new XRect(0.5, 3.3, 0.75, 0.6));
            gfx.DrawRectangle(border, XBrushes.Yellow, new XRect(p.Width.Inch - 1.25, 3.25, 0.75, 0.7));
            tf.DrawString("Default\nProgram\n" + patts["prog"].ByteValue, caliBold, XBrushes.Black, new XRect(p.Width.Inch - 1.25, 3.3, 0.75, 0.6));
        }

        double top = 3.0;

        tf.DrawString("Input Map", caliBold, XBrushes.Black, new XRect(3.0, top, p.Width.Inch - 6.0, 0.1));
        top += 0.2;
        if(useCAN) {
            PrintRow(tf, caliSm, GetFuncFromMap(0), GetFuncFromMap(12), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(1), GetFuncFromMap(13), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(2), GetFuncFromMap(14), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(3), GetFuncFromMap(15), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(4), GetFuncFromMap(16), ref top);

            PrintRow(tf, caliSm, GetFuncFromMap(5), GetFuncFromMap(17), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(6), GetFuncFromMap(18), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(7), GetFuncFromMap(19), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(8), "POWER", ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(9), "GROUND", ref top);

            PrintRow(tf, caliSm, GetFuncFromMap(10), "---", ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(11), "---", ref top);
        } else {
            PrintRow(tf, caliSm, GetFuncFromMap(1) + " - White & Yellow", "White & Orange - " + GetFuncFromMap(0), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(3) + " - Brown", "White & Red - " + GetFuncFromMap(2), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(4) + " - Yellow", "Red & Green - " + GetFuncFromMap(11), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(6) + " - Blue", "Green - " + GetFuncFromMap(5), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(8) + " - Purple & White", "Gray - " + GetFuncFromMap(7), ref top);
            PrintRow(tf, caliSm, GetFuncFromMap(10) + " - White & Pink", "Purple - " + GetFuncFromMap(9), ref top);
            PrintRow(tf, caliSm, "---", "---", ref top);
        }

        top += 0.05;

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Function Definitions", caliBold, XBrushes.Black, new XRect(3.0, top, p.Width.Inch - 6.0, 0.1));
        top += 0.2;
        gfx.DrawRectangle(border, new XRect(0.5, top, p.Width.Inch - 1.0, 0.4));
        tf.DrawString("Function", caliBold, XBrushes.Black, new XRect(0.5, top + (useCAN ? 0.0 : 0.1), 1.25, 0.2));
        if(useCAN) tf.DrawString("Break Out Box", caliBold, XBrushes.Black, new XRect(0.50, top + 0.2, 1.25, 0.2));
        gfx.DrawLine(border, 1.75, top, 1.75, top + 0.4);
        tf.DrawString("Positions", caliBold, XBrushes.Black, new XRect(1.8, top, 4.0, 0.1));
        tf.DrawString("Phase A", caliBold, XBrushes.Black, new XRect(1.8, top + 0.2, 1.95, 0.1));
        gfx.DrawLine(border, 3.8, top + 0.2, 3.8, top + 0.4);
        tf.DrawString("Phase B", caliBold, XBrushes.Black, new XRect(3.85, top + 0.2, 1.95, 0.1));
        gfx.DrawLine(border, 5.8, top, 5.8, top + 0.4);
        tf.DrawString("Pattern(s)", caliBold, XBrushes.Black, new XRect(5.85, top + 0.1, p.Width.Inch - 6.4, 0.2));

        top += 0.4;
        foreach(int func in new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 8192, 16384, 32768, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000 }) {
            for(int i = 0; i < 20; i++) {
                if((FnDragTarget.inputMap[i] & (int)func) > 0) {

                    switch(func) {
                        case 0x2: // PRIO1
                        case 0x4: // PRIO2
                        case 0x8: // PRIO3
                        case 0x100: // ICL
                        case 0x400: // FTAKEDOWN
                        case 0x800: // FALLEY
                        case 0x40000: // PRIO4
                        case 0x80000: // PRIO5
                            gfx.DrawRectangle(border, new XRect(0.5, top, p.Width.Inch - 1.0, 0.3));
                            gfx.DrawLine(border, 1.75, top, 1.75, top + 0.3);
                            gfx.DrawLine(border, 3.8, top, 3.8, top + 0.3);
                            gfx.DrawLine(border, 5.8, top, 5.8, top + 0.3);
                            tf.DrawString(GetFuncFromInt(func), caliSm, XBrushes.Black, new XRect(0.5, top + 0.075, 1.25, 0.1));
                            break;
                        default:
                            gfx.DrawRectangle(border, new XRect(0.5, top, p.Width.Inch - 1.0, 0.2));
                            gfx.DrawLine(border, 1.75, top, 1.75, top + 0.2);
                            gfx.DrawLine(border, 5.8, top, 5.8, top + 0.2);
                            tf.DrawString(GetFuncFromInt(func), caliSm, XBrushes.Black, new XRect(0.5, top + 0.025, 1.25, 0.1));
                            break;
                    }

                    List<string> parts = new List<string>();
                    AdvFunction advfunc = (AdvFunction)func;

                    PartComparer pc = new PartComparer();

                    switch(func) {
                        case 0x2: // PRIO1
                        case 0x4: // PRIO2
                        case 0x8: // PRIO3
                        case 0x100: // ICL
                        case 0x400: // FTAKEDOWN
                        case 0x800: // FALLEY
                        case 0x40000: // PRIO4
                        case 0x80000: // PRIO5
                            List<string> partsB = new List<string>();
                            foreach(LightHead alpha in headNumber) {
                                if(!alpha.hasRealHead) continue;
                                if(alpha.GetIsEnabled(advfunc, false)) {
                                    if(alpha.GetPhaseB(advfunc, false)) {
                                        partsB.Add(color1Wire[alpha]);
                                    } else {
                                        parts.Add(color1Wire[alpha]);
                                    }
                                }
                                if(alpha.lhd.style.isDualColor && alpha.GetIsEnabled(advfunc, true)) {
                                    if(alpha.GetPhaseB(advfunc, true)) {
                                        partsB.Add(color2Wire[alpha]);
                                    } else {
                                        parts.Add(color2Wire[alpha]);
                                    }
                                }
                            }

                            parts.Sort(pc);
                            partsB.Sort(pc);

                            foreach(string prefix in new string[] { "FD-", "FP-", "RD-", "RP-" }) {
                                for(int wire = 1; wire < 14; wire++) {
                                    if(parts.Contains(prefix + wire)) {
                                        for(int part = 0; part < parts.Count; part++) {
                                            if(parts[part].StartsWith(prefix) && parts[part].EndsWith("-" + (wire - 1))) {
                                                parts[part] = parts[part].Split(' ')[0] + " thru -" + wire;
                                                parts.Remove(prefix + wire);
                                                break;
                                            }
                                        }
                                    }
                                    if(partsB.Contains(prefix + wire)) {
                                        for(int part = 0; part < partsB.Count; part++) {
                                            if(partsB[part].StartsWith(prefix) && partsB[part].EndsWith("-" + (wire - 1))) {
                                                partsB[part] = partsB[part].Split(' ')[0] + " thru -" + wire;
                                                partsB.Remove(prefix + wire);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            // Write out Phase A, new XRect(1.8, top, 1.95, 0.1)
                            tf.DrawString(string.Join(", ", parts.ToArray()), caliSm, XBrushes.Black, new XRect(1.8, top + 0.025, 1.95, 0.3));
                            // Write out Phase B, new XRect(3.85, top, 1.95, 0.1)
                            tf.DrawString(string.Join(", ", partsB.ToArray()), caliSm, XBrushes.Black, new XRect(3.85, top + 0.025, 1.95, 0.3));
                            break;
                        default:
                            // Write out enabled, new XRect(1.8, top, 4.0, 0.1)
                            foreach(LightHead alpha in headNumber) {
                                if(!alpha.hasRealHead) continue;
                                if(alpha.GetIsEnabled(advfunc, false)) {
                                    parts.Add(color1Wire[alpha]);
                                }
                                if(alpha.lhd.style.isDualColor && alpha.GetIsEnabled(advfunc, true)) {
                                    parts.Add(color2Wire[alpha]);
                                }
                            }

                            parts.Sort(pc);

                            foreach(string prefix in new string[] { "FD-", "FP-", "RD-", "RP-" }) {
                                for(int wire = 0; wire < 13; wire++) {
                                    if(parts.Contains(prefix + wire)) {
                                        for(int part = 0; part < parts.Count; part++) {
                                            if(parts[part].StartsWith(prefix) && parts[part].EndsWith("-" + (wire - 1))) {
                                                parts[part] = parts[part].Split(' ')[0] + " thru -" + wire;
                                                parts.Remove(prefix + wire);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            tf.DrawString(string.Join(", ", parts.ToArray()), caliSm, XBrushes.Black, new XRect(1.8, top + 0.025, 4.0, 0.1));
                            break;
                    }
                    // Write out pattern(s), new XRect(5.85, top + 0.1, p.Width.Inch - 8.4, 0.2)
                    switch(func) {
                        case 0x2: // PRIO1
                        case 0x4: // PRIO2
                        case 0x8: // PRIO3
                        case 0x10: // TRAFFIC_LEFT
                        case 0x20: // TRAFFIC_RIGHT
                        case 0x100: // ICL
                        case 0x400: // FTAKEDOWN
                        case 0x800: // FALLEY
                        case 0x40000: // PRIO4
                        case 0x80000: // PRIO5
                            List<string> patt = new List<string>(5);
                            Pattern thisPatt;

                            foreach(LightHead alpha in headNumber) {
                                if(!alpha.hasRealHead) continue;
                                if(alpha.GetIsEnabled(advfunc, false)) {
                                    thisPatt = alpha.GetPattern(advfunc);
                                    if(thisPatt != null) {
                                        if(!patt.Contains(thisPatt.name)) {
                                            patt.Add(thisPatt.name);
                                        }
                                    }
                                }
                                if(alpha.lhd.style.isDualColor && alpha.GetIsEnabled(advfunc, true)) {
                                    thisPatt = alpha.GetPattern(advfunc, true);
                                    if(thisPatt != null) {
                                        if(!patt.Contains(thisPatt.name)) {
                                            patt.Add(thisPatt.name);
                                        }
                                    }
                                }
                                thisPatt = null;
                            }

                            if(patt.Count > 0) {
                                tf.DrawString(string.Join(", ", patt.ToArray()), caliSm, XBrushes.Black, new XRect(5.85, top + 0.025, p.Width.Inch - 6.4, 0.3));
                            }
                            break;
                        default:
                            tf.DrawString("Steady Burn", caliSm, XBrushes.Black, new XRect(5.85, top + 0.025, p.Width.Inch - 6.4, 0.2));
                            break;
                    }

                    switch(func) {
                        case 0x2: // PRIO1
                        case 0x4: // PRIO2
                        case 0x8: // PRIO3
                        case 0x100: // ICL
                        case 0x400: // FTAKEDOWN
                        case 0x800: // FALLEY
                        case 0x40000: // PRIO4
                        case 0x80000: // PRIO5
                            top += 0.3;
                            break;
                        default:
                            top += 0.2;
                            break;
                    }
                    break;
                }
            }
        }


        tf.Alignment = XParagraphAlignment.Left;
        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, caliSm, XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
    }

    public void OutputMapPage(PdfPage p, Rect capRect) {
        p.Orientation = PageOrientation.Landscape;

        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage wireImg = XImage.FromFile("tempgen\\bits.png")) {
            gfx.DrawImage(wireImg, 0.5, 2.0, capRect.width * scale, capRect.height * scale);
        }

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Output Usage Map", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));

        tf.Alignment = XParagraphAlignment.Left;
        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch), XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch), XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));

    }

    public string GetFuncFromInt(int num) {
        switch(num) {
            case 0x0: // Nothing
                return "---";
            case 0x1: // TAKEDOWN
                return "Takedown / Work Lights";
            case 0x2: // PRIO1
                return "Priority 1";
            case 0x4: // PRIO2
                return "Priority 2";
            case 0x8: // PRIO3
                return "Priority 3";
            case 0x10: // TRAFFIC_LEFT
                return "Direct Left";
            case 0x20: // TRAFFIC_RIGHT
                return "Direct Right";
            case 0x40: // ALLEY_LEFT
                return "Left Alley";
            case 0x80: // ALLEY_RIGHT
                return "Right Alley";
            case 0x100: // ICL
                return "ICL";
            case 0x200: // DIM
                return "Dimmer";
            case 0x400: // FTAKEDOWN
                return "Flashing Pursuit";
            case 0x800: // FALLEY
                return "Flashing Alley";
            case 0xC00: // FTAKEDOWN | FALLEY
                return "Flashing Alley & Pursuit";
            case 0x1000: // PATTERN
                return "Pattern";
            case 0x2000: // CRUISE
                return "Cruise";
            case 0x4000: // TURN_LEFT
                return "Turn Left";
            case 0x8000: // TURN_RIGHT
                return "Turn Right";
            case 0x10000: // TAIL
                return "Brake Lights";
            case 0x20000: // T13
                return "California T13 Steady";
            case 0x40000: // PRIO4
                return "Priority 4";
            case 0x80000: // PRIO5
                return "Priority 5";
            case 0x100000: // EMITTER
                return "Emitter";
            default:
                return "???";
        }
    }

    public string GetFuncFromMap(int which) {
        return GetFuncFromInt(FnDragTarget.inputMap[which]);
    }

    public void PrintRow(XTextFormatter tf, XFont caliSm, string left, string right, ref double top) {
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString(left, caliSm, XBrushes.Black, new XRect(2.0, top, 2.15, 0.1));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString(right, caliSm, XBrushes.Black, new XRect(4.35, top, 2.15, 0.1));
        top += 0.1;
    }

    private class PartComparer : IComparer<string> {

        public int Compare(string x, string y) {
            string xl, yl;
            string[] xBits = x.Split('-');
            string[] yBits = y.Split('-');

            xl = xBits[0];
            yl = yBits[0];

            if(xl.Equals(yl, StringComparison.CurrentCultureIgnoreCase)) {
                int xOutput = int.Parse(xBits[1]), yOutput = int.Parse(yBits[1]);
                if(xOutput == yOutput) return 0;
                else if(xOutput < yOutput) return -1;
                else return 1;
            } else {
                int xPort = 0, yPort = 0;

                switch(xl) {
                    case "FD":
                        xPort = 1;
                        break;
                    case "FP":
                        xPort = 2;
                        break;
                    case "RP":
                        xPort = 3;
                        break;
                    case "RD":
                        xPort = 4;
                        break;
                    default:
                        throw new ArgumentException("Invalid port");
                }

                switch(yl) {
                    case "FD":
                        yPort = 1;
                        break;
                    case "FP":
                        yPort = 2;
                        break;
                    case "RP":
                        yPort = 3;
                        break;
                    case "RD":
                        yPort = 4;
                        break;
                    default:
                        throw new ArgumentException("Invalid port");
                }

                if(xPort < yPort) {
                    return -1;
                } else {
                    return 1;
                }
            }
        }
    }
}