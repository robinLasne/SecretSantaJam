using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoResize : MonoBehaviour
{
	public GridData grid;
	public float space = 0;

	Camera cam;
	float hexagonRatio = Mathf.Sqrt(3) / 2;

	float screenWidth;

	private void Awake() {
		cam = GetComponent<Camera>();
	}

	// Start is called before the first frame update
	void Start()
    {

		screenWidth = Screen.width;

		float gridWidth = grid.hexagonRadius * 2 + 1;
		float gridHeight = gridWidth * hexagonRatio;

		float screenRatio = Screen.height / (float)Screen.width;

		if(screenRatio > hexagonRatio) {
			cam.orthographicSize = (gridWidth+space) * screenRatio/2;
		}
		else {
			cam.orthographicSize = (gridHeight+space)/2;
		}
		cam.orthographicSize += space*screenRatio;
    }

    // Update is called once per frame
    void Update()
    {
		if (Application.isEditor || Screen.width != screenWidth) Start();
    }
}
