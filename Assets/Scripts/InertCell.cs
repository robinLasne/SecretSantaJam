using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertCell : HexCell
{
	public override int type {
		get {
			return 0;
		}
	}

	public override bool Matching(HexCell[] neighbours, out HashSet<HexCell> matches) {
		matches = null;
		
		return false;
	}

	public override bool ApplyMatch(float dur) {
		throw new System.NotImplementedException();
	}
}
