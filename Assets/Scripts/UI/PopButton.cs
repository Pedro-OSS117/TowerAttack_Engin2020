using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// Utilisation classique des delegates
// public delegate void MyEvent(int value);

public class PopButton : Button, IPointerDownHandler, IPointerUpHandler
{
    public int index = 0;

    // Declaration variable delegate
    // public MyEvent OnPointerDownEvent;

    public UnityAction<int> OnPointerDownEvent;
    public UnityAction<int> OnPointerUpEvent;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if(interactable && OnPointerDownEvent != null)
        {
            OnPointerDownEvent.Invoke(index);
        }

    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (interactable && OnPointerUpEvent != null)
        {
            OnPointerUpEvent.Invoke(index);
        }
    }
}
