using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderHelper : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Text sliderText;
    
    void Start()
    {
        slider.onValueChanged.AddListener((v) => {
            sliderText.text = v.ToString();
            GameManager.Instance.changeKnownNumbers((int)v);
        });
        slider.value = GameManager.Instance.knownNumbers;
    }

}
