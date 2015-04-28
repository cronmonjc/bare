using UnityEngine;
using System.Collections;

public class SizeOptionControl : MonoBehaviour {

    private GameObject longGO, shortGO;

    public bool ShowLong {
        get {
            return longGO.activeInHierarchy;
        }
        set {
            longGO.SetActive(value);
            shortGO.SetActive(!value);
        }
    }


    void Start() {
        longGO = transform.FindChild("Long").gameObject;
        shortGO = transform.FindChild("Short").gameObject;

        ShowLong = true;
    }

}
