using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DebugHexagons : MonoBehaviour
{
	public HexCell[] prefabs;

	public int hexagonRadius = 4;

	HexCell[][] cells;

	Grid grid;
	Camera cam;

	bool canDrag = true, isDragging = false;
	int dragDir;
	List<HexCell> draggedCells = new List<HexCell>();
	List<Vector3Int> draggedIndices = new List<Vector3Int>();

	Coroutine snapAnim;

    // Start is called before the first frame update
    void Start()
    {
		cells = new HexCell[hexagonRadius*2+1][];
		cam = Camera.main;
		grid = GetComponent<Grid>();

        for(int j = -hexagonRadius; j <= hexagonRadius; ++j) {
			int minX = -hexagonRadius + Mathf.Abs(j)/2, maxX = hexagonRadius - (Mathf.Abs(j)+1) / 2;
			cells[j + hexagonRadius] = new HexCell[maxX - minX +1];
			for (int i = minX; i <= maxX; ++i) {
				var instance = Instantiate(prefabs[Random.Range(0,prefabs.Length)], grid.CellToWorld(new Vector3Int(i, j, 0)), Quaternion.identity, transform);
				//instance.GetComponentInChildren<TMPro.TextMeshPro>().text = "";
				//instance.GetComponent<SpriteRenderer>().color = i * j == 0 ? Color.white : i < 0 ? j < 0 ? Color.yellow : Color.red : j < 0 ? Color.blue : Color.green;
				cells[j+hexagonRadius][i-minX] = instance;
				instance.position = new Vector3Int(i, j, 0);

				// For Initial Check :
				draggedIndices.Add(new Vector3Int(i, j, 0));
				draggedCells.Add(instance);
			}
		}

		CheckMatches();
    }

    // Update is called once per frame
    void Update()
    {
		var pos = Input.mousePosition;
		pos = cam.ScreenToWorldPoint(pos);
		pos.z = 0;

		var hoveredCell = grid.WorldToCell(pos);
		//if (Input.GetMouseButtonDown(0)) {
		//	var rend = getCell(hoveredCell).GetComponent<SpriteRenderer>();
		//	if (rend.color == Color.white) rend.color = Color.green;
		//	else rend.color = Color.white;
		//}
		//foreach(var adj in GetAllAdjacentCells(hoveredCell)) {
		//	Debug.DrawLine(grid.CellToWorld(hoveredCell), grid.CellToWorld(adj), Color.red);
		//}

		if (isDragging) {

			Debug.DrawLine(debugA, debugB, Color.red);

			Debug.DrawLine(debugA, debugC, Color.green);
		}
    }

	Vector3 debugA, debugB, debugC;

	public void StartDrag(Vector2 position, Vector3 direction) {
		if (!canDrag) return;
		var startCell = grid.WorldToCell(cam.ScreenToWorldPoint(position));
		debugA = cam.ScreenToWorldPoint(position);
		debugA.z = 0;
		debugB = debugA + direction.normalized;
		Debug.DrawLine(debugA, debugB, Color.red);

		if (!inBounds(startCell)) {
			Debug.LogError("Drag dehors");
			return;
		}

		dragDir = getDirectionIdx(direction);

		isDragging = true;

		if(snapAnim != null) {
			StopCoroutine(snapAnim);
			SnapInstant();
		}

		draggedIndices.Clear();
		draggedIndices.Add(startCell);

		// Get all cells in drag direction
		var tmpCell = GetAdjacentCell(startCell, dragDir);
		while (inBounds(tmpCell)) {
			draggedIndices.Add(tmpCell);
			tmpCell = GetAdjacentCell(tmpCell, dragDir);
		}

		//Get all cells in opposite direction
		tmpCell = GetAdjacentCell(startCell, dragDir+3);
		while (inBounds(tmpCell)) {
			draggedIndices.Insert(0,tmpCell);
			tmpCell = GetAdjacentCell(tmpCell, dragDir+3);
		}

		draggedCells = draggedIndices.Select(getCell).ToList();

		UpdateDrag(direction);
	}

	public void UpdateDrag(Vector2 delta) {
		if (!isDragging) return;
		Vector3 dirVector = Vector2.right;
		switch (dragDir) {
			case 0: {
					dirVector = Vector2.right;
					break;
				}
			case 1: {
					dirVector = Utils.rotate(Vector2.right, Mathf.PI / 3);
					break;
				}
			case 2: {
					dirVector = Utils.rotate(Vector2.right, 2 * Mathf.PI / 3);
					break;
				}
		}
		if(Vector2.Dot(debugB-debugA,dirVector)>0)debugC = debugA + dirVector;
		else debugC = debugA - dirVector;

		var scaledDelta = cam.ScreenToWorldPoint(Vector3.forward+new Vector3(delta.x, delta.y,0))- cam.ScreenToWorldPoint(Vector3.forward);

		float dragDist = Vector2.Dot(scaledDelta, dirVector);

		foreach(var cell in draggedCells) {
			cell.transform.position += dirVector * dragDist;
		}

		while (!inBounds(grid.WorldToCell(draggedCells[0].transform.position))) {
			// Failsafe to be sure we're wrapping in the right direction
			if (!inBounds(grid.WorldToCell(draggedCells.Last().transform.position + dirVector))) break;

			var toMove = draggedCells[0];
			toMove.transform.position = draggedCells.Last().transform.position + dirVector;
			draggedCells.RemoveAt(0);
			draggedCells.Add(toMove);

			RefreshDraggedCells();
		}
		while (!inBounds(grid.WorldToCell(draggedCells.Last().transform.position))) {
			// Failsafe to be sure we're wrapping in the right direction
			if (!inBounds(grid.WorldToCell(draggedCells[0].transform.position - dirVector))) break;

			var toMove = draggedCells.Last();
			toMove.transform.position = draggedCells[0].transform.position - dirVector;
			draggedCells.RemoveAt(draggedCells.Count-1);
			draggedCells.Insert(0,toMove);

			RefreshDraggedCells();
		}

		RefreshDraggedCells();
	}

	public void StopDrag(Vector2 delta) {
		if (!isDragging) return;
		isDragging = false;
		snapAnim = StartCoroutine(SnapToSlots(.2f));
		CheckMatches();
	}

	IEnumerator SnapToSlots(float dur) {
		Vector3[] startPos = draggedCells.Select(e=>e.transform.position).ToArray();
		Vector3[] goalPos = startPos.Select(e=> grid.CellToWorld(grid.WorldToCell(e))).ToArray();
		for(float t=0; t<1; t += Time.deltaTime / dur) {
			for (int i = 0; i < draggedCells.Count; ++i) {
				draggedCells[i].transform.position = Vector3.Lerp(startPos[i], goalPos[i], t*t);
			}
			yield return null;
		}
		for (int i = 0; i < draggedCells.Count; ++i) {
			draggedCells[i].transform.position = goalPos[i];
		}
	}

	void CheckMatches() {
		var cellsToRemove = new HashSet<HexCell>();
		bool hasMatches = false;
		for(int i=0;i<draggedCells.Count;++i) {
			var tmpCells = new HashSet<HexCell>();

			var neighboursIdx = GetAllAdjacentCells(draggedIndices[i]);
			var neighbourCells = new HexCell[neighboursIdx.Length];
			for(int j = 0; j < neighboursIdx.Length; ++j) {
				if (inBounds(neighboursIdx[j])) neighbourCells[j] = getCell(neighboursIdx[j]);
			}

			if(draggedCells[i].Matching(neighbourCells,out tmpCells)) {
				hasMatches = true;

				cellsToRemove.Add(draggedCells[i]);
				cellsToRemove.UnionWith(tmpCells);
			}
		}

		if (hasMatches) {
			foreach(var cell in cellsToRemove) {
				Debug.Log(cell.position);
				cell.GetComponent<SpriteRenderer>().color -= new Color(.3f, 0, 0,0);
			}
		}
	}



	void SnapInstant() {
		foreach (var cell in draggedCells) {
			cell.transform.position = grid.CellToWorld(grid.WorldToCell(cell.transform.position));
		}
	}

	int lineOffset(int j) {
		return -hexagonRadius + Mathf.Abs(j) / 2;
	}

	bool inBounds(Vector3Int v) {
		//return v.x >= minX && v.x <= maxX && v.y >= minY && v.y <= maxY;
		try {
			var c = cells[v.y + hexagonRadius][v.x - lineOffset(v.y)];
		}
		catch {
			return false;
		}
		return true;
	}

	HexCell getCell(int x, int y) {
		return cells[y + hexagonRadius][x - lineOffset(y)];
	}

	HexCell getCell(Vector2Int v) {
		return getCell(v.x, v.y);
	}

	HexCell getCell(Vector3Int v) {
		return getCell(v.x, v.y);
	}

	void setCell(int x, int y, HexCell cell) {
		cells[y + hexagonRadius][x - lineOffset(y)] =cell;
		cell.position = new Vector3Int(x, y, 0);
	}

	void setCell(Vector2Int v, HexCell cell) {
		setCell(v.x, v.y, cell);
	}

	void setCell(Vector3Int v, HexCell cell) {
		setCell(v.x, v.y, cell);
	}

	void RefreshDraggedCells() {
		for(int i=0; i< draggedCells.Count; ++i) {
			setCell(draggedIndices[i], draggedCells[i]);
		}
	}

	Vector3Int[] GetAllAdjacentCells(Vector3Int cell) {
		var res = new Vector3Int[6];
		for(int i = 0; i < 6; ++i) {
			res[i] = GetAdjacentCell(cell, i);
		}

		return res;
	}

	Vector3Int GetAdjacentCell(Vector3Int cell, int idx) {
		while (idx < 0) idx += 6;
		idx = idx % 6;

		// Dans le sens trigonométrique

		switch (idx) {
			// Droite
			case 0: {
					return cell + Vector3Int.right;
				}
			//Haut-Droite
			case 1: {
					return cell + new Vector3Int(cell.y % 2 == 0 ? 0 : 1, 1, 0);
				}
			//Haut-gauche
			case 2: {
					return cell + new Vector3Int(cell.y % 2 == 0 ? -1 : 0, 1, 0);
				}
			// Gauche
			case 3: {
					return cell + Vector3Int.left;
				}
			// Bas-Gauche
			case 4: {
					return cell + new Vector3Int(cell.y % 2 == 0 ? -1 : 0, -1, 0);
				}
			//Bas-Droite
			case 5: {
					return cell + new Vector3Int(cell.y % 2 == 0 ? 0 : 1, -1, 0);
				}
		}
		throw new System.Exception("WHAT THE FUCK THO");
	}

	int getDirectionIdx(Vector2 direction) {
		int res;
		var pi = Mathf.PI;
		var angle = Mathf.Atan2(direction.y, direction.x);
		if (angle < -5 * pi / 6) {
			res = 0;
		}
		else if (angle < -3 * pi / 6) {
			res = 1;
		}
		else if (angle < -pi / 6) {
			res = 2;
		}
		else if (angle < pi / 6) {
			res = 0;
		}
		else if (angle < 3 * pi / 6) {
			res = 1;
		}
		else if (angle < 5 * pi / 6) {
			res = 2;
		}
		else res = 0;

		return res;
	}
}
