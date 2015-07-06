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
        bool enable = DoCheck();
        text.enabled = enable;
        le.ignoreLayout = !enable;
        icon.enabled = enable;
    }

    public override bool DoCheck() {
        bool rtn = false;

        if(BarManager.RefreshingBits) return false;

        LightHead[] front = new LightHead[16], rear = new LightHead[16];
        foreach(LightHead alpha in BarManager.inst.allHeads) {
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
                            foreach(BasicFunction f in vs.lhd.funcs) {
                                if(!alpha.lhd.funcs.Contains(f)) {
                                    vs.myLabel.DispError = true;
                                    alpha.myLabel.DispError = true;
                                    rtn = true;
                                }
                            }
                            if(vs.myLabel.DispError) continue;
                            foreach(BasicFunction f in alpha.lhd.funcs) {
                                if(!vs.lhd.funcs.Contains(f)) {
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
            }
        }

        return rtn;
    }

    public override string pdfText {
        get { return "This bar has two or more heads that share the same output from the central control circuit, but differ in either appearance or function - if that output is turned on for the one head, it will turn on for all that share the output.  Cross reference the image above against the one provided on the fifth page of this document to determine which ones are sharing."; }
    }
}
