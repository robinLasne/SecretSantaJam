using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

[System.Serializable]
public class LevelReward {
    public int expNeeded = 150;
	public SpriteRenderer reward;
    public GameObject gameBonus, helpWindow;
	public Sprite rewardPreviewIcon;
}

public class ScoreManager : MonoBehaviour
{
	public TextMeshProUGUI highScoreDisplay;

    [Header("In-Game Score")]
    public ScorePopup popupPrefab;
    public TextMeshProUGUI scoreDisplay, multiplyerDisplay;
	public GameObject highScoreNotification;

    int currentScore=0;
	int displayedScore = 0;

	float globalMultiplyer;

	readonly string[] multiMatchNames = { "Zero", "Simple", "Double", "Triple", "Quadruple" };

	public Color[] multiMatchColors;

	[Header("Overall Score")]
    public LevelReward[] levelRewards;
    public Image levelBar, levelSmallIcon;
	public TextMeshProUGUI levelText, levelProgressText;
    int overallScore;
    public static int curLevel { get; private set; }

	public int highScore { get; private set; }

	public static ScorePopup ScorePopup;

	private void Start() {
		overallScore = PlayerPrefs.GetInt("total_score");
		CheckScore(true);

		highScore = PlayerPrefs.GetInt("highscore");
		highScoreDisplay.text = highScore.ToString();

		ScorePopup = popupPrefab;
	}

	#region In-Game
	public void ResetScore() {
		globalMultiplyer = 1;
		multiplyerDisplay.text = string.Format("×{0:0.00}", globalMultiplyer);
		currentScore = 0;
		displayedScore = 0;
		scoreDisplay.text = currentScore.ToString();

		highScoreNotification.SetActive(false);
	}

	public void AddScore(List<CellMatch> matches)
    {
        int thisScore = 0;
        foreach (var match in matches)
        {
            thisScore += match.score;

			if (match.score > 0) {
				var popup = Instantiate(popupPrefab, match.getCenter(), Quaternion.identity);
				popup.InitAnim(match.score.ToString(), Color.white);
			}
        }

        if (matches.Count > 1)
        {
			float multiplier = (matches.Count + 1) / 2f;

            thisScore = Mathf.CeilToInt(thisScore * multiplier);

            var center = matches.Aggregate(Vector3.zero, (e, v) => e + v.getCenter()/matches.Count);
            var popup = Instantiate(popupPrefab, center, Quaternion.identity);

            var message = string.Format("Match ×{0}", .5f + .5f * matches.Count);
			if(matches.Count < multiMatchNames.Length) {
				message = multiMatchNames[matches.Count] + " match";
			}

			Color color;

			if (matches.Count < multiMatchColors.Length) color = multiMatchColors[matches.Count];
			else color = multiMatchColors.Last();

			popup.InitAnim(message, color);
        }

		thisScore = Mathf.CeilToInt(thisScore * globalMultiplyer);

		globalMultiplyer += 0.01f;
		multiplyerDisplay.text = string.Format("×{0:0.00}",globalMultiplyer);

		currentScore += thisScore;

		StartCoroutine(AddScore(thisScore, 1));

        if(!TutorialManager.Instance.InTutorial)PlayerPrefs.SetInt("total_score", overallScore+currentScore);

		if(!TutorialManager.Instance.InTutorial && currentScore > highScore) {
			highScoreNotification.SetActive(true);
			highScore = currentScore;
			PlayerPrefs.SetInt("highscore", highScore);
			highScoreDisplay.text = highScore.ToString();
		}
    }

	IEnumerator AddScore(int toAdd, float dur) {
		float timeStep = dur / toAdd;
		for(int i=0; i < toAdd; ++i) {
			++displayedScore;
			scoreDisplay.text = displayedScore.ToString();

			yield return new WaitForSeconds(timeStep);
		}
		scoreDisplay.text = displayedScore.ToString();
	}

    #endregion

    #region Overall

    void CheckScore(bool rewardsInstant, float extraPrecision = 0)
    {
        int remainingScore = overallScore;
        int lastLevel=-1;
        for(int i=0;i < levelRewards.Length; ++i)
        {
            int afterLevel = remainingScore - levelRewards[i].expNeeded;
			if (afterLevel >= 0) {
				remainingScore = afterLevel;
				lastLevel = i;
			}
			else break;
        }
        lastLevel++;

		float progress = lastLevel >= levelRewards.Length ? 1 : (remainingScore + extraPrecision) / (float)levelRewards[lastLevel].expNeeded;
		UpdateDisplay(lastLevel, remainingScore, progress);

		curLevel = lastLevel;
        for (int i = 0; i < curLevel; ++i)
        {
            if(rewardsInstant && levelRewards[i].reward != null)levelRewards[i].reward.gameObject.SetActive(true);
            if(levelRewards[i].gameBonus!=null) levelRewards[i].gameBonus.SetActive(true);

			if(rewardsInstant) CheckHelpWindow(i);
        }

		if (rewardsInstant) {
			if (curLevel < levelRewards.Length && levelRewards[curLevel].rewardPreviewIcon != null) {
				levelSmallIcon.sprite = levelRewards[curLevel].rewardPreviewIcon;
				levelSmallIcon.gameObject.SetActive(true);
			}
			else levelSmallIcon.gameObject.SetActive(false);
		}
    }

	void UpdateDisplay(int level, int score, float progress) {
		levelText.text = "Level " + (level + 1);
		levelProgressText.text = level >= levelRewards.Length ? "All done!" : score + " / " + levelRewards[level].expNeeded;
		levelBar.fillAmount = progress;
	}

	void CheckHelpWindow(int i) {
		if (levelRewards[i].helpWindow != null) {
			string helpKey = string.Format("help_{0}_seen", i);

			if (PlayerPrefs.GetInt(helpKey, 0) == 0) {
				levelRewards[i].helpWindow.SetActive(true);
				PlayerPrefs.SetInt(helpKey, 1);
			}
		}
	}

    public IEnumerator AddCurrentScoreToOverAll(float delay, float dur)
    {
        int startScore = overallScore, endScore = overallScore + currentScore;
        int oldLevel = curLevel;

        PlayerPrefs.SetInt("total_score", endScore);

        yield return new WaitForSeconds(delay);
        for(float t = 0; t < 1; t += Time.deltaTime / dur)
        {
			var floatScore = Mathf.Lerp(startScore, endScore, t);
			overallScore = (int)floatScore;
            CheckScore(false, floatScore-overallScore);
			if (curLevel > oldLevel) {
				for (int i = oldLevel; i < curLevel; ++i) {
					UpdateDisplay(i, levelRewards[i].expNeeded, 1);
					yield return StartCoroutine(ChangeReward(i + 1, .7f));
				}
				oldLevel = curLevel;
			}
			yield return null;
        }
        overallScore = endScore;
        CheckScore(false);
		if (curLevel > oldLevel) {
			for (int i = oldLevel; i < curLevel; ++i) {
				yield return StartCoroutine(ChangeReward(i + 1, .7f));
			}
		}
	}

	IEnumerator ChangeReward(int level, float dur) {
		Color baseCol = levelSmallIcon.color;

		Sprite icon = null;
		SpriteRenderer reward = null;
		if (level < levelRewards.Length) {
			icon = levelRewards[level].rewardPreviewIcon;
		}
		if (level - 1 < levelRewards.Length) {
			reward = levelRewards[level - 1].reward;
		}
		if (reward != null) {
			reward.gameObject.SetActive(true);
			foreach (Transform t in reward.transform) t.gameObject.SetActive(false);
		}
		
		for (float t = 0; t < 1; t += Time.deltaTime / dur) {
			levelSmallIcon.color = new Color(1, 1, 1, 1-t);
			levelSmallIcon.transform.localScale = Vector3.one * (1 + t);

			reward.color = new Color(1, 1, 1, t);
			yield return null;
		}

		if (reward != null) {
			reward.color = Color.white;
			foreach (Transform t in reward.transform) t.gameObject.SetActive(true);
		}

		if (level - 1 < levelRewards.Length) CheckHelpWindow(level - 1);

		if (icon != null) {
			levelSmallIcon.gameObject.SetActive(true);
			levelSmallIcon.sprite = icon;
		}
		else {
			levelSmallIcon.gameObject.SetActive(false);
		}
		levelSmallIcon.color = baseCol;
		levelSmallIcon.transform.localScale = Vector3.one;

		yield return new WaitForSeconds(dur / 2);
	}


    #endregion
}
