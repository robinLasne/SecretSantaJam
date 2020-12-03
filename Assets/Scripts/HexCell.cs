using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellMatch{
	HashSet<HexCell> cellsInMatch = new HashSet<HexCell>();
	Vector3 center;
	bool centerInit = false;

    public int score {
        get {
            return cellsInMatch.Count - 2;
        }
    }

	public Vector3 getCenter() {
		if (!centerInit) {
			centerInit = true;
			Vector3 sum = Vector3.zero;
			foreach(var cell in cellsInMatch) {
				sum += cell.transform.position;
			}
			center = sum / cellsInMatch.Count;
		}
		return center;
	}

	public static void Match(params HexCell[] thisMatch) {
		CellMatch finalMatch = new CellMatch();
		foreach(var cell in thisMatch) {
			finalMatch.cellsInMatch.Add(cell);
		}

		foreach (var cell in thisMatch) {
			if(cell.match != null) finalMatch.cellsInMatch.UnionWith(cell.match.cellsInMatch);
		}

		foreach(var cell in finalMatch.cellsInMatch) {
			cell.match = finalMatch;
		}
	}
}

public abstract class HexCell : MonoBehaviour
{
	public Vector3Int position { get; set; }
	public Vector3Int goalPosition { get; set; }

    public bool soonConsumed { get; set; }

	public CellMatch match;
    protected bool m_grown;
    public bool grown {
        get {
            return m_grown;
        }
    }

    public SpriteRenderer backGround;
    public SpriteRenderer content;
    public SpriteRenderer debug;
    Color debugColor;
    int bgOrder, frontOrder;

	public abstract int type { get; }
	public abstract bool Matching(HexCell[] neighbours, out HashSet<HexCell> otherCells);
	public abstract bool ApplyMatch(float dur);

    public void Grow(float t)
    {
        if (t > 0) { m_grown = true; debug.color = debugColor; }
        content.transform.localScale = t * Vector3.one;
    }

    private void Awake()
    {
        //content.transform.localScale = Vector3.zero;
        bgOrder = backGround.sortingOrder;
        frontOrder = content.sortingOrder;

        debugColor = debug.color;
        debug.color = Color.white;
    }

    public void SetInFront(bool state)
    {
        if (backGround == null) {
            Debug.Log("null renderer", this);
            return;
        }
        backGround.sortingOrder = bgOrder + (state ? 2 : 0);
        content.sortingOrder = frontOrder + (state ? 2 : 0);
    }
}
