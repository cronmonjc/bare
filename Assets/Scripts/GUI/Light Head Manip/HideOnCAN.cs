using UnityEngine;
using System.Collections;

public class HideOnCAN : MonoBehaviour {
    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        gameObject.SetActive(!BarManager.useCAN);
    }
}
