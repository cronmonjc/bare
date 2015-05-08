using UnityEngine;
using System.Collections;

public class SizeOptionControl : MonoBehaviour {

    private GameObject longGO, shortGO;

    public bool[] canLong = new bool[] { true, true, true, true }, canShort = new bool[] { true, true, true, true };

    public bool ShowLong {
        get {
            return longGO.activeInHierarchy;
        }
        set {
            int size = FindObjectOfType<BarManager>().BarSize;

            if(canLong[size] && !canShort[size]) {
                longGO.SetActive(true);
                shortGO.SetActive(false);
            } else if(!canLong[size] && canShort[size]) {
                longGO.SetActive(false);
                shortGO.SetActive(true);
            } else {
                longGO.SetActive(value && canLong[size]);
                shortGO.SetActive(!value && canShort[size]);
            }
        }
    }


    void Start() {
        longGO = transform.FindChild("Long").gameObject;
        shortGO = transform.FindChild("DoubleShort").gameObject;

        ShowLong = true;
    }

}
