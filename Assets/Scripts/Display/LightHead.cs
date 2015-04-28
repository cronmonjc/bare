using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightHead : MonoBehaviour {
    public Location loc;

    private static CameraControl cam;
    private static SelBoxCollider sbc;
    public Material selected, idle;

    public bool isSmall;
    public LightHeadDefinition lhd;
    [System.NonSerialized]
    public List<Function> selectedFunctions;

    public List<Function> CapableFunctions {
        get { return LightDict.inst.capableFunctions[loc]; }
    }

    private LightLabel myLabel;

    public Light[] myLights;

    void Awake() {
        lhd = new LightHeadDefinition();

        selectedFunctions = new List<Function>();
    }

    void Start() {
        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
        }

        GameObject go = GameObject.Instantiate(cam.LabelPrefab) as GameObject;
        myLabel = go.GetComponent<LightLabel>();
        myLabel.target = transform;
        myLabel.transform.SetParent(cam.LabelParent);

        go = GameObject.Instantiate(cam.PushpinPrefab) as GameObject;
        go.GetComponent<Pushpin>().target = transform;
        go.GetComponent<Pushpin>().transform.SetParent(cam.PushpinParent);

        myLights = GetComponentsInChildren<Light>(true);
    }

    void Update() {
        if(cam == null) {
            cam = FindObjectOfType<CameraControl>();
        }
        if(sbc == null) {
            sbc = cam.SelBox.GetComponent<SelBoxCollider>();
        }

        if(cam.Selected.Contains(this)) {
            GetComponent<MeshRenderer>().material = selected;
        } else {
            GetComponent<MeshRenderer>().material = idle;
        }

        if(!myLabel.gameObject.activeInHierarchy) {
            myLabel.gameObject.SetActive(true);
        }
    }

    public void SetOptic(OpticNode newOptic) {
        if(newOptic == null) SetOptic("");
        else SetOptic(newOptic.name);
    }

    public void SetOptic(string newOptic, bool doDefault = true) {
        if(newOptic.Length > 0) {
            lhd.optic = LightDict.inst.FetchOptic(loc, newOptic);
            if(doDefault && selectedFunctions.Count > 0) {
                Function highFunction = Function.LEVEL1;
                foreach(Function f in selectedFunctions) {
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
        } else {
            lhd.optic = null;
            SetStyle(null);
        }


    }

    public void SetStyle(StyleNode newStyle) {
        if(newStyle == null) lhd.style = null;
        else lhd.style = lhd.optic.styles[newStyle.name];
    }
}
