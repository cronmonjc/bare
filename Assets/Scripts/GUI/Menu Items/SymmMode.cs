using UnityEngine;
using System.Collections;

public class SymmMode : MonoBehaviour {
    public bool On {
        get { return GetComponent<UnityEngine.UI.Toggle>().isOn; }
    }
}
