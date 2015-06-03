using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// GUI Item.  Handles display of a specific section of the BOM, as defined by a value from <see cref="BOMControl.BOMType"/>.
/// </summary>
public class BOMControl : MonoBehaviour {

    /// <summary>
    /// Describes the type of BOM section in use.
    /// </summary>
    public enum BOMType {
        Lights, Lenses, Flashers
    }

    /// <summary>
    /// Is this controlling the lights?
    /// </summary>
    public BOMType type;

    /// <summary>
    /// The elements tracked by this control.  Key is the part number, value is the element.
    /// </summary>
    private Dictionary<string, BOMElement> elements;

    private Dictionary<string, int> counts;
    private Dictionary<string, LightHead> descs; 

    /// <summary>
    /// The "unconfigured" element.
    /// </summary>
    public BOMElement unconfigured;

    /// <summary>
    /// Prefab of an element.
    /// </summary>
    public GameObject elementPrefab;

    void Awake() {
        elements = new Dictionary<string, BOMElement>();
        counts = new Dictionary<string, int>();
        if(type == BOMType.Lights) {
            descs = new Dictionary<string, LightHead>();
        }
    }

    void Update() {
        List<string> parts = new List<string>();
        foreach(string alpha in new List<string>(counts.Keys)) {
            counts[alpha] = 0;
        }

        if(type == BOMType.Lights) {
            foreach(string alpha in new List<string>(descs.Keys)) {
                descs[alpha] = null;
            }
            int unconfig = 0;
            foreach(LightHead lh in BarManager.inst.allHeads) {
                if(lh.gameObject.activeInHierarchy) {
                    if(lh.lhd.style != null) {
                        string part = lh.PartNumber;
                        if(parts.Contains(part)) {
                            counts[part]++;
                        } else {
                            counts[part] = 1;
                            descs[part] = lh;
                            parts.Add(part);
                        }
                    } else {
                        unconfig++;
                    }
                }
            }

            unconfigured.quantity = unconfig;
            unconfigured.gameObject.SetActive(unconfig > 0);

            foreach(string alpha in parts) {
                if(elements.ContainsKey(alpha)) {
                    elements[alpha].quantity = counts[alpha];
                    elements[alpha].headToDescribe = descs[alpha];
                } else {
                    BOMElement newbie = AddItem(alpha);
                    newbie.type = type;
                    newbie.headToDescribe = descs[alpha];
                    newbie.quantity = counts[alpha];
                }
            }
            foreach(string alpha in new List<string>(elements.Keys)) {
                if(!parts.Contains(alpha)) {
                    RemoveItem(alpha);
                }
            }
        //} else if(type == BOMType.Lenses) {
        //    LightLensControl[] llcs = GameObject.FindObjectsOfType<LightLensControl>();

        //    Dictionary<string, LightLensControl> descs = new Dictionary<string, LightLensControl>();
        //    int unconfig = 0;
        //    foreach(LightLensControl llc in llcs) {
        //        if(llc.GetComponent<Renderer>().enabled) {
        //            if(llc.myLensOption != null) {
        //                string part = (llc.side ? MainCameraControl.sideLensPart : MainCameraControl.midLensPart) + llc.myLensOption.part + (MainCameraControl.clearCoat ? "-C" : "");
        //                if(counts.ContainsKey(part)) {
        //                    counts[part]++;
        //                } else {
        //                    counts[part] = 1;
        //                    descs[part] = llc;
        //                    parts.Add(part);
        //                }
        //            } else {
        //                unconfig++;
        //            }
        //        }
        //    }

        //    unconfigured.quantity = unconfig;
        //    unconfigured.gameObject.SetActive(unconfig > 0);

        //    foreach(string alpha in parts) {
        //        if(elements.ContainsKey(alpha)) {
        //            elements[alpha].quantity = counts[alpha];
        //            elements[alpha].lensToDescribe = descs[alpha];
        //        } else {
        //            BOMElement newbie = AddItem(alpha);
        //            newbie.type = type;
        //            newbie.lensToDescribe = descs[alpha];
        //            newbie.quantity = counts[alpha];
        //        }
        //    }
        //    foreach(string alpha in new List<string>(elements.Keys)) {
        //        if(!counts.ContainsKey(alpha)) {
        //            RemoveItem(alpha);
        //        }
        //    }
        //} else if(type == BOMType.FlasherBundles) {
        //    string[] wireClusters = MainCameraControl.GetFlasherBundles();
        //    HashSet<string> uniqueClusters = new HashSet<string>(wireClusters);
        //    foreach(string alpha in wireClusters) {
        //        if(counts.ContainsKey(alpha)) {
        //            counts[alpha]++;
        //        } else {
        //            counts[alpha] = 1;
        //        }
        //    }

        //    foreach(string alpha in uniqueClusters) {
        //        if(elements.ContainsKey(alpha)) {
        //            elements[alpha].quantity = counts[alpha];
        //            elements[alpha].bundleToDescribe = alpha;
        //        } else {
        //            BOMElement newbie = AddItem(alpha);
        //            newbie.type = type;
        //            newbie.bundleToDescribe = alpha;
        //            newbie.quantity = counts[alpha];
        //        }
        //    }
        //    foreach(string alpha in new List<string>(elements.Keys)) {
        //        if(!counts.ContainsKey(alpha)) {
        //            RemoveItem(alpha);
        //        }
        //    }


        }
    }

    /// <summary>
    /// Creates a <see cref="BOMElement"/> <see cref="UnityEngine.GameObject"/> and returns it for more processing.
    /// </summary>
    /// <param name="partNum">The part number of the element being created.</param>
    /// <returns>The new BOMElement.</returns>
    public BOMElement AddItem(string partNum) {
        if(elements.ContainsKey(partNum)) {
            return elements[partNum];
        } else {
            int childIndex = transform.GetSiblingIndex() + 2 + elements.Count; // Always place the new element at the bottom of the section
            GameObject newbie = GameObject.Instantiate(elementPrefab) as GameObject;
            newbie.transform.SetParent(transform.parent, false);
            newbie.transform.SetSiblingIndex(childIndex);
            newbie.transform.localScale = Vector3.one;
            BOMElement newelem = newbie.GetComponent<BOMElement>();
            elements[partNum] = newelem;
            return newelem;
        }
    }

    /// <summary>
    /// Destroys a BOMElement by part number.
    /// </summary>
    /// <param name="partNum">The part number of the element to be destroyed.</param>
    public void RemoveItem(string partNum) {
        if(elements.ContainsKey(partNum)) {
            Destroy(elements[partNum].gameObject);
            elements.Remove(partNum);
        }
    }
}