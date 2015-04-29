using UnityEngine;
using System.Collections;
using fNbt;
using System.Collections.Generic;

public class BarManager : MonoBehaviour {
    public int BarSize = 3;

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

            // something about saving the selected patterns

            lightList.Add(lightCmpd);
        }
        root.Add(lightList);

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

            //something about loading selected patterns
        }
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

public static class TransformExtensions {
    public static string GetPath(this Transform t) {
        if(t.parent == null) return "/" + t.name;
        else return t.parent.GetPath() + "/" + t.name;
    }

    public static string GetPath(this Component c) {
        return c.transform.GetPath() + ":" + c.GetType().ToString();
    }
}