using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu("IceMark/Button Scale")]
public class ButtonScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Transform tweenTarget;

    public bool showHover = false;
    public Vector3 hover = new Vector3(1.03f, 1.03f, 1.03f);
    public float hoverDuration = 0.2f;

    public bool showPressed = false;
    public Vector3 pressed = new Vector3(0.97f, 0.97f, 0.97f);
    public float pressedDuration = 0.2f;

    private Vector3 _mScale;
    private bool _mStarted;

    private bool _isPressed;

    void Start()
    {
        if (!_mStarted)
        {
            _mStarted = true;
            if (tweenTarget == null) tweenTarget = transform;
            _mScale = tweenTarget.localScale;
        }
    }

    private void OnPress(bool isPressed)
    {
        _isPressed = isPressed;
        if (enabled && showPressed)
        {
            if (!_mStarted) Start();
            Vector3 endValue = isPressed ? Vector3.Scale(_mScale, pressed) : (EventSystem.current.currentSelectedGameObject == gameObject ? Vector3.Scale(_mScale, hover) : _mScale);
            tweenTarget.DOScale(endValue, pressedDuration).SetEase(Ease.InOutQuad);
        }
    }

    private void OnHover(bool isOver)
    {
        if (enabled && showHover && !_isPressed)
        {
            if (!_mStarted) Start();
            Vector3 endValue = isOver ? Vector3.Scale(_mScale, hover) : _mScale;
            tweenTarget.DOScale(endValue, hoverDuration).SetEase(Ease.InOutQuad);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) { OnHover(true); }

    public void OnPointerExit(PointerEventData eventData) { OnHover(false); }

    public void OnPointerDown(PointerEventData eventData) { OnPress(true); }

    public void OnPointerUp(PointerEventData eventData) { OnPress(false); }
}