using UnityEngine;
using System.Collections;

public class Pushpin : MonoBehaviour {

    public Transform target;
    public Material defaultMaterial;
    private LightHead lh;
    private MeshRenderer mr;

    // Use this for initialization
    void Start() {
        mr = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update() {
        if(CameraControl.funcBeingTested != AdvFunction.NONE) return;
        if(lh == null) {
            lh = target.GetComponent<LightHead>();
        } else {
            if(lh.lhd.style == null || !target.gameObject.activeInHierarchy) {
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
