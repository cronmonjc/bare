using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// UI Component.  Lists off all of the presets available, as well as loads them when requested.
/// </summary>
public class PresetList : MonoBehaviour {
    /// <summary>
    /// The single instance of this class
    /// </summary>
    public static PresetList inst;

    /// <summary>
    /// The menu upon which to place the elements.  Set via Unity Inspector.
    /// </summary>
    public RectTransform menu;
    /// <summary>
    /// The reference to the element prefab GameObject.  Set via Unity Inspector.
    /// </summary>
    public GameObject prefab;
    /// <summary>
    /// The reference to the "empty bar" option, kept so it doesn't get deleted.  Set via Unity Inspector.
    /// </summary>
    public Transform empty;

    /// <summary>
    /// Awake is called once, immediately as the object is created (typically at load time)
    /// </summary>
    void Awake() {
        if(inst == null) inst = this;
    }

    /// <summary>
    /// Clears the list.
    /// </summary>
    public void Clear() {
        List<Transform> temp = new List<Transform>();
        foreach(Transform alpha in menu) {
            if(alpha != empty)
                temp.Add(alpha);
        }
        foreach(Transform alpha in temp) {
            Destroy(alpha.gameObject);
        }
    }

    /// <summary>
    /// Called immediately when the Component's GameObject is enabled
    /// </summary>
    void OnEnable() {
        Clear();
        string[] presets = Directory.GetFiles(BarManager.DirRoot + "Presets");

        foreach(string preset in presets) {
            #region Create a new Button for each preset
            GameObject newbie = Instantiate<GameObject>(prefab);
            newbie.transform.SetParent(menu, false);
            newbie.transform.SetAsLastSibling();
            string[] bits = preset.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
            newbie.GetComponentInChildren<Text>().text = bits[bits.Length - 1].Split('.')[0];
            newbie.GetComponent<Button>().onClick.AddListener(delegate() {
                transform.parent.gameObject.SetActive(false);

                BarManager.inst.Open(string.Join("\\", bits));
                TitleText.inst.preset = bits[bits.Length - 1].Split('.')[0];
                TitleText.inst.currFile = "";
            }); 
            #endregion
        }
    }
}
