using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeviceMonitor : MonoBehaviour {
    public Image icon;
    public Text label;
    public Sprite connect, disconnect;
    private DeviceManager dm;

    void Start() {
        dm = FindObjectOfType<DeviceManager>();
    }

    // Update is called once per frame
    void Update() {
        if(dm.DevConnected) {
            icon.sprite = connect;
            label.text = "Connected";
        } else {
            icon.sprite = disconnect;
            label.text = "Disconnected";
        }
    }
}
