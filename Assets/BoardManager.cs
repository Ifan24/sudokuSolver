using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    Dictionary<Vector2, InnerGrid> innerGrids;
    [HideInInspector] public int numberLeft;
    [HideInInspector] public Dictionary<Vector2, int> answers;
    [SerializeField] private float solveSpeed;
    private bool isSolved;
    private GameManager gameManager;
    [HideInInspector] public int knownNumbers;
    public void init() {
        gameManager = GameManager.Instance;
        knownNumbers = Random.Range(17, 81);
        numberLeft = 9*9;
        updateNumberLeft();
        
        isSolved = false;
        innerGrids = new Dictionary<Vector2, InnerGrid>();
        // add the first 9 blocks
        for (int i = 0; i < 9*9; i++) {
            var innerGrid = transform.GetChild(i).GetComponent<InnerGrid>();
            var pos = new Vector2(i/9, i%9);
            innerGrid.init();
            innerGrid.assignPos(pos);
            innerGrids.Add(pos, innerGrid);
        }
        GeneratePuzzle();
    }
    // void Start() {
    //     gameManager = GameManager.Instance;
    //     numberLeft = 9*9;
    //     updateNumberLeft();
        
    //     isSolved = false;
    //     innerGrids = new Dictionary<Vector2, InnerGrid>();
    //     // add the first 9 blocks
    //     for (int i = 0; i < 9*9; i++) {
    //         var innerGrid = transform.GetChild(i).GetComponent<InnerGrid>();
    //         var pos = new Vector2(i/9, i%9);
    //         innerGrid.init();
    //         innerGrid.assignPos(pos);
    //         innerGrids.Add(pos, innerGrid);
    //     }
    //     GeneratePuzzle();
    // }
    
    // 0 - no effect
    // 1 - click a number and it leads to a complete solution
    // 2 - click a number and it leads to an incomplete solution
    public int ClickNumberAtPosition(Vector2 pos, int number) {
        if (innerGrids.TryGetValue(pos, out var innerGrid)) {
            if (innerGrid.isClicked || !innerGrid.isNumberClickable(number)) {
                // no effect
                return 0;
            }
            var isComplete = true;
            innerGrid.setupPuzzle(number);
            // backprop
            // all col 
            for(int i = 0; i < 9; i++) {
                if (innerGrids.TryGetValue(new Vector2(i, pos.y), out var tmp)) {
                    if (!tmp.isClicked) {
                        tmp.SetNumberInactive(number);
                        isComplete &= tmp.IsNumberLeft();
                    }
                }
            }
            // all row 
            for(int i = 0; i < 9; i++) {
                if (innerGrids.TryGetValue(new Vector2(pos.x, i), out var tmp)) {
                    if (!tmp.isClicked) {
                        tmp.SetNumberInactive(number);
                        isComplete &= tmp.IsNumberLeft();
                    }
                }
            }
            // box
            var x = pos.x - pos.x % 3;
            var y = pos.y - pos.y % 3;
            for(int i = 0; i < 3; i++) {
                for(int j = 0; j < 3; j++) {
                    if (innerGrids.TryGetValue(new Vector2(x+i, y+j), out var tmp)) {
                        if (!tmp.isClicked) {
                            tmp.SetNumberInactive(number);
                            isComplete &= tmp.IsNumberLeft();
                        }
                    } 
                }
            }
            numberLeft--;
            updateNumberLeft();
            // if (!isComplete) {
            //     Debug.Log("isComplete");
            // }
            return isComplete ? 1 : 2;
        }
        return 0;
    }
    
    void updateNumberLeft() {
        // only one can update the UI
        if (gameObject.name == "Board") {
            gameManager.numberLeftText.text = numberLeft.ToString();
        }
    }
    public void GeneratePuzzle() {
        Sudoku sudoku = new Sudoku(9, numberLeft-knownNumbers);
        sudoku.fillValues();
        answers = new Dictionary<Vector2, int>();
        foreach (var ans in sudoku.getAnswer()) {
            var pos = new Vector2(ans.Key.Item1, ans.Key.Item2);
            answers.Add(pos, ans.Value);
        }
        var mat = sudoku.getMat();
        for (int i = 0; i < 9; i++) {
            for (int j = 0; j < 9; j++) {
                if (mat[i,j] != 0) {
                    ClickNumberAtPosition(new Vector2(i, j), mat[i,j]);
                }
            }
        }
    }
    
    public void SolvePuzzle() {
        if (isSolved) return;
        isSolved = true;
        StartCoroutine(WaitAndSolve(solveSpeed));
    }
    
    private IEnumerator WaitAndSolve(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            
            if(answers.Count == 0) break;
            
            // pick a random element
            var ans = answers.ElementAt(Random.Range(0, answers.Count));
            ClickNumberAtPosition(ans.Key, ans.Value);
            answers.Remove(ans.Key);
        }
    }
    
    public List<List<int>> GetCurrentState() {
        var states = new List<List<int>>();
        for(int i = 0; i < 9; i++) {
            for(int j = 0; j < 9; j++) {
                if (innerGrids.TryGetValue(new Vector2(i, j), out var tmp)) {
                    var state = new List<int>();
                    state.Add(tmp.isClicked ? 1 : 0);
                    state.Add(tmp.IsNumberLeft() ? 1 : 0);
                    for(int k = 1; k <= 9; k++) {
                        state.Add(tmp.isNumberClickable(k) ? 1 : 0);
                    }
                    state.Add(tmp.getNumber());
                    states.Add(state);
                } 
            }
        }
        return states;
    }
    
    public bool isPuzzleComplete() {
        if (numberLeft == 0) return true;
        for(int i = 0; i < 9; i++) {
            for(int j = 0; j < 9; j++) {
                if (innerGrids.TryGetValue(new Vector2(i, j), out var tmp)) {
                    // empty
                    if (!tmp.isClicked && !tmp.IsNumberLeft()) continue;
                    // 
                    if (!tmp.isClicked) return false;
                } 
            }
        }
        return true;
    }
}
