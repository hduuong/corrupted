﻿using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	public int rows;               //the number of rows for TILES
	public int cols;               //the number of cols for TILES
	public int offsetSpace;        //the space between the tiles and the player
	public int shiftSpace;         //the space that use to shift from the origin
	public int totalCols;          //the total collumns of the array
	public int visibleCols;        //the number of visible collumn in game
	public GameObject player;      //the player
	public GameObject [][] pieces; //the backend Array that store all game objects
	public GameObject [][] corruptedArray;//the backend Array that keeps track of corrupted area
	public GameObject [] burstVirusArray;//the backend Array that keeps track of burst Viruses
	public GameObject [] fuseVirusArrayUp;//the backend Array that keeps track of fuse Viruses from top
	public GameObject [] fuseVirusArrayDown;//the backend Array that keeps track of fuse Viruses from bottom
	public Object [] cubes;        //the array that stores assets for cube instantiation
	public Object[] burstViruses;  //the array that stores assets for burstVirus instantiation
	public Object[] fuseVirusesUp;   //the array that stores assets for fuseVirus instantiation
	public Object[] fuseVirusesDown; //the array that stores assets for fuseVirus instantiation
	public Object [] lasers;       //the array that keeps track of the laser indicator
	public GameObject firewall;    //the firewall appears at index j = 2;
	public AudioSource clear;      //the clear sound

	public int nameCounter;        //this is a name counter for all the tile in the game, their name going to be used when adding to the delete List
	public int shootCount;         //this is a counter to keep track of the number of shoot count
	public int lastshootCount;     //this is a counter to keep track of the last number of shoot count
	public int corruptionCount;    //this is a counter to keep track of the number of corruption causes by viruses
	public int redOut;             //this is the number of red tile in game
	public int blueOut;            //this is the number of blue tile in game
	public int yellowOut;          //this is the number of yellow tile in game
	public int clearCount;         //this is the number of time player clears the Tiles
	public bool firewallisOn;     //this bool flag is used to notify whether to or not to check for its visibility
	public bool findTileCountOnce;//this bool flag is used to find all the blue-red-yellow counts when first enter the game
	public bool findNeiborsOnce;  //this bool flag is used to link up the neibors for every tile in the beginning once
	public int laserFirstUpdate;  //this acts as a flag - but delays for 5 frame - because it has to wait for the projectile to finish initializing
	public int laserFireUpdate;   //this acts as a flag - but delays for 5 frame - because it has to wait for the projectile to finish initializing

	public bool burstVirusOn;     //bool flag for when burst Virus is On
	public int burstVirusCount;   //counter for how long burst virus will stay, also used as index

	public bool fuseVirusOn;     //bool flag for when burst Virus is On
	public int fuseVirusCount;   //counter for how long fuse virus will stay

	public bool chainVirusOn;    //bool flag for when burst Virus is On
	public GameObject chainVirusHead; //a pointer to the head of the chain virus
	public int chainVirusIndex;       //an index to add in the chain virus game object into the array
	public GameObject[] chainVirusArray;//the array that keeps track of all chain viruses

	public bool healOn;           //bool flag for when healing is in effect
	public GameObject healedTile; //a pointer to where the last healed game object
	public GameObject healingAnim;//a pointer to the healing object with animation

	public GameObject repairMeter;
	// Use this for initialization
	void Start () {
		firewallisOn = false;
		burstVirusOn = false;
		fuseVirusOn = false;
		chainVirusOn = false;
		healOn = false;
		visibleCols = offsetSpace + 6;     //8 is the number of visible cols at game start
		burstVirusCount = 5;
		fuseVirusCount = 5;
		chainVirusIndex = 0;
		chainVirusArray = new GameObject[100];
		redOut = 0;
		blueOut = 0;
		yellowOut = 0;
		clearCount = 0;
		findNeiborsOnce = false;
		findTileCountOnce = false;
		laserFirstUpdate = 5;
		laserFireUpdate = 5;
		totalCols = cols + offsetSpace;
		nameCounter = 0;
		corruptionCount = 0;
		shootCount = 0;
		lastshootCount = 0;
		
		clear = gameObject.AddComponent<AudioSource> ();
		clear.clip = Resources.Load ("Sounds/clear") as AudioClip;

		burstViruses = Resources.LoadAll ("Prefabs/BurstVirus", typeof(GameObject));
		fuseVirusesUp = Resources.LoadAll ("Prefabs/FuseVirusUp", typeof(GameObject));
		fuseVirusesDown = Resources.LoadAll ("Prefabs/FuseVirusDown", typeof(GameObject));
		
		lasers = new GameObject[totalCols];

		firewall = GameObject.Find ("firewall");
		firewall.GetComponent<SpriteRenderer>().enabled = false;

		repairMeter = GameObject.Find ("RepairMeter");

		healingAnim =  (GameObject)Instantiate(Resources.Load("Prefabs/Heal", typeof(GameObject)),new Vector2(0,0 - shiftSpace + rows/2), Quaternion.identity);
		healingAnim.GetComponent<SpriteRenderer> ().enabled = false;

		//------------------------------------Instantiating the corrupted Array----------------------------------------------
		corruptedArray = new GameObject[totalCols][];
		for (int i = 0; i < totalCols; i++) {
			corruptedArray[i] = new GameObject[rows];
		}
		//------------------------------------Instantiating the burst Virus Array----------------------------------------------
		burstVirusArray = new GameObject[4];
		//------------------------------------Instantiating the fuse Virus Array----------------------------------------------
		fuseVirusArrayUp = new GameObject[5];
		fuseVirusArrayDown = new GameObject[5];
		//------------------------------------Instantiating the Titles----------------------------------------------
		pieces = new GameObject[totalCols][];
		cubes = Resources.LoadAll ("Prefabs/Tile", typeof(GameObject));
		for (int i = offsetSpace; i < totalCols; i++) {
			pieces[i] = new GameObject[rows];
			for (int j = 0; j < rows; j++) {
				pieces[i][j] = (GameObject)Instantiate (cubes[Random.Range(0,3)], new Vector2(i,j - shiftSpace), Quaternion.identity);
				pieces[i][j].name = (nameCounter++).ToString();
			}
		}
		for (int i = 0; i < offsetSpace; i++) {
			pieces[i] = new GameObject[rows];
		}
		//------------------------------------Instantiating the Player----------------------------------------------	
		player = (GameObject)Instantiate(Resources.Load("Prefabs/Cannon", typeof(GameObject)),new Vector2(0,0 - shiftSpace + rows/2), Quaternion.identity);
		pieces[0][0] = player;
	}

	/**
	 * add a Tile to the array
	 * the Tile is initially created and hold by the player
	 * 
	 * @return true if successfully add - false otherwise
	 * @author Duong H Chau
	 */ 
	public bool addTile(GameObject tile){
		shootCount++;
		destroyLaser ();
		updateLaser ();
		//find the index to add the title
		int index = 0;
		bool found = false;
		//check the next index for availability
		for (int i = 0; i < totalCols - 1 && !found; i++) {
			if(pieces[i+1][(int)player.transform.position.y + shiftSpace] != null){
				found = true;
			}else{
				index++;
			}
		}
		//check for index validation
		if (index > 0 && index < totalCols) {
			pieces [index][(int)player.transform.position.y + shiftSpace]  = tile; //add the tile to the array
			tile.transform.parent = null;                                          //unparent the tile from the cannon/player
			tile.transform.position = new Vector2 (index,(int)player.transform.position.y); //set the position
			//this landed into a firewall
			if(index == 2){
				findNeibors(index,(int)player.transform.position.y + shiftSpace);
				clear.Play();
				if(tile.GetComponent<Tile>().neiborsCount == 1){
					if(tile.GetComponent<Tile>().neibors[0].GetComponent<Tile>().neiborsCount >= 1)
						tile.GetComponent<Tile>().setDelete();
					else{
						tile.GetComponent<Tile>().disableNeibor();
						tile.GetComponent<Tile>().setDelete();
					}
				}else if(tile.GetComponent<Tile>().neiborsCount > 1){
					tile.GetComponent<Tile>().setDelete();
				}else{
					tile.GetComponent<Tile>().disableNeibor();
					tile.GetComponent<Tile>().setDelete();
				}
			}else{
				findNeibors(index,(int)player.transform.position.y + shiftSpace);     //establish a map for this tile at this index
				checkForDelete(index,(int)player.transform.position.y + shiftSpace);   //check for posible clear after adding
			}
			//increment the tile count
			if(tile.GetComponent<Tile>().name == "red"){
				redOut++;
			}else if (tile.GetComponent<Tile>().name == "blue"){
				blueOut++;
			}else if (tile.GetComponent<Tile>().name == "yellow"){
				yellowOut++;
			}else{}
			return true;
		} else {
			return false;
		}
	}
	/**
	 * insert a game object to the pieces[][]
	 * @param go, the game object that will be added
	 * @param i, index i
	 * @param j, index j
	 * @author Duong H Chau
	 */ 
	public void insertTile(GameObject go, int i, int j){
		pieces [i] [j] = go;
		for (int ii = 2; ii < totalCols; ii++) {
			for (int jj = 0; jj < rows; jj++) {
				if(pieces[ii][jj] != null){
					pieces[ii][jj].GetComponent<Tile>().disableNeibor();
				}
			}
		}
		findNeiborsOnce = false;
	}

	/**
	 * This checks for potential destroyable gameObjects
	 * Link up the neibors array in each gameobject once using a boolean flag
	 */
	void Update () {
		//Initializing the map for all tile first created at game start
		if (!findNeiborsOnce) {
			for (int i = 0; i < totalCols; i++) {
				for (int j = 0; j < rows; j++) {
					findNeibors (i, j);
				}
			}
			findNeiborsOnce = true;
		}
		//Initializing the tile count for all tile created at game start
		if (!findTileCountOnce) {
			redOut = 0;
			blueOut = 0;
			yellowOut = 0;
			for (int i = 2; i < totalCols; i++) {
				for (int j = 0; j < rows; j++) {
					findColorCount (i, j);
				}
			}
			findTileCountOnce = true;
		}
		//Initializing the laser color at game start
		if (laserFirstUpdate > 0) {
			updateLaser ();
			laserFirstUpdate--;
		}

		//Update the laser color immediately after shooting
		if (player.GetComponent<Cannon> ().shot && laserFireUpdate > 0) {
			updateLaser ();
			laserFireUpdate--;
		} else if (!player.GetComponent<Cannon> ().shot && laserFireUpdate <= 0) {
			laserFireUpdate = 5;
		}



		//when burstVirus is in the game
		if (burstVirusOn) {
			if (shootCount - lastshootCount > 0) {
				burstVirusCount--;
				if (burstVirusCount == 0) {
					burstVirusOn = false;
					burstVirusCount = 5;
					burstVirusExplodes ();
				} else {
					int index = 0;
					int i = (int)burstVirusArray [0].transform.position.x;
					int j = (int)burstVirusArray [0].transform.position.y + shiftSpace;
					if (pieces [i] [j] == null) {
						burstVirusCleared ();
					} else {
						string color = pieces [i] [j].GetComponent<Tile> ().name;
						addBurstVirusHelper (color, i, j, index++);

						i = (int)burstVirusArray [1].transform.position.x;
						j = (int)burstVirusArray [1].transform.position.y + shiftSpace;
						addBurstVirusHelper (color, i, j, index++);

						i = (int)burstVirusArray [2].transform.position.x;
						j = (int)burstVirusArray [2].transform.position.y + shiftSpace;
						addBurstVirusHelper (color, i, j, index++);
						
						i = (int)burstVirusArray [3].transform.position.x;
						j = (int)burstVirusArray [3].transform.position.y + shiftSpace;
						addBurstVirusHelper (color, i, j, index++);
					}
				}
			}
		}
		//when fuseVirus is in the game
		if (fuseVirusOn) {
			if (shootCount - lastshootCount > 0) {
				fuseVirusCount--;
				if (fuseVirusCount == 0) {
					fuseVirusOn = false;
					fuseVirusCount = 5;
					fuseVirusExplodes ();
				} else {
					int index = 4 - fuseVirusCount;
					int i = (int)fuseVirusArrayUp [index].transform.position.x;
					int j = (int)fuseVirusArrayUp [index].transform.position.y + shiftSpace;
					int ii = (int)fuseVirusArrayDown [index].transform.position.x;
					int jj = (int)fuseVirusArrayDown [index].transform.position.y + shiftSpace;
					if (pieces [i] [0] == null || pieces [ii] [rows - 1] == null) {
						fuseVirusCleared ();
					} else {
						string color = pieces [i] [j].GetComponent<Tile> ().name;
						addfuseVirusHelper (color, i, j, index, 0);
						addfuseVirusHelper (color, ii, jj, index, 1);
					}
				}
			}
		}
		//when chainVirus is in the game
		if (chainVirusOn) {
			if (shootCount - lastshootCount > 0) {
				int i = (int)chainVirusHead.transform.position.x;
				int j = (int)chainVirusHead.transform.position.y + shiftSpace;
				if(pieces[i][j] == null){
					chainVirusCleared();
					chainVirusOn = false;
				}else{
					string color = pieces[i][j].GetComponent<Tile>().name;
					int neiborCount = pieces[i][j].GetComponent<Tile>().neiborsCount;
					// mark corruption for the previous infected tiles
					for(int idx = 0; idx < chainVirusIndex; idx++){
						int ii = (int)chainVirusArray[idx].transform.position.x;
						int jj = (int)chainVirusArray[idx].transform.position.y + shiftSpace;
						if(corruptedArray[ii][jj] == null){
							addCorruption(ii,jj);
						}
					}
					if(neiborCount == 0){
						if(pieces[i-1][j] != null){
							pieces[i-1][j].GetComponent<Tile>().disableNeibor();
							pieces[i-1][j].GetComponent<SpriteRenderer>().enabled = false;
							pieces[i-1][j].GetComponent<Tile>().setDelete();
						}
						if (color == "red") {
							pieces[i-1][j] = (GameObject)Instantiate(cubes[1],new Vector3(i-1,j - shiftSpace, 0 ), Quaternion.identity);
							pieces[i-1][j].name = (nameCounter++).ToString();
							redOut++;
						} else if (color == "blue") {
							pieces[i-1][j] = (GameObject)Instantiate(cubes[0],new Vector3(i-1,j - shiftSpace, 0), Quaternion.identity);
							pieces[i-1][j].name = (nameCounter++).ToString();
							blueOut++;
						} else if (color == "yellow") {
							pieces[i-1][j] = (GameObject)Instantiate(cubes[2],new Vector3(i-1,j - shiftSpace, 0 ), Quaternion.identity);
							pieces[i-1][j].name = (nameCounter++).ToString();
							yellowOut++;
						} else { //additional color goes here

						}
						chainVirusHead = (GameObject)Instantiate(Resources.Load("Prefabs/ChainVirus/ChainVirus", typeof(GameObject)),new Vector3(i-1, j - shiftSpace, -2), Quaternion.identity);
						chainVirusArray[chainVirusIndex++] = chainVirusHead;

						//HERE UPDATE THE NEIBORING LINKS
						for (int ii = 2; ii < totalCols; ii++) {
							for (int jj = 0; jj < rows; jj++) {
								if(pieces[ii][jj] != null){
									pieces[ii][jj].GetComponent<Tile>().disableNeibor();
								}
							}
						}
						findNeiborsOnce = false;
					}else{
						GameObject[] neibors = pieces[i][j].GetComponent<Tile>().neibors;
						int m = (int)neibors[0].transform.position.x;
						int n = (int)neibors[0].transform.position.y + shiftSpace;
						if(corruptedArray[m][n] != null){
							if(pieces[i-1][j] != null){
								pieces[i-1][j].GetComponent<Tile>().disableNeibor();
								pieces[i-1][j].GetComponent<SpriteRenderer>().enabled = false;
								pieces[i-1][j].GetComponent<Tile>().setDelete();
							}
							if (color == "red") {
								pieces[i-1][j] = (GameObject)Instantiate(cubes[1],new Vector3(i-1,j - shiftSpace, 0 ), Quaternion.identity);
								pieces[i-1][j].name = (nameCounter++).ToString();
								redOut++;
							} else if (color == "blue") {
								pieces[i-1][j] = (GameObject)Instantiate(cubes[0],new Vector3(i-1,j - shiftSpace, 0), Quaternion.identity);
								pieces[i-1][j].name = (nameCounter++).ToString();
								blueOut++;
							} else if (color == "yellow") {
								pieces[i-1][j] = (GameObject)Instantiate(cubes[2],new Vector3(i-1,j - shiftSpace, 0 ), Quaternion.identity);
								pieces[i-1][j].name = (nameCounter++).ToString();
								yellowOut++;
							} else { //additional color goes here
								
							}
							chainVirusHead = (GameObject)Instantiate(Resources.Load("Prefabs/ChainVirus/ChainVirus", typeof(GameObject)),new Vector3(i-1, j - shiftSpace, -2), Quaternion.identity);
							chainVirusArray[chainVirusIndex++] = chainVirusHead;
						}else{
							GameObject[] list = new GameObject[256 * 5];
							chainVirusFinder(i,j,ref list);
							addChainVirusHelper(list);
							//find the head of the chainArray
							int min = 999;
							for(int idx = 0; idx < chainVirusIndex; idx++){
								int ii = (int)chainVirusArray[idx].transform.position.x;
								if(ii < min){
									min = ii;
									chainVirusHead = chainVirusArray[idx];
								}
							}
						}
					}
				}
			}
		}

		//Healing is in active
		if (healOn) {
			if(shootCount - lastshootCount > 0){
				int i = (int)healingAnim.transform.position.x;
				int j = (int)healingAnim.transform.position.y + shiftSpace;
				Vector2 [] posiblePath = new Vector2[8];
				posiblePath[0] = new Vector2(i-1,j-1);
				posiblePath[1] = new Vector2(i-1,j);
				posiblePath[2] = new Vector2(i-1,j+1);
				posiblePath[3] = new Vector2(i,j-1);
				posiblePath[4] = new Vector2(i,j+1);
				posiblePath[5] = new Vector2(i+1,j-1);
				posiblePath[6] = new Vector2(i+1,j);
				posiblePath[7] = new Vector2(i+1,j+1);

				//checks if all posible path is invalid
				if(   ((int)posiblePath[0].x < 0 || (int)posiblePath[0].x > totalCols-1 || (int)posiblePath[0].y < 0 || (int)posiblePath[0].y > rows-1 || corruptedArray[(int)posiblePath[0].x][(int)posiblePath[0].y] == null) 
				  	&& ((int)posiblePath[1].x < 0 || (int)posiblePath[1].x > totalCols-1 || (int)posiblePath[1].y < 0 || (int)posiblePath[1].y > rows-1 || corruptedArray[(int)posiblePath[1].x][(int)posiblePath[1].y] == null)                        
				  	&& ((int)posiblePath[2].x < 0 || (int)posiblePath[2].x > totalCols-1 || (int)posiblePath[2].y < 0 || (int)posiblePath[2].y > rows-1 || corruptedArray[(int)posiblePath[2].x][(int)posiblePath[2].y] == null)
				   	&& ((int)posiblePath[3].x < 0 || (int)posiblePath[3].x > totalCols-1 || (int)posiblePath[3].y < 0 || (int)posiblePath[3].y > rows-1 || corruptedArray[(int)posiblePath[3].x][(int)posiblePath[3].y] == null)
				   	&& ((int)posiblePath[4].x < 0 || (int)posiblePath[4].x > totalCols-1 || (int)posiblePath[4].y < 0 || (int)posiblePath[4].y > rows-1 || corruptedArray[(int)posiblePath[4].x][(int)posiblePath[4].y] == null)
				   	&& ((int)posiblePath[5].x < 0 || (int)posiblePath[5].x > totalCols-1 || (int)posiblePath[5].y < 0 || (int)posiblePath[5].y > rows-1 || corruptedArray[(int)posiblePath[5].x][(int)posiblePath[5].y] == null)
				   	&& ((int)posiblePath[6].x < 0 || (int)posiblePath[6].x > totalCols-1 || (int)posiblePath[6].y < 0 || (int)posiblePath[6].y > rows-1 || corruptedArray[(int)posiblePath[6].x][(int)posiblePath[6].y] == null)
				   	&& ((int)posiblePath[7].x < 0 || (int)posiblePath[7].x > totalCols-1 || (int)posiblePath[7].y < 0 || (int)posiblePath[7].y > rows-1 || corruptedArray[(int)posiblePath[7].x][(int)posiblePath[7].y] == null)){
					healOn = false;
					healingAnim.GetComponent<SpriteRenderer>().enabled = false;
				}else{
					bool found = false;
					Vector2 destination = posiblePath[0];
					while(!found){
						destination = posiblePath[Random.Range(0,posiblePath.Length)];
						if ((int)destination.x < 0 || (int)destination.x > totalCols-1) 
							continue;
						if ((int)destination.y < 0 || (int)destination.y > rows-1)
							continue;
						if(corruptedArray[(int)destination.x][(int)destination.y] != null)
							found = true;
					}
					healHelper((int)destination.x,(int)destination.y);
					deleteCorruption ((int)destination.x,(int)destination.y);
					findTileCountOnce = false;
				}	
				if(corruptionCount == 0){
					healOn = false;
				}
			}
		}
		//Update the fireWall visibility 
		if (!firewallisOn) {
			bool found = false;
			for (int i = 0; i < rows && !found; i++) {
				if (pieces [3] [i] != null)
					found = true;
			}
			if (found)
				firewallOn ();
		} 
		else {
			bool found = false;
			for (int i = 0; i < rows && !found; i++) {
				if (pieces [3] [i] != null)
					found = true;
			}
			if (!found)
				firewallOff ();
		}

		//pressed this key to add BurstVirus --- for testing only
		if(Input.GetKeyUp(KeyCode.B)){
			addBurstVirus();
			burstVirusOn = true;
		}

		//pressed this key to add BurstVirus --- for testing only
		if(Input.GetKeyUp(KeyCode.F)){
			addFuseVirus();
			fuseVirusOn = true;
		}

		//pressed this key to add BurstVirus --- for testing only
		if(Input.GetKeyUp(KeyCode.C)){
			addChainVirus();
			chainVirusOn = true;
		}

		//pressed Mouse right to Heal if meter is full.
		if(Input.GetMouseButtonUp(1)){
			if(repairMeter.GetComponent<RepairMeter>().full){
				heal();
			}
		}


		if(Input.GetKeyUp(KeyCode.Space)){
			pushArrayDown();
		}
		//winning condition
		if (blueOut == 0 && redOut == 0 && yellowOut == 0) {
			Application.LoadLevel("win");
		}
		//loosing condition
		if (GameObject.Find ("CorruptionMarker").GetComponent<CorruptionMarker> ().lose) {
			Application.LoadLevel("lose");
		}

		lastshootCount = shootCount;
	}

	//set the firewall to be visible
	public void firewallOn(){
		firewall.GetComponent<SpriteRenderer>().enabled = true;
		firewallisOn = true;
	}
	//set the firewall to be invisible
	public void firewallOff(){
		firewall.GetComponent<SpriteRenderer>().enabled = false;
		firewallisOn = false;
	}
	/**
	 * find the count for each color tile
	 * @param i, index i
	 * @param j, index j
	 */ 
	public void findColorCount(int i, int j){
		if (pieces [i] [j] != null) {
			if (pieces [i] [j].GetComponent<Tile> ().name == "red") {
				redOut++;
			} else if (pieces [i] [j].GetComponent<Tile> ().name == "blue") {
				blueOut++;
			} else if (pieces [i] [j].GetComponent<Tile> ().name == "yellow") {
				yellowOut++;
			} else {
			}
		}
	}
	//-----------------------------------------------------CODE FOR LINKING UP NEIBORS----------------------------------
	/**
	 * Find the surrounding neibors of the game object that located at the given indexes
	 * @param i, index i
	 * @param j, index j
	 */ 
	public void findNeibors(int i, int j){
		if (pieces [i] [j] == null)
			return;
		findNeiborsHelper (pieces [i] [j], i, j+1);
		findNeiborsHelper (pieces [i] [j], i, j-1);
		findNeiborsHelper (pieces [i] [j], i-1, j+1);
		findNeiborsHelper (pieces [i] [j], i-1, j);
		findNeiborsHelper (pieces [i] [j], i-1, j-1);
		findNeiborsHelper (pieces [i] [j], i+1, j+1);
		findNeiborsHelper (pieces [i] [j], i+1, j);
		findNeiborsHelper (pieces [i] [j], i+1, j-1);
	}
	/**
	 * private helper method for findNeibor - takes a gameobject and add in the gameObject @ the indexes if it matches
	 * @param go, the game object that will be modified
	 * @param i, index i of the game object that will be compared
	 * @param j, index j of the game object that will be compared
	 */ 
	private void findNeiborsHelper(GameObject go, int i, int j){
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;
		if (pieces [i] [j] == null)
			return;
		if (go.GetComponent<Tile> () != null) {
			go.GetComponent<Tile> ().addNeibor (ref pieces [i] [j]);
		}
	}

	public void chainVirusFinder(int i, int j, ref GameObject[] list){
		if (i < 0 || i > totalCols - 1) 
			return;
		if (j < 0 || j > rows - 1)
			return;

		string name = pieces [i] [j].GetComponent<Tile> ().name;
		int count = 0;
		//call helper method to find and add more gameObject to the list
		deleteHelper (ref list, name, i, j, ref count);
	}

	//------------------------------------------------------CODE FOR DELETING/CLEARING THE TILE----------------------------------
	/**
	 * go through the entire list of gameObject and delete them if they are linked up
	 * @param i, index i
	 * @paran j, index j
	 */ 
	public void checkForDelete(int i, int j){
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;

		GameObject[] list = new GameObject[256*5];
		string name = pieces [i] [j].GetComponent<Tile> ().name;
		int count = 0;
		//call helper method to find and add more gameObject to the list
		deleteHelper (ref list, name, i, j, ref count);

		//has three or more objects -> delete
		if (count >= 3) {
			clear.Play();
			clearCount++;
			repairMeter.GetComponent<RepairMeter>().index++;

			for(int idx = 0; idx < 256*5; idx++){
				if(list[idx] != null)
					list[idx].GetComponent<Tile>().setDelete();
			}
		}
	}

	/**
	 * helper method for finding a list of deleteable game object
	 * @param list, reference to a list that will store all delete-able game object
	 * @param name, the string that indicate the color of that game object
	 * @param i, index i of the gameobject that will be compared
	 * @param j, index j of the gameobject that will be compared
	 * @param count, reference to a counter that keeps track of how many items in the list
	 */ 
	public void deleteHelper(ref GameObject[] list, string name, int i, int j, ref int count){
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;
		if (pieces [i] [j] == null)
			return;
		if (pieces [i] [j].GetComponent<Tile> ().name != name)
			return;
		addToList (ref list, pieces [i] [j], ref count);

		checkLeft (ref list, name, i - 1, j + 1, ref count);
		checkLeft (ref list, name, i-1, j, ref count);
		checkLeft (ref list, name, i-1, j-1, ref count);
		checkAbove (ref list, name, i, j+1, ref count);
		checkAbove (ref list, name, i-1, j+1, ref count);
		checkAbove (ref list, name, i+1, j+1, ref count);
		checkBelow (ref list, name, i, j-1, ref count);
		checkBelow (ref list, name, i-1, j-1, ref count);
		checkBelow (ref list, name, i+1, j-1, ref count);
		checkRight (ref list, name, i+1, j+1, ref count);
		checkRight (ref list, name, i+1, j, ref count);
		checkRight (ref list, name, i+1, j-1, ref count);
	}
	/**
	 * recursive method that checks all game objects on the left of the original GO
	 * @param list, reference to a list that will store all delete-able game object
	 * @param name, the string that indicate the color of that game object
	 * @param i, index i of the gameobject that will be compared
	 * @param j, index j of the gameobject that will be compared
	 * @param count, reference to a counter that keeps track of how many items in the list
	 */ 
	public void checkLeft(ref GameObject[] list, string name, int i, int j, ref int count){
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;
		if (pieces [i] [j] == null)
			return;
		if (pieces [i] [j].GetComponent<Tile> ().name != name)
			return;
		addToList (ref list, pieces [i] [j], ref count);
		checkLeft (ref list, name, i-1, j+1, ref count);
		checkLeft (ref list, name, i-1, j, ref count);
		checkLeft (ref list, name, i-1, j-1, ref count);
	}
	/**
	 * recursive method that checks all game objects on the right of the original GO
	 * @param list, reference to a list that will store all delete-able game object
	 * @param name, the string that indicate the color of that game object
	 * @param i, index i of the gameobject that will be compared
	 * @param j, index j of the gameobject that will be compared
	 * @param count, reference to a counter that keeps track of how many items in the list
	 */ 
	public void checkRight(ref GameObject[] list, string name, int i, int j, ref int count){
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;
		if (pieces [i] [j] == null)
			return;
		if (pieces [i] [j].GetComponent<Tile> ().name != name)
			return;
		addToList (ref list, pieces [i] [j], ref count);
		checkRight (ref list, name, i+1, j+1, ref count);
		checkRight (ref list, name, i+1, j, ref count);
		checkRight (ref list, name, i+1, j-1, ref count);
	}
	/**
	 * recursive method that checks all game objects above the original GO
	 * @param list, reference to a list that will store all delete-able game object
	 * @param name, the string that indicate the color of that game object
	 * @param i, index i of the gameobject that will be compared
	 * @param j, index j of the gameobject that will be compared
	 * @param count, reference to a counter that keeps track of how many items in the list
	 */ 
	public void checkAbove(ref GameObject[] list, string name, int i, int j, ref int count){
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;
		if (pieces [i] [j] == null)
			return;
		if (pieces [i] [j].GetComponent<Tile> ().name != name)
			return;
		addToList (ref list, pieces [i] [j], ref count);
		checkAbove (ref list, name, i, j+1, ref count);
		checkAbove (ref list, name, i-1, j+1, ref count);
		checkAbove (ref list, name, i+1, j+1, ref count);

	}
	/**
	 * recursive method that checks all game objects below the original GO
	 * @param list, reference to a list that will store all delete-able game object
	 * @param name, the string that indicate the color of that game object
	 * @param i, index i of the gameobject that will be compared
	 * @param j, index j of the gameobject that will be compared
	 * @param count, reference to a counter that keeps track of how many items in the list
	 */ 
	public void checkBelow(ref GameObject[] list, string name, int i, int j, ref int count){
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;
		if (pieces [i] [j] == null)
			return;
		if (pieces [i] [j].GetComponent<Tile> ().name != name)
			return;
		addToList (ref list, pieces [i] [j], ref count);
		checkBelow (ref list, name, i, j-1, ref count);
		checkBelow (ref list, name, i-1, j-1, ref count);
		checkBelow (ref list, name, i+1, j-1, ref count);

	}
	/**
	 * helper method to add a gameObject to the list
	 * @param list, reference to a list that will store all delete-able game object
	 * @param go, the game object that will be added to the list
	 * @param count, reference to a counter that keeps track of how many items in the list
	 */
	public void addToList(ref GameObject[] list, GameObject go, ref int count){
		if (list [int.Parse(go.name)] != null) { //already added to the list
			return;
		} else {                                 //new entry, add to the list and increment counter
			list [int.Parse(go.name)] = go;
			count++;
		}
	}

	//-------------------------------------------------CODE FOR THE LASER------------------------------------------------------
	//this method gets called to update the laser indicator everytime a player makes a decision/command
	public void updateLaser(){
		//destroys the previous 
		destroyLaser ();
		//creates the new
		int index_i = (int)(player.transform.position.x);
		int index_j = (int)(player.transform.position.y + shiftSpace);
		for (int i = index_i + 1; i < totalCols; i++) {
			//Debug.Log(i);
			//Debug.Log(index_j);
			if (pieces [i] [index_j] == null) {
				GameObject temp = createLaser (player.GetComponent<Cannon> ().projectTile.GetComponent<Tile> ().name);
				lasers [i - 1] = temp;
				temp.transform.position = new Vector2 (i, (int)player.transform.position.y); //set the position
			} else {
				break;
			}
		}
	}
	//destroys the current laserbeam
	public void destroyLaser(){
		for (int i = 0; i < totalCols; i++) {
			if(lasers[i] != null)
				Destroy(lasers[i]);
		}
	}
	//helper method to create the correct color laser
	private GameObject createLaser(string name){
		GameObject go = (GameObject)Instantiate (Resources.Load ("Prefabs/Laser"));
		if (name == "red") {
			go.GetComponent<SpriteRenderer>().material.color = Color.red;
		} else if (name == "blue") {
			go.GetComponent<SpriteRenderer>().material.color = Color.blue;
		} else if (name == "yellow"){
			go.GetComponent<SpriteRenderer>().material.color = Color.yellow;
		}else{
		}
		return go;
	}

	//--------------------------------------------------------------------------------------------------------------
	//shift all gameobjects in array back one index
	public void pushArrayDown(){
		bool landOnFirewall = false;
		//CHANGE the 3D text
		GameObject text = GameObject.Find ("Row Text");
		text.GetComponent<RowText> ().change ();

		//PUSH the Tile array
		for (int j = 0; j < rows; j++) {
			for(int i = 2; i < totalCols - 1; i++){
				if(pieces[i+1][j] != null && i == 2){
					landOnFirewall = true;
					pieces[i][j] = pieces[i+1][j];
					pieces[i][j].transform.position = new Vector3(i,j - shiftSpace,0);
					pieces[i+1][j] = null;
				}else if (pieces[i+1][j] != null ){
					pieces[i][j] = pieces[i+1][j];
					pieces[i][j].transform.position = new Vector3(i,j - shiftSpace,0);
					pieces[i+1][j] = null;
				}else{
					pieces[i][j] = null;
				}
			}
		}
		if (landOnFirewall)
			deleteAtFireWall ();

		//UPDATE the row meter
		GameObject rowmeter = GameObject.Find ("Row Meter");
		rowmeter.GetComponent<RowMeter> ().pushedCols ++;

		//PUSH the corrupted array also
		for (int j = 0; j < rows; j++) {
			for (int i = 0; i < totalCols - 1; i++) {
				if (corruptedArray[i+1][j] != null ){
					corruptedArray[i][j] = corruptedArray[i+1][j];
					corruptedArray[i][j].transform.position = new Vector3(i,j - shiftSpace,0);
					corruptedArray[i+1][j] = null;
				}else{
					corruptedArray[i][j] = null;
				}
			}
		}
		//PUSH the burst Virus array 
		if (burstVirusOn) {
			for( int i = 0; i < 4; i++){
				int idx = (int)burstVirusArray[i].transform.position.x;
				int jdx = (int)burstVirusArray[i].transform.position.y;
				burstVirusArray[i].transform.position = new Vector3(idx - 1,jdx,-2);
			}
		}
		//PUSH the fuse Virus array
		if (fuseVirusOn) {
			for( int i = 0; i < 5; i++){
				int idx = (int)fuseVirusArrayDown[i].transform.position.x;
				int jdx = (int)fuseVirusArrayDown[i].transform.position.y;
				fuseVirusArrayDown[i].transform.position = new Vector3(idx - 1,jdx,-2);
				idx = (int)fuseVirusArrayUp[i].transform.position.x;
				jdx = (int)fuseVirusArrayUp[i].transform.position.y;
				fuseVirusArrayUp[i].transform.position = new Vector3(idx - 1,jdx,-2);
			}
		}
		//PUSH the chain Virus array
		if (chainVirusOn) {
			for( int i = 0; i < chainVirusIndex; i++){
				int idx = (int)chainVirusArray[i].transform.position.x;
				int jdx = (int)chainVirusArray[i].transform.position.y;
				chainVirusArray[i].transform.position = new Vector3(idx - 1,jdx,-2);
			}
		}
	}
	//delete all tile lands on fire wall
	public void deleteAtFireWall(){
		clear.Play ();
		for(int j = 0; j < rows; j++){
			if(pieces[2][j] != null){
				pieces[2][j].GetComponent<Tile>().disableNeibor();
				pieces[2][j].GetComponent<Tile>().setDelete();
			}
			if(corruptedArray[2][j] != null){
				Destroy(corruptedArray[2][j]);
				corruptedArray[2][j] = null;
			}
		}
	}
	//---------------------------------------------Burst Virus-----------------------------------------------------------
	public void addBurstVirus(){
		bool found = false;
		GameObject go = null;
		int i = 0;
		int j = 0;
		int count = 0; //this is used to add viruses into the burstVirusesArray
		string color;
		//picks a random tile in game
		while (!found) {
			i = Random.Range (offsetSpace, visibleCols);
			j = Random.Range (0, rows);
			if (pieces [i] [j] != null) {
				go = pieces [i] [j];
				found = true;
			}
		}
		//gets the color of that tile
		color = go.GetComponent<Tile> ().name;

		if (burstVirusCornerCheck (i, j)) {  //lower left
			addBurstVirusHelper(color,i,j,count++);
			addBurstVirusHelper(color,i,j-1,count++);
			addBurstVirusHelper(color,i-1,j,count++);
			addBurstVirusHelper(color,i-1,j-1,count++);
		} else if (burstVirusCornerCheck (i, j+1)) { // upper left
			addBurstVirusHelper(color,i,j,count++);
			addBurstVirusHelper(color,i,j+1,count++);
			addBurstVirusHelper(color,i-1,j,count++);
			addBurstVirusHelper(color,i-1,j+1,count++);
		} else if (burstVirusCornerCheck (i+1, j)) { // lower right
			addBurstVirusHelper(color,i,j,count++);
			addBurstVirusHelper(color,i,j-1,count++);
			addBurstVirusHelper(color,i+1,j,count++);
			addBurstVirusHelper(color,i+1,j-1,count++);
		} else if (burstVirusCornerCheck (i+1, j+1)) { // upper right
			addBurstVirusHelper(color,i,j,count++);
			addBurstVirusHelper(color,i,j+1,count++);
			addBurstVirusHelper(color,i+1,j,count++);
			addBurstVirusHelper(color,i+1,j+1,count++);
		} else {
		}

		//HERE UPDATE THE NEIBORING LINKS
		for (int ii = 2; ii < totalCols; ii++) {
			for (int jj = 0; jj < rows; jj++) {
				if(pieces[ii][jj] != null){
						pieces[ii][jj].GetComponent<Tile>().disableNeibor();
				}
			}
		}
		findNeiborsOnce = false;
		//go.GetComponent<MeshRenderer> ().enabled = false;
	}

	//checks the lower left corner starting from the given indexes
	//@param i, i index
	//@param j, j index
	//@return true if all 4 indexes are not null, false otherwise
	private bool burstVirusCornerCheck(int i, int j){
		if (i - 1 < 0 || i > totalCols-1) 
			return false;
		if (j - 1 < 0 || j > rows-1)
			return false;
		if (pieces [i] [j] == null)
			return false;

		if (pieces [i] [j] != null && pieces [i-1] [j] != null && pieces [i-1] [j-1] != null && pieces [i] [j-1] != null)
			return true;
		return false;
	}

	private bool addBurstVirusHelper(string color, int i, int j, int index){
		if (i < 0 || i > totalCols-1) 
			return false;
		if (j < 0 || j > rows - 1)
			return false;
		if (pieces [i] [j] == null)
			return false;

		pieces [i] [j].GetComponent<Tile> ().disableNeibor ();
		pieces [i] [j].GetComponent<Tile> ().setDelete();
		if (color == "blue") {
			pieces[i][j] = (GameObject)Instantiate(cubes[0],new Vector3(i,j - shiftSpace, 0), Quaternion.identity);
			pieces[i][j].name = (nameCounter++).ToString();
			blueOut++;
		} else if (color == "red") {
			pieces[i][j] = (GameObject)Instantiate(cubes[1],new Vector3(i,j - shiftSpace, 0 ), Quaternion.identity);
			pieces[i][j].name = (nameCounter++).ToString();
			redOut++;
		} else if (color == "yellow") {
			pieces[i][j] = (GameObject)Instantiate(cubes[2],new Vector3(i,j - shiftSpace, 0 ), Quaternion.identity);
			pieces[i][j].name = (nameCounter++).ToString();
			yellowOut++;
		} else {
		}
		pieces[i][j].GetComponent<SpriteRenderer>().enabled = false;
		
		if (burstVirusArray[index] != null) {
			Destroy(burstVirusArray[index]);
			burstVirusArray[index] = null;
		}
		//Debug.Log (index);
		burstVirusArray[index]= (GameObject)Instantiate(burstViruses[burstVirusCount],new Vector3(i,j - shiftSpace,-2 ), Quaternion.identity);
		if (color == "red") {
			burstVirusArray [index].GetComponent<SpriteRenderer> ().color = Color.red;
		} else if (color == "blue") {
			burstVirusArray [index].GetComponent<SpriteRenderer> ().color = Color.blue;
		} else if (color == "yellow") {
			burstVirusArray [index].GetComponent<SpriteRenderer> ().color = Color.yellow;
		} else {
		}
		return true;
	}

	public void burstVirusCleared(){
		Destroy (burstVirusArray [0]);
		Destroy (burstVirusArray [1]);
		Destroy (burstVirusArray [2]);
		Destroy (burstVirusArray [3]);
		
		burstVirusArray [0] = null;
		burstVirusArray [1] = null;
		burstVirusArray [2] = null;
		burstVirusArray [3] = null;

		burstVirusOn = false;
		burstVirusCount = 6;
	}
	public void burstVirusExplodes(){
		Vector2 v1 = burstVirusArray [0].transform.position;
		Vector2 v2 = burstVirusArray [1].transform.position;
		Vector2 v3 = burstVirusArray [2].transform.position;
		Vector2 v4 = burstVirusArray [3].transform.position;

		Destroy (burstVirusArray [0]);
		Destroy (burstVirusArray [1]);
		Destroy (burstVirusArray [2]);
		Destroy (burstVirusArray [3]);

		burstVirusArray [0] = null;
		burstVirusArray [1] = null;
		burstVirusArray [2] = null;
		burstVirusArray [3] = null;

		burstVirusExplodesHelper (v1);
		burstVirusExplodesHelper (v2);
		burstVirusExplodesHelper (v3);
		burstVirusExplodesHelper (v4);

		burstVirusOn = false;
		//HERE UPDATE THE NEIBORING LINKS
		for (int ii = 2; ii < totalCols; ii++) {
			for (int jj = 0; jj < rows; jj++) {
				if(pieces[ii][jj] != null){
						pieces[ii][jj].GetComponent<Tile>().disableNeibor();
				}
			}
		}
		findNeiborsOnce = false;
	}

	private void burstVirusExplodesHelper(Vector2 v){
		markCorruption ((int)v.x, (int)v.y + shiftSpace);
		markCorruption ((int)v.x, (int)v.y-1+ shiftSpace);
		markCorruption ((int)v.x, (int)v.y + 1 + shiftSpace);
		markCorruption ((int)v.x-1, (int)v.y+ shiftSpace);
		markCorruption ((int)v.x-1, (int)v.y-1+ shiftSpace);
		markCorruption ((int)v.x-1, (int)v.y+1+ shiftSpace);
		markCorruption ((int)v.x+1, (int)v.y+ shiftSpace);
		markCorruption ((int)v.x+1, (int)v.y-1+ shiftSpace);
		markCorruption ((int)v.x+1, (int)v.y+1+ shiftSpace);
	}
	//--------------------------------------------------------------------------------------------------------

	//---------------------------------------------Burst Virus-----------------------------------------------------------
	public void addFuseVirus(){
		bool found1 = false;
		bool found2 = false;
		GameObject go1 = null;
		GameObject go2 = null;
		int i = 0;
		string color;
		//picks a set of 2 tiles in game
		for(i = offsetSpace; i <= visibleCols; i++){
			found1 = false;
			found2 = false;
			//bottom is found?
			if (pieces [i] [0] != null) {
				go1 = pieces [i] [0];
				found1 = true;
			}
			//top is found?
			if(pieces[i][rows - 1] != null){
				go2 = pieces [i] [rows-1];
				found2 = true;
			}
			//both is found - break the loop
			if(found1 && found2) break;
		}
		//if there is no set of 2 tiles -> active chain virus instead
		if (!found1 || !found2) {
			fuseVirusOn = false;
		}
		//gets the color of one of the 2 tiles
		color = go1.GetComponent<Tile> ().name;

		go1.GetComponent<SpriteRenderer> ().enabled = false;
		go1.GetComponent<Tile> ().disableNeibor ();
		go1.GetComponent<Tile> ().setDelete();

		go2.GetComponent<SpriteRenderer> ().enabled = false;
		go2.GetComponent<Tile> ().disableNeibor ();
		go2.GetComponent<Tile> ().setDelete();

		if (color == "blue") {
			pieces[i][0] = (GameObject)Instantiate(cubes[0],new Vector3(i,0 - shiftSpace, 0), Quaternion.identity);
			pieces[i][0].name = (nameCounter++).ToString();
			blueOut++;
			pieces[i][rows-1] = (GameObject)Instantiate(cubes[0],new Vector3(i,rows-1 - shiftSpace, 0), Quaternion.identity);
			pieces[i][rows-1].name = (nameCounter++).ToString();
			blueOut++;
		} else if (color == "red") {
			pieces[i][0] = (GameObject)Instantiate(cubes[1],new Vector3(i,0 - shiftSpace, 0 ), Quaternion.identity);
			pieces[i][0].name = (nameCounter++).ToString();
			redOut++;
			pieces[i][rows-1] = (GameObject)Instantiate(cubes[1],new Vector3(i,rows-1 - shiftSpace, 0), Quaternion.identity);
			pieces[i][rows-1].name = (nameCounter++).ToString();
			redOut++;
		} else if (color == "yellow") {
			pieces[i][0] = (GameObject)Instantiate(cubes[2],new Vector3(i,0 - shiftSpace, 0 ), Quaternion.identity);
			pieces[i][0].name = (nameCounter++).ToString();
			yellowOut++;
			pieces[i][rows-1] = (GameObject)Instantiate(cubes[2],new Vector3(i,rows-1 - shiftSpace, 0), Quaternion.identity);
			pieces[i][rows-1].name = (nameCounter++).ToString();
			yellowOut++;
		} else { //addtional color goes here
		}

		//pieces[i][0].GetComponent<SpriteRenderer> ().enabled = false;
		//pieces[i][rows-1].GetComponent<SpriteRenderer> ().enabled = false;

		fuseVirusArrayUp[0] = (GameObject)Instantiate(fuseVirusesUp[0],new Vector3(i,0 - shiftSpace, 0), Quaternion.identity);
		fuseVirusArrayDown[0] = (GameObject)Instantiate(fuseVirusesDown[0],new Vector3(i,rows-1 - shiftSpace, 0), Quaternion.identity);

		if (color == "blue") {
			fuseVirusArrayUp[0].GetComponent<SpriteRenderer>().color = Color.blue;
			fuseVirusArrayDown[0].GetComponent<SpriteRenderer>().color = Color.blue;
		} else if (color == "red") {
			fuseVirusArrayUp[0].GetComponent<SpriteRenderer>().color = Color.red;
			fuseVirusArrayDown[0].GetComponent<SpriteRenderer>().color = Color.red;
		} else if (color == "yellow") {
			fuseVirusArrayUp[0].GetComponent<SpriteRenderer>().color = Color.yellow;
			fuseVirusArrayDown[0].GetComponent<SpriteRenderer>().color = Color.yellow;
		} else { //addtional color goes here
		}
		//HERE UPDATE THE NEIBORING LINKS
		for (int ii = 2; ii < totalCols; ii++) {
			for (int jj = 0; jj < rows; jj++) {
				if(pieces[ii][jj] != null){
					pieces[ii][jj].GetComponent<Tile>().disableNeibor();
				}
			}
		}
		findNeiborsOnce = false;
	}

	private void addfuseVirusHelper(string color,int i,int j, int index, int dir){
		int jj = j;
		GameObject go = null;
		if(dir == 1){
			GameObject temp = fuseVirusArrayDown [index];
			temp.transform.position = new Vector3 (temp.transform.position.x, temp.transform.position.y - 1, -2);
			fuseVirusArrayDown [index+1] = temp;
			fuseVirusArrayDown [index] = (GameObject)Instantiate(fuseVirusesDown[1],new Vector3(i, j - shiftSpace, 0), Quaternion.identity);
			go = fuseVirusArrayDown [index];
			jj--;
		}else{
			GameObject temp = fuseVirusArrayUp [index];
			temp.transform.position = new Vector3 (temp.transform.position.x, temp.transform.position.y + 1, -2);
			fuseVirusArrayUp [index+1] = temp;
			fuseVirusArrayUp [index] = (GameObject)Instantiate(fuseVirusesUp[1],new Vector3(i, j - shiftSpace, 0), Quaternion.identity);
			go = fuseVirusArrayUp [index];
			jj++;
		}
		if (pieces [i] [jj] != null) {
			pieces [i] [jj].GetComponent<SpriteRenderer> ().enabled = false;
			pieces [i] [jj].GetComponent<Tile> ().disableNeibor ();
			pieces [i] [jj].GetComponent<Tile> ().setDelete ();
		}
		if (color == "red") {
			pieces[i][jj] = (GameObject)Instantiate(cubes[1],new Vector3(i,jj - shiftSpace, 0 ), Quaternion.identity);
			pieces[i][jj].name = (nameCounter++).ToString();
			redOut++;
			go.GetComponent<SpriteRenderer>().color = Color.red;
		} else if (color == "blue") {
			pieces[i][jj] = (GameObject)Instantiate(cubes[0],new Vector3(i,jj - shiftSpace, 0), Quaternion.identity);
			pieces[i][jj].name = (nameCounter++).ToString();
			blueOut++;
			go.GetComponent<SpriteRenderer>().color = Color.blue;
		} else if (color == "yellow") {
			pieces[i][jj] = (GameObject)Instantiate(cubes[2],new Vector3(i,jj - shiftSpace, 0 ), Quaternion.identity);
			pieces[i][jj].name = (nameCounter++).ToString();
			yellowOut++;
			go.GetComponent<SpriteRenderer>().color = Color.yellow;
		} else {

		}
		pieces[i][jj].GetComponent<SpriteRenderer> ().enabled = true;
	}
	void fuseVirusCleared(){
		for (int i = 0; i < fuseVirusArrayDown.Length; i++) {
			if(fuseVirusArrayDown[i] != null){
				Destroy(fuseVirusArrayDown[i]);
				fuseVirusArrayDown[i] = null;
			}
			if(fuseVirusArrayUp[i] != null){
				Destroy(fuseVirusArrayUp[i]);
				fuseVirusArrayUp[i] = null;
			}
		}
		fuseVirusOn = false;
		fuseVirusCount = 5;
	}

	void fuseVirusExplodes(){
		Vector2 v1 = fuseVirusArrayDown [0].transform.position;
		Vector2 v2 = fuseVirusArrayDown [1].transform.position;
		Vector2 v3 = fuseVirusArrayDown [2].transform.position;
		Vector2 v4 = fuseVirusArrayDown [3].transform.position;
		Vector2 v5 = fuseVirusArrayDown [4].transform.position;
		Vector2 v11 = fuseVirusArrayUp [0].transform.position;
		Vector2 v22 = fuseVirusArrayUp [1].transform.position;
		Vector2 v33 = fuseVirusArrayUp [2].transform.position;
		Vector2 v44 = fuseVirusArrayUp [3].transform.position;
		Vector2 v55 = fuseVirusArrayUp [4].transform.position;

		Destroy (fuseVirusArrayDown [0]);
		Destroy (fuseVirusArrayDown [1]);
		Destroy (fuseVirusArrayDown [2]);
		Destroy (fuseVirusArrayDown [3]);
		Destroy(fuseVirusArrayDown [4]);
		Destroy(fuseVirusArrayUp [0]);
		Destroy(fuseVirusArrayUp [1]);
		Destroy(fuseVirusArrayUp [2]);
		Destroy(fuseVirusArrayUp [3]);
		Destroy(fuseVirusArrayUp [4]);

		fuseVirusArrayDown [0] = null;
		fuseVirusArrayDown [1] = null;
		fuseVirusArrayDown [2] = null;
		fuseVirusArrayDown [3] = null;
		fuseVirusArrayDown [4] = null;
		fuseVirusArrayUp [0] = null;
		fuseVirusArrayUp [1] = null;
		fuseVirusArrayUp [2] = null;
		fuseVirusArrayUp [3] = null;
		fuseVirusArrayUp [4] = null;

		markCorruption ((int)v1.x,(int)v1.y+shiftSpace);
		markCorruption ((int)v2.x,(int)v2.y+shiftSpace);
		markCorruption ((int)v3.x,(int)v3.y+shiftSpace);
		markCorruption ((int)v4.x,(int)v4.y+shiftSpace);
		markCorruption ((int)v5.x,(int)v5.y+shiftSpace);
		markCorruption ((int)v11.x,(int)v11.y+shiftSpace);
		markCorruption ((int)v22.x,(int)v22.y+shiftSpace);
		markCorruption ((int)v33.x,(int)v33.y+shiftSpace);
		markCorruption ((int)v44.x,(int)v44.y+shiftSpace);
		markCorruption ((int)v55.x,(int)v55.y+shiftSpace);

		//HERE UPDATE THE NEIBORING LINKS
		for (int ii = 2; ii < totalCols; ii++) {
			for (int jj = 0; jj < rows; jj++) {
				if(pieces[ii][jj] != null){
					pieces[ii][jj].GetComponent<Tile>().disableNeibor();
				}
			}
		}
		findNeiborsOnce = false;
	}
	//-------------------------------------------------------------------------------------------------------------------

	//---------------------------------------------Chain Virus-----------------------------------------------------------
	public void addChainVirus(){
		int j = Random.Range (0, rows);
		int i = visibleCols-1;
		chainVirusHead = (GameObject)Instantiate(Resources.Load("Prefabs/ChainVirus/ChainVirus", typeof(GameObject)),new Vector3(i, j - shiftSpace, -2), Quaternion.identity);
		chainVirusArray [chainVirusIndex++] = chainVirusHead;
	}

	public void chainVirusCleared(){
		for(int i = 0; i <= chainVirusIndex; i++){
			Destroy(chainVirusArray[i]);
			chainVirusArray[i] = null;
		}
		chainVirusHead = null;
	}

	public void addChainVirusHelper(GameObject[] list){
		for(int idx = 0; idx < list.Length; idx++){
			if(list[idx]!= null){
				int ii = (int)list[idx].transform.position.x;
				int jj = (int)list[idx].transform.position.y + shiftSpace;
				chainVirusHead = (GameObject)Instantiate(Resources.Load("Prefabs/ChainVirus/ChainVirus", typeof(GameObject)),new Vector3(ii, jj - shiftSpace, -2), Quaternion.identity);
				chainVirusArray[chainVirusIndex++] = chainVirusHead;
			}
		}
	}
	//-------------------------------------------------------------------------------------------------------------------

	//---------------------------------------------Mark Corruption-----------------------------------------------------------
	public void markCorruption(int i, int j){
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;
		if (corruptedArray [i] [j] != null)
			return;
		if (pieces [i] [j] != null) {
			pieces[i][j].GetComponent<Tile>().disableNeibor();
			pieces[i][j].GetComponent<Tile>().setDelete();
		}
		corruptedArray[i][j] = (GameObject)Instantiate(Resources.Load("Prefabs/Corrupted", typeof(GameObject)),new Vector3(i,j - shiftSpace, -2 ), Quaternion.identity);
		corruptionCount++;
	}
	public void addCorruption(int i, int j){
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;
		if (corruptedArray [i] [j] != null)
			return;
		corruptedArray[i][j] = (GameObject)Instantiate(Resources.Load("Prefabs/Corrupted", typeof(GameObject)),new Vector3(i,j - shiftSpace, -2 ), Quaternion.identity);
		corruptionCount++;
	}
	//---------------------------------------------Delete Corruption-----------------------------------------------------------
	public void deleteCorruption(int i, int j){
		//Debug.Log ("i " + i + ", j " + j);
		if (i < 0 || i > totalCols-1) 
			return;
		if (j < 0 || j > rows-1)
			return;
		if (corruptedArray [i] [j] == null)
			return;
		Destroy (corruptedArray [i] [j]);
		corruptedArray [i] [j] = null;
		corruptionCount--;
	}
	//----------------------------------------------HEAL----------------------------------------------------------------------
	public void heal(){
		if (corruptionCount <= 0)
			return;
		repairMeter.GetComponent<RepairMeter> ().restart ();
		healOn = true;
		bool found = false;
		int i = 0;
		int j = 0;
		//picks a random corrupted indexes in game
		while (!found) {
			i = Random.Range (0, visibleCols);
			j = Random.Range (0, rows);
			if (corruptedArray [i] [j] != null) {
				found = true;
			}
		}
		healingAnim.transform.position = new Vector3 (i, j - shiftSpace, -2);
		healingAnim.GetComponent<SpriteRenderer> ().enabled = true;
		healHelper (i, j);
		deleteCorruption (i, j);
		findTileCountOnce = false;
	}

	private void healHelper(int i, int j){
		healingAnim.transform.position = Vector3.Lerp (healingAnim.transform.position, new Vector3 (i, j - shiftSpace, -2), 1f);
		healedTile = player.GetComponent<Cannon> ().createTile ();
		healedTile.transform.position = new Vector3 (i, j - shiftSpace, 0);
		if(pieces[i][j] != null)
			Destroy (pieces [i] [j]);
		pieces[i][j] = healedTile;
	}
}
//THIS IS THE CODE FOR DELETING ALL RED
//if(pieces[i][j].GetComponent<Tile>().name == "red"){
//	Debug.Log("im here");
//	pieces[i][j].GetComponent<Tile>().setDelete();