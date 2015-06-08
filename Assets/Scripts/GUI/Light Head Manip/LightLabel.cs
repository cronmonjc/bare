using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightLabel : MonoBehaviour {
    public Transform target;
    public Text label, label2, colorLabel;
    public Image background, secondImage, selectionImage, warnImage;

    private LightHead lh;

    public static CameraControl cam;
    public static bool showParts, showBit, showWire, wireOverride, alternateNumbering;
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
        showParts = showBit = showWire = wireOverride = alternateNumbering = false;
        Refresh();
    }

    public void Refresh(bool showHeadNumber = false) {
        colorLabel.text = "";

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
        } else if(showWire) {
            if(lh.lhd.style != null) {
                string t = prefix;
                byte bit = lh.Bit;

                Color labelColor = Color.white;

                if(!wireOverride) {
                    if(transform.position.y < 0) {
                        if(bit > 5) {
                            t = t + "P10-";
                        } else {
                            t = t + "P9-";
                        }

                        switch(lh.Bit) {
                            case 5:
                            case 6:
                                t = t + "1";
                                labelColor = new Color(0.5f, 0.25f, 0.0f);
                                colorLabel.text = "Brown";
                                break;
                            case 4:
                            case 7:
                                t = t + "2";
                                labelColor = Color.yellow;
                                colorLabel.text = "Yellow";
                                break;
                            case 3:
                            case 8:
                                t = t + "3";
                                labelColor = Color.green;
                                colorLabel.text = "Green";
                                break;
                            case 2:
                            case 9:
                                t = t + "4";
                                labelColor = Color.blue;
                                colorLabel.text = "Blue";
                                break;
                            case 1:
                            case 10:
                                t = t + "5";
                                labelColor = new Color(0.5f, 0.0f, 1.0f);
                                colorLabel.text = "Purple";
                                break;
                            case 0:
                            case 11:
                                t = t + "6";
                                colorLabel.text = "White";
                                break;
                            default:
                                t = t + "?";
                                break;
                        }
                    } else {
                        if(bit > 5 && bit != 12) {
                            t = t + "P8-";
                        } else {
                            t = t + "P3-";
                        }

                        switch(bit) {
                            case 5:
                            case 6:
                                t = t + (lh.FarWire[BarManager.inst.BarSize] ? "2" : "1");
                                labelColor = (lh.FarWire[BarManager.inst.BarSize] ? Color.yellow : new Color(0.5f, 0.25f, 0.0f));
                                colorLabel.text = (lh.FarWire[BarManager.inst.BarSize] ? "Yellow" : "Brown");
                                break;
                            case 4:
                            case 7:
                                labelColor = Color.green;
                                colorLabel.text = "Green";
                                t = t + "3";
                                break;
                            case 1:
                            case 10:
                                t = t + "4";
                                labelColor = Color.blue;
                                colorLabel.text = "Blue";
                                break;
                            case 0:
                            case 11:
                                t = t + "5";
                                labelColor = new Color(0.5f, 0.0f, 1.0f);
                                colorLabel.text = "Purple";
                                break;
                            case 12:
                            case 13:
                                t = t + "6";
                                colorLabel.text = "White";
                                break;
                            default:
                                t = t + "?";
                                break;
                        }
                    }

                    secondImage.color = background.color = labelColor;
                    if(labelColor.r + labelColor.g < labelColor.b) {
                        label2.color = label.color = Color.white;
                    } else {
                        label2.color = label.color = Color.black;
                    }

                    if(lh.lhd.style.isDualColor) {
                        t = t + " C & ";
                        if(transform.position.y < 0) {
                            if(bit > 5) {
                                t = t + "P10-";
                            } else {
                                t = t + "P9-";
                            }

                            switch(bit) {
                                case 5:
                                case 6:
                                    t = t + "7";
                                    break;
                                case 4:
                                case 7:
                                    t = t + "8";
                                    break;
                                case 3:
                                case 8:
                                    t = t + "9";
                                    break;
                                case 2:
                                case 9:
                                    t = t + "10";
                                    break;
                                default:
                                    t = t + "?";
                                    break;
                            }
                        } else {
                            if(bit > 5 && bit != 12) {
                                t = t + "P8-";
                            } else {
                                t = t + "P3-";
                            }

                            switch(bit) {
                                case 5:
                                case 6:
                                    t = t + (lh.FarWire[BarManager.inst.BarSize] ? "8" : "7");
                                    break;
                                case 4:
                                case 7:
                                    t = t + "9";
                                    break;
                                case 1:
                                case 10:
                                    t = t + "10";
                                    break;
                                case 0:
                                case 11:
                                    t = t + "11";
                                    break;
                                case 12:
                                case 13:
                                    t = t + "12";
                                    break;
                                default:
                                    t = t + "?";
                                    break;
                            }
                        }
                        t = t + " W";
                    }
                } else {
                    t = t + "\n\n";

                    label2.color = label.color = Color.black;
                    secondImage.color = background.color = Color.white;
                }

                label2.text = label.text = t;
            }
        } else {
            if(lh.lhd.style != null) {
                label2.text = label.text = prefix + (showBit ? lh.Bit + ": " : "") + (lh.lhd.optic.styles.Count > 1 ? lh.lhd.style.name + " " : "") + lh.lhd.optic.name;
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
                label2.text = label.text = prefix + (showBit ? lh.Bit + ": " : "") + "Empty";
                label2.color = label.color = Color.white;
                secondImage.color = background.color = new Color(0, 0, 0, 0.45f);
            }
        }

        lastOptic = lh.lhd.optic;
        lastStyle = lh.lhd.style;
    }

    void Update() {
        if(CameraControl.funcBeingTested != AdvFunction.NONE) return;
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
