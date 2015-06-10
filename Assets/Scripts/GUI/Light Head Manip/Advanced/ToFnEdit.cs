using UnityEngine;
using System.Collections;

public class ToFnEdit : MonoBehaviour {
    public FunctionEditPane FnEditPane;
    public AdvFunction myFunc;

    public void Clicked() {
        FnEditPane.currFunc = myFunc;
    }
}
