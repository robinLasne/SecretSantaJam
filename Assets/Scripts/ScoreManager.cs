using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI scoreDisplay;

    int currentScore=0;

    void Start()
    {
        scoreDisplay.text = "0";
    }

    public void AddScore(List<CellMatch> matches)
    {
        int thisScore = 0;
        foreach (var match in matches) thisScore += match.score;

        currentScore += (thisScore * matches.Count + 1) / 2;

        scoreDisplay.text = currentScore.ToString();
    }
}
