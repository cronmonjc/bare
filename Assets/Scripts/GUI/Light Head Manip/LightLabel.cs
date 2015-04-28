using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightLabel : MonoBehaviour {
    public Transform target;
    public Text label;
    public Image background, secondImage;

    private LineRenderer lr;
    private LightHead lh;
    private Canvas c;

    public static CameraControl cam;

    void Start() {
        lr = background.GetComponent<LineRenderer>();
        c = GetComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingLayerName = "Light Label";
    }

    void Update() {
        if(cam == null) cam = FindObjectOfType<CameraControl>();
        else {
            transform.forward = cam.transform.forward;

            if(target != null) {
                transform.position = target.position;

                if(!target.gameObject.activeInHierarchy) {
                    gameObject.SetActive(false);
                }

                lr.SetVertexCount(2);
                lr.SetPosition(0, background.rectTransform.position);
                lr.SetPosition(1, target.position);
            }
        }

        if(lh == null) lh = target.GetComponent<LightHead>();
        else {
            if(lh.lhd.style != null && cam.Selected.Contains(lh)) {
                background.gameObject.SetActive(true);
                label.text = lh.lhd.style.name + " " + lh.lhd.optic.name;
                c.sortingOrder = Mathf.RoundToInt(Vector3.Distance(cam.transform.position, transform.position)) * -1;
            } else {
                background.gameObject.SetActive(false);
            }
        }
    }
}
