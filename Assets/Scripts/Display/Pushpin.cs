﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Deprecated.  Used to show the colors of the 3D version of the bar.
/// </summary>
public class Pushpin : MonoBehaviour {

    public Transform target;
    public Material defaultMaterial;
    private LightHead lh;
    private MeshRenderer mr;

    /// <summary>
    /// Start is called once, when the containing GameObject is instantiated, after Awake.
    /// </summary>
    void Start() {
        mr = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Update is called once each frame
    /// </summary>
    void Update() {
        if(BarManager.inst.funcBeingTested != AdvFunction.NONE) return;  // Quick & dirty optimization - if we're previewing a function, do nothing
        if(lh == null) {
            lh = target.GetComponent<LightHead>();
        } else {
            if(!lh.hasRealHead || !target.gameObject.activeInHierarchy) {
                mr.enabled = false;
            } else {
                mr.enabled = true;
                mr.materials[0].SetColor("_Color", lh.lhd.style.color);
                mr.materials[0].SetColor("_EmissionColor", lh.lhd.style.color * 0.2f);
                if(lh.lhd.style.isDualColor) {
                    mr.materials[1].SetColor("_Color", lh.lhd.style.color2);
                    mr.materials[1].SetColor("_EmissionColor", lh.lhd.style.color2 * 0.2f);
                } else {
                    mr.materials[1].SetColor("_Color", lh.lhd.style.color);
                    mr.materials[1].SetColor("_EmissionColor", lh.lhd.style.color * 0.2f);
                }
            }

            transform.position = target.position;
            Vector3 eAngles = transform.eulerAngles;
            eAngles.x = -90f;
            eAngles.y += 60f * Time.deltaTime;
            eAngles.z = 0f;
            transform.eulerAngles = eAngles;
        }
    }
}
