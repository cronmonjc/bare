using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using fNbt;

public class AdvFuncDisp : MonoBehaviour {
    public static CameraControl cam;
    public AdvFunction func;

    private Text c1patt, c2patt, c1phase, c2phase;
    private Image c1enable, c2enable;

    void Start() {
        if(cam != null) cam = FindObjectOfType<CameraControl>();

        c1enable = transform.FindChild("C1Enable").FindChild("Image").GetComponent<Image>();
        c2enable = transform.FindChild("C2Enable").FindChild("Image").GetComponent<Image>();

        switch(func) {
            case AdvFunction.TRAFFIC_LEFT:
            case AdvFunction.TRAFFIC_RIGHT:
                c1patt = transform.FindChild("C1Patt").GetComponent<Text>();
                c2patt = transform.FindChild("C2Patt").GetComponent<Text>();
                break;
            case AdvFunction.LEVEL1:
            case AdvFunction.LEVEL2:
            case AdvFunction.LEVEL3:
            case AdvFunction.LEVEL4:
            case AdvFunction.LEVEL5:
            case AdvFunction.FTAKEDOWN:
            case AdvFunction.FALLEY:
            case AdvFunction.ICL:
                c1patt = transform.FindChild("C1Patt").GetComponent<Text>();
                c2patt = transform.FindChild("C2Patt").GetComponent<Text>();
                c1phase = transform.FindChild("C1Phase").GetComponent<Text>();
                c2phase = transform.FindChild("C2Phase").GetComponent<Text>();
                break;
            default:
                break;
        }

    }

    public void Refresh() {
        NbtCompound cmpd = BarManager.inst.patts.Get<NbtCompound>(BarManager.GetFnString(cam.OnlyCamSelected[0].transform, func));


        switch(func) {
            case AdvFunction.TRAFFIC_LEFT:
            case AdvFunction.TRAFFIC_RIGHT:

                break;
            case AdvFunction.LEVEL1:
            case AdvFunction.LEVEL2:
            case AdvFunction.LEVEL3:
            case AdvFunction.LEVEL4:
            case AdvFunction.LEVEL5:
            case AdvFunction.FTAKEDOWN:
            case AdvFunction.FALLEY:
            case AdvFunction.ICL:

                break;
            default:
                break;
        }
    }
}
