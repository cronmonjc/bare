using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// GUI Item.  Handles the navigation of the order finisher form using Tab and Shift-Tab.
/// </summary>
public class TabSelect : MonoBehaviour {
    public bool IsMain = false;
    private static bool fired = false;

    private EventSystem system;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        system = EventSystem.current;
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(!fired && Input.GetKeyDown(KeyCode.Tab)) {
            if(system.currentSelectedGameObject == null && IsMain) {
                system.SetSelectedGameObject(gameObject);
                fired = true;
            } else if(system.currentSelectedGameObject == gameObject) {
                if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    system.SetSelectedGameObject(GetComponent<Selectable>().FindSelectableOnUp().gameObject);
                } else {
                    system.SetSelectedGameObject(GetComponent<Selectable>().FindSelectableOnDown().gameObject);
                }
                fired = true;
            }
        }
        if(fired && Input.GetKeyUp(KeyCode.Tab)) {
            fired = false;  
        }
    }
}
