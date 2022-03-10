using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public Level myLevel;
    public PlayerController player;
    [SerializeField]
    PlayerMovement playerPrefab;
    public List<PlayerActions> playerActions;
    public List<Block> currentTiles;
    public float timeBetweenTileReveal;
    public float timeBetweenTileFall;
    public float timeBetweenActions;
    public bool shouldChangeModel;

    public Coroutine playerSpawnCoroutine;
    public Coroutine restartCoroutine;

    public Dictionary<int, List<char>> levelSolutions;
    public Dictionary<int, List<char>> levelMoves;

    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        myLevel = GetComponent<Level>();
        levelLoaderJSON = GetComponent<LevelLoaderJSON>();
    }

    public void Start()
    {
        if (playRoutineOnStart)
        {
            StartCoroutine(StartLevel());
        }
    }

    public void Update()
    {
        if (Input.GetButtonDown(KeyCode.Space.ToString()))
        {
            StartSpawnPlayer((myLevel.spawnX,myLevel.spawnY));
        }
    }

    public void AddLevelSolutions(List<char> solution)
    {
        if(levelSolutions == null)
        {
            levelSolutions = new Dictionary<int, List<char>>();
        }

        if (levelSolutions.ContainsKey(myLevel.levelIndex))
        {
            levelSolutions[myLevel.levelIndex] = solution;
            AddLevelMoves(solution);
        }
        else
        {
            levelSolutions.Add(myLevel.levelIndex, solution);
            AddLevelMoves(solution);
        }
    }

    public void AddLevelMoves(List<char> moves)
    {
        if (levelMoves == null)
        {
            levelMoves = new Dictionary<int, List<char>>();
        }

        if (levelMoves.ContainsKey(myLevel.levelIndex))
        {
            levelMoves[myLevel.levelIndex].AddRange(moves);
        }
        else
        {
            levelMoves.Add(myLevel.levelIndex, moves);
        }
    }

    public Coroutine StartSpawnPlayer((int x, int z) pos)
    {
        if(playerSpawnCoroutine != null)
        {
            return playerSpawnCoroutine;
        }

        return playerSpawnCoroutine = StartCoroutine(SpawnPlayer(pos));
    }

    public IEnumerator SpawnPlayer((int x, int z) pos)
    {
        yield return playerPrefab.Spawn(pos.x, pos.z);
        playerSpawnCoroutine = null;
        PlayerController player = FindObjectOfType<PlayerController>();
        player.OnPlayerDie += StartRestartLevelCoroutine;
    }

    public bool playRoutineOnStart;
    public IEnumerator StartLevel()
    {
        yield return StartCoroutine(myLevel.StartLevel());

        StartCoroutine(PlayPlayerAction());
    }


    public IEnumerator PlayPlayerAction()
    {
        List<PlayerActions> temp = new List<PlayerActions>();
        temp.AddRange(playerActions);
        int index = 0;
        while(index < temp.Count)
        {     
            if (player.isMoving() && !player.myPlayerMovement.CanMove())
            {
                yield return null;
            }
            else
            {
                PlayerActions action = temp[index];
                player.ProcessAction(action);
                Debug.Log("Process : " + action);
                index++;
                yield return new WaitForSeconds(0.3f);
                if(action == PlayerActions.Validate)
                {
                    StartCoroutine(FinishLevel());
                    break;
                }
                if(shouldChangeModel){
                    player.modelHolder.NextModel();
                }
                yield return new WaitForSeconds(timeBetweenActions);
            }
        }
    }

    public Block GetTileAt(Vector2 pos)
    {
        return myLevel.GetTileAt(pos);
    }

    public Block GetTileFromIndex(int index)
    {
        return myLevel.GetTileFromIndex(index);
    }

    public void StartRestartLevelCoroutine()
    {
        if(restartCoroutine != null)
        {
            return;
        }

       restartCoroutine =  StartCoroutine(RestartLevelCoroutine());
    }

    public IEnumerator RestartLevelCoroutine()
    {
        yield return new WaitForSeconds(0.6f);
        yield return StartCoroutine(TilesFall());
        yield return new WaitForSeconds(0.6f);
        yield return myLevel.StartLevelCoroutine();
        restartCoroutine = null;
    }

    public IEnumerator TilesFall()
    {
        List<Block> temp = new List<Block>();
        temp.AddRange(FindObjectsOfType<Block>());
        int index = 0;
       // Debug.Log("numbers of tiles : " + temp.Count);
        int count = temp.Count;
        for (int i = 0; i < count; i++)
        {
            index = Random.Range(0, temp.Count);
            if (temp[index] != null)
            {
                temp[index].Fall();
            }
            temp.RemoveAt(index);
            yield return new WaitForSeconds(timeBetweenTileFall);
        }
    }

    public IEnumerator FinishLevel()
    {
        List<Block> temp = new List<Block>();
        temp.AddRange(currentTiles);
        int index = 0;
        // Debug.Log("numbers of tiles : " + temp.Count);
        int count = 0;
        for (int i = 0; i < currentTiles.Count; i++)
        {
            index = Random.Range(0, temp.Count);
            temp[index].Hide(false);
            temp.RemoveAt(index);
            yield return new WaitForSeconds(timeBetweenTileReveal);
            count++;
        }
       // Debug.Log("numbers of iterations : " + count);
    }

    public bool IsPlayerOnTeleporter(Vector2 pos1, Vector2 pos2)
    {
        return myLevel.IsPlayerOnTeleporter(pos1, pos2);
    }

    public Vector2 GetOtherTeleporter(TeleporterBlock myTeleporter)
    {
        return myLevel.GetOtherTeleporter(myTeleporter);
    }

    public bool IsPlayerPosOutOfBound(Vector2 pos1)
    {
        return myLevel.IsPlayerPosOutOfBound(pos1);
    }

    public bool IsPlayerOnSoftSwitch(Vector2 pos1, Vector2 pos2)
    {
        return myLevel.IsPlayerOnSoftSwitch(pos1, pos2);
    }

    public bool IsPlayerOnHardSwitch(Vector2 pos1, Vector2 pos2)
    {
        return myLevel.IsPlayerOnHardSwitch(pos1, pos2);
    }

    public bool HasPlayerWin(Vector2 pos1, Vector2 pos2)
    {
        return myLevel.HasPlayerWin(pos1, pos2);
    }

    public bool IsOnUnstableTile(Vector2 pos1, Vector2 pos2)
    {
        return myLevel.IsOnUnstableTile(pos1, pos2);
    }
}
