using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickComponent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
    private Vector2 stickPosition;
    public Vector2 inputValue => stickPosition;

    private RectTransform panel;
    private RectTransform stick;

    private void Awake() {
        panel = GameObject.FindWithTag("joystick").GetComponent<RectTransform>();
        stick = transform.GetChild(0).GetComponent<RectTransform>();
        stickPosition = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData) {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData) {
        stick.anchoredPosition = stickPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData) {
        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                panel,
                eventData.position,
                eventData.pressEventCamera,
                out stickPosition)
           ) {
            stickPosition = new Vector2(stickPosition.x / panel.rect.width * 2, 0);

            if (stickPosition.magnitude > 1.0) {
                stickPosition.Normalize();
            }

            stick.anchoredPosition =
                new Vector2(stickPosition.x * panel.rect.width / 2, 0);
        }
    }
}