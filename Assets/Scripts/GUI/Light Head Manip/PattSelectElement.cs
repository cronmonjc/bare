using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using fNbt;

public class PattSelectElement : MonoBehaviour, IPointerClickHandler {
    public Image image;

    public short selID;
    public PattSelect ps;

    public string Function {
        set {
            GetComponentInChildren<Text>().text = value;
        }
    }

    public bool PattActive {
        get {
            return image.enabled;
        }
        set {
            image.enabled = value;
        }
    }

    public void Retest() {
        NbtCompound patts = FindObjectOfType<BarManager>().patts;
        PattActive = true;
        foreach(LightBlock lb in FindObjectsOfType<LightBlock>()) {
            if(!lb.gameObject.activeInHierarchy || !lb.Selected) continue;
            LightHead lh = null;
            for(Transform t = lb.transform; lb == null && t != null; t = t.parent) {
                lh = t.GetComponent<LightHead>();
            }
            if(lh == null) {
                Debug.LogError("lolnope - " + lb.GetPath() + " can't find a LightHead.");
                ErrorText.inst.DispError(lb.GetPath() + " can't find a LightHead.");
                continue;
            }

            string cmpdName = BarManager.GetFnString(lb.transform, ps.f);
            if(cmpdName == null) {
                Debug.LogWarning("lolnope - " + ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                ErrorText.inst.DispError(ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                return;
            }
            NbtCompound patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat" + (lh.DualR == lb ? "2" : "1"));

            string tagname = lb.transform.position.z > 0 ? "r" : "f";
            string path = lh.transform.GetPath();

            if(path.Contains("Corner")) {
                tagname = tagname + "cor";
            } else if(path.Contains("Inboard")) {
                tagname = tagname + "inb";
            } else if(path.Contains("Outboard")) {
                if(lh.loc == Location.FAR_REAR)
                    tagname = tagname + "far";
                else
                    tagname = tagname + "oub";
            } else if(path.Contains("MidSection")) {
                tagname = tagname + "cen";
            }

            if(patCmpd.Get<NbtShort>(tagname).ShortValue != selID) {
                PattActive = false;
                return;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        PattSelectElement[] allpse = GetComponentsInChildren<PattSelectElement>();

        foreach(PattSelectElement alpha in allpse) {
            alpha.PattActive = (alpha == this);
        }

        NbtCompound patts = FindObjectOfType<BarManager>().patts;
        PattSelect ps = FindObjectOfType<PattSelect>();
        foreach(LightBlock lb in FindObjectsOfType<LightBlock>()) {
            if(!lb.gameObject.activeInHierarchy || !lb.Selected) continue;
            LightHead lh = null;
            for(Transform t = lb.transform; lb == null && t != null; t = t.parent) {
                lh = t.GetComponent<LightHead>();
            }
            if(lh == null) {
                Debug.LogError("lolnope - " + lb.GetPath() + " can't find a LightHead.");
                ErrorText.inst.DispError(lb.GetPath() + " can't find a LightHead.");
                continue;
            }

            string cmpdName = BarManager.GetFnString(lb.transform, ps.f);
            if(cmpdName == null) {
                Debug.LogWarning("lolnope - " + ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                ErrorText.inst.DispError(ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                return;
            }
            NbtCompound patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat" + (lh.DualR == lb ? "2" : "1"));

            string tagname = lb.transform.position.z > 0 ? "r" : "f";
            string path = lh.transform.GetPath();

            if(path.Contains("Corner")) {
                tagname = tagname + "cor";
            } else if(path.Contains("Inboard")) {
                tagname = tagname + "inb";
            } else if(path.Contains("Outboard")) {
                if(lh.loc == Location.FAR_REAR)
                    tagname = tagname + "far";
                else
                    tagname = tagname + "oub";
            } else if(path.Contains("MidSection")) {
                tagname = tagname + "cen";
            }

            patCmpd.Get<NbtShort>(tagname).Value = selID;
        }
    }
}
