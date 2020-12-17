using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BonusPool))]
public class BonusPoolInspector : Editor
{
    BonusPool _ {
		get {
			return target as BonusPool;
		}
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		using(var change = new EditorGUI.ChangeCheckScope()) {
			int n = EditorGUILayout.IntField("Debug Amount",PlayerPrefs.GetInt(_.prefsKey));

			if (change.changed) {
				PlayerPrefs.SetInt(_.prefsKey, n);
			}
		}
	}
}
