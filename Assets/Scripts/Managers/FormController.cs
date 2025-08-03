using System;
using UnityEngine;

public class FormController : MonoBehaviour
{
    [SerializeField] private GameObject humanForm;
    [SerializeField] private GameObject spiritForm;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Spirit"))GameManager.instance.SwitchToUndead();
    }
}
