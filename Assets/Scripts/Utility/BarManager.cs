using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing.Layout;
using System;

public class BarManager : MonoBehaviour {
    private bool savePDF = false;

    [Range(0, 4)]
    public int BarSize = 3;
    public TDOption td;

    public NbtCompound patts;

    public static BarManager inst;
    public List<LightHead> allHeads;
    public static LightHead[] headNumber;

    public LightHead first;

    public InputField custName, orderNum, notes;

    public FileBrowser fb;
    private string barFilePath;

    public string BarModel {
        get {
            switch(BarSize) {
                case 0:
                    return "1300";
                case 1:
                    return "1400";
                case 2:
                    return "1500";
                case 3:
                    return "1550";
                case 4:
                    return "16xx";
                default:
                    return "????";
            }
        }
    }

    void Awake() {
        patts = new NbtCompound("pats");

        foreach(string alpha in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim", "traf" }) {
            patts.Add(new NbtCompound(alpha));
        }

        patts.Get<NbtCompound>("traf").AddRange(new NbtShort[] { new NbtShort("enr1", 0), new NbtShort("enr2", 0) });

        foreach(string alpha in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim" }) {
            patts.Get<NbtCompound>(alpha).AddRange(new NbtShort[] { new NbtShort("enf1", 0), new NbtShort("enf2", 0), new NbtShort("enr1", 0), new NbtShort("enr2", 0) });
        }

        patts.Get<NbtCompound>("dim").Add(new NbtShort("dimp", 0));

        foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw" }) {
            patts.Get<NbtCompound>(alpha).AddRange(new NbtShort[] { new NbtShort("phf1", 0), new NbtShort("phf2", 0), new NbtShort("phr1", 0), new NbtShort("phr2", 0) });
        }

        patts.Get<NbtCompound>("traf").Add(new NbtCompound("patt", new NbtTag[] { new NbtShort("left", 0), new NbtShort("rite", 0), new NbtShort("cntr", 0) }));

        foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl" }) {
            patts.Get<NbtCompound>(alpha).Add(new NbtCompound("pat1", new NbtTag[] { new NbtShort("fcen", 0), new NbtShort("finb", 0), new NbtShort("foub", 0), new NbtShort("ffar", 0), new NbtShort("fcor", 0),
                                                                                     new NbtShort("rcen", 0), new NbtShort("rinb", 0), new NbtShort("roub", 0), new NbtShort("rfar", 0), new NbtShort("rcor", 0) }));
            patts.Get<NbtCompound>(alpha).Add(new NbtCompound("pat2", new NbtTag[] { new NbtShort("fcen", 0), new NbtShort("finb", 0), new NbtShort("foub", 0), new NbtShort("ffar", 0), new NbtShort("fcor", 0),
                                                                                     new NbtShort("rcen", 0), new NbtShort("rinb", 0), new NbtShort("roub", 0), new NbtShort("rfar", 0), new NbtShort("rcor", 0) }));
        }

        allHeads = new List<LightHead>();
        inst = this;
    }

    void Start() {
        allHeads.AddRange(transform.GetComponentsInChildren<LightHead>(true));
    }

    public static string GetFnString(Transform t, AdvFunction f) {
        switch(f) {
            case AdvFunction.ALLEY:
                if(t.position.x < 0) {
                    return "lall";
                } else {
                    return "rall";
                }
            case AdvFunction.CRUISE:
                return "cru";
            case AdvFunction.DIM:
                return "dim";
            case AdvFunction.EMITTER:
                return "emi";
            case AdvFunction.ICL:
                return "icl";
            case AdvFunction.LEVEL1:
                return "l1";
            case AdvFunction.LEVEL2:
                return "l2";
            case AdvFunction.LEVEL3:
                return "l3";
            case AdvFunction.LEVEL4:
                return "l4";
            case AdvFunction.LEVEL5:
                return "l5";
            case AdvFunction.STT_AND_TAIL:
                if(t.position.x < 0) {
                    return "ltai";
                } else {
                    return "rtai";
                }
            case AdvFunction.TAKEDOWN:
                return "td";
            case AdvFunction.TRAFFIC:
                return "traf";
            case AdvFunction.T13:
                return "cal";
            default:
                return null;
        }
    }

    public void SetBarSize(float to) {
        SetBarSize(Mathf.RoundToInt(to));
    }

    public void SetBarSize(int to) {
        if(to < 5 && to > -1) {
            BarSize = to;
            foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) {
                soc.ShowLong = true;
            }
            FindObjectOfType<CameraControl>().OnlyCamSelected.Clear();
            foreach(LightLabel ll in GameObject.Find("BarCanvas/Labels").GetComponentsInChildren<LightLabel>(true)) {
                ll.Refresh();
            }
        }
    }

    public void SetTDOption(int to) {
        td = (TDOption)to;

        StartCoroutine(SetTDOption());
    }

    public IEnumerator SetTDOption() {
        switch(td) {
            case TDOption.NONE:
                foreach(LightHead lh in allHeads) {
                    if(lh.gameObject.activeInHierarchy && lh.lhd.funcs.Contains(BasicFunction.TRAFFIC)) {
                        lh.lhd.funcs.Remove(BasicFunction.TRAFFIC);
                        switch(lh.lhd.funcs.Count) {
                            case 0:
                                lh.SetOptic("");
                                break;
                            case 1:
                                switch(lh.lhd.funcs[0]) {
                                    case BasicFunction.TAKEDOWN:
                                    case BasicFunction.ALLEY:
                                    case BasicFunction.STT:
                                        if(lh.isSmall) lh.SetOptic("Starburst");
                                        else lh.SetOptic("");
                                        break;
                                    case BasicFunction.FLASHING:
                                    case BasicFunction.CAL_STEADY:
                                        if(lh.isSmall) lh.SetOptic("Small Lineum");
                                        else lh.SetOptic("Lineum");
                                        break;
                                }
                                break;
                            case 2:
                                if(!lh.isSmall) lh.SetOptic("Dual Small Lineum");
                                else lh.SetOptic("Dual Lineum");
                                break;
                        }
                    }
                }
                break;
            case TDOption.LG_SEVEN:
                SetBarSize(3);
                foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) {
                    if(soc.transform.position.y < 0) soc.ShowLong = true;
                }
                yield return new WaitForEndOfFrame();
                foreach(LightHead lh in allHeads) {
                    if(lh.gameObject.activeInHierarchy && lh.transform.position.y < 0) {
                        byte bit = lh.Bit;
                        if(bit > 1 && bit < 10) {
                            lh.lhd.funcs.Clear();
                            lh.lhd.funcs.Add(BasicFunction.TRAFFIC);
                            lh.SetOptic("Lineum", BasicFunction.TRAFFIC);
                        }
                    }
                }
                break;
            case TDOption.SM_EIGHT:
                foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) {
                    if(soc.transform.position.y < 0) soc.ShowLong = false;
                }
                yield return new WaitForEndOfFrame();
                foreach(LightHead lh in allHeads) {
                    if(lh.gameObject.activeInHierarchy && lh.transform.position.y < 0) {
                        byte bit = lh.Bit;
                        if(bit > 1 && bit < 10) {
                            lh.lhd.funcs.Clear();
                            lh.lhd.funcs.Add(BasicFunction.TRAFFIC);
                            lh.SetOptic("Starburst", BasicFunction.TRAFFIC);
                        }
                    }
                }
                break;
            case TDOption.SM_SIX:
                foreach(SizeOptionControl soc in GetComponentsInChildren<SizeOptionControl>(true)) {
                    if(soc.transform.position.y < 0) soc.ShowLong = false;
                }
                yield return new WaitForEndOfFrame();
                foreach(LightHead lh in allHeads) {
                    if(lh.gameObject.activeInHierarchy && lh.transform.position.y < 0) {
                        byte bit = lh.Bit;
                        if(bit > 2 && bit < 9) {
                            lh.lhd.funcs.Clear();
                            lh.lhd.funcs.Add(BasicFunction.TRAFFIC);
                            lh.SetOptic("Starburst", BasicFunction.TRAFFIC);
                        } else if(bit == 2 || bit == 9) {
                            if(lh.lhd.funcs.Contains(BasicFunction.TRAFFIC)) {
                                lh.lhd.funcs.Remove(BasicFunction.TRAFFIC);
                                switch(lh.lhd.funcs.Count) {
                                    case 0:
                                        lh.SetOptic("");
                                        break;
                                    case 1:
                                        switch(lh.lhd.funcs[0]) {
                                            case BasicFunction.TAKEDOWN:
                                            case BasicFunction.ALLEY:
                                            case BasicFunction.STT:
                                                if(lh.isSmall) lh.SetOptic("Starburst");
                                                else lh.SetOptic("");
                                                break;
                                            case BasicFunction.FLASHING:
                                            case BasicFunction.CAL_STEADY:
                                                if(lh.isSmall) lh.SetOptic("Small Lineum");
                                                else lh.SetOptic("Lineum");
                                                break;
                                        }
                                        break;
                                    case 2:
                                        if(!lh.isSmall) lh.SetOptic("Dual Small Lineum");
                                        else lh.SetOptic("Dual Lineum");
                                        break;
                                }
                            }
                        }
                    }
                }
                break;
        }

        foreach(LightLabel ll in GameObject.Find("BarCanvas/Labels").GetComponentsInChildren<LightLabel>(true)) {
            ll.Refresh();
        }

        yield return null;
    }

    public void Save(string filename) {
        if(savePDF) { StartCoroutine(SavePDF(filename)); return; }

        NbtCompound root = new NbtCompound("root");

        root.Add(new NbtByte("size", (byte)BarSize));

        NbtList lightList = new NbtList("lite");
        foreach(LightHead lh in GetComponentsInChildren<LightHead>(true)) {
            NbtCompound lightCmpd = new NbtCompound();
            lightCmpd.Add(new NbtString("path", lh.transform.GetPath()));
            if(lh.lhd.style != null) {
                lightCmpd.Add(new NbtString("optc", lh.lhd.optic.partNumber));
                lightCmpd.Add(new NbtString("styl", lh.lhd.style.name));
            }

            byte fn = 0;
            foreach(BasicFunction bfn in lh.lhd.funcs) {
                fn |= (byte)bfn;
            }
            lightCmpd.Add(new NbtByte("func", fn));

            lightList.Add(lightCmpd);
        }
        root.Add(lightList);

        root.Add(patts);

        NbtList socList = new NbtList("soc");
        foreach(SizeOptionControl soc in transform.GetComponentsInChildren<SizeOptionControl>(true)) {
            NbtCompound socCmpd = new NbtCompound();
            socCmpd.Add(new NbtString("path", soc.transform.GetPath()));
            socCmpd.Add(new NbtByte("isLg", soc.ShowLong ? (byte)1 : (byte)0));
            socList.Add(socCmpd);
        }
        root.Add(socList);

        NbtFile file = new NbtFile(root);
        if(!filename.EndsWith(".bar.nbt")) {
            filename = filename + ".bar.nbt";
        }
        file.SaveToFile(filename, NbtCompression.None);
    }

    public void Open(string filename) {
        NbtFile file = new NbtFile(filename);

        NbtCompound root = file.RootTag;
        BarSize = root["size"].IntValue;

        NbtList lightList = (NbtList)root["lite"];
        NbtList socList = (NbtList)root["soc"];
        Dictionary<string, LightHead> lights = new Dictionary<string, LightHead>();
        Dictionary<string, SizeOptionControl> socs = new Dictionary<string, SizeOptionControl>();

        foreach(LightHead lh in transform.GetComponentsInChildren<LightHead>(true)) {
            lights[lh.transform.GetPath()] = lh;
        }
        foreach(SizeOptionControl soc in transform.GetComponentsInChildren<SizeOptionControl>(true)) {
            socs[soc.transform.GetPath()] = soc;
        }
        foreach(NbtTag alpha in lightList) {
            NbtCompound lightCmpd = alpha as NbtCompound;
            LightHead lh = lights[lightCmpd["path"].StringValue];

            if(lightCmpd.Contains("optc")) {
                LocationNode ln = LightDict.inst.FetchLocation(lh.loc);
                string partNum = lightCmpd["optc"].StringValue;

                foreach(OpticNode on in ln.optics.Values) {
                    if(on.partNumber == partNum) {
                        lh.SetOptic(on.name, BasicFunction.NULL, false);
                        string styleName = lightCmpd["styl"].StringValue;
                        foreach(StyleNode sn in on.styles.Values) {
                            if(sn.name == styleName) {
                                lh.SetStyle(sn);
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            byte fn = lightCmpd["func"].ByteValue;
            List<BasicFunction> potential = new List<BasicFunction>();
            potential.Add(BasicFunction.FLASHING);
            switch(lh.loc) {
                case Location.ALLEY:
                    potential.Add(BasicFunction.ALLEY);
                    break;
                case Location.FRONT:
                    potential.Add(BasicFunction.TAKEDOWN);
                    potential.Add(BasicFunction.EMITTER);
                    potential.Add(BasicFunction.CAL_STEADY);
                    break;
                case Location.REAR:
                    potential.Add(BasicFunction.TAKEDOWN);
                    potential.Add(BasicFunction.TRAFFIC);
                    break;
                case Location.FAR_REAR:
                    potential.Add(BasicFunction.TAKEDOWN);
                    potential.Add(BasicFunction.STT);
                    break;
                case Location.FRONT_CORNER:
                case Location.REAR_CORNER:
                    potential.Add(BasicFunction.TAKEDOWN);
                    potential.Add(BasicFunction.CRUISE);
                    break;
            }
            lh.lhd.funcs.Clear();
            foreach(BasicFunction bfn in potential) {
                if(((byte)bfn & fn) != 0) {
                    lh.lhd.funcs.Add(bfn);
                }
            }
        }

        patts = root.Get<NbtCompound>("pats");

        foreach(NbtTag alpha in socList) {
            NbtCompound socCmpd = alpha as NbtCompound;
            SizeOptionControl soc = socs[socCmpd["path"].StringValue];
            soc.ShowLong = (socCmpd["isLg"].ByteValue == 1);
        }
    }

    public void StartPDF() {
        savePDF = true;
        barFilePath = fb.currFile;
    }

    public void JustSavePDF() {
        Directory.CreateDirectory(Application.dataPath + "\\..\\output");
        StartCoroutine(SavePDF(Application.dataPath + "\\..\\output\\output " + DateTime.Now.ToString("MMddyy HHmmssf") + ".pdf"));
    }

    public IEnumerator SavePDF(string filename) {
        if(!filename.EndsWith(".pdf")) filename = filename + ".pdf";

        PdfDocument doc = new PdfDocument();
        doc.Info.Author = "Star Headlight and Lantern Co., Inc.";
        doc.Info.Creator = "1000 Lightbar Configurator";
        doc.Info.Title = "1000 Lightbar Configuration";

        headNumber = GetCurrentHeads();
        CameraControl.ShowWhole = true;
        CanvasDisabler.CanvasEnabled = false;

        bool debugBit = LightLabel.showBit;
        LightLabel.showBit = false;

        Camera cam = FindObjectOfType<CameraControl>().GetComponent<Camera>();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Vector3 tl = Vector3.zero, br = Vector3.zero;
        foreach(ReferencePoint rp in FindObjectsOfType<ReferencePoint>()) {
            if(rp.gameObject.name == "tl") {
                tl = cam.WorldToScreenPoint(rp.transform.position);
            } else if(rp.gameObject.name == "br") {
                br = cam.WorldToScreenPoint(rp.transform.position);
            }
        }

        Rect capRect = new Rect(tl.x, br.y, br.x - tl.x, tl.y - br.y);

        yield return StartCoroutine(OverviewPage(doc.AddPage(), capRect));
        yield return StartCoroutine(PartsPage(doc.AddPage(), capRect));
        yield return StartCoroutine(WiringPage(doc.AddPage(), capRect));

        LightLabel.showParts = false;
        CanvasDisabler.CanvasEnabled = true;
        CameraControl.ShowWhole = false;

        LightLabel.showBit = debugBit;

        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh();
        }

        try {
            doc.Save(filename);
            Application.OpenURL("file://" + filename);
        } catch(IOException) {
            ErrorText.inst.DispError("Unable to produce PDF.  Do you have the PDF open elsewhere, by chance?");
        } finally {
            if(savePDF)
                fb.currFile = barFilePath;
            savePDF = false;
        }
        yield return null;
    }

    public IEnumerator OverviewPage(PdfPage p, Rect capRect) {
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont courier = new XFont("Courier New", new XUnit(12, XGraphicsUnit.Point).Inch);
        XFont courierSm = new XFont("Courier New", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliLg = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch);
        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliSmBold = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch, XFontStyle.Bold);

        LightLabel.showParts = false;
        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.DispError = false;
            alpha.Refresh(true);
        }

        Texture2D tex = new Texture2D(Mathf.RoundToInt(capRect.width), Mathf.RoundToInt(capRect.height));
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(capRect, 0, 0);
        tex.Apply();

        Directory.CreateDirectory("tempgen");
        File.WriteAllBytes("tempgen\\desc.png", tex.EncodeToPNG());

        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (tex.width * 1.0f);
        gfx.DrawImage(XImage.FromFile("tempgen\\desc.png"), 0.5, 1.3, tex.width * scale, tex.height * scale);
        gfx.DrawImage(XImage.FromFile("pdfassets\\TopLeft.png"), 0.5, 0.5, 0.74, 0.9);
        XImage tr = XImage.FromFile("pdfassets\\TopRight.png");
        gfx.DrawImage(tr, ((float)p.Width.Inch) - 2.45, 0.5, 1.95, 0.75);

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Star 1000", new XFont("Times New Roman", new XUnit(28, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.7, p.Width.Inch - 1.0, 1.0));
        tf.DrawString("Model " + BarModel, courier, XBrushes.Black, new XRect(0.5, 1.1, p.Width.Inch - 1.0, 1.0));

        tf.Alignment = XParagraphAlignment.Left;

        tf.DrawString("Description", caliSmBold, XBrushes.Black, new XRect(1.4, 3.39, 0.5, 0.1));

        for(int i = 0; i < headNumber.Length; i++) {
            LightHead lh = headNumber[i];
            tf.DrawString("Position " + (i + 1).ToString("00"), courierSm, XBrushes.Black, new XRect(0.5, 3.5 + (i * 0.10), 1.2, 0.10));
            if(headNumber[i].lhd.style == null) {
                tf.DrawString(" -- ", caliSm, XBrushes.Black, new XRect(1.4, 3.49 + (i * 0.10), 0.5, 0.10));
            } else {
                tf.DrawString((lh.lhd.optic.styles.Count > 1 ? lh.lhd.style.name + " " : "") + lh.lhd.optic.name, caliSm, XBrushes.Black, new XRect(1.4, 3.49 + (i * 0.10), 2.5, 0.10));
            }
        }

        XPen border = new XPen(XColors.Black, 0.025);
        double top = p.Height.Inch - 2.5;
        gfx.DrawRectangle(border, XBrushes.White, new XRect(0.5, top, p.Width.Inch - 1.0, 2.0));
        gfx.DrawLine(border, 0.5, top + 0.5, p.Width.Inch - 0.5, top + 0.5);

        tf.DrawString("Customer", caliSm, XBrushes.DarkGray, new XRect(0.55, top + 0.01, 1.0, 0.15));
        tf.DrawString("Order Number / PO", caliSm, XBrushes.DarkGray, new XRect(4, top + 0.01, 1.5, 0.15));
        gfx.DrawLine(border, 3.95, top, 3.95, top + 0.5);
        tf.DrawString("Order Date", caliSm, XBrushes.DarkGray, new XRect(6.2, top + 0.01, 1.0, 0.15));
        gfx.DrawLine(border, 6.15, top, 6.15, top + 0.5);

        tf.DrawString(custName.text, caliLg, XBrushes.Black, new XRect(0.6, top + 0.2, 3.0, 0.2));
        tf.DrawString(orderNum.text, courier, XBrushes.Black, new XRect(4.05, top + 0.2, 1.75, 0.2));
        tf.DrawString(System.DateTime.Now.ToString("MMM dd, yyyy"), courier, XBrushes.Black, new XRect(6.25, top + 0.2, 3.0, 0.2));

        tf.DrawString("Order Notes", caliSm, XBrushes.DarkGray, new XRect(0.55, top + 0.51, 1.0, 0.15));
        tf.DrawString(notes.text, caliSm, XBrushes.Black, new XRect(0.6, top + 0.61, p.Width.Inch - 1.2, 1.4));

        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));

        yield return null;
    }

    public IEnumerator PartsPage(PdfPage p, Rect capRect) {
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        XFont courier = new XFont("Courier New", new XUnit(8, XGraphicsUnit.Point).Inch);
        XFont caliBold = new XFont("Calibri", new XUnit(12, XGraphicsUnit.Point).Inch, XFontStyle.Bold);
        XFont caliSm = new XFont("Calibri", new XUnit(8, XGraphicsUnit.Point).Inch);

        LightLabel.showParts = true;
        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh(true);
        }
        LightLabel.showParts = false;

        Texture2D tex = new Texture2D(Mathf.RoundToInt(capRect.width), Mathf.RoundToInt(capRect.height));
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(capRect, 0, 0);
        tex.Apply();

        Directory.CreateDirectory("tempgen");
        File.WriteAllBytes("tempgen\\part.png", tex.EncodeToPNG());

        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (tex.width * 1.0f);
        gfx.DrawImage(XImage.FromFile("tempgen\\part.png"), 0.5, 1.0, tex.width * scale, tex.height * scale);

        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("Model " + BarModel, new XFont("Courier New", new XUnit(24, XGraphicsUnit.Point).Inch, XFontStyle.Bold), XBrushes.Black, new XRect(0.5, 0.5, p.Width.Inch - 1.0, 1.0));
        tf.DrawString("Production Copy - Bill of Materials", caliBold, XBrushes.Black, new XRect(0.5, 0.8, p.Width.Inch - 1.0, 1.0));

        tf.DrawString("Quantity", caliBold, XBrushes.Black, new XRect(0.5, 3.3, 1.0, 0.2));
        tf.Alignment = XParagraphAlignment.Left;
        tf.DrawString("Component", caliBold, XBrushes.Black, new XRect(1.5, 3.3, 1.0, 0.2));
        tf.DrawString("Description", caliBold, XBrushes.Black, new XRect(3.0, 3.3, 1.0, 0.2));

        List<string> parts = new List<string>();
        Dictionary<string, int> counts = new Dictionary<string, int>();
        Dictionary<string, LightHead> descs = new Dictionary<string, LightHead>();
        foreach(LightHead lh in BarManager.headNumber) {
            if(lh.lhd.style != null) {
                string part = lh.PartNumber;
                if(counts.ContainsKey(part)) {
                    counts[part]++;
                } else {
                    counts[part] = 1;
                    descs[part] = lh;
                    parts.Add(part);
                }
            }
        }

        double top = 3.5;
        foreach(string part in parts) {
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(counts[part] + "", courier, XBrushes.Black, new XRect(0.5, top, 1.0, 0.2));
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(descs[part].PartNumber, courier, XBrushes.Black, new XRect(1.5, top, 1.0, 0.2));
            tf.DrawString((descs[part].lhd.optic.styles.Count > 1 ? descs[part].lhd.style.name + " " : "") + descs[part].lhd.optic.name, caliSm, XBrushes.Black, new XRect(3.0, top, 1.0, 0.2));
            top += 0.15;
        }

        tf.Alignment = XParagraphAlignment.Right;
        tf.DrawString("(C) 2015 Star Headlight and Lantern Co., Inc.", caliSm, XBrushes.DarkGray, new XRect(0.5, p.Height.Inch - 0.49, p.Width.Inch - 1.0, 0.2));

        yield return null;
    }

    public IEnumerator WiringPage(PdfPage p, Rect capRect) {
        XGraphics gfx = XGraphics.FromPdfPage(p, XGraphicsUnit.Inch);
        XTextFormatter tf = new XTextFormatter(gfx);

        LightLabel.showWire = true;
        foreach(LightLabel alpha in FindObjectsOfType<LightLabel>()) {
            alpha.Refresh(true);
        }
        LightLabel.showWire = false;

        Texture2D tex = new Texture2D(Mathf.RoundToInt(capRect.width), Mathf.RoundToInt(capRect.height));
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(capRect, 0, 0);
        tex.Apply();

        Directory.CreateDirectory("tempgen");
        File.WriteAllBytes("tempgen\\wire.png", tex.EncodeToPNG());

        float scale = (((float)p.Width.Inch * 1.0f) - 1.0f) / (tex.width * 1.0f);
        gfx.DrawImage(XImage.FromFile("tempgen\\wire.png"), 0.5, 1.0, tex.width * scale, tex.height * scale);
    }

    public static XPoint[] XPointArray(params Vector2[] vecs) {
        XPoint[] rtn = new XPoint[vecs.Length];
        for(int i = 0; i < vecs.Length; i++) {
            rtn[i] = vecs[i].ToXPoint();
        }
        return rtn;
    }

    public void Clear() {
        foreach(LightHead lh in transform.GetComponentsInChildren<LightHead>(true)) {
            lh.SetOptic("");
            lh.patterns.Clear();
        }
        foreach(SizeOptionControl soc in transform.GetComponentsInChildren<SizeOptionControl>(true)) {
            soc.ShowLong = true;
        }
    }

    public LightHead[] GetCurrentHeads() {
        List<LightHead> rtn = new List<LightHead>(50);

        RaycastHit info;
        if(Physics.Raycast(new Ray(first.transform.position, new Vector3(1, 0.5f)), out info)) {
            rtn.Add(first);
            LightHead curr = info.transform.GetComponent<LightHead>();
            while(curr != first && rtn.Count < 50) {
                rtn.Add(curr);
                Ray ray;
                switch(curr.loc) {
                    case Location.FRONT_CORNER:
                        if(curr.transform.position.x < 0) {
                            ray = new Ray(curr.transform.position, new Vector3(1, 0));
                        } else {
                            ray = new Ray(curr.transform.position, new Vector3(1, -1));
                        }
                        break;
                    case Location.ALLEY:
                        if(curr.transform.position.x < 0) {
                            ray = new Ray(curr.transform.position, new Vector3(0, 1));
                        } else {
                            ray = new Ray(curr.transform.position, new Vector3(-0.5f, -1));
                        }
                        break;
                    case Location.REAR_CORNER:
                        if(curr.transform.position.x < 0) {
                            ray = new Ray(curr.transform.position, new Vector3(-1, 1));
                        } else {
                            ray = new Ray(curr.transform.position, new Vector3(-1, -0.5f));
                        }
                        break;
                    case Location.FAR_REAR:
                    case Location.REAR:
                        ray = new Ray(curr.transform.position, new Vector3(-1, 0));
                        break;
                    case Location.FRONT:
                    default:
                        ray = new Ray(curr.transform.position, new Vector3(1, 0));
                        break;
                }
                if(Physics.Raycast(ray, out info))
                    curr = info.transform.GetComponent<LightHead>();
                else
                    break;
            }
        }

        return rtn.ToArray();
    }

    public IEnumerator RefreshAllLabels() {
        yield return new WaitForEndOfFrame();

        foreach(LightHead alpha in allHeads) {
            if(alpha.gameObject.activeInHierarchy) alpha.myLabel.Refresh();
        }

        yield return null;
    }
}