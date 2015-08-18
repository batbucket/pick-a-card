using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Blue : Card {

	const string COLOR = "Blue";
	const string PICKS_TAG = "BluePicks";
	const string INTERMEDIATE_LOCATION = "Blue/Intermediate";
	const string HITS_TAG = "BlueHits";
	const string FIZZLE_LOCATION = "Blue/Fizzle";

	//Bandaid solution to Blue card not getting initialized before Hand
	void Awake() {
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
