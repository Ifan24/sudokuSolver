using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{

    [SerializeField] private int height, width;
    [SerializeField] private Color defaultColor, offsetColor;
    [SerializeField] private innerBlock innerBlockPrefab;
    [SerializeField] private TMP_Text _number;
    [SerializeField] private GameObject background;
    private Dictionary<int, innerBlock> innerblocks;
    private int numberLeft;
    private int clickedNumber;
    private Vector2 _blockIdx;
    public void init(bool isOffset, Vector2 blockIdx) {
        gameObject.GetComponent<Renderer>().material.color = isOffset ? offsetColor : defaultColor;
        GenerateGrid(isOffset, blockIdx);
        _blockIdx = blockIdx;
        numberLeft = height*width;
        clickedNumber = 0;
    }
    
    private void GenerateGrid(bool isOffset, Vector2 blockIdx) {
        innerblocks = new Dictionary<int, innerBlock>();
        bool flip = isOffset;
        for(int i = 0; i < height; i++) {
            for(int j = 0; j < width; j++) {
                var spawnBlock = Instantiate(innerBlockPrefab, new Vector3(i*0.3f, 0, j*0.3f), Quaternion.identity);
                spawnBlock.name = $"innerBlock {i} {j}";
                var number = i*height+j+1;
                spawnBlock.init(flip, offsetColor, defaultColor, number, blockIdx);
                spawnBlock.transform.SetParent(gameObject.transform, false);
                innerblocks[number] = spawnBlock;
                flip = !flip;
            }
        }
    }
    
    public bool ClickNumber(int number) {
        // the number already inactive
        if (innerblocks.TryGetValue(number, out var innerBlock)) {
            if (!innerBlock.gameObject.activeSelf) {
                // no effect
                return false;
            }
        }
    
        for(int i = 1; i <= 9; i++) {
            SetNumberInactive(i);
        }
        _number.text = number.ToString();
        _number.gameObject.SetActive(true);
        background.SetActive(true);
        clickedNumber = number;
        return true;
    }
    
    public void SetNumberInactive(int number) {
        if (innerblocks.TryGetValue(number, out var innerBlock)) {
            if (innerBlock.gameObject.activeSelf) {
                numberLeft--;
                if (numberLeft == 0) clickedNumber = 10;
            }
            innerBlock.gameObject.SetActive(false);
        }
    }
    public bool IsNumberLeft() {
        return numberLeft != 0;
    }
    
    // return an active random number
    public int RandomNumber() {
        var listNum = new List<int>();
        foreach(var block in innerblocks) {
            if (block.Value.gameObject.activeSelf) {
                listNum.Add(block.Key);
            }
        }
        return listNum[Random.Range(0, listNum.Count)];
    }
    
    public bool IsNumberActive(int number) {
        if (innerblocks.TryGetValue(number, out var innerBlock)) {
            return innerBlock.gameObject.activeSelf;
        }
        return false;
    }
    
    public int GetClickedNumber() {
        return clickedNumber;
    }
    
    public Vector2 GetBlockPos() {
        return _blockIdx;
    }
}
