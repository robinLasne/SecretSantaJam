using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreDisplay;
    public ScorePopup popupPrefab;

    int currentScore=0;

	public void ResetScore() {
		currentScore = 0;
		scoreDisplay.text = currentScore.ToString();
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
    }
}
