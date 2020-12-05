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

        if (!grown) return false;

		bool res = false;

		for(int i = 0; i < neighbours.Length; ++i) {
			HexCell a = neighbours[i], b = neighbours[(i + 1) % neighbours.Length];
            
			if (MatchesWith(a,b)) {
				res = true;
				otherCells.Add(a);
				otherCells.Add(b);

				CellMatch.Match(this, a, b);
			}
		}
		return res;
	}

    bool MatchesWith(HexCell a, HexCell b)
    {
        if ((a == null || !a.grown) || (b == null || !b.grown)) return false;

        List<int> types = new List<int>() { a.type, b.type, type };
        types.Sort();
        return (types[0] == 1 && types[1] == 2 && types[2] == 3) || (types[0] == 4 && types[1] == 5 && types[2] == 6);
    }

    public override void PreviewMatch(HexCell[] neighbours)
    {
        if (!grown) return;
        for (int i = 0; i < neighbours.Length; ++i)
        {
            HexCell a = neighbours[i], b = neighbours[(i + 1) % neighbours.Length];

            if (MatchesWith(a, b))
            {
                gonnaMatch = true;
                break;
            }
        }
    }

    public override bool ApplyMatch(float dur) {
		StartCoroutine(matchAnim(dur));
		return true;
	}
}
