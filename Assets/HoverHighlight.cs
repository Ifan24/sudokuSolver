using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    [SerializeField] private GameObject highLight;
    private void OnMouseEnter() {
        highLight.SetActive(true);
    }
    
    private void OnMouseExit() {
        highLight.SetActive(false);
    }
    
    private void OnMouseDown() {
        transform.parent.GetComponent<InnerGrid>().clickNumber(gameObject);
    }
}
