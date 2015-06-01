using UnityEngine;
using System.Collections;

public class SizeOptionControl : MonoBehaviour {

    private GameObject longGO, shortGO;

    public bool[] canLong = new bool[] { true, true, true, true, true }, canShort = new bool[] { true, true, true, true, true };

    public bool ShowLong {
        get {
            return longGO.activeInHierarchy;
        }
        set {
            int size = FindObjectOfType<BarManager>().BarSize;

            LightHead[] lhs = transform.GetComponentsInChildren<LightHead>(true);
            LightHead lh = null;
            foreach(LightHead alpha in lhs) {
                if(alpha.Selected && ((value && alpha.isSmall) || (!value && !alpha.isSmall))) {
                    lh = alpha;
                    break;
                }
            }

            if(canLong[size] && !canShort[size]) {
                longGO.SetActive(true);
                shortGO.SetActive(false);
            } else if(!canLong[size] && canShort[size]) {
                longGO.SetActive(false);
                shortGO.SetActive(true);
            } else {
                longGO.SetActive(value && canLong[size]);
                shortGO.SetActive(!value && canShort[size]);
            }
            if(lh != null) {
                foreach(LightHead alpha in lhs) {
                    if(!alpha.Selected) {
                        alpha.Selected = lh.Selected;
                        if(lh.lhd.optic != null) {
                            alpha.lhd.funcs.Clear();
                            foreach(BasicFunction f in lh.lhd.funcs) {
                                alpha.AddBasicFunction(f, false);
                            }

                            if(longGO.activeInHierarchy) {
                                if(lh.lhd.optic.lgEquivalent.Length > 0) {
                                    alpha.SetOptic(lh.lhd.optic.lgEquivalent, BasicFunction.NULL, false);
                                    alpha.SetStyle(lh.lhd.style);
                                }
                            } else {
                                if(lh.lhd.optic.smEquivalent.Length > 0) {
                                    alpha.SetOptic(lh.lhd.optic.smEquivalent, BasicFunction.NULL, false);
                                    alpha.SetStyle(lh.lhd.style);
                                }
                            }
                        }
                    }
                }
                lh.Selected = false;
            }
        }
    }


    void Start() {
        longGO = transform.FindChild("Long").gameObject;
        shortGO = transform.FindChild("DoubleShort").gameObject;

        ShowLong = true;
    }

}
