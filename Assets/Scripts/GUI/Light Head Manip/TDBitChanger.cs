using UnityEngine;
using System.Collections;

public class TDBitChanger : MonoBehaviour {
    public byte[] noTDBits = new byte[] { 0, 0, 0, 0, 0 },
        SvLgBits = new byte[] { 0, 0, 0, 0, 0 },
        EtSmBits = new byte[] { 0, 0, 0, 0, 0 },
        SxSmBits = new byte[] { 0, 0, 0, 0, 0 },
        EtLgBits = new byte[] { 0, 0, 0, 0, 0 },
        SxLgBits = new byte[] { 0, 0, 0, 0, 0 };

    public byte Bit {
        get {
            BarManager bm = BarManager.inst;
            int size = bm.BarSize;
            switch(bm.td) {
                case TDOption.NONE:
                    return noTDBits[size];
                case TDOption.LG_SEVEN:
                    return SvLgBits[size];
                case TDOption.SM_EIGHT:
                    return EtSmBits[size];
                case TDOption.SM_SIX:
                    return SxSmBits[size];
                case TDOption.LG_EIGHT:
                    return EtLgBits[size];
                case TDOption.LG_SIX:
                    return SxLgBits[size];
                default:
                    return 255;
            }
        }
    }
}
