using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField]
    private Text _staminaLabel;


    [SerializeField]
    private GameObject _originalPopButton;

    public void UpdateStaminaLabel(int amountStamina)
    {
        if(_staminaLabel)
        {
            _staminaLabel.text = "X " + amountStamina;
        }
        else
        {
            Debug.LogError($"No {nameof(_staminaLabel)}");
        }
    }

    public void InitializePopButtons(Deck deck)
    {
        foreach(EntityData data in deck.Entities)
        {
            GameObject newPopButton = Instantiate(_originalPopButton, _originalPopButton.transform.parent);
            newPopButton.SetActive(true);
            Text nameText = newPopButton.GetComponentInChildren<Text>();
            nameText.text = data.Name;
        }
    }
}
