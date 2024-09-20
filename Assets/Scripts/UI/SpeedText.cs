using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedText : MonoBehaviour
{
    private TextMeshProUGUI speedText;
    private float timeUpdate;
    private float updateIntervall = 0.1f;

    private void Awake()
    {
        speedText = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        timeUpdate += Time.deltaTime;
        if(timeUpdate > updateIntervall)
        {
            timeUpdate = 0;
            speedText.text = Mathf.Round(Player.Instance.finalSpeed).ToString();
        }
    }
}