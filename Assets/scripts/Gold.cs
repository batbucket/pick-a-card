using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Gold : Card {

	const string COLOR = "Gold";
	const string PICKS_TAG = "GoldPicks";
	const string INTERMEDIATE_LOCATION = "Gold/Intermediate";
	const string HITS_TAG = "GoldHits";
	const string FIZZLE_LOCATION = "Gold/Fizzle";

	// Use this for initialization
	void Start () {
		Button card = gameObject.GetComponent<Button>();
		AudioSource[] picks = Helper.massGetAudioSourceComponent(GameObject.FindGameObjectsWithTag(PICKS_TAG));
		AudioSource intermediate = GameObject.Find (INTERMEDIATE_LOCATION).GetComponent<AudioSource>();
		AudioSource[] hits = Helper.massGetAudioSourceComponent(GameObject.FindGameObjectsWithTag(HITS_TAG));
		AudioSource fizzle = GameObject.Find(FIZZLE_LOCATION).GetComponent<AudioSource>();
		base.Start(COLOR, card, picks, intermediate, hits, fizzle);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
