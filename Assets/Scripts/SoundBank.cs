using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "ScriptableObjects/SoundBank")]
public class SoundBank : SingletonScriptableObject<SoundBank>
{
	[Header("Matching sounds")]
	[SerializeField]
	private AudioClip[] matchNotes;
	[SerializeField]
	private AudioClip[] needySounds;
	[SerializeField]
	float soundsDelay = .05f;
	[Header("Other sounds")]
	[SerializeField]
	private AudioClip[] shovelsounds;
	[SerializeField]
	private AudioClip needySpawn, levelUpSound, hurtSound, deathSound;

	public AudioSource prefab;


	[System.NonSerialized]
	int[] matchNotePool=null;
	[System.NonSerialized]
	int matchNoteIndex;

	[System.NonSerialized]
	float lastSoundTime = float.NegativeInfinity;

	public void Dig() {
		PlayInstant(shovelsounds[Random.Range(0, shovelsounds.Length)], .5f);
	}

	public void Hurt() {
		PlayInstant(hurtSound, 1);
	}
	public void Death() {
		PlayInstant(deathSound, 1);
	}

	public void LevelUp() {
		PlayInstant(levelUpSound);
	}

	public void NeedyAppears() {
		PlayInstant(needySpawn, .1f);
	}

	public void PlayMatchNote() {
		if (matchNotePool == null || matchNotePool.Length!=matchNotes.Length) {
			matchNotePool = Enumerable.Range(0, matchNotes.Length).ToArray();
			matchNotePool = matchNotePool.OrderBy(x => Random.value).ToArray();
		}

		else if(matchNoteIndex == 0) {
			var scrambled = matchNotePool.OrderBy(x => Random.value).ToArray();
			if(scrambled[0] == matchNotePool[matchNotePool.Length - 1]) {
				scrambled[0] = scrambled[scrambled.Length - 1];
				scrambled[scrambled.Length - 1] = matchNotePool[matchNotePool.Length - 1];
			}
			matchNotePool = scrambled;
		}

		AddSoundToQueue(matchNotes[matchNoteIndex]);

		matchNoteIndex++;
		matchNoteIndex %= matchNotePool.Length;
	}

	public void PlayNeedySound(int level) {
		AddSoundToQueue(needySounds[level - 1]);
	}

	void AddSoundToQueue(AudioClip sound) {
		if (Time.time > lastSoundTime + soundsDelay) {
			PlayInstant(sound);
			lastSoundTime = Time.time;
		}
		else {
			lastSoundTime += soundsDelay;
			HexCell.grid.StartCoroutine(PlaySoundAtTime(lastSoundTime + soundsDelay, sound));
		}
	}

	IEnumerator PlaySoundAtTime(float time, AudioClip sound) {
		yield return new WaitWhile(() => Time.time < time);
		PlayInstant(sound);
	}

	void PlayInstant(AudioClip sound, float volume = -1) {
		var source = Instantiate(prefab, GridMovements.cam.transform);
		if (volume > -1) source.volume = volume;
		source.PlayOneShot(sound);
		Destroy(source.gameObject, sound.length);
	}
}
