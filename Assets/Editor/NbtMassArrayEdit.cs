using UnityEngine;
using UnityEditor;
using fNbt;
using System.Text;

public class NbtMassArrayEdit : EditorWindow {
    public NbtByteArray byteTag;
    public NbtIntArray intTag;
    public string text;

    public static void EditArray(Rect buttonRect, NbtByteArray byteArray) {
        NbtMassArrayEdit win = ScriptableObject.CreateInstance<NbtMassArrayEdit>();
        win.title = "Byte Array";
        win.byteTag = byteArray;
        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < byteArray.Value.Length; i++) {
            if(i != 0) sb.Append(" ");
            sb.Append(byteArray.Value[i]);
        }
        win.text = sb.ToString();
        win.ShowAsDropDown(buttonRect, new Vector2(512, 256));
    }

    public static void EditArray(Rect buttonRect, NbtIntArray intArray) {
        NbtMassArrayEdit win = ScriptableObject.CreateInstance<NbtMassArrayEdit>();
        win.title = "Int Array";
        win.intTag = intArray;
        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < intArray.Value.Length; i++) {
            if(i != 0) sb.Append(" ");
            sb.Append(intArray.Value[i]);
        }
        win.text = sb.ToString();
        win.ShowAsDropDown(buttonRect, new Vector2(512, 256));
    }

    void OnGUI() {
        if(GUI.Button(new Rect(10, position.height - 26, position.width - 20, 16), "Apply")) {
            string[] vals = text.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
            if(byteTag != null) {
                try {
                    byteTag.Value = new byte[vals.Length];
                    for(int i = 0; i < vals.Length; i++) {
                        byteTag.Value[i] = byte.Parse(vals[i]);
                    }
                    Close();
                } catch(System.FormatException) {
                    ShowNotification(new GUIContent("Cannot parse text to a byte array.  Please don't use non-numeric characters."));
                } catch(System.OverflowException) {
                    ShowNotification(new GUIContent("Cannot parse text to a byte array.  Valid bytes range from 0 to 255."));
                }
            } else if(intTag != null) {
                try {
                    intTag.Value = new int[vals.Length];
                    for(int i = 0; i < vals.Length; i++) {
                        intTag.Value[i] = int.Parse(vals[i]);
                    }
                    Close();
                } catch(System.FormatException) {
                    ShowNotification(new GUIContent("Cannot parse text to an integer array.  Please don't use non-numeric characters."));
                } catch(System.OverflowException) {
                    ShowNotification(new GUIContent("Cannot parse text to an integer array.  Some numbers are too large to fit in an int."));
                }
            } else {
                throw new System.ArgumentNullException("Window does not have a tag to assign value to!");
            }
        }
        if(byteTag != null || intTag != null) EditorGUI.LabelField(new Rect(10, 10, position.width - 20, 16), "Editing " + (byteTag == null ? intTag.Name : byteTag.Name));
        text = EditorGUI.TextArea(new Rect(10, 26, position.width - 20, position.height - 52), text);
    }
}