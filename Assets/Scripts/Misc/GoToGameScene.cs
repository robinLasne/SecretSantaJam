using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToGameScene : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButton(0)) UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
