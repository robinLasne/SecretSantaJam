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

	public override bool Matching(HexCell[] neighbours, out HashSet<HexCell> matches) {
		matches = new HashSet<HexCell>();
		bool res = false;

		for(int i = 0; i < neighbours.Length; ++i) {
			HexCell a = neighbours[i], b = neighbours[(i + 1) % neighbours.Length];

			if (a == null || b == null) continue;

			List<int> types = new List<int>() { a.type, b.type, type };
			types.Sort();
			if (types[0]==1 && types[1]==2 && types[2] == 3) {
				res = true;
				matches.Add(a);
				matches.Add(b);
			}
		}
		return res;
	}
}
