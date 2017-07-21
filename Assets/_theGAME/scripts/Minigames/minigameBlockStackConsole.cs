using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon;

public class minigameBlockStackConsole : PunBehaviour {

    [Tooltip("Prefab for minigame blocks")]
    public string block = "Minigame Block";

    [Tooltip("Array of block spawn points")]
    public BlockSpawnPoint[] spawnPoints;

    // List of spawned blocks
    public List<GameObject> Blocks = new List<GameObject>();

    // Is minigame being played?
    public bool gameActive { get; private set; }
    private BoxCollider consoleCollider;
    public ParticleSystem particleShield;
    
    public int blockCount = 5;
    public float blockSpawnHeight = 2;

    private float collectDis = 2.0f;
    private Vector3 stackPos;
    public float stackPadding = 0.6f;
    private int stackTotal = 0;

    //private Vector3 winBoxPosition { get { return transform.position + new Vector3(0.0f, 2.01f, 0.0f); } }
    //private Vector3 winBoxExtents = new Vector3(0.5f, 1.5f, 0.5f);

    public int winTotal = 5;
    private float winTimer = 5.0f;
    private bool winning;

    private float rewardDis = 20.0f;
    public Vector3 archPoint;
    public float archDuration = 2.0f;

    private float resetTimer = 0.0f;
    public float resetTimeThreshold = 10.0f;

    void Start() {
        // Console setup
        if (block == null) Debug.LogError("Minigame Console - Block Stack's 'block' prefab = null");
        spawnPoints = GetComponentsInChildren<BlockSpawnPoint>();

        // Minigame vars
        gameActive = false;
        consoleCollider = GetComponent<BoxCollider>();
        particleShield = GetComponentInChildren<ParticleSystem>();
        stackPos = transform.position + new Vector3(0, transform.localScale.y / 2 + stackPadding, 0);
        
        // Win vars
        archPoint = transform.position + new Vector3(1.0f, 7.0f, 0.0f);

        // Reset vars
        resetTimer = resetTimeThreshold;
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer player) {
        // If we are the MasterClient
        if (photonView.isMine) photonView.RPC("SetConsoleData", player, photonView.viewID, gameActive, stackPos, stackTotal);
    }

    void Update() {
        if (gameActive) {
            if (stackTotal >= winTotal && PhotonNetwork.isMasterClient && !winning) photonView.RPC("MinigameWinTimer", PhotonTargets.MasterClient);
        } else if (resetTimer <= resetTimeThreshold) resetTimer += Time.deltaTime;
    }

    public void Interact() {
        if (!gameActive) {
            if(resetTimer < resetTimeThreshold) {
                Debug.Log("Cannot reset game quite yet.");
                return;
            }
            
            photonView.RPC("SetGameActive", PhotonTargets.All, true);
            Debug.Log("Starting minigame!");
            
            // Instantiate [blockCount] blocks
            for (int i = 0; i < blockCount; i++) InstantiateBlock();
        } else {
            Debug.Log("Minigame in progress");
        }
    }

    public void InstantiateBlock() {
        Vector3 spawnPoint = Vector3.zero;
        if (spawnPoints != null && spawnPoints.Length > 0) {
            BlockSpawnPoint[] freeSpawnPoints = spawnPoints.Where(spawnPoint2 => !spawnPoint2.inUse).ToArray();

            if (freeSpawnPoints != null && freeSpawnPoints.Length > 0) {
                BlockSpawnPoint chosenSP = freeSpawnPoints[Random.Range(0, freeSpawnPoints.Length)];
                spawnPoint = chosenSP.transform.position;
                chosenSP.inUse = true;
            } else spawnPoint = new Vector3(Random.Range(-5.0f, 5.0f), transform.position.y, Random.Range(-5.0f, 5.0f));
        } else spawnPoint = new Vector3(Random.Range(-5.0f, 5.0f), transform.position.y, Random.Range(-5.0f, 5.0f));

        photonView.RPC("InstantiateBlockInScene", PhotonTargets.MasterClient, block, spawnPoint, Quaternion.identity, 0, null);
    }



    /* RPC CALLS */

    [PunRPC] // called by MasterClient to OnPhotonPlayerConnected
    void SetConsoleData(int viewID, bool active, Vector3 newStackPos, int newStackTotal) {
        Debug.Log("<Color=Magenta>SetConsoleData()</Color> -- Calling SetConsoleData");

        if (active) {
            gameActive = active;

            consoleCollider.center = new Vector3(0, 3, 0);
            consoleCollider.size = new Vector3(1, 5, 1);
            particleShield.Simulate(particleShield.main.duration);
            particleShield.Play();

            stackPos = newStackPos;
            stackTotal = newStackTotal;
        }
    }

    [PunRPC] // called to All
    public void SetGameActive(bool active) {
        Debug.Log("<Color=Magenta>SetGameActive()</Color> -- Calling SetGameActive");

        gameActive = active;

        if (gameActive) {
            consoleCollider.center = new Vector3(0, 3, 0);
            consoleCollider.size = new Vector3(1, 5, 1);
            particleShield.Play();
        }
    }

    [PunRPC] // called to MasterClient
    public void InstantiateBlockInScene(string PrefabName, Vector3 Position, Quaternion Rotation, int Group, object[] Data) {
        Debug.Log("<Color=Magenta>InstantiateBlockInScene()</Color> -- Calling InstantiateBlockInScene");

        GameObject block = PhotonNetwork.InstantiateSceneObject(PrefabName, Position, Rotation, Group, Data);
        photonView.RPC("SetBlockData", PhotonTargets.All, block.GetPhotonView().viewID);
    }

    [PunRPC] // called to All
    public void SetBlockData(int viewID) {
        Debug.Log("<Color=Magenta>SetBlockData()</Color> -- Calling SetBlockData");

        GameObject block = PhotonView.Find(viewID).gameObject;
        block.SetActive(true);
    }

    [PunRPC] // called to MasterClient
    public void CollectBlocks() {
        Debug.Log("<Color=Magenta>CollectBlocks()</Color> -- Calling CollectBlocks");

        Collider[] Colliders = Physics.OverlapSphere(transform.position, collectDis);
        if (Colliders != null) {
            Debug.Log("Colliders is NOT null: " + Colliders.Count());
            foreach (Collider collider in Colliders) {
                minigameBlock block = collider.GetComponent<minigameBlock>();
                if (block != null) {
                    Debug.Log("The collider IS a minigameBlock");
                    if (!block.hasOwner) {
                        block.transform.position = stackPos + new Vector3(0, block.transform.localScale.y / 2, 0);

                        stackPos += new Vector3(0, (block.transform.localScale.y + stackPadding), 0);
                        stackTotal++;
                        Debug.Log(stackTotal);

                        photonView.RPC("SetStackedBlockData", PhotonTargets.All, block.photonView.viewID, block.transform.position, stackPos, stackTotal);
                    }
                }
            }
        }
    }

    [PunRPC] // called to All
    void SetStackedBlockData(int viewID, Vector3 stackedPosition, Vector3 newStackPos, int newStackTotal) {
        Debug.Log("<Color=Magenta>SetStackedBlockData()</Color> -- Calling SetStackedBlockData");

        GameObject block = PhotonView.Find(viewID).gameObject;
        block.transform.position = stackedPosition;

        block.GetComponent<BoxCollider>().enabled = false;
        block.gameObject.layer = 0;

        // keep updated incase MasterClient changes
        block.GetComponent<minigameBlock>().stacked = true;
        stackPos = newStackPos;
        stackTotal = newStackTotal;
    }

    [PunRPC] // called to MasterClient
    public void MinigameWinTimer() {
        Debug.Log("<Color=Magenta>MinigameWinTimer()</Color> -- Calling MinigameWinTimer");

        winning = true;
        StartCoroutine(MinigameWin());
    }

    private IEnumerator MinigameWin() {
        // Change particleShield visually to inform nearby players the minigame has been won
        photonView.RPC("ParticleShieldWin", PhotonTargets.All);
        
        // Wait for winTimer in order to inform nearby players (through particleShield changes) that minigame has been won
        yield return new WaitForSeconds(winTimer);
        
        // Start 'winning' functionality
        photonView.RPC("SetGameActive", PhotonTargets.All, false);

        // Destroy minigame blocks
        if (Blocks.Count > 0) {
            foreach (GameObject Block in Blocks) {
                PhotonNetwork.Destroy(Block);
            }
        }

        // Determine player count
        int playerCount = 0;
        Collider[] Colliders = Physics.OverlapSphere(transform.position, rewardDis);
        if (Colliders != null) {
            foreach (Collider collider in Colliders) {
                PlayerController pcon = collider.GetComponent<PlayerController>();
                if (pcon != null) {
                    Debug.Log("We found another player!" + pcon.name);
                    playerCount++;
                }
            }
        }

        // Reward players
        for (int i = 0; i < playerCount; i++) {
            GameObject coin = PhotonNetwork.InstantiateSceneObject("Reward Awesomeness", transform.position, Quaternion.Euler(0.0f, 0.0f, -90.0f), 0, null);

            // Coin's endpoint = random direction * distance from console + console's position
            Vector3 endPoint = Quaternion.Euler(0, Random.Range(-180f, 180f), 0) * new Vector3(0.0f, 0.0f, 4.0f) + transform.position;

            // Start coin's coroutine animation
            StartCoroutine(CoinArch(coin.transform, archPoint, endPoint, archDuration));
        }

        // Reset minigame variables
        photonView.RPC("MinigameReset", PhotonTargets.All);

        Debug.Log("We win!! Awesomeness for everyone here :)");
    }

    [PunRPC]
    void ParticleShieldWin() {
        Debug.Log("<Color=Magenta>ParticleShieldWin()</Color> -- Calling ParticleShieldWin");

        ParticleSystem.MainModule main = particleShield.main;
        ParticleSystem.EmissionModule emission = particleShield.emission;
        ParticleSystem.MinMaxGradient newColor = Color.magenta;

        main.simulationSpeed = 3.0f;    // particle speed
        emission.rateOverTime = 300;    // emission rate
        main.startColor = newColor;     // particle color
    }

    private IEnumerator CoinArch(Transform coinTransform, Vector3 archPoint, Vector3 endPoint, float duration) {
        Vector3 startPos = coinTransform.position;
        float startTime = Time.time;
        float elapsedTime = Time.time - startTime;

        while (elapsedTime < duration) {
            elapsedTime = Time.time - startTime;
            float t = elapsedTime / duration;

            // Coin's position is...
            if (coinTransform == null) break;
            coinTransform.position = startPos * (1.0f - t) * (1.0f - t)
                                + 2.0f * (1.0f - t) * t * archPoint
                                + t * t * endPoint;

            yield return new WaitForEndOfFrame();
        }

        if (coinTransform != null) {
            coinTransform.position = endPoint;
        }
    }

    [PunRPC] // called to All
    void MinigameReset() {
        Debug.Log("<Color=Magenta>MinigameReset()</Color> -- Calling MinigameReset");

        // Clear blocks list
        Blocks.Clear();

        // Reset spawn points
        foreach (BlockSpawnPoint sp in spawnPoints) sp.inUse = false;

        // Reset collider
        consoleCollider.center = new Vector3(0, 0, 0);
        consoleCollider.size = new Vector3(1, 1, 1);

        // Reset particle shield
        particleShield.Stop();

        ParticleSystem.MainModule main = particleShield.main;
        ParticleSystem.EmissionModule emission = particleShield.emission;
        ParticleSystem.MinMaxGradient newColor = new Color(255.0f / 255.0f, 251.0f / 255.0f, 233.0f / 255.0f, 150.0f / 255.0f);
        main.simulationSpeed = 1.0f;    // particle speed
        emission.rateOverTime = 200;    // emission rate
        main.startColor = newColor;     // particle color

        // Reset stack variables
        stackPos = transform.position + new Vector3(0, transform.localScale.y / 2 + stackPadding, 0);
        stackTotal = 0;

        // Reset winning boolean
        winning = false;

        // Reset timers
        resetTimer = 0.0f;
    }

    //void OnDrawGizmos() {
    //    var boxes = Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact"));
    //    Gizmos.color = boxes.Length >= 3 ? Color.green : Color.yellow;
    //    Gizmos.DrawWireCube(winBoxPosition, winBoxExtents * 2f);
    //}
}

// TODO: spawnPoints array isUse boolean not synced. Consider -- InstantiateBlock an RPC and sync inUse to PT.All and OnPhotonPlayerConnect
// TODO: Create and implement a method for RPC's Debug.Log line.
// TODO: Consider adding UI Text element for MinigameWinTimer. Visual countdown to reward



// Develop system for 'reward' awesomeness
// Players can only collect one coin of reward awesomeness within the first # of seconds of the reward spawning

// All 'reward' awesomeness contains a collect timer of a # of seconds
// When a player collects a 'reward' awesomeness coin, coin's collect timer is added to player as a 'reward' awesomeness collect cooldown
// Once player's collect cooldown expires (reaches 0), player can collect more 'reward' awesomeness
// If 'reward' awesomeness's collect timer is 0 when a player trys to collect it, no cooldown is added to the player

// Consider adjusting archPoint so players right next to console cannot 'intercept' coins before they arch



// After minigame, fix up player movement/physics/lag
// Hide coin upon collection
// Reposition block upon 'stacking' drop

// Build Terrain (marching cubes)