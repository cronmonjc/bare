﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using fNbt;

public class DisableEnablePatt : MonoBehaviour, IPointerClickHandler {
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

    public void Retest() {
        NbtCompound patts = FindObjectOfType<BarManager>().patts;
        PattSelect ps = FindObjectOfType<PattSelect>();
        bool show = true;
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
            NbtCompound func = patts.Get<NbtCompound>(cmpdName);

            NbtShort en = func.Get<NbtShort>("en" + (lb.transform.position.z > 0 ? "r" : "f") + (lh.DualR == lb ? "2" : "1"));

            string path = lh.transform.GetPath();
            byte bit = 16;
            if(lh.transform.position.x < 0) {
                if(path.Contains("MidSection")) {
                    // Center L, bit 5
                    bit = 5;
                } else if(path.Contains("Corner")) {
                    // Corner L, bit 0
                    bit = 0;
                } else if(path.Contains("Outboard")) {
                    if(lh.loc == Location.FAR_REAR) {
                        // Outboard Far L, bit 1
                        bit = 1;
                    } else {
                        // Outboard Near L, bit 2
                        bit = 2;
                    }
                } else if(path.Contains("Alley")) {
                    // Left Alley, bit 12
                    bit = 12;
                }
            } else {
                if(path.Contains("MidSection")) {
                    // Center R, bit 6
                    bit = 6;
                } else if(path.Contains("Corner")) {
                    // Corner L, bit 11
                    bit = 11;
                } else if(path.Contains("Outboard")) {
                    if(lh.loc == Location.FAR_REAR) {
                        // Outboard Far R, bit 2
                        bit = 10;
                    } else {
                        // Outboard Near R, bit 2
                        bit = 9;
                    }
                } else if(path.Contains("Alley")) {
                    // Right Alley, bit 12
                    bit = 13;
                }
            }

            if(bit < 16) {
                if(IsEnable)
                    show = show && ((en.Value & (0x1 << bit)) > 0);
                else
                    show = show && ((en.Value & (0x1 << bit)) == 0);
            }
        }
        Active = show;
    }

    public void OnPointerClick(PointerEventData eventData) {
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
            NbtCompound func = patts.Get<NbtCompound>(cmpdName);

            NbtShort en = func.Get<NbtShort>("en" + (lh.transform.position.z > 0 ? "r" : "f") + (lh.DualR == lb ? "2" : "1"));

            string path = lh.transform.GetPath();
            byte bit = 16;
            if(lh.transform.position.x < 0) {
                if(path.Contains("MidSection")) {
                    // Center L, bit 5
                    bit = 5;
                } else if(path.Contains("Corner")) {
                    // Corner L, bit 0
                    bit = 0;
                } else if(path.Contains("Outboard")) {
                    if(lh.loc == Location.FAR_REAR) {
                        // Outboard Far L, bit 1
                        bit = 1;
                    } else {
                        // Outboard Near L, bit 2
                        bit = 2;
                    }
                } else if(path.Contains("Alley")) {
                    // Left Alley, bit 12
                    bit = 12;
                }
            } else {
                if(path.Contains("MidSection")) {
                    // Center R, bit 6
                    bit = 6;
                } else if(path.Contains("Corner")) {
                    // Corner L, bit 11
                    bit = 11;
                } else if(path.Contains("Outboard")) {
                    if(lh.loc == Location.FAR_REAR) {
                        // Outboard Far R, bit 2
                        bit = 10;
                    } else {
                        // Outboard Near R, bit 2
                        bit = 9;
                    }
                } else if(path.Contains("Alley")) {
                    // Right Alley, bit 12
                    bit = 13;
                }
            }

            if(bit < 16) {
                if(IsEnable)
                    en.EnableBit(bit);
                else
                    en.DisableBit(bit);
            }
        }

        foreach(DisableEnablePatt dep in transform.parent.GetComponentsInChildren<DisableEnablePatt>(true)) {
            Active = (dep == this);
        }
    }
}