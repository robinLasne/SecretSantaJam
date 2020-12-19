using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPopup : MonoBehaviour
{
	public float animDur=1, rotationSpeed=200;

	SpriteRenderer sprite;

	public void InitAnim() {
		sprite = GetComponent<SpriteRenderer>();
		StartCoroutine(Animate());
	}

	IEnumerator Animate() {
		var startScale = transform.localScale;
		var spriteCol = sprite.color;

		for(float t=0; t<1; t += Time.deltaTime / animDur) {
			transform.localScale = startScale * (1-t);
			transform.localEulerAngles = new Vector3(0, 0, -t * rotationSpeed);

			spriteCol.a = t * 10;
			sprite.color = spriteCol;

			yield return null;
		}

		Destroy(gameObject);
	}
}
