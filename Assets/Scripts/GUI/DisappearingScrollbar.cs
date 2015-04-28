using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DisappearingScrollbar : MonoBehaviour {
    private Scrollbar s;

    // Use this for initialization
    void Start() {
        s = GetComponent<Scrollbar>();
    }

    // Update is called once per frame
    void Update() {
        bool show = !(s.size > 0.98f);
        GetComponent<Image>().enabled = show;
        foreach(Transform t in transform) {
            t.gameObject.SetActive(show);
        }
    }
}
