using UnityEngine;
using System.Collections;

public class PawnScript : MonoBehaviour {

	public Material playerMaterial;
	public Material aiMaterial;
	private Vector3 lastPosition;
	private int playerID;
	private bool queen;
		
	// Use this for initialization
	void Start () {
		queen = false;
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	void OnMouseDown() {
		lastPosition = transform.position;
	}

	void OnMouseDrag() {
		float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
		Vector3 pos_move = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen ));
		transform.position = new Vector3( pos_move.x, 1.75f, pos_move.z );
	}

	void OnMouseUp() {
		bool properPosition = false;
		Vector3 newPosition = new Vector3(0.0f,0.0f,0.0f);
		float temp;

		if(!queen) newPosition.y = 1.05f;
		else newPosition.y = 1.4f;
		for (newPosition.x=0.5f; newPosition.x<8.0f; newPosition.x = newPosition.x+1.0f) {
			if(newPosition.x == 0.5f || newPosition.x == 2.5f || newPosition.x == 4.5f || newPosition.x == 6.5f) temp = 1.5f;
			else temp = 0.5f;
			for (newPosition.z=temp; newPosition.z<8.0f; newPosition.z = newPosition.z+2.0f) {
				if(Vector3.Distance(transform.position,newPosition) < 0.9f) {
					properPosition = true;
					break;
				}
			}
			if(properPosition) break;
		}
		if (properPosition && newPosition != lastPosition && GameObject.FindWithTag ("Board").GetComponent<Board> ().checkAndMakeMove(lastPosition, newPosition, playerID)) {
			transform.position = newPosition;
		} else {
			transform.position = lastPosition;
		}
	}

	public void initializePawn(int id) {
		playerID = id;
		if(id == 2) GetComponent<Renderer>().material = playerMaterial;
		else GetComponent<Renderer>().material = aiMaterial;
	}

	public bool isQueen() {
		return queen;
	}

	public void makeQueen() {
		queen = true;
		transform.localScale = new Vector3 (transform.localScale.x, 0.4f, transform.localScale.z);
		transform.position = new Vector3 (transform.position.x, 1.4f, transform.position.z);
	}

}
