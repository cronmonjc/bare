using UnityEngine;
using System.Collections.Generic;

public class InUseDisabler : MonoBehaviour {
    List<FnDrag> draggables;
    List<AdvFunction> funcsInUse;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        draggables = new List<FnDrag>(transform.GetComponentsInChildren<FnDrag>());
        funcsInUse = new List<AdvFunction>();
    }

    void Update() {
        funcsInUse.Clear();

        foreach(int i in FnDragTarget.inputMap.Value) {
            for(int func = 1; func < 0x140000; func = func << 1) {
                if((i & func) > 0) {
                    funcsInUse.Add((AdvFunction)func);
                }
            }
        }

        foreach(FnDrag alpha in draggables) {
            if(!alpha.gameObject.activeInHierarchy && !funcsInUse.Contains(alpha.myFunc)) {
                alpha.gameObject.SetActive(true);
            } else if(alpha.gameObject.activeInHierarchy && funcsInUse.Contains(alpha.myFunc)) {
                alpha.gameObject.SetActive(false);
            }
        }
    }
}
