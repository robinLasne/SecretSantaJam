﻿using System.Collections;
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
	HexCell draggedCell;
	Vector3Int draggedCellStartPos;
	List<HexCell> draggedCellsLine = new List<HexCell>();
	List<Vector3Int> draggedIndices = new List<Vector3Int>();
	HexCell[] ghostWrapCells = new HexCell[2];

	Coroutine snapAnim;

	Vector2[] directionByIndex;

    // Start is called before the first frame update
    void Start()
    {
		directionByIndex = new Vector2[]{ Vector2.right,Utils.rotate(Vector2.right, Mathf.PI / 3),Utils.rotate(Vector2.right, 2 * Mathf.PI / 3)};

		cells = new HexCell[hexagonRadius*2+1][];
		cam = Camera.main;
		grid = GetComponent<Grid>();

        for(int j = -hexagonRadius; j <= hexagonRadius; ++j) {
			int minX = -hexagonRadius + Mathf.Abs(j)/2, maxX = hexagonRadius - (Mathf.Abs(j)+1) / 2;
			cells[j + hexagonRadius] = new HexCell[maxX - minX +1];
			for (int i = minX; i <= maxX; ++i) {
				var position = new Vector3Int(i, j, 0);

				var instance = PlaceNewCell(Random.Range(0, prefabs.Length), position);

				// For Initial Check :
				draggedIndices.Add(position);
				draggedCellsLine.Add(instance);
			}
		}

		CheckMatches();
    }

	HexCell PlaceNewCell(int type, Vector3Int position) {
		var instance = Instantiate(prefabs[type], grid.CellToWorld(position), Quaternion.identity, transform);
		setCell(position, instance);
		instance.goalPosition = position;
		return instance;
	}

	// Update is called once per frame
	void Update()
    {
		var pos = Input.mousePosition;
		pos = cam.ScreenToWorldPoint(pos);
		pos.z = 0;

		var hoveredCell = grid.WorldToCell(pos);
    }

	public void StartDrag(Vector2 position, Vector3 direction) {
		if (!canDrag) return;
		draggedCellStartPos = grid.WorldToCell(cam.ScreenToWorldPoint(position));

		if (!inBounds(draggedCellStartPos)) {
			Debug.LogError("Drag dehors");
			return;
		}

		draggedCellStartPos.z = 0;
		draggedCell = getCell(draggedCellStartPos);
		
		dragDir = getDirectionIdx(direction);

		isDragging = true;

		if(snapAnim != null) {
			StopCoroutine(snapAnim);
			SnapInstant();
		}

		draggedIndices.Clear();
		draggedIndices.Add(draggedCellStartPos);

		// Get all cells in drag direction
		var tmpCell = GetAdjacentCell(draggedCellStartPos, dragDir);
		while (inBounds(tmpCell)) {
			draggedIndices.Add(tmpCell);
			tmpCell = GetAdjacentCell(tmpCell, dragDir);
		}

		//Get all cells in opposite direction
		tmpCell = GetAdjacentCell(draggedCellStartPos, dragDir+3);
		while (inBounds(tmpCell)) {
			draggedIndices.Insert(0,tmpCell);
			tmpCell = GetAdjacentCell(tmpCell, dragDir+3);
		}

		draggedCellsLine = draggedIndices.Select(getCell).ToList();

		DestroyGhostCells();
		GenerateGhostCells();

		UpdateDrag(direction);
	}

	public void UpdateDrag(Vector2 delta) {
		if (!isDragging) return;
		Vector3 dirVector = directionByIndex[dragDir];

		var scaledDelta = cam.ScreenToWorldPoint(Vector3.forward+new Vector3(delta.x, delta.y,0))- cam.ScreenToWorldPoint(Vector3.forward);

		float dragDist = Vector2.Dot(scaledDelta, dirVector);

		foreach(var cell in draggedCellsLine) {
			cell.transform.position += dirVector * dragDist;
		}

		WrapDraggedCells(dirVector);
	}

	public IEnumerator StopDrag(Vector2 delta) {
		if (!isDragging) yield break;
		isDragging = false;
		if (CheckMatches()) {
			snapAnim = StartCoroutine(SnapToSlots(.2f));
		}
		else {
			snapAnim = StartCoroutine(SnapToStart(.2f));
		}
		yield return snapAnim;
		DestroyGhostCells();
	}

	IEnumerator SnapToSlots(float dur) {
		Vector3[] startPos = draggedCellsLine.Select(e=>e.transform.position).ToArray();
		Vector3[] goalPos = startPos.Select(e=> grid.CellToWorld(grid.WorldToCell(e))).ToArray();
		for(float t=0; t<1; t += Time.deltaTime / dur) {
			for (int i = 0; i < draggedCellsLine.Count; ++i) {
				draggedCellsLine[i].transform.position = Vector3.Lerp(startPos[i], goalPos[i], t*t);
			}
			yield return null;
		}
		for (int i = 0; i < draggedCellsLine.Count; ++i) {
			draggedCellsLine[i].transform.position = goalPos[i];
		}
	}

	IEnumerator SnapToStart(float dur) {
		Vector3 startPos = draggedCell.transform.position;
		Vector3 goalPos = grid.CellToWorld(draggedCellStartPos);
		goalPos.z = 0;
		Vector2 direction = directionByIndex[dragDir];
		for (float t = 0; t < 1; t += Time.deltaTime / dur) {
			MoveDraggedCellTo(Vector3.Lerp(startPos, goalPos, t * t), direction);
			yield return null;
		}
		MoveDraggedCellTo(goalPos, direction);
	}

	void MoveDraggedCellTo(Vector3 position, Vector2 direction) {
		Vector3 delta = draggedCell.transform.position;
		draggedCell.transform.position = position;
		delta -= draggedCell.transform.position;
		for (int i = 0; i < draggedCellsLine.Count; ++i) {
			if (draggedCellsLine[i] != draggedCell) draggedCellsLine[i].transform.position -= delta;
		}
		WrapDraggedCells(direction);
	}

	void WrapDraggedCells(Vector3 dirVector) {
		while (!inBounds(grid.WorldToCell(draggedCellsLine[0].transform.position))) {
			// Failsafe to be sure we're wrapping in the right direction
			if (!inBounds(grid.WorldToCell(draggedCellsLine.Last().transform.position + dirVector))) break;

			var toMove = draggedCellsLine[0];
			toMove.transform.position = draggedCellsLine.Last().transform.position + dirVector;
			draggedCellsLine.RemoveAt(0);
			draggedCellsLine.Add(toMove);

			RefreshDraggedCells();
		}
		while (!inBounds(grid.WorldToCell(draggedCellsLine.Last().transform.position))) {
			// Failsafe to be sure we're wrapping in the right direction
			if (!inBounds(grid.WorldToCell(draggedCellsLine[0].transform.position - dirVector))) break;

			var toMove = draggedCellsLine.Last();
			toMove.transform.position = draggedCellsLine[0].transform.position - dirVector;
			draggedCellsLine.RemoveAt(draggedCellsLine.Count - 1);
			draggedCellsLine.Insert(0, toMove);

			RefreshDraggedCells();
		}

		ghostWrapCells[0].transform.position = draggedCellsLine[0].transform.position - dirVector;
		ghostWrapCells[1].transform.position = draggedCellsLine.Last().transform.position + dirVector;
	}

	bool CheckMatches() {
		var cellsToRemove = new HashSet<HexCell>();
		bool hasMatches = false;
		for(int i=0;i<draggedCellsLine.Count;++i) {
			var tmpCells = new HashSet<HexCell>();

			var neighboursIdx = GetAllAdjacentCells(draggedIndices[i]);
			var neighbourCells = new HexCell[neighboursIdx.Length];
			for(int j = 0; j < neighboursIdx.Length; ++j) {
				if (inBounds(neighboursIdx[j])) neighbourCells[j] = getCell(neighboursIdx[j]);
			}

			if(draggedCellsLine[i].Matching(neighbourCells,out tmpCells)) {
				hasMatches = true;

				cellsToRemove.Add(draggedCellsLine[i]);
				cellsToRemove.UnionWith(tmpCells);
			}
		}

		if (hasMatches) {
			foreach(var cell in cellsToRemove) {
				if (cell.ApplyMatch()) {
					var newCell = PlaceNewCell(0, cell.position);
					newCell.transform.position = cell.transform.position;

					if (cell == draggedCell) {
						draggedCell = newCell;
					}
					var i = draggedCellsLine.IndexOf(cell);
					if (i >= 0) {
						draggedCellsLine[i] = newCell;
					}

					//Destroy(cell.gameObject);
				}
				
			}

			for(int i=0; i<draggedIndices.Count; ++i) {
				draggedCellsLine[i].goalPosition = draggedIndices[i];
			}
		}

		return hasMatches;
	}



	void SnapInstant() {
		foreach (var cell in draggedCellsLine) {
			setCell(cell.goalPosition, cell);
			cell.transform.position = grid.CellToWorld(cell.goalPosition);
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
		for(int i=0; i< draggedCellsLine.Count; ++i) {
			setCell(draggedIndices[i], draggedCellsLine[i]);
		}
		DestroyGhostCells();
		GenerateGhostCells();
	}

	void DestroyGhostCells() {
		if (ghostWrapCells[0] != null) Destroy(ghostWrapCells[0].gameObject);
		if (ghostWrapCells[1] != null) Destroy(ghostWrapCells[1].gameObject);
	}

	void GenerateGhostCells() {
		ghostWrapCells[0] = Instantiate(draggedCellsLine.Last());
		ghostWrapCells[1] = Instantiate(draggedCellsLine[0]);
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
