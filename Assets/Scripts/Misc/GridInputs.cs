﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridInputs : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public DebugHexagons grid;
	public void OnBeginDrag(PointerEventData eventData) {
		grid.StartDrag(eventData.position, eventData.delta);
	}

	public void OnDrag(PointerEventData eventData) {
		grid.UpdateDrag(eventData.delta);
	}

	public void OnEndDrag(PointerEventData eventData) {
		grid.StopDrag(eventData.delta);
	}
}
