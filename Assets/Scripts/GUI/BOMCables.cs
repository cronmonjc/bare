using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing.Layout;

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

    public CableObject singleL, singleR, dualL, dualR, yCable, power, bar, circuit, canMod;

    [System.NonSerialized]
    public byte flags, singleLCount, singleRCount, dualLCount, dualRCount, yCount;
    [System.NonSerialized]
    public uint consumed;
    [System.NonSerialized]
    public bool second5, second6;
    [System.NonSerialized]
    public string internLongPrefix, internShortPrefix, internSplitPart, circuitPrefix, externPowerPrefix, externCanPrefix, externHardPrefix, canPart;
    [System.NonSerialized]
    public uint intSingL, intSingS, intDualL, intDualS, intSplit, crtSing, crtDual, crtCan;
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

        canPart = cmpd["canModPart"].StringValue;


        NbtCompound priceSubCmpd = priceCmpd.Get<NbtCompound>("intern");
        intSingS = (uint)priceSubCmpd["singS"].IntValue;
        intSingL = (uint)priceSubCmpd["singL"].IntValue;
        intDualS = (uint)priceSubCmpd["dualS"].IntValue;
        intDualL = (uint)priceSubCmpd["dualL"].IntValue;
        intSplit = (uint)priceSubCmpd["split"].IntValue;

        priceSubCmpd = priceCmpd.Get<NbtCompound>("circuit");
        crtSing = (uint)priceSubCmpd["sing"].IntValue;
        crtDual = (uint)priceSubCmpd["dual"].IntValue;
        crtCan = (uint)priceSubCmpd["canMod"].IntValue;
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

        if(BarManager.useCAN) {
            canMod.SetActive(true);
            canMod.text = canPart + " -- CAN Breakout Box";
            totalCost += canMod.cost = crtCan;
        } else {
            canMod.SetActive(false);
        }

        if(BarManager.useCAN || BarManager.cableType == 1) {
            power.SetActive(true);
            power.quantity = 1;
            power.text = externPowerPrefix + (opt.length) + " -- 10 Gauge Power Cable - " + (opt.length) + "'";
            totalCost += power.cost = opt.pwrPrice;
        } else {
            power.SetActive(false);
        }
    }

    public void PDFExportSummary(ref double top, XTextFormatter tf, XFont courierSm, XFont caliSm, XFont caliSmBold) {
        CableLengthOption opt = LightDict.inst.cableLengths[BarManager.cableLength];
        bool useLong = BarManager.inst.BarSize > 2;

        tf.DrawString("Control Circuit - " + ((dualLCount + dualRCount) > 0 ? "Dual-Color Capable" : "Single-Color Only"), caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
        if(CameraControl.ShowPricing)
            tf.DrawString("$" + (((dualLCount + dualRCount) > 0 ? crtDual : crtSing) * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        top += 0.1;
        if(BarManager.useCAN) {
            tf.DrawString("CAN Breakout Box", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + ((crtCan) * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        }

        top += 0.15;

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Quantity", caliSmBold, XBrushes.Black, new XRect(0.5, top - 0.01, 0.9, 0.1));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Cables", caliSmBold, XBrushes.Black, new XRect(1.4, top - 0.01, 2.0, 0.1));
        if(CameraControl.ShowPricing)
            tf.DrawString("List Price", caliSmBold, XBrushes.Black, new XRect(3.625, top - 0.01, 0.5, 0.1));

        top += 0.1;
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("1", courierSm, XBrushes.Black, new XRect(0.5, top, 0.9, 0.10));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("External Control Cable - " + (opt.length) + "'", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
        if(CameraControl.ShowPricing)
            tf.DrawString("$" + ((BarManager.useCAN ? opt.canPrice : opt.hardPrice) * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));

        if(BarManager.useCAN || BarManager.cableType == 1) {
            top += 0.1;
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("1", courierSm, XBrushes.Black, new XRect(0.5, top, 0.9, 0.10));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("10 Gauge Power Cable - " + (opt.length) + "'", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (opt.pwrPrice * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        }

        if(singleLCount > 0) {
            top += 0.1;
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(singleLCount + "", courierSm, XBrushes.Black, new XRect(0.5, top, 0.9, 0.10));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("Internal Control Cable - Single Color, Left", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (singleLCount * (useLong ? intSingL : intSingS) * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        }

        if(dualLCount > 0) {
            top += 0.1;
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(dualLCount + "", courierSm, XBrushes.Black, new XRect(0.5, top, 0.9, 0.10));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("Internal Control Cable - Dual Color, Left", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (dualLCount * (useLong ? intDualL : intDualS) * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        }

        if(singleRCount > 0) {
            top += 0.1;
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(singleRCount + "", courierSm, XBrushes.Black, new XRect(0.5, top, 0.9, 0.10));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("Internal Control Cable - Single Color, Right", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (singleRCount * intSingS * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        }

        if(dualRCount > 0) {
            top += 0.1;
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(dualRCount + "", courierSm, XBrushes.Black, new XRect(0.5, top, 0.9, 0.10));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("Internal Control Cable - Dual Color, Right", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (dualRCount * intDualS * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        }

        if(yCount > 0) {
            top += 0.1;
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(yCount + "", courierSm, XBrushes.Black, new XRect(0.5, top, 0.9, 0.10));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("Internal Output Splitter", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (yCount * intSplit * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        }
    }

    public void PDFExportParts(ref double top, XTextFormatter tf, XFont courier, XFont caliSm) {
        bool useLong = BarManager.inst.BarSize > 2;
        CableLengthOption opt = LightDict.inst.cableLengths[BarManager.cableLength];

        top += 0.2;
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString((BarManager.useCAN ? externCanPrefix : externHardPrefix) + (opt.length), courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
        tf.DrawString("External Control Cable - " + (opt.length) + "'", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
        top += 0.15;
        if(BarManager.cableType == 1 || BarManager.useCAN) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(externPowerPrefix + (opt.length), courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("10 Gauge Power Cable - " + (opt.length) + "'", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }
        if(singleLCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + singleLCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString((useLong ? internLongPrefix : internShortPrefix) + "SL", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Control Cable - Single Color, Left", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }
        if(singleRCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + singleRCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(internShortPrefix + "SR", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Control Cable - Single Color, Right", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }
        if(dualLCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + dualLCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString((useLong ? internLongPrefix : internShortPrefix) + "DL", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Control Cable - Dual Color, Left", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }
        if(dualRCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + dualRCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(internShortPrefix + "DR", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Control Cable - Dual Color, Right", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }

        if(yCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + yCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(internSplitPart, courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Output Splitter", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }

    }

    void Update() {
        if(showingPricing ^ CameraControl.ShowPricing) {
            showingPricing = CameraControl.ShowPricing;
            Refresh();
        }
    }
}
