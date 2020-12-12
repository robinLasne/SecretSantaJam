using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;

public class GridMovements : MonoBehaviour
{
    GridData data;
    Camera cam;

    public bool canDrag { get; set; }
    bool isDragging = false;
    int dragDir;
    HexCell draggedCell;
    Vector3Int draggedCellStartPos;
    List<HexCell> draggedCellsLine = new List<HexCell>();
    List<Vector3Int> draggedIndices = new List<Vector3Int>();
    HexCell[] ghostWrapCells = new HexCell[2];

	Coroutine snapAnim;

    private void Awake()
    {
        data = GetComponent<GridData>();
        cam = Camera.main;
    }

    public void StartDrag(Vector2 position, Vector3 direction)
    {
        if (!canDrag) return;
        draggedCellStartPos = data.grid.WorldToCell(cam.ScreenToWorldPoint(position));

        if (!data.inBounds(draggedCellStartPos))
        {
            Debug.LogError("Drag dehors");
            return;
        }

        draggedCellStartPos.z = 0;
        draggedCell = data.getCell(draggedCellStartPos);

        dragDir = data.getDirectionIdx(direction);

        isDragging = true;

        if (snapAnim != null)
        {
            StopCoroutine(snapAnim);
            SnapInstant();
        }

        draggedIndices.Clear();
        draggedIndices.Add(draggedCellStartPos);

        // Get all cells in drag direction
        var tmpCell = data.GetAdjacentCell(draggedCellStartPos, dragDir);
        while (data.inBounds(tmpCell))
        {
            draggedIndices.Add(tmpCell);
            tmpCell = data.GetAdjacentCell(tmpCell, dragDir);
        }

        //Get all cells in opposite direction
        tmpCell = data.GetAdjacentCell(draggedCellStartPos, dragDir + 3);
        while (data.inBounds(tmpCell))
        {
            draggedIndices.Insert(0, tmpCell);
            tmpCell = data.GetAdjacentCell(tmpCell, dragDir + 3);
        }

        // Place the old line in the back
        foreach (var cell in draggedCellsLine) if (cell != null) cell.SetInFront(false);

        draggedCellsLine = draggedIndices.Select(data.getCell).ToList();

        foreach (var cell in draggedCellsLine) cell.SetInFront(true);

        RefreshGhostCells();

        UpdateDrag(direction);
    }

    public void UpdateDrag(Vector2 delta)
    {
        if (!isDragging) return;
        Vector3 dirVector = data.directionByIndex[dragDir];

        var scaledDelta = cam.ScreenToWorldPoint(Vector3.forward + new Vector3(delta.x, delta.y, 0)) - cam.ScreenToWorldPoint(Vector3.forward);

        float dragDist = Vector2.Dot(scaledDelta, dirVector);

        foreach (var cell in draggedCellsLine)
        {
            cell.transform.position += dirVector * dragDist;
        }

        WrapDraggedCells(dirVector);
    }

    public IEnumerator StopDrag(Vector2 delta)
    {
        if (!isDragging) yield break;
        isDragging = false;

        data.StopPreviewMatches();

        if (data.CheckMatches(true))
        {
            for (int i = 0; i < draggedIndices.Count; ++i)
            {
                draggedCellsLine[i].goalPosition = draggedIndices[i];
            }

            snapAnim = StartCoroutine(SnapToSlots(.2f));
        }
        else
        {
            snapAnim = StartCoroutine(SnapToStart(.2f));
        }
        yield return snapAnim;
        RefreshGhostCells(false);
    }

    IEnumerator SnapToSlots(float dur)
    {
        Vector3[] startPos = draggedCellsLine.Select(e => e.transform.position).ToArray();
        Vector3[] goalPos = startPos.Select(e => data.grid.CellToWorld(data.grid.WorldToCell(e))).ToArray();
        for (float t = 0; t < 1; t += Time.deltaTime / dur)
        {
            for (int i = 0; i < draggedCellsLine.Count; ++i)
            {
                draggedCellsLine[i].transform.position = Vector3.Lerp(startPos[i], goalPos[i], t * t);
            }
            yield return null;
        }
        for (int i = 0; i < draggedCellsLine.Count; ++i)
        {
            draggedCellsLine[i].transform.position = goalPos[i];
        }
        snapAnim = null;
    }

    IEnumerator SnapToStart(float dur)
    {
        Vector3 startPos = draggedCell.transform.localPosition;
        Vector3 goalPos = data.grid.CellToLocal(draggedCellStartPos);
        goalPos.z = startPos.z;
        if(Vector2.SqrMagnitude(goalPos-startPos) > SpeedSquareHalf(draggedCellsLine.Count))
        {
            goalPos += (startPos - goalPos).normalized * draggedCellsLine.Count;
        }
        Vector2 direction = data.directionByIndex[dragDir];
        for (float t = 0; t < 1; t += Time.deltaTime / dur)
        {
            Vector3 toGo = Vector3.Lerp(startPos, goalPos, t * t);
            MoveDraggedCellTo(toGo, direction);
            if(draggedCell.transform.localPosition != toGo)
            {
                startPos += (startPos - goalPos).normalized * draggedCellsLine.Count;
                goalPos = data.grid.CellToLocal(draggedCellStartPos);
            }
            yield return null;
        }
        MoveDraggedCellTo(goalPos, direction);
        snapAnim = null;
    }

    void MoveDraggedCellTo(Vector3 position, Vector2 direction)
    {
        Vector3 delta = draggedCell.transform.localPosition;
        draggedCell.transform.localPosition = position;
        delta -= draggedCell.transform.localPosition;
        for (int i = 0; i < draggedCellsLine.Count; ++i)
        {
            if (draggedCellsLine[i] != draggedCell) draggedCellsLine[i].transform.localPosition -= delta;
        }
        WrapDraggedCells(direction);
    }

    void WrapDraggedCells(Vector3 dirVector)
    {
        bool hasChanged = false;

        while (!data.inBounds(data.grid.LocalToCell(draggedCellsLine[0].transform.localPosition)))
        {
			var warpPos = draggedCellsLine.Last().transform.localPosition + dirVector;

			// Failsafe to be sure we're wrapping in the right direction
			if (!data.inBounds(data.grid.LocalToCell(warpPos))) break;

            var toMove = draggedCellsLine[0];
            toMove.transform.localPosition = warpPos;
            draggedCellsLine.RemoveAt(0);
            draggedCellsLine.Add(toMove);

            RefreshDraggedCells();
            hasChanged = true;
        }
        while (!data.inBounds(data.grid.LocalToCell(draggedCellsLine.Last().transform.localPosition)))
        {
			var warpPos = draggedCellsLine[0].transform.localPosition - dirVector;

			// Failsafe to be sure we're wrapping in the right direction
			if (!data.inBounds(data.grid.LocalToCell(warpPos))) break;

            var toMove = draggedCellsLine.Last();
			toMove.transform.localPosition = warpPos;
            draggedCellsLine.RemoveAt(draggedCellsLine.Count - 1);
            draggedCellsLine.Insert(0, toMove);

            RefreshDraggedCells();
            hasChanged = true;
        }

        ghostWrapCells[0].transform.localPosition = draggedCellsLine[0].transform.localPosition - dirVector;
        ghostWrapCells[1].transform.localPosition = draggedCellsLine.Last().transform.localPosition + dirVector;

        if (hasChanged && isDragging)
        {
            data.PreviewMatches();
        }
    }

    void RefreshGhostCells(bool reCreate = true)
    {
        if (ghostWrapCells[0] != null) Destroy(ghostWrapCells[0].gameObject);
        if (ghostWrapCells[1] != null) Destroy(ghostWrapCells[1].gameObject);
        if (reCreate)
        {
            ghostWrapCells[0] = Instantiate(draggedCellsLine.Last(), this.transform);
            ghostWrapCells[1] = Instantiate(draggedCellsLine[0], this.transform);

            ghostWrapCells[0].StopAnim();
            ghostWrapCells[1].StopAnim();
        }
    }

    void RefreshDraggedCells()
    {
        for (int i = 0; i < draggedCellsLine.Count; ++i)
        {
            data.setCell(draggedIndices[i], draggedCellsLine[i]);
        }
        RefreshGhostCells();
    }

    void SnapInstant()
    {
        foreach (var cell in draggedCellsLine)
        {
            data.setCell(cell.goalPosition, cell);
            cell.transform.position = data.grid.CellToWorld(cell.goalPosition);
        }
    }

    public void CellHasBeenReplaced(HexCell oldCell, HexCell newCell)
    {
        if (oldCell == draggedCell)
        {
            draggedCell = newCell;
            newCell.SetInFront(true);
        }
        var i = draggedCellsLine.IndexOf(oldCell);
        if (i >= 0)
        {
            draggedCellsLine[i] = newCell;
            newCell.SetInFront(true);
        }
    }
}
