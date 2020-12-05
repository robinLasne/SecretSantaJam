using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Utils;

public class GridData : MonoBehaviour {
    public NeedyCell needyPrefab;
	public BaseCell[] prefabs;
    public SpriteMask spriteMask;

    public NeedyPlantsData needyPlantsData;

	public int hexagonRadius = 4;

	HexCell[][] cells;

	public Grid grid { get; private set; }

	List<HexCell> toRegrowNext;

	public Vector2[] directionByIndex { get; private set; }

    ScoreManager scoreMng;

    GridMovements movements;

    private void Awake()
    {
        movements = GetComponent<GridMovements>();
        scoreMng = GetComponent<ScoreManager>();
    }

    void Start() {

		directionByIndex = new Vector2[] { Vector2.right, rotate(Vector2.right, Mathf.PI / 3), rotate(Vector2.right, 2 * Mathf.PI / 3) };

		cells = new HexCell[hexagonRadius * 2 + 1][];
		grid = GetComponent<Grid>();

		for (int j = -hexagonRadius; j <= hexagonRadius; ++j) {
			int minX = -hexagonRadius + Mathf.Abs(j) / 2, maxX = hexagonRadius - (Mathf.Abs(j) + 1) / 2;
			cells[j + hexagonRadius] = new HexCell[maxX - minX + 1];
			for (int i = minX; i <= maxX; ++i) {
				var position = new Vector3Int(i, j, 0);

                if (i == 0 && j == 0)
                {
                    var needy = PlaceNewCellInstant(needyPrefab, position);
                    needy.Grow(1);
                    continue;
                }

				var instance = PlaceNewCellInstant(Random.Range(0, prefabs.Length), position);
                instance.Grow(1);
			}
		}

        spriteMask.transform.localScale = Vector3.one * (2*hexagonRadius + 2f / 3);

        //Initial check
		CheckMatches(false);
	}

	HexCell PlaceNewCellInstant(int type, Vector3Int position) {
        return PlaceNewCellInstant(prefabs[type], position);
	}

    HexCell PlaceNewCellInstant(HexCell prefab, Vector3Int position)
    {
        var instance = Instantiate(prefab, grid.CellToWorld(position), Quaternion.identity, transform);
        instance.Grow(0);
        setCell(position, instance);
        instance.goalPosition = position;
        return instance;
    }

    IEnumerator GrowNewCells(List<HexCell> cells, float dur) {
		for (float t = 0; t < 1; t += Time.deltaTime / dur) {
			foreach (var cell in cells) {
                cell.Grow(t);
			}
			yield return null;
		}
		foreach (var cell in cells) {
            cell.Grow(1);
		}
	}
	
    public void PreviewMatches()
    {
        StopPreviewMatches();
        var toCheck = cells.SelectMany(x => x).ToList();
        foreach (var cell in toCheck)
        {
            var neighboursIdx = GetAllAdjacentCells(cell.position);
            var neighbourCells = new HexCell[neighboursIdx.Length];
            for (int j = 0; j < neighboursIdx.Length; ++j)
            {
                if (inBounds(neighboursIdx[j]))
                {
                    neighbourCells[j] = getCell(neighboursIdx[j]);
                }
            }

            cell.PreviewMatch(neighbourCells);
        }
    }

    public void StopPreviewMatches()
    {
        foreach (var cell in cells.SelectMany(x => x))
        {
            cell.StopAnim();
            cell.gonnaMatch = false;
        }
    }

	public bool CheckMatches(bool reGrow) {
        var toCheck = cells.SelectMany(x => x).ToList();

		var cellsToRemove = new HashSet<HexCell>();
		bool hasMatches = false;
		for (int i = 0; i < toCheck.Count; ++i) {
			var tmpCells = new HashSet<HexCell>();

			var neighboursIdx = GetAllAdjacentCells(toCheck[i].position);
			var neighbourCells = new HexCell[neighboursIdx.Length];
			for (int j = 0; j < neighboursIdx.Length; ++j) {
                if (inBounds(neighboursIdx[j]))
                {
                    neighbourCells[j] = getCell(neighboursIdx[j]);
                }
			}

			if (toCheck[i].Matching(neighbourCells, out tmpCells)) {
                hasMatches = true;

				cellsToRemove.Add(toCheck[i]);
				cellsToRemove.UnionWith(tmpCells);
			}
		}

		if (hasMatches) {
			movements.canDrag = false;
			var lastMatchCells = cellsToRemove.GroupBy(e => e.match);

            scoreMng.AddScore(lastMatchCells.Select(g => g.Key).ToList());

            var newCells = new List<HexCell>();

            foreach(var cell in cellsToRemove)
            {
                cell.justMatched = true;
            }

            foreach (var match in lastMatchCells)
            {
                // Calculate least occuring cell type
                int[] counting = new int[prefabs.Length];
                foreach (var line in cells)
                {
                    foreach (var hex in line)
                    {
                        if (!hex.justMatched && hex.type>0 && hex.type<=prefabs.Length) counting[hex.type - 1]++;
                    }
                }
                int toSpawn = counting.IndexOfMin();

                foreach (var cell in match)
                {
                    if (cell.ApplyMatch(.3f)) // Some cells don't disappear after their first match
                    {
                        var newCell = PlaceNewCellInstant(toSpawn, cell.position);
                        newCell.transform.position = cell.transform.position;

                        newCells.Add(newCell);

                        movements.CellHasBeenReplaced(cell, newCell);
                    }

                }
            }

			if (reGrow) {
                StartCoroutine(RespawnPreviousMatches(toRegrowNext,.3f));
                toRegrowNext = newCells;
			}
			else {
				if (toRegrowNext == null) toRegrowNext = new List<HexCell>();
				toRegrowNext.AddRange(newCells);
				movements.canDrag = true;
            }
		}

		return hasMatches;
	}

	IEnumerator RespawnPreviousMatches(List<HexCell> toGrow, float animLength) {
        // This should never happen
        if (toGrow.Any(e => e.grown)) {
            Debug.Break();
            Debug.LogError("Growing an already grown plant");
        }

		if (toGrow == null) {
            movements.canDrag = true;
			yield break;
		}

        yield return StartCoroutine(GrowNewCells(toGrow, animLength));

        if (!CheckMatches(false))
        {
            movements.canDrag = true;
        }
    }

	

	int lineOffset(int j) {
		return -hexagonRadius + Mathf.Abs(j) / 2;
	}

	public bool inBounds(Vector3Int v) {
        var y = v.y + hexagonRadius;
        var x = v.x - lineOffset(v.y);

        if(y>=0 && y < cells.Length)
        {
            if(x>=0 && x < cells[y].Length)
            {
                return true;
            }
        }

        return false;
	}

	public HexCell getCell(int x, int y) {
		return cells[y + hexagonRadius][x - lineOffset(y)];
	}

    public HexCell getCell(Vector2Int v) {
		return getCell(v.x, v.y);
	}

    public HexCell getCell(Vector3Int v) {
		return getCell(v.x, v.y);
	}

    public void setCell(int x, int y, HexCell cell) {
		cells[y + hexagonRadius][x - lineOffset(y)] = cell;
		cell.position = new Vector3Int(x, y, 0);
	}

    public void setCell(Vector2Int v, HexCell cell) {
		setCell(v.x, v.y, cell);
	}

    public void setCell(Vector3Int v, HexCell cell) {
		setCell(v.x, v.y, cell);
	}

    public Vector3Int[] GetAllAdjacentCells(Vector3Int cell) {
		var res = new Vector3Int[6];
		for (int i = 0; i < 6; ++i) {
			res[i] = GetAdjacentCell(cell, i);
		}

		return res;
	}

    public Vector3Int GetAdjacentCell(Vector3Int cell, int idx) {
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

    public int getDirectionIdx(Vector2 direction) {
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
