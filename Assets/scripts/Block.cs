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
    public void init(bool isOffset, Vector2 blockIdx) {
        gameObject.GetComponent<Renderer>().material.color = isOffset ? offsetColor : defaultColor;
        GenerateGrid(isOffset, blockIdx);
        numberLeft = height*width;
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
            setNumberInactive(i);
        }
        _number.text = number.ToString();
        _number.gameObject.SetActive(true);
        background.SetActive(true);
        return true;
    }
    
    public void setNumberInactive(int number) {
        if (innerblocks.TryGetValue(number, out var innerBlock)) {
            if (innerBlock.gameObject.activeSelf) {
                numberLeft--;
            }
            innerBlock.gameObject.SetActive(false);
        }
    }
    public bool isNumberLeft() {
        return numberLeft != 0;
    }
    
    // return an active random number
    public int randomNumber() {
        var listNum = new List<int>();
        foreach(var block in innerblocks) {
            if (block.Value.gameObject.activeSelf) {
                listNum.Add(block.Key);
            }
        }
        return listNum[Random.Range(0, listNum.Count)];
    }
    
    public bool isNumberActive(int number) {
        if (innerblocks.TryGetValue(number, out var innerBlock)) {
            return innerBlock.gameObject.activeSelf;
        }
        return false;
    }
}
