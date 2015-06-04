using UnityEngine;
using UnityEngine.UI;
using fNbt;

public class PattSelectElement : MonoBehaviour {
    public Image image;

    public short selID;
    public PattSelect ps;

    public string FuncText {
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
        PattActive = ps.TestPatternAll(selID);
    }

    public void Clicked() {
        NbtCompound patts = FindObjectOfType<BarManager>().patts;
        PattSelect ps = FindObjectOfType<PattSelect>();
        foreach(LightBlock lb in FindObjectsOfType<LightBlock>()) {
            if(!lb.gameObject.activeInHierarchy || !lb.Selected) continue;
            LightHead lh = null;
            for(Transform t = lb.transform; lh == null && t != null; t = t.parent) {
                lh = t.GetComponent<LightHead>();
            }
            if(lh == null) {
                Debug.LogError("lolnope - " + lb.GetPath() + " can't find a LightHead.", lb);
                ErrorText.inst.DispError(lb.GetPath() + " can't find a LightHead.");
                continue;
            }

            if(ps.f != AdvFunction.TRAFFIC && ps.f != AdvFunction.DIM) {
                string cmpdName = BarManager.GetFnString(lb.transform, ps.f);
                if(cmpdName == null) {
                    Debug.LogWarning("lolnope - " + ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                    ErrorText.inst.DispError(ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                    return;
                }
                NbtCompound patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat1");

                string tagname = lb.transform.position.z < 0 ? "r" : "f";
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

                patCmpd = patts.Get<NbtCompound>(cmpdName).Get<NbtCompound>("pat2");
                patCmpd.Get<NbtShort>(tagname).Value = selID;
            } else if(ps.f == AdvFunction.TRAFFIC) {
                NbtCompound patCmpd = patts.Get<NbtCompound>("traf").Get<NbtCompound>("patt");
                patCmpd.Get<NbtShort>("l").Value = selID;
                patCmpd.Get<NbtShort>("r").Value = selID;
                patCmpd.Get<NbtShort>("c").Value = selID;
            }
        }

        PattSelectElement[] allpse = ps.GetComponentsInChildren<PattSelectElement>();

        foreach(PattSelectElement alpha in allpse) {
            alpha.Retest();
        }

        FnSelManager.inst.RefreshLabels();
        ps.RelabelEnable();
    }
}
