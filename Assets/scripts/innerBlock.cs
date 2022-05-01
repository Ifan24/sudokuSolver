using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class innerBlock : MonoBehaviour
{
    [SerializeField] private GameObject _highlight;
    [SerializeField] private TMP_Text _numberText;
    private int _number;
    private Vector2 _blockIdx;
    
    public void init(bool isOffset, Color offsetColor, Color defaultColor, int number, Vector2 blockIdx) {
        gameObject.GetComponent<Renderer>().material.color = isOffset ? offsetColor : defaultColor;
        _numberText.text = number.ToString();
        _blockIdx = blockIdx;
        _number = number;
    }
    
    private void OnMouseEnter() {
        _highlight.SetActive(true);
    }
    private void OnMouseDown() {
        // Debug.Log($"click on {_blockIdx.x} {_blockIdx.y} number {_numberText.text}");
        GridManager.Instance.ClickNumberAtPosition(_blockIdx, _number);
    }

    private void OnMouseExit() {
        _highlight.SetActive(false);
    }
}
