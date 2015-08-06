using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class PresetList : MonoBehaviour {
    public static PresetList inst;

    public RectTransform menu;
    public GameObject prefab;
    public Transform empty;

    /// <summary>
    /// Awake is called once, immediately as the object is created (typically at load time)
    /// </summary>
    void Awake() {
        if(inst == null) inst = this;
    }

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

    void OnEnable() {
        Clear();
        string[] presets = Directory.GetFiles(BarManager.DirRoot + "Presets");

        foreach(string preset in presets) {
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
        }
    }
}
