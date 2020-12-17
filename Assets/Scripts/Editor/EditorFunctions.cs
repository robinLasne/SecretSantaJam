using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorFunctions
{
	[MenuItem("Game/ResetProgress")]
	public static void ResetProgress() {
		PlayerPrefs.SetInt("total_score", 0);
	}
}
