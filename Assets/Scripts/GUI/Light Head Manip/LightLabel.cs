using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightLabel : MonoBehaviour {
    public Transform target;
    public Text label, label2;
    public Image background, secondImage, selectionImage, warnImage;

    private LightHead lh;

    public static CameraControl cam;
    public static bool showParts, showBit, showWire, wireOverride;
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
        showParts = showBit = showWire = wireOverride = false;
        Refresh();
    }

    public void Refresh(bool showHeadNumber = false) {
        string prefix = "";

        if(showHeadNumber) {
            for(int i = 0; i < BarManager.headNumber.Length; i++) {
                if(BarManager.headNumber[i] == lh) {
                    prefix = "(" + (i + 1) + ") ";
                    break;
                }
            }
            if(prefix == "") {
                Debug.LogError("Failed to find a head number for " + transform.GetPath());
                prefix = "(?) ";
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

                if(!wireOverride) {
                    if(transform.position.y < 0) {
                        if(transform.position.x > 0) {
                            t = t + "P10-";
                        } else {
                            t = t + "P9-";
                        }

                        switch(lh.Bit) {
                            case 5:
                            case 6:
                                t = t + "1";
                                break;
                            case 4:
                            case 7:
                                t = t + "2";
                                break;
                            case 3:
                            case 8:
                                t = t + "3";
                                break;
                            case 2:
                            case 9:
                                t = t + "4";
                                break;
                            case 1:
                            case 10:
                                t = t + "5";
                                break;
                            case 0:
                            case 11:
                                t = t + "6";
                                break;
                            default:
                                t = t + "?";
                                break;
                        }
                    } else {
                        if(transform.position.x > 0) {
                            t = t + "P8-";
                        } else {
                            t = t + "P3-";
                        }

                        switch(lh.Bit) {
                            case 5:
                            case 6:
                                t = t + (lh.FarWire[BarManager.inst.BarSize] ? "2" : "1");
                                break;
                            case 4:
                            case 7:
                                t = t + "3";
                                break;
                            case 1:
                            case 10:
                                t = t + "4";
                                break;
                            case 0:
                            case 11:
                                t = t + "5";
                                break;
                            case 12:
                            case 13:
                                t = t + "6";
                                break;
                            default:
                                t = t + "?";
                                break;
                        }
                    }

                    if(lh.lhd.style.isDualColor) {
                        t = t + " C & ";
                        if(transform.position.y < 0) {
                            if(transform.position.x > 0) {
                                t = t + "P10-";
                            } else {
                                t = t + "P9-";
                            }

                            switch(lh.Bit) {
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
                            if(transform.position.x > 0) {
                                t = t + "P8-";
                            } else {
                                t = t + "P3-";
                            }

                            switch(lh.Bit) {
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
                    t = t + "\n\n\n";
                }

                label2.text = label.text = t;
                label2.color = label.color = Color.black;
                secondImage.color = background.color = Color.white;
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
        if(lh.lhd.style != lastStyle) {
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
