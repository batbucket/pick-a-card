using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/**
 * Twisted Fate uses three different kinds of cards:
 * Blue, Red, and Gold
 * 
 * This class is a generic representation of a card that would be used
 */
public abstract class Card : MonoBehaviour {

	string cardName;

	AudioSource[] attackFlights; //Travel sound
	float flightDuration; //Duration of travel sound
	int flightCycle; //Track the current sound used

	AudioSource[] attackLands; //Impact sound
	int landCycle;

	Button card;
	AudioSource[] picks; //Quote TF says when picking a particular card
	int pickCycle;

	AudioSource intermediate; //Click sound made when a card is selected

	AudioSource[] hits; //Other impact sound since Pick A Card is an autoattack modifier
	int hitCycle;

	AudioSource fizzle; //Sound made when card times out

	// Use this for initialization
	void Start () {
	}

	//Overloaded!
	public void Start(string name, Button card, AudioSource[] pick, AudioSource intermediate, AudioSource[] impact, AudioSource fizzle) {
		this.cardName = name;
		this.card = card;
		this.picks = pick;
		this.intermediate = intermediate;
		this.hits = impact;
		this.fizzle = fizzle;

		attackFlights = Helper.massGetAudioSourceComponent(GameObject.FindGameObjectsWithTag("Throws"));
		attackLands = Helper.massGetAudioSourceComponent(GameObject.FindGameObjectsWithTag("Lands"));
		flightDuration = attackFlights[0].clip.length;

		Start ();	
	}

	/**
	 * Equality determined by color string
	 */
	public override bool Equals(object obj) 
	{
		// Check for null values and compare run-time types.
		if (obj == null || GetType() != obj.GetType()) 
			return false;
		
		Card c = (Card) obj;
		return this.cardName.Equals(c.cardName);
	}
	
	public override int GetHashCode() 
	{
		return cardName.GetHashCode();
	}

	public Button getButton() {
		return card;
	}

	public string getName() {
		return cardName;
	}

	
	public void visible(bool isVisible) {
		card.gameObject.SetActive(isVisible);
	}

	public void disable() {
		card.GetComponent<Button>().interactable = false;
	}

	public void enable() {
		card.GetComponent<Button>().interactable = true;
	}

	/**
	 * Cycles through pick sounds so each one
	 * sounds distinct
	 */
	public void playPick() {
		pickCycle %= picks.Length;
		picks[pickCycle++].Play();
	}

	public void playIntermediate() {
		intermediate.Play();
	}

	public void playHit() {
		hitCycle %= picks.Length;
		hits[hitCycle++].PlayDelayed(flightDuration); //Delayed until the card actually lands
	}

	public void playFizzle() {
		fizzle.Play();
	}

	public void playFlight() {
		flightCycle %= attackFlights.Length;
		attackFlights[flightCycle++].Play();
	}

	public void playLand() {
		landCycle %= attackLands.Length;
		attackLands[landCycle++].PlayDelayed(flightDuration);
	}

}
