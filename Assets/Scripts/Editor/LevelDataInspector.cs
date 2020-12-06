using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelStartData))]
public class LevelDataInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorUtility.SetDirty(target);

        var _ = (LevelStartData)target;

        if(_.hexagonRadius*2+1 != _.cells.Length)
        {
            _.cells = new intArray[_.hexagonRadius * 2 + 1];
            for(int i=0; i < _.hexagonRadius * 2 + 1; ++i)
            {
                _.cells[i].array = new int[_.cells.Length - Mathf.Abs(_.hexagonRadius - i)];
            }
        }

        for(int i=0; i < _.cells.Length; ++i)
        {
            using(new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space((2*_.hexagonRadius-_.cells[i].array.Length) * 15);
                for(int j=0; j < _.cells[i].array.Length;++j)
                {
                    using(var scope = new EditorGUI.ChangeCheckScope())
                    {
                        _.cells[i][j] = EditorGUILayout.IntField(_.cells[i][j], GUILayout.Width(30));

                        if (scope.changed) Undo.RecordObject(_, "Change grid content");
                    }
                }
            }
        }
    }
}
