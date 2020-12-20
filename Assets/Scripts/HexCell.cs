using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CellMatch{
	public HashSet<HexCell> cellsInMatch = new HashSet<HexCell>();
	Vector3 center;
	bool centerInit = false;

    public bool bonusUsed;

    public int score {
        get {
            int needyModifier = 0;
            var needies = cellsInMatch.Where(x => x is NeedyCell).Select(x => (NeedyCell)x);
            foreach (var needy in needies)
            {
                if (needy.complete) needyModifier += 9+needy.health;
                else needyModifier -= 1;
            }
            return (cellsInMatch.Count - 2 + needyModifier + (bonusUsed?1:0))*10;
        }
    }

	public void PlaySound() {
		var needies = cellsInMatch.Where(x => x is NeedyCell).Select(x => (NeedyCell)x);
		if (needies.Count() == 0) {
			SoundBank.Instance.PlayMatchNote();
		}
		else {
			foreach (var needy in needies) {
				SoundBank.Instance.PlayNeedySound(needy.howManyCouplesDone);
			}
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
                center.z = -1;
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
            if (cell.match != null)
            {
                finalMatch.cellsInMatch.UnionWith(cell.match.cellsInMatch);
                finalMatch.bonusUsed |= cell.match.bonusUsed;
            }
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

    [HideInInspector]
    public bool gonnaMatch;
    Coroutine gonnaMatchAnim;

	public static GridData grid;

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

        debugColor = debug.color;
        debug.color = Color.white;
    }

    private void Update()
    {
        if (gonnaMatch)
        {
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
        var pos = transform.position;
        pos.z = state ? -1 : 0;
        transform.position = pos;
    }

    void LaunchAnim()
    {
        if (gonnaMatchAnim != null) StopCoroutine(gonnaMatchAnim);
        gonnaMatchAnim = StartCoroutine(HighLight());
    }

    public virtual void StopAnim()
    {
        gonnaMatch = false;
        if (gonnaMatchAnim != null)
        {
            StopCoroutine(gonnaMatchAnim);
            gonnaMatchAnim = null;
        }
		transform.eulerAngles = Vector3.zero;
    }

    protected virtual IEnumerator HighLight()
    {
        float period = .5f;
        while (true)
        {
            for (float t = 0; t < 1; t += Time.deltaTime / period)
            {
                transform.localEulerAngles = 4 * Vector3.forward * Mathf.Sin(t * 2 * Mathf.PI);

                yield return null;
            }
        }
    }

    protected IEnumerator matchAnim(float dur)
    {
		yield return StartCoroutine(DisAppearAnim(dur, match.getCenter()));
    }

	protected IEnumerator DisAppearAnim(float dur, Vector3 goalPos) {

		var startPos = transform.position;
		startPos.z = goalPos.z;
		for (float t = 0; t < 1; t += Time.deltaTime / dur) {
			yield return null;
			transform.position = Vector3.Lerp(startPos, goalPos, t);
			transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
		}
		Destroy(gameObject);
	}
}
