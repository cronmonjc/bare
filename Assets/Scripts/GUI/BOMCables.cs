using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

public class BOMCables : MonoBehaviour {
    [System.Serializable]
    public struct CableObject {
        public GameObject Gameobject;
        public Text descrip, price;

        public void SetActive(bool to) {
            Gameobject.SetActive(to);
        }

        public string text {
            get { return descrip.text; }
            set { descrip.text = value; }
        }

        public uint cost {
            set {
                price.gameObject.SetActive(CameraControl.ShowPricing);
                price.text = "$" + (value * 0.01f).ToString("2F");
            }
        }
    }

    public CableObject singleL, singleR, dualL, dualR, power, bar;

    public Text circuit, barCable;
    [System.NonSerialized]
    public byte flags, singleLCount, singleRCount, dualLCount, dualRCount;
    [System.NonSerialized]
    public string internLongPrefix, internShortPrefix, circuitPrefix, externPowerPrefix, externCanPrefix, externHardPrefix;
    [System.NonSerialized]
    public uint extCanS, extCanL, extHardS, extHardL, intSingL, intSingS, intDualL, intDualS, crtSing, crtDual, pwrShrt, pwrLong;
    
    public void Initialize(NbtCompound cmpd) {
        NbtCompound internCmpd = cmpd.Get<NbtCompound>("intern"), externCmpd = cmpd.Get<NbtCompound>("extern"), priceCmpd = cmpd.Get<NbtCompound>("prices");

        internLongPrefix = internCmpd["long"].StringValue;
        internShortPrefix = internCmpd["shrt"].StringValue;

        circuitPrefix = externCmpd["circuit"].StringValue;
        externPowerPrefix = externCmpd["power"].StringValue;
        externCanPrefix = externCmpd["CanPre"].StringValue;
        externHardPrefix = externCmpd["HardPre"].StringValue;


        NbtCompound priceSubCmpd = priceCmpd.Get<NbtCompound>("barCable");
        extHardS = (uint)priceSubCmpd["hardS"].IntValue;
        extHardL = (uint)priceSubCmpd["hardL"].IntValue;
        extCanS = (uint)priceSubCmpd["canS"].IntValue;
        extCanL = (uint)priceSubCmpd["canL"].IntValue;

        priceSubCmpd = priceCmpd.Get<NbtCompound>("intern");
        intSingS = (uint)priceSubCmpd["canS"].IntValue;
        intSingL = (uint)priceSubCmpd["canL"].IntValue;
        intDualS = (uint)priceSubCmpd["hardS"].IntValue;
        intDualL = (uint)priceSubCmpd["hardL"].IntValue;

        priceSubCmpd = priceCmpd.Get<NbtCompound>("circuit");
        crtSing = (uint)priceSubCmpd["sing"].IntValue;
        crtDual = (uint)priceSubCmpd["dual"].IntValue;

        priceSubCmpd = priceCmpd.Get<NbtCompound>("power");
        pwrShrt = (uint)priceSubCmpd["shrt"].IntValue;
        pwrLong = (uint)priceSubCmpd["long"].IntValue;
    }

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

        singleL.SetActive(singleLCount > 0);
        singleR.SetActive(singleRCount > 0);
        dualL.SetActive(dualLCount > 0);
        dualR.SetActive(dualRCount > 0);

        bool useLong = BarManager.inst.BarSize > 2;

        singleL.text = singleLCount + "x " + (useLong ? internLongPrefix : internShortPrefix) + "SL -- Internal Control Cable - Single Color, Left";
        singleR.text = singleRCount + "x " + internShortPrefix + "SR -- Internal Control Cable - Single Color, Right";
        dualL.text = dualLCount + "x " + (useLong ? internLongPrefix : internShortPrefix) + "DL -- Internal Control Cable - Dual Color, Left";
        dualR.text = dualRCount + "x " + internShortPrefix + "DR -- Internal Control Cable - Dual Color, Right";

        circuit.text = circuitPrefix + ((dualLCount + dualRCount) > 0 ? "2" : "1") + " -- Control Circuit - " + ((dualLCount + dualRCount) > 0 ? "Dual-Color Capable" : "Single-Color Only");

        bar.text = "1x " + (BarManager.useCAN ? externCanPrefix : externHardPrefix) + (BarManager.cableLength == 1 ? "25" : "17") + " -- External Control Cable - " + (BarManager.cableLength == 1 ? "25" : "17") + "'";
        
        if(BarManager.useCAN || BarManager.cableType == 1) {
            power.SetActive(true);
            power.text = "1x " + externPowerPrefix + (BarManager.cableLength == 1 ? "25" : "17") + " -- 10 Gauge Power Cable - " + (BarManager.cableLength == 1 ? "25" : "17") + "'";
        } else {
            power.SetActive(false);
        }
    }
}
