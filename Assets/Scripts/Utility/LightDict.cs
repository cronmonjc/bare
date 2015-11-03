using System;
using System.Collections.Generic;
using System.IO;
using fNbt;
using UnityEngine;

/// <summary>
/// Class that manages the dictionary of options for lenses, optics, styles, patterns... everything found in the library file.
/// </summary>
public class LightDict : MonoBehaviour {
    /// <summary>
    /// A static reference to the one and only instance of this class.
    /// </summary>
    public static LightDict inst;
    /// <summary>
    /// A static reference to the one and only instance of the Steady Burn Pattern.
    /// </summary>
    public static SteadyPattern stdy;
    /// <summary>
    /// A Dictionary containing all of the different optic and style options, organized by location.
    /// </summary>
    public Dictionary<Location, LocationNode> lights;
    /// <summary>
    /// A List of all the lens options available.
    /// </summary>
    public List<Lens> lenses;
    /// <summary>
    /// A List of what functions are considered "steady burn" functions.  Set via the Unity Inspector.
    /// </summary>
    public List<AdvFunction> steadyBurn;
    /// <summary>
    /// A static array of the functions that can flash.
    /// </summary>
    public static List<AdvFunction> flashingFuncs = new List<AdvFunction>(new AdvFunction[] { AdvFunction.PRIO1, AdvFunction.PRIO2, AdvFunction.PRIO3, AdvFunction.PRIO4, AdvFunction.PRIO5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL });
    /// <summary>
    /// A List of Patterns for flashing heads.
    /// </summary>
    public List<Pattern> flashPatts;
    /// <summary>
    /// A List of Patterns for traffic director heads.
    /// </summary>
    public List<Pattern> tdPatts;
    /// <summary>
    /// How long a single pattern tick is in units of 10ms.
    /// </summary>
    [System.NonSerialized]
    public short pattBase = 0;
    /// <summary>
    /// A reference to a BOMCables object, to initialize it at load time.  Set by the Unity Inspector.
    /// </summary>
    public BOMCables BomCableRef;
    /// <summary>
    /// The static price for mounting brackets.
    /// </summary>
    [System.NonSerialized]
    public uint bracketPrice = 0;
    /// <summary>
    /// An array of options for cable lengths.
    /// </summary>
    [System.NonSerialized]
    public CableLengthOption[] cableLengths;
    /// <summary>
    /// An array of options for mounting kits.
    /// </summary>
    public MountingKitOption[] mountKits;

    /// <summary>
    /// Awake is called once, immediately as the object is created (typically at load time)
    /// </summary>
    void Awake() {
        if(inst == null) inst = this;  // Make sure everything can reference this object.

        lights = new Dictionary<Location, LocationNode>(); // Instantiate all of the data containers
        this.lenses = new List<Lens>();
        stdy = new SteadyPattern();

        if(File.Exists(BarManager.DirRoot + "lib.nbt")) { // Look for the library file
            foreach(Location l in new Location[] { Location.FRONT, Location.FRONT_CORNER, Location.ALLEY, Location.REAR_CORNER, Location.FAR_REAR, Location.REAR }) {
                lights[l] = new LocationNode(); // More container instantiation
            }

            try {
                NbtFile cat = new NbtFile(BarManager.DirRoot + "lib.nbt"); // Open the library file

                if(!cat.RootTag.Contains("pric")) { // If the library file doesn't contain a "pric" tag... (type doesn't matter, but NbtTagType.Byte takes up the least space)
                    Destroy(FindObjectOfType<DispPricing>().gameObject);  // ...destroy the DispPricing button, preventing the user from seeing or using it to toggle pricing on and off.
                }
                if(cat.RootTag.Contains("canPub")) { // If the library file contains a "canPub" tag... (should be a NbtTagType.Byte tag)
                    BarManager.canPub = cat.RootTag["canPub"].ByteValue;  // ...get the value of it.
                }

                NbtList heads = (NbtList)(cat.RootTag["heads"]);  // Get the list of potential head options.

                foreach(NbtTag head in heads) {
                    OpticNode optNode = new OpticNode((NbtCompound)head);  // Generate the OpticNode from each tag in the list

                    NbtList locs = (NbtList)head["locs"];  // Figure out where the optic can go and add a reference to it
                    foreach(NbtTag loc in locs) {
                        string strLoc = loc.StringValue;  // Each tag in the list should be of type NbtTagType.String

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

                NbtCompound pattstag = cat.RootTag.Get<NbtCompound>("patts");  // Get the pattern reference from the library

                pattBase = pattstag["base"].ShortValue;  // Get the pattern base value

                flashPatts = new List<Pattern>();  // Instantiate flashing pattern container
                NbtList patlist = pattstag.Get<NbtList>("sflsh");  // Get all of the single-color flashing patterns
                foreach(NbtTag alpha in patlist) {
                    if(((NbtCompound)alpha).Contains("ref")) { // This pattern references other patterns.
                        flashPatts.Add(new SingleFlashRefPattern((NbtCompound)alpha));
                    } else { // This pattern stands alone.
                        flashPatts.Add(new FlashPatt((NbtCompound)alpha));
                    }
                }
                patlist = pattstag.Get<NbtList>("dcflash");  // All of the dual-color flashing patterns
                foreach(NbtTag alpha in patlist) {
                    flashPatts.Add(new DoubleFlashRefPattern((NbtCompound)alpha));
                }
                patlist = pattstag.Get<NbtList>("flash");  // All of the polyhead flashing patterns
                foreach(NbtTag alpha in patlist) {
                    flashPatts.Add(new WarnPatt((NbtCompound)alpha));
                }
                flashPatts.Add(new DCCirclePattern());  // Hard-coded Dual-Color Circle Pattern
                flashPatts.Add(new DCDoubleRotatorPattern());  // Hard-coded Dual-Color Double Rotator Pattern


                tdPatts = new List<Pattern>(); // Instantiate traffic director pattern container
                patlist = pattstag.Get<NbtList>("traff"); // Get all of the traffic director patterns
                foreach(NbtTag alpha in patlist) {
                    tdPatts.Add(new TraffPatt((NbtCompound)alpha));
                }


                NbtCompound lensCmpd = cat.RootTag.Get<NbtCompound>("lenses"); // Get the lens definition package
                Lens.lgPrefix = lensCmpd["lgFix"].StringValue; // Get the prefixes for both size of lens.
                Lens.smPrefix = lensCmpd["smFix"].StringValue;
                NbtList lenses = lensCmpd.Get<NbtList>("opts"); // Get the different options out
                foreach(NbtTag alpha in lenses) {
                    this.lenses.Add(new Lens(alpha as NbtCompound));
                }


                NbtCompound optsCmpd = cat.RootTag.Get<NbtCompound>("opts"); // Get the option definition package
                BomCableRef.Initialize(optsCmpd.Get<NbtCompound>("cables")); // Have the BomCableRef object parse the "cables" NbtTagType.Compound tag.

                FindObjectOfType<BarManager>().Initialize(optsCmpd.Get<NbtCompound>("base")); // Have the BarManager extract model numbers and prices from the "base" NbtTagType.Compound tag.

                bracketPrice = (uint)optsCmpd["bracket"].IntValue; // Get the bracket price.

                NbtList lenOpts = optsCmpd.Get<NbtList>("cableLength"); // Extract all of the cable length options...
                cableLengths = new CableLengthOption[lenOpts.Count];

                for(int i = 0; i < cableLengths.Length; i++) {
                    NbtCompound opt = lenOpts[i] as NbtCompound;
                    cableLengths[i] = new CableLengthOption() { // ...extract all of the information from each option, then save it in the cableLengths array.
                        length = opt["len"].ByteValue,
                        canPrice = (uint)opt["can"].IntValue,
                        hardPrice = (uint)opt["hard"].IntValue,
                        pwrPrice = (uint)opt["pwr"].IntValue
                    };
                }

                NbtList kitOpts = optsCmpd.Get<NbtList>("mountingKits"); // Extract all of the mounting kit options...
                mountKits = new MountingKitOption[kitOpts.Count];

                for(int i = 0; i < mountKits.Length; i++) {
                    NbtCompound opt = kitOpts[i] as NbtCompound;
                    mountKits[opt["which"].ByteValue] = new MountingKitOption() { // ...extract all of the information from each option, then save it in the mountKits array.
                        price = (uint)opt["cost"].IntValue,
                        part = opt["part"].StringValue,
                        name = opt["name"].StringValue
                    };
                }


            } catch(NbtFormatException ex) { // Thrown if the file doesn't make sense to the NBT Parser.
                ErrorText.inst.DispError("Could not parse the file.  Are you certain you got this file from Star?");
                Debug.LogException(ex);
            } catch(EndOfStreamException ex) { // Thrown if the file ends before the Parser expected it to end.
                ErrorText.inst.DispError("Could not parse the file.  Are you certain you got this file from Star?");
                Debug.LogException(ex);
            } catch(InvalidCastException ex) { // Thrown if tag types don't match up to expected.
                ErrorText.inst.DispError("Could not parse the file.  Are you certain you got this file from Star?");
                Debug.LogException(ex);
            } catch(NullReferenceException ex) { // Thrown if a tag doesn't exist
                ErrorText.inst.DispError("Could not parse the file.  Are you certain you got this file from Star?");
                Debug.LogException(ex);
            }
        } else { // The library file's MIA.
            ErrorText.inst.DispError("You seem to be missing the 'lib.nbt' file.  Make sure it's in the same directory as the executable.");
        }
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// EDITOR ONLY.  Tells Unity how to render this GameObject in the Scene View.
    /// </summary>
    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "Dict.png", true);
    }
#endif

    /// <summary>
    /// Function used to grab a LocationNode from the dictionary.
    /// </summary>
    /// <param name="locs">One or more locations to reference.</param>
    /// <returns>A LocationNode that has all of the different options to pick from.</returns>
    public LocationNode FetchLocation(params Location[] locs) {
        if(locs.Length == 0) { // No locations?  Exception for you!
            throw new ArgumentException();
        }
        if(locs.Length == 1) {
            return lights[locs[0]]; // Only one Location selected, give the node wholesale.
        }

        LocationNode rtn = lights[locs[0]].Clone(); // Clone the first Location's LocationNode.
        List<Location> loclist = new List<Location>(locs); // Make a mutable List of the requested Locations.
        loclist.RemoveAt(0); // We've already done the first, so remove that.
        foreach(Location l in loclist) {
            LocationNode dln = lights[l]; // Get the subsequent Location's LocationNode for reference.
            foreach(OpticNode rtnon in new List<OpticNode>(rtn.optics.Values)) { // For each OpticNode in the LocationNode to return...
                if(dln.optics.ContainsKey(rtnon.name)) { // If this LocationNode has a matching OpticNode...
                    foreach(StyleNode rtnsn in new List<StyleNode>(rtnon.styles.Values)) { // Go through each StyleNode and remove any that don't match from the LocationNode to return.
                        if(!(dln.optics[rtnon.name].styles.ContainsKey(rtnsn.name) && dln.optics[rtnon.name].styles[rtnsn.name] == rtnsn)) {
                            rtnon.Remove(rtnsn);
                        } else {
                            rtnsn.selectable &= dln.optics[rtnon.name].styles[rtnsn.name].selectable;
                        }
                    }
                } else if(dln.optics.ContainsKey(rtnon.smEquivalent)) {  // Do the same if this LocationNode has the small equivalent
                    foreach(StyleNode rtnsn in new List<StyleNode>(rtnon.styles.Values)) {
                        if(!(dln.optics[rtnon.smEquivalent].styles.ContainsKey(rtnsn.name) && dln.optics[rtnon.smEquivalent].styles[rtnsn.name] == rtnsn)) {
                            rtnon.Remove(rtnsn);
                        } else {
                            rtnsn.selectable &= dln.optics[rtnon.smEquivalent].styles[rtnsn.name].selectable;
                        }
                    }
                } else if(dln.optics.ContainsKey(rtnon.lgEquivalent)) {  // Do the same if this LocationNode has the large equivalent
                    foreach(StyleNode rtnsn in new List<StyleNode>(rtnon.styles.Values)) {
                        if(!(dln.optics[rtnon.lgEquivalent].styles.ContainsKey(rtnsn.name) && dln.optics[rtnon.lgEquivalent].styles[rtnsn.name] == rtnsn)) {
                            rtnon.Remove(rtnsn);
                        } else {
                            rtnsn.selectable &= dln.optics[rtnon.lgEquivalent].styles[rtnsn.name].selectable;
                        }
                    }
                } else {
                    rtn.Remove(rtnon); // No matches found, so remove it from the LocationNode to return.
                }
            }
        }

        return rtn;  // Return the LocationNode
    }

    /// <summary>
    /// Fetches an OpticNode given its name and a matching Location.
    /// </summary>
    /// <param name="loc">Which Location is the node being fetched for?</param>
    /// <param name="optic">Name of the optic to fetch</param>
    /// <returns>An OpticNode if one was found, otherwise null</returns>
    public OpticNode FetchOptic(Location loc, string optic) {
        if(lights[loc].optics.ContainsKey(optic))
            return lights[loc].optics[optic];
        else
            return null;
    }
}

#region Patterns

/// <summary>
/// Abstract class, representing one of a number of patterns for Flashing or Traffic Director heads
/// </summary>
public abstract class Pattern {
    /// <summary>
    /// Name of the pattern
    /// </summary>
    public string name;
    /// <summary>
    /// ID of the pattern
    /// </summary>
    public ushort id;

    /// <summary>
    /// Pattern's t0 keyframe time length
    /// </summary>
    public ushort t0;
    /// <summary>
    /// Pattern's t1 keyframe time length
    /// </summary>
    public ushort t1;
    /// <summary>
    /// Pattern's t2 keyframe time length
    /// </summary>
    public ushort t2;
    /// <summary>
    /// Pattern's t3 keyframe time length
    /// </summary>
    public ushort t3;
    /// <summary>
    /// How long does the pattern run for, in ticks?
    /// </summary>
    public abstract ulong period { get; }
    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables
    /// </summary>
    /// <param name="tick">Which tick is it currently?</param>
    /// <param name="phaseB">Is the head on Phase B?</param>
    /// <param name="color2">Are we checking color 2 of the head?</param>
    /// <param name="bit">Which bit is the head on?</param>
    /// <returns>True if the head should be active, false if it should not</returns>
    public abstract bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit);
}

/// <summary>
/// A Pattern that affects multiple heads
/// </summary>
public class WarnPatt : Pattern {
    /// <summary>
    /// Information this pattern's holding on what flashes when
    /// </summary>
    public short[] definition;
    /// <summary>
    /// Personal variable so it doesn't have to recalculate the period every time it's asked
    /// </summary>
    private ulong _period = 0;
    /// <summary>
    /// How long does the pattern run for, in ticks?
    /// </summary>
    public override ulong period {
        get { return _period; }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cmpd">The NbtCompound we're extracting pattern information from</param>
    public WarnPatt(NbtCompound cmpd) {
        name = cmpd["name"].StringValue; // Get the basics.
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
            definition[i] = (short)(vals[i] & 0xFFFF); // Copy down all of the pattern information
            switch((vals[i] >> 14) & 0x3) { // Calculate the period now so we don't have to calculate it every time we're asked for it
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

    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables
    /// </summary>
    /// <param name="tick">Which tick is it currently?</param>
    /// <param name="phaseB">Is the head on Phase B?</param>
    /// <param name="color2">Are we checking color 2 of the head?</param>
    /// <param name="bit">Which bit is the head on?</param>
    /// <returns>True if the head should be active, false if it should not</returns>
    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        tick %= period; // Reduce the tick down so it's somewhere in the pattern definition.
        foreach(short b in definition) {
            switch(0x3 & (b >> 14)) { // Go through each short, look for when this tick should occur, then test it for when it should be lit.
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

/// <summary>
/// A Pattern that affects a single head
/// </summary>
public class FlashPatt : Pattern {
    /// <summary>
    /// Information this pattern's holding on what flashes when
    /// </summary>
    public byte[] definition;
    /// <summary>
    /// Personal variable so it doesn't have to recalculate the period every time it's asked
    /// </summary>
    private ulong _period = 0;
    /// <summary>
    /// How long does the pattern run for, in ticks?
    /// </summary>
    public override ulong period {
        get { return _period; }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cmpd">The NbtCompound we're extracting pattern information from</param>
    public FlashPatt(NbtCompound cmpd) {
        name = cmpd["name"].StringValue; // Get the basics.
        id = (ushort)cmpd["id"].ShortValue;
        t0 = (ushort)cmpd["t0"].ShortValue;
        t1 = (ushort)cmpd["t1"].ShortValue;
        t2 = (ushort)cmpd["t2"].ShortValue;
        t3 = (ushort)cmpd["t3"].ShortValue;

        NbtByteArray patttag = cmpd.Get<NbtByteArray>("patt");
        definition = patttag.Value; // Copy down all of the pattern information

        _period = 0uL;

        for(int i = 0; i < definition.Length; i++) {
            switch((definition[i] >> 2) & 0x3) { // Calculate the period now so we don't have to calculate it every time we're asked for it
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

    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables
    /// </summary>
    /// <param name="tick">Which tick is it currently?</param>
    /// <param name="phaseB">Is the head on Phase B?</param>
    /// <param name="color2">Are we checking color 2 of the head?</param>
    /// <param name="bit">Which bit is the head on?</param>
    /// <returns>True if the head should be active, false if it should not</returns>
    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        tick %= period; // Reduce the tick down so it's somewhere in the pattern definition.

        foreach(byte b in definition) {
            switch(0x3 & (b >> 2)) { // Go through each byte, look for when this tick should occur, then test it for when it should be lit.
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

/// <summary>
/// A Pattern that affects traffic director heads
/// </summary>
public class TraffPatt : Pattern {
    /// <summary>
    /// Display the leftward directing if true, rightward if false
    /// </summary>
    public static bool directLeft = false;
    /// <summary>
    /// Display the six-head version if true, the eight-head version if false
    /// </summary>
    public static bool sixHeads = false;

    /// <summary>
    /// Information this pattern's holding on what flashes when
    /// </summary>
    public short[] left6, right6, center6, left8, right8, center8;
    /// <summary>
    /// Personal variable so it doesn't have to recalculate the period every time it's asked
    /// </summary>
    public ulong period6, period8;
    /// <summary>
    /// How long does the pattern run for, in ticks?
    /// </summary>
    public override ulong period {
        get { return (sixHeads ? period6 : period8); }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cmpd">The NbtCompound we're extracting pattern information from</param>
    public TraffPatt(NbtCompound cmpd) {
        name = cmpd["name"].StringValue; // Get the basics.
        id = (ushort)cmpd["id"].ShortValue;
        t0 = (ushort)cmpd["t0"].ShortValue;
        t1 = (ushort)cmpd["t1"].ShortValue;
        t2 = (ushort)cmpd["t2"].ShortValue;
        t3 = (ushort)cmpd["t3"].ShortValue;

        // Copy down all of the pattern information for each of the six definitions

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

        // End information copy

        period6 = period8 = 0;
        
        // Calculate the periods now so we don't have to calculate it every time we're asked for it
        foreach(short alpha in left6) {
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
        foreach(short alpha in left8) {
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

    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables
    /// </summary>
    /// <param name="tick">Which tick is it currently?</param>
    /// <param name="phaseB">Is the head on Phase B?</param>
    /// <param name="color2">Are we checking color 2 of the head?</param>
    /// <param name="bit">Which bit is the head on?</param>
    /// <returns>True if the head should be active, false if it should not</returns>
    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {

        if(sixHeads) {
            tick %= period6; // Reduce the tick down so it's somewhere in the pattern definition.
            if(directLeft) {
                foreach(short b in left6) {
                    switch(0x3 & (b >> 14)) { // Go through each short, look for when this tick should occur, then test it for when it should be lit.
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
                    switch(0x3 & (b >> 14)) { // Go through each short, look for when this tick should occur, then test it for when it should be lit.
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
            tick %= period8; // Reduce the tick down so it's somewhere in the pattern definition.
            if(directLeft) {
                foreach(short b in left8) {
                    switch(0x3 & (b >> 14)) { // Go through each short, look for when this tick should occur, then test it for when it should be lit.
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
                    switch(0x3 & (b >> 14)) { // Go through each short, look for when this tick should occur, then test it for when it should be lit.
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

/// <summary>
/// A simple Pattern representing a steady burn head.
/// </summary>
public class SteadyPattern : Pattern {
    /// <summary>
    /// Static period
    /// </summary>
    public override ulong period {
        get {
            return 1;
        }
    }

    /// <summary>
    /// Constructor.  Gives itself a name.
    /// </summary>
    public SteadyPattern() {
        name = "Steady Burn";
    }

    /// <summary>
    /// Always returns true.  Because steady burn.
    /// </summary>
    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        return true;
    }
}

/// <summary>
/// A Pattern that affects a single head, using other patterns for reference
/// </summary>
public class SingleFlashRefPattern : Pattern {
    /// <summary>
    /// Personal variable so it doesn't have to recalculate the period every time it's asked
    /// </summary>
    public ulong _period;

    /// <summary>
    /// How long does the pattern run for, in ticks?
    /// </summary>
    public override ulong period {
        get { return _period; }
    }

    /// <summary>
    /// Information about the references it needs to hold
    /// </summary>
    public struct Reference {
        /// <summary>
        /// Which pattern is it referencing?
        /// </summary>
        public Pattern patt;
        /// <summary>
        /// How many times do we cycle through this pattern?
        /// </summary>
        public short count;

        /// <summary>
        /// Total period for this Reference
        /// </summary>
        public ulong totalPeriod {
            get {
                return patt.period * (ulong)count;
            }
        }
    }

    /// <summary>
    /// Information this pattern's holding on what references its using
    /// </summary>
    public Reference[] definition;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cmpd">The NbtCompound we're extracting pattern information from</param>
    public SingleFlashRefPattern(NbtCompound cmpd) {
        id = (ushort)cmpd["id"].ShortValue; // Get the basics.
        name = cmpd["name"].StringValue;
        _period = t0 = t1 = t2 = t3 = 0; // t0 thru t3 should be 0, because we aren't using those in this Pattern.  Initialize _pattern.

        NbtList refs = cmpd.Get<NbtList>("ref"); // Examine references
        definition = new Reference[refs.Count];
        for(int i = 0; i < refs.Count; i++) {
            NbtCompound alpha = refs.Get<NbtCompound>(i);
            definition[i] = new Reference();
            definition[i].count = alpha["cnt"].ShortValue;
            short pattID = alpha["id"].ShortValue;

            foreach(Pattern p in LightDict.inst.flashPatts) { // Look for the pattern this Reference should be referencing
                if(p.id == pattID) {
                    definition[i].patt = p; // Found it.
                    break;
                }
            }

            _period += definition[i].totalPeriod; // Add the period
        }
    }

    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables
    /// </summary>
    /// <param name="tick">Which tick is it currently?</param>
    /// <param name="phaseB">Is the head on Phase B?</param>
    /// <param name="color2">Are we checking color 2 of the head?</param>
    /// <param name="bit">Which bit is the head on?</param>
    /// <returns>True if the head should be active, false if it should not</returns>
    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        tick %= period; // Reduce the tick down so it's somewhere in the pattern definition.

        foreach(Reference r in definition) {
            if(tick < r.totalPeriod) { // If the tick resides somewhere in this Reference...
                return r.patt.GetIsActive(tick, phaseB, color2, bit); // Have the Pattern figure out the rest.
            } else {
                tick -= r.totalPeriod;
            }
        }

        return false;
    }
}

/// <summary>
/// A Pattern that affects both colors of a single head, using other patterns for reference
/// </summary>
public class DoubleFlashRefPattern : Pattern {
    /// <summary>
    /// Personal variable so it doesn't have to recalculate the period every time it's asked
    /// </summary>
    public ulong _period;
    
    /// <summary>
    /// How long does the pattern run for, in ticks?
    /// </summary>
    public override ulong period {
        get { return _period; }
    }

    /// <summary>
    /// Information about the references it needs to hold
    /// </summary>
    public struct Reference {
        /// <summary>
        /// Which pattern is it referencing?
        /// </summary>
        public Pattern patt;
        /// <summary>
        /// Is this reference influencing Color 2?
        /// </summary>
        public bool forColor2;
        /// <summary>
        /// How many times do we cycle through this pattern?
        /// </summary>
        public short count;

        /// <summary>
        /// Total period for this Reference
        /// </summary>
        public ulong totalPeriod {
            get {
                return patt.period * (ulong)count;
            }
        }
    }

    /// <summary>
    /// Information this pattern's holding on what references its using
    /// </summary>
    public Reference[] definition;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cmpd">The NbtCompound we're extracting pattern information from</param>
    public DoubleFlashRefPattern(NbtCompound cmpd) {
        id = (ushort)cmpd["id"].ShortValue; // Get the basics.
        name = cmpd["name"].StringValue;
        _period = t0 = t1 = t2 = t3 = 0; // t0 thru t3 should be 0, because we aren't using those in this Pattern.  Initialize _pattern.

        NbtList refs = cmpd.Get<NbtList>("ref"); // Examine references
        definition = new Reference[refs.Count];
        for(int i = 0; i < refs.Count; i++) {
            NbtCompound alpha = refs.Get<NbtCompound>(i);
            definition[i] = new Reference();
            definition[i].count = alpha["cnt"].ShortValue;
            definition[i].forColor2 = alpha["clr"].ByteValue == 1;
            short pattID = alpha["id"].ShortValue;

            foreach(Pattern p in LightDict.inst.flashPatts) { // Look for the pattern this Reference should be referencing
                if(p.id == pattID) {
                    definition[i].patt = p; // Found it.
                    break;
                }
            }

            _period += definition[i].totalPeriod; // Add the period
        }
    }

    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables
    /// </summary>
    /// <param name="tick">Which tick is it currently?</param>
    /// <param name="phaseB">Is the head on Phase B?</param>
    /// <param name="color2">Are we checking color 2 of the head?</param>
    /// <param name="bit">Which bit is the head on?</param>
    /// <returns>True if the head should be active, false if it should not</returns>
    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        tick %= period; // Reduce the tick down so it's somewhere in the pattern definition.

        foreach(Reference r in definition) {
            if(tick < r.totalPeriod) { // If the tick resides somewhere in this Reference...
                return (!(color2 ^ r.forColor2)) && r.patt.GetIsActive(tick, phaseB, color2, bit); // Check first if we're checking the right color.  If we are, then have the Pattern figure out the rest.
            } else {
                tick -= r.totalPeriod;
            }
        }

        return false;
    }
}

/// <summary>
/// A statically-defined Pattern that will have two lights circling the bar, one on each color.
/// </summary>
public class DCCirclePattern : Pattern {
    /// <summary>
    /// Constructor.
    /// </summary>
    public DCCirclePattern() {
        name = "DC Circle";
        id = 36;					// was 34 changed JJC 11-3-15
        t0 = 5;
        t1 = t2 = t3 = 0;
    }

    /// <summary>
    /// How long does the pattern run for, in ticks?
    /// </summary>
    public override ulong period {
        get { return 120; }
    }

    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables.  Not implemented, requires additional care
    /// </summary>
    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables
    /// </summary>
    /// <param name="tick">Which tick is it currently?</param>
    /// <param name="phaseB">Is the head on Phase B?</param>
    /// <param name="color2">Are we checking color 2 of the head?</param>
    /// <param name="bit">Which bit is the head on?</param>
    /// <param name="isRear">Is the head in the rear?</param>
    /// <returns>True if the head should be active, false if it should not</returns>
    public bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit, bool isRear) {
        byte frame = (byte)((tick % 120) / 5);  // Figure out which frame we're on
        if(isRear) {
            switch(frame) { // Rear frames
                case 0:
                    return ((color2 ? 0x0C00 : 0x0003) & (0x1 << bit)) > 0;
                case 1:
                    return ((color2 ? 0x0800 : 0x0001) & (0x1 << bit)) > 0;
                case 12:
                    return ((color2 ? 0x0001 : 0x0800) & (0x1 << bit)) > 0;
                case 13:
                    return ((color2 ? 0x0003 : 0x0C00) & (0x1 << bit)) > 0;
                case 14:
                    return ((color2 ? 0x0007 : 0x0E00) & (0x1 << bit)) > 0;
                case 15:
                    return ((color2 ? 0x000E : 0x0700) & (0x1 << bit)) > 0;
                case 16:
                    return ((color2 ? 0x001C : 0x0380) & (0x1 << bit)) > 0;
                case 17:
                    return ((color2 ? 0x0038 : 0x01C0) & (0x1 << bit)) > 0;
                case 18:
                    return ((color2 ? 0x0070 : 0x00E0) & (0x1 << bit)) > 0;
                case 19:
                    return ((color2 ? 0x00E0 : 0x0070) & (0x1 << bit)) > 0;
                case 20:
                    return ((color2 ? 0x01C0 : 0x0038) & (0x1 << bit)) > 0;
                case 21:
                    return ((color2 ? 0x0380 : 0x001C) & (0x1 << bit)) > 0;
                case 22:
                    return ((color2 ? 0x0700 : 0x000E) & (0x1 << bit)) > 0;
                case 23:
                    return ((color2 ? 0x0E00 : 0x0007) & (0x1 << bit)) > 0;
                default:
                    return false;
            }
        } else {
            switch(frame) { // Front frames
                case 0:
                    return ((color2 ? 0x2000 : 0x1000) & (0x1 << bit)) > 0;
                case 1:
                    return ((color2 ? 0x2800 : 0x1001) & (0x1 << bit)) > 0;
                case 2:
                    return ((color2 ? 0x2C00 : 0x1003) & (0x1 << bit)) > 0;
                case 3:
                    return ((color2 ? 0x0700 : 0x000E) & (0x1 << bit)) > 0;
                case 4:
                    return ((color2 ? 0x0380 : 0x001C) & (0x1 << bit)) > 0;
                case 5:
                    return ((color2 ? 0x01C0 : 0x0038) & (0x1 << bit)) > 0;
                case 6:
                    return ((color2 ? 0x00E0 : 0x0070) & (0x1 << bit)) > 0;
                case 7:
                    return ((color2 ? 0x0070 : 0x00E0) & (0x1 << bit)) > 0;
                case 8:
                    return ((color2 ? 0x0038 : 0x01C0) & (0x1 << bit)) > 0;
                case 9:
                    return ((color2 ? 0x001C : 0x0380) & (0x1 << bit)) > 0;
                case 10:
                    return ((color2 ? 0x000E : 0x0700) & (0x1 << bit)) > 0;
                case 11:
                    return ((color2 ? 0x1003 : 0x2C00) & (0x1 << bit)) > 0;
                case 12:
                    return ((color2 ? 0x1001 : 0x2800) & (0x1 << bit)) > 0;
                case 13:
                    return ((color2 ? 0x1000 : 0x2000) & (0x1 << bit)) > 0;
                default:
                    return false;
            }
        }
    }
}

/// <summary>
/// A statically-defined Pattern that will have four lights circling the bar, one on each color on each half.
/// </summary>
public class DCDoubleRotatorPattern : Pattern {
    /// <summary>
    /// Constructor.
    /// </summary>
    public DCDoubleRotatorPattern() {
        name = "DC Double Rotator";
        id = 37;					// was 35 changed jjc 11-3-15
        t0 = 5;
        t1 = t2 = t3 = 0;
    }

    /// <summary>
    /// How long does the pattern run for, in ticks?
    /// </summary>
    public override ulong period {
        get { return 70; }
    }

    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables.  Not implemented, requires additional care
    /// </summary>
    public override bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns whether or not a head should be on for this pattern given a few different variables
    /// </summary>
    /// <param name="tick">Which tick is it currently?</param>
    /// <param name="phaseB">Is the head on Phase B?</param>
    /// <param name="color2">Are we checking color 2 of the head?</param>
    /// <param name="bit">Which bit is the head on?</param>
    /// <param name="isRear">Is the head in the rear?</param>
    /// <returns>True if the head should be active, false if it should not</returns>
    public bool GetIsActive(ulong tick, bool phaseB, bool color2, byte bit, bool isRear) {
        byte frame = (byte)((tick % 70) / 5);  // Figure out which frame we're on
        if(isRear) {
            switch(frame) { // Rear frames
                case 0:
                    return ((color2 ? 0x1041 : 0x2030) & (0x1 << bit)) > 0;
                case 1:
                    return ((color2 ? 0x10C0 : 0x2820) & (0x1 << bit)) > 0;
                case 2:
                    return ((color2 ? 0x01C0 : 0x2C00) & (0x1 << bit)) > 0;
                case 3:
                    return ((color2 ? 0x0380 : 0x0E00) & (0x1 << bit)) > 0;
                case 4:
                    return ((color2 ? 0x0700 : 0x0700) & (0x1 << bit)) > 0;
                case 5:
                    return ((color2 ? 0x0E00 : 0x0380) & (0x1 << bit)) > 0;
                case 6:
                    return ((color2 ? 0x2C00 : 0x01C0) & (0x1 << bit)) > 0;
                case 7:
                    return ((color2 ? 0x2820 : 0x10C0) & (0x1 << bit)) > 0;
                case 8:
                    return ((color2 ? 0x2030 : 0x1041) & (0x1 << bit)) > 0;
                case 9:
                    return ((color2 ? 0x0038 : 0x1003) & (0x1 << bit)) > 0;
                case 10:
                    return ((color2 ? 0x001C : 0x0007) & (0x1 << bit)) > 0;
                case 11:
                    return ((color2 ? 0x000E : 0x000E) & (0x1 << bit)) > 0;
                case 12:
                    return ((color2 ? 0x0007 : 0x001C) & (0x1 << bit)) > 0;
                case 13:
                    return ((color2 ? 0x0003 : 0x0038) & (0x1 << bit)) > 0;
                default:
                    return false;
            }
        } else {
            switch(frame) {
                case 0:
                    return ((color2 ? 0x10C0 : 0x2820) & (0x1 << bit)) > 0;
                case 1:
                    return ((color2 ? 0x1041 : 0x2030) & (0x1 << bit)) > 0;
                case 2:
                    return ((color2 ? 0x1003 : 0x0038) & (0x1 << bit)) > 0;
                case 3:
                    return ((color2 ? 0x0007 : 0x001C) & (0x1 << bit)) > 0;
                case 4:
                    return ((color2 ? 0x000E : 0x000E) & (0x1 << bit)) > 0;
                case 5:
                    return ((color2 ? 0x001C : 0x0007) & (0x1 << bit)) > 0;
                case 6:
                    return ((color2 ? 0x0038 : 0x1003) & (0x1 << bit)) > 0;
                case 7:
                    return ((color2 ? 0x2030 : 0x1041) & (0x1 << bit)) > 0;
                case 8:
                    return ((color2 ? 0x2820 : 0x10C0) & (0x1 << bit)) > 0;
                case 9:
                    return ((color2 ? 0x2C00 : 0x01C0) & (0x1 << bit)) > 0;
                case 10:
                    return ((color2 ? 0x0E00 : 0x0380) & (0x1 << bit)) > 0;
                case 11:
                    return ((color2 ? 0x0700 : 0x0700) & (0x1 << bit)) > 0;
                case 12:
                    return ((color2 ? 0x0380 : 0x0E00) & (0x1 << bit)) > 0;
                case 13:
                    return ((color2 ? 0x01C0 : 0x2C00) & (0x1 << bit)) > 0;
                default:
                    return false;
            }
        }
    }
}

#endregion

#region Optics

/// <summary>
/// A node that holds optics for a specific Location.
/// </summary>
public class LocationNode {
    /// <summary>
    /// All of the OpticNode options
    /// </summary>
    public Dictionary<string, OpticNode> optics;

    /// <summary>
    /// Constructor.
    /// </summary>
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

/// <summary>
/// A node that holds styles for a specific optic.
/// </summary>
public class OpticNode {
    /// <summary>
    /// All of the StyleNode options
    /// </summary>
    public Dictionary<string, StyleNode> styles;

    /// <summary>
    /// Name of the optic
    /// </summary>
    public string name;
    /// <summary>
    /// Part number prefix for this optic
    /// </summary>
    public string partNumber;
    /// <summary>
    /// Is this optic for a large head?
    /// </summary>
    public bool fitsLg = false;
    /// <summary>
    /// Is this optic for a small head?
    /// </summary>
    public bool fitsSm = false;
    /// <summary>
    /// When splitting, the small equivalent of this head
    /// </summary>
    public string smEquivalent;
    /// <summary>
    /// When merging, the large equivalent of this head
    /// </summary>
    public string lgEquivalent;

    /// <summary>
    /// The sale price of this optic, in whole cents
    /// </summary>
    public uint cost;
    /// <summary>
    /// The current draw of this optic, in whole milliamps
    /// </summary>
    public uint amperage;

    /// <summary>
    /// Is this head a dual-color head?
    /// </summary>
    public bool dual = false;

    /// <summary>
    /// Generic Contructor
    /// </summary>
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

    /// <summary>
    /// Constructor using information gleaned from the library file
    /// </summary>
    /// <param name="defTag">The CompoundTag containing information on this optic</param>
    public OpticNode(NbtCompound defTag) {
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
    /// Creates a copy of this OpticNode.
    /// </summary>
    /// <returns>A copy of this OpticNode.</returns>
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

/// <summary>
/// A node that holds a specific style's information
/// </summary>
public class StyleNode {
    /// <summary>
    /// Name of the style
    /// </summary>
    public string name;
    /// <summary>
    /// Part number suffix for this style
    /// </summary>
    public string partSuffix;
    /// <summary>
    /// Is this style a dual-color style?
    /// </summary>
    public bool isDualColor = false;
    /// <summary>
    /// The display color of this style, also used in the testing against lenses for color compatibility
    /// </summary>
    public Color color;
    /// <summary>
    /// The display color of the second color of this style, not applicable if not dual-color.
    /// </summary>
    public Color color2;
    /// <summary>
    /// Can this style be selected for use?
    /// </summary>
    public bool selectable = true;

    /// <summary>
    /// Generic Contructor
    /// </summary>
    public StyleNode() {
        name = "";
        partSuffix = "";
        isDualColor = false;
        color = color2 = Color.white;
        selectable = true;
    }

    /// <summary>
    /// Parameterized Constructor using information gleaned from the library file
    /// </summary>
    /// <param name="defTag">The CompoundTag containing information on this style</param>
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
    /// Creates a copy of this StyleNode.
    /// </summary>
    /// <returns>A copy of this StyleNode.</returns>
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

#endregion

/// <summary>
/// A class containing information about an option for lens / dome color.  I call them lenses, you call them domes, means the same thing.
/// </summary>
public class Lens {
    /// <summary>
    /// Static variable holding information on the prefix of lenses
    /// </summary>
    public static string smPrefix, lgPrefix;
    /// <summary>
    /// Name of the color of this lens
    /// </summary>
    public string name;
    /// <summary>
    /// This lens's color suffix
    /// </summary>
    public string partSuffix;
    /// <summary>
    /// The sale price of this lens, in whole cents
    /// </summary>
    public uint cost;
    /// <summary>
    /// The display color of this lens, also used to test compatibility with light heads
    /// </summary>
    public Color color;

    /// <summary>
    /// Generic Constructor
    /// </summary>
    public Lens() {
        name = partSuffix = "";
        color = Color.white;
    }

    /// <summary>
    /// Parameterized Constructor using information gleaned from the library file
    /// </summary>
    /// <param name="cmpd">NbtCompound that contains this lens's information</param>
    public Lens(NbtCompound cmpd) {
        name = cmpd["name"].StringValue;
        partSuffix = cmpd["part"].StringValue;
        byte[] clr = cmpd["clr"].ByteArrayValue;
        color = new Color32(clr[0], clr[1], clr[2], clr[3]);
    }

    /// <summary>
    /// Test this lens's color against another's
    /// </summary>
    /// <param name="testAgainst">Color to test against</param>
    /// <returns>Whether the other color would shine through</returns>
    public bool Test(Color testAgainst) {
        float r = Mathf.Min(color.r, testAgainst.r), b = Mathf.Min(color.b, testAgainst.b), g = Mathf.Min(color.g, testAgainst.g);
        return r + b + g >= 1f;
    }
}

/// <summary>
/// Enumeration with all of the possible locations on the bar
/// </summary>
public enum Location {
    FRONT = 0x1, FRONT_CORNER = 0x2, ALLEY = 0x4, REAR_CORNER = 0x8, REAR = 0x10, FAR_REAR = 0x20
}

/// <summary>
/// Enumeration with all of the possible basic functions a head can take on
/// </summary>
public enum BasicFunction {
    NULL = 0x0, FLASHING = 0x1, STEADY = 0x2, EMITTER = 0x4, CAL_STEADY = 0x8, CRUISE = 0x10, STT = 0x20, TRAFFIC = 0x40, BLOCK_OFF = 0x80
}

/// <summary>
/// Enumeration with all of the known advanced functions the control circuit can handle
/// </summary>
public enum AdvFunction {
    NONE = 0x0,
    TAKEDOWN = 0x1, PRIO1 = 0x2, PRIO2 = 0x4, PRIO3 = 0x8,
    TRAFFIC_LEFT = 0x10, TRAFFIC_RIGHT = 0x20, ALLEY_LEFT = 0x40, ALLEY_RIGHT = 0x80,
    ICL = 0x100, DIM = 0x200, FTAKEDOWN = 0x400, FALLEY = 0x800,
    PATTERN = 0x1000, CRUISE = 0x2000, TURN_LEFT = 0x4000, TURN_RIGHT = 0x8000,
    TAIL = 0x10000, T13 = 0x20000, PRIO4 = 0x40000, PRIO5 = 0x80000,
    EMITTER = 0x100000
}

/// <summary>
/// Eumeration with all known Traffic Director options
/// </summary>
public enum TDOption {
    NONE = 0, LG_SEVEN = 1, SM_EIGHT = 2, SM_SIX = 3, LG_EIGHT = 4, LG_SIX = 5
}

/// <summary>
/// Struct holding a cable length option
/// </summary>
[System.Serializable]
public struct CableLengthOption {
    /// <summary>
    /// Length of this option, in feet
    /// </summary>
    public byte length;
    /// <summary>
    /// Sale price of a power cable of this length, in whole cents
    /// </summary>
    public uint pwrPrice;
    /// <summary>
    /// Sale price of a CAN communication cable of this length, in whole cents
    /// </summary>
    public uint canPrice;
    /// <summary>
    /// Sale price of a Hardwire communication cable of this length, in whole cents
    /// </summary>
    public uint hardPrice;
}

/// <summary>
/// Struct holding a mounting foot kit option
/// </summary>
[System.Serializable]
public struct MountingKitOption {
    /// <summary>
    /// Name of this option
    /// </summary>
    public string name;
    /// <summary>
    /// Part number of this option
    /// </summary>
    public string part;
    /// <summary>
    /// Sale price of this option
    /// </summary>
    public uint price;

    public static bool operator ==(MountingKitOption left, MountingKitOption right) {
        return left.part == right.part;
    }

    public static bool operator !=(MountingKitOption left, MountingKitOption right) {
        return left.part != right.part;
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
}

/// <summary>
/// A class holding various extension methods created for the project
/// </summary>
public static class Extensions {
    /// <summary>
    /// Disables a bit in this NbtShort's value
    /// </summary>
    /// <param name="value">The NbtShort to modify</param>
    /// <param name="bit">Which bit to flick off</param>
    public static void DisableBit(this NbtShort value, byte bit) {
        value.Value = (short)(value.Value & ~(0x1 << bit));
    }

    /// <summary>
    /// Enables a bit in this NbtShort's value
    /// </summary>
    /// <param name="value">The NbtShort to modify</param>
    /// <param name="bit">Which bit to flick on</param>
    public static void EnableBit(this NbtShort value, byte bit) {
        value.Value = (short)(value.Value | (short)(0x1 << bit));
    }

    /// <summary>
    /// Recursively gets the path of this Transform
    /// </summary>
    /// <param name="t">The Transform to generate a path for</param>
    /// <returns>A path that represents this Transform's ancestry, ie ("/Bar/DF/FO/L")</returns>
    public static string GetPath(this Transform t) {
        if(t.parent == null) return "/" + t.name;
        else return t.parent.GetPath() + "/" + t.name;
    }

    /// <summary>
    /// Gets the path of this Component
    /// </summary>
    /// <param name="c">The Component to generate a path for</param>
    /// <returns>A path that represents this Component's ancestry, ie ("/Bar/DF/FO/L:LightHead")</returns>
    public static string GetPath(this Component c) {
        return c.transform.GetPath() + ":" + c.GetType().ToString();
    }

    //public static PdfSharp.Drawing.XColor ToXColor(this Color c) {
    //    return PdfSharp.Drawing.XColor.FromArgb(Mathf.RoundToInt(c.a * 255), Mathf.RoundToInt(c.r * 255), Mathf.RoundToInt(c.g * 255), Mathf.RoundToInt(c.b * 255));
    //}

    //public static PdfSharp.Drawing.XColor ToXColor(this Color32 c) {
    //    return PdfSharp.Drawing.XColor.FromArgb(c.a, c.r, c.g, c.b);
    //}

    //public static PdfSharp.Drawing.XPoint ToXPoint(this Vector2 v) {
    //    return new PdfSharp.Drawing.XPoint(v.x, v.y);
    //}
}