using System;
﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class encryptedMazeScript : MonoBehaviour
{
	public KMAudio Audio;
	public KMBombInfo bomb;
    //Directional Buttons
	public KMSelectable moveUp;
	public KMSelectable moveDown;
	public KMSelectable moveLeft;
	public KMSelectable moveRight;

	private int xPos = 0; //current position in the maze
	private int yPos = 0;

	private int xGoal; //position of the goal
	private int yGoal;
	//the goal's reference table has a shuffled x and y axis therfore these arrays map each number/feature onto a new coordinate
	private int[] xGoalReference = new int[6] {3, 5, 1, 4, 0, 2};
	private int[] yGoalReference = new int[6] {2, 4, 3, 0, 1, 5};

	private int xMarkerCw; //postition of the clockwise spinning marker
	private int yMarkerCw;

	private int xMarkerCcw; //position of the counter-clockwise spinning marker.
	private int yMarkerCcw;

	private int shapeMarkerCw; //holds the shapes and the features (thick-line, filled, crossed-out, etc.) of the spinning markers
	private int shapeMarkerCcw;
	private int featureMarkerCw;
	private int featureMarkerCcw;

	private int xManhattan = 0; //used to guarantee the goal and the start are at least 4 moves apart
	private int yManhattan = 0;

	private int[] modifier = new int[5]; //saves the value received from edgework based on the shape: 0=tri, 1=squ, 2=pent, 3=hex, 4=oct

	//maps every position in the maze to a unique number
	private int[,] mazeCoordinate = new int[6,6] { {0, 1, 2, 3, 4, 5},
												   {6, 7, 8, 9, 10, 11},
												   {12, 13, 14, 15, 16, 17},
												   {18, 19, 20, 21, 22, 23},
												   {24, 25, 26, 27, 28, 29},
 												   {30, 31, 32, 33, 34, 35} };

	//maps each unique number to the corresponding marker on the grid
	public TextMesh[] mazeIndex;
	//maps each unique number to the cooresponding pivot for rotation
	public GameObject[] rotationIndex;
	//maps each unique number to a corresponding dot on the grid
	public TextMesh[] mazeDots;

	//indexing all possible symbols [shape, feature] : thin, thick-dot, filled, thin-dot, thick, crossed-out
	private string[,] markerIndex = new string[5,6] { {"f", "H", "$", "l", "B", "N"},		//triangle
													  {"g", "I", "%", "m", "C", "O"},		//square
													  {"h", "J", "&", "n", "D", "P"},		//pentagon
													  {"i", "K", "'", "o", "E", "Q"},		//hexagon
													  {"j", "L", "(", "p", "F", "R"} };		//octagon

	//indexing the 3x3 grid of mazes from page 2 of the manual
	private int[,] mazeGridIndex = new int[3,3] { {0, 1, 2},
												  {3, 4, 5},
												  {6, 7, 8} };
  private int pickedMaze; //saves the index value (0-8) of the maze

	//hardcoding if its possible to move left in any maze from any position: 0 = no, 1 = yes [Maze, y, x]
	private int[,,] validMovLeft = new int[9,6,6] { { {0, 1, 1, 0, 1, 1},	//Maze 0
 													  {0, 0, 1, 0, 1, 1},
													  {0, 0, 1, 0, 1, 1},
													  {0, 0, 1, 1, 0, 1},
													  {0, 1, 1, 0, 1, 0},
													  {0, 1, 0, 1, 0, 1} },

													{ {0, 1, 1, 0, 1, 1},	//Maze 1
													  {0, 1, 0, 1, 0, 1},
													  {0, 0, 1, 0, 1, 1},
													  {0, 1, 0, 1, 0, 0},
													  {0, 0, 0, 0, 1, 0},
													  {0, 0, 1, 0, 1, 1} },

													{ {0, 1, 1, 0, 0, 1},	//Maze 2
													  {0, 0, 0, 0, 1, 0},
													  {0, 1, 0, 0, 1, 0},
													  {0, 0, 0, 0, 0, 0},
													  {0, 0, 1, 0, 0, 0},
													  {0, 1, 1, 1, 0, 1} },

													{ {0, 1, 0, 1, 1, 1},	//Maze 3
													  {0, 0, 0, 1, 1, 1},
													  {0, 0, 1, 0, 1, 0},
													  {0, 0, 1, 1, 1, 1},
												 	  {0, 1, 1, 1, 1, 0},
													  {0, 1, 1, 0, 1, 0} },

													{ {0, 1, 1, 1, 1, 1},	//Maze 4
													  {0, 1, 1, 1, 1, 0},
													  {0, 1, 0, 1, 0, 1},
													  {0, 0, 1, 1, 0, 0},
													  {0, 0, 1, 1, 1, 0},
													  {0, 0, 1, 1, 1, 1} },

													{ {0, 0, 1, 0, 1, 1},	//Maze 5
												 	  {0, 0, 0, 0, 1, 0},
													  {0, 1, 0, 0, 0, 1},
													  {0, 1, 0, 1, 0, 0},
													  {0, 1, 0, 0, 0, 1},
													  {0, 1, 1, 1, 0, 1} },

													{ {0, 1, 1, 1, 0, 1},	//Maze 6
													  {0, 0, 1, 0, 1, 0},
													  {0, 1, 0, 1, 0, 1},
													  {0, 1, 0, 1, 1, 0},
													  {0, 0, 0, 1, 1, 0},
													  {0, 1, 1, 1, 1, 1} },

													{ {0, 0, 1, 1, 0, 1},	//Maze 7
													  {0, 1, 1, 0, 1, 0},
													  {0, 0, 1, 1, 1, 0},
													  {0, 0, 1, 0, 1, 1},
													  {0, 0, 0, 1, 1, 1},
													  {0, 1, 1, 1, 1, 1} },

													{ {0, 1, 1, 1, 1, 1},	//Maze 8
													  {0, 0, 0, 1, 0, 0},
													  {0, 1, 1, 0, 1, 0},
													  {0, 0, 0, 1, 0, 1},
													  {0, 0, 0, 0, 1, 0},
													  {0, 1, 0, 1, 0, 1} } };

	//hardcoding if its possible to move up in any maze from any position: 0 = no, 1 = yes [Maze, y, x]
	private int[,,] validMovUp = new int[9,6,6] { { {0, 0, 0, 0, 0, 0},	//Maze 0
 													{1, 0, 1, 1, 0, 0},
													{1, 1, 0, 0, 0, 1},
													{1, 0, 1, 1, 0, 1},
													{1, 0, 0, 0, 0, 1},
													{1, 0, 1, 1, 0, 1} },

												  { {0, 0, 0, 0, 0, 0},	//Maze 1
													{1, 1, 0, 1, 1, 0},
													{1, 0, 1, 0, 0, 1},
													{1, 1, 0, 1, 0, 1},
													{1, 0, 1, 0, 1, 1},
													{1, 1, 1, 1, 0, 1} },

												  { {0, 0, 0, 0, 0, 0},	//Maze 2
													{1, 0, 1, 1, 1, 1},
													{0, 1, 1, 0, 0, 1},
													{1, 1, 1, 1, 1, 1},
													{1, 1, 1, 1, 1, 1},
													{1, 0, 0, 1, 1, 1} },

												  { {0, 0, 0, 0, 0, 0},	//Maze 3
													{1, 1, 0, 0, 0, 1},
													{1, 1, 1, 0, 0, 1},
													{1, 0, 0, 1, 0, 1},
													{1, 0, 0, 0, 0, 1},
													{1, 0, 0, 0, 1, 1} },

												  { {0, 0, 0, 0, 0, 0},	//Maze 4
													{0, 0, 0, 0, 1, 1},
													{1, 0, 0, 1, 0, 0},
													{1, 1, 0, 0, 1, 1},
													{1, 0, 0, 1, 0, 1},
													{1, 1, 0, 0, 0, 1} },

												  { {0, 0, 0, 0, 0, 0}, //Maze 5
													{1, 1, 1, 0, 1, 1},
													{1, 1, 1, 1, 0, 1},
													{1, 0, 0, 1, 1, 0},
													{0, 1, 1, 1, 1, 1},
													{1, 0, 0, 1, 0, 1} },

												  { {0, 0, 0, 0, 0, 0}, //Maze 6
													{1, 0, 0, 1, 1, 1},
													{1, 1, 0, 0, 0, 1},
													{0, 0, 1, 0, 1, 0},
													{1, 1, 1, 0, 0, 1},
													{1, 0, 0, 0, 1, 1} },

												  { {0, 0, 0, 0, 0, 0}, //Maze 7
													{1, 1, 0, 1, 1, 1},
													{1, 0, 0, 0, 0, 1},
													{1, 1, 0, 0, 1, 1},
													{1, 0, 1, 0, 0, 0},
													{1, 1, 0, 0, 0, 0} },

												  { {0, 0, 0, 0, 0, 0}, //Maze 8
													{1, 1, 0, 0, 1, 1},
													{1, 1, 1, 0, 1, 1},
													{1, 0, 0, 1, 0, 1},
													{1, 1, 1, 0, 0, 1},
													{1, 1, 1, 1, 1, 0} } };



	public Color[] fontColors; //these are the colors to change font colors 0 = darkblue, 1 = white, 2 = invisible, 3 = green
 	public Material[] screenMaterial; //materials for the background screen 0 = blue, 1 = red
	public MeshRenderer screen; // holds the connection to the screen object

	//Logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;
	private bool inStrike;
	private string[] shapeLog = new string[5] {"triangle", "square", "pentagon", "hexagon", "octagon"};
	private string[] featureLog = new string[6] {"thin-lined", "thick-dotted", "filled", "thin-dotted", "thick-lined", "crossed-out"};



	void Awake ()
	{
		moduleId = moduleIdCounter++;
        //disable all text on the module until the lights turn on
        foreach (TextMesh text in mazeIndex)
        {
          text.gameObject.SetActive(false);
        }

		foreach (TextMesh dot in mazeDots)
		{
		  dot.gameObject.SetActive(false);
		}

		//delegate button press to method
		moveLeft.OnInteract += delegate () { PressLeft(); return false; };
		moveRight.OnInteract += delegate () { PressRight(); return false; };
		moveUp.OnInteract += delegate () { PressUp(); return false; };
		moveDown.OnInteract += delegate () { PressDown(); return false; };
		GetComponent<KMBombModule>().OnActivate += OnActivate;
	}

	// Use this for initialization
	void Start ()
	{
		Debug.LogFormat("[Encrypted Maze #{0}] All coordinates are logged as: (x, y) with 'x' as the horizontal axis, 'y' as the vertical axis and (1, 1) as the upper left-hand corner.", moduleId, xPos+1, yPos+1);
		GetEdgework();
		CalculateStartingPoint();
		CalculateGoal();
		Debug.LogFormat("[Encrypted Maze #{0}] Your starting position is: ({1}, {2})", moduleId, xPos+1, yPos+1);
		Debug.LogFormat("[Encrypted Maze #{0}] Your goal is on: ({1}, {2})", moduleId, xGoal+1, yGoal+1);
		PickMaze();
		UpdateDisplay();
	}

	// Update is called once per frame
	void Update ()
	{
		if (moduleSolved) {return;}
		//assign CW rotation to marker
		rotationIndex[mazeCoordinate[yMarkerCw, xMarkerCw]].transform.Rotate(new Vector3(0f, 40f, 0f) * Time.deltaTime);
		//assign CCW rotation to marker
		rotationIndex[mazeCoordinate[yMarkerCcw, xMarkerCcw]].transform.Rotate(new Vector3(0f, -40f, 0f) * Time.deltaTime);
	}

	void OnActivate()
	{
        //the lights have turned on, activate all text
        foreach (TextMesh text in mazeIndex)
        {
          text.gameObject.SetActive(true);
        }

		foreach (TextMesh dot in mazeDots)
		{
		  dot.gameObject.SetActive(true);
		}
	}

	void GetEdgework()
	{
		//get modifiers from edgework
		modifier[0] = bomb.GetBatteryCount(Battery.D); //triangle
		modifier[1] = bomb.GetOffIndicators().Count(); //square
		modifier[2] = bomb.GetBatteryCount(Battery.AA); //pentagon
		modifier[3] = bomb.GetOnIndicators().Count(); //hexagon
		modifier[4] = bomb.CountUniquePorts(); //octagon
		Debug.LogFormat("[Encrypted Maze #{0}] The shape values are: triangle = {1}, square = {2}, pentagon = {3}, hexagon = {4}, octagon = {5}", moduleId, modifier[0], modifier[1], modifier[2], modifier[3], modifier[4]);
	}

	void CalculateStartingPoint()
	{
		//set the marker spinning clockwise
		xMarkerCw = UnityEngine.Random.Range(0, 6); // random value between 0 and 5
		yMarkerCw = UnityEngine.Random.Range(0, 6);
		shapeMarkerCw = UnityEngine.Random.Range(0, 5);
		featureMarkerCw = UnityEngine.Random.Range(0, 6);
		Debug.LogFormat("[Encrypted Maze #{0}] The marker spinning clockwise is a {1} {2} on:  ({3}, {4})", moduleId, featureLog[featureMarkerCw], shapeLog[shapeMarkerCw], xMarkerCw+1, yMarkerCw+1);
		//starting point references the y coordinate and the symbol of the marker spinning CW
		xPos = Mod((yMarkerCw + modifier[shapeMarkerCw]), 6); // y value gets modyfied by edgework based on the shape of the marker
		yPos = featureMarkerCw;
	}


	void CalculateGoal()
	{
		//keep resetting the goal's position as long as the manhattan distance is too low.
		do
		{
			do
			{
				xMarkerCcw = UnityEngine.Random.Range(0, 6); //assign a positon to the CCW marker.
				yMarkerCcw = UnityEngine.Random.Range(0, 6);
			}
			while (xMarkerCcw == xMarkerCw); //if the CCW marker recieves the same column as the CW marker, reassign the CCW marker. (this is to prevent both markers from ever overlapping)

			shapeMarkerCcw = UnityEngine.Random.Range(0, 5); //assing a shape and feature to the CCW marker.
			featureMarkerCcw = UnityEngine.Random.Range(0, 6);

			//goal is referenced by the y coordinate and the symbol of the marker spinning CCW
			xGoal = xGoalReference[featureMarkerCcw];
			yGoal = yGoalReference[Mod((yMarkerCcw + modifier[shapeMarkerCcw]), 6)]; // y value gets modyfied by edgework based on the shape of the marker, both values get reassiged via the lookup fot the shuffeled table.

			//calculate the manhattan distance between the start and the goal
			if (xPos >= xGoal)	{xManhattan = xPos - xGoal;}
			else	{xManhattan = xGoal - xPos;}

			if (yPos >= yGoal)	{yManhattan = yPos - yGoal;}
			else  {xManhattan = yGoal - yPos;}
			//Debug.LogFormat("[Encrypted Maze #{0}] The manhattan distance between the start and the goal is: {1}. Aiming for at least 4.", moduleId, xManhattan + yManhattan);
		}
		while ((xManhattan + yManhattan) < 4); //check if the manhattan distance is at least 4

		Debug.LogFormat("[Encrypted Maze #{0}] The marker spinning counter-clockwise is a {1} {2} on:  ({3}, {4})", moduleId, featureLog[featureMarkerCcw], shapeLog[shapeMarkerCcw], xMarkerCcw+1, yMarkerCcw+1);
	}

	void PickMaze()
	{
		//use the x coordinate of the spinning markers to determine the maze (left = CW,  down = CCW)
		pickedMaze = mazeGridIndex[Mod(xMarkerCcw, 3), Mod(xMarkerCw, 3)];
		Debug.LogFormat("[Encrypted Maze #{0}] Picked maze Nr. {1} (in reading order).", moduleId, (pickedMaze + 1));
	}

	int Mod(int x, int m) // modulo function that always returns a positive value
	{
		return (x % m + m) % m;
	}


	void UpdateDisplay()
	{
		foreach (TextMesh text in mazeIndex) //refers to every symbol in the grid
		{
			text.text = "";
		}
		foreach (TextMesh dot in mazeDots) //refers to all the dots.
		{
			dot.color = fontColors[0];
		}
		//set the CW spinning marker
		mazeIndex[mazeCoordinate[yMarkerCw, xMarkerCw]].text = markerIndex[shapeMarkerCw, featureMarkerCw];
		mazeDots[mazeCoordinate[yMarkerCw, xMarkerCw]].color = fontColors[2];

		//set the CCW spinning marker
		mazeIndex[mazeCoordinate[yMarkerCcw, xMarkerCcw]].text = markerIndex[shapeMarkerCcw, featureMarkerCcw];
		mazeDots[mazeCoordinate[yMarkerCcw, xMarkerCcw]].color = fontColors[2];
	}



	void PressLeft()
	{
		if(moduleSolved || inStrike){return;}
		moveLeft.AddInteractionPunch();
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, moveLeft.transform);

		if ((xPos > 0) && (validMovLeft[pickedMaze, yPos, xPos] == 1)) // if the move is allowed (no wall, no edge), increase the x value
		{
			xPos --;
			yMarkerCw = Mod((yMarkerCw - 1), 6);
			UpdateDisplay();
			Debug.LogFormat("[Encrypted Maze #{0}] You moved left to ({1}, {2})", moduleId, xPos+1, yPos+1);
		}
		else // if the move is not allowed handle a strike
		{
			GetComponent<KMBombModule>().HandleStrike();
			StartCoroutine(Strike());
			Debug.LogFormat("[Encrypted Maze #{0}] You tried to move left from ({1}, {2}) and ran into a wall, strike!", moduleId, xPos+1, yPos+1);
		}

		if (xPos == xGoal && yPos == yGoal) // if you reached the goal
		{
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
			screen.material = screenMaterial[2];
			Audio.PlaySoundAtTransform("jingle", transform);
			foreach (TextMesh text in mazeIndex) //refers to every symbol in the grid
			{
				text.color = fontColors[3];
			}
			foreach (TextMesh dot in mazeDots)
			{
				dot.color = fontColors[2];
			}
			Debug.LogFormat("[Encrypted Maze #{0}] You moved on ({1}, {2}) and reached the goal. The module is solved!", moduleId, xPos+1, yPos+1);
		}

	}


	void PressRight()
	{
		if(moduleSolved || inStrike){return;}
		moveRight.AddInteractionPunch();
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, moveRight.transform);

		if ((xPos < 5) && (validMovLeft[pickedMaze, yPos, (xPos + 1)] == 1))
		{
			xPos ++;
			yMarkerCw = Mod((yMarkerCw + 1), 6);
			UpdateDisplay();
			Debug.LogFormat("[Encrypted Maze #{0}] You moved right to ({1}, {2})", moduleId, xPos+1, yPos+1);
		}
		else
		{
			GetComponent<KMBombModule>().HandleStrike();
			StartCoroutine(Strike());
			Debug.LogFormat("[Encrypted Maze #{0}] You tried to move right from ({1}, {2}) and ran into a wall, strike!", moduleId, xPos+1, yPos+1);
		}

		if (xPos == xGoal && yPos == yGoal) // if you reached the goal
		{
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
			screen.material = screenMaterial[2];
			Audio.PlaySoundAtTransform("jingle", transform);
			foreach (TextMesh text in mazeIndex) //refers to every symbol in the grid
			{
				text.color = fontColors[3];
			}
			foreach (TextMesh dot in mazeDots)
			{
				dot.color = fontColors[2];
			}
			Debug.LogFormat("[Encrypted Maze #{0}] You moved on ({1}, {2}) and reached the goal. The module is solved!", moduleId, xPos+1, yPos+1);
		}
	}



	void PressUp()
	{
		if(moduleSolved || inStrike){return;}
		moveUp.AddInteractionPunch();
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, moveUp.transform);

		if ((yPos > 0) && (validMovUp[pickedMaze, yPos, xPos] == 1))
		{
			yPos --;
			featureMarkerCw = Mod((featureMarkerCw - 1), 6);
			UpdateDisplay();
			Debug.LogFormat("[Encrypted Maze #{0}] You moved up to ({1}, {2})", moduleId, xPos+1, yPos+1);
		}
		else
		{
			GetComponent<KMBombModule>().HandleStrike();
			StartCoroutine(Strike());
			Debug.LogFormat("[Encrypted Maze #{0}] You tried to move up from ({1}, {2}) and ran into a wall, strike!", moduleId, xPos+1, yPos+1);
		}

		if (xPos == xGoal && yPos == yGoal) // if you reached the goal
		{
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
			screen.material = screenMaterial[2];
			Audio.PlaySoundAtTransform("jingle", transform);
			foreach (TextMesh text in mazeIndex) //refers to every symbol in the grid
			{
				text.color = fontColors[3];
			}
			foreach (TextMesh dot in mazeDots)
			{
				dot.color = fontColors[2];
			}
			Debug.LogFormat("[Encrypted Maze #{0}] You moved on ({1}, {2}) and reached the goal. The module is solved!", moduleId, xPos+1, yPos+1);
		}
	}



	void PressDown()
	{
		if(moduleSolved || inStrike){return;}
		moveDown.AddInteractionPunch();
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, moveDown.transform);

		if ((yPos < 5) && (validMovUp[pickedMaze, (yPos + 1), xPos] == 1))
		{
			yPos ++;
			featureMarkerCw = Mod((featureMarkerCw + 1), 6);
			UpdateDisplay();
			Debug.LogFormat("[Encrypted Maze #{0}] You moved down to ({1}, {2})", moduleId, xPos+1, yPos+1);
		}
		else
		{
			GetComponent<KMBombModule>().HandleStrike();
			StartCoroutine(Strike());
			Debug.LogFormat("[Encrypted Maze #{0}] You tried to move down from ({1}, {2}) and ran into a wall, strike!", moduleId, xPos+1, yPos+1);
		}

		if (xPos == xGoal && yPos == yGoal) // if you reached the goal
		{
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
			screen.material = screenMaterial[2];
			Audio.PlaySoundAtTransform("jingle", transform);
			foreach (TextMesh text in mazeIndex) //refers to every symbol in the grid
			{
				text.color = fontColors[3];
			}
			foreach (TextMesh dot in mazeDots)
			{
				dot.color = fontColors[2];
			}
			Debug.LogFormat("[Encrypted Maze #{0}] You moved on ({1}, {2}) and reached the goal. The module is solved!", moduleId, xPos+1, yPos+1);
		}
	}


    IEnumerator Strike()
	{
		inStrike = true;
		screen.material = screenMaterial[1]; // set screen to red
		yield return new WaitForSeconds(.8f);
		screen.material = screenMaterial[0]; // set screen to blue
		inStrike = false;
	}

    //twitch plays (adopted code from eXish made for Shifted Maze)
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} move udlr [Move in the specified directions in order; u = up, r = right, d = down, l = left]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*move\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify which directions to move!";
            }
            else
            {
                char[] parameters2 = parameters[1].ToCharArray();
                var buttonsToPress = new List<KMSelectable>();
                foreach (char c in parameters2)
                {
                    if (c.Equals('u') || c.Equals('U'))
                    {
                        buttonsToPress.Add(moveUp);
                    }
                    else if (c.Equals('r') || c.Equals('R'))
                    {
                        buttonsToPress.Add(moveRight);
                    }
                    else if (c.Equals('d') || c.Equals('D'))
                    {
                        buttonsToPress.Add(moveDown);
                    }
                    else if (c.Equals('l') || c.Equals('L'))
                    {
                        buttonsToPress.Add(moveLeft);
                    }
                    else
                    {
                        yield return "sendtochaterror The specified direction to move '" + c + "' is invalid!";
                        yield break;
                    }
                }
                foreach (KMSelectable km in buttonsToPress)
                {
                    km.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield break;
        }
    }

	//this code is basically the autosolver eXish made for 'shifted maze' with some adjustments.
	IEnumerator TwitchHandleForcedSolve()
	{
		while (inStrike) { yield return true; yield return new WaitForSeconds(0.1f); }

		var q = new Queue<int[]>();
		var allMoves = new List<Movement>();
		var startPoint = new int[] { xPos, yPos };
		var target = new int[] { xGoal, yGoal };
		q.Enqueue(startPoint);
		while (q.Count > 0)
		{
			var next = q.Dequeue();
			if (next[0] == target[0] && next[1] == target[1])
				goto readyToSubmit;
			string paths = "";
			if ((next[1] > 0) && (validMovUp[pickedMaze, next[1], next[0]] == 1)) { paths += "U"; }
			if ((next[0] < 5) && (validMovLeft[pickedMaze, next[1], next[0] + 1] == 1)) { paths += "R"; }
			if ((next[1] < 5) && (validMovUp[pickedMaze, next[1] + 1, next[0]] == 1)) { paths += "D"; }
			if ((next[0] > 0) && (validMovLeft[pickedMaze, next[1], next[0]] == 1)) { paths += "L"; }
			var cell = paths;
			var allDirections = "URDL";
			var offsets = new int[,] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
			for (int i = 0; i < 4; i++)
			{
				var check = new int[] { next[0] + offsets[i, 0], next[1] + offsets[i, 1] };
                if (cell.Contains(allDirections[i]) && !allMoves.Any(x => x.start[0] == check[0] && x.start[1] == check[1]))
                {
                    q.Enqueue(new int[] { next[0] + offsets[i, 0], next[1] + offsets[i, 1] });
                    allMoves.Add(new Movement { start = next, end = new int[] { next[0] + offsets[i, 0], next[1] + offsets[i, 1] }, direction = i });
                }
			}
		}
		throw new InvalidOperationException("There is a bug in maze generation.");
		readyToSubmit:
		KMSelectable[] buttons = new KMSelectable[] { moveUp, moveRight, moveDown, moveLeft };
		if (allMoves.Count != 0) // Checks for position already being a target
		{
			var lastMove = allMoves.First(x => x.end[0] == target[0] && x.end[1] == target[1]);
			var relevantMoves = new List<Movement> { lastMove };
			while (lastMove.start != startPoint)
			{
				lastMove = allMoves.First(x => x.end[0] == lastMove.start[0] && x.end[1] == lastMove.start[1]);
				relevantMoves.Add(lastMove);
			}
			for (int i = 0; i < relevantMoves.Count; i++)
			{
				buttons[relevantMoves[relevantMoves.Count - 1 - i]. direction].OnInteract();
				yield return new WaitForSeconds(.1f);
			}
		}
	}

	class Movement
	{
		public int[] start;
		public int[] end;
		public int direction;
	}
}
