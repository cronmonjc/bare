using UnityEngine;
using System.Collections;
using fNbt;
using System.Collections.Generic;

public class BarManager : MonoBehaviour {
    public int BarSize = 3;

    public NbtCompound patts;

    void Start() {
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
    }

    public static string GetFnString(Transform t, Function f) {
        switch(f) {
            case Function.ALLEY:
                if(t.position.x < 0) {
                    return "lall";
                } else {
                    return "rall";
                }
            case Function.CRUISE:
                return "cru";
            case Function.DIM:
                return "dim";
            case Function.EMITTER:
                return "emi";
            case Function.ICL:
                return "icl";
            case Function.LEVEL1:
                return "l1";
            case Function.LEVEL2:
                return "l2";
            case Function.LEVEL3:
                return "l3";
            case Function.LEVEL4:
                return "l4";
            case Function.LEVEL5:
                return "l5";
            case Function.STT_AND_TAIL:
                if(t.position.x < 0) {
                    return "ltai";
                } else {
                    return "rtai";
                }
            case Function.TAKEDOWN:
                return "td";
            case Function.TRAFFIC:
                return "traf";
            case Function.T13:
                return "cal";
            default:
                return null;
        }
    }

    public void SetBarSize(int to) {
        if(to < 4 && to > -1) {
            BarSize = to;
        }
    }

    public void Save(string filename) {
        NbtCompound root = new NbtCompound("root");

        root.Add(new NbtByte("size", (byte)BarSize));

        NbtList lightList = new NbtList("lite");
        foreach(LightHead lh in transform.GetComponentsInChildren<LightHead>(true)) {
            NbtCompound lightCmpd = new NbtCompound();
            lightCmpd.Add(new NbtString("path", lh.transform.GetPath()));
            if(lh.lhd.style != null) {
                lightCmpd.Add(new NbtString("optc", lh.lhd.optic.partNumber));
                lightCmpd.Add(new NbtString("styl", lh.lhd.style.name));
            }

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
                        lh.SetOptic(on.name, false);
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
        }

        patts = root.Get<NbtCompound>("pats");

        foreach(NbtTag alpha in socList) {
            NbtCompound socCmpd = alpha as NbtCompound;
            SizeOptionControl soc = socs[socCmpd["path"].StringValue];
            soc.ShowLong = (socCmpd["isLg"].ByteValue == 1);
        }
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
}