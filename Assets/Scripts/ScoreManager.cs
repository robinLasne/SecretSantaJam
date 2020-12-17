using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

[System.Serializable]
public class LevelReward {
    public int expNeeded = 150;
    public GameObject reward, gameBonus;
}

public class ScoreManager : MonoBehaviour
{
    [Header("In-Game Score")]
    public TextMeshProUGUI scoreDisplay;
    public ScorePopup popupPrefab;
	public GameObject highScoreDisplay;

    int currentScore=0;

    [Header("Overall Score")]
    public LevelReward[] levelRewards;
    public Image levelBar;
    int overallScore;
    int curLevel;

	public int highScore { get; private set; }

	public static ScorePopup ScorePopup;

	private void Start() {
		overallScore = PlayerPrefs.GetInt("total_score");
		CheckScore(true);

		highScore = PlayerPrefs.GetInt("highscore");

		ScorePopup = popupPrefab;
	}

	#region In-Game
	public void ResetScore() {
		currentScore = 0;
		scoreDisplay.text = currentScore.ToString();

		highScoreDisplay.SetActive(false);
	}

	public void AddScore(List<CellMatch> matches)
    {
        int thisScore = 0;
        foreach (var match in matches)
        {
            thisScore += match.score;

            var popup = Instantiate(popupPrefab, match.getCenter(), Quaternion.identity);
            popup.InitAnim(match.score.ToString(), Color.white, Color.black);
        }

        if (matches.Count > 1)
        {
            thisScore = (thisScore * (matches.Count + 1) + 1) / 2;

            var center = matches.Aggregate(Vector3.zero, (e, v) => e + v.getCenter()/matches.Count);
            var popup = Instantiate(popupPrefab, center, Quaternion.identity);
            var message = string.Format("×{0}", .5f + .5f * matches.Count);
            popup.InitAnim(message, Color.red, Color.green);
        }

        currentScore += thisScore;
        

        scoreDisplay.text = currentScore.ToString();

        PlayerPrefs.SetInt("total_score", overallScore+currentScore);

		if(currentScore > highScore) {
			highScoreDisplay.SetActive(true);
			highScore = currentScore;
			PlayerPrefs.SetInt("highscore", highScore);
		}
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
            if(afterLevel >= 0)
            {
                remainingScore = afterLevel;
                lastLevel = i;
            }
        }
        lastLevel++;

        float progress = lastLevel>=levelRewards.Length?1 : (remainingScore + extraPrecision) / (float)levelRewards[lastLevel].expNeeded;
		
        curLevel = lastLevel;
        levelBar.fillAmount = progress;
        for (int i = 0; i < curLevel; ++i)
        {
            if(rewardsInstant && levelRewards[i].reward != null)levelRewards[i].reward.SetActive(true);
            if(levelRewards[i].gameBonus!=null) levelRewards[i].gameBonus.SetActive(true);
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
            if(curLevel > oldLevel)
            {
                for(int i= oldLevel; i< curLevel; ++i)
                {
                    // Replace this with animation maybe
                    if (levelRewards[i].reward != null) levelRewards[i].reward.SetActive(true);
                }
                oldLevel = curLevel;
            }
            yield return null;
        }
        overallScore = endScore;
        CheckScore(false);
        if (curLevel > oldLevel)
        {
            for (int i = oldLevel; i < curLevel; ++i)
            {
                // Replace this with animation maybe
                if (levelRewards[i].reward != null) levelRewards[i].reward.SetActive(true);
            }
        }
    }


    #endregion
}
