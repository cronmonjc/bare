using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UI Component, Issue.  Checks if two differing heads (where differing means different colors or different functions) share an output.
/// </summary>
public class SameOutputWarning : IssueChecker {

    /// <summary>
    /// The image used to represent the icon shown above each light label
    /// </summary>
    private Image icon;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        text = GetComponentInChildren<Text>();
        le = GetComponent<LayoutElement>();
        icon = GetComponentInChildren<Image>();
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        bool enable = BarManager.moddedBar && DoCheck();
        text.enabled = enable;
        le.ignoreLayout = !enable;
        icon.enabled = enable;
    }

    /// <summary>
    /// Examined to see whether or not the issue being examined arises.
    /// </summary>
    /// <returns>
    /// True if there is an issue, false if there is no issue.
    /// </returns>
    public override bool DoCheck() {
        bool rtn = false;

        if(BarManager.RefreshingBits) return false; // If the bar is re-assigning bits right now, do nothing

        LightHead[] front = new LightHead[16], rear = new LightHead[16]; // Set up reference slots
        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead alpha = BarManager.inst.allHeads[h];
            if(alpha.gameObject.activeInHierarchy && alpha.hasRealHead) { // If we're dealing with a real head...
                if(alpha.Bit == 255) {
                    Debug.LogWarning("An attempt was made to check LightHead " + alpha.transform.GetPath() + " for Same Output but that head has no bit assigned!", alpha);
                }
                if(alpha.myLabel != null) {
                    alpha.myLabel.DispError = false; // Clear out icons first...

                    byte bit = alpha.Bit;
                    LightHead[] array;
                    if(alpha.loc == Location.ALLEY || alpha.transform.position.y > 0)
                        array = front; // Dealing with front heads
                    else
                        array = rear; // Dealing with rear heads

                    if(array[bit] == null) { // Don't have reference, store it
                        array[bit] = alpha;
                    } else if(array[bit].myLabel.DispError) { // Reference is showing icon, so show icon here too
                        alpha.myLabel.DispError = true;
                    } else { // Test colors
                        LightHead vs = array[bit];
                        if(!vs.hasRealHead && alpha.hasRealHead) { // If reference was not a real head, replace and continue.  Insurance.
                            array[bit] = alpha;
                        } else if(vs.hasRealHead && alpha.hasRealHead) {
                            for(byte f = 0; f < vs.lhd.funcs.Count; f++) {
                                if(!alpha.lhd.funcs.Contains(vs.lhd.funcs[f])) { // Function lists don't match, display image
                                    vs.myLabel.DispError = true;
                                    alpha.myLabel.DispError = true;
                                    rtn = true;
                                }
                            }
                            if(vs.myLabel.DispError) continue; // If we're already displaying image, continue on to next head
                            for(byte f = 0; f < alpha.lhd.funcs.Count; f++) {
                                if(!vs.lhd.funcs.Contains(alpha.lhd.funcs[f])) { // Function lists don't match, display image
                                    vs.myLabel.DispError = true;
                                    alpha.myLabel.DispError = true;
                                    rtn = true;
                                }
                            }
                            if(vs.myLabel.DispError) continue; // If we're already displaying image, continue on to next head
                            if(vs.lhd.style != alpha.lhd.style) { // Head style doesn't match, display image
                                vs.myLabel.DispError = true;
                                alpha.myLabel.DispError = true;
                                rtn = true;
                            }
                        }
                    }
                }
            } else { // Does not have a real head
                if(alpha.myLabel != null)
                    alpha.myLabel.DispError = false; // Hide icon
            }
        }

        return rtn;
    }

    /// <summary>
    /// Gets the text used to describe an issue on the exported PDF.
    /// </summary>
    public override string pdfText {
        get { return "This bar has two or more heads that share the same output from the central control circuit, but differ in either appearance or function - if that output is turned on for the one head, it will turn on for all that share the output.  Cross reference the image above against the one provided on the fifth page of this document to determine which ones are sharing."; }
    }
}
