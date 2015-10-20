using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing.Layout;

/// <summary>
/// UI Component.  Manages the bill of materials for all of the cables the bar will need.
/// </summary>
public class BOMCables : MonoBehaviour {
    /// <summary>
    /// A struct containing information on the display of a certain cable type.  Set via Unity Inspector.
    /// </summary>
    [System.Serializable]
    public struct CableObject {
        /// <summary>
        /// The reference to the GameObject that contains the entirety of this CableObject.  Set via Unity Inspector.
        /// </summary>
        public GameObject Gameobject;
        /// <summary>
        /// The reference to the Text Component showing the quantity.  Set via Unity Inspector.
        /// </summary>
        public Text qty;
        /// <summary>
        /// The reference to the Text Component showing the description.  Set via Unity Inspector.
        /// </summary>
        public Text descrip;
        /// <summary>
        /// The reference to the Text Component showing the sale price.  Set via Unity Inspector.
        /// </summary>
        public Text price;

        /// <summary>
        /// Sets whether the CableObject is active or not.
        /// </summary>
        public void SetActive(bool to) {
            Gameobject.SetActive(to);
        }

        /// <summary>
        /// Sets the quantity text.
        /// </summary>
        public byte quantity {
            set {
                qty.text = value + "x";
            }
        }

        /// <summary>
        /// Gets or sets the description text.
        /// </summary>
        public string text {
            get { return descrip.text; }
            set { descrip.text = value; }
        }

        /// <summary>
        /// Sets the displayed sale price.
        /// </summary>
        public uint cost {
            set {
                price.gameObject.SetActive(CameraControl.ShowPricing);
                price.text = "$" + (value * 0.01f).ToString("F2");
            }
        }
    }

    #region Lots of variables
    #region CableObjects
    /// <summary>
    /// The single left CableObject
    /// </summary>
    public CableObject singleL;
    /// <summary>
    /// The single right CableObject
    /// </summary>
    public CableObject singleR;
    /// <summary>
    /// The dual left CableObject
    /// </summary>
    public CableObject dualL;
    /// <summary>
    /// The dual right CableObject
    /// </summary>
    public CableObject dualR;
    /// <summary>
    /// The Y-Splitter CableObject
    /// </summary>
    public CableObject yCable;
    /// <summary>
    /// The power CableObject
    /// </summary>
    public CableObject power;
    /// <summary>
    /// The bar communication CableObject
    /// </summary>
    public CableObject bar;
    /// <summary>
    /// The circuit "Cable"Object
    /// </summary>
    public CableObject circuit;
    /// <summary>
    /// The CAN Module "Cable"Object
    /// </summary>
    public CableObject canMod; 
    #endregion

    #region Cable Counts
    /// <summary>
    /// A bit field used to keep track of which quadrant uses which kind of cable.
    /// </summary>
    [System.NonSerialized]
    public byte flags;
    /// <summary>
    /// The single left cable count
    /// </summary>
    [System.NonSerialized]
    public byte singleLCount;
    /// <summary>
    /// The single right cable count
    /// </summary>
    [System.NonSerialized]
    public byte singleRCount;
    /// <summary>
    /// The dual left cable count
    /// </summary>
    [System.NonSerialized]
    public byte dualLCount;
    /// <summary>
    /// The dual right cable count
    /// </summary>
    [System.NonSerialized]
    public byte dualRCount;
    /// <summary>
    /// The Y-Splitter count
    /// </summary>
    [System.NonSerialized]
    public byte yCount; 
    #endregion

    /// <summary>
    /// A bit field indicating the bits that have already had their original wires consumed.  If a bit is already high, a Y-Splitter will be needed.
    /// </summary>
    [System.NonSerialized]
    public uint consumed;

    #region Second wire outputs
    /// <summary>
    /// Whether the second bit-5 output has already been used
    /// </summary>
    [System.NonSerialized]
    public bool second5;
    /// <summary>
    /// Whether the second bit-6 output has already been used
    /// </summary>
    [System.NonSerialized]
    public bool second6; 
    #endregion

    #region Part Numbers
    /// <summary>
    /// The internal longer cable harness part number prefix
    /// </summary>
    [System.NonSerialized]
    public string internLongPrefix;
    /// <summary>
    /// The internal shorter cable harness part number prefix
    /// </summary>
    [System.NonSerialized]
    public string internShortPrefix;
    /// <summary>
    /// The internal splitter part number
    /// </summary>
    [System.NonSerialized]
    public string internSplitPart;
    /// <summary>
    /// The central control circuit part number prefix
    /// </summary>
    [System.NonSerialized]
    public string circuitPrefix;
    /// <summary>
    /// The external power cable part number prefix
    /// </summary>
    [System.NonSerialized]
    public string externPowerPrefix;
    /// <summary>
    /// The external CAN communication cable part number prefix
    /// </summary>
    [System.NonSerialized]
    public string externCanPrefix;
    /// <summary>
    /// The external Hardwire communication cable part number prefix
    /// </summary>
    [System.NonSerialized]
    public string externHardPrefix;
    /// <summary>
    /// The CAN module circuit part number
    /// </summary>
    [System.NonSerialized]
    public string canPart; 
    #endregion

    #region Prices for cables
    /// <summary>
    /// The sale price of an internal single long cable harness
    /// </summary>
    [System.NonSerialized]
    public uint intSingL;
    /// <summary>
    /// The sale price of an internal single short cable harness
    /// </summary>
    [System.NonSerialized]
    public uint intSingS;
    /// <summary>
    /// The sale price of an internal dual long cable harness
    /// </summary>
    [System.NonSerialized]
    public uint intDualL;
    /// <summary>
    /// The sale price of an internal dual short cable harness
    /// </summary>
    [System.NonSerialized]
    public uint intDualS;
    /// <summary>
    /// The sale price of a Y-Splitter
    /// </summary>
    [System.NonSerialized]
    public uint intSplit; 
    #endregion

    #region Prices for circuits
    /// <summary>
    /// The sale price of a single-color-capable central control circuit
    /// </summary>
    [System.NonSerialized]
    public uint crtSing;
    /// <summary>
    /// The sale price of a dual-color-capable central control circuit
    /// </summary>
    [System.NonSerialized]
    public uint crtDual;
    /// <summary>
    /// The sale price of a CAN Module
    /// </summary>
    [System.NonSerialized]
    public uint crtCan; 
    #endregion

    /// <summary>
    /// The total sale price of all of the cables
    /// </summary>
    [System.NonSerialized]
    public uint totalCost;

    /// <summary>
    /// The showing pricing
    /// </summary>
    private bool showingPricing = false; 
    #endregion

    /// <summary>
    /// Initializes this Component with the specified cable data from the library.
    /// </summary>
    public void Initialize(NbtCompound cmpd) {
        NbtCompound internCmpd = cmpd.Get<NbtCompound>("intern"), externCmpd = cmpd.Get<NbtCompound>("extern"), priceCmpd = cmpd.Get<NbtCompound>("prices");

        internLongPrefix = internCmpd["long"].StringValue;
        internShortPrefix = internCmpd["shrt"].StringValue;
        internSplitPart = internCmpd["split"].StringValue;

        externPowerPrefix = externCmpd["power"].StringValue;
        externCanPrefix = externCmpd["CanPre"].StringValue;
        externHardPrefix = externCmpd["HardPre"].StringValue;

        canPart = cmpd["canModPart"].StringValue;
        circuitPrefix = cmpd["circuit"].StringValue;
        

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

    /// <summary>
    /// Refreshes this Component.  Recalculates the required cables and sale prices of such.
    /// </summary>
    public void Refresh() {
        #region Setup
        flags = 0; // Bit Field:  trDual, trSingle, brDual, brSingle, tlDual, tlSingle, blDual, blSingle
        yCount = 0;
        consumed = 0;
        second5 = second6 = false; 
        #endregion

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy) continue;
            if(alpha.Bit == 255 || !alpha.hasRealHead) continue;

            #region Find the output this head consumes
            uint bit = (uint)(0x1 << (alpha.Bit + (alpha.isRear ? 16 : 0)));
            if((consumed & bit) > 0) {
                if(alpha.Bit == 5) {
                    if(second5) {
                        yCount++; // Output and bit 5's splitter already used, get a Y-Splitter in to share it
                    } else {
                        second5 = true; // Output already used, but bit 5 has a built-in splitter already.  Use it.
                    }
                } else if(alpha.Bit == 6) {
                    if(second6) {
                        yCount++; // Output and bit 6's splitter already used, get a Y-Splitter in to share it
                    } else {
                        second6 = true; // Output already used, but bit 6 has a built-in splitter already.  Use it.
                    }
                } else {
                    yCount++; // Output already used, get a Y-Splitter in to share it
                }
            } else {
                consumed |= bit;
            } 
            #endregion

            #region Figure out if this corner needs a dual cable or can get away with a single
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
            #endregion
        }

        dualRCount = singleRCount = dualLCount = singleLCount = 0; // Init counts

        #region Count off needed cables
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
        #endregion

        #region Show/Hide CableObjects if necessary
        singleL.SetActive(singleLCount > 0);
        singleR.SetActive(singleRCount > 0);
        dualL.SetActive(dualLCount > 0);
        dualR.SetActive(dualRCount > 0);
        yCable.SetActive(yCount > 0); 
        #endregion

        bool useLong = BarManager.inst.BarSize > 2; // Do we need to use the longer cables?

        totalCost = 0; // Init pricing

        #region Set information on each of the CableObjects
        // Single Left
        singleL.quantity = singleLCount;
        singleL.text = (useLong ? internLongPrefix : internShortPrefix) + "L -- Internal Control Cable - Single Color, Left";
        totalCost += singleL.cost = singleLCount * (useLong ? intSingL : intSingS);

        // Single Right
        singleR.quantity = singleRCount;
        singleR.text = internShortPrefix + "R -- Internal Control Cable - Single Color, Right";
        totalCost += singleR.cost = singleRCount * intSingS;

        // Dual Left
        dualL.quantity = dualLCount;
        dualL.text = (useLong ? internLongPrefix : internShortPrefix) + "DL -- Internal Control Cable - Dual Color, Left";
        totalCost += dualL.cost = dualLCount * (useLong ? intDualL : intDualS);

        // Dual Right
        dualR.quantity = dualRCount;
        dualR.text = internShortPrefix + "DR -- Internal Control Cable - Dual Color, Right";
        totalCost += dualR.cost = dualRCount * intDualS;

        // Y-Splitter
        yCable.quantity = yCount;
        yCable.text = internSplitPart + " -- Internal Output Splitter";
        totalCost += yCable.cost = yCount * intSplit; 

        // Central Control Circuit - always shown, all bars need one
        circuit.text = circuitPrefix + ((dualLCount + dualRCount) > 0 ? "2" : "1") + " -- Control Circuit - " + ((dualLCount + dualRCount) > 0 ? "Dual-Color Capable" : "Single-Color Only");
        totalCost += circuit.cost = ((dualLCount + dualRCount) > 0 ? crtDual : crtSing);

        // External comm cable
        CableLengthOption opt = LightDict.inst.cableLengths[BarManager.cableLength];
        bar.quantity = 1;
        bar.text = (BarManager.useCAN ? externCanPrefix : externHardPrefix) + (opt.length) + " -- External Control Cable - " + (opt.length) + "'";
        totalCost += bar.cost = (BarManager.useCAN ? opt.canPrice : opt.hardPrice);

        // Show CAN Module if needed
        if(BarManager.useCAN) {
            canMod.SetActive(true);
            canMod.text = canPart + " -- CAN Breakout Box";
            totalCost += canMod.cost = crtCan;
        } else {
            canMod.SetActive(false);
        }

        // Show Power Cable if needed
        if(BarManager.useCAN || BarManager.cableType == 1) {
            power.SetActive(true);
            power.quantity = 1;
            power.text = externPowerPrefix + (opt.length) + " -- 10 Gauge Power Cable - " + (opt.length) + "'";
            totalCost += power.cost = opt.pwrPrice;
        } else {
            power.SetActive(false);
        }
        #endregion
    }

    /// <summary>
    /// Writes the cable summary onto the PDF.
    /// </summary>
    /// <param name="top">The top reference.</param>
    /// <param name="tf">The Text Formatter reference.</param>
    /// <param name="courierSm">The courier new small font reference.</param>
    /// <param name="caliSm">The calibri small font reference.</param>
    /// <param name="caliSmBold">The calibri small bold font reference.</param>
    public void PDFExportSummary(ref double top, XTextFormatter tf, XFont courierSm, XFont caliSm, XFont caliSmBold) {
        #region Fetch a few pieces of info
        CableLengthOption opt = LightDict.inst.cableLengths[BarManager.cableLength];
        bool useLong = BarManager.inst.BarSize > 2; 
        #endregion

        #region Write out needed circuitry
        tf.DrawString("Control Circuit - " + ((dualLCount + dualRCount) > 0 ? "Dual-Color Capable" : "Single-Color Only"), caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
        if(CameraControl.ShowPricing)
            tf.DrawString("$" + (((dualLCount + dualRCount) > 0 ? crtDual : crtSing) * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        top += 0.1;
        if(BarManager.useCAN) {
            tf.DrawString("CAN Breakout Box", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + ((crtCan) * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        } 
        #endregion

        top += 0.15;

        #region Write out the header
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Quantity", caliSmBold, XBrushes.Black, new XRect(0.5, top - 0.01, 0.9, 0.1));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Cables", caliSmBold, XBrushes.Black, new XRect(1.4, top - 0.01, 2.0, 0.1));
        if(CameraControl.ShowPricing)
            tf.DrawString("List Price", caliSmBold, XBrushes.Black, new XRect(3.625, top - 0.01, 0.5, 0.1)); 
        #endregion

        top += 0.1;
        #region Write out the external comm cable
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("1", courierSm, XBrushes.Black, new XRect(0.5, top, 0.9, 0.10));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("External Control Cable - " + (opt.length) + "'", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
        if(CameraControl.ShowPricing)
            tf.DrawString("$" + ((BarManager.useCAN ? opt.canPrice : opt.hardPrice) * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10)); 
        #endregion

        #region Write out power cable if needed
        if(BarManager.useCAN || BarManager.cableType == 1) {
            top += 0.1;
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("1", courierSm, XBrushes.Black, new XRect(0.5, top, 0.9, 0.10));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString("10 Gauge Power Cable - " + (opt.length) + "'", caliSm, XBrushes.Black, new XRect(1.4, (top - 0.01), 2.5, 0.10));
            if(CameraControl.ShowPricing)
                tf.DrawString("$" + (opt.pwrPrice * 0.01f).ToString("F2"), courierSm, XBrushes.Black, new XRect(3.625, top, 1.0, 0.10));
        } 
        #endregion

        #region Write out any needed internal cables needed
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
        #endregion
    }

    /// <summary>
    /// Writes the cable part numbers onto the PDF.
    /// </summary>
    /// <param name="top">The top reference.</param>
    /// <param name="tf">The Text Formatter reference.</param>
    /// <param name="courier">The courier new font reference.</param>
    /// <param name="caliSm">The calibri small font reference.</param>
    public void PDFExportParts(ref double top, XTextFormatter tf, XFont courier, XFont caliSm) {
        #region Fetch a few pieces of info
        CableLengthOption opt = LightDict.inst.cableLengths[BarManager.cableLength];
        bool useLong = BarManager.inst.BarSize > 2; 
        #endregion

        top += 0.2;
        #region Write out the external comm cable
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString((BarManager.useCAN ? externCanPrefix : externHardPrefix) + (opt.length), courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
        tf.DrawString("External Control Cable - " + (opt.length) + "'", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2)); 
        #endregion
        top += 0.15;
        #region Write out power cable if needed
        if(BarManager.cableType == 1 || BarManager.useCAN) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("1", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(externPowerPrefix + (opt.length), courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("10 Gauge Power Cable - " + (opt.length) + "'", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        } 
        #endregion
        #region Write out any needed internal cables needed
        if(singleLCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + singleLCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString((useLong ? internLongPrefix : internShortPrefix) + "L", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString("Internal Control Cable - Single Color, Left", caliSm, XBrushes.Black, new XRect(3.0, top, 4.0, 0.2));
            top += 0.15;
        }
        if(singleRCount > 0) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString("" + singleRCount, courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(internShortPrefix + "R", courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
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
        #endregion

    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(showingPricing ^ CameraControl.ShowPricing) {
            showingPricing = CameraControl.ShowPricing;
            Refresh();
        }
    }
}
