using UnityEngine;
using System.Collections;

/// <summary>
/// UI Component.  Tests if any of the corners are selected, and displays warnings in the optic editing pane if they are.
/// </summary>
public class CornerCheck : MonoBehaviour {
    /// <summary>
    /// The reference to the "Corners can only be long" GameObject
    /// </summary>
    public GameObject CornerLong;
    /// <summary>
    /// The reference to the "Rear corners are single color only" GameObject
    /// </summary>
    public GameObject RearCorner;

    /// <summary>
    /// Performs the retest.
    /// </summary>
    public void Retest() {
        CornerLong.SetActive(false);
        RearCorner.SetActive(false);

        foreach(LightHead alpha in BarManager.inst.allHeads) {
            if(!alpha.gameObject.activeInHierarchy || !alpha.Selected) continue;

            if(alpha.loc == Location.FRONT_CORNER || alpha.loc == Location.REAR_CORNER) {
                CornerLong.SetActive(true);
            }
            if(alpha.loc == Location.REAR_CORNER) {
                RearCorner.SetActive(true);
            }
        }
    }
}
