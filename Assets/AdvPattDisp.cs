﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AdvPattDisp : MonoBehaviour {
    public Text color1, color2;
    public Dictionary<AdvFunction, AdvFuncDisp> displays;

    void Start() {
        displays = new Dictionary<AdvFunction, AdvFuncDisp>();
        foreach(AdvFuncDisp alpha in GetComponentsInChildren<AdvFuncDisp>()) {
            displays[alpha.func] = alpha;
        }
    }

    void Refresh() {
        foreach(AdvFuncDisp alpha in displays.Values) {
            alpha.gameObject.SetActive(false);
        }

        HashSet<AdvFunction> funcs = new HashSet<AdvFunction>();
        funcs.Add(AdvFunction.DIM);

        foreach(LightHead alpha in AdvFuncDisp.cam.OnlyCamSelected) {
            foreach(BasicFunction beta in alpha.lhd.funcs) {
                switch(beta) {
                    case BasicFunction.CAL_STEADY:
                        funcs.Add(AdvFunction.T13);
                        break;
                    case BasicFunction.CRUISE:
                        funcs.Add(AdvFunction.CRUISE);
                        break;
                    case BasicFunction.EMITTER:
                        funcs.Add(AdvFunction.EMITTER);
                        break;
                    case BasicFunction.FLASHING:
                        foreach(AdvFunction charlie in new AdvFunction[] { AdvFunction.LEVEL1, AdvFunction.LEVEL2, AdvFunction.LEVEL3, AdvFunction.LEVEL4, AdvFunction.LEVEL5, AdvFunction.FTAKEDOWN, AdvFunction.FALLEY, AdvFunction.ICL }) {
                            funcs.Add(charlie);
                        }
                        break;
                    case BasicFunction.STEADY:
                        funcs.Add(AdvFunction.TAKEDOWN);
                        funcs.Add(AdvFunction.ALLEY_LEFT);
                        funcs.Add(AdvFunction.ALLEY_RIGHT);
                        break;
                    case BasicFunction.STT:
                        funcs.Add(AdvFunction.TURN_LEFT);
                        funcs.Add(AdvFunction.TURN_RIGHT);
                        funcs.Add(AdvFunction.TAIL);
                        break;
                    case BasicFunction.TRAFFIC:
                        funcs.Add(AdvFunction.TRAFFIC_LEFT);
                        funcs.Add(AdvFunction.TRAFFIC_RIGHT);
                        break;
                    default:
                        break;
                }
            }
        }

        foreach(AdvFunction alpha in funcs) {
            displays[alpha].gameObject.SetActive(true);
            displays[alpha].Refresh();
        }
    }
}