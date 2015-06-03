using System;
using System.Collections.Generic;
using System.IO;
using fNbt;
using UnityEngine;

public class LightDict : MonoBehaviour {
    public static LightDict inst;
    public static SteadyPattern stdy;
    public Dictionary<Location, LocationNode> lights;
    public List<AdvFunction> steadyBurn;
    public List<Pattern> flashPatts, warnPatts, tdPatts;

    public Dictionary<Location, List<AdvFunction>> capableFunctions;

    void Awake() {
        if(inst == null) inst = this;

        lights = new Dictionary<Location, LocationNode>();
        steadyBurn = new List<AdvFunction>(new AdvFunction[] { AdvFunction.TAKEDOWN, AdvFunction.ALLEY, AdvFunction.STT_AND_TAIL, AdvFunction.T13, AdvFunction.EMITTER });
        stdy = new SteadyPattern();

        if(File.Exists("lib.nbt")) {
            foreach(Location l in new Location[] { Location.FRONT, Location.FRONT_CORNER, Location.ALLEY, Location.REAR_CORNER, Location.FAR_REAR, Location.REAR }) {
                lights[l] = new LocationNode();
            }

            try {
                NbtFile cat = new NbtFile("lib.nbt");

                if(!cat.RootTag.Contains("pric")) {
                    Destroy(FindObjectOfType<DispPricing>().gameObject);
                }

                NbtList heads = (NbtList)(cat.RootTag["heads"]);

                foreach(NbtTag head in heads) {
                    OpticNode optNode = new OpticNode((NbtCompound)head);

                    NbtList locs = (NbtList)head["locs"];
                    foreach(NbtTag loc in locs) {
                        string strLoc = loc.StringValue;

                        switch(strLoc) {
                            case "f":
                                lights[Location.FRONT].Add(optNode);
                                break;
                            case "fc":
                                lights[Location.FRONT_CORNER].Add(optNode);
                                break;
                            case "a":
                                lights[Location.ALLEY].Add(optNode);
                                break;
                            case "rc":
                                lights[Location.REAR_CORNER].Add(optNode);
                                break;
                            case "rf":
                                lights[Location.FAR_REAR].Add(optNode);
                                break;
                            case "r":
                                lights[Location.REAR].Add(optNode);
                                break;
                            default:
                                break;
                        }
                    }
                }

                NbtCompound pattstag = cat.RootTag.Get<NbtCompound>("patts");

                warnPatts = new List<Pattern>();
                NbtList patlist = pattstag.Get<NbtList>("flash");
                foreach(NbtTag alpha in patlist) {
                    warnPatts.Add(new WarnPatt((NbtCompound)alpha));
                }

                flashPatts = new List<Pattern>();
                patlist = pattstag.Get<NbtList>("sflsh");
                foreach(NbtTag alpha in patlist) {
                    flashPatts.Add(new FlashPatt((NbtCompound)alpha));
                }


                tdPatts = new List<Pattern>();
                patlist = pattstag.Get<NbtList>("traff");
                foreach(NbtTag alpha in patlist) {
                    tdPatts.Add(new TraffPatt((NbtCompound)alpha));
                }



            } catch(NbtFormatException ex) {
                ErrorText.inst.DispError("Could not parse the file.  Are you certain you got this file from Star?");
                Debug.LogException(ex);
            } catch(EndOfStreamException ex) {
                ErrorText.inst.DispError("Could not parse the file.  Are you certain you got this file from Star?");
                Debug.LogException(ex);
            } catch(InvalidCastException ex) {
                ErrorText.inst.DispError("Could not parse the file.  Are you certain you got this file from Star?");
                Debug.LogException(ex);
            } catch(NullReferenceException ex) {
                ErrorText.inst.DispError("Could not parse the file.  Are you certain you got this file from Star?");
                Debug.LogException(ex);
            }
        } else {
            ErrorText.inst.DispError("You seem to be missing the 'lib.nbt' file.  Make sure it's in the same directory as the executable.");
        }

        capableFunctions = new Dictionary<Location, List<AdvFunction>>();

        capableFunctions[Location.FRONT] = new List<AdvFunction>(new AdvFunction[] { AdvFunction.TAKEDOWN, AdvFunction.T13, AdvFunction.DIM, AdvFunction.EMITTER });
        capableFunctions[Location.FRONT_CORNER] = new List<AdvFunction>(new AdvFunction[] { AdvFunction.TAKEDOWN, AdvFunction.ICL, AdvFunction.DIM, AdvFunction.CRUISE });
        capableFunctions[Location.REAR_CORNER] = new List<AdvFunction>(new AdvFunction[] { AdvFunction.TAKEDOWN, AdvFunction.DIM, AdvFunction.CRUISE });
        capableFunctions[Location.ALLEY] = new List<AdvFunction>(new AdvFunction[] { AdvFunction.ALLEY, AdvFunction.DIM });
        capableFunctions[Location.REAR] = new List<AdvFunction>(new AdvFunction[] { AdvFunction.TAKEDOWN, AdvFunction.TRAFFIC, AdvFunction.DIM });
        capableFunctions[Location.FAR_REAR] = new List<AdvFunction>(new AdvFunction[] { AdvFunction.STT_AND_TAIL, AdvFunction.DIM });

        foreach(Location l in new Location[] { Location.FRONT, Location.FRONT_CORNER, Location.ALLEY, Location.REAR_CORNER, Location.FAR_REAR, Location.REAR }) {
            capableFunctions[l].AddRange(new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5 });
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "Dict.png", true);
    }

    public LocationNode FetchLocation(params Location[] locs) {
        if(locs.Length == 0) {
            throw new ArgumentException();
        }
        if(locs.Length == 1) {
            return lights[locs[0]];
        }

        LocationNode rtn = lights[locs[0]].Clone();
        List<Location> loclist = new List<Location>(locs);
        loclist.RemoveAt(0);
        foreach(Location l in loclist) {
            LocationNode dln = lights[l];
            foreach(OpticNode rtnon in new List<OpticNode>(rtn.optics.Values)) {
                if(dln.optics.ContainsKey(rtnon.name)) {
                    foreach(StyleNode rtnsn in new List<StyleNode>(rtnon.styles.Values)) {
                        if(!(dln.optics[rtnon.name].styles.ContainsKey(rtnsn.name) && dln.optics[rtnon.name].styles[rtnsn.name] == rtnsn)) {
                            rtnon.Remove(rtnsn);
                        } else {
                            rtnsn.selectable &= dln.optics[rtnon.name].styles[rtnsn.name].selectable;
                        }
                    }

                } else {
                    rtn.Remove(rtnon);
                }
            }
        }

        return rtn;
    }

    public OpticNode FetchOptic(Location loc, string optic) {
        if(lights[loc].optics.ContainsKey(optic))
            return lights[loc].optics[optic];
        else
            return null;
    }
}

public abstract class Pattern {
    public string name;

    public ushort id;
    public ushort t0, t1, t2, t3;
}

public class WarnPatt : Pattern {
    public short[] definition;

    public WarnPatt(NbtCompound cmpd) {
        name = cmpd["name"].StringValue;
        id = (ushort)cmpd["id"].ShortValue;
        t0 = (ushort)cmpd["t0"].ShortValue;
        t1 = (ushort)cmpd["t1"].ShortValue;
        t2 = (ushort)cmpd["t2"].ShortValue;
        t3 = (ushort)cmpd["t3"].ShortValue;

        NbtIntArray patttag = cmpd.Get<NbtIntArray>("patt");
        int[] vals = patttag.Value;

        definition = new short[vals.Length];

        for(int i = 0; i < vals.Length; i++) {
            definition[i] = (short)(vals[i] & 0xFFFF);
        }
    }
}

public class FlashPatt : Pattern {
    public byte[] definition;

    public FlashPatt(NbtCompound cmpd) {
        name = cmpd["name"].StringValue;
        id = (ushort)cmpd["id"].ShortValue;
        t0 = (ushort)cmpd["t0"].ShortValue;
        t1 = (ushort)cmpd["t1"].ShortValue;
        t2 = (ushort)cmpd["t2"].ShortValue;
        t3 = (ushort)cmpd["t3"].ShortValue;

        NbtByteArray patttag = cmpd.Get<NbtByteArray>("patt");
        definition = patttag.Value;
    }
}

public class TraffPatt : Pattern {
    public short[] left6, right6, center6, left8, right8, center8;

    public TraffPatt(NbtCompound cmpd) {
        name = cmpd["name"].StringValue;
        id = (ushort)cmpd["id"].ShortValue;
        t0 = (ushort)cmpd["t0"].ShortValue;
        t1 = (ushort)cmpd["t1"].ShortValue;
        t2 = (ushort)cmpd["t2"].ShortValue;
        t3 = (ushort)cmpd["t3"].ShortValue;

        NbtIntArray patttag = cmpd.Get<NbtCompound>("6hed").Get<NbtIntArray>("cntr");
        center6 = new short[patttag.Value.Length];
        for(int i = 0; i < patttag.Value.Length; i++) {
            center6[i] = (short)(patttag[i] & 0xFFFF);
        }
        patttag = cmpd.Get<NbtCompound>("6hed").Get<NbtIntArray>("left");
        left6 = new short[patttag.Value.Length];
        for(int i = 0; i < patttag.Value.Length; i++) {
            left6[i] = (short)(patttag[i] & 0xFFFF);
        }
        patttag = cmpd.Get<NbtCompound>("6hed").Get<NbtIntArray>("rite");
        right6 = new short[patttag.Value.Length];
        for(int i = 0; i < patttag.Value.Length; i++) {
            right6[i] = (short)(patttag[i] & 0xFFFF);
        }
        patttag = cmpd.Get<NbtCompound>("8hed").Get<NbtIntArray>("cntr");
        center8 = new short[patttag.Value.Length];
        for(int i = 0; i < patttag.Value.Length; i++) {
            center8[i] = (short)(patttag[i] & 0xFFFF);
        }
        patttag = cmpd.Get<NbtCompound>("8hed").Get<NbtIntArray>("left");
        left8 = new short[patttag.Value.Length];
        for(int i = 0; i < patttag.Value.Length; i++) {
            left8[i] = (short)(patttag[i] & 0xFFFF);
        }
        patttag = cmpd.Get<NbtCompound>("8hed").Get<NbtIntArray>("rite");
        right8 = new short[patttag.Value.Length];
        for(int i = 0; i < patttag.Value.Length; i++) {
            right8[i] = (short)(patttag[i] & 0xFFFF);
        }
    }
}

public class SteadyPattern : Pattern {
    public SteadyPattern() {
        name = "Steady Burn";
    }
}

public class LocationNode {
    public Dictionary<string, OpticNode> optics;

    public LocationNode() {
        optics = new Dictionary<string, OpticNode>();
    }

    /// <summary>
    /// Add a OpticNode to this Function.
    /// </summary>
    public void Add(OpticNode t) {
        optics[t.name] = t;
    }

    /// <summary>
    /// Add multiple OpticNodes to this Function.
    /// </summary>
    public void AddAll(IEnumerable<OpticNode> ts) {
        foreach(OpticNode t in ts) {
            Add(t);
        }
    }

    /// <summary>
    /// Remove a OpticNode from this Function.
    /// </summary>
    public void Remove(OpticNode t) {
        if(optics.ContainsKey(t.name)) {
            optics.Remove(t.name);
        }
    }

    /// <summary>
    /// Creates a copy of this Function Node.
    /// </summary>
    /// <returns>A copy of this Function Node.</returns>
    public LocationNode Clone() {
        LocationNode rtn = new LocationNode();

        foreach(string alpha in optics.Keys) {
            rtn.optics[alpha] = optics[alpha].Clone();
        }

        return rtn;
    }

    public static bool operator ==(LocationNode left, LocationNode right) {
        if((System.Object)left == null || (System.Object)right == null) {
            return (System.Object)left == (System.Object)right;
        } else if(left.Equals(right)) {
            return true;
        } else {
            return false;
        }
    }

    public static bool operator !=(LocationNode left, LocationNode right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return (obj != null) && (obj.GetType() == typeof(LocationNode)) && (((LocationNode)obj).GetHashCode() == this.GetHashCode());
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
}

public class OpticNode {
    public Dictionary<string, StyleNode> styles;

    public string name;
    public string partNumber;
    public bool fitsLg = false, fitsSm = false;
    public string smEquivalent, lgEquivalent;

    public uint cost, amperage;

    public bool dual = false;

    public OpticNode() {
        styles = new Dictionary<string, StyleNode>();
        name = "";
        partNumber = "";
        smEquivalent = "";
        lgEquivalent = "";
        cost = amperage = 0;
        fitsLg = false;
        fitsSm = false;
        dual = false;
    }

    public OpticNode(fNbt.NbtCompound defTag) {
        styles = new Dictionary<string, StyleNode>();
        this.name = defTag["name"].StringValue;
        this.partNumber = defTag["part"].StringValue;
        this.fitsSm = defTag.Contains("sm");
        this.fitsLg = defTag.Contains("lg");
        if(defTag.Contains("smEq")) {
            smEquivalent = defTag["smEq"].StringValue;
        } else {
            smEquivalent = "";
        }
        if(defTag.Contains("lgEq")) {
            lgEquivalent = defTag["lgEq"].StringValue;
        } else {
            lgEquivalent = "";
        }
        dual = false;

        cost = (defTag.Contains("cost")) ? (uint)defTag["cost"].IntValue : 0;
        amperage = (uint)defTag["amp"].IntValue;

        NbtList stylList = (NbtList)defTag["styl"];
        foreach(NbtTag style in stylList) {
            Add(new StyleNode((NbtCompound)style));
        }
    }

    /// <summary>
    /// Add a StyleNode to this Function.
    /// </summary>
    public void Add(StyleNode t) {
        styles[t.name] = t;
        if(t.isDualColor) {
            dual = true;
        }
    }

    /// <summary>
    /// Add multiple StyleNodes to this Function.
    /// </summary>
    public void AddAll(IEnumerable<StyleNode> ts) {
        foreach(StyleNode t in ts) {
            Add(t);
        }
    }

    /// <summary>
    /// Remove a StyleNode from this Function.
    /// </summary>
    public void Remove(StyleNode t) {
        if(styles.ContainsKey(t.name)) {
            styles.Remove(t.name);
        }
    }

    /// <summary>
    /// Creates a copy of this Function Node.
    /// </summary>
    /// <returns>A copy of this Function Node.</returns>
    public OpticNode Clone() {
        OpticNode rtn = new OpticNode();

        foreach(string alpha in styles.Keys) {
            rtn.styles[alpha] = styles[alpha].Clone();
        }

        rtn.name = name;
        rtn.partNumber = partNumber;
        rtn.fitsLg = fitsLg;
        rtn.fitsSm = fitsSm;
        rtn.lgEquivalent = lgEquivalent;
        rtn.smEquivalent = smEquivalent;
        rtn.dual = dual;
        rtn.cost = cost;
        rtn.amperage = amperage;

        return rtn;
    }

    public static bool operator ==(OpticNode left, OpticNode right) {
        if((System.Object)left == null || (System.Object)right == null) {
            return (System.Object)left == (System.Object)right;
        } else if(left.Equals(right)) {
            return true;
        } else {
            return (left.name.Equals(right.name));
        }
    }

    public static bool operator !=(OpticNode left, OpticNode right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return (obj != null) && (obj.GetType() == typeof(OpticNode)) && (((OpticNode)obj).GetHashCode() == this.GetHashCode());
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
}

public class StyleNode {
    public string name;
    public string partSuffix;
    public bool isDualColor = false;
    public Color color, color2;
    public bool selectable = true;

    public StyleNode() {
        name = "";
        partSuffix = "";
        isDualColor = false;
        color = color2 = Color.white;
        selectable = true;
    }

    public StyleNode(NbtCompound defTag) {
        this.name = defTag["name"].StringValue;
        this.partSuffix = defTag["suff"].StringValue;
        byte[] clrA = defTag["clr"].ByteArrayValue;
        this.color = new Color32(clrA[0], clrA[1], clrA[2], clrA[3]);
        if(defTag.Contains("clr2")) {
            this.isDualColor = true;
            byte[] clrB = defTag["clr2"].ByteArrayValue;
            this.color2 = new Color32(clrB[0], clrB[1], clrB[2], clrB[3]);
        } else {
            this.isDualColor = false;
            this.color2 = Color.white;
        }
        selectable = !defTag.Contains("ns");
    }

    /// <summary>
    /// Creates a copy of this Function Node.
    /// </summary>
    /// <returns>A copy of this Function Node.</returns>
    public StyleNode Clone() {
        StyleNode rtn = new StyleNode();

        rtn.name = name;
        rtn.partSuffix = partSuffix;
        rtn.isDualColor = isDualColor;
        rtn.color = color;
        if(isDualColor) {
            rtn.color2 = color2;
        }
        rtn.selectable = selectable;

        return rtn;
    }

    public static bool operator ==(StyleNode left, StyleNode right) {
        if((System.Object)left == null || (System.Object)right == null) {
            return (System.Object)left == (System.Object)right;
        } else if(left.Equals(right)) {
            return true;
        } else {
            return (left.name.Equals(right.name));
        }
    }

    public static bool operator !=(StyleNode left, StyleNode right) {
        return !(left == right);
    }

    public override bool Equals(object obj) {
        return (obj != null) && (obj.GetType() == typeof(StyleNode)) && (((StyleNode)obj).GetHashCode() == this.GetHashCode());
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
}

public enum Location {
    FRONT, FRONT_CORNER, ALLEY, REAR_CORNER, REAR, FAR_REAR
}

public enum BasicFunction {
    NULL = 0x0, FLASHING = 0x1, STEADY = 0x2, EMITTER = 0x4, CAL_STEADY = 0x8, CRUISE = 0x10, STT = 0x20, TRAFFIC = 0x40
}

public enum AdvFunction {
    NONE = 0x0,
    LEVEL1 = 0x1, LEVEL2 = 0x2, LEVEL3 = 0x4, LEVEL4 = 0x8, LEVEL5 = 0x10,
    TAKEDOWN = 0x20, ICL = 0x40, ALLEY = 0x80, T13 = 0x100, CRUISE = 0x200,
    TRAFFIC = 0x400, DIM = 0x800, STT_AND_TAIL = 0x1000, EMITTER = 0x2000
}

public enum TDOption {
    NONE = 0, LG_SEVEN = 1, SM_EIGHT = 2, SM_SIX = 3, LG_EIGHT = 4, LG_SIX = 5
}

public static class Extensions {
    public static void DisableBit(this NbtShort value, byte bit) {
        value.Value = (short)(value.Value & ~(0x1 << bit));
    }

    public static void EnableBit(this NbtShort value, byte bit) {
        value.Value = (short)(value.Value | (short)(0x1 << bit));
    }

    public static string GetPath(this Transform t) {
        if(t.parent == null) return "/" + t.name;
        else return t.parent.GetPath() + "/" + t.name;
    }

    public static string GetPath(this Component c) {
        return c.transform.GetPath() + ":" + c.GetType().ToString();
    }

    public static PdfSharp.Drawing.XColor ToXColor(this Color c) {
        return PdfSharp.Drawing.XColor.FromArgb(Mathf.RoundToInt(c.a * 255), Mathf.RoundToInt(c.r * 255), Mathf.RoundToInt(c.g * 255), Mathf.RoundToInt(c.b * 255));
    }

    public static PdfSharp.Drawing.XColor ToXColor(this Color32 c) {
        return PdfSharp.Drawing.XColor.FromArgb(c.a, c.r, c.g, c.b);
    }

    public static PdfSharp.Drawing.XPoint ToXPoint(this Vector2 v) {
        return new PdfSharp.Drawing.XPoint(v.x, v.y);
    }
}