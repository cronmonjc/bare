using UnityEngine;
using System.Collections;

public class TDImageSwapper : MonoBehaviour {
    public Sprite idle, selected;
    public TDOption myOpt;
    private UnityEngine.UI.Image img;

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(img == null) img = GetComponent<UnityEngine.UI.Image>();

        img.sprite = (BarManager.inst.td == myOpt ? selected : idle);
    }
}
