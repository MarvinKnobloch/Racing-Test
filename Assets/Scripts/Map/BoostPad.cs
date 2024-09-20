using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] private float bonusBoost;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == Player.Instance.gameObject)
        {
            Player.Instance.BonusSpeed += bonusBoost;
        }
    }
}
