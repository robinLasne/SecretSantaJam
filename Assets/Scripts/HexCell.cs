﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CellMatch{
	HashSet<HexCell> cellsInMatch = new HashSet<HexCell>();
	Vector3 center;
	bool centerInit = false;

    public int score {
        get {
            return cellsInMatch.Count - 2;
        }
    }

	public Vector3 getCenter() {
		if (!centerInit) {
			centerInit = true;

            HexCell needy = cellsInMatch.FirstOrDefault(x => x is NeedyCell);
            if (needy != null)
            {
                center = needy.transform.position;
            }
            else
            {
                Vector3 sum = Vector3.zero;
                foreach (var cell in cellsInMatch)
                {
                    sum += cell.transform.position;
                }
                center = sum / cellsInMatch.Count;
            }
		}
		return center;
	}

	public static void Match(params HexCell[] thisMatch) {
		CellMatch finalMatch = new CellMatch();
		foreach(var cell in thisMatch) {
			finalMatch.cellsInMatch.Add(cell);
		}

		foreach (var cell in thisMatch) {
			if(cell.match != null) finalMatch.cellsInMatch.UnionWith(cell.match.cellsInMatch);
		}

		foreach(var cell in finalMatch.cellsInMatch) {
			cell.match = finalMatch;
		}
	}
}

public abstract class HexCell : MonoBehaviour
{
	public Vector3Int position { get; set; }
	public Vector3Int goalPosition { get; set; }

    public bool justMatched { get; set; }

	public CellMatch match;
    protected bool m_grown;
    public bool grown {
        get {
            return m_grown;
        }
    }

    public SpriteRenderer backGround;
    public Transform toGrow;
    public SpriteRenderer debug;
    Color debugColor;
    int bgOrder;

    [HideInInspector]
    public bool gonnaMatch;
    Coroutine gonnaMatchAnim;

    public abstract int type { get; }
	public abstract bool Matching(HexCell[] neighbours, out HashSet<HexCell> otherCells);
    public abstract void PreviewMatch(HexCell[] neighbours);
	public abstract bool ApplyMatch(float dur);

    public void Grow(float t)
    {
        if (t > 0) { m_grown = true; debug.color = debugColor; }
        toGrow.transform.localScale = t * Vector3.one;
    }

    protected virtual void Awake()
    {
        //content.transform.localScale = Vector3.zero;
        bgOrder = backGround.sortingOrder;

        debugColor = debug.color;
        debug.color = Color.white;
    }

    private void Update()
    {
        if (gonnaMatch)
        {
            Debug.Log("must anim");
            if (gonnaMatchAnim == null) LaunchAnim();
        }
        else
        {
            StopAnim();
        }
    }

    public virtual void SetInFront(bool state)
    {
        if (backGround == null) {
            Debug.Log("null renderer", this);
            return;
        }
        backGround.sortingOrder = bgOrder + (state ? 10 : 0);
    }

    void LaunchAnim()
    {
        if (gonnaMatchAnim != null) StopCoroutine(gonnaMatchAnim);
        gonnaMatchAnim = StartCoroutine(HighLight());
    }

    public void StopAnim()
    {
        if (gonnaMatchAnim != null)
        {
            StopCoroutine(gonnaMatchAnim);
            gonnaMatchAnim = null;
            backGround.color = Color.clear;
            transform.eulerAngles = Vector3.zero;
        }
    }

    IEnumerator HighLight()
    {
        float halfPeriod = .5f, maxIntensity = .2f;
        while (true)
        {
            for (float t = 0; t < 1; t += Time.deltaTime / halfPeriod)
            {
                backGround.color = new Color(1, 1, 1, t * maxIntensity);

                transform.localEulerAngles = 4 * Vector3.forward * Mathf.Sin(t * 2 * Mathf.PI);

                yield return null;
            }
            for (float t = 1; t > 0; t -= Time.deltaTime / halfPeriod)
            {
                backGround.color = new Color(1, 1, 1, t * maxIntensity);

                transform.localEulerAngles = 4 * Vector3.forward * Mathf.Sin(-t * 2 * Mathf.PI);

                yield return null;
            }
        }
    }

    protected IEnumerator matchAnim(float dur)
    {
        var startPos = transform.position;
        var goalPos = match.getCenter();
        for (float t = 0; t < 1; t += Time.deltaTime / dur)
        {
            transform.position = Vector3.Lerp(startPos, goalPos, t);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }
        Destroy(gameObject);
    }
}
