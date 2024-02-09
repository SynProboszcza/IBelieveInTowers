using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerMainGameLoop : MonoBehaviourPunCallbacks, IPunObservable
{
    // Public instance so we can enable the correct one
    // Every scene(map) has two parts for simplicity
    public GameObject attackerPart;
    public GameObject defenderPart;

    // Public instance so we can enable the correct one
    // Every scene(map) has two parts for simplicity
    public GameObject attackerMatchResults;
    public GameObject defenderMatchResults;

    // Parent GO for enemy nickname, win history and map #
    public GameObject matchInfo;

    public GameObject nodePrefab;
    [HideInInspector]
    public List<GameObject> nodes = new List<GameObject>();
    [HideInInspector]
    public GameObject shopNodesCollection;

    // playArenaCorners is an object that has 4 properly named child gameObjects
    // Their transform.position 's are corners of play arena
    public Transform playArenaCorners;
    public GameObject msgToPlayerCanvas;
    public Transform[] waypoints;
    public Transform[] obstacles;
    public float roundTimeSeconds = 180;
    public float currentTime = 5f;
    public float defenderHealthToSync;
    private bool isTimerRunning = false;
    [Header("Attacker parts")]
    public TMP_Text a_timer;
    public TMP_Text a_enemyHealthTextField;
    public TMP_Text a_playerMoneyTextField;
    public TMP_Text a_playerManaTextField;
    // at the time of creating only holds money per second
    public AttackerData attackerData;
    public int a_moneyPerSecond = 10;
    [Header("Defender parts")]
    public TMP_Text d_timer;
    public TMP_Text d_enemyHealthTextField;
    public TMP_Text d_playerMoneyTextField;
    public TMP_Text d_playerManaTextField;
    [HideInInspector]
    public Vector2 topLeft;
    [HideInInspector]
    public Vector2 topRight;   // Despite what VS says, they are used in
    [HideInInspector]
    public Vector2 bottomLeft; // GetPlayArenaCorners() 
    [HideInInspector]
    public Vector2 bottomRight;
    [HideInInspector]
    public int nodesInstantiated = 0;
    [HideInInspector]
    public int nodesDestroyed = 0;
    public int secondsToWaitAfterGameEnd = 3;
    //public int playerMoney = 0;
    //public int playerMana = 0;
    //public int defenderHealth;
    [Tooltip("Specifies an offset on a Z axis. 0 is default map, camera is -15, positive will be hidden. This is mainly for colliders to work properly")]
    public static float shopNodesZOffset = -2;
    public bool isDebugging = false;
    [HideInInspector]
    public bool isShopOpen = false;
    //private bool isAllowedInstantiating = true;
    [HideInInspector]
    public bool amIMaster;
    [HideInInspector]
    public bool amIDefender;
    [HideInInspector]
    private bool matchResultsShown = false; // Flag so it gets run only once
    private bool addingMoneySet = false;
    private Coroutine moneyPerSecond;

    // FPS limit and SIMPLEConnect
    void Awake()
    {
        // Doing this because difference in framerate between Editor and 
        // an application causes differences in projectile speed
        //
        // This should be copied inside MainMenuLoop and MainGameLoop
        // (i think(?)(and maybe every scene?))
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
        if (GameObject.Find("SIMPLEConnect") != null && GameObject.Find("SIMPLEConnect").activeSelf)
        {
            print("Disabling MainGame for SIMPLEConnect to be enabled when connection is established");
            gameObject.SetActive(false);
        }
    }

    // Parts de/activating, setting match duration, amIMaster/amIDefender from CSM
    void Start()
    {
        //print("true && false && true: " + (true && false && true));
        // Check if defending, then activate proper part
        attackerPart.gameObject.SetActive(false);
        defenderPart.gameObject.SetActive(false);
        attackerMatchResults.gameObject.SetActive(false);
        attackerMatchResults.transform.Find("Win").gameObject.SetActive(false);
        attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false);
        defenderMatchResults.gameObject.SetActive(false);
        defenderMatchResults.transform.Find("Win").gameObject.SetActive(false);
        defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false);
        GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(false);
        GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(false);
        amIMaster = CrossSceneManager.instance.amIMaster;
        amIDefender = CrossSceneManager.instance.amIDefender;
        defenderHealthToSync = CrossSceneManager.instance.defenderHealth;
        GetPlayArenaCorners();
        //playerMoney = CrossSceneManager.instance.playerMoney;
        //playerMana = CrossSceneManager.instance.playerMana;
        if (amIDefender)
        {
            defenderPart.gameObject.SetActive(true);
            shopNodesCollection = new GameObject("ShopNodesCollection");
            GenerateAndConfigureNodeMesh(topLeft, bottomRight);
            // TODO: ShopNode needs to show spells
        }
        else
        {
            attackerPart.gameObject.SetActive(true);
            // defenderHealth = CrossSceneManager.instance.defenderHealth;
        }

        // =====================================================================================================================================================
        // Temporarily disabled - for testing
        // =====================================================================================================================================================
        roundTimeSeconds = CrossSceneManager.instance.currentMatchMaxTime;
        //print("Got this for time: " + CrossSceneManager.instance.currentMatchMaxTime);
        //print("Now it is: " + roundTimeSeconds);
        //print("CSM: " + CrossSceneManager.instance);
        currentTime = roundTimeSeconds;

        // start the timer, syncing is done by sending current time and compensating lag
        isTimerRunning = true;
        //CrossSceneManager.instance.SoftReset();

        // -----------------------------------------------------------------------
        // Set up MatchInfo child objects so player can see basic information
        // during gameplay
        // -----------------------------------------------------------------------
        matchInfo.transform.Find("EnemyNick").gameObject.GetComponent<TMP_Text>().text = "Enemy: " + CrossSceneManager.instance.enemyNickname;
        matchInfo.transform.Find("RoundHistory").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.PlayerFriendlyMatchHistory();
        /* 
        // "Map3Multiplayer"
        // "Map #3Multiplayer"
        // "Map #3"
        */
        string mapName = SceneManager.GetActiveScene().name;
        mapName = mapName.Replace("Map", "Map #");
        mapName = mapName.Replace("Multiplayer", "");
        matchInfo.transform.Find("CurrentMapName").gameObject.GetComponent<TMP_Text>().text = mapName;


    }

    void Update()
    {
        // Stream is writing directly to CSM
        defenderHealthToSync = CrossSceneManager.instance.defenderHealth;
        UpdatePlayerStats();
        // Check if defending when ded
        //  checking is done when dealing damage
        if (amIMaster && CrossSceneManager.instance.hasDefenderDied && !matchResultsShown)
        {
            print("Calling RoundEnd() because Defender died!");
            Debug.LogWarning("RPC SetDefenderDiedTrue bc i am master(" + amIMaster + ") and csm.defenderdied: " + CrossSceneManager.instance.hasDefenderDied + " and !matchresultsShown: " + matchResultsShown);
            RoundEnd(amIDefender, true);
            gameObject.GetComponent<PhotonView>().RPC("SetDefenderDiedTrue", RpcTarget.All);
            matchResultsShown = true; // Flag so it gets run only once
        }
        // -----------------------------------------------------------------------
        // Timer logic
        // -----------------------------------------------------------------------
        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                if (amIDefender)
                {
                    DisplayTime(d_timer, currentTime);
                }
                else
                {
                    if (!addingMoneySet)
                    {
                        moneyPerSecond = StartCoroutine(AddMoneyPerSecond(attackerData.moneyPerSecond));
                        addingMoneySet = true;
                    }
                    DisplayTime(a_timer, currentTime);
                }
            }
            else
            {
                // Time has passed!
                currentTime = -1f; // DisplayTime adds 1, so the result is 0:00
                if (amIDefender)
                {
                    DisplayTime(d_timer, currentTime);
                }
                else
                {
                    DisplayTime(a_timer, currentTime);
                }
                if (amIMaster)
                {
                    print("Calling RoundEnd() because time passed!");
                    RoundEnd(amIDefender, false);
                }
                isTimerRunning = false;
            }
        }

        //synchronize with host/client
        //maybe cheats?
        //wait for escape menu
        //maybe listen for quitting? or add button for it

    }

    private void DisplayTime(TMP_Text timer, float time)
    {
        time += 1; // this is so it does not show 0 for whole last second
        int seconds = Mathf.FloorToInt(time % 60);
        int minutes = Mathf.FloorToInt(time / 60);
        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void RoundEnd(bool amIDefending, bool didDefenderDie)
    {
        // -----------------------------------------------------------------------
        // Stop current round and add result
        // -----------------------------------------------------------------------
        //print("Starting to RoundEnd");
        isTimerRunning = false;
        if (moneyPerSecond != null)
        {
            StopCoroutine(moneyPerSecond);
        }
        // -----------------------------------------------------------------------
        // If master update match history and send it to roomProperties
        // Also RoundEndCleaning() changes isMasterDefending
        // -----------------------------------------------------------------------
        if (amIMaster)
        {
            string matchHistory = (string)PhotonNetwork.CurrentRoom.CustomProperties["DidMasterWon"];
            // if it contains t or f this is false only when len == 0
            // "" == false ; "t" == false ; "f" == false ; "a" = true ; "tfa" = true ; etc.
            if (matchHistory.Length > 3 || (!(matchHistory.Contains("t") || matchHistory.Contains("f"))) && !(matchHistory.Length == 0))
            {
                Debug.LogError("Got bad data about match history: [[[" + matchHistory + "]]]");
                Debug.LogError("What triggered:" +
                    "\nlength: " + matchHistory.Length +
                    "\ncontains(t): " + matchHistory.Contains("t") +
                    "\ncontains(f): " + matchHistory.Contains("f") +
                    "\nshort contains logic: " + !(matchHistory.Contains("t") || matchHistory.Contains("f")) +
                    "\nfull contains logic: " + ((!(matchHistory.Contains("t") || matchHistory.Contains("f"))) && !(matchHistory.Length == 0)) +
                    "\ncurrent history is set to \"t\"");
                matchHistory = "t";
            }
            else if (amIDefending == !didDefenderDie)
            {
                // Logic helper table: only master checks this
                // t t -> t f -> f
                // t f -> t t -> t
                // f t -> f f -> t
                // f f -> f t -> f
                matchHistory += "t"; // add current win
                print("I (master) (defender?: " + amIDefender + ") won round!!");
            }
            else
            {
                print("I (master) (defender?: " + amIDefender + ") lost round!!");
                Debug.LogWarning("Sending RPC SetDefenderDiedTrue to everyone bc i am master and lost round");
                gameObject.GetComponent<PhotonView>().RPC("SetDefenderDiedTrue", RpcTarget.All);
                matchHistory += "f"; // add current loss
            }
            // -----------------------------------------------------------------------
            // -----------------------------------------------------------------------
            // -----------------------------------------------------------------------
            //ShowResultText(amIDefending, didDefenderDie);
            RoundEndCleaning(matchHistory, amIDefending);
        }
        /*
        roomCreatorNickname
        roomJoinedNickname
        isMasterDefending
        UnlimitedMoney
        UnlimitedMana
        InvincibleTurrets
        SpecialRules
        MatchTime
        DidMasterWon
        isMasterReady
        isJoinedReady
         */

        /*
        // -----------------------------------------------------------------------
        // Updating local win history; if master send this history over
        // -----------------------------------------------------------------------
        // amimaster == (amidefending != diddefenderdie)
        print("added to my local win history: " + (amIMaster == (amIDefending != didDefenderDie)).ToString());
        CrossSceneManager.instance.didMasterWin.Add(amIMaster == (amIDefending != didDefenderDie));
        print("whole local win history: " + CrossSceneManager.instance.MatchHistoryToString());
        if (amIMaster)
        {
            print("Sending defender wins to sync: " + CrossSceneManager.instance.MatchHistoryToString());
            gameObject.GetComponent<PhotonView>().RPC("SyncRoundResults", RpcTarget.Others, CrossSceneManager.instance.MatchHistoryToString());
        }


        // -----------------------------------------------------------------------
        // Check if we finish match or go into next round and do it
        // Call either:
        //  NextRound(amIDefending, didDefenderDie)
        //  FinishMatch(amIDefending) 
        // -----------------------------------------------------------------------
        if (!amIMaster && CrossSceneManager.instance.didMasterWin.Count == 0)
        {
            // We are joined and just ended the first round, sync data did not come yet
            print("next round because count is zero");
            print("NexRound("+ amIDefending + ", " + didDefenderDie+") from RoundEnd()");
            NextRound(amIDefending, didDefenderDie);
        }
        else if (CrossSceneManager.instance.didMasterWin.Count == 1)
        {
            print("next round because count is one");
            print("NexRound("+ amIDefending + ", " + didDefenderDie+") from RoundEnd()");
            NextRound(amIDefending, didDefenderDie);
        }
        else if (CrossSceneManager.instance.didMasterWin.Count == 2)
        {
            // check if somebody won
            //  true: finish match 
            //  false: next round
            if (CrossSceneManager.instance.didMasterWin[0] == CrossSceneManager.instance.didMasterWin[1])
            {
                print("finishing match [0] == [1]:\n[0]:" + CrossSceneManager.instance.didMasterWin[0] + ", [1]:" + CrossSceneManager.instance.didMasterWin[1]);
                print("FinishMatch(" + amIDefending + ") from RoundEnd()");
                FinishMatch(amIDefending);
            }
            else
            {
                print("next round because count is two, but different");
                print("NexRound("+ amIDefending + ", " + didDefenderDie+") from RoundEnd()");
                NextRound(amIDefending, didDefenderDie);
            }
        }
        else if (CrossSceneManager.instance.didMasterWin.Count == 3)
        {
            // finish match
            print("finishing match because count is 3");
            print("FinishMatch(" + amIDefending + ") from RoundEnd()");
            FinishMatch(amIDefending);
        }
        else
        {
            // never should be
            Debug.LogError("SOMETHING WENT CRAZY WRONG");
            Debug.LogError("SOMETHING WENT CRAZY WRONG");
            Debug.LogError("SOMETHING WENT CRAZY WRONG");
            print("CrossSceneManager.instance.didDefenderWin.Count == " + CrossSceneManager.instance.didMasterWin.Count);
        }
        //CrossSceneManager.instance.ResetInBetweenRounds();*/
        print("Ending to RoundEnd");
    }

    private void ShowResultText(bool amIDefending, bool didDefenderDie, bool isMatchFinished)
    {
        if (amIDefending)
        {
            // i defender died
            if (didDefenderDie)
            {
                if (isMatchFinished)
                {
                    // In this we prematurely activate match end texts (as opposed to roundend text)
                    // Next parts will activate it again, but it will do nothing
                    defenderMatchResults.SetActive(false);
                    defenderMatchResults.transform.Find("Win").gameObject.SetActive(false); // fallback
                    //defenderMatchResults.transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundLost;
                    defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false);
                    UpdateMatchHistory();
                    print("I defender died, match is over in: " + secondsToWaitAfterGameEnd + " seconds");
                    //ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
                }
                else
                {
                    // We don't activate attacker/defender parts, we expect the proper one to be active
                    defenderMatchResults.SetActive(true);
                    defenderMatchResults.transform.Find("Win").gameObject.SetActive(false); // fallback
                    defenderMatchResults.transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundLost;
                    defenderMatchResults.transform.Find("Loose").gameObject.SetActive(true);
                    UpdateMatchHistory();
                    print("I defender died, next round in: " + secondsToWaitAfterGameEnd + " seconds");
                    //ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
                }
            }
            // i defender won by time
            else
            {
                if (isMatchFinished)
                {
                    // In this we prematurely activate match end texts (as opposed to roundend text)
                    // Next parts will activate it again, but it will do nothing
                    defenderMatchResults.SetActive(false);
                    //defenderMatchResults.transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundWon;
                    defenderMatchResults.transform.Find("Win").gameObject.SetActive(false);
                    defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false); // fallback
                    UpdateMatchHistory();
                    print("I defender won by surviving, match is over in: " + secondsToWaitAfterGameEnd + " seconds");
                    //hangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
                }
                else
                {
                    // We don't activate attacker/defender parts, we expect the proper one to be active
                    defenderMatchResults.SetActive(true);
                    defenderMatchResults.transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundWon;
                    defenderMatchResults.transform.Find("Win").gameObject.SetActive(true);
                    defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false); // fallback
                    UpdateMatchHistory();
                    print("I defender won by surviving, next round in: " + secondsToWaitAfterGameEnd + " seconds");
                    //hangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
                }
            }
        }
        else
        {
            // i attacker won by killing defender
            if (didDefenderDie)
            {
                if (isMatchFinished)
                {
                    // In this we prematurely activate match end texts (as opposed to roundend text)
                    // Next parts will activate it again, but it will do nothing
                    attackerMatchResults.SetActive(false);
                    attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false); // fallback
                    //attackerMatchResults.transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundWon;
                    attackerMatchResults.transform.Find("Win").gameObject.SetActive(false);
                    UpdateMatchHistory();
                    print("I attacker won by killing defender, match is over in: " + secondsToWaitAfterGameEnd + " seconds");
                    //ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
                }
                else
                {
                    attackerMatchResults.SetActive(true);
                    attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false); // fallback
                    attackerMatchResults.transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundWon;
                    attackerMatchResults.transform.Find("Win").gameObject.SetActive(true);
                    UpdateMatchHistory();
                    print("I attacker won by killing defender, next round in: " + secondsToWaitAfterGameEnd + " seconds");
                    //ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
                }
            }
            // i attacker lost by time passing
            else
            {
                if (isMatchFinished)
                {
                    // In this we prematurely activate match end texts (as opposed to roundend text)
                    // Next parts will activate it again, but it will do nothing
                    attackerMatchResults.SetActive(false);
                    //attackerMatchResults.transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundLost;
                    attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false);
                    attackerMatchResults.transform.Find("Win").gameObject.SetActive(false); // fallback
                    UpdateMatchHistory();
                    print("I attacker lost by time, next round in: " + secondsToWaitAfterGameEnd + " seconds");
                    //ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
                }
                else
                {
                    attackerMatchResults.SetActive(true);
                    attackerMatchResults.transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundLost;
                    attackerMatchResults.transform.Find("Loose").gameObject.SetActive(true);
                    attackerMatchResults.transform.Find("Win").gameObject.SetActive(false); // fallback
                    UpdateMatchHistory();
                    print("I attacker lost by time, next round in: " + secondsToWaitAfterGameEnd + " seconds");
                    //ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
                }
            }
        }
        CrossSceneManager.instance.SoftReset();
    }

    /* //NextRound(bool amIDefending, bool didDefenderDie) && FinishMatch(bool amIDefending)
    private void NextRound(bool amIDefending, bool didDefenderDie)
    {
        print("Starting to NextRound");
        // next:
        //  show text won/lost
        //  change scene to InBetweenScene
        //  roundEndCleaning()
        // 
        // This does not add anything to CSM, only soft reset

        // --------------------------------------------------------------
        // Check if match finished and return early if true 
        // --------------------------------------------------------------
        if (CrossSceneManager.instance.isMatchOver)
        {
            RoundEndCleaning();
            return;
        }
        
        // --------------------------------------------------------------
        // Show win/loose round text from amIDefendig and didDefenderDie
        // --------------------------------------------------------------
        string sceneName = "InBetweenScene";
        if (amIDefending)
        {
            // i defender died
            if (didDefenderDie)
            {
                // We don't activate attacker/defender parts, we expect the proper one to be active
                defenderMatchResults.SetActive(true);
                defenderMatchResults.transform.Find("Win").gameObject.SetActive(false); // fallback
                defenderMatchResults.transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundLost;
                defenderMatchResults.transform.Find("Loose").gameObject.SetActive(true);
                print("I defender died, next round in: " + secondsToWaitAfterGameEnd + " seconds");
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
            }
            // i defender won by time
            else
            {
                // We don't activate attacker/defender parts, we expect the proper one to be active
                defenderMatchResults.SetActive(true);
                defenderMatchResults.transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundWon;
                defenderMatchResults.transform.Find("Win").gameObject.SetActive(true);
                defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false); // fallback
                print("I defender won by surviving, next round in: " + secondsToWaitAfterGameEnd + " seconds");
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
            }
        }
        else
        {
            // i attacker won by killing defender
            if (didDefenderDie)
            {
                attackerMatchResults.SetActive(true);
                attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false); // fallback
                attackerMatchResults.transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundWon;
                attackerMatchResults.transform.Find("Win").gameObject.SetActive(true);
                print("I attacker won by killing defender, next round in: " + secondsToWaitAfterGameEnd + " seconds");
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
            }
            // i attacker lost by time passing
            else
            {
                attackerMatchResults.SetActive(true);
                attackerMatchResults.transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.roundLost;
                attackerMatchResults.transform.Find("Loose").gameObject.SetActive(true);
                attackerMatchResults.transform.Find("Win").gameObject.SetActive(false); // fallback
                print("I attacker lost by time, next round in: " + secondsToWaitAfterGameEnd + " seconds");
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, false);
            }
        }
        
        // --------------------------------------------------------------
        // RoundEndCleaning()
        // --------------------------------------------------------------
        //RoundEndCleaning();
        print("Ending to RoundEnd");
    }

    private void FinishMatch(bool amIDefending)
    {
        print("Starting to FinishMatch");
        // finish match: 
        //  show text won/lost match
        //  leave room
        //  change scene to main menu
        //      CSM full reset in main menu
        // 
        // --------------------------------------------------------------
        // Set isMatchOver in CSM in both clients
        // --------------------------------------------------------------
        CrossSceneManager.instance.isMatchOver = true;
        gameObject.GetComponent<PhotonView>().RPC("MatchIsOver", RpcTarget.All);
        
        // --------------------------------------------------------------
        // Check if master won
        // --------------------------------------------------------------
        bool didMasterWin = false;
        if (CrossSceneManager.instance.didMasterWin.Count == 2 && (CrossSceneManager.instance.didMasterWin[0] == CrossSceneManager.instance.didMasterWin[1]))
        {
            didMasterWin = CrossSceneManager.instance.didMasterWin[0];
        }
        else if (CrossSceneManager.instance.didMasterWin.Count == 3)
        {
            int masterWinsAmonut = 0;
            for (int i = 0; i < 3; i++)
            {
                if (CrossSceneManager.instance.didMasterWin[i])
                {
                    masterWinsAmonut++;
                }
            }
            if (masterWinsAmonut >= 2)
            {
                didMasterWin = true;
            }
            else
            {
                didMasterWin = false;
            }
        }
        else
        {
            Debug.LogError("Something called finish match when there is not enough rounds played: " + CrossSceneManager.instance.didMasterWin.Count);
            return;
        }

        // --------------------------------------------------------------
        // Check if it is local win
        // --------------------------------------------------------------
        string sceneName = "MainMenu";
        if (amIMaster)
        {
            if (didMasterWin)
            {
                // i defender won match by time
                print("i defender won match: " + CrossSceneManager.instance.MatchHistoryToString());
                defenderMatchResults.SetActive(false); // disable round won/lost texts
                attackerMatchResults.SetActive(false);
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchWon;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
            else
            {
                // i defender died match
                print("i defender lost match: " + CrossSceneManager.instance.MatchHistoryToString());
                defenderMatchResults.SetActive(false); // disable round won/lost texts
                attackerMatchResults.SetActive(false);
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchLost;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
        }
        else
        {
            if (didMasterWin)
            {
                // i attacker lost match by time
                print("i attacker lost match: " + CrossSceneManager.instance.MatchHistoryToString());
                defenderMatchResults.SetActive(false); // disable round won/lost texts
                attackerMatchResults.SetActive(false);
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchLost;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
            else
            {
                // i attacker won match by killing defender
                print("i attacker won match: " + CrossSceneManager.instance.MatchHistoryToString());
                defenderMatchResults.SetActive(false); // disable round won/lost texts
                attackerMatchResults.SetActive(false);
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchWon;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
        }
        print("Ending to FinishMatch");
    }
    */

    private void FinishMatch(bool didMasterWin)
    {
        string sceneName = "MainMenu";
        if (amIMaster)
        {
            if (didMasterWin)
            {
                // i master won 
                print("i master won match: " + CrossSceneManager.instance.MatchHistoryToString());
                UpdateMatchHistory();
                defenderMatchResults.SetActive(false); // disable round won/lost texts
                attackerMatchResults.SetActive(false);
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchWon;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
            else
            {
                // i master lost match
                print("i master lost match: " + CrossSceneManager.instance.MatchHistoryToString());
                UpdateMatchHistory();
                defenderMatchResults.SetActive(false); // disable round won/lost texts
                attackerMatchResults.SetActive(false);
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchLost;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
        }
        else
        {
            if (didMasterWin)
            {
                // i joined lost match 
                print("i joined lost match: " + CrossSceneManager.instance.MatchHistoryToString());
                UpdateMatchHistory();
                defenderMatchResults.SetActive(false); // disable round won/lost texts
                attackerMatchResults.SetActive(false);
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchLost;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
            else
            {
                // i joined won match
                print("i joined won match: " + CrossSceneManager.instance.MatchHistoryToString());
                UpdateMatchHistory();
                defenderMatchResults.SetActive(false); // disable round won/lost texts
                attackerMatchResults.SetActive(false);
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchWon;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }

        }

    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        //print("Room props changed: " + propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey("DidMasterWon"))
        {
            // Received match history from master (even if we're master)
            //print("Got history, before update: " + CrossSceneManager.instance.MatchHistoryToString());
            CrossSceneManager.instance.MatchHistoryFromString((string)propertiesThatChanged["DidMasterWon"]);
            //print("Got history, after update: " + CrossSceneManager.instance.MatchHistoryToString());
            // -----------------------------------------------------------------------
            // Detect if match is over
            // -----------------------------------------------------------------------
            string matchHistory = CrossSceneManager.instance.MatchHistoryToString();

            // Finish match
            if (matchHistory.Length == 2 && matchHistory[0] == matchHistory[1])
            {
                gameObject.GetComponent<PhotonView>().RPC("MatchIsOver", RpcTarget.All);
                MatchIsOver();
                print("Match is over! history: " + CrossSceneManager.instance.MatchHistoryToString());
                if (matchHistory[0] == 't')
                {
                    FinishMatch(true);
                }
                else if (matchHistory[0] == 'f')
                {
                    FinishMatch(false);
                }
                else
                {
                    Debug.Log("Match should end, but data is sad: [" + matchHistory[0] + "]");
                }
            }
            // Finish match
            else if (matchHistory.Length == 3)
            {
                int amountOfMasterWins = 0;
                foreach (char c in CrossSceneManager.instance.MatchHistoryToString())
                {
                    if (c == 't')
                    {
                        amountOfMasterWins++;
                    }
                }
                if (amountOfMasterWins >= 2) // master won
                {
                    if (amIMaster)
                    {
                        print("I master won match!");
                        //didMasterWin = true;
                        FinishMatch(true);
                    }
                    else
                    {
                        print("I joined lost match!");
                        //didMasterWin = false;
                        FinishMatch(true);
                    }
                }
                else // joined won
                {
                    if (amIMaster)
                    {
                        print("I master lost match!");
                        //didMasterWin = false;
                        FinishMatch(false);
                    }
                    else
                    {
                        print("I joined won match!");
                        //didMasterWin = true;
                        FinishMatch(false);
                    }
                }
                gameObject.GetComponent<PhotonView>().RPC("MatchIsOver", RpcTarget.All);
                MatchIsOver();
                print("Match is over! history: " + CrossSceneManager.instance.MatchHistoryToString());
            }
            // Next round
            else
            {
                // if amidefender and won => defender lived
                // if amidefender and lost => defender died
                // joined defender + f
                // joined defender + master lost
                // joined defender + master attacker lost
                // joined defender won + master attacker lost
                // joined defender won => defender not died
                //
                // joined:
                //  defender + t => defender lost
                //  defender + f => defender won
                //
                // master:
                //  defender + t => defender won
                //  defender + f => defender lost
                // 
                // 
                ////////////////////////////////////////////// // if defending && (amimaster && master won)
                if (amIDefender && (amIMaster == matchHistory[matchHistory.Length - 1].Equals('t')))
                {
                    // i am master defender won
                    ShowResultText(amIDefender, false, false);
                    ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "InBetweenScene", false);
                }
                ////////////////////////////////////////////// //      if defending && (amimaster && master lost)
                else if (amIDefender && (amIMaster == matchHistory[matchHistory.Length - 1].Equals('f')))
                {
                    // i am master defender lost
                    ShowResultText(amIDefender, true, false);
                    ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "InBetweenScene", false);
                }
                ////////////////////////////////////////////// //       if joined attacking && (ilost ) => master defender won / joined attacker lost
                ////////////////////////////////////////////// //       if attacking && (amijoined  && master won)
                else if (!amIDefender && (!amIMaster == matchHistory[matchHistory.Length - 1].Equals('t')))
                {
                    // i am joined attacker lost
                    ShowResultText(amIDefender, false, false);
                    ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "InBetweenScene", false);
                }
                ////////////////////////////////////////////// //         if attacking && (amijoined  won)
                else if (!amIDefender && (!amIMaster == matchHistory[matchHistory.Length - 1].Equals('f')))
                {
                    // i am joined attacker won
                    ShowResultText(amIDefender, true, false);
                    ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "InBetweenScene", false);
                }
                else
                {
                    Debug.LogError("Every planned scenario failed");
                    string _errorTmp = "";
                    _errorTmp += "amIDefender: " + amIDefender + " amIMaster: " + amIMaster + "\n";
                    _errorTmp += "matchHistory: " + matchHistory + "\n";
                    _errorTmp += "matchHistory[matchHistory.Length - 1].Equals('t'): " + matchHistory[matchHistory.Length - 1].Equals('t') + "\n";
                    _errorTmp += "matchHistory[matchHistory.Length - 1].Equals('f'): " + matchHistory[matchHistory.Length - 1].Equals('f') + "\n";
                    _errorTmp += "third right half: " + (!amIMaster && matchHistory[matchHistory.Length - 1].Equals('f')) + "\n";
                    _errorTmp += "======================================================================================" + "\n";
                    _errorTmp += "first full:  " + (amIDefender && (amIMaster && matchHistory[matchHistory.Length - 1].Equals('t'))) + "\n";
                    _errorTmp += "second full: " + (amIDefender && (amIMaster && matchHistory[matchHistory.Length - 1].Equals('f'))) + "\n";
                    _errorTmp += "third full:  " + (!amIDefender && (!amIMaster && matchHistory[matchHistory.Length - 1].Equals('t'))) + "\n";
                    _errorTmp += "fourth full: " + (!amIDefender && (!amIMaster && matchHistory[matchHistory.Length - 1].Equals('f'))) + "\n";
                    print(_errorTmp);
                }
            }

            if (propertiesThatChanged.ContainsKey("isMasterDefending") && !CrossSceneManager.instance.isMatchOver)
            {
                if (amIMaster)
                {
                    print("Changing defending side(master), was defender?: " + amIDefender);
                    amIDefender = (bool)propertiesThatChanged["isMasterDefending"];
                    CrossSceneManager.instance.amIDefender = (bool)propertiesThatChanged["isMasterDefending"];
                    print("Changing defending side(master), am i defender?: " + amIDefender);
                }
                else
                {
                    print("Changing defending side(joined), was defender?: " + amIDefender);
                    amIDefender = !(bool)propertiesThatChanged["isMasterDefending"];
                    CrossSceneManager.instance.amIDefender = !(bool)propertiesThatChanged["isMasterDefending"];
                    print("Changing defending side(joined), am i defender?: " + amIDefender);
                }
            }
        }
    }

    [PunRPC]
    public void SetDefenderDiedTrue()
    {
        print("Received RPC that defender has died!");
        CrossSceneManager.instance.hasDefenderDied = true;
    }

    [PunRPC]
    public void MatchIsOver()
    {
        print("Received RPC that match is over!");
        CrossSceneManager.instance.isMatchOver = true;
    }
    /*
    [PunRPC]
    public void SyncRoundResults(string defenderWins)
    {
        // We expect something like "ttf" "f" "ft"
        // amount of letters show how many rounds were played
        // t = true; defender won that round;; f = false; defender lost that round
        // letters need to be in correct order
        print("Got round results for syncing: " + defenderWins);
        if (defenderWins.Length < 1 || defenderWins.Length > 3)
        {
            // Early return
            Debug.LogError("Received incorrect wins list: " + defenderWins, gameObject);
            Debug.LogError("leaving");
            PhotonNetwork.Disconnect();
            ChangeSceneAfterNSeconds(5, "HostGame", false);
        }
        CrossSceneManager.instance.didMasterWin.Clear();
        CrossSceneManager.instance.MatchHistoryFromString(defenderWins); // overwrite local win history
        print("cleared list of wins, now its: " + CrossSceneManager.instance.MatchHistoryToString());
        bool isHistoryCorrect = true;
        for (int i = 0; i < defenderWins.Length; i++)
        {
            if (defenderWins[i].Equals('t')) // needs to be '' not "" because its char, not string
            {
                //print("added win for defender: true");
                //CrossSceneManager.instance.didDefenderWin.Add(true);
                if (!CrossSceneManager.instance.didMasterWin[i])
                {
                    Debug.LogError("Incorrect win history at i=" + i + ": " + CrossSceneManager.instance.didMasterWin[i]);
                    isHistoryCorrect = false;
                }
                else
                {
                    print("Correct win history at i=" + i + ": " + CrossSceneManager.instance.didMasterWin[i]);
                }
            }
            else if (defenderWins[i].Equals('f'))
            {
                //print("added loose for defender: false");
                //CrossSceneManager.instance.didDefenderWin.Add(false);
                if (CrossSceneManager.instance.didMasterWin[i])
                {
                    Debug.LogError("Incorrect win history at i=" + i + ": " + CrossSceneManager.instance.didMasterWin[i]);
                    isHistoryCorrect = false;
                }
                else
                {
                    print("Correct win history at i=" + i + ": " + CrossSceneManager.instance.didMasterWin[i]);
                }
            }
            else
            {
                Debug.LogError("what have you passed on this cursed land: \"" + defenderWins[i] + "\" of length: " + defenderWins.Length, gameObject);
            }
        }
        if (CrossSceneManager.instance.didMasterWin.Count > 1 && (CrossSceneManager.instance.didMasterWin[0] == CrossSceneManager.instance.didMasterWin[1]))
        {
            print("FinishMatch("+ amIDefender + ") from SyncRoundResults");
            FinishMatch(amIDefender);
        } else if(isHistoryCorrect)
        {
            print("starting next round using stats from CSM: \nCrossSceneManager.instance.amIDefender: " +  CrossSceneManager.instance.amIDefender +
                "\nCrossSceneManager.instance.hasDefenderDied: " + CrossSceneManager.instance.hasDefenderDied);
            NextRound(CrossSceneManager.instance.amIDefender, CrossSceneManager.instance.hasDefenderDied);
        }
    }
    */
    [PunRPC]
    public void AddResourcesShowAtMouse(bool forDefender, bool isMoney, int amount)
    {
        if (CrossSceneManager.instance.amIDefender == forDefender)
        {
            if (isMoney)
            {
                CrossSceneManager.instance.AddMoney(amount);
            }
            else
            {
                CrossSceneManager.instance.AddMana(amount);
            }
        }
        else
        {
            // Do nothing
        }
    }

    [PunRPC]
    public void AddResourcesShowAtSpecifiedPoint(bool forDefender, bool isMoney, int amount, Vector2 fromWhere)
    {
        if (CrossSceneManager.instance.amIDefender == forDefender)
        {
            if (isMoney)
            {
                CrossSceneManager.instance.AddMoney(amount, fromWhere);
            }
            else
            {
                CrossSceneManager.instance.AddMana(amount, fromWhere); //ShowManaChange is not implemented yet
            }
        }
        else
        {
            // Do nothing
        }
    }

    private void UpdateMatchHistory()
    {
        matchInfo.transform.Find("RoundHistory").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.PlayerFriendlyMatchHistory();
    }

    private void RoundEndCleaning(string matchHist, bool didMasterDefend)
    {
        // --------------------------------------------------------------
        // Reset both ready; Update match history;
        // Change defender; CSM SoftReset;
        // --------------------------------------------------------------
        print("Doing round end cleaning!");
        ExitGames.Client.Photon.Hashtable roundEndCleaning = new ExitGames.Client.Photon.Hashtable();
        roundEndCleaning.Add("isMasterReady", false);
        roundEndCleaning.Add("isJoinedReady", false);
        roundEndCleaning.Add("DidMasterWon", matchHist);
        roundEndCleaning.Add("isMasterDefending", !didMasterDefend);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roundEndCleaning);
        CrossSceneManager.instance.SoftReset();

    }

    private void ChangeSceneAfterNSeconds(int seconds, string sceneName, bool alsoLeaveRoom)
    {
        if (alsoLeaveRoom)
        {
            StartCoroutine(ChangeSceneDelayedAndLeaveRoom(seconds, sceneName));
        }
        else
        {
            StartCoroutine(ChangeSceneDelayed(seconds, sceneName));
        }
    }

    private IEnumerator AddMoneyPerSecond(int amount)
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            CrossSceneManager.instance.AddMoney(amount, topRight);
        }
    }

    private IEnumerator ChangeSceneDelayed(int seconds, string sceneName)
    {
        yield return new WaitForSeconds(seconds);
        PhotonNetwork.LoadLevel(sceneName);
    }

    private IEnumerator ChangeSceneDelayedAndLeaveRoom(int seconds, string sceneName)
    {
        yield return new WaitForSeconds(seconds);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(sceneName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        print("SOMEBODY LEFTT");
        isTimerRunning = false;
        // Disable round end and match end texts so its not overlapped
        GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(false);
        GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(false);
        attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false);
        attackerMatchResults.transform.Find("Win").gameObject.SetActive(false);
        defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false);
        defenderMatchResults.transform.Find("Win").gameObject.SetActive(false);
        // Enable parent canvas for player left text
        msgToPlayerCanvas.gameObject.SetActive(true);
        msgToPlayerCanvas.transform.Find("EnemyLeft").gameObject.SetActive(true);
        PhotonNetwork.LeaveRoom();
        StartCoroutine(GoBackAfterEnemyPlayerLeaves(3));
    }

    System.Collections.IEnumerator GoBackAfterEnemyPlayerLeaves(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("HostGame");
    }

    public void LeaveMatchButton()
    {
        isTimerRunning = false;
        msgToPlayerCanvas.gameObject.SetActive(true);
        msgToPlayerCanvas.transform.Find("EnemyLeft").gameObject.SetActive(true);
        msgToPlayerCanvas.transform.Find("EnemyLeft").gameObject.GetComponent<TMP_Text>().text = "You left";
        PhotonNetwork.LeaveRoom();
        StartCoroutine(GoBackButton(3));
    }

    private void UpdatePlayerStats()
    {
        // Both players have their own gold and mana, but only defender has health
        // We need to show defender health to both players, but attacker needs to see it as "Enemy health"
        if (amIDefender)
        {
            if (CrossSceneManager.instance.isMoneyInfinite)
            {
                string playerMoneyTextTemplate = "Gold: inf";
                d_playerMoneyTextField.text = playerMoneyTextTemplate;

            }
            else
            {
                string playerMoneyTextTemplate = "Gold: " + CrossSceneManager.instance.playerMoney;
                d_playerMoneyTextField.text = playerMoneyTextTemplate;
            }
            // ------------------------------------------------------------------------------------
            if (CrossSceneManager.instance.isManaInfinite)
            {
                //string playerManaTextTemplate = "Mana: Inf";
                string playerManaTextTemplate = "";
                d_playerManaTextField.text = playerManaTextTemplate;
            }
            else
            {
                //string playerManaTextTemplate = "Mana: " + CrossSceneManager.instance.playerMana;
                string playerManaTextTemplate = "";
                d_playerManaTextField.text = playerManaTextTemplate;
            }
            // ------------------------------------------------------------------------------------
            string enemyHealthTextTemplate = "Health: " + CrossSceneManager.instance.defenderHealth;
            d_enemyHealthTextField.text = enemyHealthTextTemplate;
        }
        else
        {
            if (CrossSceneManager.instance.isMoneyInfinite)
            {
                string playerMoneyTextTemplate = "Gold: inf";
                a_playerMoneyTextField.text = playerMoneyTextTemplate;

            }
            else
            {
                string playerMoneyTextTemplate = "Gold: " + CrossSceneManager.instance.playerMoney;
                a_playerMoneyTextField.text = playerMoneyTextTemplate;
            }
            if (CrossSceneManager.instance.isManaInfinite)
            {
                //string playerManaTextTemplate = "Mana: Inf";
                string playerManaTextTemplate = "";
                a_playerManaTextField.text = playerManaTextTemplate;
            }
            else
            {
                //string playerManaTextTemplate = "Mana: " + CrossSceneManager.instance.playerMana;
                string playerManaTextTemplate = "";
                a_playerManaTextField.text = playerManaTextTemplate;
            }
            string enemyHealthTextTemplate = "Enemy health: " + CrossSceneManager.instance.defenderHealth;
            a_enemyHealthTextField.text = enemyHealthTextTemplate;
        }
    }

    System.Collections.IEnumerator GoBackButton(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("HostGame");
    }

    // ----------------------------------------------------
    private void DestroyNode(GameObject node)
    {
        Destroy(node);
        nodesDestroyed++;
    }

    private void GenerateAndConfigureNodeMesh(Vector2 topleft, Vector2 bottomright)
    {
        nodesInstantiated = 0;
        if (nodePrefab != null)
        {
            Vector2 distance = (new Vector2(Mathf.Abs(topleft.x), Mathf.Abs(topleft.y))) - new Vector2(Mathf.Abs(bottomright.x), Mathf.Abs(bottomright.y));
            distance = new Vector2(Mathf.Abs(distance.x), Mathf.Abs(distance.y));
            for (float x = topleft.x; x <= distance.x + topleft.x; x++)
            {
                for (float y = bottomright.y; y <= bottomright.y + distance.y; y++)
                {
                    //it would be better to check if node is on
                    //forbidden tile, then skip that tile
                    GameObject singleNode = Instantiate(nodePrefab, new Vector3(x, y, shopNodesZOffset), Quaternion.identity);
                    singleNode.transform.SetParent(shopNodesCollection.transform);
                    //configure node right here and right now
                    //reduce number of nodes - we cant place them on:
                    //paths, obstacles, spawners, despawners,
                    //other turrets need to be configured to not let any more
                    //or they need to be upgradable
                    nodesInstantiated++;
                    nodes.Add(singleNode);
                }
            }


            // ---------------------------------------------------------------------------------------------
            // Remove nodes from obstacles
            // ---------------------------------------------------------------------------------------------
            //print("trying to remove nodes");
            //print(nodes);
            //print(obstacles[0].transform.position);
            foreach (GameObject node in nodes)
            {
                foreach (Transform obstacle in obstacles)
                {
                    //if(node.transform.position == obstacle.transform.position)
                    if (Mathf.Approximately(node.transform.position.x, obstacle.transform.position.x)
                        && Mathf.Approximately(node.transform.position.y, obstacle.transform.position.y))
                    {
                        DestroyNode(node);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Node prefabs were not set");
        }
        Debug.Log("shopNodes instantiated: " + nodesInstantiated + ", destroyed: " + nodesDestroyed + ",  left: " + (nodesInstantiated - nodesDestroyed));
    }

    private void GetPlayArenaCorners()
    {
        foreach (Transform child in playArenaCorners)
        {
            if (child.name == "topLeft")
            {
                if (isDebugging) { Debug.Log("got topleft:" + child.transform.position); }
                topLeft = child.transform.position;
            }
            else if (child.name == "topRight")
            {
                if (isDebugging) { Debug.Log("got topright"); }
                topRight = child.transform.position;
            }
            else if (child.name == "bottomLeft")
            {
                if (isDebugging) { Debug.Log("got botleft"); }
                bottomLeft = child.transform.position;
            }
            else if (child.name == "bottomRight")
            {
                if (isDebugging) { Debug.Log("got botright" + child.transform.position); }
                bottomRight = child.transform.position;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.currentTime);
            stream.SendNext(defenderHealthToSync);
            //stream.SendNext(CrossSceneManager.instance.MatchHistoryToString());
            //print("Sent time and health: " + defenderHealthToSync);
            //stream.SendNext(CrossSceneManager.instance.hasDefenderDied);
        }
        else
        {
            float _currentTime = (float)stream.ReceiveNext();
            // Basic lag compensation
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            this.currentTime = _currentTime - lag;
            CrossSceneManager.instance.defenderHealth = Mathf.FloorToInt((float)stream.ReceiveNext());
            //CrossSceneManager.instance.hasDefenderDied = (bool)stream.ReceiveNext();

            //print("Received time and health: " + defenderHealthToSync);
        }
    }

}
