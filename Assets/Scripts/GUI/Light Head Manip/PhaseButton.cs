using UnityEngine;
using fNbt;

public class PhaseButton : MonoBehaviour {
    public UnityEngine.UI.Image image;
    public bool IsPhaseB;

    public bool Active {
        get {
            return image.enabled;
        }
        set {
            image.enabled = value;
        }
    }

    public void Retest() {
        PattSelect ps = FindObjectOfType<PattSelect>();
        if(LightDict.inst.steadyBurn.Contains(ps.f) || ps.f == AdvFunction.DIM) {
            Active = false;
            return;
        }

        NbtCompound patts = FindObjectOfType<BarManager>().patts;
        bool show = true;
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

            string cmpdName = BarManager.GetFnString(lb.transform, ps.f);
            if(cmpdName == null) {
                Debug.LogWarning("lolnope - " + ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                ErrorText.inst.DispError(ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                return;
            }
            NbtCompound func = patts.Get<NbtCompound>(cmpdName);

            short ph = func.Get<NbtShort>("p" + (lb.transform.position.z < 0 ? "r" : "f") + "1").ShortValue;
            ph |= func.Get<NbtShort>("p" + (lb.transform.position.z < 0 ? "r" : "f") + "2").ShortValue;

            if(IsPhaseB)
                show &= ((ph & (0x1 << lh.Bit)) > 0);
            else
                show &= ((ph & (0x1 << lh.Bit)) == 0);
        }
        Active = show;
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

            string cmpdName = BarManager.GetFnString(lb.transform, ps.f);
            if(cmpdName == null) {
                Debug.LogWarning("lolnope - " + ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                ErrorText.inst.DispError(ps.f.ToString() + " has no similar setting in the data bytes.  Ask James.");
                return;
            }
            NbtCompound func = patts.Get<NbtCompound>(cmpdName);

            NbtShort ph = func.Get<NbtShort>("p" + (lb.transform.position.z < 0 ? "r" : "f") + "1");

            if(IsPhaseB)
                ph.EnableBit(lh.Bit);
            else
                ph.DisableBit(lh.Bit);

            ph = func.Get<NbtShort>("p" + (lb.transform.position.z < 0 ? "r" : "f") + "2");

            if(IsPhaseB)
                ph.EnableBit(lh.Bit);
            else
                ph.DisableBit(lh.Bit);
        }

        ps.PhaseA.Retest();
        ps.PhaseB.Retest();

        FnSelManager.inst.RefreshLabels();
    }
}
