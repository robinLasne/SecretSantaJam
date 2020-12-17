using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BonusPool : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
	public int type;

	SpriteRenderer mySprite;

	GameObject draggedInstance;

	public static Vector3Int lastCellPos;

	void Start() {
		mySprite = GetComponent<SpriteRenderer>();
	}

	public void OnBeginDrag(PointerEventData eventData) {
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

		Destroy(draggedInstance);
	}
}
