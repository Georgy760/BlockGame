using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public LevelLoaderJSON levelLoaderJSON;

    [SerializeField]
    public int levelIndex;
    public float timeBetweenBlockSpawn;
  
    [SerializeField]
    public int spawnX,spawnY;
    [SerializeField]
    Block levelBlockPrefab;
    [SerializeField]
    IntBlockDictionary levelBlockPrefabs;

    public void Awake()
    {
        levelLoaderJSON = GetComponent<LevelLoaderJSON>();
    }

    [ContextMenu("LoadThisLevel")]
    public List<Block> LoadThisLevel()
    {
        return levelLoaderJSON.LoadThisLvL();
    }

    public Block GetTileAt(Vector2 pos, bool skipVerification = false)
    {
        if (!skipVerification && IsPlayerPosOutOfBound(pos))
        {
            return null;
        }

        return levelLoaderJSON.blocksPos[pos];
    }

    public Block GetTileFromIndex(int index)
    {
        return levelLoaderJSON.blocksIndex[index];
    }

    public BlockType GetTileType(Vector2 pos)
    {
        if (IsPlayerPosOutOfBound(pos))
        {
            return 0;
        }

        return levelLoaderJSON.blocksPos[pos].type;
    }

    IEnumerator RevealLevel(){
        for (int i = 0; i < levelLoaderJSON.blockList.Count; i++)
        {
            if(levelLoaderJSON.blockList[i] != null){
                if(levelLoaderJSON.blockList[i].type == BlockType.Bridge)
                {
                    BridgeBlock bridge = (BridgeBlock)levelLoaderJSON.blockList[i];
                    if (bridge.isFirstTileOfBridge)
                    {
                        Vector3 pos = levelLoaderJSON.blocksIndex[bridge.bridgeAnchorTileId].transform.position;
                        bridge.transform.position = new Vector3(pos.x, -0.25f, pos.z);
                        bridge.Show(true);
                    }
                }
                else
                {
                     levelLoaderJSON.blockList[i].Show();
                }
                yield return new WaitForSeconds(timeBetweenBlockSpawn);
            }
        }
    }

    public Coroutine StartLevelCoroutine()
    {
        return StartCoroutine(StartLevel());
    }

    public IEnumerator StartLevel(){
        LoadThisLevel();
        yield return StartCoroutine(RevealLevel());
        yield return LevelManager.Instance.StartSpawnPlayer((spawnX,spawnY));
    }

    public bool IsPlayerPosOutOfBound(Vector2 pos)
    {
        bool outOfBound = !levelLoaderJSON.blocksPos.ContainsKey(pos);
        if (!outOfBound)
        {
            Block block = GetTileAt(pos, true);
            if(block is BridgeBlock)
            {
                return !((BridgeBlock)block).GetIsOpen();
            }

        }
        return outOfBound;
    }

    public bool HasPlayerWin(Vector2 pos1, Vector2 pos2)
    {
        if(pos1 != pos2 || IsPlayerPosOutOfBound(pos1))
        {
            return false;
        }

        return (levelLoaderJSON.blocksPos[pos1].type == BlockType.Goal);
    }

    public bool IsPlayerOnTeleporter(Vector2 pos1, Vector2 pos2)
    {
        if (pos1 != pos2 || IsPlayerPosOutOfBound(pos1))
        {
            return false;
        }

        return (levelLoaderJSON.blocksPos[pos1].type == BlockType.Teleporter);
    }

    public bool IsPlayerOnSoftSwitch(Vector2 pos1, Vector2 pos2)
    {
        if (IsPlayerPosOutOfBound(pos2) || IsPlayerPosOutOfBound(pos1))
        {
            return false;
        }

        return (levelLoaderJSON.blocksPos[pos1].type == BlockType.SoftButton || levelLoaderJSON.blocksPos[pos2].type == BlockType.SoftButton);
    }

    public bool IsPlayerOnHardSwitch(Vector2 pos1, Vector2 pos2)
    {
        if (pos1 != pos2 || IsPlayerPosOutOfBound(pos1))
        {
            return false;
        }

        return (levelLoaderJSON.blocksPos[pos1].type == BlockType.HardButton);
    }

    public bool IsOnUnstableTile(Vector2 pos1, Vector2 pos2)
    {
        if (pos1 != pos2 || IsPlayerPosOutOfBound(pos1))
        {
            return false;
        }

        return (levelLoaderJSON.blocksPos[pos1].type == BlockType.Unstable);
    }

    public Vector2 GetOtherTeleporter(TeleporterBlock myTeleporter)
    {
        TeleporterBlock other = (TeleporterBlock)levelLoaderJSON.blocksIndex[myTeleporter.linkedTeleporterId];
        if(other == null)
        {
            Debug.Log("no teleporter");
            return Vector2.one * -1;
        }
        return new Vector2(other.transform.position.x, other.transform.position.z);
    }

    public bool spawnOnStart;
    void Start()
    {
        if(spawnOnStart){
           StartCoroutine(StartLevel());
        }
    }
}
