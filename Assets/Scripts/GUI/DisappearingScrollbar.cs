using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DisappearingScrollbar : MonoBehaviour {
    private Scrollbar s;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        s = GetComponent<Scrollbar>();
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        bool show = !(s.size > 0.98f);
        foreach(Transform t in transform) {
            t.gameObject.SetActive(show);
        }
    }
}
