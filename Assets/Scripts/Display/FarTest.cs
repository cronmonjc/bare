using UnityEngine;
using System.Collections;

public class FarTest : MonoBehaviour {
    [Range(0, 3)]
    public byte FarMin = 3;

    public static BarManager bm;
    private LightHead lh;

    // Update is called once per frame
    void Update() {
        if(bm == null) bm = FindObjectOfType<BarManager>();
        if(lh == null) lh = GetComponent<LightHead>();
        lh.loc = bm.BarSize >= FarMin ? Location.FAR_REAR : Location.REAR;
    }
}
