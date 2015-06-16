using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FuncPatt : MonoBehaviour {
    public Text text;
    public Pattern patt;
    public Image i;
    public FuncPattSelect fps;

    public void Refresh() {
        bool on = true;
        text.text = patt.name;

        if(!BarManager.inst.patts.Get<fNbt.NbtCompound>(BarManager.GetFnString(BarManager.inst.transform, FunctionEditPane.currFunc)).Contains(patt is TraffPatt ? "patt" : "pat1")) {
            return;
        }

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;
            on &= alpha.GetPattern(FunctionEditPane.currFunc, fps.IsColor2) == patt;
        }

        i.enabled = on;
    }

    public void Clicked() {
        fps.SetSelection(patt);
    }
}
