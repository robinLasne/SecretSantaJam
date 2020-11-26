using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HexCell : MonoBehaviour
{
	public Vector3Int position { get; set; }

	public abstract int type { get; }
	public abstract bool Matching(HexCell[] neighbours, out HashSet<HexCell> matches);
}
