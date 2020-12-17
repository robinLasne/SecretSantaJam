using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualBook : MonoBehaviour
{
	public Transform pagesParent;

	int curPage;
	private void OnEnable() {

		foreach (Transform page in pagesParent) page.gameObject.SetActive(false);

		pagesParent.GetChild(0).gameObject.SetActive(true);
		curPage = 0;
	}

	public void NextPage() {
		if(curPage < pagesParent.childCount - 1) {
			pagesParent.GetChild(curPage).gameObject.SetActive(false);
			curPage++;
			pagesParent.GetChild(curPage).gameObject.SetActive(true);
		}
	}

	public void PrevPage() {
		if (curPage > 0) {
			pagesParent.GetChild(curPage).gameObject.SetActive(false);
			curPage--;
			pagesParent.GetChild(curPage).gameObject.SetActive(true);
		}
	}
}
