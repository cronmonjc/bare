using UnityEngine;
using System.Collections.Generic;

public class InUseDisabler : MonoBehaviour {
    List<FnDrag> draggables;

    void Start() {
        draggables = new List<FnDrag>(transform.GetComponentsInChildren<FnDrag>());
    }

    void Update() {
        foreach(FnDrag alpha in draggables) {
            alpha.gameObject.SetActive(true);
        }

        List<FnDrag> test = new List<FnDrag>(draggables);

        foreach(int i in FnDragTarget.inputMap.Value) {
            foreach(FnDrag alpha in new List<FnDrag>(test)) {
                if((((int)alpha.myFunc) & i) > 0) {
                    alpha.gameObject.SetActive(false);
                    test.Remove(alpha);
                }
            }
        }
    }
}
