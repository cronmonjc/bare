using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;
using System.Text;

public class LightLabel : MonoBehaviour {
    public Transform target;
    public Text label, label2, colorLabel;
    public Image background, secondImage, selectionImage, warnImage;

    private LightHead lh;

    public static CameraControl cam;
    public static bool showParts, showBit, showJustBit, showWire, colorlessWire, wireOverride, alternateNumbering, showPatt;
    private OpticNode lastOptic;
    private StyleNode lastStyle;

    public bool DispError {
        get { return warnImage.enabled; }
        set { warnImage.enabled = value; }
    }

    void Start() {
        selectionImage.gameObject.SetActive(false);
        selectionImage.transform.rotation = Quaternion.identity;
        if(lh == null) lh = target.GetComponent<LightHead>();
        if(lh.isSmall) {
            ((RectTransform)transform).sizeDelta = new Vector2(65, 48);
        }
        showParts = showBit = showJustBit = showWire = colorlessWire = wireOverride = alternateNumbering = false;
        Refresh();
    }

    public void Refresh(bool showHeadNumber = false) {
        colorLabel.text = "";

        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) {
            StartCoroutine(PatternSim());
            return;
        }

        string prefix = "";

        if(showHeadNumber) {
            if(alternateNumbering) {
                if(BarManager.altHeadNumber.ContainsKey(lh))
                    prefix = "(" + BarManager.altHeadNumber[lh] + ") ";
                else {
                    Debug.LogError("Failed to find a head ID for " + lh.transform.GetPath());
                    prefix = "(?) ";
                }
            } else {
                for(int i = 0; i < BarManager.headNumber.Length; i++) {
                    if(BarManager.headNumber[i] == lh) {
                        prefix = "(" + (i + 1) + ") ";
                        break;
                    }
                }
                if(prefix == "") {
                    Debug.LogError("Failed to find a head number for " + lh.transform.GetPath());
                    prefix = "(?) ";
                }
            }
        }

        if(showParts) {
            if(lh.lhd.style != null) {
                label2.text = label.text = prefix + lh.PartNumber;
                label2.color = label.color = Color.black;
                secondImage.color = background.color = Color.white;
            }
        } else if(showJustBit) {
            if(lh.hasRealHead) {
                label2.text = label.text = ((lh.Bit != 255) ? (lh.Bit + 1) + "" : "None");
                label2.color = label.color = Color.black;
                secondImage.color = background.color = Color.white;
            } else {
                label2.text = label.text = "";
                secondImage.color = background.color = new Color(0f, 0f, 0f, 0f);
            }
        } else if(showWire) {
            if(lh.hasRealHead) {
                byte bit = lh.Bit;

                Color labelColor = Color.white;

                if(!wireOverride) {
                    if(lh.transform.position.y < 0) {
                        switch(lh.Bit) {
                            case 5:
                            case 6:
                                labelColor = new Color(0.5f, 0.25f, 0.0f);
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
                                labelColor = new Color(0.5f, 0.0f, 1.0f);
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
                                labelColor = (lh.FarWire ? Color.yellow : new Color(0.5f, 0.25f, 0.0f));
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
                    if(labelColor.r + labelColor.g < labelColor.b) {
                        label2.color = label.color = Color.white;
                    } else {
                        label2.color = label.color = Color.black;
                    }

                    label2.text = label.text = prefix + BarManager.GetWire(lh);

                } else {
                    label2.color = label.color = Color.black;
                    secondImage.color = background.color = Color.white;

                    label2.text = label.text = prefix + "\n\n";
                }

                if(colorlessWire) {
                    label2.color = label.color = Color.black;
                    secondImage.color = background.color = Color.white;
                    colorLabel.text = "";
                }
            }
        } else if(showPatt) {
            if(lh.hasRealHead) {
                bool canEnable;
                switch(FunctionEditPane.currFunc) {
                    case AdvFunction.LEVEL1:
                    case AdvFunction.LEVEL2:
                    case AdvFunction.LEVEL3:
                    case AdvFunction.LEVEL4:
                    case AdvFunction.LEVEL5:
                    case AdvFunction.FALLEY:
                    case AdvFunction.FTAKEDOWN:
                    case AdvFunction.ICL:
                        canEnable = lh.lhd.funcs.Contains(BasicFunction.FLASHING);
                        break;
                    case AdvFunction.TAKEDOWN:
                    case AdvFunction.ALLEY_LEFT:
                    case AdvFunction.ALLEY_RIGHT:
                        canEnable = lh.lhd.funcs.Contains(BasicFunction.STEADY);
                        break;
                    case AdvFunction.TURN_LEFT:
                    case AdvFunction.TURN_RIGHT:
                    case AdvFunction.TAIL:
                        canEnable = lh.lhd.funcs.Contains(BasicFunction.STT);
                        break;
                    case AdvFunction.T13:
                        canEnable = lh.lhd.funcs.Contains(BasicFunction.CAL_STEADY);
                        break;
                    case AdvFunction.TRAFFIC_LEFT:
                    case AdvFunction.TRAFFIC_RIGHT:
                        canEnable = lh.lhd.funcs.Contains(BasicFunction.TRAFFIC);
                        break;
                    case AdvFunction.CRUISE:
                        canEnable = lh.lhd.funcs.Contains(BasicFunction.CRUISE);
                        break;
                    case AdvFunction.DIM:
                        canEnable = true;
                        break;
                    case AdvFunction.EMITTER:
                        canEnable = lh.lhd.funcs.Contains(BasicFunction.EMITTER);
                        break;
                    default:
                        canEnable = false;
                        break;
                }

                NbtCompound patts = BarManager.inst.patts;
                string cmpdName = BarManager.GetFnString(lh.transform, FunctionEditPane.currFunc);
                if(cmpdName == null) {
                    Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                    return;
                }

                NbtCompound func = patts.Get<NbtCompound>(cmpdName);

                string t = ((showBit && lh.Bit != 255) ? lh.Bit + ": " : "");

                if(func.Contains("e" + (lh.transform.position.y < 0 ? "r" : "f") + "1")) {
                    bool thisEnabled1 = ((func.Get<NbtShort>("e" + (lh.transform.position.y < 0 ? "r" : "f") + "1").ShortValue & (0x1 << lh.Bit)) > 0),
                         thisEnabled2 = ((func.Get<NbtShort>("e" + (lh.transform.position.y < 0 ? "r" : "f") + "2").ShortValue & (0x1 << lh.Bit)) > 0);

                    thisEnabled1 &= canEnable;
                    thisEnabled2 &= canEnable;

                    Color clr = lh.lhd.style.color;
                    if(!thisEnabled1) clr.a = 0.25f;
                    background.color = clr;
                    if(!thisEnabled1 || clr.r + clr.g < clr.b) {
                        label.color = Color.white;
                    } else {
                        label.color = Color.black;
                    }
                    if(lh.lhd.style.isDualColor) {
                        clr = lh.lhd.style.color2;
                        if(!thisEnabled2) clr.a = 0.25f;
                    }
                    if((!lh.lhd.style.isDualColor && !thisEnabled1) || (lh.lhd.style.isDualColor && !thisEnabled2) || clr.r + clr.g < clr.b) {
                        label2.color = Color.white;
                    } else {
                        label2.color = Color.black;
                    }
                    secondImage.color = clr;

                    if(thisEnabled1) {
                        if(func.Contains("pat1")) {
                            bool thisPhase = ((func.Get<NbtShort>("p" + (lh.transform.position.y < 0 ? "r" : "f") + "1").ShortValue & (0x1 << lh.Bit)) > 0);

                            Pattern pat = lh.GetPattern(FunctionEditPane.currFunc, false);
                            if(pat == null)
                                t = t + "No Patt";
                            else {
                                t = t + pat.name + (pat is FlashPatt ? (thisPhase ? " B" : " A") : "");
                            }
                        } else {
                            t = t + "Enabled";
                        }
                    } else {
                        t = t + "Disabled";
                    }
                    if(lh.lhd.style.isDualColor) {
                        if(thisEnabled2) {
                            if(func.Contains("pat2")) {
                                bool thisPhase = ((func.Get<NbtShort>("p" + (lh.transform.position.y < 0 ? "r" : "f") + "2").ShortValue & (0x1 << lh.Bit)) > 0);

                                Pattern pat = lh.GetPattern(FunctionEditPane.currFunc, true);
                                if(pat == null)
                                    t = t + " / No Patt";
                                else
                                    t = t + " / " + pat.name + (pat is FlashPatt ? (thisPhase ? " B" : " A") : "");
                            } else {
                                t = t + " / Enabled";
                            }
                        } else {
                            t = t + " / Disabled";
                        }
                    }
                } else {
                    Color clr = lh.lhd.style.color;
                    clr.a = 0.25f;
                    background.color = clr;
                    if(lh.lhd.style.isDualColor) {
                        clr = lh.lhd.style.color2;
                        clr.a = 0.25f;
                    }
                    secondImage.color = clr;
                    label2.color = label.color = Color.white;
                    t = t + "Disabled";
                }

                label2.text = label.text = t;

            } else {
                label2.text = label.text = ((showBit && lh.Bit != 255) ? lh.Bit + ": " : "") + "No Head";
                label2.color = label.color = Color.white;
                secondImage.color = background.color = new Color(0, 0, 0, 0.45f);
            }
        } else {
            if(lh.lhd.style != null) {
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
                        case BasicFunction.EMITTER:
                            chars = "E";
                            break;
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

                label2.text = label.text = prefix + ((showBit && lh.Bit != 255) ? lh.Bit + ": " : "") + sb.ToString() + (lh.lhd.optic.styles.Count > 1 ? lh.lhd.style.name + " " : "") + lh.lhd.optic.name;
                Color clr = lh.lhd.style.color;
                background.color = clr;
                if(clr.r + clr.g < clr.b) {
                    label.color = Color.white;
                } else {
                    label.color = Color.black;
                }
                if(lh.lhd.style.isDualColor) {
                    clr = lh.lhd.style.color2;
                }
                if(clr.r + clr.g < clr.b) {
                    label2.color = Color.white;
                } else {
                    label2.color = Color.black;
                }
                secondImage.color = clr;
            } else {
                label2.text = label.text = prefix + ((showBit && lh.Bit != 255) ? lh.Bit + ": " : "") + "Empty";
                label2.color = label.color = Color.white;
                secondImage.color = background.color = new Color(0, 0, 0, 0.45f);
            }
        }

        lastOptic = lh.lhd.optic;
        lastStyle = lh.lhd.style;
    }

    private IEnumerator PatternSim() {
        selectionImage.gameObject.SetActive(false);

        if(lh.lhd.style == null) {
            label2.text = label.text = "";
            label2.color = label.color = Color.white;
            secondImage.color = background.color = new Color(0, 0, 0, 0f);
            yield return null;
        } else {
            NbtCompound patts = BarManager.inst.patts;
            string cmpdName = BarManager.GetFnString(lh.transform, FunctionEditPane.currFunc);
            if(cmpdName == null) {
                Debug.LogWarning(FunctionEditPane.currFunc.ToString() + " has no similar setting in the data bytes.");
                yield return null;
            } else {
                label2.text = label.text = "";

                NbtCompound func = patts.Get<NbtCompound>(cmpdName);

                Pattern p1 = (lh.lhd.funcs.Contains(BasicFunction.TRAFFIC) || !(FunctionEditPane.currFunc == AdvFunction.TRAFFIC_LEFT || FunctionEditPane.currFunc == AdvFunction.TRAFFIC_RIGHT)) ? lh.GetPattern(BarManager.inst.funcBeingTested, false) : null;
                Pattern p2 = (lh.lhd.funcs.Contains(BasicFunction.TRAFFIC) || !(FunctionEditPane.currFunc == AdvFunction.TRAFFIC_LEFT || FunctionEditPane.currFunc == AdvFunction.TRAFFIC_RIGHT)) ? lh.GetPattern(BarManager.inst.funcBeingTested, true) : null;

                if(p1 != null) {
                    Debug.Log(lh.transform.GetPath() + " : " + p1.name);

                    bool phase1 = false, phase2 = false;
                    bool directLeft = (FunctionEditPane.currFunc == AdvFunction.TRAFFIC_LEFT);
                    bool sixHeads = false;
                    short[] list = new short[0];

                    if(!(p1 is TraffPatt)) {
                        phase1 = ((func.Get<NbtShort>("p" + (lh.transform.position.y < 0 ? "r" : "f") + "1").ShortValue & (0x1 << lh.Bit)) > 0);
                        phase2 = ((func.Get<NbtShort>("p" + (lh.transform.position.y < 0 ? "r" : "f") + "2").ShortValue & (0x1 << lh.Bit)) > 0);
                    } else {
                        byte count = 0;
                        foreach(LightHead alpha in BarManager.inst.allHeads) {
                            if(alpha.gameObject.activeInHierarchy && alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC)) {
                                count++;
                            }
                        }
                        sixHeads = count == 6;
                        list = (directLeft ? (sixHeads ? ((TraffPatt)p1).left6 : ((TraffPatt)p1).left8) : (sixHeads ? ((TraffPatt)p1).right6 : ((TraffPatt)p1).right8));
                    }

                    ulong p1period = p1.period, p2period = (p2 == null ? 0uL : p2.period);

                    ulong ticksPast;
                    byte bit = lh.Bit;

                    bool en1 = (func["e" + (lh.transform.position.y < 0 ? "r" : "f") + "1"].ShortValue & (0x1 << bit)) > 0,
                         en2 = (func["e" + (lh.transform.position.y < 0 ? "r" : "f") + "2"].ShortValue & (0x1 << bit)) > 0;

                    if(!lh.lhd.style.isDualColor) en2 = false;

                    if(en1 || en2) {
                        while(BarManager.inst.funcBeingTested != AdvFunction.NONE) {
                            ticksPast = PattTimer.inst.passedTicks;

                            background.color = lh.lhd.style.color * ((en1 && p1.GetIsActive(ticksPast, phase2, true, bit)) ? 1.0f : 0.25f);
                            if(lh.lhd.style.isDualColor) {
                                secondImage.color = lh.lhd.style.color2 * ((en2 && p2 != null && p2.GetIsActive(ticksPast, phase2, true, bit)) ? 1.0f : 0.25f);
                            } else {
                                secondImage.color = background.color;
                            }

                            yield return null;
                        }
                    }
                }
            }
        }
        yield return null;
    }

    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        else {
            if(target != null) {
                transform.position = target.position;
                transform.rotation = target.rotation;

                if(!target.gameObject.activeInHierarchy) {
                    gameObject.SetActive(false);
                }
            }
        }

        if(lh == null) lh = target.GetComponent<LightHead>();
        if(lh.lhd.style != lastStyle || lh.lhd.optic != lastOptic) {
            Refresh();
        }

        if(lh.Selected) {
            selectionImage.gameObject.SetActive(true);
            selectionImage.transform.Rotate(new Vector3(0, 0, 20f) * Time.deltaTime);
        } else {
            selectionImage.gameObject.SetActive(false);
            selectionImage.transform.rotation = Quaternion.identity;
        }
    }
}
