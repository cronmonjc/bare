using System.Collections;
using UnityEngine;

/// <summary>
/// UI Component.  Manages information regarding the two shorts holding the Cycles TD and Cycles Warn values.
/// </summary>
public class TDCyclesSliders : MonoBehaviour {
    /// <summary>
    /// The output Text Component.  Set via Unity Inspector.
    /// </summary>
    public UnityEngine.UI.Text outputText;
    /// <summary>
    /// The Cycles TD NbtShort Tag
    /// </summary>
    private fNbt.NbtShort td;
    /// <summary>
    /// The Cycles Warn NbtShort Tag
    /// </summary>
    private fNbt.NbtShort warn;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        Display(0, 0);
    }

    /// <summary>
    /// Sets the Cycles TD value.  Called by one of the two sliders.
    /// </summary>
    /// <param name="to">The value passed by the slider</param>
    public void SetTD(float to) {
        byte toB = (byte)(Mathf.RoundToInt(to));
        if(td == null) {
            FetchTags();
        }
        td.Value = toB;
        Refresh();
    }

    /// <summary>
    /// Sets the Cycles Warn value.  Called by one of the two sliders.
    /// </summary>
    /// <param name="to">The value passed by the slider</param>
    public void SetWarn(float to) {
        byte toB = (byte)(Mathf.RoundToInt(to));
        if(td == null) {
            FetchTags();
        }
        warn.Value = toB;
        Refresh();
    }

    /// <summary>
    /// Fetches the Tags.
    /// </summary>
    public void FetchTags() {
        fNbt.NbtCompound cmpd = BarManager.inst.patts.Get<fNbt.NbtCompound>("traf");
        td = cmpd.Get<fNbt.NbtShort>("ctd");
        warn = cmpd.Get<fNbt.NbtShort>("cwn");
    }

    /// <summary>
    /// Refreshes this Component.
    /// </summary>
    public void Refresh() {
        if(td == null) {
            FetchTags();
        } else {
            Display((byte)td.Value, (byte)warn.Value);
        }
    }

    /// <summary>
    /// Displays text on the Display Text Component.
    /// </summary>
    /// <param name="tdCycles">The Cycles TD to display.</param>
    /// <param name="tdWarnCycles">The Cycles Warn to display.</param>
    public void Display(byte tdCycles, byte tdWarnCycles) {
        outputText.text = "If both the Traffic Director and a Priority are enabled, the above heads can alternate between Traffic Director and Priority patterns.  It will cycle through the Traffic Director's pattern <b><color=#D00F>" + tdCycles + "</color></b> times, then through the Priority's pattern <b><color=#00DF>" + tdWarnCycles + "</color></b> times, then loop.";
    }
}
