using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Dots : MonoBehaviour
{
    [Header("board variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatch = false;


    private EndGameManager endGameManager;
    private HintManager hintManager;
    private FindMatches findm;
    private Board board;
    public GameObject otherDot;
    private Vector2 firstTouchPosition = Vector2.zero;

    private Vector2 finalTouchPosition = Vector2.zero;
    private Vector2 tempPosition;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject adjacentMarker;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;



    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        endGameManager = FindObjectOfType<EndGameManager>();
        hintManager = FindObjectOfType<HintManager>();
        board = GameObject.FindWithTag("Board").GetComponent<Board>();
        findm = FindObjectOfType<FindMatches>();
        Debug.Assert(findm != null, "FindMatches component is not found.");
    }
    //testing and debugging purposes
    private void OnMouseOver()
    {
        if (Input.GetMouseButton(1))
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        targetX = column;
        targetY = row;

        Debug.Log($"Updating Dot Position: ({targetX}, {targetY})");

        if (board != null && board.allDots != null)
        {
            // Check bounds
            if (column < 0 || column >= board.allDots.GetLength(0) || row < 0 || row >= board.allDots.GetLength(1))
            {
                Debug.LogError($"Invalid indices! column: {column}, row: {row}");
                return;
            }

            // Horizontal movement
            if (Mathf.Abs(targetX - transform.position.x) > .1)
            {
                //move towards target
                tempPosition = new Vector2(targetX, transform.position.y);
                transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);

                if (board.allDots[column, row] != this.gameObject)
                {
                    board.allDots[column, row] = this.gameObject;
                    findm.findAllMatches();
                }
            }
            else
            {
                //directly set position
                tempPosition = new Vector2(targetX, transform.position.y);
                transform.position = tempPosition;
            }

            // Vertical movement
            if (Mathf.Abs(targetY - transform.position.y) > .1)
            {
                tempPosition = new Vector2(transform.position.x, targetY);
                transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);

                if (board.allDots[column, row] != this.gameObject)
                {
                    board.allDots[column, row] = this.gameObject;
                    findm.findAllMatches();
                }
            }
            else
            {
                tempPosition = new Vector2(transform.position.x, targetY);
                transform.position = tempPosition;
            }
        }
        else
        {
            Debug.LogError("Board or allDots is not initialized!");
        }
    }


    public IEnumerator CheckMoveCo()
    {
        if (isColorBomb)
        {
            //this piece is the color bomb and other piece is the color to destroy
            findm.MatchPiecesOfColor(otherDot.tag);
            isMatch = true;
        }
        else if (otherDot.GetComponent<Dots>().isColorBomb)
        {
            //other piece is color bomb and this piece has color to destroy
            findm.MatchPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<Dots>().isMatch = true;
        }
        //add check to see if both are color bombs 
        yield return new WaitForSeconds(.5f);
        if (otherDot != null)
        {
            if (!isMatch && !otherDot.GetComponent<Dots>().isMatch)
            {
                // Debug.Log("No matches found. Reverting move.");
                otherDot.GetComponent<Dots>().row = row;
                otherDot.GetComponent<Dots>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;

            }
            else
            {
                if (endGameManager != null)
                {
                    if (endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.DecreaseCounterValue();
                    }
                }
                board.DestroyMatches();

            }

            // otherDot = null;
        }

    }
    private void OnMouseDown()
    {
        // Destroy hint
        if (hintManager != null)
        {
            hintManager.DestroyHint();
        }
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("Mouse Down at: " + firstTouchPosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("Mouse Up at: " + finalTouchPosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            Debug.Log("Swipe Angle: " + swipeAngle);
            MovePieces();
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }
    void MovePiecesActual(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        if (board.lockTiles[column, row] == null && board.lockTiles[column + (int)direction.x, row + (int)direction.y] == null)
        {
            if (otherDot == null)
            {
                board.currentState = GameState.move;
            }

            Debug.Log($"Attempting to move from ({column}, {row}) to ({column + (int)direction.x}, {row + (int)direction.y})");

            otherDot.GetComponent<Dots>().column += -1 * (int)direction.x;
            otherDot.GetComponent<Dots>().row += -1 * (int)direction.y;
            column += (int)direction.x;
            row += (int)direction.y;
            StartCoroutine(CheckMoveCo());
        }
        else
        {
            board.currentState = GameState.move;
        }

    }
    void MovePieces()
    {
        Debug.Log("Current Game State: " + board.currentState);
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            // right swipe
            MovePiecesActual(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            // up swipe
            MovePiecesActual(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            // left swipe
            MovePiecesActual(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            // down swipe
            MovePiecesActual(Vector2.down);
        }
        else
        {
            board.currentState = GameState.move;
        }

    }

    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dots>().isMatch = true;
                    rightDot1.GetComponent<Dots>().isMatch = true;
                    isMatch = true;

                }
            }
        }

        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dots>().isMatch = true;
                    downDot1.GetComponent<Dots>().isMatch = true;
                    isMatch = true;

                }
            }
        }
    }

    public void MakeRowBomb()
    {
        if (!isColumnBomb && !isColorBomb && !isAdjacentBomb)
        {
            isRowBomb = true;
            GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }

    }
    public void MakeColumnBomb()
    {
        if (!isRowBomb && !isColorBomb && !isAdjacentBomb)
        {
            isColumnBomb = true;
            GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }
    public void MakeColorBomb()
    {
        if (!isRowBomb && !isColumnBomb && !isAdjacentBomb)
        {
            isColorBomb = true;
            GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
            color.transform.parent = this.transform;
            this.gameObject.tag = "Color";
        }
    }

    public void AdjacentBomb()
    {
        if (!isRowBomb && !isColumnBomb && !isColorBomb)
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }

    }

}
