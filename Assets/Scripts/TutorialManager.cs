using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct TutorialStep {
	public LevelStartData level;
	public GameObject TutorialHelp;
	public bool needyTutorial;
	public int matchesNeeded;
}

public class TutorialManager : MonoBehaviour {
	public GameObject StepDoneScreen;

	public TutorialStep[] tutorialData;
	int currentTutorialStep, currentStepProgress;


	GameManager gameMng;

	void Start() {
		gameMng = GetComponent<GameManager>();
		HideAllTutorial();
	}

	public void StartTutorial() {
		SetTutorialStep(0);
		GridData.matchEvent += CheckMatch;
	}

	private void CheckMatch(List<CellMatch> matches, bool fromMovement) {
		var step = tutorialData[currentTutorialStep];
		if (step.needyTutorial) {
			var needyCells = matches.SelectMany(match => match.cellsInMatch).Where(cell => cell is NeedyCell).Select(cell => cell as NeedyCell).Where(cell => cell.complete);
			currentStepProgress += needyCells.Count();
		}
		else {
			currentStepProgress += matches.Count;
		}

		if(currentStepProgress >= step.matchesNeeded) {
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
		}
	}

	void HideAllTutorial() {
		StepDoneScreen.SetActive(false);
		foreach (var step in tutorialData) {
			if (step.TutorialHelp != null) step.TutorialHelp.SetActive(false);
		}
	}

}
