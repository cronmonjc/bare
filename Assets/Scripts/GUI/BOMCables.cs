using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BOMCables : MonoBehaviour {
    public GameObject singleLGO, singleRGO, dualLGO, dualRGO, powerGO;
    public Text circuit, singleL, singleR, dualL, dualR, barCable, powerCable;
    public byte flags, singleLCount, singleRCount, dualLCount, dualRCount;

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

        dualRCount = singleRCount = dualLCount = singleLCount = 0;

        if((flags & 0x80) > 0) {  // Top Right
            dualRCount++;
        } else if((flags & 0x40) > 0) {
            singleRCount++;
        }
        if((flags & 0x20) > 0) {  // Bottom Right
            dualRCount++;
        } else if((flags & 0x10) > 0) {
            singleRCount++;
        }
        if((flags & 0x8) > 0) {   // Top Left
            dualLCount++;
        } else if((flags & 0x4) > 0) {
            singleLCount++;
        }
        if((flags & 0x2) > 0) {   // Bottom Left
            dualLCount++;
        } else if((flags & 0x1) > 0) {
            singleLCount++;
        }

        singleLGO.SetActive(singleLCount > 0);
        singleRGO.SetActive(singleRCount > 0);
        dualLGO.SetActive(dualLCount > 0);
        dualRGO.SetActive(dualRCount > 0);

        singleL.text = singleLCount + "x SWH-1000-51L -- Internal Control Cable - Single Color, Left";
        singleR.text = singleRCount + "x SWH-1000-51R -- Internal Control Cable - Single Color, Right";
        dualL.text = dualLCount + "x SWH-1000-51DL -- Internal Control Cable - Dual Color, Left";
        dualR.text = dualRCount + "x SWH-1000-51DR -- Internal Control Cable - Dual Color, Right";

        circuit.text = "S8070-454-" + ((dualLCount + dualRCount) > 0 ? "2" : "1") + " -- Control Circuit - " + ((dualLCount + dualRCount) > 0 ? "Dual-Color Capable" : "Single-Color Only");

        barCable.text = "1x SWH-" + (BarManager.useCAN ? "CAN" : "1000BAR") + (BarManager.cableLength == 1 ? "25" : "17") + " -- External Control Cable - " + (BarManager.cableLength == 1 ? "25" : "17") + "'";
        
        if(BarManager.useCAN || BarManager.cableType == 1) {
            powerGO.SetActive(true);
            powerCable.text = "1x S271-POWER10-" + (BarManager.cableLength == 1 ? "25" : "17") + " -- 10 Gauge Power Cable - " + (BarManager.cableLength == 1 ? "25" : "17") + "'";
        } else {
            powerGO.SetActive(false);
        }
    }
}
