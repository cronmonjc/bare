using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Component.  Manages all of the AdvFuncDisp Components.
/// </summary>
public class AdvPattDisp : MonoBehaviour {
    /// <summary>
    /// Reference to CameraControl Component to easily tell what's selected
    /// </summary>
    private CameraControl cam;
    /// <summary>
    /// Reference to the color text, allowing indication of what color the selected heads are.  Set via Unity Inspector.
    /// </summary>
    public Text color1, color2;
    /// <summary>
    /// The dictionary of AdvFuncDisps
    /// </summary>
    public Dictionary<AdvFunction, AdvFuncDisp> displays;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        if(displays == null) {
            displays = new Dictionary<AdvFunction, AdvFuncDisp>();
            foreach(AdvFuncDisp alpha in GetComponentsInChildren<AdvFuncDisp>(true)) { // Generate dictionary
                displays[alpha.func] = alpha;
            }
        }
    }

    /// <summary>
    /// Refreshes this Component.
    /// </summary>
    public void Refresh() {
        if(displays == null) {
            displays = new Dictionary<AdvFunction, AdvFuncDisp>();
            foreach(AdvFuncDisp alpha in GetComponentsInChildren<AdvFuncDisp>(true)) { // Generate dictionary, just in case it doesn't exist
                displays[alpha.func] = alpha;
            }
        }
        foreach(AdvFuncDisp alpha in displays.Values) { // Hide all displays
            alpha.gameObject.SetActive(false);
        }

        HashSet<AdvFunction> funcs = new HashSet<AdvFunction>(); // Make list of functions

        if(cam == null) cam = FindObjectOfType<CameraControl>(); // Get cam reference if we don't already have it

        color1.text = color2.text = "";

        foreach(LightHead alpha in cam.SelectedHead) {
            if(!alpha.hasRealHead) continue; // If it doesn't have a real head, go to next head
            #region Compile set of functions in use
            foreach(BasicFunction beta in alpha.lhd.funcs) {
                switch(beta) {
                    case BasicFunction.CAL_STEADY:
                        funcs.Add(AdvFunction.T13);
                        break;
                    case BasicFunction.CRUISE:
                        funcs.Add(AdvFunction.CRUISE);
                        break;
                    case BasicFunction.EMITTER:
                        funcs.Add(AdvFunction.EMITTER);
                        break;
                    case BasicFunction.FLASHING:
                        foreach(AdvFunction charlie in LightDict.flashingFuncs) {
                            funcs.Add(charlie);
                        }
                        break;
                    case BasicFunction.STEADY:
                        funcs.Add(AdvFunction.TAKEDOWN);
                        funcs.Add(AdvFunction.ALLEY_LEFT);
                        funcs.Add(AdvFunction.ALLEY_RIGHT);
                        break;
                    case BasicFunction.STT:
                        funcs.Add(AdvFunction.TURN_LEFT);
                        funcs.Add(AdvFunction.TURN_RIGHT);
                        break;
                    case BasicFunction.TRAFFIC:
                        funcs.Add(AdvFunction.TRAFFIC_LEFT);
                        funcs.Add(AdvFunction.TRAFFIC_RIGHT);
                        break;
                    default:
                        break;
                }
            }
            funcs.Add(AdvFunction.DIM); 
            #endregion

            #region Get color names to apply to texts on header
            string[] colors = alpha.lhd.style.name.Split('/', '\\');
            if(colors.Length == 2) {
                if(color2.text.Equals("")) {
                    color2.text = colors[1];
                } else if(!color2.text.Equals(colors[1], System.StringComparison.CurrentCultureIgnoreCase)) {
                    color2.text = "Color 2";
                }
            }
            if(color1.text.Equals("")) {
                color1.text = colors[0];
            } else if(!color1.text.Equals(colors[0], System.StringComparison.CurrentCultureIgnoreCase)) {
                color1.text = "Color 1";
            } 
            #endregion
        }

        foreach(AdvFunction alpha in funcs) {
            displays[alpha].gameObject.SetActive(true); // Show displays for functions that are a part of the selected heads
            displays[alpha].Refresh();
        }
    }
}
