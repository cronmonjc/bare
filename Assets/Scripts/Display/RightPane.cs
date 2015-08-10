using UnityEngine;
using System.Collections;

/// <summary>
/// Simple UI Component, provides hook to perform animation of right pane sliding.  Piggybacks on UnityEngine.Animator.
/// </summary>
public class RightPane : MonoBehaviour {
    /// <summary>
    /// Begins animation.  Called by the "Continue with Order" button and "Return to Editing" button.
    /// </summary>
    public void SetOrder(bool order) {
        GetComponent<Animator>().SetBool("Order", order);
    }
}
