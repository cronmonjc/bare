using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using fNbt;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The Component that handles management of every piece of information about the bar itself
/// </summary>
/// <remarks>
/// <para>The BarManager Component does exactly as its name implies – it manages the entire bar,
/// from holding a list of every head that’s on it (visible or not), making sure each head
/// gets the right bits, applying auto-phase on demand, holding a list of all of the lenses,
/// tracking size, tracking pattern setup, and tracking accessories.</para>
/// <para>However, it also manages a few other things, such as bar saving / loading, handling the
/// quit callback (preventing the application from closing when the “close” button is pressed
/// under certain conditions), and handling PDF production.</para>
/// </remarks>
public class BarManager : MonoBehaviour {
    /// <summary>
    /// The directory that this application resides in
    /// </summary>
    public static string DirRoot;
    /// <summary>
    /// Bit field of pages we're allowed to publish
    /// </summary>
    public static byte canPub = 0;

    /// <summary>
    /// The function we're previewing right now
    /// </summary>
    public AdvFunction funcBeingTested = AdvFunction.NONE;

    /// <summary>
    /// Are we selecting a spot to save the PDF in the file browser?
    /// </summary>
    [NonSerialized]
    public bool savePDF = false;

    /// <summary>
    /// The size of the bar (in number of center sections)
    /// </summary>
    [Range(0, 4)]
    public int BarSize = 3;
    /// <summary>
    /// The TDOption currently in use
    /// </summary>
    public TDOption td;
    /// <summary>
    /// Are we using the CAN module?
    /// </summary>
    public static bool _useCAN = false;
    /// <summary>
    /// Are we using the CAN module?
    /// </summary>
    public static bool useCAN {
        get { return _useCAN; }
        set {
            _useCAN = value;
            GameObject.Find("UI/Canvas/FuncAssign/FunctionSelection/Panel/InputSelects/Hardwire").SetActive(!value);
            GameObject.Find("UI/Canvas/FuncAssign/FunctionSelection/Panel/InputSelects/CAN").SetActive(value);

            if(value) {
                FnDragTarget.inputMap.Value = new int[] { 0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x2000,
                                                      0x800, 0x4000, 0x8000, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000 };
            } else {
                FnDragTarget.inputMap.Value = new int[] { 1, 2, 3072, 4, 8, 512, 256, 32, 16, 128, 64, 4096, 0, 0, 0, 0, 0, 0, 0, 0 };
            }
        }
    }
    /// <summary>
    /// The type of cable we're using (single hard vs dual hard)
    /// </summary>
    public static int cableType = 0;
    /// <summary>
    /// The length of cable we're using (index of LightDict.cableLengths)
    /// </summary>
    public static int cableLength = 0;
    /// <summary>
    /// The mounting kit we're using (0 = no kit; remainder minus 1 is index of LightDict.mountKits)
    /// </summary>
    public static int mountingKit = 0;

    /// <summary>
    /// The pattern byte NbtCompound
    /// </summary>
    public NbtCompound patts;

    /// <summary>
    /// The single instance of the BarManager
    /// </summary>
    public static BarManager inst;
    /// <summary>
    /// All LightHeads on the bar, enabled and disabled; used to iterate through every head in a less memory-/CPU-intensive manner
    /// </summary>
    public List<LightHead> allHeads;
    /// <summary>
    /// All BarSegments on the bar, enabled and disabled
    /// </summary>
    public List<BarSegment> allSegs;
    /// <summary>
    /// An array of only enabled LightHeads, ordered by position on the light bar
    /// </summary>
    public static LightHead[] headNumber;

    /// <summary>
    /// The "first" LightHead, typically the driver front corner.  Set by the Unity Inspector.
    /// </summary>
    public LightHead first;

    /// <summary>
    /// InputField holding the customer name.  Set by the Unity Inspector.
    /// </summary>
    public InputField custName;
    /// <summary>
    /// InputField holding the order number.  Set by the Unity Inspector.
    /// </summary>
    public InputField orderNum;
    /// <summary>
    /// InputField holding the order notes.  Set by the Unity Inspector.
    /// </summary>
    public InputField notes;

    /// <summary>
    /// Reference to the File Browser.  Set by the Unity Inspector.
    /// </summary>
    public FileBrowser fb;
    /// <summary>
    /// The path to the bar file
    /// </summary>
    /// <remarks>Variable is used only when trying to save a PDF to restore the file path after selecting a place to save the PDF.</remarks>
    [NonSerialized]
    public string barFilePath;

    /// <summary>
    /// The reference to the Size Slider, used to display current size on slider after loading bar.  Set by the Unity Inspector.
    /// </summary>
    public Slider SizeSlider;

    /// <summary>
    /// List of all potential issues with the bar.  Set by the Unity Inspector.
    /// </summary>
    public IssueChecker[] issues;

    /// <summary>
    /// Is the BarManager currently refreshing bits on the heads?
    /// </summary>
    public static bool RefreshingBits = false;

    /// <summary>
    /// Reference to the Quit Dialog GameObject.  Set by the Unity Inspector.
    /// </summary>
    public GameObject quitDialog;
    /// <summary>
    /// Has the user modified the bar?
    /// </summary>
    public static bool moddedBar = false;
    /// <summary>
    /// Is the user trying to save the bar and quit?
    /// </summary>
    public static bool quitAfterSave = false;
    /// <summary>
    /// Does the user want to just discard changes and quit?
    /// </summary>
    public static bool forceQuit = false;

    /// <summary>
    /// Reference to the staggered output toggle.  Set via Unity Inspector.
    /// </summary>
    public Toggle AlternateOutputs;

    /// <summary>
    /// Small container of objects used in displaying PDF export progress
    /// </summary>
    [Serializable]
    public class ProgressStuff {
        /// <summary>
        /// Reference to the progress bar graphically displaying the progress.  Set via Unity Inspector.
        /// </summary>
        public Slider progressBar;
        /// <summary>
        /// Reference to the GameObject containing the progress bar.  Set via Unity Inspector.
        /// </summary>
        public GameObject progressGO;
        /// <summary>
        /// Reference to the progress text displaying the current state.  Set via Unity Inspector.
        /// </summary>
        public Text progressText;

        /// <summary>
        /// Is the progress bar shown?
        /// </summary>
        public bool Shown {
            set { progressGO.SetActive(value); }
        }

        /// <summary>
        /// Progress of the task.  Use a value between 0 and 100.
        /// </summary>
        public float Progress {
            set { progressBar.value = value; }
        }

        /// <summary>
        /// Text describing the current state of the task
        /// </summary>
        public string Text {
            set { progressText.text = value; }
        }
    }

    /// <summary>
    /// The object containing everything for the progress bar.  Elements set via Unity Inspector.
    /// </summary>
    public ProgressStuff progressStuff;

    /// <summary>
    /// All of the base bar model numbers
    /// </summary>
    public string[] models;
    /// <summary>
    /// All of the base bar sale prices, in whole cents
    /// </summary>
    public uint[] prices;

    /// <summary>
    /// The bar model number currently in use
    /// </summary>
    public string BarModel {
        get {
            try {
                return models[BarSize];
            } catch(Exception) {
                return "????";
            }
        }
    }

    /// <summary>
    /// The human-friendly bar width
    /// </summary>
    public string BarWidth {
        get {
            switch(BarSize) {
                case 0:
                    return "37\"";
                case 1:
                    return "44\"";
                case 2:
                    return "51\"";
                case 3:
                    return "58\"";
                case 4:
                    return "65\"";
                default:
                    return "??\"";
            }
        }
    }

    /// <summary>
    /// The sale price of the base bar currently in use
    /// </summary>
    public uint BarPrice {
        get {
            try {
                return prices[BarSize];
            } catch(Exception) {
                return 999999;
            }
        }
    }

    /// <summary>
    /// Extract information from the library file
    /// </summary>
    /// <param name="cmpd">The NbtCompound Tag containing the information the BarManager needs</param>
    public void Initialize(NbtCompound cmpd) {
        NbtCompound partCmpd = cmpd.Get<NbtCompound>("part");
        models = new string[] { partCmpd["0"].StringValue, partCmpd["1"].StringValue, partCmpd["2"].StringValue, partCmpd["3"].StringValue, partCmpd["4"].StringValue };
        NbtCompound priceCmpd = cmpd.Get<NbtCompound>("price");
        prices = new uint[] { (uint)priceCmpd["0"].IntValue, (uint)priceCmpd["1"].IntValue, (uint)priceCmpd["2"].IntValue, (uint)priceCmpd["3"].IntValue, (uint)priceCmpd["4"].IntValue };
    }

    /// <summary>
    /// Awake is called once, immediately as the object is created (typically at load time)
    /// </summary>
    void Awake() {
        CreatePatts();

        allHeads = new List<LightHead>();
        inst = this;

        string[] parts = Application.dataPath.Split('/', '\\');
        DirRoot = string.Join("/", parts, 0, parts.Length - 1) + "/";
    }

    /// <summary>
    /// Create skeleton pattern tag
    /// </summary>
    public void CreatePatts() {
        patts = new NbtCompound("pats"); // Get the root created

        foreach(string alpha in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim", "traf" }) {
            patts.Add(new NbtCompound(alpha)); // Create roots for each of the 19 definable functions
        }

        // Traffic Director's special, gets its own personalized set of shorts
        patts.Get<NbtCompound>("traf").AddRange(new NbtShort[] { new NbtShort("er1", 0), new NbtShort("er2", 0), new NbtShort("ctd", 0), new NbtShort("cwn", 0) });

        foreach(string alpha in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim" }) {
            // Create enable shorts for every other function
            patts.Get<NbtCompound>(alpha).AddRange(new NbtShort[] { new NbtShort("ef1", 0), new NbtShort("ef2", 0), new NbtShort("er1", 0), new NbtShort("er2", 0) });
        }

        // Give Dimmer function a percentage short (even if it's not in use)
        patts.Get<NbtCompound>("dim").Add(new NbtShort("dimp", 15));

        foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw" }) {
            // Create phase shorts for every Flashing function
            patts.Get<NbtCompound>(alpha).AddRange(new NbtShort[] { new NbtShort("pf1", 0), new NbtShort("pf2", 0), new NbtShort("pr1", 0), new NbtShort("pr2", 0) });
        }

        // Traffic Director pattern
        patts.Get<NbtCompound>("traf").Add(new NbtShort("patt", 0));

        foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl" }) {
            // Flashing patterns for color 1
            patts.Get<NbtCompound>(alpha).Add(new NbtCompound("pat1", new NbtTag[] { new NbtShort("fcen", 0), new NbtShort("finb", 0), new NbtShort("foub", 0), new NbtShort("ffar", 0), new NbtShort("fcor", 0),
                                                                                     new NbtShort("rcen", 0), new NbtShort("rinb", 0), new NbtShort("roub", 0), new NbtShort("rfar", 0), new NbtShort("rcor", 0) }));
            // Flashing patterns for color 2
            patts.Get<NbtCompound>(alpha).Add(new NbtCompound("pat2", new NbtTag[] { new NbtShort("fcen", 0), new NbtShort("finb", 0), new NbtShort("foub", 0), new NbtShort("ffar", 0), new NbtShort("fcor", 0),
                                                                                     new NbtShort("rcen", 0), new NbtShort("rinb", 0), new NbtShort("roub", 0), new NbtShort("rfar", 0), new NbtShort("rcor", 0) }));
        }

        // Create the Input Map IntArray Tag
        FnDragTarget.inputMap = new NbtIntArray("map", new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 3072, 4096, 0, 0, 0, 0, 0, 0, 0, 0 });
        patts.Add(FnDragTarget.inputMap);
    }

    /// <summary>
    /// Set whether or not we're using CAN.  Called by the two buttons enabling / disabling CAN.
    /// </summary>
    public void SetCAN(bool to) {
        useCAN = to;
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        allHeads.AddRange(transform.GetComponentsInChildren<LightHead>(true)); // Get all of the light heads together
        allSegs.AddRange(transform.GetComponentsInChildren<BarSegment>(true)); // Same for bar segments
        StartCoroutine(RefreshBitsIEnum()); // Refresh the bits
        progressStuff.Shown = false; // Hide progress stuff

        foreach(Lens opt in LightDict.inst.lenses) {
            if(opt.partSuffix == "C-C") { // Give every bar segment a clear coated clear lens
                foreach(BarSegment seg in allSegs) {
                    seg.lens = opt;
                }
            }
        }
    }

    /// <summary>
    /// Converts a function from an AdvFunction value to a pattern Tag name.
    /// </summary>
    /// <param name="left">If the function is AdvFunction.TAIL, do we return the left half of the function?</param>
    /// <param name="f">The function to fetch a Tag name for</param>
    /// <returns>The NbtCompound Tag name for the function in the BarManager.patts NbtCompound Tag</returns>
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

    /// <summary>
    /// Converts a function from an AdvFunction value to a pattern Tag name.
    /// </summary>
    /// <param name="t">The Transform of the head we're fetching the function name for</param>
    /// <param name="f">The function to fetch a Tag name for</param>
    /// <returns>The NbtCompound Tag name for the function in the BarManager.patts NbtCompound Tag</returns>
    public static string GetFnString(Transform t, AdvFunction f) {
        return GetFnString(t.position.x < 0, f);
    }

    /// <summary>
    /// Gets the full wire information for a light head
    /// </summary>
    /// <param name="lh">The light head to fetch the wire information for</param>
    /// <returns>A string describing the wire setup for the light head</returns>
    public static string GetWire(LightHead lh) {
        return GetWireColor1(lh) + (lh.lhd.style.isDualColor ? (" C & " + GetWireColor2(lh) + " W") : "");
    }

    /// <summary>
    /// Gets the color 1 wire for a light head
    /// </summary>
    /// <param name="lh">The light head to fetch the wire information for</param>
    /// <returns>The color 1 wire for the light head</returns>
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

    /// <summary>
    /// Gets the color 2 wire for a light head
    /// </summary>
    /// <param name="lh">The light head to fetch the wire information for</param>
    /// <returns>The color 2 wire for the light head</returns>
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

    /// <summary>
    /// Set the bar size we're using.  Called by the bar size slider.
    /// </summary>
    public void SetBarSize(float to) {
        SetBarSize(Mathf.RoundToInt(to), true);
    }

    /// <summary>
    /// Set the bar size we're using
    /// </summary>
    /// <param name="sliding">Was this function called because the slider was slid?</param>
    public void SetBarSize(int to, bool sliding = false) {
        if(to < 5 && to > -1) { // Ensure size is valid
            if(!sliding) {
                SizeSlider.GetComponent<SliderSnap>().lastWholeVal = to; // Size wasn't set by sliding, set the slider to the proper value
                SizeSlider.value = to;
            } else if(ErrorLogging.logInput) {
                ErrorLogging.LogInput("Set Size to " + to);
            }

            td = TDOption.NONE; // Clear out TD Option, because chances are it's not compatible with the new size

            foreach(LightHead lh in allHeads) {
                if(lh.transform.position.y < 0) {
                    lh.shouldBeTD = false;
                    lh.RemoveBasicFunction(BasicFunction.TRAFFIC);
                }
                if(lh.myLabel != null)
                    lh.myLabel.Refresh();
            }

            BarSize = to;
            foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) { // Also make every head long (if capable)
                soc.ShowLong = true;
            }
            FindObjectOfType<CameraControl>().SelectedHead.Clear(); // While not possible to change size while heads are selected, still a good safeguard
        }
        StartCoroutine(RefreshBitsIEnum()); // Refresh the bits
    }

    /// <summary>
    /// Sets the Traffic Director option.  Called by the bar menu option buttons.  Invalid setups cannot be selected in the first place.
    /// </summary>
    public void SetTDOption(int to) {
        SetTDOption((TDOption)to);
    }

    /// <summary>
    /// Sets the Traffic Director option
    /// </summary>
    public void SetTDOption(TDOption to) {
        StartCoroutine(SetTDOptionCoroutine(to));
    }

    /// <summary>
    /// Coroutine.  Applies the Traffic Director option.
    /// </summary>
    public IEnumerator SetTDOptionCoroutine(TDOption to) {
        RaycastHit[] hits; // Create variable for raycasting
        foreach(LightHead lh in allHeads) { // Remove existing traffic options
            if(lh.transform.position.y < 0) {
                lh.shouldBeTD = false;
                lh.RemoveBasicFunction(BasicFunction.TRAFFIC);
            }
        }
        td = to; // Set internal variable first

        switch(td) {
            case TDOption.NONE:
                foreach(LightHead lh in allHeads) { // Don't even care anymore, clear out everything
                    if(lh.transform.position.y < 0) {
                        lh.RemoveBasicFunction(BasicFunction.TRAFFIC);
                    }
                }
                break;
            case TDOption.LG_SEVEN:
            case TDOption.LG_EIGHT:
                yield return new WaitForEndOfFrame(); // Wait for dust to settle
                foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) { // Force rears to large heads
                    if(soc.transform.position.y < 0) soc.ShowLong = true;
                }
                yield return new WaitForEndOfFrame(); // Wait for dust to settle
                yield return new WaitForEndOfFrame(); // Makes sure that heads get disabled / enabled properly
                hits = Physics.RaycastAll(new Vector3(-8, -1.25f), new Vector3(1, 0)); // Cast a ray against the entire rear; will hit anything with a Collider (aka heads)

                foreach(RaycastHit hit in hits) {
                    LightHead lh = hit.transform.GetComponent<LightHead>(); // Nab the light head from the hit
                    lh.lhd.funcs.Clear(); // Clear that sucker's functions out
                    lh.shouldBeTD = true; // This head should be traffic director
                    lh.AddBasicFunction(BasicFunction.TRAFFIC, false); // Apply Traffic Basic Func
                    lh.AddBasicFunction(BasicFunction.FLASHING); // Apply Flashing Basic Func
                }
                break;
            case TDOption.LG_SIX:
                yield return new WaitForEndOfFrame(); // Wait for dust to settle
                foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) { // Force rears to large heads
                    if(soc.transform.position.y < 0) soc.ShowLong = (soc.transform.position.x != 0); // ...except for the very center section
                }
                yield return new WaitForEndOfFrame(); // Wait for dust to settle
                yield return new WaitForEndOfFrame(); // Makes sure that heads get disabled / enabled properly
                hits = Physics.RaycastAll(new Vector3(-8f, -1.25f), new Vector3(1, 0)); // Cast a ray against the entire rear; will hit anything with a Collider (aka heads)

                foreach(RaycastHit hit in hits) {
                    if(BarSize == 4 && hit.transform.GetPath().Contains("RO")) continue; // If we're looking at a 4-size bar, don't do the two end heads
                    LightHead lh = hit.transform.GetComponent<LightHead>(); // Nab the light head from the hit
                    if(lh.isSmall) continue; // If the head's small (ie with 3-size bar in center), skip
                    lh.lhd.funcs.Clear(); // Clear that sucker's functions out
                    lh.shouldBeTD = true; // This head should be traffic director
                    lh.AddBasicFunction(BasicFunction.TRAFFIC, false); // Apply Traffic Basic Func
                    lh.AddBasicFunction(BasicFunction.FLASHING); // Apply Flashing Basic Func
                }
                break;
            case TDOption.SM_EIGHT:
            case TDOption.SM_SIX:
                yield return new WaitForEndOfFrame(); // Wait for dust to settle
                foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) { // Force rears to small heads
                    if(soc.transform.position.y < 0) soc.ShowLong = false;
                }
                yield return new WaitForEndOfFrame(); // Wait for dust to settle
                yield return new WaitForEndOfFrame(); // Makes sure that heads get disabled / enabled properly
                if(td == TDOption.SM_SIX)
                    hits = Physics.RaycastAll(new Vector3(-2.4f, -1.25f), new Vector3(1, 0), 4.4f); // Cast a ray of precalculated length against the rear; will hit anything with a Collider (aka heads)
                else //SM_EIGHT
                    hits = Physics.RaycastAll(new Vector3(-3.2f, -1.25f), new Vector3(1, 0), 6.0f);

                foreach(RaycastHit hit in hits) {
                    LightHead lh = hit.transform.GetComponent<LightHead>(); // Nab the light head from the hit
                    lh.lhd.funcs.Clear(); // Clear that sucker's functions out
                    lh.shouldBeTD = true; // This head should be traffic director
                    lh.AddBasicFunction(BasicFunction.TRAFFIC, false); // Apply Traffic Basic Func
                    lh.AddBasicFunction(BasicFunction.FLASHING); // Apply Flashing Basic Func
                }
                break;
        }
        yield return StartCoroutine(RefreshBitsIEnum()); // Refresh the bits

        yield return StartCoroutine(RefreshAllLabels()); // Refresh the labels

        patts.Get<NbtCompound>("traf").Get<NbtShort>("er1").Value = (short)(td == TDOption.NONE ? 0 : 1020);  // Enable the heads for traffic
        patts.Get<NbtCompound>("traf").Get<NbtShort>("patt").Value = 7; // Default pattern: Snake

        yield return null;
    }

    /// <summary>
    /// Refreshes the bits.  Called by the staggered head toggle.
    /// </summary>
    public void RefreshBits() {
        StartCoroutine(RefreshBitsIEnum());
    }

    /// <summary>
    /// Coroutine.  Applies new bits to all of the heads.
    /// </summary>
    public IEnumerator RefreshBitsIEnum() {
        RefreshingBits = true; // Tell rest of application that bits are being refreshed

        Dictionary<string, LightHead> headDict = new Dictionary<string, LightHead>(); // Cache paths so we aren't fetching them a billion times, and for easier reference
        foreach(LightHead alpha in allHeads) {
            alpha.myBit = 255; // Clear out whatever bit it already has
            alpha.FarWire = false;
            headDict[alpha.Path] = alpha; // Cache path
        }


        yield return new WaitForEndOfFrame(); // Wait for the dust to settle
        yield return new WaitForEndOfFrame();

        #region Test for Staggered Output
        bool staggCenter = (AlternateOutputs.isOn) && (BarSize >= 2 && BarSize <= 4); // Test if we should apply staggered outputs
        if(staggCenter) {
            StyleNode node;

            switch(BarSize) {
                case 2:
                    staggCenter &= (headDict["/Bar/DF/F/DS/L"].gameObject.activeInHierarchy) && (headDict["/Bar/PF/F/DS/R"].gameObject.activeInHierarchy); // Check if the sizes are compatible

                    node = headDict["/Bar/DF/F/DS/L"].lhd.style; // Check if colors are same

                    if(staggCenter) staggCenter &= (node == headDict["/Bar/DF/F/DS/R"].lhd.style);
                    if(staggCenter) staggCenter &= (node == headDict["/Bar/PF/F/DS/L"].lhd.style);
                    if(staggCenter) staggCenter &= (node == headDict["/Bar/PF/F/DS/R"].lhd.style);
                    break;
                case 3:
                    staggCenter &= (headDict["/Bar/DF/F/DS/L"].gameObject.activeInHierarchy) && (headDict["/Bar/PN/F/DS/R"].gameObject.activeInHierarchy) && (headDict["/Bar/PF/F/DS/R"].gameObject.activeInHierarchy); // Check if the sizes are compatible

                    staggCenter &= !(headDict["/Bar/DF/F/DS/L"].hasRealHead || headDict["/Bar/PF/F/DS/R"].hasRealHead); // Only allow alternate numbering if the two far center smalls aren't using real otpics

                    node = headDict["/Bar/DF/F/DS/R"].lhd.style; // Check if colors are same

                    if(staggCenter) staggCenter &= (node == headDict["/Bar/DN/F/DS/L"].lhd.style);
                    if(staggCenter) staggCenter &= (node == headDict["/Bar/DN/F/DS/R"].lhd.style);
                    if(staggCenter) staggCenter &= (node == headDict["/Bar/PF/F/DS/L"].lhd.style);
                    break;
                case 4:
                    staggCenter &= (headDict["/Bar/DF/F/L"].gameObject.activeInHierarchy) && (headDict["/Bar/PF/F/L"].gameObject.activeInHierarchy); // Check if the sizes are compatible
                    staggCenter &= (headDict["/Bar/DN/F/L"].gameObject.activeInHierarchy) && (headDict["/Bar/PN/F/L"].gameObject.activeInHierarchy);

                    node = headDict["/Bar/DF/F/L"].lhd.style; // Check if colors are same

                    if(staggCenter) staggCenter &= (node == headDict["/Bar/DN/F/L"].lhd.style);
                    if(staggCenter) staggCenter &= (node == headDict["/Bar/PN/F/L"].lhd.style);
                    if(staggCenter) staggCenter &= (node == headDict["/Bar/PF/F/L"].lhd.style);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Assign the more static bits
        foreach(LightHead alpha in allHeads) {
            if(!alpha.gameObject.activeInHierarchy) continue; // Inactive head = no bit
            if(alpha.loc == Location.FRONT_CORNER || alpha.loc == Location.REAR_CORNER) { // Corners always 0 or 11
                if(alpha.transform.position.x < 0) {
                    alpha.myBit = 0;
                } else {
                    alpha.myBit = 11;
                }
            } else if(alpha.loc == Location.ALLEY) { // Alleys always 12 or 13
                if(alpha.transform.position.x < 0) {
                    alpha.myBit = 12;
                } else {
                    alpha.myBit = 13;
                }
            } else {
                if(BarSize > 1 && alpha.transform.position.y < 0) continue; // Size 0 and 1 have static rears.  2 thru 4 do not.
                string[] path = alpha.Path.Split('/'); // Get path and split

                switch(path[2]) {
                    case "DE":
                        switch(path[3]) {
                            #region /Bar/DE/FO
                            case "FO":
                                if(alpha.isSmall) {
                                    if(path[5] == "L") // /Bar/DE/FO/DS/L always 1
                                        alpha.myBit = 1;
                                    else {
                                        if(alpha.lhd.style == headDict["/Bar/DE/FO/DS/L"].lhd.style) {
                                            alpha.myBit = 1; // /Bar/DE/FO/DS/R 1 if it matches left style
                                        } else {
                                            alpha.myBit = 4; // /Bar/DE/FO/DS/R 4 if it doesn't match
                                        }
                                    }
                                } else {
                                    alpha.myBit = 1; // /Bar/DE/FO/L always 1
                                }
                                break;
                            #endregion
                            #region /Bar/DE/FI
                            case "FI":
                                if(alpha.isSmall) {
                                    if(path[5] == "R") // /Bar/DE/FI/DS/R always 4
                                        alpha.myBit = 4;
                                    else {
                                        if(alpha.lhd.style == headDict["/Bar/DE/FI/DS/R"].lhd.style) {
                                            alpha.myBit = 4; // /Bar/DE/FI/DS/L 4 if it matches right style
                                        } else {
                                            alpha.myBit = 1; // /Bar/DE/FI/DS/L 1 if it doesn't match
                                        }
                                    }
                                } else {
                                    alpha.myBit = 4; // /Bar/DE/FI/L always 4
                                }
                                break;
                            #endregion
                            case "RO": alpha.myBit = (byte)(alpha.isSmall ? ((path[5] == "L" ? 2 : 3) - BarSize) : 2); break; // Size 0 and 1
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
                                        alpha.FarWire = path[5] == "L"; // Far wire flag set when necessary
                                        if(!alpha.FarWire) { // Right head
                                            if(!staggCenter && alpha.lhd.style == headDict["/Bar/DF/F/DS/L"].lhd.style) {
                                                alpha.myBit = 5; // No staggered output and matching style = /Bar/DF/F/DS/R is 5
                                            } else {
                                                alpha.myBit = 6; // Staggered output or no matching style = /Bar/DF/F/DS/R is 6
                                            }
                                        } else { // Left head
                                            alpha.myBit = 5;
                                        }
                                    } else {
                                        alpha.myBit = 5; // /Bar/DF/F/L is 5
                                        alpha.FarWire = true;
                                    }
                                } else {
                                    alpha.myBit = 5; // /Bar/DF/F/L is 5, no matter the size
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
                                if(BarSize == 4) {
                                    if(!staggCenter && ((headDict["/Bar/DF/F/L"].gameObject.activeInHierarchy && alpha.lhd.style == headDict["/Bar/DF/F/L"].lhd.style) || (headDict["/Bar/DF/F/DS/L"].gameObject.activeInHierarchy && alpha.lhd.style == headDict["/Bar/DF/F/DS/L"].lhd.style && alpha.lhd.style == headDict["/Bar/DF/F/DS/R"].lhd.style))) {
                                        // No staggered output, and /Bar/DN/F/* matches the color of /Bar/DF/F/* (no matter the size)
                                        alpha.myBit = 5;
                                    } else {
                                        alpha.myBit = 6;
                                    }
                                } else {
                                    alpha.myBit = (byte)(alpha.transform.position.x > 0 ? 6 : 5);
                                    alpha.FarWire = (BarSize == 1); // Use far wire only on size 1; forced two shorts on size 1 bars
                                }

                                break;
                            #endregion
                            case "R": alpha.myBit = (byte)(path[path.Length - 1] == "L" ? 5 : 6); break; // Size 1 only
                            default: break;
                        }
                        break;
                    case "PN":
                        switch(path[3]) {
                            #region /Bar/PN/F
                            case "F":
                                if(BarSize == 4) {
                                    if(!staggCenter && ((headDict["/Bar/PF/F/L"].gameObject.activeInHierarchy && alpha.lhd.style == headDict["/Bar/PF/F/L"].lhd.style) || (alpha.lhd.style == headDict["/Bar/PF/F/DS/L"].lhd.style && alpha.lhd.style == headDict["/Bar/PF/F/DS/R"].lhd.style))) {
                                        // No staggered output, and /Bar/PN/F/* matches the color of /Bar/PF/F/* (no matter the size)
                                        alpha.myBit = 6;
                                    } else {
                                        alpha.myBit = 5;
                                    }
                                } else {
                                    alpha.myBit = 6;
                                }
                                break;
                            #endregion
                            default: break;
                        }
                        break;
                    case "PF":
                        switch(path[3]) {
                            #region /Bar/PF/F
                            case "F":
                                if(BarSize == 2) {
                                    if(alpha.isSmall) {
                                        alpha.FarWire = path[5] == "R"; // Far wire flag set when necessary
                                        if(!alpha.FarWire) { // Left head
                                            if(!staggCenter && alpha.lhd.style == headDict["/Bar/PF/F/DS/R"].lhd.style) {
                                                alpha.myBit = 6; // No staggered output and matching style = /Bar/PF/F/DS/L is 6
                                            } else {
                                                alpha.myBit = 5; // Staggered output or no matching style = /Bar/PF/F/DS/L is 5
                                            }
                                        } else {
                                            alpha.myBit = 6;
                                        }
                                    } else {
                                        alpha.myBit = 6; // /Bar/PF/F/L is 6
                                        alpha.FarWire = true;
                                    }
                                } else {
                                    alpha.myBit = 6; // /Bar/PF/F/L is 6, no matter the size
                                    alpha.FarWire = true;
                                }
                                break;
                            #endregion
                            default: break;
                        }
                        break;
                    case "PE":
                        switch(path[3]) {
                            #region /Bar/PE/FO
                            case "FO":
                                if(alpha.isSmall) {
                                    if(path[5] == "R") // /Bar/PE/FO/DS/R always 10
                                        alpha.myBit = 10;
                                    else {
                                        if(alpha.lhd.style == headDict["/Bar/PE/FO/DS/R"].lhd.style) {
                                            alpha.myBit = 10; // /Bar/PE/FO/DS/L 10 if it matches right style
                                        } else {
                                            alpha.myBit = 7; // /Bar/PE/FO/DS/L 7 if it doesn't match
                                        }
                                    }
                                } else {
                                    alpha.myBit = 10; // /Bar/PE/FO/L always 10
                                }
                                break;
                            #endregion
                            #region /Bar/PE/FI
                            case "FI":
                                if(alpha.isSmall) {
                                    if(path[5] == "L") // /Bar/PE/FI/DS/L always 7
                                        alpha.myBit = 7;
                                    else {
                                        if(alpha.lhd.style == headDict["/Bar/PE/FI/DS/L"].lhd.style) {
                                            alpha.myBit = 7; // /Bar/PE/FI/DS/R 7 if it matches left style
                                        } else {
                                            alpha.myBit = 10; // /Bar/PE/FI/DS/R 10 if it doesn't match
                                        }
                                    }
                                } else {
                                    alpha.myBit = 7; // /Bar/PE/FI/L always 7
                                }
                                break;
                            #endregion
                            case "RO": alpha.myBit = (byte)(alpha.isSmall ? ((path[5] == "L" ? 8 : 9) + BarSize) : 9); break; // Size 0 and 1
                            case "RI": alpha.myBit = (byte)(alpha.isSmall ? ((path[5] == "L" ? 6 : 7) + BarSize) : 7); break;
                            default: break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        #region Dynamic rears
        if(BarSize > 1) {
            List<LightHead> heads = new List<LightHead>(10); // Ready some variables
            List<RaycastHit> test = new List<RaycastHit>(Physics.RaycastAll(new Vector3(0, -1.25f), new Vector3(-1f, 0))); // Raycast from rear center toward driver
            RaycastHit far; LightHead farHead;

            #region Give driver-far head bit 2 if large
            if(td == TDOption.NONE) {
                far = test[0]; // Find farthest head
                foreach(RaycastHit alpha in test) {
                    if(far.transform != alpha.transform && far.distance < alpha.distance)
                        far = alpha;
                }

                farHead = far.transform.GetComponent<LightHead>();
                if(!farHead.isSmall) {
                    farHead.myBit = 2; // Make farthest head 2 if large
                }
            }
            #endregion

            byte bit = 5; // Start at bit 5
            #region Test for center-straddling head
            RaycastHit center;
            if(Physics.Raycast(new Vector3(0, 0), new Vector3(0, -1), out center)) { // If there's a (long) head dead center
                //(  note: RaycastHit List test won't have this head, Physics.RaycastAll only tracks what Colliders it *enters*, not exits  )
                LightHead alpha = center.transform.GetComponent<LightHead>();
                if(alpha.hasRealHead) { // If it uses a real head
                    alpha.myBit = 5; // Give it bit 5
                    bit = 4; // Start at 4
                } else {
                    alpha.myBit = 255; // Take its bit away
                }
            }
            #endregion

            #region Remove undefined / Block Off heads
            bool cont = true;
            while(cont && test.Count > 0) {
                for(int i = 0; i < test.Count; i++) {
                    LightHead testHead = test[i].transform.GetComponent<LightHead>();
                    if(!testHead.hasRealHead) {
                        testHead.myBit = 255;
                        test.RemoveAt(i);
                        break; // Need to break, else we'll get a ConcurrentModificationException
                    }
                    if(i == test.Count - 1) {
                        cont = false;
                        break;
                    }
                }
            }
            #endregion

            #region Compile List of ordered LightHeads
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
            #endregion

            #region Assign bits
            for(int i = 0; i < heads.Count; i++) {
                if(!heads[i].hasRealHead) { // Extra catch for undefined heads
                    heads[i].myBit = 255;
                    continue;
                }
                if(heads[i].shouldBeTD) { // Head should be a traffic head, always give it its own bit
                    heads[i].myBit = bit--;
                } else if(bit == 1) { // Down to last bit, remaining heads get it
                    heads[i].myBit = 1;
                    if(heads[i].lhd.style.isDualColor) { // Bit 1 is a single-color-only output.  Drop any that have duals
                        heads[i].SetOptic("");
                        heads[i].useDual = false;
                    }
                } else {
                    if(heads.Count - i > bit) { // More heads left than remaining bits to give
                        if(heads[i].isSmall ^ heads[i + 1].isSmall) { // If sizes don't match
                            heads[i].myBit = bit--; // Don't share
                        } else {
                            heads[i].myBit = bit; // Share
                            heads[++i].myBit = bit--;
                        }
                    } else {
                        heads[i].myBit = bit--; // Enough bits left to give remaining heads their own
                    }
                }
            }
            #endregion

            #region Cleanup
            if(bit == 1) {
                test = new List<RaycastHit>(Physics.RaycastAll(new Vector3(heads[heads.Count - 1].transform.position.x, -1.25f), new Vector3(-1f, 0)));
                if(test.Count > 0) { // Have undefined heads to the driver-side
                    foreach(RaycastHit hit in test) {
                        hit.transform.GetComponent<LightHead>().myBit = 1; // Give 'em a bit anyway
                    }
                }
            }
            #endregion



            heads.Clear(); // Reprep for passenger side
            test.Clear();
            test.AddRange(Physics.RaycastAll(new Vector3(0, -1.25f), new Vector3(1f, 0)));
            #region Give passenger-far head bit 9 if large
            if(td == TDOption.NONE) {
                far = test[0]; // Find farthest head
                foreach(RaycastHit alpha in test) {
                    if(far.transform != alpha.transform && far.distance < alpha.distance)
                        far = alpha;
                }

                farHead = far.transform.GetComponent<LightHead>();
                if(!farHead.isSmall) {
                    farHead.myBit = 9; // Make farthest head 9 if large
                }
            }
            #endregion

            bit = 6; // Start at bit 6
            #region Test for center-straddling head
            if(Physics.Raycast(new Vector3(0, 0), new Vector3(0, -1))) {
                bit = 7; // The head already has a bit (5) - just skip to next one
            }
            #endregion

            #region Remove undefined / Block Off heads
            cont = true;
            while(cont && test.Count > 0) {
                for(int i = 0; i < test.Count; i++) {
                    LightHead testHead = test[i].transform.GetComponent<LightHead>();
                    if(!testHead.hasRealHead) {
                        testHead.myBit = 255;
                        test.RemoveAt(i);
                        break; // Need to break, else we'll get a ConcurrentModificationException
                    }
                    if(i == test.Count - 1) {
                        cont = false;
                        break;
                    }
                }
            }
            #endregion

            #region Compile List of ordered LightHeads
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
            #endregion

            #region Assign bits
            for(int i = 0; i < heads.Count; i++) {
                if(!heads[i].hasRealHead) { // Extra catch for undefined heads
                    heads[i].myBit = 255;
                    continue;
                }
                if(heads[i].shouldBeTD) { // Head should be a traffic head, always give it its own bit
                    heads[i].myBit = bit++;
                } else if(bit == 10) { // Down to last bit, remaining heads get it
                    heads[i].myBit = 10;
                    if(heads[i].lhd.style.isDualColor) { // Bit 10 is a single-color-only output.  Drop any that have duals
                        heads[i].SetOptic("");
                        heads[i].useDual = false;
                    }
                } else {
                    if(heads.Count - i > (11 - bit)) { // More heads left than remaining bits to give
                        if(heads[i].isSmall ^ heads[i + 1].isSmall) { // If sizes don't match
                            heads[i].myBit = bit++; // Don't share
                        } else {
                            heads[i].myBit = bit; // Share
                            heads[++i].myBit = bit++;
                        }
                    } else {
                        heads[i].myBit = bit++; // Enough bits left to give remaining heads their own
                    }
                }
            }
            #endregion

            #region Cleanup
            if(bit == 10) {
                test.Clear();
                test.AddRange(Physics.RaycastAll(new Vector3(heads[heads.Count - 1].transform.position.x, -1.25f), new Vector3(1f, 0)));
                if(test.Count > 0) { // Have undefined heads to the passenger-side
                    foreach(RaycastHit hit in test) {
                        hit.transform.GetComponent<LightHead>().myBit = 10; // Give 'em a bit anyway
                    }
                }
            }
            #endregion

            LightHead centermost;

            #region Switch outputs around for staggers
            if(BarSize == 2) {
                test.Clear();
                test.AddRange(Physics.RaycastAll(new Vector3(-10f, -1.25f), new Vector3(1f, 0f)));
                if(test.Count == 12) { // If there's 12 heads in rear (aka all small)
                    centermost = headDict["/Bar/DF/R/DS/R"];
                    if(centermost.lhd.style != headDict["/Bar/DF/R/DS/L"].lhd.style) { // Stagger if rears don't match
                        centermost.myBit = 6;
                    }
                    centermost = headDict["/Bar/PF/R/DS/L"];
                    if(centermost.lhd.style != headDict["/Bar/PF/R/DS/R"].lhd.style) {
                        centermost.myBit = 5;
                    }
                }
            } else if(BarSize == 4) {
                test.Clear();
                test.AddRange(Physics.RaycastAll(new Vector3(-10f, -1.25f), new Vector3(1f, 0f)));
                if(test.Count == 16) { // If there's 16 heads in rear (aka all small)
                    centermost = headDict["/Bar/DF/R/DS/R"];
                    if(centermost.lhd.style != headDict["/Bar/DF/R/DS/L"].lhd.style) { // Stagger if rears don't match
                        centermost.myBit = 5;
                    }
                    centermost = headDict["/Bar/DN/R/DS/L"];
                    if(centermost.lhd.style != headDict["/Bar/DN/R/DS/R"].lhd.style) {
                        centermost.myBit = 4;
                    }
                    centermost = headDict["/Bar/PN/R/DS/R"];
                    if(centermost.lhd.style != headDict["/Bar/PN/R/DS/L"].lhd.style) {
                        centermost.myBit = 7;
                    }
                    centermost = headDict["/Bar/PF/R/DS/L"];
                    if(centermost.lhd.style != headDict["/Bar/PF/R/DS/R"].lhd.style) {
                        centermost.myBit = 6;
                    }
                }
            }
            #endregion
        }
        #endregion


        RefreshingBits = false;
        yield return StartCoroutine(RefreshAllLabels()); // Refresh Labels
        yield return null;
    }

    /// <summary>
    /// Begins saving the bar.  Called via Save Button on File Menu.
    /// </summary>
    public void BeginSave() {
        savePDF = false;
    }

    /// <summary>
    /// Save bar to specified filename.  Called via callback from File Browser.
    /// </summary>
    /// <param name="filename">Where to save the bar to</param>
    public void Save(string filename) {
        #region Strip known file extensions
        if(filename.EndsWith(".bar.nbt")) {
            filename = filename.Substring(0, filename.Length - 8);
        }
        if(filename.EndsWith(".pdf")) {
            filename = filename.Substring(0, filename.Length - 4);
        } 
        #endregion

        try {
            NbtCompound root = new NbtCompound("root"); // Generate root tag

            #region Stash bar options (size, TD option, CAN, cable type & length)
            NbtCompound opts = new NbtCompound("opts");
            opts.Add(new NbtByte("size", (byte)BarSize));
            opts.Add(new NbtByte("tdop", (byte)td));
            opts.Add(new NbtByte("can", (byte)(useCAN ? 1 : 0)));
            opts.Add(new NbtByte("cabt", (byte)cableType));
            opts.Add(new NbtByte("cabl", (byte)cableLength));
            opts.Add(new NbtByte("mkit", (byte)mountingKit));
            root.Add(opts); 
            #endregion

            #region Stash order information (cust name, order num, notes)
            NbtCompound order = new NbtCompound("ordr");
            order.Add(new NbtString("name", custName.text));
            order.Add(new NbtString("num", orderNum.text));
            order.Add(new NbtString("note", notes.text));
            root.Add(order); 
            #endregion

            #region Stash light head information
            NbtList lightList = new NbtList("lite");
            foreach(LightHead lh in allHeads) {
                if(!lh.gameObject.activeInHierarchy) continue; // Only save information about active lights
                NbtCompound lightCmpd = new NbtCompound();
                lightCmpd.Add(new NbtString("path", lh.Path)); // Save head's path
                if(lh.lhd.style != null) { // Only save other head information if it contains a fully defined head
                    lightCmpd.Add(new NbtString("optc", lh.lhd.optic.partNumber));
                    lightCmpd.Add(new NbtString("styl", lh.lhd.style.name));
                }

                byte fn = 0;
                foreach(BasicFunction bfn in lh.lhd.funcs) {
                    fn |= (byte)bfn;
                }
                lightCmpd.Add(new NbtByte("func", fn)); // Save function list as bit field

                lightList.Add(lightCmpd);
            }
            root.Add(lightList); 
            #endregion

            root.Add(patts.Clone()); // Stash pattern information as a clone (otherwise pattern tag might have two parents)

            #region Stash Size Option Control information (whether heads are large or small)
            NbtList socList = new NbtList("soc");
            foreach(SizeOptionControl soc in transform.GetComponentsInChildren<SizeOptionControl>(true)) {
                NbtCompound socCmpd = new NbtCompound();
                socCmpd.Add(new NbtString("path", soc.transform.GetPath()));
                socCmpd.Add(new NbtByte("isLg", soc.ShowLong ? (byte)1 : (byte)0));
                socList.Add(socCmpd);
            }
            root.Add(socList); 
            #endregion

            #region Stash Lens information
            NbtList lensList = new NbtList("lens");
            foreach(BarSegment seg in allSegs) {
                NbtCompound segCmpd = new NbtCompound();
                segCmpd.Add(new NbtString("path", seg.transform.GetPath()));
                segCmpd.Add(new NbtString("part", seg.lens.partSuffix));
                lensList.Add(segCmpd);
            }
            root.Add(lensList); 
            #endregion

            NbtFile file = new NbtFile(root); // Create file to save

            if(savePDF) {
                Directory.CreateDirectory(filename);
                file.SaveToFile(filename + "\\Bar Savefile.bar.nbt", NbtCompression.None); // Save file
                StartCoroutine(SavePDF(filename + "\\Bar Information.pdf")); // Save PDF too, if needed
            } else {
                file.SaveToFile(filename + ".bar.nbt", NbtCompression.None); // Save file
            }

            if(quitAfterSave) { Application.Quit(); } // If user wanted to quit and chose to save, quit now

            moddedBar = false; // Bar is saved, can quit without repercussion
            TitleText.inst.currFile = filename; // Change title text
        } catch(Exception ex) {
            ErrorText.inst.DispError("Problem saving: " + ex.Message); // We had problem
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// Open bar from specified filename.  Called via callback from File Browser and presets.
    /// </summary>
    /// <param name="filename">Where to load the bar from</param>
    public void Open(string filename) {
        StartCoroutine(OpenIEnum(filename));
    }

    /// <summary>
    /// Coroutine.  Loads bar information from a file.
    /// </summary>
    /// <param name="filename">Where to load the bar from</param>
    public IEnumerator OpenIEnum(string filename) {
        Clear();

        NbtFile file = new NbtFile(filename); // Load file

        NbtCompound root = file.RootTag; // Extract root tag

        #region Extract bar options (size, TD option, CAN, cable type & length)
        NbtCompound opts = root.Get<NbtCompound>("opts");
        SetBarSize(opts["size"].IntValue, false);
        SetTDOption((TDOption)opts["tdop"].ByteValue);
        useCAN = opts["can"].ByteValue == 1;
        cableType = opts["cabt"].IntValue;
        cableLength = opts["cabl"].IntValue;
        if(opts.Contains("mkit")) mountingKit = opts["mkit"].IntValue; else mountingKit = 0;
        #endregion

        yield return StartCoroutine(RefreshBitsIEnum()); // Make sure that Traffic Director and Bar Size information get squared away, align the bits

        #region Extract order information (cust name, order num, notes)
        NbtCompound order = root.Get<NbtCompound>("ordr");
        custName.text = order["name"].StringValue;
        orderNum.text = order["num"].StringValue;
        notes.text = order["note"].StringValue; 
        #endregion

        NbtList lightList = (NbtList)root["lite"]; // Extracting other information before applying
        NbtList socList = (NbtList)root["soc"];
        NbtList lensList = (NbtList)root["lens"];
        Dictionary<string, LightHead> lights = new Dictionary<string, LightHead>();
        Dictionary<string, SizeOptionControl> socs = new Dictionary<string, SizeOptionControl>();

        #region Path caching
        foreach(LightHead lh in allHeads) { // Cache path of all light heads
            lights[lh.Path] = lh;
        }
        foreach(SizeOptionControl soc in transform.GetComponentsInChildren<SizeOptionControl>(true)) { // Cache path of all SOCs
            socs[soc.transform.GetPath()] = soc;
        } 
        #endregion
        List<NbtTag> stts = new List<NbtTag>(); // Prepare list of Stop Tail Turns (need to be applied last)
        #region Light head application
        foreach(NbtTag alpha in lightList) {
            NbtCompound lightCmpd = alpha as NbtCompound;
            LightHead lh = lights[lightCmpd["path"].StringValue]; // Find the light head necessary

            byte fn = lightCmpd["func"].ByteValue;
            lh.lhd.funcs.Clear();
            foreach(BasicFunction bfn in lh.CapableBasicFunctions) {
                if(((byte)bfn & fn) != 0) {
                    lh.lhd.funcs.Add(bfn); // Add all of the functions
                }
            }

            if(lh.lhd.funcs.Contains(BasicFunction.STT)) { // If head is a Stop Tail Turn, wait until the end to apply it
                stts.Add(alpha);
                continue;
            }

            if(lightCmpd.Contains("optc")) { // Head was defined when saved
                LocationNode ln = LightDict.inst.FetchLocation(lh.loc);
                string partNum = lightCmpd["optc"].StringValue;

                foreach(OpticNode on in ln.optics.Values) {
                    if(on.partNumber == partNum) { // Found optic
                        lh.SetOptic(on.name, false); // Apply optic and style
                        lh.SetStyle(lightCmpd["styl"].StringValue);
                        break;
                    }
                }
            }

            lh.TestSingleDual(); // Double-check single/dual
        } 
        #endregion

        #region Extract pattern information
        patts = root.Get<NbtCompound>("pats"); // Capture pattern tag
        if(!patts.Get<NbtCompound>("traf").Contains("ctd")) { // Older savefile, doesn't have cycles TD or cycles Warn shorts
            patts.Get<NbtCompound>("traf").AddRange(new NbtTag[] { new NbtShort("ctd", 0), new NbtShort("cwn", 0) }); // Get placeholders in
        }
        TDCyclesSliders tdcs = GameObject.Find("UI/Canvas/LightInteractionPanel/Panes/FuncEdit/TrafficOpts").GetComponent<TDCyclesSliders>();
        tdcs.FetchTags();
        tdcs.Refresh();
        FnDragTarget.inputMap = patts.Get<NbtIntArray>("map"); 
        #endregion

        #region SOC information application
        foreach(NbtTag alpha in socList) {
            NbtCompound socCmpd = alpha as NbtCompound;
            SizeOptionControl soc = socs[socCmpd["path"].StringValue];
            soc.ShowLong = (socCmpd["isLg"].ByteValue == 1);
        } 
        #endregion

        // Refresh bits again, because we've now applied almost everything and just want to make sure STTs stick
        yield return StartCoroutine(RefreshBitsIEnum());

        #region Apply Stop Tail Turns
        if(stts.Count > 0) {
            foreach(NbtTag alpha in stts) {
                NbtCompound lightCmpd = alpha as NbtCompound;
                LightHead lh = lights[lightCmpd["path"].StringValue];

                //Skip adding basic functions, done already

                if(lightCmpd.Contains("optc")) { // Head was fully defined when saved
                    LocationNode ln = LightDict.inst.FetchLocation(lh.loc);
                    string partNum = lightCmpd["optc"].StringValue;

                    foreach(OpticNode on in ln.optics.Values) {
                        if(on.partNumber == partNum) { // Found optic
                            lh.SetOptic(on.name, false); // Apply optic and style
                            lh.SetStyle(lightCmpd["styl"].StringValue);
                            break;
                        }
                    }
                }

                lh.TestSingleDual(); // Double-check single/dual
            }

            // Refresh bits a third time, because we applied more heads
            yield return StartCoroutine(RefreshBitsIEnum());
        } 
        #endregion

        #region Apply lenses
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
        #endregion

        FindObjectOfType<CameraControl>().RefreshOnSelect.Invoke(); // Have CameraControl refresh things
        moddedBar = false; // Just loaded bar, no need to try to save when closing
        #region Apply Title Text stuff
        if(TitleText.inst.preset.Length > 0) {
            TitleText.inst.currFile = "";
        } else {
            TitleText.inst.currFile = filename;
        } 
        #endregion
    }

    /// <summary>
    /// Begins the PDF saving.  Called by the "Save Order/Quote" button
    /// </summary>
    public void StartPDF() {
        savePDF = true;
        barFilePath = fb.currFile; // Stash bar file path for later

        Directory.CreateDirectory(DirRoot + "Lightbar Drawings");  // Create Lightbar Drawing folder and start there
        fb.currFile = "";
        fb.Navigate(DirRoot + "Lightbar Drawings");
        fb.fileFieldText = custName.text + "_" + (System.Environment.MachineName) + "_" + DateTime.Now.ToString("yyMMddHHmmssf");  // Auto-fill file name
    }

    /// <summary>
    /// Begins PDF Preview saving.  Called by the "Preview PDF" button
    /// </summary>
    public void JustSavePDF() {
        Directory.CreateDirectory(DirRoot + "output");
        StartCoroutine(SavePDF(DirRoot + "output/" + (System.Environment.MachineName) + " Preview.pdf"));
    }

    /// <summary>
    /// Coroutine.  Saves the PDF, spawning an external thread to do so.
    /// </summary>
    /// <param name="filename">Where to save the PDF to</param>
    public IEnumerator SavePDF(string filename) {
        bool attempt = true;
        #region Test if we can delete file (and thus save)
        try {
            File.Delete(filename);
        } catch(IOException) {
            ErrorText.inst.DispError("Problem saving the PDF.  Do you still have it open?");
            attempt = false;
        } 
        #endregion

        if(attempt) {
            #region Hide UI stuff
            progressStuff.Shown = false;
            progressStuff.Progress = 0;
            CameraControl.ShowWhole = true;
            CanvasDisabler.CanvasEnabled = false; 
            #endregion

            #region Capture rectangle
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
            #endregion

            // Capture images
            yield return StartCoroutine(CapImages(capRect));

            progressStuff.Shown = true;
            progressStuff.Text = "Finished capturing images.";

            PDFExportJob pej = new PDFExportJob();

            pej.Start(filename); // Begin Async export
            while(!pej.Update()) { // Continually check in, make sure it's finished before continuing
                yield return null;
            }
        }

        yield return null;
    }

    /// <summary>
    /// Coroutine.  Captures images of the bar.
    /// </summary>
    /// <param name="capRect">The rectangle to capture</param>
    public IEnumerator CapImages(Rect capRect) {
        RefreshCurrentHeads(); // Make sure the labels get their numbers right

        Camera cam = FindObjectOfType<CameraControl>().GetComponent<Camera>();

        #region Capture descriptive image (more descriptive comments, rest follow same workflow)
        bool debugBit = LightLabel.showBit;
        LightLabel.showBit = false;
        LightLabel.showParts = false;
        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) { // Gets light labels ready for capture
            alpha.DispError = false;
            alpha.Refresh(true);
        }
        foreach(LensLabel alpha in FindObjectsOfType<LensLabel>()) { // Gets lens labels ready for capture
            alpha.Refresh();
        }
        Texture2D tex = new Texture2D(Mathf.RoundToInt(capRect.width), Mathf.RoundToInt(capRect.height)); // Creates a new texture
        yield return new WaitForEndOfFrame(); // Wait until the right moment to capture
        tex.ReadPixels(capRect, 0, 0); // Capture
        tex.Apply();

        Directory.CreateDirectory("tempgen"); // Create folder to store images in
        using(FileStream imgOut = new FileStream("tempgen\\desc.png", FileMode.OpenOrCreate)) { // Open a fresh file to dump image in
            byte[] imgbytes = tex.EncodeToPNG(); // Encode image
            imgOut.Write(imgbytes, 0, imgbytes.Length); // Dump
        } 
        #endregion

        #region Capture bit image
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
        #endregion

        #region Capture part number image
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
        #endregion

        #region Capture wiring image
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
        #endregion

        #region Capture colorless wiring image
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
        #endregion

        #region Reset
        LightLabel.showBit = debugBit;

        cam.orthographicSize = cam.GetComponent<CameraControl>().partialOrtho;

        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh();
        }
        foreach(LensLabel alpha in FindObjectsOfType<LensLabel>()) {
            alpha.Refresh();
        } 
        #endregion

    }

    /// <summary>
    /// Clears the bar out.  Called by either of the clear bar buttons and by load
    /// </summary>
    public void Clear() {
        #region Clear out heads
        foreach(LightHead lh in allHeads) {
            lh.lhd.funcs.Clear();
            lh.RefreshBasicFuncDefault();
            if(lh.myLabel != null) {
                lh.myLabel.DispError = false;
                lh.myLabel.Refresh();
            }
        } 
        #endregion
        #region Make heads long
        foreach(SizeOptionControl soc in transform.GetComponentsInChildren<SizeOptionControl>(true))
            soc.ShowLong = true; 
        #endregion
        #region Default lenses
        foreach(Lens opt in LightDict.inst.lenses) {
            if(opt.partSuffix == "C-C") {
                foreach(BarSegment seg in transform.GetComponentsInChildren<BarSegment>(true)) {
                    seg.lens = opt;
                }
            }
        } 
        #endregion

        TitleText.inst.currFile = ""; // Clear out Title Text
        TitleText.inst.preset = "";

        td = TDOption.NONE; // Default options
        useCAN = false;
        cableLength = cableType = 0;

        moddedBar = false; // Nothing to save anymore

        RefreshBitsIEnum(); // Refresh bits

        CreatePatts(); // Wipe pattern
    }

    /// <summary>
    /// Refreshes the array of heads ordered by position
    /// </summary>
    public void RefreshCurrentHeads() {
        List<LightHead> headList = new List<LightHead>(50); // List of all active heads

        RaycastHit info;
        if(Physics.Raycast(new Ray(first.transform.position, new Vector3(1, 0.5f)), out info)) { // Can find second head
            headList.Add(first); // Add first head
            LightHead curr = info.transform.GetComponent<LightHead>(); // Get second head from information
            Ray ray;
            while(curr != first && headList.Count < 50) {
                headList.Add(curr); // Add next head
                switch(curr.loc) { // Figure out where it needs to cast next to find next head
                    case Location.FRONT_CORNER:
                        if(curr.transform.position.x < 0) {
                            ray = new Ray(curr.transform.position, new Vector3(1, 0.5f));
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
                if(Physics.Raycast(ray, out info)) // Cast and search
                    if(curr == first) break; // Looped back, now stop
                    else curr = info.transform.GetComponent<LightHead>(); // Ready for next head
                else
                    break; // Stop, could not find next head
            }
        }

        headNumber = headList.ToArray(); // Save heads
    }

    /// <summary>
    /// Perform the auto-phase function.  Uses Color 1 of driver front corner as definition of Phase A.
    /// </summary>
    public void AutoPhase() {
        string AStyle = first.lhd.style != null ? first.lhd.style.name.Split('\\', '/')[0] : ""; // Gets name of first color

        if(string.IsNullOrEmpty(AStyle)) {
            ErrorText.inst.DispError("Define the color that's using Phase A on the front driver's corner light, first."); // Mandatory light is not defined, cannot continue
            return;
        }

        string[] alphaStyles = new string[2];
        foreach(LightHead alpha in allHeads) {
            if(alpha.gameObject.activeInHierarchy && alpha.hasRealHead && alpha.lhd.funcs.Contains(BasicFunction.FLASHING)) {
                alphaStyles = alpha.lhd.style != null ? alpha.lhd.style.name.Split('\\', '/') : new string[2];
                alpha.basicPhaseA = (alphaStyles[0].Equals(AStyle, StringComparison.CurrentCultureIgnoreCase)); // Color 1 should always check to see if it matches
                if(alphaStyles.Length > 1) {
                    alpha.basicPhaseA2 = (alpha.isRear ? (alphaStyles[1].Equals(AStyle, StringComparison.CurrentCultureIgnoreCase)) : !alpha.basicPhaseA); // Color 2 should check if it matches in the front only.  Rear heads can only be "not A"
                }
            }
        }
    }

    /// <summary>
    /// Coroutine.  Refreshes all of the labels.
    /// </summary>
    public IEnumerator RefreshAllLabels() {
        yield return new WaitForEndOfFrame();

        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh();
        }

        yield return null;
    }

    /// <summary>
    /// Sets things up to begin previewing.  Called by the Begin Preview button.
    /// </summary>
    public void BeginPreview() {
        funcBeingTested = FunctionEditPane.currFunc;
        FindObjectOfType<CameraControl>().SelectedHead.Clear();
        CameraControl.ShowWhole = true;
        CanvasDisabler.CanvasEnabled = false;
        PattTimer.inst.StartTimer();
        StartCoroutine(RefreshAllLabels());
    }

    /// <summary>
    /// Ends preview.  Called by the End Preview button or by attempting to close application.
    /// </summary>
    public void EndPreview() {
        PattTimer.inst.StopTimer();
        CameraControl.ShowWhole = false;
        CanvasDisabler.CanvasEnabled = true;
        funcBeingTested = AdvFunction.NONE;
        StartCoroutine(RefreshAllLabels());
    }

    /// <summary>
    /// Set things up to quit after saving.  Called by the Save and Quit button on the Quit Dialog.
    /// </summary>
    public void SaveAndQuit() {
        quitAfterSave = true;
        fb.BeginSave();
    }

    /// <summary>
    /// Just quits.  Called by the Discard and Quit button on the Quit Dialog.
    /// </summary>
    public void ForceQuit() {
        forceQuit = true;
        Application.Quit();
    }

    /// <summary>
    /// Quit callback, called by Unity when user is trying to quit the application
    /// </summary>
    public void OnApplicationQuit() {
        if(fb.gameObject.activeInHierarchy) { // If file browser open, just close it
            Application.CancelQuit();
            fb.gameObject.SetActive(false);
            return;
        }
        if(funcBeingTested != AdvFunction.NONE) { // If previewing, stop preview
            Application.CancelQuit();
            EndPreview();
            return;
        }
        if(!forceQuit && moddedBar) { // If bar is not saved and trying to quit, show quit dialog
            Application.CancelQuit();
            quitDialog.SetActive(true);
            return;
        }
    }

}

/// <summary>
/// Base class for any job that needs to be done on a separate thread
/// </summary>
public class ThreadedJob {
    /// <summary>
    /// Is the job done?
    /// </summary>
    private bool m_IsDone = false;
    /// <summary>
    /// The handle for the thread.
    /// </summary>
    private object m_Handle = new object();
    /// <summary>
    /// The thread doing the work.
    /// </summary>
    protected System.Threading.Thread m_Thread = null;
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

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    public void Start() {
        m_Thread = new System.Threading.Thread(Run);
        m_Thread.Start();
    }
    /// <summary>
    /// Aborts this job.
    /// </summary>
    public virtual void Abort() {
        m_Thread.Abort();
    }

    /// <summary>
    /// Thread's function
    /// </summary>
    protected virtual void ThreadFunction() { }

    /// <summary>
    /// Finalization of the job - performed on Unity thread
    /// </summary>
    protected virtual void OnFinished() { }

    /// <summary>
    /// Unity check-in method - performed on Unity thread
    /// </summary>
    /// <returns>True if the method's done</returns>
    public virtual bool Update() {
        if(IsDone) {
            OnFinished();
            return true;
        }
        return false;
    }
    /// <summary>
    /// Generic "do things" function.
    /// </summary>
    private void Run() {
        ThreadFunction();
        IsDone = true;
    }
}

/// <summary>
/// This is the class that handles the asynchronous production of the PDFs.
/// </summary>
public class PDFExportJob : ThreadedJob {
    /// <summary>
    /// Provided customer name
    /// </summary>
    public string custName;
    /// <summary>
    /// Provided order number
    /// </summary>
    public string orderNumber;
    /// <summary>
    /// Provided order notes
    /// </summary>
    public string notes;
    /// <summary>
    /// The description of the current state of production
    /// </summary>
    public string progressText = "...";
    /// <summary>
    /// A value representing the current progress of production.  Valid from 0 to 100.
    /// </summary>
    public float progressPercentage = 0f;
    /// <summary>
    /// A reference to the NbtCompound pattern bytes.  Cloned at the start.
    /// </summary>
    public NbtCompound patts;
    /// <summary>
    /// Is this bar using CAN?
    /// </summary>
    public bool useCAN = false;
    /// <summary>
    /// The model number of the bar
    /// </summary>
    public string BarModel = "";
    /// <summary>
    /// The human-friendly width of the bar
    /// </summary>
    public string BarWidth = "";
    /// <summary>
    /// The path to which the file will be written
    /// </summary>
    public string filename = "";
    /// <summary>
    /// The rectangle used to capture the images
    /// </summary>
    public Rect capRect;
    /// <summary>
    /// A list of issues that were found with the bar
    /// </summary>
    public List<String> issues;
    /// <summary>
    /// A referece to the Progress Stuff that is used to display progress to the screen
    /// </summary>
    public BarManager.ProgressStuff progressStuff;
    /// <summary>
    /// A list of all of the heads
    /// </summary>
    public LightHead[] headNumber;
    /// <summary>
    /// Dictionary containing all of the wires in use
    /// </summary>
    public Dictionary<LightHead, string> color1Wire, color2Wire;
    /// <summary>
    /// Reference to BOMCables object, so it can write its own things to the PDF as well
    /// </summary>
    public BOMCables bomcables;
    /// <summary>
    /// Reference to the Mounting Kit Option in use
    /// </summary>
    public MountingKitOption mntOpt;
    /// <summary>
    /// Total current draw, in whole milliamps
    /// </summary>
    public uint ampTotal;
    /// <summary>
    /// Total sale price of the bar, in whole cents
    /// </summary>
    public uint costTotal;
    /// <summary>
    /// Cost of the base subassembly alone, in whole cents
    /// </summary>
    public uint barCost;

    /// <summary>
    /// If an exception occured while producing the PDF, it's stored here so it can be processed on Unity's thread
    /// </summary>
    public Exception thrownExcep;

    /// <summary>
    /// Initializes the job.  Gathers the information it needs before handing it to a new thread.
    /// </summary>
    /// <param name="fname">Where are we saving the file?</param>
    public void Start(string fname) {
        #region Information fetching
        BarManager bm = BarManager.inst; // Gathering all of the information we need from the rest of the application
        custName = bm.custName.text;
        orderNumber = bm.orderNum.text;
        notes = bm.notes.text;
        useCAN = BarManager.useCAN;
        patts = bm.patts.Clone() as NbtCompound; // Create a clone of the pattern tag
        progressStuff = bm.progressStuff;
        BarModel = bm.BarModel;
        BarWidth = bm.BarWidth;
        barCost = bm.BarPrice;
        filename = fname;
        if(BarManager.moddedBar) {
            issues = new List<string>(); // Get the issue list only if it's gonna show in the first place
            foreach(IssueChecker issue in bm.issues) {
                if(issue.DoCheck()) {
                    issues.Add(issue.pdfText);
                }
            }
        }
        headNumber = BarManager.headNumber;
        color1Wire = new Dictionary<LightHead, string>();
        color2Wire = new Dictionary<LightHead, string>();
        if(BarManager.mountingKit != 0)
            mntOpt = LightDict.inst.mountKits[BarManager.mountingKit - 1];
        bomcables = GameObject.FindObjectOfType<BOMCables>();
        if(bomcables == null)
            Debug.LogError("Couldn't find BOMCable Object");
        try {
            ampTotal = GameObject.FindObjectOfType<AmperageTotal>().totalAmp;
            costTotal = GameObject.FindObjectOfType<CostTotaler>().total;
        } catch(NullReferenceException) {
            Debug.LogError("Couldn't find AmperageTotal or CostTotaler Object");
        }
        #endregion

        foreach(LightHead alpha in headNumber) {
            alpha.PrefetchPatterns(); // Have each head cache what pattern its using
            if(alpha.hasRealHead) {
                color1Wire[alpha] = BarManager.GetWireColor1(alpha); // Precache each head's wire
                if(alpha.lhd.style.isDualColor) color2Wire[alpha] = BarManager.GetWireColor2(alpha);
            }
        }

        #region Capture Rectangle production
        Camera cam = GameObject.FindObjectOfType<CameraControl>().GetComponent<Camera>();

        //Find two reference points and make a rectangle out of 'em
        Vector3 tl = Vector3.zero, br = Vector3.zero;
        foreach(ReferencePoint rp in GameObject.FindObjectsOfType<ReferencePoint>()) {
            if(rp.gameObject.name == "tl") {
                tl = cam.WorldToScreenPoint(rp.transform.position);
            } else if(rp.gameObject.name == "br") {
                br = cam.WorldToScreenPoint(rp.transform.position);
            }
        }

        capRect = new Rect(tl.x, br.y, br.x - tl.x, tl.y - br.y); // Produce the capture rectangle for scaling's use 
        #endregion

        base.Start(); // Actually make the separate thread
        m_Thread.Name = "PDF Export Thread"; // Name other thread for debugging purposes
    }

    /// <summary>
    /// Unity check-in method - performed on Unity thread
    /// </summary>
    /// <returns>True if the method's done</returns>
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

    /// <summary>
    /// Finalization of the job - performed on Unity thread
    /// </summary>
    protected override void OnFinished() {
        if(thrownExcep != null) { // If an exception was thrown...
            try {
                throw thrownExcep; // Rethrow it in a try-catch to process
            } catch(IOException) { // Can't read/write file
                ErrorText.inst.DispError("Problem saving the PDF.  Do you still have it open?");
            } catch(Exception) { // Something else happened
                ErrorText.inst.DispError("Problem saving the PDF.  Something happened that wasn't accounted for.  Please try again.");
                Debug.LogException(thrownExcep);
            }

        } else {
            Application.OpenURL("file://" + filename); // Open the file up for the user
        }

        if(BarManager.inst.savePDF)
            BarManager.inst.fb.currFile = BarManager.inst.barFilePath; // Restore path to current file in the file browser
        BarManager.inst.savePDF = false;

        CanvasDisabler.CanvasEnabled = true; // Show the main UI
        CameraControl.ShowWhole = false; // Shrink down camera view

        progressStuff.Shown = false; // Hide progress stuff

    }

    /// <summary>
    /// Thread's function - creates and produces PDF off of Unity's thread
    /// </summary>
    protected override void ThreadFunction() {
        lock(progressStuff) {
            progressText = "Finished capturing images.";
        }

        PdfDocument doc = new PdfDocument();

        // Count out how many pages we're actually exporting
        byte pages = 0;
        for(int i = 0x1; i < 0x40; i = i << 1) {
            if((BarManager.canPub & i) > 0) {
                pages++;
            }
        }

        byte currPage = 1;

        try {
            doc.Info.Author = "Star Headlight and Lantern Co., Inc."; // Insert document metadata
            doc.Info.Creator = "Phaser Lightbar Configurator";
            doc.Info.Title = "Phaser Lightbar Configuration";
            if((BarManager.canPub & 0x1) > 0) {
                lock(progressStuff) {
                    progressText = string.Format("Publishing Page {0}/{1} : Overview...", currPage++, pages);
                    progressPercentage = 10;
                }
                OverviewPage(doc.AddPage(), capRect); // Produce Overview Page with a new page
            }
            if((BarManager.canPub & 0x2) > 0) {
                lock(progressStuff) {
                    progressText = string.Format("Publishing Page {0}/{1} : BOM...", currPage++, pages);
                    progressPercentage = 30;
                }
                PartsPage(doc.AddPage(), capRect); // Produce Parts Page with a new page
            }
            if((BarManager.canPub & 0x4) > 0) {
                lock(progressStuff) {
                    progressText = string.Format("Publishing Page {0}/{1} : Wiring...", currPage++, pages);
                    progressPercentage = 50;
                }
                WiringPage(doc.AddPage(), capRect); // Produce Wiring Page with a new page
            }
            if((BarManager.canPub & 0x8) > 0) {
                lock(progressStuff) {
                    progressText = string.Format("Publishing Page {0}/{1} : Programming...", currPage++, pages);
                    progressPercentage = 80;
                }
                PatternPage(doc.AddPage(), capRect); // Produce Pattern Page with a new page
            }
            if((BarManager.canPub & 0x10) > 0) {
                lock(progressStuff) {
                    progressText = string.Format("Publishing Page {0}/{1} : Checklist...", currPage++, pages);
                    progressPercentage = 88;
                }
                ChecklistPage(doc.AddPage()); // Produce Checklist Page with a new page
            }
            if((BarManager.canPub & 0x20) > 0) {
                lock(progressStuff) {
                    progressText = string.Format("Publishing Page {0}/{1} : Output Map...", currPage++, pages);
                    progressPercentage = 90;
                }
                OutputMapPage(doc.AddPage(), capRect); // Produce Output Map Page with a new page
            }
            lock(progressStuff) {
                progressText = "Saving...";
                progressPercentage = 99;
            }

            doc.Save(filename); // Save
        } catch(Exception ex) { // An exception was thrown.  Don't care which one (at this moment)
            thrownExcep = ex; // Save the exception to rethrow on Unity's thread
        } finally {
            doc.Close();
            doc.Dispose(); // Dispose the document, it's been saved already (frees internal resources)
        }
    }

    /// <summary>
    /// Produces the overview page (page 1 of 6)
    /// </summary>
    /// <param name="p">The PDF page we're using</param>
    /// <param name="capRect">Reference of the capture rectangle</param>
    public void OverviewPage(PdfPage p, Rect capRect) {
        #region Setup
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont courier = new XFont("Courier New", new XUnit(12, XGraphicsUnit.Point).Inch);
        XFont courierSm = new XFont("Courier New", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliLg = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch);
        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliSmBold = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch, XFontStyle.Bold);
        #endregion

        #region Paste Images
        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage descImg = XImage.FromFile("tempgen\\desc.png")) { // Large description image
            gfx.DrawImage(descImg, 0.5, 1.3, capRect.width * scale, capRect.height * scale);
        }
        using(XImage tl = XImage.FromFile("pdfassets\\TopLeft.png")) { // Top left header image
            gfx.DrawImage(tl, 0.5, 0.5, 0.74, 0.9);
        }
        using(XImage tr = XImage.FromFile("pdfassets\\TopRight.png")) { // Top right header image
            gfx.DrawImage(tr, ((float)p.Width.Inch) - 2.45, 0.5, 1.95, 0.75);
        }

        lock(progressStuff)
            progressPercentage = 20;
        #endregion

        #region Write Page Header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Star Phaser", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));
        tf.DrawString("Model " + BarModel + " - " + BarWidth, courier, XBrushes.Black, new XRect(0.5, 1.1, p.Width.Inch - 1.0, 1.0));
        #endregion

        #region Write Light Head Header
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Light Head Type and Style", caliSmBold, XBrushes.Black, new XRect(1.0, 3.09, 2.0, 0.1));
        tf.DrawString("Light Head Type and Style", caliSmBold, XBrushes.Black, new XRect(5.0, 3.09, 2.0, 0.1));
        tf.DrawString("Amperage", caliSmBold, XBrushes.Black, new XRect(3.0, 3.09, 0.5, 0.1));
        tf.DrawString("Amperage", caliSmBold, XBrushes.Black, new XRect(7.0, 3.09, 0.5, 0.1));
        if(CameraControl.ShowPricing) { // Write header for List Price only if it'll be shown
            tf.DrawString("List Price", caliSmBold, XBrushes.Black, new XRect(3.625, 3.09, 0.5, 0.1));
            tf.DrawString("List Price", caliSmBold, XBrushes.Black, new XRect(7.625, 3.09, 0.5, 0.1));
        }
        #endregion

        double top = 3.2;

        #region Write Out Light Heads
        for(int i = 0; i < headNumber.Length; i++) {
            LightHead lh = headNumber[i];
            tf.DrawString("Pos " + (i + 1).ToString("00"), courierSm, XBrushes.Black, new XRect((i > (headNumber.Length / 2) - 1 ? 4.5 : 0.5), top + ((i > (headNumber.Length / 2) - 1 ? i - (headNumber.Length / 2) : i) * 0.10), 0.5, 0.10));
            PrintHead(tf, caliSm, courierSm, top + ((i > (headNumber.Length / 2) - 1 ? i - (headNumber.Length / 2) : i) * 0.10), lh, i > (headNumber.Length / 2) - 1);
        }
        top += (headNumber.Length / 2) * 0.1;
        top += 0.15;
        #endregion

        #region Additional Parts Header
        tf.DrawString("Additional Parts", caliSmBold, XBrushes.Black, new XRect(1.4, top - 0.01, 2.0, 0.1));
        if(CameraControl.ShowPricing)
            tf.DrawString("List Price", caliSmBold, XBrushes.Black, new XRect(3.625, top - 0.01, 0.5, 0.1));
        #endregion

        #region Write Out Totals
        tf.DrawString("Totals:", caliLg, XBrushes.Black, new XRect(4.5, top + 0.39, 2.0, 0.2));
        tf.DrawString(string.Format("{0:F3}A max", ampTotal * 0.001f), courierSm, XBrushes.Black, new XRect(7.0, top + 0.4, 1.0, 0.10));
        tf.DrawString(string.Format("{0:F3}A avg", ampTotal * 0.0005f), courierSm, XBrushes.Black, new XRect(7.0, top + 0.5, 1.0, 0.10));
        if(CameraControl.ShowPricing) {
            tf.Alignment = XParagraphAlignment.Right;
            tf.DrawString(string.Format("${0:F2}", costTotal * 0.01f), courier, XBrushes.Black, new XRect(6.0, top + 0.7, 2.0, 0.20));
            tf.Alignment = XParagraphAlignment.Left;
        }
        #endregion

        #region Write Bar Base
        top += 0.1;
        tf.DrawString(BarModel + " Bar Base - " + BarWidth, caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
        if(CameraControl.ShowPricing)
            tf.DrawString("$" + (barCost * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        #endregion

        #region Write Mounting Bracket
        top += 0.1;
        tf.DrawString("Mounting Bracket", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
        if(CameraControl.ShowPricing)
            tf.DrawString("$" + (LightDict.inst.bracketPrice * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        #endregion

        #region Write Mounting Kit (if any)
        if(BarManager.mountingKit != 0) {
            top += 0.1;
            tf.DrawString(mntOpt.name, caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (mntOpt.price * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        }
        #endregion

        #region Write Cable Summary
        top += 0.1;
        bomcables.PDFExportSummary(ref top, tf, courierSm, caliSm, caliSmBold);

        top += 0.3;
        #endregion

        XPen border = new XPen(XColors.Black, 0.025);

        #region List Off Issues (if any, if bar was modded)
        if(BarManager.moddedBar) {
            StringBuilder sb = new StringBuilder(1024);
            foreach(string issue in issues) {
                sb.AppendLine(issue);
            }
            if(sb.Length > 0) {
                tf.DrawString("Issues found:", caliSmBold, XBrushes.Black, new XRect(0.5, top, 1.2, 0.10));
                tf.DrawString("Sign Off:", caliLg, XBrushes.Black, new XRect(5.2, top - .1, 0.6, 0.2));
                gfx.DrawLine(border, 5.8, top + 0.1, 6.0, top - 0.1);
                gfx.DrawLine(border, 5.8, top - 0.1, 6.0, top + 0.1);
                gfx.DrawLine(border, 6.0, top + 0.1, 8.0, top + 0.1);
                tf.DrawString(sb.ToString(), caliSm, XBrushes.Black, new XRect(0.6, top + 0.10, p.Width.Inch - 1.1, 2.0));
            }
        }
        #endregion

        #region Produce Bottom Section
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
        tf.DrawString(notes + "\nFilename: " + filename, caliSm, XBrushes.Black, new XRect(0.6, top + 0.61, p.Width.Inch - 1.2, 1.4));
        #endregion

        #region Write Footer
        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, caliSm, XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        #endregion
    }

    /// <summary>
    /// Helper function to print out a head.
    /// </summary>
    /// <param name="tf">The TextFormatter in use</param>
    /// <param name="caliSm">Small Calibri font</param>
    /// <param name="courierSm">Small Courier font</param>
    /// <param name="top">The "top" variable for reference</param>
    /// <param name="lh">The light head to print out</param>
    /// <param name="rightSide">Is this the right column?</param>
    private static void PrintHead(XTextFormatter tf, XFont caliSm, XFont courierSm, double top, LightHead lh, bool rightSide) {
        if(lh.lhd.style == null) { // No head?  Write nothing.
            tf.DrawString(" -- ", caliSm, XBrushes.Black, new XRect((rightSide ? 5.4 : 1.4), (top - 0.01), 0.5, 0.10));
        } else {
            // Write out the head name, with style if more than one style exists
            tf.DrawString((lh.lhd.optic.styles.Count > 1 ? lh.lhd.style.name + " " : "") + lh.lhd.optic.name, caliSm, XBrushes.Black, new XRect((rightSide ? 5.0 : 1.0), (top - 0.01), 2.0, 0.10));
            tf.DrawString((lh.lhd.optic.amperage * 0.001f).ToString("0.000A"), courierSm, XBrushes.Black, new XRect((rightSide ? 7.0 : 3.0), top, 0.625, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (lh.lhd.optic.cost * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect((rightSide ? 7.625 : 3.625), top, 0.5, 0.10));
        }
    }

    /// <summary>
    /// Produces the parts page (page 2 of 6)
    /// </summary>
    /// <param name="p">The PDF page we're using</param>
    /// <param name="capRect">Reference of the capture rectangle</param>
    public void PartsPage(PdfPage p, Rect capRect) {
        #region Setup
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont courier = new XFont("Courier New", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliBold = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch, XFontStyle.Bold);
        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);
        #endregion

        #region Paste Images
        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage partImg = XImage.FromFile("tempgen\\part.png")) {
            gfx.DrawImage(partImg, 0.5, 1.0, capRect.width * scale, capRect.height * scale);
        }

        lock(progressStuff)
            progressPercentage = 40;
        #endregion

        #region Write Page Header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Model " + BarModel, new XFont("Courier New", new XUnit(24, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.5, p.Width.Inch - 1.0, 1.0));
        tf.DrawString("Production Copy - Bill of Materials", caliBold, XBrushes.Black, new XRect(0.5, 0.8, p.Width.Inch - 1.0, 1.0));
        #endregion

        #region Write Component Header
        tf.DrawString("Quantity", caliBold, XBrushes.Black, new XRect(0.5, 3.3, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Component", caliBold, XBrushes.Black, new XRect(1.5, 3.3, 1.0, 0.2));
        tf.DrawString("Description", caliBold, XBrushes.Black, new XRect(3.0, 3.3, 4.0, 0.2));
        #endregion

        #region Compile List of Heads
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
        #endregion

        #region Circuit, Gutter Mount Bracket, and Mounting Kit
        double top = 3.5;
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString(bomcables.circuitPrefix + ((bomcables.dualLCount + bomcables.dualRCount) > 0 ? "2" : "1"), courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
        tf.DrawString("Control Circuit - " + ((bomcables.dualLCount + bomcables.dualRCount) > 0 ? "Dual-Color Capable" : "Single-Color Only"), caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
        top += 0.15;
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Gutter Mount Bracket", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
        if(BarManager.mountingKit != 0) {
            top += 0.15;
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(mntOpt.part, courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString(mntOpt.name, caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
        }
        #endregion

        #region Output Light Heads
        top += 0.25;
        foreach(string part in parts) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(counts[part] + "", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString((descs[part] as LightHead).PartNumber, courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString(((descs[part] as LightHead).lhd.optic.styles.Count > 1 ? (descs[part] as LightHead).lhd.style.name + " " : "") + (descs[part] as LightHead).lhd.optic.name, caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }

        lock(progressStuff)
            progressPercentage = 45;
        #endregion

        top += 0.2;

        #region Write Lenses Header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Quantity", caliBold, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Lenses", caliBold, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
        tf.DrawString("Description", caliBold, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
        #endregion

        #region Compile List of Lenses
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
        #endregion

        #region Output Lenses
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
        #endregion

        #region Write Cable Header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Quantity", caliBold, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Cables", caliBold, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
        tf.DrawString("Description", caliBold, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
        #endregion

        #region Write Cable Parts
        bomcables.PDFExportParts(ref top, tf, courier, caliSm);
        #endregion

        #region Write Footer
        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, caliSm, XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        #endregion
    }

    /// <summary>
    /// Produces the wiring page (page 3 of 6)
    /// </summary>
    /// <param name="p">The PDF page we're using</param>
    /// <param name="capRect">Reference of the capture rectangle</param>
    public void WiringPage(PdfPage p, Rect capRect) {
        #region Setup
        p.Orientation = PageOrientation.Landscape;

        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);
        #endregion

        #region Paste Wire Image
        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage wireImg = XImage.FromFile("tempgen\\wire.png")) {
            gfx.DrawImage(wireImg, 0.5, 1.2, capRect.width * scale, capRect.height * scale);
        }

        lock(progressStuff)
            progressPercentage = 60;
        #endregion

        #region Write Page Header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Wiring Diagram", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));
        #endregion

        #region Paste Static Circuit Reference
        XImage circuit = XImage.FromFile("pdfassets\\Circuit.png");
        scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (circuit.PixelWidth * 1.0f);
        gfx.DrawImage(circuit, 0.5, 3.75, circuit.PixelWidth * scale, circuit.PixelHeight * scale);

        lock(progressStuff)
            progressPercentage = 70;
        #endregion

        #region Write Footer
        tf.Alignment = XParagraphAlignment.Left;
        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, caliSm, XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        #endregion
    }

    /// <summary>
    /// Produces the pattern page (page 4 of 6)
    /// </summary>
    /// <param name="p">The PDF page we're using</param>
    /// <param name="capRect">Reference of the capture rectangle</param>
    public void PatternPage(PdfPage p, Rect capRect) {
        #region Setup
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliBold = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch, XFontStyle.Bold);

        XPen border = new XPen(XColors.Black, 0.025);
        #endregion

        #region Paste Image
        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage wireImg = XImage.FromFile("tempgen\\wireClrless.png")) {
            gfx.DrawImage(wireImg, 0.5, 1.2, capRect.width * scale, capRect.height * scale);
        }

        lock(progressStuff)
            progressPercentage = 90;
        #endregion

        #region Write Page Header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Bar Programming", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));
        #endregion

        #region Write Default Program Box (if applicable)
        if(patts.Contains("prog")) {
            gfx.DrawRectangle(border, XBrushes.Yellow, new XRect(0.5, 0.65, 0.75, 0.7));
            tf.DrawString("Default\nProgram\n" + patts["prog"].ByteValue, caliBold, XBrushes.Black, new XRect(0.5, 0.7, 0.75, 0.6));
            gfx.DrawRectangle(border, XBrushes.Yellow, new XRect(p.Width.Inch - 1.25, 0.65, 0.75, 0.7));
            tf.DrawString("Default\nProgram\n" + patts["prog"].ByteValue, caliBold, XBrushes.Black, new XRect(p.Width.Inch - 1.25, 0.7, 0.75, 0.6));
        }
        #endregion

        double top = 3.0;

        //  ** Commented: do not need the input map currently **
        #region Input Map
        //tf.DrawString("Input Map", caliBold, XBrushes.Black, new XRect(3.0, top, p.Width.Inch - 6.0, 0.1));
        //top += 0.2;
        //if(useCAN) {
        //    PrintRow(tf, caliSm, GetFuncFromMap(0), GetFuncFromMap(12), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(1), GetFuncFromMap(13), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(2), GetFuncFromMap(14), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(3), GetFuncFromMap(15), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(4), GetFuncFromMap(16), ref top);

        //    PrintRow(tf, caliSm, GetFuncFromMap(5), GetFuncFromMap(17), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(6), GetFuncFromMap(18), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(7), GetFuncFromMap(19), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(8), "POWER", ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(9), "GROUND", ref top);

        //    PrintRow(tf, caliSm, GetFuncFromMap(10), "---", ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(11), "---", ref top);
        //} else {
        //    PrintRow(tf, caliSm, GetFuncFromMap(1) + " - White & Yellow", "White & Orange - " + GetFuncFromMap(0), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(3) + " - Brown", "White & Red - " + GetFuncFromMap(2), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(4) + " - Yellow", "Red & Green - " + GetFuncFromMap(11), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(6) + " - Blue", "Green - " + GetFuncFromMap(5), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(8) + " - Purple & White", "Gray - " + GetFuncFromMap(7), ref top);
        //    PrintRow(tf, caliSm, GetFuncFromMap(10) + " - White & Pink", "Purple - " + GetFuncFromMap(9), ref top);
        //    PrintRow(tf, caliSm, "---", "---", ref top);
        //} 
        #endregion

        top += 0.05;

        #region Write Table Header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Function Definitions", caliBold, XBrushes.Black, new XRect(3.0, top, p.Width.Inch - 6.0, 0.1));
        top += 0.2;
        gfx.DrawRectangle(border, new XRect(0.5, top, p.Width.Inch - 1.3, 0.4));
        tf.DrawString("Function", caliBold, XBrushes.Black, new XRect(0.5, top + (useCAN ? 0.0 : 0.1), 1.25, 0.2));
        if(useCAN) tf.DrawString("Break Out Box", caliBold, XBrushes.Black, new XRect(0.50, top + 0.2, 1.25, 0.2));
        gfx.DrawLine(border, 1.75, top, 1.75, top + 0.4);
        tf.DrawString("Positions", caliBold, XBrushes.Black, new XRect(1.8, top, 4.0, 0.1));
        tf.DrawString("Phase A", caliBold, XBrushes.Black, new XRect(1.8, top + 0.2, 1.95, 0.1));
        gfx.DrawLine(border, 3.8, top + 0.2, 3.8, top + 0.4);
        tf.DrawString("Phase B", caliBold, XBrushes.Black, new XRect(3.85, top + 0.2, 1.95, 0.1));
        gfx.DrawLine(border, 5.8, top, 5.8, top + 0.4);
        tf.DrawString("Pattern(s)", caliBold, XBrushes.Black, new XRect(5.85, top + 0.1, p.Width.Inch - 6.7, 0.2));
        #endregion

        #region Write Functions
        top += 0.4;
        foreach(int func in new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 8192, 16384, 32768, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000 }) {
            for(int i = 0; i < 20; i++) {
                if((FnDragTarget.inputMap[i] & (int)func) > 0) {

                    #region Draw Borders, Write Function
                    gfx.DrawRectangle(border, new XRect(0.5, top, p.Width.Inch - 1.0, 0.3));
                    gfx.DrawLine(border, 1.75, top, 1.75, top + 0.3);
                    gfx.DrawLine(border, 5.8, top, 5.8, top + 0.3);
                    gfx.DrawLine(border, p.Width.Inch - 0.8, top, p.Width.Inch - 0.8, top + 0.3);
                    tf.DrawString(GetFuncFromInt(func) + "\n" + GetInput(i), caliSm, XBrushes.Black, new XRect(0.5, top + 0.025, 1.25, 0.1));
                    #endregion

                    List<string> parts = new List<string>();
                    AdvFunction advfunc = (AdvFunction)func;

                    PartComparer pc = new PartComparer();

                    #region Writing Wires
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
                            #region Compile List of Wires for each Enable
                            foreach(LightHead alpha in headNumber) {
                                if(!alpha.hasRealHead) continue;

                                if(alpha.GetIsEnabled(advfunc, false, true)) {
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
                            #endregion

                            #region Sort Parts
                            parts.Sort(pc);
                            partsB.Sort(pc);
                            #endregion

                            #region Condense Part Lists
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
                            #endregion

                            #region Output
                            if(partsB.Count > 0) {
                                // Both phases used

                                gfx.DrawLine(border, 3.8, top, 3.8, top + 0.3);
                                // Write out Phase A
                                tf.DrawString(string.Join(", ", parts.ToArray()), caliSm, XBrushes.Black, new XRect(1.8, top + 0.025, 1.95, 0.3));
                                // Write out Phase B
                                tf.DrawString(string.Join(", ", partsB.ToArray()), caliSm, XBrushes.Black, new XRect(3.85, top + 0.025, 1.95, 0.3));
                            } else {
                                // Only Phase A used

                                // Write out enabled
                                tf.DrawString(string.Join(", ", parts.ToArray()), caliSm, XBrushes.Black, new XRect(1.8, top + 0.025, 4.0, 0.3));
                            }
                            #endregion

                            break;
                        default:
                            // Write out enable
                            #region Compile List of Wires
                            foreach(LightHead alpha in headNumber) {
                                if(!alpha.hasRealHead) continue;
                                if(alpha.GetIsEnabled(advfunc, false, true)) {
                                    parts.Add(color1Wire[alpha]);
                                }
                                if(alpha.lhd.style.isDualColor && alpha.GetIsEnabled(advfunc, true)) {
                                    parts.Add(color2Wire[alpha]);
                                }
                            }
                            #endregion

                            // Sort Wires
                            parts.Sort(pc);

                            #region Condense Part List
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
                            #endregion

                            // Output
                            tf.DrawString(string.Join(", ", parts.ToArray()), caliSm, XBrushes.Black, new XRect(1.8, top + 0.025, 4.0, 0.3));
                            break;
                    }
                    #endregion

                    #region Write out pattern(s)
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

                            #region Get Patterns
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
                            #endregion

                            #region Write Patterns (if there are any)
                            if(patt.Count > 0) {
                                tf.DrawString(string.Join(", ", patt.ToArray()), caliSm, XBrushes.Black, new XRect(5.85, top + 0.025, p.Width.Inch - 6.7, 0.3));
                            }
                            #endregion
                            break;
                        case 0x200: // DIM
                            tf.DrawString("Dimmer", caliSm, XBrushes.Black, new XRect(5.85, top + 0.025, p.Width.Inch - 6.7, 0.2));
                            break;
                        default:
                            tf.DrawString("Steady Burn", caliSm, XBrushes.Black, new XRect(5.85, top + 0.025, p.Width.Inch - 6.7, 0.2));
                            break;
                    }
                    #endregion

                    top += 0.3;
                    break;
                }
            }
        }
        #endregion

        #region Add Arrow to Bottom
        top += 0.025;
        gfx.DrawLine(border, p.Width.Inch - 0.65, top, p.Width.Inch - 0.8, top + 0.15);
        gfx.DrawLine(border, p.Width.Inch - 0.65, top, p.Width.Inch - 0.5, top + 0.15);
        gfx.DrawLine(border, p.Width.Inch - 0.65, top, p.Width.Inch - 0.65, top + 0.2);
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("Tested for Accuracy", caliSm, XBrushes.Black, new XRect(p.Width.Inch - 2.0, top + 0.025, 1.1, 0.2));
        gfx.DrawLines(border, p.Width.Inch - 2.0, top + 0.2, p.Width.Inch - 0.65, top + 0.2);
        #endregion

        #region Write Footer
        tf.Alignment = XParagraphAlignment.Left;
        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, caliSm, XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        #endregion
    }

    /// <summary>
    /// Produces the checklist page (page 5 of 6)
    /// </summary>
    /// <param name="p">The PDF page we're using</param>
    public void ChecklistPage(PdfPage p) {
        #region Setup
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont cali = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch);
        XFont caliBold = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch, XFontStyle.Bold);

        XPen border = new XPen(XColors.Black, 0.025);
        #endregion

        #region Write Page Header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Phaser Cable Assembly Checklist", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));
        tf.Alignment = XParagraphAlignment.Left;
        #endregion

        #region Main Checklist Header
        tf.DrawString("Initials", caliSm, XBrushes.Black, new XRect(0.5, 1.0, 0.5, 0.1));
        tf.DrawString("Assembly / Inspection", caliBold, XBrushes.Black, new XRect(0.5, 1.15, 2.0, 0.1));
        #endregion

        double top = 1.4;

        #region Order, Serial, SO, Date
        tf.Alignment = XParagraphAlignment.Right;
        string[] list = new string[] { "Order #:", "Serial #:", "S.O. #:" };

        foreach(string alpha in list) {
            gfx.DrawLine(border, 7.0, top + 0.25, 8.0, top + 0.25);
            tf.DrawString(alpha, cali, XBrushes.Black, new XRect(6.0, top + 0.05, 0.9, 0.2));
            top += 0.3;
        }
        tf.DrawString("Date:", cali, XBrushes.Black, new XRect(6.0, 1.15, 0.9, 0.2));
        tf.Alignment = XParagraphAlignment.Left;

        tf.DrawString(DateTime.Now.ToString("MMM dd, \\'yy"), cali, XBrushes.Black, new XRect(7.0, 1.15, 1.0, 0.2));
        #endregion

        #region Checklist
        top = 1.4;
        list = new string[] { "Confirm correct light components / color with order", "Check for loose/splayed wires in terminal block", "Check for pinched wires",
                                       "Check for and remove loose hardware, wire insulation, etc.", "Wipe all components clean", "Power components individually / check off approriate color wire below",
                                       "Apply all labels (Model #, Serial #, USA, etc.)", "Run burn-in test" };

        foreach(string alpha in list) {
            gfx.DrawRectangle(border, XBrushes.White, new XRect(0.5, top, 0.75, 0.3));
            gfx.DrawLine(border, 0.5, top + 0.3, 1.25, top);
            tf.DrawString(alpha, cali, XBrushes.Black, new XRect(1.35, top + 0.05, 5.5, 0.2));
            top += 0.3;
        }
        #endregion

        #region Burn-In Test Supplemental
        tf.Alignment = XParagraphAlignment.Right;
        top -= 0.3;
        tf.DrawString("Start:", cali, XBrushes.Black, new XRect(3.0, top + 0.05, 0.95, 0.2));
        gfx.DrawLine(border, 4.0, top + 0.25, 5.0, top + 0.25);
        tf.DrawString("Stop:", cali, XBrushes.Black, new XRect(5.0, top + 0.05, 0.95, 0.2));
        gfx.DrawLine(border, 6.0, top + 0.25, 7.0, top + 0.25);
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("(1 hour typical, 30 minutes minimum)", caliBold, XBrushes.Black, new XRect(3.5, top + 0.25, 3.5, 0.2));
        top += 0.4;
        tf.Alignment = XParagraphAlignment.Left;
        #endregion

        #region Secondary Checklist
        tf.DrawString("Packing", caliBold, XBrushes.Black, new XRect(0.5, top, 2.0, 0.1));
        top += 0.25;
        #region Left Column
        list = new string[] { "Polish Domes", "Add mounting kit", "Add gutter mount", "Add instruction sheet", "Add carton labels" };

        foreach(string alpha in list) {
            gfx.DrawRectangle(border, XBrushes.White, new XRect(0.5, top, 0.75, 0.3));
            tf.DrawString(alpha, cali, XBrushes.Black, new XRect(1.35, top + 0.05, 5.5, 0.2));
            top += 0.3;
        }
        #endregion
        top -= (list.Length - 1) * 0.3;
        #region Right Column
        list = new string[] { "No mounting kit required", "No gutter mount required", "Add Checklist (Confirm Serial #!)", "Program Lightbar" };

        foreach(string alpha in list) {
            gfx.DrawRectangle(border, XBrushes.White, new XRect(4.0, top, 0.75, 0.3));
            tf.DrawString(alpha, cali, XBrushes.Black, new XRect(4.85, top + 0.05, 5.5, 0.2));
            top += 0.3;
        }
        #endregion
        #endregion

        gfx.DrawLine(new XPen(XColors.Black, 0.02), 5.85, top - 0.35, 7.0, top - 0.35);
    }

    /// <summary>
    /// Produces the output mapping page (page 6 of 6)
    /// </summary>
    /// <param name="p">The PDF page we're using</param>
    /// <param name="capRect">Reference of the capture rectangle</param>
    public void OutputMapPage(PdfPage p, Rect capRect) {
        #region Setup
        p.Orientation = PageOrientation.Landscape;

        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);
        #endregion

        #region Paste the One Image
        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (capRect.width * 1.0f);
        using(XImage wireImg = XImage.FromFile("tempgen\\bits.png")) {
            gfx.DrawImage(wireImg, 0.5, 2.0, capRect.width * scale, capRect.height * scale);
        }
        #endregion

        #region Write Page Header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Output Usage Map", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));
        #endregion

        #region Write Footer
        tf.Alignment = XParagraphAlignment.Left;
        if(orderNumber.Length > 0)
            tf.DrawString("Order Number: " + orderNumber, new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch), XBrushes.Black, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch), XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));
        #endregion
    }

    /// <summary>
    /// Get the Advanced Function name given a function number
    /// </summary>
    /// <param name="num">The number that corresponds to a specific function</param>
    /// <returns>The name of that function</returns>
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

    /// <summary>
    /// Gets the name of input corresponding to an input number
    /// </summary>
    /// <param name="which">The number of the input</param>
    /// <returns>The color of the wire for Hardwire, or the name of the pin on the CAN Breakout Board</returns>
    public string GetInput(int which) {
        if(useCAN) {
            return (which > 11 ? "P5" : "P4") + ", Pin " + ((which % 12) + 1);
        } else {
            switch(which) {
                case 0:
                    return "White & Orange";
                case 1:
                    return "White & Yellow";
                case 2:
                    return "White & Red";
                case 3:
                    return "Brown";
                case 4:
                    return "Yellow";
                case 5:
                    return "Green";
                case 6:
                    return "Blue";
                case 7:
                    return "Gray";
                case 8:
                    return "Purple & White";
                case 9:
                    return "Purple";
                case 10:
                    return "White & Pink";
                case 11:
                    return "Red & Green";
                default:
                    return "???";
            }
        }
    }

    /// <summary>
    /// Get the function name corresponding to a function on the input map
    /// </summary>
    /// <param name="which">The number of the input</param>
    /// <returns>The name of the function on that input</returns>
    public string GetFuncFromMap(int which) {
        return GetFuncFromInt(patts.Get<NbtIntArray>("map").Value[which]);
    }

    /// <summary>
    /// Prints two strings side-by-side.  Used for the input map.
    /// </summary>
    /// <param name="tf">The TextFormatter to use</param>
    /// <param name="caliSm">The font to use</param>
    /// <param name="left">The string to write on the left</param>
    /// <param name="right">The string to write on the right</param>
    /// <param name="top">Where to print it vertically on the page</param>
    public void PrintRow(XTextFormatter tf, XFont caliSm, string left, string right, ref double top) {
        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString(left, caliSm, XBrushes.Black, new XRect(2.0, top, 2.15, 0.1));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString(right, caliSm, XBrushes.Black, new XRect(4.35, top, 2.15, 0.1));
        top += 0.1;
    }

    /// <summary>
    /// A small class that compares the wires for the enables.  Utilized when sorting the wire lists.
    /// </summary>
    private class PartComparer : IComparer<string> {

        /// <summary>
        /// Compares two wires
        /// </summary>
        /// <param name="x">Wire 1</param>
        /// <param name="y">Wire 2</param>
        /// <returns>Less than 0 if Wire 1 comes before Wire 2.  More than 0 if vice versa.  0 if they're the same.</returns>
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