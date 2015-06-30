using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BOMCables : MonoBehaviour {
    public GameObject singleGO, dualGO;
    public Text circuit, single, dual;
    public byte flags, singleCount, dualCount;

    public void Refresh() {
        flags = 0; // Bit Field:  trDual, trSingle, brDual, brSingle, tlDual, tlSingle, blDual, blSingle

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy) continue;
            if(alpha.Bit == 255 || !alpha.hasRealHead) continue;

            switch(alpha.loc) {
                case Location.FRONT_CORNER:
                    if(alpha.Bit == 0) {
                        flags |= (byte)(alpha.lhd.style.isDualColor ? 0xC : 0x4); // tlSingle always, + tlDual when dual color
                    } else {
                        flags |= (byte)(alpha.lhd.style.isDualColor ? 0xC0 : 0x40); // trSingle always, + trDual when dual color
                    }
                    break;
                case Location.REAR_CORNER:
                    if(alpha.Bit == 0) {
                        flags |= (byte)(alpha.lhd.style.isDualColor ? 0x3 : 0x1); // blSingle always, + blDual when dual color
                    } else {
                        flags |= (byte)(alpha.lhd.style.isDualColor ? 0x30 : 0x10); // brSingle always, + brDual when dual color
                    }
                    break;
                case Location.ALLEY:
                    if(alpha.Bit == 12) {
                        flags |= (byte)(alpha.lhd.style.isDualColor ? 0xC : 0x4); // tlSingle always, + tlDual when dual color
                    } else {
                        flags |= (byte)(alpha.lhd.style.isDualColor ? 0xC0 : 0x40); // trSingle always, + trDual when dual color
                    }
                    break;
                default:
                    if(alpha.transform.position.y > 0) {
                        if(alpha.Bit < 6) {
                            flags |= (byte)(alpha.lhd.style.isDualColor ? 0xC : 0x4); // tlSingle always, + tlDual when dual color
                        } else {
                            flags |= (byte)(alpha.lhd.style.isDualColor ? 0xC0 : 0x40); // trSingle always, + trDual when dual color
                        }
                    } else {
                        if(alpha.Bit < 6) {
                            flags |= (byte)(alpha.lhd.style.isDualColor ? 0x3 : 0x1); // blSingle always, + blDual when dual color
                        } else {
                            flags |= (byte)(alpha.lhd.style.isDualColor ? 0x30 : 0x10); // brSingle always, + brDual when dual color
                        }
                    }
                    break;
            }
        }

        dualCount = singleCount = 0;

        if((flags & 0x80) > 0) {  // Top Right
            dualCount++;
        } else if((flags & 0x40) > 0) {
            singleCount++;
        }
        if((flags & 0x20) > 0) {  // Bottom Right
            dualCount++;
        } else if((flags & 0x10) > 0) {
            singleCount++;
        }
        if((flags & 0x8) > 0) {   // Top Left
            dualCount++;
        } else if((flags & 0x4) > 0) {
            singleCount++;
        }
        if((flags & 0x2) > 0) {   // Bottom Left
            dualCount++;
        } else if((flags & 0x1) > 0) {
            singleCount++;
        }

        singleGO.SetActive(singleCount > 0);
        dualGO.SetActive(dualCount > 0);

        single.text = singleCount + " Control Cable" + (singleCount == 1 ? "" : "s") + " - Single Color";
        dual.text = dualCount + " Control Cable" + (dualCount == 1 ? "" : "s") + " - Dual Color";

        circuit.text = "Control Circuit - " + (dualCount > 0 ? "Dual-Color Capable" : "Single-Color Only");
    }
}
