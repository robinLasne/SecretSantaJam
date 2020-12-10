using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public Camera cam;
	public Transform welcomeScreen, gameScreen;
	public GridData grid;
	public ScoreManager scoreMnG;

	public Canvas gameCanvas;
	public GraphicRaycaster WelcomeCanvasRaycasts;

	bool gameReady;

    void Start()
    {
		// Will check here if the tutorial needs to be played

		gameCanvas.gameObject.SetActive(false);
		grid.matchEvent += (e) => { if (gameReady) scoreMnG.AddScore(e); };
    }

	public void SelectLevel(LevelStartData level) {
		gameReady = false;
		grid.InitGrid(level);
		gameReady = true;
		WelcomeCanvasRaycasts.enabled = false;
		scoreMnG.ResetScore();
		StartCoroutine(SwipeFromTo(welcomeScreen, gameScreen, true));
	}

	public void GenerateRandomLevel(int size) {
		gameReady = false;
		grid.InitRandomGrid(size);
		gameReady = true;
		WelcomeCanvasRaycasts.enabled = false;
		scoreMnG.ResetScore();
		StartCoroutine(SwipeFromTo(welcomeScreen, gameScreen, true));
	}

	public void BackToWelcome() {
		gameCanvas.gameObject.SetActive(false);
		WelcomeCanvasRaycasts.enabled = true;

		StartCoroutine(SwipeFromTo(gameScreen, welcomeScreen, false));
	}

	IEnumerator SwipeFromTo(Transform fromT, Transform toT, bool enableCanvas, float dur = .5f) {
		Vector3 from = fromT.position, to = toT.position;
		from.z = to.z = cam.transform.position.z;

		for (float t = 0; t < 1; t += Time.deltaTime/dur) {
			cam.transform.position = Vector3.Lerp(from, to, t);

			yield return null;
		}

		cam.transform.position = to;
		if(enableCanvas)gameCanvas.gameObject.SetActive(true);
	}
}
