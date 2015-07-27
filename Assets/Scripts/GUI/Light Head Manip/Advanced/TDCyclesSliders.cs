using System.Collections;
using UnityEngine;

public class TDCyclesSliders : MonoBehaviour {
    public UnityEngine.UI.Text outputText;
    private fNbt.NbtShort td, warn;

    void Start() {
        Display(0, 0);
    }

    public void SetTD(float to) {
        byte toB = (byte)(Mathf.RoundToInt(to));
        if(td == null) {
            FetchTags();
        }
        td.Value = toB;
        Refresh();
    }

    public void SetWarn(float to) {
        byte toB = (byte)(Mathf.RoundToInt(to));
        if(td == null) {
            FetchTags();
        }
        warn.Value = toB;
        Refresh();
    }

    public void FetchTags() {
        fNbt.NbtCompound cmpd = BarManager.inst.patts.Get<fNbt.NbtCompound>("traf");
        td = cmpd.Get<fNbt.NbtShort>("ctd");
        warn = cmpd.Get<fNbt.NbtShort>("cwn");
    }

    public void Refresh() {
        if(td == null) {
            FetchTags();
        } else {
            Display((byte)td.Value, (byte)warn.Value);
        }
    }

    public void Display(byte tdCycles, byte tdWarnCycles) {
        outputText.text = "If both the Traffic Director and a Priority are enabled, the above heads can alternate between Traffic Director and Priority patterns.  It will cycle through the Traffic Director's pattern <b><color=#D00F>" + tdCycles + "</color></b> times, then through the Priority's pattern <b><color=#00DF>" + tdWarnCycles + "</color></b> times, then loop.";
    }
}
