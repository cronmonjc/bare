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
            int size = BarManager.inst.BarSize;

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
                        if(lh.lhd.optic != null) {
                            if(alpha.lhd.funcs.Contains(BasicFunction.TRAFFIC) && alpha.shouldBeTD) {
                                BarManager.inst.td = TDOption.NONE;
                                foreach(LightHead beta in BarManager.inst.allHeads) {
                                    beta.shouldBeTD = false;
                                }
                            }
                            alpha.lhd.funcs.Clear();
                            foreach(BasicFunction f in lh.lhd.funcs) {
                                alpha.AddBasicFunction(f, false);
                            }
                            alpha.RefreshBasicFuncDefault();

                            if(alpha.lhd.optic.styles[lh.lhd.style.name].selectable)
                                alpha.SetStyle(lh.lhd.style);

                        }
                    }
                }
                foreach(LightHead alpha in lhs) {
                    alpha.Selected = alpha.isSmall ^ value;
                }
            }
        }
    }


    void Start() {
        longGO = transform.FindChild("L").gameObject;
        shortGO = transform.FindChild("DS").gameObject;

        ShowLong = true;
    }

}
