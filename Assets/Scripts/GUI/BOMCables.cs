using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

public class BOMCables : MonoBehaviour {
    [System.Serializable]
    public struct CableObject {
        public GameObject Gameobject;
        public Text qty, descrip, price;

        public void SetActive(bool to) {
            Gameobject.SetActive(to);
        }

        public byte quantity {
            set {
                qty.text = value + "x";
            }
        }

        public string text {
            get { return descrip.text; }
            set { descrip.text = value; }
        }

        public uint cost {
            set {
                price.gameObject.SetActive(CameraControl.ShowPricing);
                price.text = "$" + (value * 0.01f).ToString("F2");
            }
        }
    }

    public CableObject singleL, singleR, dualL, dualR, yCable, power, bar, circuit;

    [System.NonSerialized]
    public byte flags, singleLCount, singleRCount, dualLCount, dualRCount, yCount;
    [System.NonSerialized]
    public uint consumed;
    [System.NonSerialized]
    public bool second5, second6;
    [System.NonSerialized]
    public string internLongPrefix, internShortPrefix, internSplitPart, circuitPrefix, externPowerPrefix, externCanPrefix, externHardPrefix;
    [System.NonSerialized]
    public uint extCanS, extCanL, extHardS, extHardL, intSingL, intSingS, intDualL, intDualS, intSplit, crtSing, crtDual, pwrShrt, pwrLong;
    [System.NonSerialized]
    public uint totalCost;

    private bool showingPricing = false;
    
    public void Initialize(NbtCompound cmpd) {
        NbtCompound internCmpd = cmpd.Get<NbtCompound>("intern"), externCmpd = cmpd.Get<NbtCompound>("extern"), priceCmpd = cmpd.Get<NbtCompound>("prices");

        internLongPrefix = internCmpd["long"].StringValue;
        internShortPrefix = internCmpd["shrt"].StringValue;
        internSplitPart = internCmpd["split"].StringValue;

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
        intSingS = (uint)priceSubCmpd["singS"].IntValue;
        intSingL = (uint)priceSubCmpd["singL"].IntValue;
        intDualS = (uint)priceSubCmpd["dualS"].IntValue;
        intDualL = (uint)priceSubCmpd["dualL"].IntValue;
        intSplit = (uint)priceSubCmpd["split"].IntValue;

        priceSubCmpd = priceCmpd.Get<NbtCompound>("circuit");
        crtSing = (uint)priceSubCmpd["sing"].IntValue;
        crtDual = (uint)priceSubCmpd["dual"].IntValue;

        priceSubCmpd = priceCmpd.Get<NbtCompound>("power");
        pwrShrt = (uint)priceSubCmpd["shrt"].IntValue;
        pwrLong = (uint)priceSubCmpd["long"].IntValue;
    }

    public void Refresh() {
        flags = 0; // Bit Field:  trDual, trSingle, brDual, brSingle, tlDual, tlSingle, blDual, blSingle
        yCount = 0;
        consumed = 0;
        second5 = second6 = false;

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy) continue;
            if(alpha.Bit == 255 || !alpha.hasRealHead) continue;

            uint bit = (uint)(0x1 << (alpha.Bit + (alpha.isRear ? 16 : 0)));
            if((consumed & bit) > 0) {
                if(alpha.Bit == 5) {
                    if(second5) {
                        yCount++;
                    } else {
                        second5 = true;
                    }
                } else if(alpha.Bit == 6) {
                    if(second6) {
                        yCount++;
                    } else {
                        second6 = true;
                    }
                } else {
                    yCount++;
                }
            } else {
                consumed |= bit;
            }

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
        yCable.SetActive(yCount > 0);

        bool useLong = BarManager.inst.BarSize > 2;

        totalCost = 0;

        singleL.quantity = singleLCount;
        singleL.text = (useLong ? internLongPrefix : internShortPrefix) + "SL -- Internal Control Cable - Single Color, Left";
        totalCost += singleL.cost = singleLCount * (useLong ? intSingL : intSingS);
        singleR.quantity = singleRCount;
        singleR.text = internShortPrefix + "SR -- Internal Control Cable - Single Color, Right";
        totalCost += singleR.cost = singleRCount * intSingS;
        dualL.quantity = dualLCount;
        dualL.text = (useLong ? internLongPrefix : internShortPrefix) + "DL -- Internal Control Cable - Dual Color, Left";
        totalCost += dualL.cost = dualLCount * (useLong ? intDualL : intDualS);
        dualR.quantity = dualRCount;
        dualR.text = internShortPrefix + "DR -- Internal Control Cable - Dual Color, Right";
        totalCost += dualR.cost = dualRCount * intDualS;
        yCable.quantity = yCount;
        yCable.text = internSplitPart + " -- Internal Output Splitter";
        totalCost += yCable.cost = yCount * intSplit;

        circuit.text = circuitPrefix + ((dualLCount + dualRCount) > 0 ? "2" : "1") + " -- Control Circuit - " + ((dualLCount + dualRCount) > 0 ? "Dual-Color Capable" : "Single-Color Only");
        totalCost += circuit.cost = ((dualLCount + dualRCount) > 0 ? crtDual : crtSing);

        CableLengthOption opt = LightDict.inst.cableLengths[BarManager.cableLength];
        bar.quantity = 1;
        bar.text = (BarManager.useCAN ? externCanPrefix : externHardPrefix) + (opt.length) + " -- External Control Cable - " + (opt.length) + "'";
        totalCost += bar.cost = (BarManager.useCAN ? opt.canPrice : opt.hardPrice);
        
        if(BarManager.useCAN || BarManager.cableType == 1) {
            power.SetActive(true);
            power.quantity = 1;
            power.text = externPowerPrefix + (opt.length) + " -- 10 Gauge Power Cable - " + (opt.length) + "'";
            totalCost += power.cost = opt.pwrPrice;
        } else {
            power.SetActive(false);
        }
    }

    void Update() {
        if(showingPricing ^ CameraControl.ShowPricing) {
            showingPricing = CameraControl.ShowPricing;
            Refresh();
        }
    }
}
