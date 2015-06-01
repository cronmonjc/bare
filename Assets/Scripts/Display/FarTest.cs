using UnityEngine;
using System.Collections;

public class FarTest : MonoBehaviour {

    public static BarManager bm;
    private LightHead lh;

    // Update is called once per frame
    void Update() {
        if(CameraControl.funcBeingTested != AdvFunction.NONE) return;
        if(bm == null) bm = FindObjectOfType<BarManager>();
        if(lh == null) lh = GetComponent<LightHead>();
        bool far = (lh.Bit == 1 || lh.Bit == 10);
        lh.loc = far ? Location.FAR_REAR : Location.REAR;
        if(far && lh.lhd.funcs.Contains(BasicFunction.TRAFFIC)) {
            lh.RemoveBasicFunction(BasicFunction.TRAFFIC);
        } else if(!far && lh.lhd.funcs.Contains(BasicFunction.STT)) {
            lh.RemoveBasicFunction(BasicFunction.STT);
        }
    }
}
