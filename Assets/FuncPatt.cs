using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FuncPatt : MonoBehaviour {
    public Text text;
    public Pattern patt;
    public Image i;
    public FuncPattSelect fps;

    void Start() {
        text.text = patt.name;
    }

    void Update() {
        bool on = true;

        foreach(LightHead alpha in BarManager.inst.allHeads) {

        }
    }

    public void Clicked() {
        fps.SetSelection(patt);
    }
}
