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
    public LensSelect lensSel;
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
    public Lens lens;
    /// <summary>
    /// Reference to the Toggle to show this is selected.
    /// </summary>
    public Toggle t;
    /// <summary>
    /// The color to show when this item isn't recommended.
    /// </summary>
    public Color nrColor;

    public bool recommended;

    private CameraControl cam;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        Update();
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
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
                    if(cam == null) cam = FindObjectOfType<CameraControl>();

                    byte locs = 0x0;
                    foreach(LightHead alpha in cam.OnlyCamSelectedHead) {
                        locs |= (byte)alpha.loc;
                    }

                    switch(locs) {
                        case 0x1:
                        case 0x2:
                        case 0x3:
                            name = "Takedown";
                            break;
                        case 0x4:
                            byte side = 0x0;
                            foreach(LightHead alpha in cam.OnlyCamSelectedHead) {
                                if(alpha.Bit == 12) {
                                    side |= 0x1;
                                } else if(alpha.Bit == 13) {
                                    side |= 0x2;
                                }
                            }
                            switch(side) {
                                case 0x1:
                                    name = "Left Alley";
                                    break;
                                case 0x2:
                                    name = "Right Alley";
                                    break;
                                default:
                                    name = "Alley";
                                    break;
                            }
                            break;
                        case 0x8:
                        case 0x10:
                        case 0x18:
                        case 0x20:
                        case 0x28:
                        case 0x30:
                        case 0x38:
                            name = "Work Light";
                            break;
                        default:
                            name = "Steady Burn";
                            break;
                    }
                    break;
                case BasicFunction.EMITTER:
                    name = "Emitter";
                    break;
                case BasicFunction.CAL_STEADY:
                    name = "California T13 Steady";
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
                case BasicFunction.BLOCK_OFF:
                    name = "Block Off";
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
        } else if(lensSel != null) {
            bool on = true;
            foreach(BarSegment seg in lensSel.cam.SelectedLens) {
                if(seg.Visible && seg.lens != lens) {
                    on = false;
                }
            }
            t.isOn = on;
            t.image.color = Color.white;
            t.interactable = true;
            text.color = Color.black;
            text.text = "Clear Coated " + lens.name + " Lens";
        }

    }

    public void OnPointerClick(PointerEventData eventData) {
        if(funcSel != null)
            funcSel.SetSelection(fn);
        else if(optSel != null)
            optSel.SetSelection(optNode);
        else if(stySel != null && styNode.selectable)
            stySel.SetSelection(styNode);
        else if(lensSel != null)
            lensSel.SetSelection(lens);
    }
}
