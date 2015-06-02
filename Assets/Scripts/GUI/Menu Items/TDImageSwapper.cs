using UnityEngine;
using System.Collections;

public class TDImageSwapper : MonoBehaviour {
    public Sprite idle, selected;
    public TDOption myOpt;
    private UnityEngine.UI.Image img;

    void Update() {
        if(img == null) img = GetComponent<UnityEngine.UI.Image>();

        img.sprite = (BarManager.inst.td == myOpt ? selected : idle);
    }
}
