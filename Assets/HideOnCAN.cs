using UnityEngine;
using System.Collections;

public class HideOnCAN : MonoBehaviour {
    void OnEnable() {
        gameObject.SetActive(!BarManager.useCAN);
    }
}
