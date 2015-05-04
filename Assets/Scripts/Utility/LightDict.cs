using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

using fNbt;

public class LightDict : MonoBehaviour {
    public static LightDict inst;
    public Dictionary<Location, LocationNode> lights;
    public List<Function> steadyBurn;
    public List<Pattern> flashPatts, warnPatts, tdPatts;

    public Dictionary<Location, List<Function>> capableFunctions;

    void Awake() {
        if(inst == null) inst = this;

        lights = new Dictionary<Location, LocationNode>();
        steadyBurn = new List<Function>(new Function[] { Function.TAKEDOWN, Function.ALLEY, Function.STT_AND_TAIL, Function.T13, Function.EMITTER });

        if(File.Exists("lib.nbt")) {
            foreach(Location l in new Location[] { Location.FRONT, Location.FRONT_CORNER, Location.ALLEY, Location.REAR_CORNER, Location.FAR_REAR, Location.REAR }) {
                lights[l] = new LocationNode();
            }

            try {
                NbtFile cat = new NbtFile("lib.nbt");

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
                    NbtCompound alphaCmpd = (NbtCompound)alpha;

                    Pattern alphaPat = new Pattern() { id = (ushort)alphaCmpd["id"].ShortValue, name = alphaCmpd["name"].StringValue };
                    warnPatts.Add(alphaPat);
                }

                flashPatts = new List<Pattern>();
                patlist = pattstag.Get<NbtList>("sflsh");
                foreach(NbtTag alpha in patlist) {
                    NbtCompound alphaCmpd = (NbtCompound)alpha;

                    Pattern alphaPat = new Pattern() { id = (ushort)alphaCmpd["id"].ShortValue, name = alphaCmpd["name"].StringValue };
                    flashPatts.Add(alphaPat);
                }

                
                tdPatts = new List<Pattern>();
                patlist = pattstag.Get<NbtList>("traff");
                foreach(NbtTag alpha in patlist) {
                    NbtCompound alphaCmpd = (NbtCompound)alpha;

                    Pattern alphaPat = new Pattern() { id = (ushort)alphaCmpd["id"].ShortValue, name = alphaCmpd["name"].StringValue };
                    tdPatts.Add(alphaPat);
                }

                // todo: load patterns



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

        capableFunctions = new Dictionary<Location, List<Function>>();

        capableFunctions[Location.FRONT] = new List<Function>(new Function[] { Function.TAKEDOWN, Function.T13, Function.DIM, Function.EMITTER });
        capableFunctions[Location.FRONT_CORNER] = new List<Function>(new Function[] { Function.TAKEDOWN, Function.ICL, Function.DIM, Function.CRUISE });
        capableFunctions[Location.REAR_CORNER] = new List<Function>(new Function[] { Function.TAKEDOWN, Function.DIM, Function.CRUISE });
        capableFunctions[Location.ALLEY] = new List<Function>(new Function[] { Function.ALLEY, Function.DIM });
        capableFunctions[Location.REAR] = new List<Function>(new Function[] { Function.TAKEDOWN, Function.TRAFFIC, Function.DIM });
        capableFunctions[Location.FAR_REAR] = new List<Function>(new Function[] { Function.STT_AND_TAIL, Function.DIM });

        foreach(Location l in new Location[] { Location.FRONT, Location.FRONT_CORNER, Location.ALLEY, Location.REAR_CORNER, Location.FAR_REAR, Location.REAR }) {
            capableFunctions[l].AddRange(new Function[] { Function.LEVEL1, Function.LEVEL2, Function.LEVEL3, Function.LEVEL4, Function.LEVEL5 });
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
        return lights[loc].optics[optic];
    }
}

[System.Serializable]
public class Pattern {
    public string name;

    public ushort id;
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

    public OpticNode() {
        styles = new Dictionary<string, StyleNode>();
        name = "";
        partNumber = "";
        smEquivalent = "";
        lgEquivalent = "";
        fitsLg = false;
        fitsSm = false;
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

public enum Function {
    NONE = 0x0,
    LEVEL1 = 0x1, LEVEL2 = 0x2, LEVEL3 = 0x4, LEVEL4 = 0x8, LEVEL5 = 0x10,
    TAKEDOWN = 0x20, ICL = 0x40, ALLEY = 0x80, T13 = 0x100, CRUISE = 0x200,
    TRAFFIC = 0x400, DIM = 0x800, STT_AND_TAIL = 0x1000, EMITTER = 0x2000
}