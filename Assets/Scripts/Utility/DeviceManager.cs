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
        d = new Device();
    }

    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "ChipBase.png", true);
    }

    void OnApplicationQuit() {
        d.Dispose();
    }
}
