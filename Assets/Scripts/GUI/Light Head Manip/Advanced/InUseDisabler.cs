using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Component.  Hides FnDrags when the functions they represent are in use.
/// </summary>
public class InUseDisabler : MonoBehaviour {
    /// <summary>
    /// The list of FnDrags
    /// </summary>
    List<FnDrag> draggables;
    /// <summary>
    /// The list of functions in use
    /// </summary>
    List<AdvFunction> funcsInUse;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        draggables = new List<FnDrag>(transform.GetComponentsInChildren<FnDrag>());
        funcsInUse = new List<AdvFunction>();
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        #region Compile list of functions
        funcsInUse.Clear();

        foreach(int i in FnDragTarget.inputMap.Value) {
            for(int func = 1; func < 0x140000; func = func << 1) {
                if((i & func) > 0) {
                    funcsInUse.Add((AdvFunction)func);
                }
            }
        } 
        #endregion

        #region Hide / Show where necessary
        foreach(FnDrag alpha in draggables) {
            if(!alpha.gameObject.activeInHierarchy && !funcsInUse.Contains(alpha.myFunc)) {
                alpha.gameObject.SetActive(true);
            } else if(alpha.gameObject.activeInHierarchy && funcsInUse.Contains(alpha.myFunc)) {
                alpha.gameObject.SetActive(false);
            }
        } 
        #endregion
    }
}
