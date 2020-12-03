using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NeedyCell : HexCell {
    public int[] needs = new int[6];
    public SpriteRenderer centerIcon;
    public SpriteRenderer[] leaves = new SpriteRenderer[6];

    int centerIconOrder, leavesOrder;

    bool[] couplesDone = new bool[3];

    public override int type {
        get {
            return 0;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        centerIconOrder = centerIcon.sortingOrder;
        leavesOrder = leaves[0].sortingOrder;

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

        bool complete = couplesDone.All(c => c);

        if (complete)
        {
            StartCoroutine(matchAnim(.3f));
        }

        return complete;
    }

    public override bool Matching(HexCell[] neighbours, out HashSet<HexCell> otherCells)
    {
        otherCells = new HashSet<HexCell>();

        bool hasMatched = false;

        for(int i=0; i < 3; ++i)
        {
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

    public override void SetInFront(bool state)
    {
        base.SetInFront(state);
        foreach(var leaf in leaves)leaf.sortingOrder = leavesOrder + (state ? 10 : 0);
        centerIcon.sortingOrder = centerIconOrder + (state ? 10 : 0);
    }
}
