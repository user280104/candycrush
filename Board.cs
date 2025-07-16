using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
//using UnityEngine.Scripting.APIUpdating;


public enum GameState
{
    wait, move, win, lose, pause
}


public enum TileKind
{
    breakable, blank, normal, Lock, concrete, slime
}
[System.Serializable]
public class MatchType
{
    public int type;
    public string color;
}
//custom class so unity does not know that it has to be displayed in editor so we serialize it 
[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;

}

public class Board : MonoBehaviour
{
    public World world;
    public int level;
    private HintManager hintManager;
    public GameState currentState = GameState.move;
    [Header("Board Dimensions")]
    public int width;
    public int height;
    public int offSet;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject lockTilePrefab;
    public GameObject concreteTilePrefab;
    public GameObject slimePrefab;

    public GameObject[] dots;
    public GameObject destroyEffect;

    [Header("Layout")]
    public TileType[] boardlayout;
    private bool[,] blankSpaces;
    private BackgroundTiles[,] breakableTiles;
    public BackgroundTiles[,] lockTiles;
    public BackgroundTiles[,] concreteTiles;
    public BackgroundTiles[,] slimeTiles;
    public GameObject[,] allDots;


    [Header("Match Stuff")]
    public MatchType matchType;
    public Dots currentDot;
    private FindMatches findm;
    public int basePieceValue = 20;
    private int streakValue = 1;
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    private GoalManager goalManager;
    public float refillDelay = 0.5f;
    public int[] scoreGoals;
    private bool makeSlime = true;

    // to see if a world has been assigned
    private void Awake()
    {
        if (PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if (world != null)
        {
            if (level < world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    scoreGoals = world.levels[level].scoreGoals;
                    boardlayout = world.levels[level].boardLayout;
                }
            }

        }
    }


    // Start is called before the first frame update
    void Start()
    {
        goalManager = FindObjectOfType<GoalManager>();
        soundManager = FindObjectOfType<SoundManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        breakableTiles = new BackgroundTiles[width, height];
        lockTiles = new BackgroundTiles[width, height];
        concreteTiles = new BackgroundTiles[width, height];
        slimeTiles = new BackgroundTiles[width, height];
        findm = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];
        SetUp();
        currentState = GameState.move;
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardlayout.Length; i++)
        {
            if (boardlayout[i].tileKind == TileKind.blank)
            {
                blankSpaces[boardlayout[i].x, boardlayout[i].y] = true;
            }
        }
    }
    public void GenerateBreakableTiles()
    {
        //look at all tiles in layout

        for (int i = 0; i < boardlayout.Length; i++)
        {
            //if a tile is a "jelly" tile 
            if (boardlayout[i].tileKind == TileKind.breakable)
            {
                //create "jelly" tile at that position
                Vector2 tempPosition = new Vector2(boardlayout[i].x, boardlayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardlayout[i].x, boardlayout[i].y] = tile.GetComponent<BackgroundTiles>();
            }
        }
    }
    private void GenerateLockTiles()
    {
        //look at all tiles in layout

        for (int i = 0; i < boardlayout.Length; i++)
        {
            //if a tile is a "Lock" tile 
            if (boardlayout[i].tileKind == TileKind.Lock)
            {
                //create "Lock" tile at that position
                Vector2 tempPosition = new Vector2(boardlayout[i].x, boardlayout[i].y);
                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardlayout[i].x, boardlayout[i].y] = tile.GetComponent<BackgroundTiles>();
            }
        }
    }
    private void GenerateConcreteTiles()
    {
        //look at all tiles in layout

        for (int i = 0; i < boardlayout.Length; i++)
        {
            //if a tile is a "concrete" tile 
            if (boardlayout[i].tileKind == TileKind.concrete)
            {
                //create "concrete" tile at that position
                Vector2 tempPosition = new Vector2(boardlayout[i].x, boardlayout[i].y);
                GameObject tile = Instantiate(concreteTilePrefab, tempPosition, Quaternion.identity);
                concreteTiles[boardlayout[i].x, boardlayout[i].y] = tile.GetComponent<BackgroundTiles>();
            }
        }
    }
    private void GenerateSlimeTiles()
    {
        //look at all tiles in layout

        for (int i = 0; i < boardlayout.Length; i++)
        {
            //if a tile is a "concrete" tile 
            if (boardlayout[i].tileKind == TileKind.slime)
            {
                //create "concrete" tile at that position
                Vector2 tempPosition = new Vector2(boardlayout[i].x, boardlayout[i].y);
                GameObject tile = Instantiate(slimePrefab, tempPosition, Quaternion.identity);
                slimeTiles[boardlayout[i].x, boardlayout[i].y] = tile.GetComponent<BackgroundTiles>();
            }
        }
    }

    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        GenerateLockTiles();
        GenerateConcreteTiles();
        GenerateSlimeTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    Vector2 tilePosition = new Vector2(i, j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "(" + i + "," + j + ")";
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                    }
                    maxIterations = 0;

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dots>().row = j;
                    dot.GetComponent<Dots>().column = i;

                    dot.transform.parent = this.transform;
                    dot.name = "(" + i + "," + j + ")";
                    allDots[i, j] = dot;
                }
            }
        }

        if (IsDeadLocked())
        {
            ShuffleBoard();
        }

    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }

        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
                {
                    if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }

            }
            if (column > 1)
            {
                if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
                {
                    if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }

            }
        }
        return false;
    }
    private MatchType ColumnOrRow()
    {
        //make a copy of current matches
        List<GameObject> matchCopy = findm.currentMatches as List<GameObject>;
        matchType.type = 0;
        matchType.color = "";
        //cycle thru all match copy and decide if a bomb needs to be made
        for (int i = 0; i < matchCopy.Count; i++)
        {
            //store this dot
            Dots thisDot = matchCopy[i].GetComponent<Dots>();
            string color = matchCopy[i].tag;
            int column = thisDot.column;
            int row = thisDot.row;
            int columnMatch = 0;
            int rowMatch = 0;
            //cycle thru rest of dots and compare
            for (int j = 0; j < matchCopy.Count; j++)
            {
                //store the next dot 
                Dots nextDot = matchCopy[j].GetComponent<Dots>();
                if (nextDot == thisDot)
                {
                    continue;//go to next iteration of for loop
                }
                if (nextDot.column == thisDot.column && nextDot.tag == color)
                {
                    columnMatch++;
                }
                if (nextDot.row == thisDot.row && nextDot.tag == color)
                {
                    rowMatch++;
                }
            }
            //check what column and row matches are
            //return 3 if column or row match
            //return 2 if adjacent match
            //return 1 if its a color bomb
            if (columnMatch == 4 || rowMatch == 4)
            {
                matchType.type = 1;
                matchType.color = color;
                return matchType;
            }
            else if (columnMatch == 2 && rowMatch == 2)
            {
                matchType.type = 2;
                matchType.color = color;
                return matchType;
            }
            else if (columnMatch == 3 || rowMatch == 3)
            {
                matchType.type = 1;
                matchType.color = color;
                return matchType;
            }
        }
        matchType.type = 0;
        matchType.color = "";
        return matchType;
        /* int numberHorizontal = 0;
         int numberVertical = 0;
         Dots firstpiece = findm.currentMatches[0].GetComponent<Dots>();
         if (firstpiece != null)
         {
             foreach (GameObject currentPiece in findm.currentMatches)
             {
                 Dots dot = currentPiece.GetComponent<Dots>();
                 if (dot.row == firstpiece.row)
                 {
                     numberHorizontal++;

                 }
                 if (dot.row == firstpiece.column)
                 {
                     numberVertical++;
                 }
             }
         }
         return (numberVertical == 5 || numberHorizontal == 5);
 */
    }
    private void CheckToMakeBombs()
    {
        //how many objects are in findm currentMatches
        if (findm.currentMatches.Count > 3)
        {
            //type of match row or column returns 
            MatchType typeOfMatch = ColumnOrRow();
            if (typeOfMatch.type == 1)

            {
                //make color bomb 
                if (currentDot != null && currentDot.isMatch && currentDot.tag == typeOfMatch.color)
                {


                    currentDot.isMatch = false;
                    currentDot.MakeColorBomb();
                }

                else
                {
                    if (currentDot.otherDot != null)
                    {
                        Dots otherDot = currentDot.otherDot.GetComponent<Dots>();
                        if (otherDot.isMatch && otherDot.tag == typeOfMatch.color)
                        {

                            otherDot.isMatch = false;
                            otherDot.MakeColorBomb();
                        }

                    }
                }
            }


            else if (typeOfMatch.type == 2)
            {
                //make adjacent bomb
                if (currentDot != null && currentDot.isMatch && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatch = false;
                    currentDot.AdjacentBomb();
                }

                else
                {
                    if (currentDot.otherDot != null)
                    {
                        Dots otherDot = currentDot.otherDot.GetComponent<Dots>();
                        if (otherDot.isMatch && otherDot.tag == typeOfMatch.color)
                        {

                            otherDot.isMatch = false;
                            otherDot.AdjacentBomb();


                        }

                    }

                }


            }
            else if (typeOfMatch.type == 3)
            {
                findm.CheckBombs(typeOfMatch);
            }
        }
    }

    public void BombRow(int row)
    {
        for (int i = 0; i < width; i++)
        {

            if (concreteTiles[i, row])
            {
                concreteTiles[i, row].TakeDamage(1);
                if (concreteTiles[i, row].hitPoints <= 0)
                {
                    concreteTiles[i, row] = null;
                }

            }

        }
    }
    public void BombColumn(int column)
    {
        for (int i = 0; i < width; i++)
        {

            if (concreteTiles[column, i])
            {
                concreteTiles[column, i].TakeDamage(1);
                if (concreteTiles[column, i].hitPoints <= 0)
                {
                    concreteTiles[column, i] = null;
                }

            }

        }
    }
    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dots>().isMatch)
        {

            //does a tile need to break
            if (breakableTiles[column, row] != null)
            {
                //if it does, give one damage
                breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }
            if (lockTiles[column, row] != null)
            {
                //if it does, give one damage
                lockTiles[column, row].TakeDamage(1);
                if (lockTiles[column, row].hitPoints <= 0)
                {
                    lockTiles[column, row] = null;
                }
            }
            DamageConcrete(column, row);
            DamageSlime(column, row);
            if (goalManager != null)
            {
                goalManager.CompareGoal(allDots[column, row].tag.ToString());
                goalManager.UpdateGoals();
            }
            //does sound manager exist
            if (soundManager != null)
            {
                soundManager.PlayRandomDestroyNoise();
            }
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .4f);
            Destroy(allDots[column, row]);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
            allDots[column, row] = null;
        }
    }
    public void DestroyMatches()
    {
        //how many elements are in the matched pieces list from findmatches
        if (findm.currentMatches.Count >= 4)
        {
            CheckToMakeBombs();
        }
        findm.currentMatches.Clear();

        if (hintManager != null)
        {
            hintManager.DestroyHint();
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }

        StartCoroutine(DecreaseRowCo2());
    }
    private void DamageConcrete(int column, int row)
    {
        //left
        if (column > 0)
        {
            if (concreteTiles[column - 1, row])
            {
                concreteTiles[column - 1, row].TakeDamage(1);
                if (concreteTiles[column - 1, row].hitPoints <= 0)
                {
                    concreteTiles[column - 1, row] = null;
                }
            }
        }
        //right
        if (column < width - 1)
        {
            if (concreteTiles[column + 1, row])
            {
                concreteTiles[column + 1, row].TakeDamage(1);
                if (concreteTiles[column + 1, row].hitPoints <= 0)
                {
                    concreteTiles[column + 1, row] = null;
                }
            }
        }
        //down
        if (row > 0)
        {
            if (concreteTiles[column, row - 1])
            {
                concreteTiles[column, row - 1].TakeDamage(1);
                if (concreteTiles[column, row - 1].hitPoints <= 0)
                {
                    concreteTiles[column, row - 1] = null;
                }
            }
        }
        //up
        if (row < height - 1)
        {
            if (concreteTiles[column, row + 1])
            {
                concreteTiles[column, row + 1].TakeDamage(1);
                if (concreteTiles[column, row + 1].hitPoints <= 0)
                {
                    concreteTiles[column - 1, row + 1] = null;
                }
            }
        }
    }

    private void DamageSlime(int column, int row)
    {
        //left
        if (column > 0)
        {
            if (slimeTiles[column - 1, row])
            {
                slimeTiles[column - 1, row].TakeDamage(1);
                if (slimeTiles[column - 1, row].hitPoints <= 0)
                {
                    slimeTiles[column - 1, row] = null;
                }
                makeSlime = false;
            }
        }
        //right
        if (column < width - 1)
        {
            if (slimeTiles[column + 1, row])
            {
                slimeTiles[column + 1, row].TakeDamage(1);
                if (slimeTiles[column + 1, row].hitPoints <= 0)
                {
                    slimeTiles[column + 1, row] = null;
                }
                makeSlime = false;
            }
        }
        //down
        if (row > 0)
        {
            if (slimeTiles[column, row - 1])
            {
                slimeTiles[column, row - 1].TakeDamage(1);
                if (slimeTiles[column, row - 1].hitPoints <= 0)
                {
                    slimeTiles[column, row - 1] = null;
                }
                makeSlime = false;
            }
        }
        //up
        if (row < height - 1)
        {
            if (slimeTiles[column, row + 1])
            {
                slimeTiles[column, row + 1].TakeDamage(1);
                if (slimeTiles[column, row + 1].hitPoints <= 0)
                {
                    slimeTiles[column - 1, row + 1] = null;
                }
                makeSlime = false;
            }
        }
    }
    private IEnumerator DecreaseRowCo2()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if cuurent spot isnt blank and is empty
                if (!blankSpaces[i, j] && allDots[i, j] == null && !concreteTiles[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
                {
                    //loop from space above to top of column
                    for (int k = j + 1; k < height; k++)
                    {
                        //if a dot is found
                        if (allDots[i, k] != null)
                        {
                            //move dot to this empty space
                            allDots[i, k].GetComponent<Dots>().row = j;
                            //set that spot to be null
                            allDots[i, k] = null;
                            //break out of loop
                            break;
                        }
                    }
                }

            }

        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(fillBoardCo());
    }
    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dots>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }

        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(fillBoardCo());
    }


    private void refillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        // if match at spot increase iterations
                        maxIterations++;
                        dotToUse = Random.Range(0, dots.Length);
                    }
                    maxIterations = 0;
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dots>().row = j;
                    piece.GetComponent<Dots>().column = i;
                }
            }
        }


    }

    private bool MatchesOnBoard()
    {
        findm.findAllMatches();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dots>().isMatch)
                    {
                        return true;
                    }
                }

            }
        }
        return false;
    }


    private IEnumerator fillBoardCo()
    {
        yield return new WaitForSeconds(refillDelay);
        refillBoard();

        while (MatchesOnBoard())
        {
            streakValue++;
            DestroyMatches();
            yield break;

        }

        currentDot = null;
        CheckToMakeSlime();

        if (IsDeadLocked())
        {
            ShuffleBoard();
        }
        yield return new WaitForSeconds(refillDelay);
        if (currentState != GameState.pause)
        {
            currentState = GameState.move;
        }
        makeSlime = true;
        streakValue = 1;
    }
    private void CheckToMakeSlime()
    {
        //check slime tiles array 
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (slimeTiles[i, j] != null && makeSlime)
                {
                    //call another method to make a new slime
                    MakeNewSlime();
                }
            }
        }
    }
    private Vector2 CheckForAdjacent(int column, int row)
    {
        //if we have dot to the right and column is one less than the width
        if (allDots[column + 1, row] && column < width - 1)
        {
            return Vector2.right;
        }
        if (allDots[column - 1, row] && column > 0)
        {
            return Vector2.left;
        }
        if (allDots[column, row + 1] && row < height - 1)
        {
            return Vector2.up;
        }
        if (allDots[column, row - 1] && row > 0)
        {
            return Vector2.down;
        }
        return Vector2.zero;

    }
    private void MakeNewSlime()
    {
        bool slime = false;
        int loops = 0;
        while (!slime && loops < 200)
        {
            int newX = Random.Range(0, width);
            int newY = Random.Range(0, height);
            //if slime tile exists
            if (slimeTiles[newX, newY])
            {
                //check for any adjacent tile
                Vector2 adjacent = CheckForAdjacent(newX, newY);
                //destroy dot on that adjacent tile
                if (adjacent != Vector2.zero)
                {
                    Destroy(allDots[newX + (int)adjacent.x, newY + (int)adjacent.y]);
                    //where to create new slime tile at 
                    Vector2 tempPosition = new Vector2(newX + (int)adjacent.x, newY + (int)adjacent.y);
                    GameObject tile = Instantiate(slimePrefab, tempPosition, Quaternion.identity);
                    slimeTiles[newX + (int)adjacent.x, newY + (int)adjacent.y] = tile.GetComponent<BackgroundTiles>();
                    slime = true;
                }
            }
            loops++;
        }
    }
    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        if (allDots[column + (int)direction.x, row + (int)direction.y] != null)
        {
            //take second piece and save it in a holder
            GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
            //switching first dot to be the second position
            allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
            //set first dot to be second dot
            allDots[column, row] = holder;
        }

    }
    private bool checkForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    //make sure that one and two to the right are in the
                    //board
                    if (i < width - 2)
                    {
                        //check if the dots to the right and two to the right exist
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].tag == allDots[i, j].tag && allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                    if (j < height - 2)
                    {
                        //check if dots above exist
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            if (allDots[i, j + 1].tag == allDots[i, j].tag && allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }

                }
            }
        }
        return false;
    }
    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (checkForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }
    private bool IsDeadLocked()
    {
        if (hintManager != null)
        {
            hintManager.DestroyHint();
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (i < width - 1)
                    {
                        if (allDots[i + 1, j] != null)
                        {
                            if (SwitchAndCheck(i, j, Vector2.right))
                            {
                                return false;
                            }
                        }
                    }
                    if (j < height - 1)
                    {
                        if (allDots[i, j + 1] != null)
                        {
                            if (SwitchAndCheck(i, j, Vector2.up))
                            {
                                return false;
                            }
                        }
                    }
                    if (allDots[i, j].GetComponent<Dots>().isColorBomb)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    private void ShuffleBoard()
    {
        //create list of game objects 
        List<GameObject> newBoard = new List<GameObject>();
        //add every piece to this list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }
        //for every spot on the board
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if this spot shouldn't be blank
                if (!blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
                {
                    //pick a random number
                    int pieceToUse = Random.Range(0, newBoard.Count);

                    //assign the column to the piece
                    int maxIterations = 0;
                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                    }
                    //make a container for the piece
                    Dots piece = newBoard[pieceToUse].GetComponent<Dots>();
                    maxIterations = 0;
                    //assign column to the piece
                    piece.column = i;
                    //assign the row to the place
                    piece.row = j;
                    //fill in the dots array with this new piece
                    allDots[i, j] = newBoard[pieceToUse];
                    //remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);

                }
            }
        }
        //check if it's still deadlocked
        if (IsDeadLocked())
        {
            ShuffleBoard();
        }
    }
}
