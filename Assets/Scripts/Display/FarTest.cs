using UnityEngine;
using System.Collections;

public class FarTest : MonoBehaviour {
    [Range(0, 4)]
    public byte FarMin = 3;

    public static BarManager bm;
    private LightHead lh;

    // Update is called once per frame
    void Update() {
        if(CameraControl.funcBeingTested != AdvFunction.NONE) return;
        if(bm == null) bm = FindObjectOfType<BarManager>();
        if(lh == null) lh = GetComponent<LightHead>();
        bool far = bm.BarSize >= FarMin && (!lh.isSmall && bm.td != TDOption.LG_SEVEN);
        lh.loc = far ? Location.FAR_REAR : Location.REAR;
        if(far && lh.lhd.funcs.Contains(BasicFunction.TRAFFIC)) {
            lh.lhd.funcs.Remove(BasicFunction.TRAFFIC);
            switch(lh.lhd.funcs.Count) {
                case 0:
                    lh.SetOptic("");
                    break;
                case 1:
                    switch(lh.lhd.funcs[0]) {
                        case BasicFunction.TAKEDOWN:
                        case BasicFunction.ALLEY:
                        case BasicFunction.STT:
                            if(lh.isSmall) lh.SetOptic("Starburst");
                            else lh.SetOptic("");
                            break;
                        case BasicFunction.FLASHING:
                        case BasicFunction.CAL_STEADY:
                            if(lh.isSmall) lh.SetOptic("Small Lineum");
                            else lh.SetOptic("Lineum");
                            break;
                    }
                    break;
                case 2:
                    if(!lh.isSmall) lh.SetOptic("Dual Small Lineum");
                    else lh.SetOptic("Dual Lineum");
                    break;
            }
        } else if(!far && lh.lhd.funcs.Contains(BasicFunction.STT)) {
            lh.lhd.funcs.Remove(BasicFunction.STT);

        }
    }
}
