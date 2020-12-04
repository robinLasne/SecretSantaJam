using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NeedyCell : HexCell {
    public int[] needs = new int[6];
    public SpriteRenderer centerIcon;
    public SpriteRenderer[] leaves = new SpriteRenderer[6];

    bool[] couplesDone = new bool[3];

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

    protected override void Awake()
    {
        base.Awake();

        for(int i = 0; i < 6; ++i)
        {
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

    public override void PreviewMatch(HexCell[] neighbours)
    {
        for (int i = 0; i < 3; ++i)
        {
            if (couplesDone[i]) continue;
            HexCell a = neighbours[i * 2], b = neighbours[i * 2 + 1];
            if (MatchesWith(a, b, i))
            {
                gonnaMatch = true;
                a.gonnaMatch = true;
                b.gonnaMatch = true;
            }
        }
    }

    bool MatchesWith(HexCell a, HexCell b, int i)
    {
        if (!grown || a == null || b == null || !a.grown || !b.grown) return false;

        int a_need = needs[i * 2], b_need = needs[i * 2 + 1];

        return (a != null && b != null && a.type == a_need && b.type == b_need);
    }
}
