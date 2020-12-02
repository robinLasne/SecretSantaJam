using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    public float animDur;
    public float animDist;
    
    TextMeshPro text;
    
    public void InitAnim(string content, Color inside, Color outside)
    {
        text = GetComponent<TextMeshPro>();

        text.text = content;
        text.color = inside;
        text.outlineColor = outside;

        StartCoroutine(Anim());
    }

    IEnumerator Anim()
    {
        Color startColor = text.color;
        Color endColor = startColor;
        endColor.a = 0;

        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + Vector2.up*animDist;

        for(float t = 0; t < 1; t += Time.deltaTime / animDur)
        {
            text.color = Color.Lerp(startColor, endColor, t);
            transform.position = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
