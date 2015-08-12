using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Manages options for traffic director setups.
/// </summary>
public class TDPulldown : PulldownItem {
    /// <summary>
    /// The text color to use when enabled.  Set via Unity Inspector.
    /// </summary>
    public Color textEnabled = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    /// <summary>
    /// The text color to use when disabled.  Set via Unity Inspector.
    /// </summary>
    public Color textDisabled = new Color(0.75f, 0.75f, 0.75f, 1.0f);
    /// <summary>
    /// The Text Component itself.  Set via Unity Inspector.
    /// </summary>
    public UnityEngine.UI.Text text;

    /// <summary>
    /// Determines whether this Component's option is selected.  Will always return false, because coloration is handled by this Component.
    /// </summary>
    /// <returns>
    /// False always
    /// </returns>
    protected override bool IsSelected() {
        return false;
    }

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        prev = false;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(b == null) b = GetComponent<UnityEngine.UI.Button>();
        bool curr = BarManager.inst.td == (TDOption)number;
        if(prev ^ curr) {
            b.colors = curr ? selected : unselected;
            prev = curr;
        }


        switch((TDOption)number) {
            case TDOption.LG_SIX:
                b.interactable = BarManager.inst.BarSize > 1;
                text.color = b.interactable ? textEnabled : textDisabled;
                break;
            case TDOption.LG_SEVEN:
                b.interactable = BarManager.inst.BarSize > 2 && BarManager.inst.BarSize != 4;
                text.color = b.interactable ? textEnabled : textDisabled;
                break;
            case TDOption.LG_EIGHT:
                b.interactable = BarManager.inst.BarSize > 3;
                text.color = b.interactable ? textEnabled : textDisabled;
                break;
            default:
                text.color = textEnabled;
                break;
        }
    }

    /// <summary>
    /// Called when the user clicks on the Button Component on this GameObject.
    /// </summary>
    public override void Clicked() {
        BarManager.moddedBar = true;

        BOMCables bomc = FindObjectOfType<BOMCables>();
        if(bomc != null)
            bomc.Refresh();
    }
}
