using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/**
 * Helper class
 */
public class Helper : MonoBehaviour {

	/**
	 * Converts an AudioClip to an AudioSource
	 */
	public static AudioSource clipToSource(AudioClip clip) {
		AudioSource source = new AudioSource();
		source.clip = clip;
		return source;
	}

	/**
	 * Mass version of above
	 */
	public static AudioSource[] clipToSourceArray(AudioClip[] clips) {
		AudioSource[] sources = new AudioSource[clips.Length];
		for (int i = 0; i < clips.Length; i++) {
			sources[i] = clipToSource(clips[i]);
		}
		return sources;
	}

	/**
	 * Mass version for getting AudioSources from GameObjects
	 */
	public static AudioSource[] massGetAudioSourceComponent(GameObject[] arr) {
		AudioSource[] sources = new AudioSource[arr.Length];
		for (int i = 0; i < arr.Length; i++) {
			sources[i] = arr[i].GetComponent<AudioSource>();
		}
		return sources;
	}

	/**
	 * Mass GameObject activator
	 */
	public static void massEnabler(GameObject[] arr) {
		for (int i = 0; i < arr.Length; i++) {
			arr[i].SetActive(true);
		}
	}
}
