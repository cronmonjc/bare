using UnityEngine;
using System.Collections;

public class BarSizeDisplay : MonoBehaviour {
    public void SetSize(float to) {
        string text = "S????";
        switch(Mathf.RoundToInt(to)) {
            case 0:
                text = "S1300";
                break;
            case 1:
                text = "S1400";
                break;
            case 2:
                text = "S1500";
                break;
            case 3:
                text = "S1550";
                break;
            case 4:
                text = "S1xxx";
                break;
            default:
                text = "S????";
                break;
        }
        GetComponent<UnityEngine.UI.Text>().text = text;
    }
}
