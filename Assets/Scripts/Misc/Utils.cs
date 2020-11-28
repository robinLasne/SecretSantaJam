using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
	public static Vector2 rotate(Vector2 v, float delta) {
		return new Vector2(
			v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
			v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
		);
	}

	public static int IndexOfMin(this IEnumerable<int> source) {
		if (source == null)
			throw new ArgumentNullException("source");

		int minValue = int.MaxValue;
		int minIndex = -1;
		int index = -1;

		foreach (int num in source) {
			index++;

			if (num <= minValue) {
				minValue = num;
				minIndex = index;
			}
		}

		if (index == -1)
			throw new InvalidOperationException("Sequence was empty");

		return minIndex;
	}

}
