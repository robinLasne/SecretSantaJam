using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NeedyCell : HexCell {
    public int[] needs = new int[6];
    public SpriteRenderer centerIcon;
    public SpriteRenderer[] leaves = new SpriteRenderer[6];

    bool[] couplesDone = new bool[3];

	public int health = 10;
	public TMPro.TextMeshPro healthDisplay;

	bool[] leafGonnaMatch = new bool[6];

    public override int type {
        get {
            return 0;
        }
    }

    public bool complete {
        get {
            return couplesDone.All(c => c);
        }
    }

	protected override void Awake() {
		base.Awake();
		healthDisplay.text = health.ToString();
	}

	private void MatchDone(List<CellMatch> matches, bool fromMovement) {
		if (fromMovement && grown && !complete) {
			health--;
			healthDisplay.text = health.ToString();
		}
	}

	private void CheckDeath() {
		if (!complete && health <= 0) {
			grid.ReplaceNeedy(this);
			StartCoroutine(DisAppearAnim(.3f, transform.position));
		}
	}

	public void InitLeaves() {
		GridData.matchEvent += MatchDone;
		GridData.postMatchEvent += CheckDeath;

		needs = needs.OrderBy(x => Random.value).ToArray();

        for (int i = 0; i < 6; ++i)
        {
			needs[i] = Random.Range(1, 7);
            leaves[i].sprite = NeedyPlantsData.Instance.data.First(x => x.type == needs[i]).leafSprite;
        }
    }

    public override bool ApplyMatch(float dur)
    {
        for(int i=0; i < couplesDone.Length; ++i)
        {
            if (couplesDone[i])
            {
                leaves[i * 2].enabled = false;
                leaves[i * 2+1].enabled = false;
            }
        }

        if (complete)
        {
            var cR = centerIcon.gameObject.AddComponent<CoroutineRunner>();
			cR.StartCoroutine(NeedySuccessAnim(dur*5));
            StartCoroutine(matchAnim(dur));
            return true;
        }

        match = new CellMatch();

        return false;
    }

    public override bool Matching(HexCell[] neighbours, out HashSet<HexCell> otherCells)
    {
        otherCells = new HashSet<HexCell>();

        bool hasMatched = false;

        for(int i=0; i < 3; ++i)
        {
            if (couplesDone[i]) continue;
            HexCell a = neighbours[i * 2], b = neighbours[i * 2 + 1];
            if (MatchesWith(a, b, i))
            {
                hasMatched = true;
                couplesDone[i] = true;

                otherCells.Add(a);
                otherCells.Add(b);

                CellMatch.Match(this, a, b);
            }
        }

        return hasMatched;
    }

    public bool MatchingWithBonus(int bonusType, HexCell[] neighbours, out HashSet<HexCell> otherCells)
    {
        otherCells = new HashSet<HexCell>();

        bool hasMatched = false;

        for (int i = 0; i < 3; ++i)
        {
            if (couplesDone[i]) continue;
            HexCell a = neighbours[i * 2], b = neighbours[i * 2 + 1];
            if (HalfMatchesWith(a, i * 2) && BonusMatches(bonusType, i * 2 + 1))
            {
                hasMatched = true;
                couplesDone[i] = true;

                otherCells.Add(a);

                CellMatch.Match(this, a);
                match.bonusUsed = true;
            }
            else if (BonusMatches(bonusType, i * 2) && HalfMatchesWith(b, i * 2 + 1))
            {
                hasMatched = true;
                couplesDone[i] = true;

                otherCells.Add(b);

                CellMatch.Match(this, b);
                match.bonusUsed = true;
            }
        }

        return hasMatched;
    }

    public override void PreviewMatch(HexCell[] neighbours)
    {
        if (!grown) return;
        for (int i = 0; i < 3; ++i)
        {
            if (couplesDone[i]) continue;
            HexCell a = neighbours[i * 2], b = neighbours[i * 2 + 1];
            if (MatchesWith(a, b, i))
            {
                gonnaMatch = true;
                a.gonnaMatch = true;
                b.gonnaMatch = true;

				leafGonnaMatch[i * 2] = leafGonnaMatch[i * 2 + 1] = true;

			}
        }
    }

    public void PreviewMatchWithBonus(int bonusType, HexCell[] neighbours)
    {
        if (!grown) return;
        for (int i = 0; i < 3; ++i)
        {
            if (couplesDone[i]) continue;
            HexCell a = neighbours[i * 2], b = neighbours[i * 2 + 1];
            if (HalfMatchesWith(a,i*2) && BonusMatches(bonusType, i*2+1))
            {
                gonnaMatch = true;
                a.gonnaMatch = true;
            }
            else if (BonusMatches(bonusType, i * 2) && HalfMatchesWith(b, i * 2 + 1))
            {
                gonnaMatch = true;
                b.gonnaMatch = true;
            }
        }
    }

    bool MatchesWith(HexCell a, HexCell b, int coupleIdx)
    {
        return (HalfMatchesWith(a,coupleIdx*2) && HalfMatchesWith(b,coupleIdx*2+1)) || (HalfMatchesWith(b, coupleIdx * 2) && HalfMatchesWith(a, coupleIdx * 2 + 1));
    }

    bool HalfMatchesWith(HexCell other, int i)
    {
        return other != null && other.grown && other.type == needs[i];
    }

    bool BonusMatches(int bonusType, int i)
    {
        return bonusType == needs[i];
    }

	IEnumerator NeedySuccessAnim(float dur) {
		centerIcon.transform.parent = null;
        centerIcon.maskInteraction = SpriteMaskInteraction.None;
        healthDisplay.gameObject.SetActive(false);
		var initialScale = centerIcon.transform.localScale;
		for (float t = 0; t < 1; t += Time.deltaTime / dur) {
			centerIcon.color = new Color(1, 1, 1, 1 - t);
			centerIcon.transform.localScale = initialScale * (1 + t*5);
			yield return null;
		}
		Destroy(centerIcon.gameObject);
	}

	protected override IEnumerator HighLight() {
		float halfPeriod = .5f;
		while (true) {
			for (float t = 0; t < 1; t += Time.deltaTime / halfPeriod) {
				for (int i = 0; i < 6; ++i) {
					if (leafGonnaMatch[i]) leaves[i].transform.eulerAngles = 6 * Vector3.forward * Mathf.Sin(t * 2 * Mathf.PI);
					else leaves[i].transform.eulerAngles = Vector3.zero;
				}

				yield return null;
			}
		}
	}

	public override void StopAnim() {
		base.StopAnim();
		foreach (var leaf in leaves) leaf.transform.eulerAngles = Vector3.zero;
		leafGonnaMatch = new bool[6];
	}

	private void OnDestroy() {
		GridData.matchEvent -= MatchDone;
		GridData.postMatchEvent -= CheckDeath;
	}
}
