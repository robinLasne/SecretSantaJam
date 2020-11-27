using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellMatch{
	HashSet<HexCell> cellsInMatch = new HashSet<HexCell>();
	Vector3 center;
	bool centerInit = false;

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

	public CellMatch match;

	public abstract int type { get; }
	public abstract bool Matching(HexCell[] neighbours, out HashSet<HexCell> otherCells);
	public abstract bool ApplyMatch();
}
