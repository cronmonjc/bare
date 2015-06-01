using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// GUI Item.  Used to display light choices and handle selection.
/// </summary>
public class LightOptionElement : MonoBehaviour, IPointerClickHandler {
    /// <summary>
    /// Reference to this item's text component.
    /// </summary>
    public Text text;
    /// <summary>
    /// If this is for selecting a function, this is a reference to the Function Select.
    /// </summary>
    public FunctionSelect funcSel;
    /// <summary>
    /// If this is for selecting an optic, this is a reference to the Optic Select.
    /// </summary>
    public OpticSelect optSel;
    /// <summary>
    /// If this is for selecting a style, this is a reference to the Style Select.
    /// </summary>
    public StyleSelect stySel;
    /// <summary>
    /// If this is for selecting a function, this is the function that would be selected when this item is clicked.
    /// </summary>
    public BasicFunction fn;
    /// <summary>
    /// If this is for selecting an optic, this is the type that would be selected when this item is clicked.
    /// </summary>
    public OpticNode optNode;
    /// <summary>
    /// If this is for selecting a style, this is the style that would be selected when this item is clicked.
    /// </summary>
    public StyleNode styNode;
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
        Update();
    }

    void Update() {
        if(funcSel != null) {
            t.isOn = funcSel.selFuncs.Contains(fn);
            t.image.color = Color.white;
            t.interactable = true;
            text.color = Color.black;
            string name = "";
            switch(fn) {
                case BasicFunction.FLASHING:
                    name = "Flashing";
                    break;
                case BasicFunction.STEADY:
                    name = "Steady Burn";
                    break;
                case BasicFunction.EMITTER:
                    name = "Emitter";
                    break;
                case BasicFunction.CAL_STEADY:
                    name = "Cali Steady";
                    break;
                case BasicFunction.CRUISE:
                    name = "Cruise";
                    break;
                case BasicFunction.STT:
                    name = "Stop/Tail/Turn";
                    break;
                case BasicFunction.TRAFFIC:
                    name = "Traffic Director";
                    break;
                default:
                    throw new System.ArgumentException();
            }
            text.text = name;
        } else if(optSel != null) {
            bool on = true;
            foreach(LightHead lh in BarManager.inst.allHeads) {
                if(lh.gameObject.activeInHierarchy && lh.Selected && lh.lhd.optic != optNode) {
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
        } else if(stySel != null) {
            bool on = true;
            foreach(LightHead lh in BarManager.inst.allHeads) {
                if(lh.gameObject.activeInHierarchy && lh.Selected && lh.lhd.style != styNode) {
                    on = false;
                }
            }
            t.isOn = on;
            if(styNode.selectable && recommended) {
                t.image.color = Color.white;
                t.interactable = true;
                text.color = Color.black;
                text.text = styNode.name;
            } else if(styNode.selectable) {
                t.image.color = nrColor;
                t.interactable = true;
                text.color = Color.black;
                text.text = styNode.name + " -- Not Recommended";
            } else {
                t.interactable = false;
                text.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
                text.text = styNode.name + " -- Not an Option";
            }
        }

    }

    public void OnPointerClick(PointerEventData eventData) {
        if(funcSel != null)
            funcSel.SetSelection(fn);
        else if(optSel != null)
            optSel.SetSelection(optNode);
        else if(stySel != null && styNode.selectable)
            stySel.SetSelection(styNode);
    }
}
