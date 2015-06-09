using UnityEngine;
using fNbt;

public class DisableEnablePatt : MonoBehaviour {
    public UnityEngine.UI.Image image;
    public bool IsEnable;

    public bool Active {
        get {
            return image.enabled;
        }
        set {
            image.enabled = value;
        }
    }

    public string text {
        get {
            return GetComponentInChildren<UnityEngine.UI.Text>().text;
        }
        set {
            GetComponentInChildren<UnityEngine.UI.Text>().text = value;
        }
    }

    public void Retest() {
        NbtCompound patts = BarManager.inst.patts;
        PattSelect ps = FindObjectOfType<PattSelect>();
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

            short en = func.Get<NbtShort>("e" + (lb.transform.position.z < 0 ? "r" : "f") + "1").ShortValue;
            en |= func.Get<NbtShort>("e" + (lb.transform.position.z < 0 ? "r" : "f") + "2").ShortValue;
            
            if(IsEnable)
                show = show && ((en & (0x1 << lh.Bit)) > 0);
            else
                show = show && ((en & (0x1 << lh.Bit)) == 0);
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

            NbtShort en = func.Get<NbtShort>("e" + (lh.transform.position.z < 0 ? "r" : "f") + "1");

            if(IsEnable)
                en.EnableBit(lh.Bit);
            else
                en.DisableBit(lh.Bit);
            
            en = func.Get<NbtShort>("e" + (lh.transform.position.z < 0 ? "r" : "f") + "2");

            if(IsEnable)
                en.EnableBit(lh.Bit);
            else
                en.DisableBit(lh.Bit);
        }

        ps.enableButton.Retest();
        ps.disableButton.Retest();

        FnSelManager.inst.RefreshLabels();
    }
}
