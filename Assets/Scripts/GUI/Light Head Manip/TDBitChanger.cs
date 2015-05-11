using UnityEngine;
using System.Collections;

public class TDBitChanger : MonoBehaviour {
    public byte[] noTDBits = new byte[] { 0, 0, 0, 0 },
        SvLgBits = new byte[] { 0, 0, 0, 0 },
        EtSmBits = new byte[] { 0, 0, 0, 0 },
        SxSmBits = new byte[] { 0, 0, 0, 0 };

    public byte Bit {
        get {
            BarManager bm = FindObjectOfType<BarManager>();
            int size = bm.BarSize;
            TDOption td = bm.td;
            switch(td) {
                case TDOption.NONE:
                    return noTDBits[size];
                case TDOption.LG_SEVEN:
                    return SvLgBits[size];
                case TDOption.SM_EIGHT:
                    return EtSmBits[size];
                case TDOption.SM_SIX:
                    return SxSmBits[size];
                default:
                    return 255;
            }
        }
    }
}
