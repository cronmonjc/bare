using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// GUI Item.  Used to display light choices and handle selection.
/// </summary>
public class LightOptionElement : MonoBehaviour {
    /// <summary>
    /// Reference to this item's text component.
    /// </summary>
    public Text text;
    /// <summary>
    /// If this is for selecting an optic, this is a reference to the Optic Select.
    /// </summary>
    public OpticSelect os;
    /// <summary>
    /// If this is for selecting a style, this is a reference to the Style Select.
    /// </summary>
    public StyleSelect ss;
    /// <summary>
    /// If this is for selecting a optic, this is the type that would be selected when this item is clicked.
    /// </summary>
    public OpticNode optNode;
    /// <summary>
    /// If this is for selecting a style, this is the style that would be selected when this item is clicked.
    /// </summary>
    public StyleNode sn;
    /// <summary>
    /// Reference to the Toggle to show this is selected.
    /// </summary>
    public Toggle t;
    /// <summary>
    /// The color to show when this item isn't recommended.
    /// </summary>
    public Color nrColor;

    public bool recommended;

    void Start() {
        t.onValueChanged.AddListener(delegate(bool on) {
            if(on) {
                if(os != null)
                    os.SetSelection(optNode);
                else if(ss != null)
                    ss.SetSelection(sn);
            }
        });

        Update();
    }

    void Update() {
        if(os != null) {
            bool on = true;
            foreach(LightHead lh in os.cam.OnlyCamSelected) {
                if(lh.lhd.optic != optNode) {
                    on = false;
                }
            }
            t.isOn = on;
            if(optNode != null) {
                t.image.color = Color.white;
                t.interactable = true;
                text.color = Color.black;
                text.text = optNode.name;
            } else {
                text.text = "Empty Slot";
            }
        } else if(ss != null) {
            bool on = true;
            foreach(LightHead lh in ss.cam.OnlyCamSelected) {
                if(lh.lhd.style != sn) {
                    on = false;
                }
            }
            t.isOn = on;
            if(sn.selectable && recommended) {
                t.image.color = Color.white;
                t.interactable = true;
                text.color = Color.black;
                text.text = sn.name;
            } else if(sn.selectable) {
                t.image.color = nrColor;
                t.interactable = true;
                text.color = Color.black;
                text.text = sn.name + " -- Not Recommended";
            } else {
                t.interactable = false;
                text.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
                text.text = sn.name + " -- Not an Option";
            }
        }

    }

}
