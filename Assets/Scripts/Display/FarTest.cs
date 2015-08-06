using UnityEngine;
using System.Collections;

/// <summary>
/// A small class that tests whether a head is "far rear" -- deprecated because anything that uses it simply examines bits now
/// </summary>
[System.Obsolete("Anything that used to depend on the FarTest Component to set it on \"far head\" now simply examines the bits")]
public class FarTest : MonoBehaviour {

    public bool canFarOnPos = false;
    [Range(0, 4)]
    public byte sizeForFar = 3;

    public static BarManager bm;
    private LightHead lh;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
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
