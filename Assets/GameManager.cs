using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    private void Awake() {
        Instance = this as GameManager;
        if (currBoard != null) {
            currBoard.init();
        }
    }
    private void OnApplicationQuit() {
        Instance = null;
        Destroy(gameObject);
    }
    
    [SerializeField] private GameObject boardPrefab;
    [SerializeField] private BoardManager currBoard;
    [Range(17, 81)]
    public int knownNumbers;
    public Text numberLeftText;
    [SerializeField] private GameObject agent;
    
    public void resetPuzzle() {
        // currBoard.init();
        agent.GetComponent<SudokuAgent>().OnEpisodeBegin();
    }
    
    public void solvePuzzle() {
        // currBoard.SolvePuzzle();
        // agent.GetComponent<SudokuAgent>().EndEpisode();
    }
    public void changeKnownNumbers(int number) {
        knownNumbers = number;
    }
    
    // public List<int> GetCurrentState() {
    //     return currBoard.GetCurrentState();
    // }
    
    public int ClickNumberAtPosition(Vector2 pos, int number) {
        return currBoard.ClickNumberAtPosition(pos, number);
    }
    
    // no more number to click
    public bool isPuzzleComplete() {
        return currBoard.isPuzzleComplete();
    }
}
