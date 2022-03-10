using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public PlayerMovement myPlayerMovement;
    public ModelHolder modelHolder;

    public float validateTweenDuration;
    public float hideTweenDuration;
    public float fallTweenDuration;


    public float delayBetweenTpTween;
    public float tpDespawnDurationTween;
    public float tpRespawnDurationTween;
  
    public Ease tpRespawnEase;

    public delegate void PlayerAction();
    public event PlayerAction OnPlayerDie;

    private Vector3 newPos;

    public bool alive;

    [SerializeField] private List<char> moves;
    [SerializeField] private TMP_Text _score;

    public void Start()
    {
        alive = true;
    }

    public void Reset()
    {
        Show();
        transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        moves = new List<char>();
        if(_score != null) _score.text = "Score: " + moves.Count;
    }

    public void AddMove(char move)
    {
        if(!(move == 'u' || move == 'd' || move == 'l' || move == 'r'))
        {
            return;
        }

        moves.Add(move);
        if(_score != null) _score.text = "Score: " + moves.Count;
        
    }

    [ContextMenu("Validate")]
    public void ValidateLevel()
    {
        transform.DOMoveY(-12, validateTweenDuration).SetEase(Ease.InBack).OnComplete(Hide);
        LevelManager.Instance.AddLevelSolutions(moves);
    }

    public void Hide()
    {
        transform.DOScale(Vector3.zero, hideTweenDuration);
    }

    public void Show(bool pop = false)
    {
        if (pop)
        {
            transform.DOScale(Vector3.one, hideTweenDuration);
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    public void Kill()
    {
        alive = false;
        LevelManager.Instance.AddLevelMoves(moves);

        if (OnPlayerDie != null)
        {
            OnPlayerDie.Invoke();
        }
    }

    public void TeleportTo(Vector2 newPos)
    {
        myPlayerMovement.SetCanMove(false);
        this.newPos = new Vector3(newPos.x, transform.position.y, newPos.y);

        Sequence mySequence = DOTween.Sequence();

        mySequence.PrependInterval(0.15f);
        Block block = LevelManager.Instance.GetTileAt(myPlayerMovement.GetPos1());
        if(block is TeleporterBlock)
        {
            ((TeleporterBlock)block).Teleport();
            myPlayerMovement.SetPos(newPos, newPos);
        }
        else
        {
            return;
        }
        
        mySequence.Insert(0, transform.DOScaleZ(0f, tpDespawnDurationTween));
        mySequence.Insert(0, transform.DOScaleX(0f,tpDespawnDurationTween));
        mySequence.OnComplete(TeleportToBis);
    }

    public void TeleportToBis()
    {
        transform.position = newPos;
        Sequence mySequence = DOTween.Sequence();
        Block block = LevelManager.Instance.GetTileAt(myPlayerMovement.GetPos1());
        if (block is TeleporterBlock)
        {
            ((TeleporterBlock)block).ReceiveTeleport();
        }
        mySequence.PrependInterval(delayBetweenTpTween);
        mySequence.OnComplete(TeleportToBisBis);
        
    }

    public void TeleportToBisBis()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Insert(0, transform.DOScaleX(1, tpRespawnDurationTween).SetEase(tpRespawnEase));
        mySequence.Insert(0, transform.DOScaleZ(1, tpRespawnDurationTween).SetEase(tpRespawnEase));
        mySequence.OnComplete(ActivateMovement);
    }

    public void ActivateMovement()
    {
        myPlayerMovement.SetCanMove(true);
    }

    public void Fall()
    {
        Sequence mySequence = DOTween.Sequence();

        mySequence.PrependInterval(0.15f);
        mySequence.Append(transform.DOScale(Vector3.zero, fallTweenDuration));
    }

    public bool isMoving()
    {
        return myPlayerMovement.isRotate;
    }

    public void ProcessAction(PlayerActions action)
    {
        float x = 0;
        float y = 0;
        switch (action)
        {
            case PlayerActions.Up:
                x = 1;
                y = 0;
                break;
            case PlayerActions.Down:
                x = -1;
                y = 0;
                break;
            case PlayerActions.Left:
                x = 0;
                y = 1;
                break;
            case PlayerActions.Right:
                x = 0;
                y = -1;
                break;
            case PlayerActions.Validate:
                ValidateLevel();
                break;

            default:
                break;
        }

        if(x != 0 || y!= 0)
        {
            myPlayerMovement.ProcessInput(x, y);
        }
    }
}

public enum PlayerActions { Up, Down, Left, Right, Validate}
