using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using fNbt;
using System.Text;

/// <summary>
/// GUI object.  Displays information about light heads.
/// </summary>
public class LightLabel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    /// <summary>
    /// Where are we positioning the label?
    /// </summary>
    public Transform target;
    /// <summary>
    /// Text Component on color 1 of head, should match label2.  Set via Unity Inspector.
    /// </summary>
    public Text label;
    /// <summary>
    /// Text Component on color 2 of head, should match label.  Set via Unity Inspector.
    /// </summary>
    public Text label2;
    /// <summary>
    /// Wire color label.  Set via Unity Inspector.
    /// </summary>
    public Text colorLabel;
    /// <summary>
    /// Color 1 background.  Set via Unity Inspector.
    /// </summary>
    public Image background;
    /// <summary>
    /// Color 2 background.  Set via Unity Inspector.
    /// </summary>
    public Image secondImage;
    /// <summary>
    /// The image displaying the selection status of the head.  Set via Unity Inspector.
    /// </summary>
    public Image selectionImage;
    /// <summary>
    /// The image displaying the SameOutputWarning icon.  Set via Unity Inspector.
    /// </summary>
    public Image warnImage;

    /// <summary>
    /// The target LightHead
    /// </summary>
    private LightHead lh;

    /// <summary>
    /// Reference to the CameraControl object
    /// </summary>
    public static CameraControl cam;

    /// <summary>
    /// Are we showing part numbers?
    /// </summary>
    public static bool showParts = false;
    /// <summary>
    /// Are we showing the bits?
    /// </summary>
    public static bool showBit = false;
    /// <summary>
    /// Are we showing only the bits?
    /// </summary>
    public static bool showJustBit = false;
    /// <summary>
    /// Are we showing wires?
    /// </summary>
    public static bool showWire = false;
    /// <summary>
    /// Are we showing wires without colors?
    /// </summary>
    public static bool colorlessWire = false;
    /// <summary>
    /// Are we showing empty wire boxes for manual entry?
    /// </summary>
    public static bool wireOverride = false;
    /// <summary>
    /// Are we showing patterns for a specific Advanced Function?
    /// </summary>
    public static bool showPatt = false;
    /// <summary>
    /// Reference to the tooltip object for better display of text on mouseover.
    /// </summary>
    public static LabelTooltip tooltip;
    /// <summary>
    /// The last optic this label was aware of the head having (for automatic refreshing)
    /// </summary>
    private OpticNode lastOptic;
    /// <summary>
    /// The last style this label was aware of the head having (for automatic refreshing)
    /// </summary>
    private StyleNode lastStyle;

    /// <summary>
    /// Do we show the SameOutputWarning icon on this label?
    /// </summary>
    public bool DispError {
        get { return warnImage.enabled; }
        set { warnImage.enabled = value; }
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        selectionImage.gameObject.SetActive(false);
        selectionImage.transform.rotation = Quaternion.identity;
        if(lh == null) lh = target.GetComponent<LightHead>();
        if(lh.isSmall) {
            ((RectTransform)transform).sizeDelta = new Vector2(65, 48);
        }
        Refresh();
    }

    /// <summary>
    /// Refreshes the light label, optionally showing the head number as well.
    /// </summary>
    /// <param name="showHeadNumber">Whether or not to show the head's number</param>
    public void Refresh(bool showHeadNumber = false) {
        if(lh == null) lh = target.GetComponent<LightHead>(); // Don't have reference to light head yet?  Get it.

        colorLabel.text = "";

        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) {
            StartCoroutine(PatternSim()); // Begin previewing the flashing of the light head instead of static information
            return;
        }

        string prefix = "";

        #region Show Head Number
        if(showHeadNumber) {
            for(int i = 0; i < BarManager.headNumber.Length; i++) {
                if(BarManager.headNumber[i] == lh) {
                    prefix = "(" + (i + 1) + ") ";
                    break;
                }
            }
            if(prefix == "") {
                Debug.LogError("Failed to find a head number for " + lh.Path);
                prefix = "(?) ";
            }
        }
        #endregion

        #region Show Part Numbers
        if(showParts) {
            if(lh.lhd.style != null) {
                label2.text = label.text = prefix + lh.PartNumber;
                label2.color = label.color = Color.black;
                secondImage.color = background.color = Color.white;
            }
        }
        #endregion
        #region Show Just Bit
 else if(showJustBit) {
            if(lh.hasRealHead) {
                label2.text = label.text = ((lh.Bit != 255) ? (lh.Bit + 1) + "" : "None");
                label2.color = label.color = Color.black;
                secondImage.color = background.color = Color.white;
            } else {
                label2.text = label.text = "";
                secondImage.color = background.color = new Color(0f, 0f, 0f, 0f);
            }
        }
        #endregion
        #region Show Output Wire
 else if(showWire) {
            if(lh.hasRealHead) {
                byte bit = lh.Bit;

                Color labelColor = Color.white;

                if(!wireOverride) {
                    if(lh.transform.position.y < 0) {
                        switch(lh.Bit) {
                            case 5:
                            case 6:
                                labelColor = new Color(0.5f, 0.25f, 0.0f); // Brown  (Red with some green)
                                colorLabel.text = "Brown";
                                break;
                            case 4:
                            case 7:
                                labelColor = Color.yellow;
                                colorLabel.text = "Yellow";
                                break;
                            case 3:
                            case 8:
                                labelColor = Color.green;
                                colorLabel.text = "Green";
                                break;
                            case 2:
                            case 9:
                                labelColor = Color.blue;
                                colorLabel.text = "Blue";
                                break;
                            case 1:
                            case 10:
                                labelColor = new Color(0.5f, 0.0f, 1.0f); // Purple  (Blue with some red)
                                colorLabel.text = "Purple";
                                break;
                            case 0:
                            case 11:
                                colorLabel.text = "White";
                                break;
                            default:
                                break;
                        }
                    } else {
                        switch(bit) {
                            case 5:
                            case 6:
                                labelColor = (lh.FarWire ? Color.yellow : new Color(0.5f, 0.25f, 0.0f)); // Yellow if far wire, brown if not
                                colorLabel.text = (lh.FarWire ? "Yellow" : "Brown");
                                break;
                            case 4:
                            case 7:
                                labelColor = Color.green;
                                colorLabel.text = "Green";
                                break;
                            case 1:
                            case 10:
                                labelColor = Color.blue;
                                colorLabel.text = "Blue";
                                break;
                            case 0:
                            case 11:
                                labelColor = new Color(0.5f, 0.0f, 1.0f);
                                colorLabel.text = "Purple";
                                break;
                            case 12:
                            case 13:
                                colorLabel.text = "White";
                                break;
                            default:
                                break;
                        }
                    }

                    secondImage.color = background.color = labelColor;
                    if(labelColor.r + labelColor.g < labelColor.b || (labelColor.r + labelColor.g + labelColor.b) < 1.0f) {
                        label2.color = label.color = Color.white;
                    } else {
                        label2.color = label.color = Color.black;
                    }

                    label2.text = label.text = prefix + BarManager.GetWire(lh); // Get the wiring text from the BarManager

                } else { // Overriding the pre-generated wiring scheme, just show empty boxes
                    label2.color = label.color = Color.black;
                    secondImage.color = background.color = Color.white;

                    label2.text = label.text = prefix + "\n\n";
                }

                if(colorlessWire) { // Don't show colors, just the text
                    label2.color = label.color = Color.black;
                    secondImage.color = background.color = Color.white;
                    colorLabel.text = "";
                }
            }
        }
        #endregion
        #region Show Pattern
 else if(showPatt) {
            if(lh.hasRealHead) {
                bool canEnable = lh.GetCanEnable(FunctionEditPane.currFunc);

                #region Find function NbtCompound Tag
                NbtCompound patts = BarManager.inst.patts;
                string cmpdName = BarManager.GetFnString(lh.transform, FunctionEditPane.currFunc);
                if(cmpdName == null) {
                    Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                    return;
                }

                NbtCompound func = patts.Get<NbtCompound>(cmpdName);
                #endregion

                string t = ((showBit && lh.Bit != 255) ? lh.Bit + ": " : "");

                if(func.Contains("e" + (lh.transform.position.y < 0 ? "r" : "f") + "1")) {
                    bool thisEnabled1 = lh.GetIsEnabled(FunctionEditPane.currFunc, false, true),
                         thisEnabled2 = lh.GetIsEnabled(FunctionEditPane.currFunc, true);

                    thisEnabled1 &= canEnable;
                    thisEnabled2 &= canEnable;

                    #region Give label color(s)
                    Color clr = lh.lhd.style.color;
                    if(!thisEnabled1) clr.a = 0.25f;
                    background.color = clr;
                    if(!thisEnabled1 || clr.r + clr.g < clr.b || (clr.r + clr.g + clr.b) < 1.0f) {
                        label.color = Color.white;
                    } else {
                        label.color = Color.black;
                    }
                    if(lh.lhd.style.isDualColor) {
                        clr = lh.lhd.style.color2;
                        if(!thisEnabled2) clr.a = 0.25f;
                    }
                    if((!lh.lhd.style.isDualColor && !thisEnabled1) || (lh.lhd.style.isDualColor && !thisEnabled2) || clr.r + clr.g < clr.b || (clr.r + clr.g + clr.b) < 1.0f) {
                        label2.color = Color.white;
                    } else {
                        label2.color = Color.black;
                    }
                    secondImage.color = clr;
                    #endregion

                    #region Color 1
                    if(thisEnabled1) { // Head is enabled
                        if(func.Contains("pat1")) { // Flashing function
                            bool thisPhase = lh.GetPhaseB(FunctionEditPane.currFunc);

                            Pattern pat = lh.GetPattern(FunctionEditPane.currFunc, false);
                            if(pat == null)
                                t = t + "No Patt";
                            else {
                                t = t + pat.name + ((pat is FlashPatt || pat is SingleFlashRefPattern || pat is DoubleFlashRefPattern) ? (thisPhase ? " B" : " A") : ""); // Show Phase if function can be phased
                            }
                        } else if(func.Contains("patt")) { // Traffic function
                            Pattern pat = lh.GetPattern(AdvFunction.TRAFFIC_LEFT, false);
                            if(pat == null)
                                t = t + "No Patt";
                            else {
                                t = t + pat.name;
                            }
                        } else { // Steady function
                            t = t + "Enabled";
                        }
                    } else if(canEnable) { // Head can enable, but isn't
                        t = t + "Disabled";
                    } else { // Head cannot enable
                        t = t + "Optic Fn Off";
                    }
                    #endregion

                    #region Color 2
                    if(canEnable && lh.lhd.style.isDualColor) { // Head can enable
                        if(thisEnabled2) { // Head is enabled
                            if(func.Contains("pat2")) { // Flashing function
                                bool thisPhase = lh.GetPhaseB(FunctionEditPane.currFunc, true);

                                Pattern pat = lh.GetPattern(FunctionEditPane.currFunc, true);
                                if(pat == null)
                                    t = t + " / No Patt";
                                else
                                    t = t + " / " + pat.name + ((pat is FlashPatt || pat is SingleFlashRefPattern || pat is DoubleFlashRefPattern) ? (thisPhase ? " B" : " A") : ""); // Show Phase if function can be phased
                            } else if(func.Contains("patt")) { // Traffic function
                                Pattern pat = lh.GetPattern(AdvFunction.TRAFFIC_LEFT, true);
                                if(pat == null) {
                                    t = t + " / No Patt";
                                } else {
                                    t = t + " / " + pat.name;
                                }
                            } else { // Steady function
                                t = t + " / Enabled";
                            }
                        } else { // Head is not enabled
                            t = t + " / Disabled";
                        }
                    }
                    #endregion
                } else { // Enable tag doesn't exist.  Probably front head while examining Traffic function
                    Color clr = lh.lhd.style.color;
                    clr.a = 0.25f;
                    background.color = clr;
                    if(lh.lhd.style.isDualColor) {
                        clr = lh.lhd.style.color2;
                        clr.a = 0.25f;
                    }
                    secondImage.color = clr;
                    label2.color = label.color = Color.white;
                    t = t + "Optic Fn Off";
                }

                label2.text = label.text = t;

            } else { // Head does not have proper optic
                label2.text = label.text = ((showBit && lh.Bit != 255) ? lh.Bit + ": " : "") + "No Head";
                label2.color = label.color = Color.white;
                secondImage.color = background.color = new Color(0, 0, 0, 0.45f);
            }
        }
        #endregion
        #region Show Description
 else {
            if(lh.lhd.style != null) {
                #region Build Function list
                StringBuilder sb = new StringBuilder(20);
                foreach(BasicFunction func in new BasicFunction[] { BasicFunction.FLASHING, BasicFunction.STEADY, BasicFunction.EMITTER, BasicFunction.CAL_STEADY, BasicFunction.CRUISE, BasicFunction.STT, BasicFunction.TRAFFIC }) {
                    if(!lh.lhd.funcs.Contains(func)) continue;
                    string chars = "";
                    switch(func) {
                        case BasicFunction.CAL_STEADY:
                            chars = "T13";
                            break;
                        case BasicFunction.CRUISE:
                            chars = "C";
                            break;
                        //case BasicFunction.EMITTER:  // Probably don't need to show Emitter function, would be redundant
                        //    chars = "E";
                        //    break;
                        case BasicFunction.FLASHING:
                            chars = "F";
                            break;
                        case BasicFunction.STEADY:
                            chars = "SB";
                            break;
                        case BasicFunction.STT:
                            chars = "STT";
                            break;
                        case BasicFunction.TRAFFIC:
                            chars = "TD";
                            break;
                        default:
                            continue;
                    }
                    if(sb.Length > 0)
                        sb.Append('/');
                    sb.Append(chars);
                }
                if(sb.Length > 0)
                    sb.Append(' ');
                #endregion

                label2.text = label.text = prefix + ((showBit && lh.Bit != 255) ? lh.Bit + ": " : "") + sb.ToString() + (lh.lhd.optic.styles.Count > 1 ? lh.lhd.style.name + " " : "") + lh.lhd.optic.name;
                #region Colorize
                Color clr = lh.lhd.style.color;
                background.color = clr;
                if(clr.r + clr.g < clr.b || (clr.r + clr.g + clr.b) < 1.0f) {
                    label.color = Color.white;
                } else {
                    label.color = Color.black;
                }
                if(lh.lhd.style.isDualColor) {
                    clr = lh.lhd.style.color2;
                }
                if(clr.r + clr.g < clr.b || (clr.r + clr.g + clr.b) < 1.0f) {
                    label2.color = Color.white;
                } else {
                    label2.color = Color.black;
                }
                secondImage.color = clr;
                #endregion
            } else {
                label2.text = label.text = prefix + ((showBit && lh.Bit != 255) ? lh.Bit + ": " : "") + "Empty";
                label2.color = label.color = Color.white;
                secondImage.color = background.color = new Color(0, 0, 0, 0.45f);
            }
        }
        #endregion

        lastOptic = lh.lhd.optic;
        lastStyle = lh.lhd.style;
    }

    /// <summary>
    /// Coroutine.  Simulates pattern for preview.
    /// </summary>
    private IEnumerator PatternSim() {
        selectionImage.gameObject.SetActive(false); // Hide selection image

        label2.text = label.text = "";
        label2.color = label.color = Color.white;

        if(!lh.hasRealHead) {  // Head will not flash
            secondImage.color = background.color = new Color(0, 0, 0, 0f);
            yield return null;
        } else if(lh.GetCanEnable(BarManager.inst.funcBeingTested)) {
            // Fetch necessary patterns
            Pattern p1 = (lh.lhd.funcs.Contains(BasicFunction.TRAFFIC) || !(BarManager.inst.funcBeingTested == AdvFunction.TRAFFIC_LEFT || BarManager.inst.funcBeingTested == AdvFunction.TRAFFIC_RIGHT)) ? lh.GetPattern(BarManager.inst.funcBeingTested, false) : null;
            Pattern p2 = (lh.lhd.funcs.Contains(BasicFunction.TRAFFIC) || !(BarManager.inst.funcBeingTested == AdvFunction.TRAFFIC_LEFT || BarManager.inst.funcBeingTested == AdvFunction.TRAFFIC_RIGHT)) ? lh.GetPattern(BarManager.inst.funcBeingTested, true) : null;

            if(p1 != null) {
                bool phase1 = false, phase2 = false;
                TraffPatt.directLeft = (BarManager.inst.funcBeingTested == AdvFunction.TRAFFIC_LEFT);
                TraffPatt.sixHeads = false;

                if(!(p1 is TraffPatt)) { // Not a traffic pattern
                    phase1 = lh.GetPhaseB(BarManager.inst.funcBeingTested, false); // Fetch phases
                    phase2 = lh.GetPhaseB(BarManager.inst.funcBeingTested, true);
                } else { // Traffic pattern
                    byte count = 0;
                    foreach(LightHead alpha in BarManager.inst.allHeads) {
                        if(alpha.gameObject.activeInHierarchy && alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC)) {
                            count++;
                        }
                    }
                    TraffPatt.sixHeads = count == 6; // If 6 traffic heads, then use 6-head patterns
                }

                ulong ticksPast;
                byte bit = lh.Bit;

                #region Get Enables
                bool en1 = lh.GetIsEnabled(BarManager.inst.funcBeingTested, false, true),
                             en2 = lh.GetIsEnabled(BarManager.inst.funcBeingTested, true);

                if(BarManager.inst.funcBeingTested == AdvFunction.FTAKEDOWN || BarManager.inst.funcBeingTested == AdvFunction.FALLEY) { // If testing Flashing Pursuit or Flashing Alley...
                    for(byte i = 0; i < 20; i++) {
                        if(FnDragTarget.inputMap.Value[i] == 0xC00) { // If Flashing Pursuit and Flashing Alley share, show both
                            lh.GetIsEnabled(BarManager.inst.funcBeingTested == AdvFunction.FTAKEDOWN ? AdvFunction.FALLEY : AdvFunction.FTAKEDOWN, false, true);

                            if(!en1) {
                                p1 = lh.GetPattern(BarManager.inst.funcBeingTested == AdvFunction.FTAKEDOWN ? AdvFunction.FALLEY : AdvFunction.FTAKEDOWN, false);
                                en1 |= lh.GetIsEnabled(BarManager.inst.funcBeingTested == AdvFunction.FTAKEDOWN ? AdvFunction.FALLEY : AdvFunction.FTAKEDOWN, false);
                            }

                            if(!en2) {
                                p2 = lh.GetPattern(BarManager.inst.funcBeingTested == AdvFunction.FTAKEDOWN ? AdvFunction.FALLEY : AdvFunction.FTAKEDOWN, true);
                                en2 |= lh.GetIsEnabled(BarManager.inst.funcBeingTested == AdvFunction.FTAKEDOWN ? AdvFunction.FALLEY : AdvFunction.FTAKEDOWN, true);
                            }
                        }
                    }
                } 
                #endregion

                if(!lh.lhd.style.isDualColor) en2 = false; // If no dual color, forget about second color.

                bool light1 = en1, light2 = en2;

                if(en1 || en2) {  // If either one is enabled
                    while(BarManager.inst.funcBeingTested != AdvFunction.NONE) {
                        ticksPast = PattTimer.inst.passedTicks;  // figure out how long since we started

                        light1 = en1;
                        light2 = en2 && p2 != null;

                        #region Flash color 1
                        if(light1) {
                            if(p1 is DCCirclePattern) {
                                light1 &= ((DCCirclePattern)p1).GetIsActive(ticksPast, phase1, false, bit, lh.isRear);
                            } else if(p1 is DCDoubleRotatorPattern) {
                                light1 &= ((DCDoubleRotatorPattern)p1).GetIsActive(ticksPast, phase1, false, bit, lh.isRear);
                            } else {
                                light1 &= p1.GetIsActive(ticksPast, phase1, false, bit);
                            }
                        } 
                        #endregion
                        #region Flash color 2
                        if(light2) {
                            if(p2 is DCCirclePattern) {
                                light2 &= ((DCCirclePattern)p2).GetIsActive(ticksPast, phase2, true, bit, lh.isRear);
                            } else if(p2 is DCDoubleRotatorPattern) {
                                light2 &= ((DCDoubleRotatorPattern)p2).GetIsActive(ticksPast, phase2, true, bit, lh.isRear);
                            } else {
                                light2 &= p2.GetIsActive(ticksPast, phase2, true, bit);
                            }
                        } 
                        #endregion

                        #region Apply colors to simulate flashing
                        background.color = lh.lhd.style.color * (light1 ? 1.0f : 0.25f);
                        if(lh.lhd.style.isDualColor) {
                            secondImage.color = lh.lhd.style.color2 * (light2 ? 1.0f : 0.25f);
                        } else {
                            secondImage.color = background.color;
                        } 
                        #endregion

                        yield return null;
                    }
                }
            }
        }
        yield return null;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing
        if(cam == null) cam = FindObjectOfType<CameraControl>(); // Find Camera if not found already

        if(target != null) {
            #region Reposition self
            transform.position = target.position;
            transform.rotation = target.rotation;

            if(!target.gameObject.activeInHierarchy) {
                gameObject.SetActive(false);
            }
            #endregion

            if(lh == null) lh = target.GetComponent<LightHead>(); // Find LightHead of Target if not found already
            if(lh.lhd.style != lastStyle || lh.lhd.optic != lastOptic) {
                Refresh(); // Refresh now if styles / optics don't match
            }

            if(lh.Selected) { // Show selection image if selected
                selectionImage.gameObject.SetActive(true);
                selectionImage.transform.Rotate(new Vector3(0, 0, 20f) * Time.deltaTime); // Slow rotation
            } else { // Hide selection image if not selected
                selectionImage.gameObject.SetActive(false);
                selectionImage.transform.rotation = Quaternion.identity; // Revert to original orientation
            }
        }
    }

    /// <summary>
    /// Called when the mouse begins hovering over the object.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        if(tooltip == null) tooltip = FindObjectOfType<LabelTooltip>();  // Find tooltip if not found already
        if(label.text.Length > 0)
            tooltip.Show(label.text); // Put label's text on tooltip
    }

    /// <summary>
    /// Called when the mouse stops hovering over the object.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        tooltip.Hide();  // Hide tooltip
    }
}
