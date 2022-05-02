using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance {get; private set;}
    private void Awake() {
        Instance = this as GridManager;
    }
    private void OnApplicationQuit() {
        Instance = null;
        Destroy(gameObject);
    }

    public int height, width;
    [SerializeField] private Block blockPrefab;
    private Dictionary<Vector2, Block> blocks;
    private List<GameObject> seperators;
    
    [SerializeField] private GameObject seperatorPrefab;
    [SerializeField] private Vector3 seperatorOffset;
    
    [Range(17, 81)]
    [SerializeField] private int knownNumbers;
    private int numberLeft;
    private void Start() {
        blocks = new Dictionary<Vector2, Block>();
        seperators = new List<GameObject>();
        numberLeft = width*height-knownNumbers;
        GenerateGrid();
        GeneratePuzzle();
    }
    private void GenerateGrid() {
        for(int i = 0; i < height; i++) {
            for(int j = 0; j < width; j++) {
                var pos = new Vector3(i, 0, j);
                var spawnBlock = Instantiate(blockPrefab, pos, Quaternion.identity);
                spawnBlock.name = $"Block {i} {j}";
                var blockIdx = new Vector2(i, j);
                spawnBlock.init((i+j)%2 == 1, blockIdx);
                blocks[blockIdx] = spawnBlock;
                
                // Instantiate seperator
                if (i == 0 && j%3 == 0 && j != 0) {
                    var seperator = Instantiate(seperatorPrefab, pos+seperatorOffset, Quaternion.identity);
                    seperator.name = $"seperator {i} {j}";
                    seperators.Add(seperator);
                }
                if (j == 0 && i%3 == 0 && i != 0) {
                    var rotateOffset = new Vector3(seperatorOffset.z, seperatorOffset.y, seperatorOffset.x);
                    var seperator = Instantiate(seperatorPrefab, pos+rotateOffset, Quaternion.identity);
                    seperator.name = $"seperator {i} {j}";
                    var rotationVector = seperator.transform.rotation.eulerAngles;
                    rotationVector.y = 90;
                    seperator.transform.rotation = Quaternion.Euler(rotationVector);
                    seperators.Add(seperator);
                }
            }
        }
    }
    public bool isPuzzleComplete() {
        return numberLeft == 0;
    }
    public Block GetBlockAtPosition(Vector2 pos) {
        if (blocks.TryGetValue(pos, out var block)) {
            return block;
        }
        return null;
    }
    // return false if after the click the puzzle is incomplete
    public bool ClickNumberAtPosition(Vector2 pos, int number) {
        if (blocks.TryGetValue(pos, out var block)) {
            if (!block.ClickNumber(number)) {
                // the number already clicked
                return true;
            }
            var isComplete = true;
            // after effect
            // all col 
            for(int i = 0; i < height; i++) {
                if (blocks.TryGetValue(new Vector2(i, pos.y), out var tmp)) {
                    tmp.SetNumberInactive(number);
                    isComplete &= tmp.IsNumberLeft();
                }
            }
            // all row 
            for(int i = 0; i < width; i++) {
                if (blocks.TryGetValue(new Vector2(pos.x, i), out var tmp)) {
                    tmp.SetNumberInactive(number);
                    isComplete &= tmp.IsNumberLeft();
                }
            }
            // box
            var x = pos.x - pos.x % 3;
            var y = pos.y - pos.y % 3;
            for(int i = 0; i < 3; i++) {
                for(int j = 0; j < 3; j++) {
                    if (blocks.TryGetValue(new Vector2(x+i, y+j), out var tmp)) {
                        tmp.SetNumberInactive(number);
                        isComplete &= tmp.IsNumberLeft();
                    } 
                }
            }
            numberLeft--;
            return isComplete;
        }
        return false;
    }
    
    
    public void GeneratePuzzle() {
        Sudoku sudoku = new Sudoku(height, height*width-knownNumbers);
        sudoku.fillValues();
        var mat = sudoku.getMat();
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if (mat[i,j] != 0) {
                    ClickNumberAtPosition(new Vector2(i, j), mat[i,j]);
                }
            }
        }
    }
    
    public void resetPuzzle() {
        // remove 
        foreach (var block in blocks) {
            Destroy(block.Value.gameObject); 
        }
        foreach (var seperator in seperators) {
            Destroy(seperator); 
        }
        
        GenerateGrid();
        GeneratePuzzle();
    }
    
    public List<int> GetCurrentState() {
        var state = new List<int>();
        foreach (var block in blocks) {
            state.Add(block.Value.GetClickedNumber());
        }
        return state;
    }
    // public void GeneratePuzzle() {
    //     int generateNumber = 0;
    //     var listPos = new List<Vector2>();
    //     for(int i = 0; i < height; i++) {
    //         for(int j = 0; j < width; j++) {
    //             listPos.Add(new Vector2(i, j));
    //         }
    //     }
    //     int loop = 0;
    //     while(generateNumber < knownNumbers) {
    //         loop++;
    //         if (loop >= 5000) break;
    //         // random pos from the list
            
    //         var pos = listPos[Random.Range(0, listPos.Count)];
    //         // try to click a number from this position
    //         if (blocks.TryGetValue(pos, out var block)) {
    //             // no number left for this block
    //             if (!block.isNumberLeft()) {
    //                 // is it even possable?
    //                 Debug.Log("no number left");
    //                 continue;
    //             }
    //             var number = block.randomNumber();
    //             // Debug.Log($"try position {pos.x} {pos.y} number {number}");
                
    //             // check after effect
    //             bool isComplete = true;
    //             // all col 
    //             for(int i = 0; i < height; i++) {
    //                 if (blocks.TryGetValue(new Vector2(i, pos.y), out var tmp)) {
    //                     isComplete &= tmp.isNumberActive(number);
    //                 }
    //             }
    //             // all row 
    //             for(int i = 0; i < width; i++) {
    //                 if (blocks.TryGetValue(new Vector2(pos.x, i), out var tmp)) {
    //                     isComplete &= tmp.isNumberActive(number);
    //                 }
    //             }
    //             // box
    //             var x = pos.x - pos.x % 3;
    //             var y = pos.y - pos.y % 3;
    //             for(int i = 0; i < 3; i++) {
    //                 for(int j = 0; j < 3; j++) {
    //                     if (blocks.TryGetValue(new Vector2(x+i, y+j), out var tmp)) {
    //                         isComplete &= tmp.isNumberActive(number);
    //                     } 
    //                 }
    //             }
    //             // the puzzle is complete
    //             if (isComplete) {
    //                 ClickNumberAtPosition(pos, number);
    //                 generateNumber++;
    //                 listPos.Remove(pos);
    //             }
    //         }
    //     }
    // }
}
