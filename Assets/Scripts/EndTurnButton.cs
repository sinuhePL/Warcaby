using UnityEngine;
using System.Collections;

public class EndTurnButton : MonoBehaviour {
	private int playerTurn = 2;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseUp() {
		Board.moveEnded = false;
		if (playerTurn == 2) {
			playerTurn = 1;
			GameObject.FindWithTag ("Board").GetComponent<Board> ().computeMove ();
			Board.moveEnded = false;
			playerTurn = 2;
		}
		else playerTurn = 1;
	}
}
