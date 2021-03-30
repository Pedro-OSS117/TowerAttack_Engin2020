using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField]
    private Text _staminaLabel;
    
    [SerializeField]
    private GameObject _originalPopButton;

    private List<PopButton> _listPopButtons;

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

    public void InitializePopButtons(Deck deck, UnityAction<int> onPointDown, UnityAction<int> onPointerUp)
    {
        _originalPopButton.SetActive(false);
        int index = 0;
        _listPopButtons = new List<PopButton>();
        foreach(EntityData data in deck.Entities)
        {
            GameObject newPopButton = Instantiate(_originalPopButton, _originalPopButton.transform.parent);
            newPopButton.SetActive(true);

            Text nameText = newPopButton.GetComponentInChildren<Text>();
            nameText.text = data.Name + "\n" + data.PopAmount;

            PopButton newPopButtonCompo = newPopButton.GetComponent<PopButton>();
            newPopButtonCompo.index = index;

            newPopButtonCompo.OnPointerDownEvent += onPointDown;
            newPopButtonCompo.OnPointerUpEvent += onPointerUp;
            index++;

            _listPopButtons.Add(newPopButtonCompo);
        }
    }

    public void UpdateStatePopButton(float currentStamina, Deck deck)
    {
        for (int i = 0; i < deck.Entities.Count; i++)
        {
            _listPopButtons[i].interactable = currentStamina >= deck.Entities[i].PopAmount; 
        }
    }
}
