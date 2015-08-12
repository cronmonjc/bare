using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI Component.  Disables the "Save Order / Quote" button if the customer name or PO Field are blank.
/// </summary>
public class OrderDisable : MonoBehaviour {
    /// <summary>
    /// The button to disable.  Set via Unity Inspector.
    /// </summary>
    public Button b;
    /// <summary>
    /// The name field
    /// </summary>
    public InputField nameField;
    /// <summary>
    /// The PO field
    /// </summary>
    public InputField poField;

    /// <summary>
    /// Tests this Component.
    /// </summary>
    public void Test() {
        b.interactable = (nameField.text.Trim() != "" && poField.text.Trim() != "");
    }
}
