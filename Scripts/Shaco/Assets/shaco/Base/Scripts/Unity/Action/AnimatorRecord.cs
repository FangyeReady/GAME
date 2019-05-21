using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Animator))]
public class AnimatorRecord : MonoBehaviour {

    public delegate void CALL_FUNC_0();

    [HideInInspector]
    public Animator AnimatorTarget;
    private string _strNextState = string.Empty;
    private string _strCurrentState = string.Empty;
    private int _iLayerIndex = 0;
    private bool _isAutoPlay = false;
    private string _strAnimationName = string.Empty;
    private CALL_FUNC_0 _callFuncAnimation = null;

    private float _delayPlayTime = 0.03f;
	void Start () {
        AnimatorTarget = GetComponent<Animator>();
	}

    void Update()
    {
        if (_callFuncAnimation != null)
        {
            var ss = AnimatorTarget.GetCurrentAnimatorStateInfo(0);
            if (ss.normalizedTime >= 1)
            {
                bool isCall = false;
                if (_strAnimationName.Length > 0)
                {
                    if (ss.IsName(_strAnimationName))
                        isCall = true;
                }
                else
                    isCall = true;
                if (isCall)
                {
                    _callFuncAnimation();
                    _callFuncAnimation = null;
                }
            }
        }

        if (_strNextState.Length > 0)
        {
            var info = AnimatorTarget.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(_strNextState))
            {
                //Here must be some seconds delay, otherwise there will be a model of flicker bug
                //For reasons still unknown...
                Invoke("BeginRecord", _delayPlayTime);
            }
        }
    }

    void BeginRecord()
    {
        if (_strNextState == string.Empty)
            return;

        if (!_isAutoPlay)
        {
            AnimatorTarget.StartRecording(2);
            AnimatorTarget.Update(0);
            AnimatorTarget.StopRecording();
            AnimatorTarget.StartPlayback();
        }
        _strNextState = string.Empty;
    }

    public void setPlayPercent(float percent)
    {
        if (AnimatorTarget == null)
            return;
        if (_strNextState.Length > 0)
        {
            //if action running, this function don't work
            return;
        }

        if (percent < 0)
            percent = 0;
        if (percent >= 1)
            percent = GetPlayPercent();

        var info = AnimatorTarget.GetCurrentAnimatorStateInfo(_iLayerIndex);
        AnimatorTarget.playbackTime = percent * info.length;
        AnimatorTarget.Update(0);
    }

    public float GetPlayPercent()
    {
        var info = AnimatorTarget.GetCurrentAnimatorStateInfo(_iLayerIndex);
        return AnimatorTarget.playbackTime / info.length;
    }
  
    public bool RecordFrame(string nextState= "", int layerIndex = 0, bool isAutoPlay = false)
    {
        if (isName(nextState) || string.IsNullOrEmpty(nextState) || AnimatorTarget == null)
            return false;

        _strCurrentState = nextState;
        _strNextState = nextState;
        _iLayerIndex = layerIndex;
        _isAutoPlay = isAutoPlay;
        
        AnimatorTarget.StopPlayback();
        

        AnimatorTarget.Play(_strNextState);
        AnimatorTarget.Update(0);
        if (isAutoPlay)
        {
            _strNextState = string.Empty;
        }
        return true;
    }

    public bool isName(string actionName)
    {
        if (AnimatorTarget == null)
            return false;

        return _strCurrentState == actionName;
    }

    public void setAnimationCallBack(string animationName, CALL_FUNC_0 callfunc)
    {
        _strAnimationName = animationName;
        _callFuncAnimation = callfunc;
    }
}
