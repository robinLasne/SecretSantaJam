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

	public GraphicRaycaster WelcomeCanvasRaycasts, gameCanvasRaycasts;

	bool gameReady;

    TutorialManager tutorial;

    void Start()
    {
        tutorial = GetComponent<TutorialManager>();

		// Will check here if the tutorial needs to be played

		gameCanvasRaycasts.enabled = false;
		GridData.matchEvent += (e,fromMove) => { if (gameReady) scoreMnG.AddScore(e); };
    }

	public void SelectLevel(LevelStartData level, bool instant = false) {
		gameReady = false;
		grid.InitGrid(level);
		gameReady = true;
		WelcomeCanvasRaycasts.enabled = false;
		scoreMnG.ResetScore();
		if(instant)StartCoroutine(SwipeFromTo(welcomeScreen, gameScreen, true, 0));
		else StartCoroutine(SwipeFromTo(welcomeScreen, gameScreen, true));
	}

	public void GenerateRandomLevel(int size) {
		gameReady = false;
		grid.InitRandomGrid(size);
		gameReady = true;
		WelcomeCanvasRaycasts.enabled = false;
		scoreMnG.ResetScore();
		StartCoroutine(SwipeFromTo(welcomeScreen, gameScreen, true));
	}

	public void BackToWelcome(bool instant = false) {
		gameCanvasRaycasts.enabled = false;
		WelcomeCanvasRaycasts.enabled = true;

		if(instant)StartCoroutine(SwipeFromTo(gameScreen, welcomeScreen, false, 0));
		else StartCoroutine(SwipeFromTo(gameScreen, welcomeScreen, false));
	}

    public void OnClickBackButton()
    {
        if (tutorial.InTutorial) tutorial.ExitTutorial();
        else
        {
            BackToWelcome();
            StartCoroutine(scoreMnG.AddCurrentScoreToOverAll(.5f,1));
			StartCoroutine(BonusPool.AddCollectedToStored(.6f));
        }
    }

	IEnumerator SwipeFromTo(Transform fromT, Transform toT, bool enableCanvas, float dur = .5f) {
		Vector3 from = fromT.position, to = toT.position;
		from.z = to.z = cam.transform.position.z;

		for (float t = 0; dur>0 && t < 1; t += Time.deltaTime/dur) {
			cam.transform.position = Vector3.Lerp(from, to, t);

			yield return null;
		}

		cam.transform.position = to;
		if (enableCanvas) gameCanvasRaycasts.enabled = true;
	}
}
