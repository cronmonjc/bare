using UnityEngine;
using System.Collections;

public class RightPane : MonoBehaviour {
    public void SetOrder(bool order) {
        GetComponent<Animator>().SetBool("Order", order);
    }
}
