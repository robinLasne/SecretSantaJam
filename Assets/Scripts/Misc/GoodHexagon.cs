using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GoodHexagon : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		var grid = GetComponent<Grid>();
		float value = 2 / Mathf.Sqrt(3);
		if(grid.cellSize.y != value) {
			grid.cellSize = new Vector3(1, value, 1);
		}
    }
}
