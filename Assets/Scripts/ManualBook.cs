using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualBook : MonoBehaviour
{
	public Transform pagesParent;
	public GameObject prevButton, nextButton;
	public TMPro.TextMeshProUGUI pageNumberL, pageNumberR;

	int _curPage;
	int curPage {
		get {
			return _curPage;
		}
		set {
			_curPage = Mathf.Clamp(value, 0, pagesParent.childCount - 1);
			foreach (Transform page in pagesParent) page.gameObject.SetActive(page.GetSiblingIndex() == _curPage);
			nextButton.SetActive(_curPage < pagesParent.childCount - 1);
			prevButton.SetActive(_curPage > 0);
			pageNumberL.text = (curPage * 2 + 1).ToString();
			pageNumberR.text = (curPage * 2 + 2).ToString();
		}
	}
	private void OnEnable() {
		curPage = 0;
	}

	public void NextPage() {
		curPage++;
	}

	public void PrevPage() {
		curPage--;
	}
}
