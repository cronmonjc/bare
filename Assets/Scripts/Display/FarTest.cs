using UnityEngine;
using System.Collections;

public class FarTest : MonoBehaviour {

    public bool canFarOnPos = false;
    [Range(0, 4)]
    public byte sizeForFar = 3;

    public static BarManager bm;
    private LightHead lh;

    // Update is called once per frame
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing
        if(bm == null) bm = FindObjectOfType<BarManager>();
        if(lh == null) lh = GetComponent<LightHead>();
        bool far = (lh.Bit == 1 || lh.Bit == 10);
        if(canFarOnPos) 
            far |= bm.BarSize >= sizeForFar; 
        if(lh.shouldBeTD) far = false;
        lh.loc = far ? Location.FAR_REAR : Location.REAR;
        if(far && lh.lhd.funcs.Contains(BasicFunction.TRAFFIC)) {
            lh.RemoveBasicFunction(BasicFunction.TRAFFIC);
        } else if(!far && lh.lhd.funcs.Contains(BasicFunction.STT)) {
            lh.RemoveBasicFunction(BasicFunction.STT);
        }
    }
}
