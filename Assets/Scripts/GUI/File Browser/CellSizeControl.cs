using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CellSizeControl : MonoBehaviour {
    private GridLayoutGroup grid;

    public Vector2 CellSize {
        get {
            if(grid == null) grid = GetComponent<GridLayoutGroup>();
            return grid.cellSize;
        }
        set {
            if(grid == null) grid = GetComponent<GridLayoutGroup>();
            grid.cellSize = value;
            LayoutRebuilder.MarkLayoutForRebuild(grid.transform as RectTransform);
        }
    }

    public void SetSize(float to) {
        CellSize = new Vector2(48, 48) * to;
    }
}
