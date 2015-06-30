using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LabelTooltip : MonoBehaviour {
    public GameObject labelGO;
    public Text text;

    public void Show(string msg) {
        text.text = msg;
        labelGO.SetActive(true);
    }

    public void Hide() {
        labelGO.SetActive(false);
    }
}
