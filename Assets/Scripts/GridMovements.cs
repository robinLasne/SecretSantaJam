using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        DestroyGhostCells();
        GenerateGhostCells();

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
        if (data.CheckMatches(draggedCellsLine, true))
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
        DestroyGhostCells();
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
        Vector3 startPos = draggedCell.transform.position;
        Vector3 goalPos = data.grid.CellToWorld(draggedCellStartPos);
        goalPos.z = 0;
        Vector2 direction = data.directionByIndex[dragDir];
        for (float t = 0; t < 1; t += Time.deltaTime / dur)
        {
            MoveDraggedCellTo(Vector3.Lerp(startPos, goalPos, t * t), direction);
            yield return null;
        }
        MoveDraggedCellTo(goalPos, direction);
        snapAnim = null;
    }

    void MoveDraggedCellTo(Vector3 position, Vector2 direction)
    {
        Vector3 delta = draggedCell.transform.position;
        draggedCell.transform.position = position;
        delta -= draggedCell.transform.position;
        for (int i = 0; i < draggedCellsLine.Count; ++i)
        {
            if (draggedCellsLine[i] != draggedCell) draggedCellsLine[i].transform.position -= delta;
        }
        WrapDraggedCells(direction);
    }

    void WrapDraggedCells(Vector3 dirVector)
    {
        while (!data.inBounds(data.grid.WorldToCell(draggedCellsLine[0].transform.position)))
        {
            // Failsafe to be sure we're wrapping in the right direction
            if (!data.inBounds(data.grid.WorldToCell(draggedCellsLine.Last().transform.position + dirVector))) break;

            var toMove = draggedCellsLine[0];
            toMove.transform.position = draggedCellsLine.Last().transform.position + dirVector;
            draggedCellsLine.RemoveAt(0);
            draggedCellsLine.Add(toMove);

            RefreshDraggedCells();
        }
        while (!data.inBounds(data.grid.WorldToCell(draggedCellsLine.Last().transform.position)))
        {
            // Failsafe to be sure we're wrapping in the right direction
            if (!data.inBounds(data.grid.WorldToCell(draggedCellsLine[0].transform.position - dirVector))) break;

            var toMove = draggedCellsLine.Last();
            toMove.transform.position = draggedCellsLine[0].transform.position - dirVector;
            draggedCellsLine.RemoveAt(draggedCellsLine.Count - 1);
            draggedCellsLine.Insert(0, toMove);

            RefreshDraggedCells();
        }

        ghostWrapCells[0].transform.position = draggedCellsLine[0].transform.position - dirVector;
        ghostWrapCells[1].transform.position = draggedCellsLine.Last().transform.position + dirVector;
    }

    void DestroyGhostCells()
    {
        if (ghostWrapCells[0] != null) Destroy(ghostWrapCells[0].gameObject);
        if (ghostWrapCells[1] != null) Destroy(ghostWrapCells[1].gameObject);
    }

    void GenerateGhostCells()
    {
        ghostWrapCells[0] = Instantiate(draggedCellsLine.Last());
        ghostWrapCells[1] = Instantiate(draggedCellsLine[0]);
    }

    void RefreshDraggedCells()
    {
        for (int i = 0; i < draggedCellsLine.Count; ++i)
        {
            data.setCell(draggedIndices[i], draggedCellsLine[i]);
        }
        DestroyGhostCells();
        GenerateGhostCells();
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
