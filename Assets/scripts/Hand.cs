using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/**
 * The main class for simulating Twisted Fate's Pick A Card mechanic
 * 
 * Someone please find out what happens to the card timer in game when you fail to select a card.
 * This simulator currently just does a hard reset.
 */
public class Hand : MonoBehaviour {

	/**
	 * Used for the state machine
	 * Stasis: Spell hasn't been used
	 * Unchosen: Card hasn't been chosen
	 * Chosen: Card has been chosen
	 */
	public enum Status {
		Stasis, Unchosen, Chosen
	}

	Card blue;
	Card red;
	Card gold;

	Button[] buttons;
	Button spellButton; //The Pick A Card spell icon
	Button debugButton; //Debug button hidden on gold card
	Button resetButton; //Reset button hidden on red card

	Card[] cards;
	const int NUMBER_OF_CARDS = 3;

	Card current;
	const float TIME_PER_CARD = 0.4f; //.4 seconds per card on the shuffle component
	const float MAX_CYCLE_TIME = 6.0f; //Cards will shuffle for 6 seconds before the ability ends
	const float MAX_THROW_TIME = 6.0f; //When a card is chosen one has 6 seconds to throw it before it ends
	float perCardTime; //Time on the current card during the shuffle sequence
	float cycleTime; //Time between spell cast and choosing a card
	float throwTime; //Time between card being chosen and thrown
	float notPlayingTime; //Time between shuffles

	AudioSource[] flips; //Transition sound between cards during the shuffle
	int flipCycle; //Tracks the current sound used for cycling

	AudioSource[] destinies; //Debug view sound
	int destinyCycle;

	AudioSource[] gates ; //Debug exit sound
	int gateCycle;

	AudioSource hat; //Don't touch it

	Status status; //Status for the state machine

	bool debug; //Tracks if debug view is turned on

	const string BRANDON_EXCEPTION_TEXT = "Card is somehow null. NICE ONE BRANDON."; //Text exception makes when a null card is picked
	const string TRUNCATION_CODE = "F2"; //Truncates 2 decimal digits
	const string DEBUG_ON_TEXT = "Debug view activated.";
	const string DEBUG_OFF_TEXT = "Debug view disabled.";
	const string RESET_TEXT = "Card order and timers reset.";

	const float SHAKE_THRESHOLD = 2.0f; //How strong a shake has to trigger the cards
	const float SHAKE_DELAY = 1.0f; //Delay so you don't activate all 3 things at once

	// Use this for initialization
	void Start () {

		/**
		 * Assign references to game objects
		 */
		blue = GameObject.Find("Cards/Blue").GetComponent<Card>();
		red = GameObject.Find("Cards/Red").GetComponent<Card>();
		gold = GameObject.Find("Cards/Gold").GetComponent<Card>();
		spellButton = GameObject.Find ("Image/Spell").GetComponent<Button>();

		/**
		 * The spell button will now start the Pick A Card sequence
		 */
		spellButton.onClick.AddListener(() => {
			pickACard();
		});

		/**
		 * Array view of the cards
		 */
		cards = new Card[NUMBER_OF_CARDS];
		cards[0] = blue;
		cards[1] = red;
		cards[2] = gold;

		/**
		 * Sets up sounds for the cards
		 */
		for (int i = 0; i < cards.Length; i++) {
			setupListeners(cards[i]);
		}

		/*
		 * TF starts on a blue card
		 */
		current = blue;

		flips = Helper.massGetAudioSourceComponent(GameObject.FindGameObjectsWithTag("Flips")); //Get all flip sounds
		hat = GameObject.Find("Debug/Hat").GetComponent<AudioSource>(); //Seriously don't touch it
		destinies = Helper.massGetAudioSourceComponent(GameObject.FindGameObjectsWithTag("Destiny")); //Get destiny quotes
		gates = Helper.massGetAudioSourceComponent(GameObject.FindGameObjectsWithTag("Gate")); //Get gate sounds

		status = Status.Stasis; //Starting status is stasis

		hideAll(); //Hide all the cards that are shown initially for ease of access

		/**
		 * Toggle debugger mode hidden on gold card in background
		 */
		this.debugButton = GameObject.Find("Debug/ToggleDebug").GetComponent<Button>();
		debugButton.onClick.AddListener(() => {
			debug = !debug;

			if (debug) {
				Debug.Log (DEBUG_ON_TEXT);
				playDestiny();
			} else {
				Debug.Log (DEBUG_OFF_TEXT);
				playGate();
			}
		});

		/**
		 * Set up reset button hidden on hat in background
		 */
		this.resetButton = GameObject.Find("Debug/ToggleReset").GetComponent<Button>();
		resetButton.onClick.AddListener(() => {
			doNotTouchHat();
			hardReset();
			Debug.Log (RESET_TEXT);
		});



		buttons = new Button[] {spellButton, debugButton, resetButton};


	}

	/**
	 * Creates the various sounds and effects for the cards on click
	 */
	void setupListeners(Card card) {
		card.getButton().onClick.AddListener(() => {

			/**
			 * Clicking on a card that you have chosen will throw it
			 */
			if (status.Equals(Status.Chosen)) {
				cardThrown(card);

			/**
			 * Otherwise clicking on a card will pick it
			 */
			} else {
				cardPicked(card);
			}
		});
	}

	void cardThrown(Card card) {
		card.playFlight(); //This is played first
		card.playLand();
		card.playHit();
		reset();
	}
	
	void cardPicked(Card card) {
		card.playPick();
		card.playIntermediate();
		status = Status.Chosen;
	}
	
	/**
	 * Start shuffling cards
	 */
	void pickACard() {
		status = Status.Unchosen;
		CancelInvoke(); //Guard against spamming the spell button, only the last one will go through
		showCard(current);
		playFlip();
		InvokeRepeating("nextCard", TIME_PER_CARD - perCardTime, TIME_PER_CARD); //Saves last card's time just like in the game
	}

	void incrementPerCardTimer() {
		perCardTime %= TIME_PER_CARD;
		perCardTime += Time.deltaTime;
	}

	void incrementCycleTimer() {
		cycleTime += Time.deltaTime;
	}

	void incrementThrowTimer() {
		throwTime += Time.deltaTime;
	}

	void incrementNotPlayingTimer() {
		notPlayingTime += Time.deltaTime;
	}

	void clearPerCardTimer() {
		perCardTime = 0;
	}

	void clearCycleTime() {
		cycleTime = 0;
	}

	void clearThrowTimer() {
		throwTime = 0;
	}

	void clearNotPlayingTimer() {
		notPlayingTime = 0;
	}

	float totalTime() {
		return cycleTime + throwTime;
	}

	void showCard(Card card) {
		if (card.Equals(blue)) {
			showBlue();
		} else if (card.Equals(red)) {
			showRed();
		} else if (card.Equals(gold)) {
			showGold();
		} else { //This should never happen
			throwUnidentifiedBrandonException();
		}
	}

	Card nextCard(Card card) {
		if (card.Equals(blue)) {
			return red;
		} else if (card.Equals(red)) {
			return gold;
		} else if (card.Equals(gold)) {
			return blue;
		} else { //This should never happen
			throwUnidentifiedBrandonException();
			return blue;
		}
	}

	void nextCard() {
		current = nextCard(current);
		showCard(current);
		playFlip();
	}

	void showBlue() {
		blue.visible(true);
		red.visible(false);
		gold.visible (false);
	}

	void showRed() {
		red.visible(true);
		blue.visible (false);
		gold.visible (false);
	}

	void showGold() {
		gold.visible(true);
		blue.visible(false);
		red.visible (false);
	}

	void hideAll() {
		blue.visible(false);
		red.visible(false);
		gold.visible(false);
	}

	void playFlip() {
		flipCycle %= flips.Length;
		flips[flipCycle++].Play();
	}

	void playDestiny() {
		destinyCycle %= destinies.Length;
		destinies[destinyCycle++].Play();
	}

	void playGate() {
		gateCycle %= gates.Length;
		gates[gateCycle++].Play();
	}

	void reset() {
		CancelInvoke();
		status = Status.Stasis;
		hideAll();
		clearCycleTime();
		clearThrowTimer();
		clearNotPlayingTimer();
	}

	/**
	 * Default card is blue
	 */
	void setCurrentToDefault() {
		current = blue;
	}

	/**
	 * Resets everything including card timers and current card
	 */
	void hardReset() {
		CancelInvoke();
		status = Status.Stasis;
		hideAll();
		clearPerCardTimer();
		clearCycleTime();
		clearThrowTimer();
		setCurrentToDefault();

	}

	string getCurrentCardName() {
		return current.getName();
	}

	void throwUnidentifiedBrandonException() {
		throw new UnityException(BRANDON_EXCEPTION_TEXT);
	}

	void doNotTouchHat() {
		hat.Play();
	}

	// Update is called once per frame
	void Update () {

		if (status.Equals(Status.Stasis)) {
			incrementNotPlayingTimer();
		}

		//The state machine
			
		/**
		 * Ensures that you can't click through a card to a button
		 */
		if (!status.Equals(Status.Stasis)) {
			for (int i = 0; i < buttons.Length; i++) {
				buttons[i].gameObject.SetActive(false);
			}
		} else {
			for (int i = 0; i < buttons.Length; i++) {
				buttons[i].gameObject.SetActive(true);
			}
		}

		/**
		 * Card has been chosen
		 */
		if (status.Equals(Status.Chosen)) {
			CancelInvoke(); //Stop shuffling cards
			incrementThrowTimer(); //Start counting down for how long you have to throw it
			if (throwTime > MAX_THROW_TIME) { //Passed the throw time limit
				current.playFizzle();
				reset(); //Reset cards but not the current or its timer
			}
		}

		/**
		 * No card has been chosen
		 */
		if (status.Equals(Status.Unchosen)) {
			incrementPerCardTimer(); //Start counting time per card transition
			incrementCycleTimer(); //Start counting total time you have to pick a card
			if (cycleTime > MAX_CYCLE_TIME) {
				hardReset(); //Not sure if this is how it actually works, but if you don't pick a card and it times out, you start all over in this simulator
			}
		}

		/**
		 * Shake detection for card throwing
		 */
		if (Input.acceleration.magnitude > SHAKE_THRESHOLD) {
			Debug.Log (status);
			if (status.Equals(Status.Stasis) && notPlayingTime > SHAKE_DELAY) {
				pickACard();
				
			} else if (status.Equals(Status.Unchosen) && cycleTime > SHAKE_DELAY) {
				cardPicked(current);
			} else if (status.Equals(Status.Chosen) && throwTime > SHAKE_DELAY) {
				cardThrown(current);
			}
		}

		/**
		 * Debug view display
		 */
		if (debug) {
			GameObject.Find("Debug/DebugText").GetComponent<Text>().text =
				"Debug view activated. \n\n Toggle off by tapping on the gold card in the top right corner. \n\n Tapping his hat in the background will reset your timers and current card. \n\n" +

					"Current card color: \n" + getCurrentCardName() + ". \n\n" +
					"Time remaining on this card: \n" + Mathf.Max((TIME_PER_CARD - perCardTime), 0).ToString(TRUNCATION_CODE) + " seconds. \n\n"+
					"Time remaining to pick a card: \n" +  Mathf.Max((MAX_CYCLE_TIME - cycleTime), 0).ToString(TRUNCATION_CODE) + " seconds. \n\n" +
					"Time remaining to throw a card: \n" + Mathf.Max((MAX_THROW_TIME - throwTime), 0).ToString(TRUNCATION_CODE) + " seconds.";
		} else {
			GameObject.Find("Debug/DebugText").GetComponent<Text>().text = "";
		}
	}
}
