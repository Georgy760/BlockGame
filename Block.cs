using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Block : MonoBehaviour
{
    int value=1;
    [SerializeField]
    protected int index;
    public BlockType type;

    public Vector3 normalScale;
    public float showTweenDuration;
    public float showFallTweenDuration;
    public float hideTweenDuration;
    public float fallTweenDuration;
    public float shakefallTweenDuration;
    public float shakefallTweenForce;

    public Sequence colorSwitchSequence;
    public bool isPlayerOnTile = false;
    public Color activatedColor;
    public Color idleColor;
    public float loseColor;
    public float gainColor;

    public virtual void Awake()
    {
        isPlayerOnTile = false;
    }

    public Tween Hide(bool skipAnim = true)
    {
        if (skipAnim)
        {
            transform.localScale = Vector3.zero;
        }
        else
        {
           return transform.DOScale(Vector3.zero, hideTweenDuration);
        }

        return null;
    }

    public void Fall()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOPunchRotation(shakefallTweenForce * Vector3.one,shakefallTweenDuration));
        mySequence.Append(transform.DOMoveY(-10, fallTweenDuration));
        mySequence.OnComplete(DestroyBlock);
    }

    public void DestroyBlock()
    {
        Destroy(this.gameObject);
    }

    public void SetIsPlayerOn(bool value)
    {
        isPlayerOnTile = value;
        ToggleColor();
    }

    public void ToggleColor()
    {
        if(colorSwitchSequence != null && colorSwitchSequence.IsPlaying())
        {
            colorSwitchSequence.Kill();
        }

        Sequence mySequence = DOTween.Sequence();
        // Debug.Log("gameobject : " + gameObject);
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if(mesh.materials.Length > 1)
        {
            Material mat = mesh.materials[1];
            if (mat != null)
            {
                if (isPlayerOnTile)
                {
                    mySequence.Append(mat.DOColor(activatedColor, gainColor));
                }
                else
                {
                    mySequence.Append(mat.DOColor(activatedColor, 0));
                    mySequence.Append(mat.DOColor(idleColor, loseColor));
                }
            }
        }
        colorSwitchSequence = mySequence;
    }

    public Tween Show(bool pop = false)
    {
        if(pop)
        {
           return transform.DOScale(normalScale, showTweenDuration).SetEase(Ease.OutBack);
        }
        else
        {
            float y = transform.position.y;
            transform.position = transform.position + new Vector3(0,8,0);
            transform.localScale = normalScale;
           return transform.DOMoveY(y, showFallTweenDuration);
        }
    }

    

    public void AnimateHeight(float delay,float targetY,float duration,Ease ease= Ease.InOutSine){
        StartCoroutine(AnimateHeightRoutine(delay,targetY,duration,ease));
    }

    IEnumerator AnimateHeightRoutine(float delay, float targetY,float duration,Ease ease){
        yield return new WaitForSeconds(delay);
        transform.DOMoveY(targetY,duration).SetEase(ease);
    }

    public int GetIndex()
    {
        return index;
    }

    public void SetIndex(int value)
    {
        if(value < 0)
        {
            return;
        }

        index = value;
    }
}


public enum BlockType
{
    Empty,
    Normal,
    Teleporter,
    Bridge,
    PressurePlate,
    Unstable,
    SoftButton,
    HardButton,
    Goal
}

