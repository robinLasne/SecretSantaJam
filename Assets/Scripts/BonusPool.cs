using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BonusPool : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
	public int type;
	public TMPro.TextMeshPro countDisplay;

	public TMPro.TextMeshPro storageDisplay;

	SpriteRenderer mySprite;

	GameObject draggedInstance;

	public static Vector3Int lastCellPos;

	int currentAmount;
	int storedAmount;

	static List<BonusPool> instances = new List<BonusPool>();

	public string prefsKey {
		get {
			return PrefsKey(type);
		}
	}

	public static string PrefsKey(int type) {
		return string.Format("bonus{0}", type);
	}

	void Start() {
		mySprite = GetComponent<SpriteRenderer>();

		instances.Add(this);

		storageDisplay.text = PlayerPrefs.GetInt(prefsKey).ToString();
	}

	public void OnBeginDrag(PointerEventData eventData) {
		if (currentAmount <= 0) return;

		draggedInstance = new GameObject();
		draggedInstance.transform.localScale = transform.localScale/2;
		var newSprite = draggedInstance.AddComponent<SpriteRenderer>();
		newSprite.sprite = mySprite.sprite;
		newSprite.color = new Color(1, 1, 1, .6f);
		newSprite.sortingLayerID = mySprite.sortingLayerID;
	}

	public void OnDrag(PointerEventData eventData) {
		var pos = GridMovements.cam.ScreenToWorldPoint(eventData.position);
		pos.z = 0;
		draggedInstance.transform.position = pos;

		HexCell.grid.PreviewBonus(type, pos);
	}

	public void OnEndDrag(PointerEventData eventData) {
		HexCell.grid.PreviewBonus(type, transform.position);

		bool matched = HexCell.grid.TryBonusMatch(type, draggedInstance.transform.position);

		if (matched) {
			currentAmount--;
			countDisplay.text = currentAmount.ToString();
			PlayerPrefs.SetInt(prefsKey, storedAmount + currentAmount);
		}

		Destroy(draggedInstance);
	}

	public static void StartGame() {
		foreach(var pool in instances) {
			pool.currentAmount = PlayerPrefs.GetInt(pool.prefsKey);

			pool.storedAmount = 0;

			pool.countDisplay.text = pool.currentAmount.ToString();
		}
	}

	public static void CollectBonus(int type) {
		var pool = instances.Find(e => e.type == type);

		if (pool != null) {
			pool.storedAmount++;

			PlayerPrefs.SetInt(pool.prefsKey, pool.storedAmount + pool.currentAmount);
		}
	}

	public static IEnumerator AddCollectedToStored(float delay) {
		yield return new WaitForSeconds(delay);
		foreach(var pool in instances) {
			pool.storageDisplay.text = (pool.storedAmount + pool.currentAmount).ToString();

			if (pool.currentAmount > 0) {
				var popup = Instantiate(ScoreManager.ScorePopup, pool.storageDisplay.transform.position, Quaternion.identity);
				popup.InitAnim("+" + pool.currentAmount, pool.storageDisplay.color);
			}
		}
	}
}
