using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class FollowRectTransform : MonoBehaviour
{

    public RectTransform target;
    public bool followPosition = false;
    public bool followSize = true;
    public Vector2 positionOffset = Vector2.zero;

    private Text _textTarget;
    private RectTransform _rectTransformComponent;

    void OnEnable()
    {
        _textTarget = target.GetComponent<Text>();
        _rectTransformComponent = GetComponent<RectTransform>();
    }

    void Update()
    {
        UpdateFollow();
    }

    private void UpdateFollow()
    {
        if (target == null)
        {
            shaco.Log.Warning("FollowRectTransform UpdateFollow warning: missing target, name=" + this.name);
            return;
        }

        if (followPosition)
        {
            _rectTransformComponent.localPosition = target.localPosition + new Vector3(positionOffset.x, positionOffset.y);
        }

        if (null != _textTarget)
        {
            if (followSize)
            {
                _rectTransformComponent.sizeDelta = shaco.UnityHelper.GetTextRealSize(_textTarget);
            }
        }
        else
        {
            if (followSize)
            {
                _rectTransformComponent.sizeDelta = target.sizeDelta;
            }
        }
    }
}
