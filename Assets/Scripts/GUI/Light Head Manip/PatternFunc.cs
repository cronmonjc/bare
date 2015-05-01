using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PatternFunc : MonoBehaviour, IPointerClickHandler {
    public FnSelManager fsl;
    public Text t;
    public Function fn;

    public string DispPattern {
        set {
            string func = "UnknownFn: ";
            switch(fn) {
                case Function.ALLEY:
                    func = "Alley: ";
                    break;
                case Function.CRUISE:
                    func = "Cruise: ";
                    break;
                case Function.DIM:
                    func = "DIM: ";
                    break;
                case Function.EMITTER:
                    func = "Emitter: ";
                    break;
                case Function.ICL:
                    func = "ICL: ";
                    break;
                case Function.LEVEL1:
                    func = "Level 1: ";
                    break;
                case Function.LEVEL2:
                    func = "Level 2: ";
                    break;
                case Function.LEVEL3:
                    func = "Level 3: ";
                    break;
                case Function.LEVEL4:
                    func = "Level 4: ";
                    break;
                case Function.LEVEL5:
                    func = "Level 5: ";
                    break;
                case Function.STT_AND_TAIL:
                    func = "STT&T: ";
                    break;
                case Function.T13:
                    func = "Title 13: ";
                    break;
                case Function.TAKEDOWN:
                    func = "Takedown: ";
                    break;
                case Function.TRAFFIC:
                    func = "Traffic: ";
                    break;
                default:
                    break;
            }
            t.text = func + value;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        fsl.OnSelect(fn);
    }
}
