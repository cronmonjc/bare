using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OrderDisable : MonoBehaviour {
    public Button b;
    public InputField nameField, poField;

    public void Test() {
        b.interactable = (nameField.text.Trim() != "" && poField.text.Trim() != "");
    }
}
