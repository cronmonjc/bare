using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SameOutputWarning : IssueChecker {

    private Image icon;

    void Start() {
        text = GetComponentInChildren<Text>();
        le = GetComponent<LayoutElement>();
        icon = GetComponentInChildren<Image>();
    }

    void Update() {
        bool enable = BarManager.moddedBar && DoCheck();
        text.enabled = enable;
        le.ignoreLayout = !enable;
        icon.enabled = enable;
    }

    public override bool DoCheck() {
        bool rtn = false;

        if(BarManager.RefreshingBits) return false;

        LightHead[] front = new LightHead[16], rear = new LightHead[16];
        for(byte h = 0; h < BarManager.inst.allHeads.Count; h++) {
            LightHead alpha = BarManager.inst.allHeads[h];
            if(alpha.gameObject.activeInHierarchy && alpha.hasRealHead) {
                if(alpha.myLabel != null) {
                    alpha.myLabel.DispError = false;

                    byte bit = alpha.Bit;
                    LightHead[] array;
                    if(alpha.loc == Location.ALLEY || alpha.transform.position.y > 0)
                        array = front;
                    else
                        array = rear;

                    if(array[bit] == null) {
                        array[bit] = alpha;
                    } else if(array[bit].myLabel.DispError) {
                        alpha.myLabel.DispError = true;
                    } else {
                        LightHead vs = array[bit];
                        if(!vs.hasRealHead && alpha.hasRealHead) {
                            array[bit] = alpha;
                        } else if(vs.hasRealHead && alpha.hasRealHead) {
                            for(byte f = 0; f < vs.lhd.funcs.Count; f++) {
                                if(!alpha.lhd.funcs.Contains(vs.lhd.funcs[f])) {
                                    vs.myLabel.DispError = true;
                                    alpha.myLabel.DispError = true;
                                    rtn = true;
                                }
                            }
                            if(vs.myLabel.DispError) continue;
                            for(byte f = 0; f < alpha.lhd.funcs.Count; f++) {
                                if(!vs.lhd.funcs.Contains(alpha.lhd.funcs[f])) {
                                    vs.myLabel.DispError = true;
                                    alpha.myLabel.DispError = true;
                                    rtn = true;
                                }
                            }
                            if(vs.myLabel.DispError) continue;
                            if(vs.lhd.style != alpha.lhd.style) {
                                vs.myLabel.DispError = true;
                                alpha.myLabel.DispError = true;
                                rtn = true;
                            }
                        }
                    }
                }
            } else {
                if(alpha.myLabel != null)
                    alpha.myLabel.DispError = false;
            }
        }

        return rtn;
    }

    public override string pdfText {
        get { return "This bar has two or more heads that share the same output from the central control circuit, but differ in either appearance or function - if that output is turned on for the one head, it will turn on for all that share the output.  Cross reference the image above against the one provided on the fifth page of this document to determine which ones are sharing."; }
    }
}
