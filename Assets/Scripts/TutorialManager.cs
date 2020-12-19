using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct TutorialStep {
	public LevelStartData level;
	public GameObject TutorialHelp;
}

public class TutorialManager : MonoBehaviour {
	public GameObject StepDoneScreen;
	public TMPro.TextMeshProUGUI stepMessage, stepButtonText;

	public GameObject bonuses;

	public TutorialStep[] tutorialData;
	int currentTutorialStep = -1, currentStepProgress;

	public static TutorialManager Instance;

    public bool InTutorial {
        get {
            return currentTutorialStep >= 0 && currentTutorialStep < tutorialData.Length;
        }
    }

	GameManager gameMng;

	void Start() {
		gameMng = GetComponent<GameManager>();
		Instance = this;
		HideAllTutorial();

		//if (PlayerPrefs.GetInt("tutorial_done", 0) == 0) {
		//	PlayerPrefs.SetInt("tutorial_done", 1);
		//	StartTutorial();
		//}
	}

	public void StartTutorial() {
		SetTutorialStep(0);
		GridData.matchEvent += CheckMatch;
		bonuses.SetActive(false);
	}

	private void CheckMatch(List<CellMatch> matches, bool fromMovement) {
		var step = tutorialData[currentTutorialStep];
		if (step.level.needyTutorial) {
			var needyCells = matches.SelectMany(match => match.cellsInMatch).Where(cell => cell is NeedyCell).Select(cell => cell as NeedyCell).Where(cell => cell.complete);
			currentStepProgress += needyCells.Count();
		}
		else {
			if(fromMovement)currentStepProgress++;
		}

		if(currentStepProgress >= step.level.matchesNeeded) {
			HideAllTutorial();
			stepMessage.text = step.level.nextSteptext;
			stepButtonText.text = currentTutorialStep == tutorialData.Length - 1 ? "Go to game" : "Next";
			StepDoneScreen.SetActive(true);
		}
	}

	public void NextStep() {
		SetTutorialStep(currentTutorialStep + 1);
	}

	void SetTutorialStep(int i) {
		HideAllTutorial();
		currentTutorialStep=i;
		if (i < tutorialData.Length) {
			var step = tutorialData[i];

			currentStepProgress = 0;

			if (step.TutorialHelp != null) step.TutorialHelp.SetActive(true);
			gameMng.SelectLevel(step.level, true);
		}
		else {
			GridData.matchEvent -= CheckMatch;
			gameMng.BackToWelcome(true);
			bonuses.SetActive(true);
		}
	}

	void HideAllTutorial() {
		StepDoneScreen.SetActive(false);
		foreach (var step in tutorialData) {
			if (step.TutorialHelp != null) step.TutorialHelp.SetActive(false);
		}
	}

    public void ExitTutorial()
    {
        SetTutorialStep(int.MaxValue);
    }
}
