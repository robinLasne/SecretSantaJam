using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCell : HexCell
{
	[SerializeField]
	private int m_type;

	public override int type {
		get {
			return m_type;
		}
	}

	public override bool Matching(HexCell[] neighbours, out HashSet<HexCell> otherCells) {
		otherCells = new HashSet<HexCell>();
		bool res = false;

		for(int i = 0; i < neighbours.Length; ++i) {
			HexCell a = neighbours[i], b = neighbours[(i + 1) % neighbours.Length];

			if (!grown || (a == null||!a.grown) || (b == null || !b.grown)) continue;

			List<int> types = new List<int>() { a.type, b.type, type };
			types.Sort();
			if ((types[0]==1 && types[1]==2 && types[2] == 3) || (types[0] == 4 && types[1] == 5 && types[2] == 6)) {
				res = true;
				otherCells.Add(a);
				otherCells.Add(b);

				CellMatch.Match(this, a, b);
			}
		}
		return res;
	}

	public override bool ApplyMatch(float dur) {
		StartCoroutine(matchAnim(dur));
		return true;
	}

	IEnumerator matchAnim(float dur) {
		var startPos = transform.position;
		var goalPos = match.getCenter();
		for(float t = 0; t < 1; t += Time.deltaTime / dur) {
			transform.position = Vector3.Lerp(startPos, goalPos, t);
			transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
			yield return null;
		}
		Destroy(gameObject);
	}
}
