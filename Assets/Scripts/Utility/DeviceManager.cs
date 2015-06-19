using UnityEngine;
using System.Collections;

public class DeviceManager : MonoBehaviour {
    public Device d;

    public bool DevConnected {
        get {
#if UNITY_EDITOR
            return false;
#else
            return d != null && d.Connected;
#endif
        }
    }

    void Start() {
        try {
            d = new Device();
        } catch(DeviceErrorException) {
            d = null;
        }
    }

    public void Upload() {
        try {
            if(d == null) d = new Device();
        } catch(DeviceErrorException ex) {
            switch(ex.errCode) {
                case -2:
                case -8:
                case -10:
                    ErrorText.inst.DispError("Cannot communicate with bar.  Wait a few seconds and try again. (" + ex.errCode + ")");
                    break;
                case -9:
                    ErrorText.inst.DispError("Cannot communicate with bar.  The communication channel might be damaged. (" + ex.errCode + ")");
                    break;
                case -101:
                    ErrorText.inst.DispError("Cannot communicate with bar.  Is it connected? (" + ex.errCode + ")");
                    d = null;
                    break;
                default:
                    ErrorText.inst.DispError("Unknown error communicating with bar. (" + ex.errCode + ")");
                    d = null;
                    break;
            }
        }
    }

    public void Download() {
        try {
            if(d == null) d = new Device();
        } catch(DeviceErrorException ex) {
            switch(ex.errCode) {
                case -2:
                case -8:
                case -10:
                    ErrorText.inst.DispError("Cannot communicate with bar.  Wait a few seconds and try again. (" + ex.errCode + ")");
                    break;
                case -9:
                    ErrorText.inst.DispError("Cannot communicate with bar.  The communication channel might be damaged. (" + ex.errCode + ")");
                    break;
                case -101:
                    ErrorText.inst.DispError("Cannot communicate with bar.  Is it connected? (" + ex.errCode + ")");
                    d = null;
                    break;
                default:
                    ErrorText.inst.DispError("Unknown error communicating with bar. (" + ex.errCode + ")");
                    d = null;
                    break;
            }
        }
    }

    public void Wipe() {
        try {
            if(d == null) d = new Device();
        } catch(DeviceErrorException ex) {
            switch(ex.errCode) {
                case -2:
                case -8:
                case -10:
                    ErrorText.inst.DispError("Cannot communicate with bar.  Wait a few seconds and try again. (" + ex.errCode + ")");
                    break;
                case -9:
                    ErrorText.inst.DispError("Cannot communicate with bar.  The communication channel might be damaged. (" + ex.errCode + ")");
                    break;
                case -101:
                    ErrorText.inst.DispError("Cannot communicate with bar.  Is it connected? (" + ex.errCode + ")");
                    d = null;
                    break;
                default:
                    ErrorText.inst.DispError("Unknown error communicating with bar. (" + ex.errCode + ")");
                    d = null;
                    break;
            }
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "ChipBase.png", true);
    }

    void OnApplicationQuit() {
        d.Dispose();
    }
}
