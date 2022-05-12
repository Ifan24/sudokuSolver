using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InnerGrid : MonoBehaviour
{

    List<GameObject> children;
    public bool isClicked;
    Vector2 pos;
    int _number;
    int activeLeft;
    public void init()
    {
        _number = 0;
        activeLeft = 9;
        isClicked = false;
        children = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++) {
            children.Add(transform.GetChild(i).gameObject);
        }
        for(int i = 0; i < 9; i++) {
            children[i].SetActive(true);
        }
        for(int i = 9; i < 9*2; i++) {
            children[i].SetActive(false);
        }
    }
    
    public void setupPuzzle(int number) {
        // only allow click once
        if (isClicked) return;
        isClicked = true;
        _number = number;
        activeLeft = 0;
        // hide first 9 children
        for (int i = 0; i < 9; i++) {
            children[i].SetActive(false);
        }
        // show the clicked child
        var idx = number-1;
        children[9+idx].SetActive(true);
    }
    public void clickNumber(GameObject child) {
        var idx = children.IndexOf(child);
        // setupPuzzle(idx+1);
        
        // notify parent
        // Debug.Log($"clicked at {pos.x} {pos.y} with number {idx+1}");
        transform.GetComponentInParent<BoardManager>().ClickNumberAtPosition(pos, idx+1);
    }
    
    public void assignPos(Vector2 pos) {
        this.pos = pos;
    } 
    
    // set the option inactive
    public void SetNumberInactive(int number) {
        if (children[number-1].activeSelf) {
            children[number-1].SetActive(false);
            activeLeft--;
        }
    }
    
    public bool IsNumberLeft() {
        return activeLeft > 0;
    }
    
    public int getNumber() {
        return _number;
    }
    
    public bool isNumberClickable(int number) {
        return children[number-1].activeSelf;
    }
}
