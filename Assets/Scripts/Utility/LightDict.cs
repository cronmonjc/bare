using System;
using System.Collections.Generic;
using System.IO;
using fNbt;
using UnityEngine;

public class LightDict : MonoBehaviour {
    public static LightDict inst;
    public static SteadyPattern stdy;
    public Dictionary<Location, LocationNode> lights;
    public List<Lens> lenses;
    public List<AdvFunction> steadyBurn;
    public List<Pattern> flashPatts, tdPatts;
    public short pattBase = 0;

    void Awake() {
        if(inst == null) inst = this;

        lights = new Dictionary<Location, LocationNode>();
        this.lenses = new List<Lens>();
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

                pattBase = pattstag["base"].ShortValue;

                flashPatts = new List<Pattern>();
                NbtList patlist = pattstag.Get<NbtList>("sflsh");
                foreach(NbtTag alpha in patlist) {
                    if(((NbtCompound)alpha).Contains("ref")) { flashPatts.Add(new SingleFlashRefPattern((NbtCompound)alpha)); } else { flashPatts.Add(new FlashPatt((NbtCompound)alpha)); }
                }
                patlist = pattstag.Get<NbtList>("dcflash");
                foreach(NbtTag alpha in patlist) {
                    flashPatts.Add(new DoubleFlashRefPattern((NbtCompound)alpha));
                }
                patlist = pattstag.Get<NbtList>("flash");
                foreach(NbtTag alpha in patlist) {
                    flashPatts.Add(new WarnPatt((NbtCompound)alpha));
                }


                tdPatts = new List<Pattern>();
                patlist = pattstag.Get<NbtList>("traff");
                foreach(NbtTag alpha in patlist) {
                    tdPatts.Add(new TraffPatt((NbtCompound)alpha));
                }


                NbtCompound lensCmpd = cat.RootTag.Get<NbtCompound>("lenses");
                Lens.lgPrefix = lensCmpd["lgFix"].StringValue;
                Lens.smPrefix = lensCmpd["smFix"].StringValue;
                NbtList lenses = lensCmpd.Get<NbtList>("opts");
                foreach(NbtTag alpha in lenses) {
                    this.lenses.Add(new Lens(alpha as NbtCompound));
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
                } else if(dln.optics.ContainsKey(rtnon.smEquivalent)) {
                    foreach(StyleNode rtnsn in new List<StyleNode>(rtnon.styles.Values)) {
                        if(!(dln.optics[rtnon.smEquivalent].styles.ContainsKey(rtnsn.name) && dln.optics[rtnon.smEquivalent].styles[rtnsn.name] == rtnsn)) {
                            rtnon.Remove(rtnsn);
                        } else {
                            rtnsn.selectable &= dln.optics[rtnon.smEquivalent].styles[rtnsn.name].selectable;
                        }
                    }
                } else if(dln.optics.ContainsKey(rtnon.lgEquivalent)) {
                    foreach(StyleNode rtnsn in new List<StyleNode>(rtnon.styles.Values)) {
                        if(!(dln.optics[rtnon.lgEquivalent].styles.ContainsKey(rtnsn.name) && dln.optics[rtnon.lgEquivalent].styles[rtnsn.name] == rtnsn)) {
                            rtnon.Remove(rtnsn);
                        } else {
                            rtnsn.selectable &= dln.optics[rtnon.lgEquivalent].styles[rtnsn.name].selectable;
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
    public abstract ulong period { get; }
    public abstract bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit);
}

public class WarnPatt : Pattern {
    public short[] definition;
    private ulong _period = 0;
    public override ulong period {
        get { return _period; }
    }


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

        _period = 0uL;

        for(int i = 0; i < vals.Length; i++) {
            definition[i] = (short)(vals[i] & 0xFFFF);
            switch((vals[i] >> 14) & 0x3) {
                case 0:
                    _period += t0;
                    break;
                case 1:
                    _period += t1;
                    break;
                case 2:
                    _period += t2;
                    break;
                case 3:
                    _period += t3;
                    break;
            }
        }
    }

    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        tick %= period;
        foreach(short b in definition) {
            switch(0x3 & (b >> 14)) {
                case 0:
                    if(tick < t0) {
                        return (b & (0x1 << bit)) > 0;
                    } else {
                        tick -= t0;
                    }
                    break;
                case 1:
                    if(tick < t1) {
                        return (b & (0x1 << bit)) > 0;
                    } else {
                        tick -= t1;
                    }
                    break;
                case 2:
                    if(tick < t2) {
                        return (b & (0x1 << bit)) > 0;
                    } else {
                        tick -= t2;
                    }
                    break;
                case 3:
                    if(tick < t3) {
                        return (b & (0x1 << bit)) > 0;
                    } else {
                        tick -= t3;
                    }
                    break;
                default:
                    break;
            }
        }
        return false;
    }
}

public class FlashPatt : Pattern {
    public byte[] definition;
    private ulong _period = 0;
    public override ulong period {
        get { return _period; }
    }

    public FlashPatt(NbtCompound cmpd) {
        name = cmpd["name"].StringValue;
        id = (ushort)cmpd["id"].ShortValue;
        t0 = (ushort)cmpd["t0"].ShortValue;
        t1 = (ushort)cmpd["t1"].ShortValue;
        t2 = (ushort)cmpd["t2"].ShortValue;
        t3 = (ushort)cmpd["t3"].ShortValue;

        NbtByteArray patttag = cmpd.Get<NbtByteArray>("patt");
        definition = patttag.Value;

        _period = 0uL;

        for(int i = 0; i < definition.Length; i++) {
            switch((definition[i] >> 2) & 0x3) {
                case 0:
                    _period += t0;
                    break;
                case 1:
                    _period += t1;
                    break;
                case 2:
                    _period += t2;
                    break;
                case 3:
                    _period += t3;
                    break;
            }
        }
    }

    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        tick %= period;

        foreach(byte b in definition) {
            switch(0x3 & (b >> 2)) {
                case 0:
                    if(tick < t0) {
                        return (b & (phaseB ? 0x2 : 0x1)) > 0;
                    } else {
                        tick -= t0;
                    }
                    break;
                case 1:
                    if(tick < t1) {
                        return (b & (phaseB ? 0x2 : 0x1)) > 0;
                    } else {
                        tick -= t1;
                    }
                    break;
                case 2:
                    if(tick < t2) {
                        return (b & (phaseB ? 0x2 : 0x1)) > 0;
                    } else {
                        tick -= t2;
                    }
                    break;
                case 3:
                    if(tick < t3) {
                        return (b & (phaseB ? 0x2 : 0x1)) > 0;
                    } else {
                        tick -= t3;
                    }
                    break;
                default:
                    break;
            }
        }

        return false;
    }
}

public class TraffPatt : Pattern {
    public static bool directLeft = false, sixHeads = false;

    public short[] left6, right6, center6, left8, right8, center8;
    public ulong period6, period8;
    public override ulong period {
        get { return (sixHeads ? period6 : period8); }
    }

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

        period6 = period8 = 0;

        foreach(short alpha in center6) {
            switch((alpha >> 14) & 0x3) {
                case 0:
                    period6 += t0;
                    break;
                case 1:
                    period6 += t1;
                    break;
                case 2:
                    period6 += t2;
                    break;
                case 3:
                    period6 += t3;
                    break;
            }
        }
        foreach(short alpha in center8) {
            switch((alpha >> 14) & 0x3) {
                case 0:
                    period8 += t0;
                    break;
                case 1:
                    period8 += t1;
                    break;
                case 2:
                    period8 += t2;
                    break;
                case 3:
                    period8 += t3;
                    break;
            }
        }
    }

    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {

        if(sixHeads) {
            tick %= period6;
            if(directLeft) {
                foreach(short b in left6) {
                    switch(0x3 & (b >> 14)) {
                        case 0:
                            if(tick < t0) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t0;
                            }
                            break;
                        case 1:
                            if(tick < t1) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t1;
                            }
                            break;
                        case 2:
                            if(tick < t2) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t2;
                            }
                            break;
                        case 3:
                            if(tick < t3) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t3;
                            }
                            break;
                        default:
                            break;
                    }
                }
            } else {
                foreach(short b in right6) {
                    switch(0x3 & (b >> 14)) {
                        case 0:
                            if(tick < t0) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t0;
                            }
                            break;
                        case 1:
                            if(tick < t1) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t1;
                            }
                            break;
                        case 2:
                            if(tick < t2) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t2;
                            }
                            break;
                        case 3:
                            if(tick < t3) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t3;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        } else {
            tick %= period8;
            if(directLeft) {
                foreach(short b in left8) {
                    switch(0x3 & (b >> 14)) {
                        case 0:
                            if(tick < t0) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t0;
                            }
                            break;
                        case 1:
                            if(tick < t1) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t1;
                            }
                            break;
                        case 2:
                            if(tick < t2) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t2;
                            }
                            break;
                        case 3:
                            if(tick < t3) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t3;
                            }
                            break;
                        default:
                            break;
                    }
                }
            } else {
                foreach(short b in right8) {
                    switch(0x3 & (b >> 14)) {
                        case 0:
                            if(tick < t0) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t0;
                            }
                            break;
                        case 1:
                            if(tick < t1) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t1;
                            }
                            break;
                        case 2:
                            if(tick < t2) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t2;
                            }
                            break;
                        case 3:
                            if(tick < t3) {
                                return (b & (0x1 << bit)) > 0;
                            } else {
                                tick -= t3;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        return false;
    }
}

public class SteadyPattern : Pattern {
    public override ulong period {
        get {
            return 1;
        }
    }

    public SteadyPattern() {
        name = "Steady Burn";
    }

    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        return true;
    }
}

public class SingleFlashRefPattern : Pattern {
    public ulong _period;

    public override ulong period {
        get { return _period; }
    }

    public struct Reference {
        public Pattern patt;
        public short count;

        public ulong totalPeriod {
            get {
                return patt.period * (ulong)count;
            }
        }
    }

    public Reference[] definition;

    public SingleFlashRefPattern(NbtCompound cmpd) {
        id = (ushort)cmpd["id"].ShortValue;
        name = cmpd["name"].StringValue;
        _period = t0 = t1 = t2 = t3 = 0;

        NbtList refs = cmpd.Get<NbtList>("ref");
        definition = new Reference[refs.Count];
        for(int i = 0; i < refs.Count; i++) {
            NbtCompound alpha = refs.Get<NbtCompound>(i);
            definition[i] = new Reference();
            definition[i].count = alpha["cnt"].ShortValue;
            short pattID = alpha["id"].ShortValue;

            foreach(Pattern p in LightDict.inst.flashPatts) {
                if(p.id == pattID) {
                    definition[i].patt = p;
                    break;
                }
            }

            _period += definition[i].totalPeriod;
        }
    }

    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        tick %= period;

        foreach(Reference r in definition) {
            if(tick < r.totalPeriod) {
                return r.patt.GetIsActive(tick, phaseB, color2, bit);
            } else {
                tick -= r.totalPeriod;
            }
        }

        return false;
    }
}

public class DoubleFlashRefPattern : Pattern {
    public ulong _period;
    public override ulong period {
        get { return _period; }
    }

    public struct Reference {
        public Pattern patt;
        public bool forColor2;
        public short count;

        public ulong totalPeriod {
            get {
                return patt.period * (ulong)count;
            }
        }
    }

    public Reference[] definition;

    public DoubleFlashRefPattern(NbtCompound cmpd) {
        id = (ushort)cmpd["id"].ShortValue;
        name = cmpd["name"].StringValue;
        _period = t0 = t1 = t2 = t3 = 0;

        NbtList refs = cmpd.Get<NbtList>("ref");
        definition = new Reference[refs.Count];
        for(int i = 0; i < refs.Count; i++) {
            NbtCompound alpha = refs.Get<NbtCompound>(i);
            definition[i] = new Reference();
            definition[i].count = alpha["cnt"].ShortValue;
            definition[i].forColor2 = alpha["clr"].ByteValue == 1;
            short pattID = alpha["id"].ShortValue;

            foreach(Pattern p in LightDict.inst.flashPatts) {
                if(p.id == pattID) {
                    definition[i].patt = p;
                    break;
                }
            }

            _period += definition[i].totalPeriod;
        }
    }

    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        tick %= period;

        foreach(Reference r in definition) {
            if(tick < r.totalPeriod) {
                return (!(color2 ^ r.forColor2)) && r.patt.GetIsActive(tick, phaseB, color2, bit);
            } else {
                tick -= r.totalPeriod;
            }
        }

        return false;
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

public class Lens {
    public static string smPrefix, lgPrefix;
    public string name;
    public string partSuffix;
    public uint cost;
    public Color color;

    public Lens() {
        name = partSuffix = "";
        color = Color.white;
    }

    public Lens(NbtCompound cmpd) {
        name = cmpd["name"].StringValue;
        partSuffix = cmpd["part"].StringValue;
        byte[] clr = cmpd["clr"].ByteArrayValue;
        color = new Color32(clr[0], clr[1], clr[2], clr[3]);
    }
}

public enum Location {
    FRONT, FRONT_CORNER, ALLEY, REAR_CORNER, REAR, FAR_REAR
}

public enum BasicFunction {
    NULL = 0x0, FLASHING = 0x1, STEADY = 0x2, EMITTER = 0x4, CAL_STEADY = 0x8, CRUISE = 0x10, STT = 0x20, TRAFFIC = 0x40, BLOCK_OFF = 0x80
}

public enum AdvFunction {
    NONE = 0x0,
    TAKEDOWN = 0x1, LEVEL1 = 0x2, LEVEL2 = 0x4, LEVEL3 = 0x8,
    TRAFFIC_LEFT = 0x10, TRAFFIC_RIGHT = 0x20, ALLEY_LEFT = 0x40, ALLEY_RIGHT = 0x80,
    ICL = 0x100, DIM = 0x200, FTAKEDOWN = 0x400, FALLEY = 0x800,
    PATTERN = 0x1000, CRUISE = 0x2000, TURN_LEFT = 0x4000, TURN_RIGHT = 0x8000,
    TAIL = 0x10000, T13 = 0x20000, LEVEL4 = 0x40000, LEVEL5 = 0x80000,
    EMITTER = 0x100000
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