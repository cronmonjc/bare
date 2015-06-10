using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PatternFunc : MonoBehaviour, IPointerClickHandler {
    //public FnSelManager fsl;
    //public Text t;
    //public AdvFunction fn;

    //public string DispPattern {
    //    set {
    //        string func = "UnknownFn: ";
    //        switch(fn) {
    //            case AdvFunction.ALLEY:
    //                func = "Alley: ";
    //                break;
    //            case AdvFunction.CRUISE:
    //                func = "Cruise: ";
    //                break;
    //            case AdvFunction.DIM:
    //                func = "DIM: ";
    //                break;
    //            case AdvFunction.EMITTER:
    //                func = "Emitter: ";
    //                break;
    //            case AdvFunction.ICL:
    //                func = "ICL: ";
    //                break;
    //            case AdvFunction.LEVEL1:
    //                func = "Level 1: ";
    //                break;
    //            case AdvFunction.LEVEL2:
    //                func = "Level 2: ";
    //                break;
    //            case AdvFunction.LEVEL3:
    //                func = "Level 3: ";
    //                break;
    //            case AdvFunction.LEVEL4:
    //                func = "Level 4: ";
    //                break;
    //            case AdvFunction.LEVEL5:
    //                func = "Level 5: ";
    //                break;
    //            case AdvFunction.STT_AND_TAIL:
    //                func = "STT&T: ";
    //                break;
    //            case AdvFunction.T13:
    //                func = "Title 13: ";
    //                break;
    //            case AdvFunction.TAKEDOWN:
    //                func = "Takedown: ";
    //                break;
    //            case AdvFunction.TRAFFIC:
    //                func = "Traffic: ";
    //                break;
    //            default:
    //                break;
    //        }
    //        t.text = func + value;
    //    }
    //}

    public void OnPointerClick(PointerEventData eventData) {
        //fsl.OnSelect(fn);
    }
}
