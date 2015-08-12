using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Immediately disables the owning GameObject if the CAN Module is enabled - used on the Cable Type option.
/// </summary>
public class HideOnCAN : MonoBehaviour {
    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        gameObject.SetActive(!BarManager.useCAN);
    }
}
