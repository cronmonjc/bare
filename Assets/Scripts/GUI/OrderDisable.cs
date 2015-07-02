using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OrderDisable : MonoBehaviour {
    public Button b;
    public InputField name, po;

    public void Test() {
        b.interactable = (name.text != "" && po.text != "");
    }
}
