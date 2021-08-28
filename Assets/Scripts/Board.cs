using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Board : MonoBehaviour {
	public GameObject pawnPrefab;
    public Text endText;
	public static bool moveEnded = false;

	public class Move {
		public int startField;
		public int endField;
		public List<int> delField;
		public Move(int s, int e, List<int> d) {
			startField = s;
			endField = e;
			delField = d;
		}
		public void setStartField(int sf) {
			startField = sf;
		}

		public bool uniqueBeats() {
			int i, j;

			if(delField == null) {
				return false;
			}
			else {
				for(i=0;i<delField.Count-1;i++) {
					for(j=i+1;j<delField.Count;j++) {
						if(delField[i] == delField[j]) return false;
					}
				}
				return true;
			}
		}
	} // end of class Move

	private class GameBoard {
		private class BoardField {
			public BoardField rightTop, rightBottom, leftBottom, leftTop;
			private Vector3 position;
			private GameObject myPawn;
			private int playerID;
			private int fieldID;
			private bool queen;

			public BoardField(float x, float z, int i) {
				position = new Vector3(x, 1.05f, z);
				myPawn = null;
				playerID = 0;
				fieldID = i;
				queen = false;
			}

			public void setPawn(GameObject tempPawn, int pID, bool queenFlag){
				myPawn = tempPawn;
				playerID = pID;
				queen = queenFlag;
			}

			public void emptyField() {
				myPawn = null;
				playerID = 0;
				queen = false;
			}

			public void delPawn() {
				GameObject.Destroy (myPawn);
				myPawn = null;
				playerID = 0;
				queen = false;
			}

			public GameObject getPawn() {
				return myPawn;
			}

			public bool isOccupied() {
				if (myPawn == null && playerID == 0) {
					return false;
				} 
				else {
					return true;
				}
			}

			public Vector3 getPosition() {
				return position;
			}

			public int getPlayerID() {
				return playerID;
			}

			public void setPlayerID(int id) {
				playerID = id;
			}

			public void setID(int i) {
				fieldID = i;
			}

			public int getID() {
				return fieldID;
			}

			public bool isQueen() {
				return queen;
			}

			public void makeQueen() {
				queen = true;
				if(myPawn != null) myPawn.GetComponent<PawnScript> ().makeQueen ();
			}

		}// end of class BoardField

		private BoardField[] fieldList;
		private Board myBoard;

		private void initializeBoard() {
			int i;

			fieldList = new BoardField[32];
			for (i=0; i<32; i++) { // liczymy od lewego gornego rogu
				if ((i / 4) % 2 == 0) { // jesli rzad parzysty
					fieldList [i] = new BoardField (0.5f + Mathf.Floor (i / 4), 1.5f + 2 * (i % 4), i);
				} else {
					fieldList [i] = new BoardField (0.5f + Mathf.Floor (i / 4), 0.5f + 2 * (i % 4), i);
				}
			}
			for(i=0;i<32;i++) {	// ustawia referencje na sąsiednie pola
				if(Mathf.Floor(i/4) == 0 || (i+4)%8 == 0) fieldList[i].leftTop = null; // wskażnik na górne lewe pole
				else if(Mathf.Floor (i/4)%2 == 0) {
					fieldList[i].leftTop = fieldList[i-4];
				}
				else {
					fieldList[i].leftTop = fieldList[i-5];
				}
				if(Mathf.Floor(i/4) == 0 || (i+5)%8 == 0) fieldList[i].rightTop = null; // wskażnik na górne prawe pole
				else if(Mathf.Floor (i/4)%2 == 0) {
					fieldList[i].rightTop = fieldList[i-3];
				}
				else {
					fieldList[i].rightTop = fieldList[i-4];
				}
				if(Mathf.Floor(i/4) == 7 || (i+4)%8 == 0) fieldList[i].leftBottom = null; // wskażnik na dolne lewe pole
				else if(Mathf.Floor (i/4)%2 == 0) {
					fieldList[i].leftBottom = fieldList[i+4];
				}
				else {
					fieldList[i].leftBottom = fieldList[i+3];
				}
				if(Mathf.Floor(i/4) == 7 || (i+5)%8 == 0) fieldList[i].rightBottom = null; // wskażnik na dolne prawe pole
				else if(Mathf.Floor (i/4)%2 == 0) {
					fieldList[i].rightBottom = fieldList[i+5];
				}
				else {
					fieldList[i].rightBottom = fieldList[i+4];
				}
			}
		}

		public GameBoard(GameBoard modelBoard) {	// konstruktor kopiujący
			int i;

			initializeBoard();
			myBoard = null;
			for(i=0;i<32;i++) {
				fieldList[i].setPlayerID(modelBoard.fieldList[i].getPlayerID());
				if(modelBoard.fieldList[i].isQueen()) fieldList[i].makeQueen();
			}
		}

		public GameBoard(Board tempBoard) {		// konstruktor
			int i;
			GameObject tempPawn;

			initializeBoard();
			myBoard = tempBoard;
			for(i=0;i<32;i++) { // liczymy od lewego gornego rogu
				if(i<12) {
					tempPawn = Instantiate(myBoard.pawnPrefab, fieldList[i].getPosition(), Quaternion.identity) as GameObject;
					fieldList[i].setPawn(tempPawn,1, false);
					fieldList[i].getPawn().GetComponent<PawnScript>().initializePawn(1);
				}
				if(i>19) {
					tempPawn = Instantiate(myBoard.pawnPrefab, fieldList[i].getPosition(), Quaternion.identity) as GameObject;
					fieldList[i].setPawn(tempPawn,2, false);
					fieldList[i].getPawn().GetComponent<PawnScript>().initializePawn(2);
				}
			}
		}


		public bool isMoveOK(Move move, int p_playerID) {
			BoardField tempBF;

			if (fieldList [move.endField].isOccupied ()) {	// jeśli pole docelowe zajęte cofnij ruch
				return false;
			}
			if (fieldList [move.startField].isQueen()) {
				tempBF = fieldList [move.startField];
				while(tempBF.leftBottom != null) {
					if(tempBF.leftBottom.getID() == move.endField) {
						return true;
					} 
					if(tempBF.leftBottom.isOccupied() && tempBF.leftBottom.getPlayerID() == p_playerID) {
						break;
					}
					if(tempBF.leftBottom.isOccupied() && tempBF.leftBottom.leftBottom != null && tempBF.leftBottom.leftBottom.isOccupied()) {
						break;
					}
					tempBF = tempBF.leftBottom;
				}
				tempBF = fieldList [move.startField];
				while(tempBF.leftTop != null) {
					if(tempBF.leftTop.getID() == move.endField) {
						return true;
					} 
					if(tempBF.leftTop.isOccupied() && tempBF.leftTop.getPlayerID() == p_playerID) {
						break;
					}
					if(tempBF.leftTop.isOccupied() && tempBF.leftTop.leftTop != null && tempBF.leftTop.leftTop.isOccupied()) {
						break;
					}
					tempBF = tempBF.leftTop;
				}
				tempBF = fieldList [move.startField];
				while(tempBF.rightBottom != null) {
					if(tempBF.rightBottom.getID() == move.endField) {
						return true;
					} 
					if(tempBF.rightBottom.isOccupied() && tempBF.rightBottom.getPlayerID() == p_playerID) {
						break;
					}
					if(tempBF.rightBottom.isOccupied() && tempBF.rightBottom.rightBottom != null && tempBF.rightBottom.rightBottom.isOccupied()) {
						break;
					}
					tempBF = tempBF.rightBottom;
				}
				tempBF = fieldList [move.startField];
				while(tempBF.rightTop != null) {
					if(tempBF.rightTop.getID() == move.endField) {
						return true;
					} 
					if(tempBF.rightTop.isOccupied() && tempBF.rightTop.getPlayerID() == p_playerID) {
						break;
					}
					if(tempBF.rightTop.isOccupied() && tempBF.rightTop.rightTop != null && tempBF.rightTop.rightTop.isOccupied()) {
						break;
					}
					tempBF = tempBF.rightTop;
				}
				return false;
			} else {
				if ((fieldList [move.startField].leftTop == fieldList [move.endField] || fieldList [move.startField].rightBottom == fieldList [move.endField]) && p_playerID == 1 && !Board.moveEnded) { // jeśli pole docelowe sąsiaduje z polem początkowym i w tej turze nie był wykonany ruch i jest to tura pierwszego gracza
					return  true;
				} 
				if ((fieldList [move.startField].leftTop == fieldList [move.endField] || fieldList [move.startField].rightTop == fieldList [move.endField]) && p_playerID == 2 && !Board.moveEnded) {	// jeśli pole docelowe sąsiaduje z polem początkowym i w tej turze nie był wykonany ruch i jest to tura drugiego gracza
					return true;
				}
				if (fieldList [move.startField].leftBottom != null && fieldList [move.startField].leftBottom == fieldList [move.endField].rightTop && fieldList [move.startField].leftBottom.isOccupied () && fieldList [move.startField].leftBottom.getPlayerID () != fieldList [move.startField].getPlayerID ()) {	// jeśli bicie w lewy dół
					return true;
				}
				if (fieldList [move.startField].rightBottom != null && fieldList [move.startField].rightBottom == fieldList [move.endField].leftTop && fieldList [move.startField].rightBottom.isOccupied () && fieldList [move.startField].rightBottom.getPlayerID () != fieldList [move.startField].getPlayerID ()) {	// jeśli bicie w prawy dół
					return true;
				}
				if (fieldList [move.startField].leftTop != null && fieldList [move.startField].leftTop == fieldList [move.endField].rightBottom && fieldList [move.startField].leftTop.isOccupied () && fieldList [move.startField].leftTop.getPlayerID () != fieldList [move.startField].getPlayerID ()) {	// jeśli bicie w lewą górę
					return true;
				}
				if (fieldList [move.startField].rightTop != null && fieldList [move.startField].rightTop == fieldList [move.endField].leftBottom && fieldList [move.startField].rightTop.isOccupied () && fieldList [move.startField].rightTop.getPlayerID () != fieldList [move.startField].getPlayerID ()) {	// jeśli bicie w prawą górę
					return true;
				}
			}
			return false;
		}

		public Move addBeats(Move move) {
			BoardField tempField;

			if (fieldList [move.startField].isQueen ()) {
				tempField = fieldList[move.startField].leftBottom;
				while(tempField != null && tempField != fieldList[move.endField]) {
					tempField = tempField.leftBottom;
				}
				if(tempField == fieldList[move.endField]) {
					tempField = fieldList[move.startField].leftBottom;
					while(tempField != null && !tempField.isOccupied() && tempField != fieldList[move.endField]) {
						tempField = tempField.leftBottom;
					}
					if(tempField != null && tempField != fieldList[move.endField]) move.delField.Add(tempField.getID());
					return move;
				}
				tempField = fieldList[move.startField].leftTop;
				while(tempField != null && tempField != fieldList[move.endField]) {
					tempField = tempField.leftTop;
				}
				if(tempField == fieldList[move.endField]) {
					tempField = fieldList[move.startField].leftTop;
					while(tempField != null && !tempField.isOccupied() && tempField != fieldList[move.endField]) {
						tempField = tempField.leftTop;
					}
					if(tempField != null && tempField != fieldList[move.endField]) move.delField.Add(tempField.getID());
					return move;
				}
				tempField = fieldList[move.startField].rightBottom;
				while(tempField != null && tempField != fieldList[move.endField]) {
					tempField = tempField.rightBottom;
				}
				if(tempField == fieldList[move.endField]) {
					tempField = fieldList[move.startField].rightBottom;
					while(tempField != null && !tempField.isOccupied() && tempField != fieldList[move.endField]) {
						tempField = tempField.rightBottom;
					}
					if(tempField != null && tempField != fieldList[move.endField]) move.delField.Add(tempField.getID());
					return move;
				}
				tempField = fieldList[move.startField].rightTop;
				while(tempField != null && tempField != fieldList[move.endField]) {
					tempField = tempField.rightTop;
				}
				if(tempField == fieldList[move.endField]) {
					tempField = fieldList[move.startField].rightTop;
					while(tempField != null && !tempField.isOccupied() && tempField != fieldList[move.endField]) {
						tempField = tempField.rightTop;
					}
					if(tempField != null && tempField != fieldList[move.endField]) move.delField.Add(tempField.getID());
					return move;
				}
				
			} else {
				if (fieldList [move.startField].leftBottom != null && fieldList [move.startField].leftBottom == fieldList [move.endField].rightTop)  {
					move.delField.Add(fieldList [move.startField].leftBottom.getID());
					return move;
				}
				if (fieldList [move.startField].rightBottom != null && fieldList [move.startField].rightBottom == fieldList [move.endField].leftTop) {
					move.delField.Add(fieldList [move.startField].rightBottom.getID());
					return move;
				}
				if (fieldList [move.startField].leftTop != null && fieldList [move.startField].leftTop == fieldList [move.endField].rightBottom) {
					move.delField.Add(fieldList [move.startField].leftTop.getID());
					return move;
				}
				if (fieldList [move.startField].rightTop != null && fieldList [move.startField].rightTop == fieldList [move.endField].leftBottom) {
					move.delField.Add(fieldList [move.startField].rightTop.getID());
					return move;
				}
			}
			return move;
		}

		public void makeMove(Move move) {
			int i;

			for (i=0; i<move.delField.Count; i++) {	// usuwa zbite piony
				fieldList [move.delField [i]].delPawn ();
			}
			if (fieldList [move.startField].getPlayerID () == 1 && move.endField > 27 || fieldList [move.startField].getPlayerID () == 2 && move.endField < 4) {	// wykrywa awans na damkę
				fieldList[move.startField].makeQueen();
			}
			fieldList [move.endField].setPawn(fieldList [move.startField].getPawn(), fieldList [move.startField].getPlayerID(), fieldList[move.startField].isQueen()); // stawia pion w nowym miejscu
			fieldList [move.startField].emptyField();	// usuwa pion ze starego miejsca
			Board.moveEnded = true;
		}

		public void visualiseMove(Move myMove) {
			Vector3 tempVector;

			tempVector = fieldList [myMove.endField].getPosition ();
			fieldList [myMove.startField].getPawn ().transform.position = new Vector3(tempVector.x,fieldList [myMove.startField].getPawn ().transform.position.y,tempVector.z);
			makeMove (myMove);
		}

		private List<Move> lookForBeat(int fID, int pID, List<int> prevFields, bool isQueen){
			List<Move> ltBeatList = null, lbBeatList = null, rbBeatList = null, rtBeatList = null, tempList;
			Move tempMove;
			BoardField tempField;
			int i, pawnPassed;
			List<int> tempPrevFields;

			tempList = new List<Move>();
			if (isQueen) {
				tempField = fieldList[fID].leftBottom;
				pawnPassed = -1;
				while(tempField != null) {
					if(tempField.isOccupied() && tempField.getPlayerID() == pID) break;
					if(prevFields.Contains(tempField.getID())) break;
					if(pawnPassed >= 0 && tempField.isOccupied()) break;
					if(tempField.isOccupied() && tempField.getPlayerID() != pID) pawnPassed = tempField.getID();
					if(pawnPassed >= 0 && !tempField.isOccupied()) {
						tempPrevFields = new List<int>(prevFields);
						tempPrevFields.Add (pawnPassed);	// dla damki prev_fields zawiera pola na ktorych stały zbite w tym ruchu piony
						lbBeatList = lookForBeat (tempField.getID(), pID, tempPrevFields, true);
						for (i=0; i<lbBeatList.Count; i++) {
							lbBeatList [i].delField.Add (pawnPassed);
							lbBeatList [i].setStartField (fID);
						}
						tempList.AddRange(lbBeatList);
					}
					tempField = tempField.leftBottom;
				}
				tempField = fieldList[fID].leftTop;
				pawnPassed = -1;
				while(tempField != null) {
					if(tempField.isOccupied() && tempField.getPlayerID() == pID) break;
					if(prevFields.Contains(tempField.getID())) break;
					if(pawnPassed >= 0 && tempField.isOccupied()) break;
					if(tempField.isOccupied() && tempField.getPlayerID() != pID) pawnPassed = tempField.getID();
					if(pawnPassed >= 0 && !tempField.isOccupied()) {
						tempPrevFields = new List<int>(prevFields);
						tempPrevFields.Add (pawnPassed);
						ltBeatList = lookForBeat (tempField.getID(), pID, tempPrevFields, true);
						for (i=0; i<ltBeatList.Count; i++) {
							ltBeatList [i].delField.Add (pawnPassed);
							ltBeatList [i].setStartField (fID);
						}
						tempList.AddRange(ltBeatList);
					}
					tempField = tempField.leftTop;
				}
				tempField = fieldList[fID].rightBottom;
				pawnPassed = -1;
				while(tempField != null) {
					if(tempField.isOccupied() && tempField.getPlayerID() == pID) break;
					if(prevFields.Contains(tempField.getID())) break;
					if(pawnPassed >= 0 && tempField.isOccupied()) break;
					if(tempField.isOccupied() && tempField.getPlayerID() != pID) pawnPassed = tempField.getID();
					if(pawnPassed >= 0 && !tempField.isOccupied()) {
						tempPrevFields = new List<int>(prevFields);
						tempPrevFields.Add (pawnPassed);
						rbBeatList = lookForBeat (tempField.getID(), pID, tempPrevFields, true);
						for (i=0; i<rbBeatList.Count; i++) {
							rbBeatList [i].delField.Add (pawnPassed);
							rbBeatList [i].setStartField (fID);
						}
						tempList.AddRange(rbBeatList);
					}
					tempField = tempField.rightBottom;
				}
				tempField = fieldList[fID].rightTop;
				pawnPassed = -1;
				while(tempField != null) {
					if(tempField.isOccupied() && tempField.getPlayerID() == pID) break;
					if(prevFields.Contains(tempField.getID())) break;
					if(pawnPassed >= 0 && tempField.isOccupied()) break;
					if(tempField.isOccupied() && tempField.getPlayerID() != pID) pawnPassed = tempField.getID();
					if(pawnPassed >= 0 && !tempField.isOccupied()) {
						tempPrevFields = new List<int>(prevFields);
						tempPrevFields.Add (pawnPassed);
						rtBeatList = lookForBeat (tempField.getID(), pID, tempPrevFields, true);
						for (i=0; i<rtBeatList.Count; i++) {
							rtBeatList [i].delField.Add (pawnPassed);
							rtBeatList [i].setStartField (fID);
						}
						tempList.AddRange(rtBeatList);
					}
					tempField = tempField.rightTop;
				}
			} else { 
				/* warunki znalezienia bicia
				 * 1. pole sasiednie istnieje
				 * 2. pole za sasiadnim istnieje
				 * 3. pole za sasiednim nie bylo juz sprawdzane
				 * 4. na pole sasiednim znajduje sie pionek
				 * 5. pionek na sasiednim polu nalezy do innego gracza niz pionek ktorego ruch jest rozpatrywany
				 * 6. pole za sasiednim jest wolne
				 */
				prevFields.Add (fID);	// dla piona prev fields zawiera pola na których w tym urchu był pion wcześniej
				if (fieldList [fID].leftBottom != null && fieldList [fID].leftBottom.leftBottom != null && !prevFields.Contains (fieldList [fID].leftBottom.leftBottom.getID ()) && fieldList [fID].leftBottom.isOccupied () && pID != fieldList [fID].leftBottom.getPlayerID () && !fieldList [fID].leftBottom.leftBottom.isOccupied ()) {
					lbBeatList = lookForBeat (fieldList [fID].leftBottom.leftBottom.getID (), pID, prevFields, false);
					for (i=0; i<lbBeatList.Count; i++) {
						lbBeatList [i].delField.Add (fieldList [fID].leftBottom.getID ());
						lbBeatList [i].setStartField (fID);
					}
					tempList.AddRange(lbBeatList);
				}
				if (fieldList [fID].leftTop != null && fieldList [fID].leftTop.leftTop != null && !prevFields.Contains (fieldList [fID].leftTop.leftTop.getID ()) && fieldList [fID].leftTop.isOccupied () && pID != fieldList [fID].leftTop.getPlayerID () && !fieldList [fID].leftTop.leftTop.isOccupied ()) {
					ltBeatList = lookForBeat (fieldList [fID].leftTop.leftTop.getID (), pID, prevFields, false);
					for (i=0; i<ltBeatList.Count; i++) {
						ltBeatList [i].delField.Add (fieldList [fID].leftTop.getID ());
						ltBeatList [i].setStartField (fID);
					}
					tempList.AddRange(ltBeatList);
				}
				if (fieldList [fID].rightBottom != null && fieldList [fID].rightBottom.rightBottom != null && !prevFields.Contains (fieldList [fID].rightBottom.rightBottom.getID ()) && fieldList [fID].rightBottom.isOccupied () && pID != fieldList [fID].rightBottom.getPlayerID () && !fieldList [fID].rightBottom.rightBottom.isOccupied ()) {
					rbBeatList = lookForBeat (fieldList [fID].rightBottom.rightBottom.getID (), pID, prevFields, false);
					for (i=0; i<rbBeatList.Count; i++) {
						rbBeatList [i].delField.Add (fieldList [fID].rightBottom.getID ());
						rbBeatList [i].setStartField (fID);
					}
					tempList.AddRange(rbBeatList);
				}
				if (fieldList [fID].rightTop != null && fieldList [fID].rightTop.rightTop != null && !prevFields.Contains (fieldList [fID].rightTop.rightTop.getID ()) && fieldList [fID].rightTop.isOccupied () && pID != fieldList [fID].rightTop.getPlayerID () && !fieldList [fID].rightTop.rightTop.isOccupied ()) {
					rtBeatList = lookForBeat (fieldList [fID].rightTop.rightTop.getID (), pID, prevFields, false);
					for (i=0; i<rtBeatList.Count; i++) {
						rtBeatList [i].delField.Add (fieldList [fID].rightTop.getID ());
						rtBeatList [i].setStartField (fID);
					}
					tempList.AddRange(rtBeatList);
				}
			}
			if (lbBeatList == null && ltBeatList == null && rbBeatList == null && rtBeatList == null) {
				tempMove = new Move(fID,fID, new List<int>());
				tempList.Add (tempMove);
				return tempList;
			} else {
				return tempList;
			}
		}

		public List<Move> getAllMoves(int p_playerID) {
			List<Move> foundMoves, foundBeats;
			BoardField tempField;
			int i, j, maxBeats;
			bool badMove;

			foundMoves = new List<Move>();
			for (i=0; i<32; i++) {
				if(fieldList[i].getPlayerID() == p_playerID) {
					foundBeats = lookForBeat(i,p_playerID, new List<int>(), fieldList [i].isQueen ());
					do {
						badMove = false;
						for(j=0;j<foundBeats.Count;j++) {	// usuwam ruchy ktorych pole poczatkowe jest takie samo jak pole koncowe lub króre dwa razy biją ten sam pionek
							if(foundBeats[j].startField == foundBeats[j].endField || !foundBeats[j].uniqueBeats()) {
								badMove = true;
								break;
							}
						}
						if(badMove) foundBeats.RemoveAt(j);
					}
					while (badMove);
					maxBeats = 0;
					for(j=0;j<foundBeats.Count;j++) {	// wyznaczam maksymalna liczbe bic
						if(foundBeats[j].delField.Count > maxBeats) maxBeats = foundBeats[j].delField.Count;
					}
					do {	// tylko ruchy o maksymalnej ilosci bic przechodza dalej
						badMove = false;
						for(j=0;j<foundBeats.Count;j++) {
							if(foundBeats[j].delField.Count != maxBeats) {
								badMove = true;
								break;
							}
						}
						if(badMove) foundBeats.RemoveAt(j);
					}
					while(badMove);
					foundMoves.AddRange(foundBeats);
					if(fieldList[i].isQueen()) {
						tempField = fieldList[i].leftBottom;
						while(tempField != null && !tempField.isOccupied()) {
							foundMoves.Add(new Move(i,tempField.getID(), new List<int>()));
							tempField = tempField.leftBottom;
						}
						tempField = fieldList[i].leftTop;
						while(tempField != null && !tempField.isOccupied()) {
							foundMoves.Add(new Move(i,tempField.getID(), new List<int>()));
							tempField = tempField.leftTop;
						}
						tempField = fieldList[i].rightBottom;
						while(tempField != null && !tempField.isOccupied()) {
							foundMoves.Add(new Move(i,tempField.getID(), new List<int>()));
							tempField = tempField.rightBottom;
						}
						tempField = fieldList[i].rightTop;
						while(tempField != null && !tempField.isOccupied()) {
							foundMoves.Add(new Move(i,tempField.getID(), new List<int>()));
							tempField = tempField.rightTop;
						}
					} else {
						if(p_playerID == 1 && fieldList[i].leftBottom != null && !fieldList[i].leftBottom.isOccupied()) foundMoves.Add(new Move(i,fieldList[i].leftBottom.getID(), new List<int>()));
						if(p_playerID == 1 && fieldList[i].rightBottom != null && !fieldList[i].rightBottom.isOccupied()) foundMoves.Add(new Move(i,fieldList[i].rightBottom.getID(), new List<int>()));
						if(p_playerID == 2 && fieldList[i].leftTop != null && !fieldList[i].leftTop.isOccupied()) foundMoves.Add(new Move(i,fieldList[i].leftTop.getID(), new List<int>()));
						if(p_playerID == 2 && fieldList[i].rightTop != null && !fieldList[i].rightTop.isOccupied()) foundMoves.Add(new Move(i,fieldList[i].rightTop.getID(), new List<int>()));
					}
				}
			}
			maxBeats = 0;
			for(j=0;j<foundMoves.Count;j++) {	// wyznaczam maksymalna liczbe bic
				if(foundMoves[j].delField.Count > maxBeats) maxBeats = foundMoves[j].delField.Count;
			}
			do {	// tylko ruchy o maksymalnej ilosci bic przechodza dalej
				badMove = false;
				for(j=0;j<foundMoves.Count;j++) {
					if(foundMoves[j].delField.Count != maxBeats) {
						badMove = true;
						break;
					}
				}
				if(badMove) foundMoves.RemoveAt(j);
			}
			while(badMove);
			return foundMoves;
		}

		public int evaluateBoard(int playerID) {	// funkcja oceniajaca
			int i, score=0;

			for (i=0; i<32; i++) {
				if(fieldList[i].isOccupied() && fieldList[i].getPlayerID() == playerID) {
					if(fieldList [i].isQueen ()) {
						score = score + 10;
					} else {
						score = score + 2;
					}
					if(i == 9 || i == 10 || i == 13 || i == 14 || i == 17 || i == 18 || i == 21 || i == 22) score = score + 1; 
				}
				if(fieldList[i].isOccupied() && fieldList[i].getPlayerID() != playerID) {
					if(fieldList [i].isQueen ()) {
						score = score - 10;
					} else {
						score = score - 2;
					}
					if(i == 9 || i == 10 || i == 13 || i == 14 || i == 17 || i == 18 || i == 21 || i == 22) score = score - 1;
				}
			}
			return score;
		}
	} // end of class GameBoard

	private GameBoard actualBoard;

	// Use this for initialization
	void Start () {
		actualBoard = new GameBoard(this);
		moveEnded = false;
        endText.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool checkAndMakeMove(Vector3 start, Vector3 destination, int p_playerID) {
		int startFieldID, destinationFieldID, i;
		bool moveOK, hasBeat;
		Move tempMove;
		List<Move> tempMoves;

		if ((start.x - 0.5f) % 2 == 0) {	// mapuje położenie początkowe na indeks pola w liście pól
			startFieldID = (int) ((start.x - 0.5f) * 4 + (start.z - 1.5f) / 2);
		} else {
			startFieldID = (int) ((start.x - 0.5f) * 4 + (start.z - 0.5f) / 2);
		}
		if ((destination.x - 0.5f) % 2 == 0) {	// mapuje położenie końcowe na indeks pola w liście pól
			destinationFieldID = (int) ((destination.x - 0.5f) * 4 + (destination.z - 1.5f) / 2);
		} else {
			destinationFieldID = (int) ((destination.x - 0.5f) * 4 + (destination.z - 0.5f) / 2);
		}
		tempMove = new Move (startFieldID, destinationFieldID, new List<int>());
		moveOK = actualBoard.isMoveOK (tempMove, p_playerID);
		if (moveOK) {
			tempMove = actualBoard.addBeats (tempMove);
			tempMoves = actualBoard.getAllMoves(2);
			hasBeat = false;
			for(i=0;i<tempMoves.Count;i++) {
				if(tempMoves[i].delField.Count > 0) {
					hasBeat = true;
					break;
				}
			}
			if(hasBeat && tempMove.delField.Count == 0) return false;
			actualBoard.makeMove (tempMove);
		}
		return moveOK;
	}

	private int minmax(GameBoard tempBoard, float limit, int pID, int alfa, int beta) {
		int otherID, score, maxscore, i;
		GameBoard newBoard;
		List<Move> moves;

		if (pID == 1) otherID = 2;
		else otherID = 1;
		if (tempBoard.getAllMoves (otherID).Count == 0) {
			return -100;
		}
		if (limit <= 1.0f) return tempBoard.evaluateBoard (otherID);
		moves = tempBoard.getAllMoves (otherID);
		maxscore = -1000;
		for (i=0; i<moves.Count; i++) {
			newBoard = new GameBoard(tempBoard);
			newBoard.makeMove(moves[i]);
			score = -minmax (newBoard, limit/moves.Count, otherID, -beta, -alfa);
			if(score > maxscore) maxscore = score;
			if(maxscore >= beta) break;
			if(maxscore > alfa) alfa = score;
		}
		return maxscore;
	}

	public void computeMove() {
		List<Move> moves;
		int i, chosenOne, score, maxscore;
		GameBoard newBoard;

		moves = actualBoard.getAllMoves (1);
		chosenOne = -1;
		maxscore = -1000;
		for (i=0; i<moves.Count; i++) {
			newBoard = new GameBoard(actualBoard);
			newBoard.makeMove(moves[i]);
			score = -minmax (newBoard, 30.0f, 1, -1000, 1000);
			if(score > maxscore) {
				maxscore = score;
				chosenOne = i;
			}
		}
		if (chosenOne >= 0) {
			actualBoard.visualiseMove (moves [chosenOne]);
		}
		Debug.Log ("Ruch komputera");
	}
}

