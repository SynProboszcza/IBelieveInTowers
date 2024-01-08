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
    public float currentTime;
    public float defenderHealthToSync;
    private bool isTimerRunning = false;
    [Header("Attacker parts")]
    public TMP_Text a_timer;
    public TMP_Text a_enemyHealthTextField;
    public TMP_Text a_playerMoneyTextField;
    public TMP_Text a_playerManaTextField;
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

    // Parts de- and activating, setting match dur.
    void Start()
    {
        // Check if defending, then activate proper part
        attackerPart.gameObject.SetActive(false);
        defenderPart.gameObject.SetActive(false);
        attackerMatchResults.gameObject.SetActive(false);
        attackerMatchResults.transform.Find("Win").gameObject.SetActive(false);
        attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false);
        defenderMatchResults.gameObject.SetActive(false);
        defenderMatchResults.transform.Find("Win").gameObject.SetActive(false);
        defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false);
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
        // HACK: comment roundtimeseconds to use SIMPLEConnect
        // =====================================================================================================================================================
        roundTimeSeconds = CrossSceneManager.instance.currentMatchMaxTime;
        currentTime = roundTimeSeconds;

        // start the timer, syncing is done by sending current time and compensating lag
        isTimerRunning = true;

    }

    void Update()
    {
        // Stream is writing directly to CSM
        defenderHealthToSync = CrossSceneManager.instance.defenderHealth;
        UpdatePlayerStats();
        // Check if defending when ded
        //  checking is done when dealing damage
        if (CrossSceneManager.instance.hasDefenderDied && !matchResultsShown)
        {
            RoundEnd(amIDefender, true);
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
                        moneyPerSecond = StartCoroutine(AddMoneyPerSecond(a_moneyPerSecond));
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
                RoundEnd(amIDefender, false);
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
        // Called only when time ends or defender died

        // -----------------------------------------------------------------------
        // Stop current round and add result
        // -----------------------------------------------------------------------
        isTimerRunning = false;
        if (moneyPerSecond != null)
        {
            StopCoroutine(moneyPerSecond);
        }
        print("adding result");
        CrossSceneManager.instance.didDefenderWin.Add(!didDefenderDie);
        print("added result");
        // TODO: send rpc to sync
        // Code <bools> into string like "tft"
        string defenderWins = "";
        for (int i = 0; i < CrossSceneManager.instance.didDefenderWin.Count; i++)
        {
            if (CrossSceneManager.instance.didDefenderWin[i])
            {
                defenderWins += "t";
            } else
            {
                defenderWins += "f";
            }
        }
        print("Sending defender wins to sync: " + defenderWins);
        gameObject.GetComponent<PhotonView>().RPC("SyncRoundResults", RpcTarget.Others, defenderWins);


        // -----------------------------------------------------------------------
        // Check if we finish match or go into next round and do it
        // -----------------------------------------------------------------------
        if (CrossSceneManager.instance.didDefenderWin.Count == 1)
        {
            NextRound(amIDefending, didDefenderDie);
        }
        else if (CrossSceneManager.instance.didDefenderWin.Count == 2)
        {
            // check if somebody won
            //  true: finish match 
            //  false: next round
            if (CrossSceneManager.instance.didDefenderWin[0] == CrossSceneManager.instance.didDefenderWin[1])
            {
                FinishMatch(amIDefending);
            }
            else
            {
                NextRound(amIDefending, didDefenderDie);
            }
        }
        else if (CrossSceneManager.instance.didDefenderWin.Count == 3)
        {
            // finish match
            FinishMatch(amIDefending);
        }
        else
        {
            // never should be
            Debug.LogError("SOMETHING WENT CRAZY WRONG");
            Debug.LogError("SOMETHING WENT CRAZY WRONG");
            Debug.LogError("SOMETHING WENT CRAZY WRONG");
            print("CrossSceneManager.instance.didDefenderWin.Count == " + CrossSceneManager.instance.didDefenderWin.Count);
        }

        /*
        // // // =====================================================================================
        // // // =====================================================================================
        // // string sceneName = "HostGame";
        // // if (CrossSceneManager.instance.didDefenderWin.Count < 3)
        // // {
        // //     if (CrossSceneManager.instance.didDefenderWin.Count == 2 && (CrossSceneManager.instance.didDefenderWin[0] == CrossSceneManager.instance.didDefenderWin[1]))
        // //     {
        // //         // somebody won by 2:0
        // //         if (amIDefending)
        // //         {
        // //             if (didDefenderDie)
        // //             {
        // //                 // i defender died
        // //                 GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
        // //                 PhotonNetwork.LeaveRoom();
        // //                 ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "MainMenu");
        // //                 return;
        // //             }
        // //             else
        // //             {
        // //                 // i defender won by time
        // //                 GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
        // //                 PhotonNetwork.LeaveRoom();
        // //                 ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "MainMenu");
        // //                 return;
        // //             }
        // //         }
        // //         else
        // //         {
        // //             if (didDefenderDie)
        // //             {
        // //                 // i attacker won by killing defender
        // //                 GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
        // //                 PhotonNetwork.LeaveRoom();
        // //                 ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "MainMenu");
        // //                 return;
        // //             }
        // //             else
        // //             {
        // //                 // i attacker lost by time
        // //                 GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
        // //                 PhotonNetwork.LeaveRoom();
        // //                 ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "MainMenu");
        // //                 return;
        // //             }
        // //         }
        // //     }
        // //     sceneName = "InBetweenScene";
        // // }
        // // else if (CrossSceneManager.instance.didDefenderWin.Count >= 3)
        // // {
        // //     sceneName = "MainMenu";
        // // }
        // // if (amIDefending)
        // // {
        // //     if (didDefenderDie)
        // //     {
        // //         // i defender died
        // //         // We don't activate attacker/defender parts, we expect the proper one to be active
        // //         defenderMatchResults.SetActive(true);
        // //         defenderMatchResults.transform.Find("Win").gameObject.SetActive(false); // fallback
        // //         defenderMatchResults.transform.Find("Loose").gameObject.SetActive(true);
        // //         print("I lost by deadly death, going back to host game after " + secondsToWaitAfterGameEnd + " seconds");
        // //         ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName);
        // //     }
        // //     else
        // //     {
        // //         // i defender won by time
        // //         // We don't activate attacker/defender parts, we expect the proper one to be active
        // //         defenderMatchResults.SetActive(true);
        // //         defenderMatchResults.transform.Find("Win").gameObject.SetActive(true); // fallback
        // //         defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false);
        // //         print("I won by surviving, going back to host game after " + secondsToWaitAfterGameEnd + " seconds");
        // //         ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName);
        // //     }
        // // }
        // // else
        // // {
        // //     if (didDefenderDie)
        // //     {
        // //         // i attacker won by killing defender
        // //         attackerMatchResults.SetActive(true);
        // //         attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false); // fallback
        // //         attackerMatchResults.transform.Find("Win").gameObject.SetActive(true);
        // //         print("I won by killing defender, going back to host game after " + secondsToWaitAfterGameEnd + " seconds");
        // //         ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName);
        // //     }
        // //     else
        // //     {
        // //         // i attacker lost by time passing
        // //         attackerMatchResults.SetActive(true);
        // //         attackerMatchResults.transform.Find("Loose").gameObject.SetActive(true); // fallback
        // //         attackerMatchResults.transform.Find("Win").gameObject.SetActive(false);
        // //         print("I lost by time, going back to host game after " + secondsToWaitAfterGameEnd + " seconds");
        // //         ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName);
        // //     }
        // // }
        // // // =====================================================================================
        // // // =====================================================================================
        */
    }

    private void NextRound(bool amIDefending, bool didDefenderDie)
    {
        // next:
        //  show text won/lost
        //  change scene to InBetweenScene
        //  roundEndCleaning()
        // 
        // This does not add anything to CSM, only soft reset
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
        RoundEndCleaning();

    }

    private void FinishMatch(bool amIDefending)
    {
        // finish match: 
        //  show text won/lost match
        //  leave room
        //  CSM full reset
        //  change scene to main menu

        string sceneName = "MainMenu";
        bool didDefenderWin = CrossSceneManager.instance.didDefenderWin[0];
        if (amIDefending)
        {
            if (didDefenderWin)
            {
                // i defender won by time
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchWon;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
            else
            {
                // i defender died
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchLost;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
        }
        else
        {
            if (didDefenderWin)
            {
                // i attacker lost by time
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchLost;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
            else
            {
                // i attacker won by killing defender
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.GetComponent<TMP_Text>().text = CrossSceneManager.instance.matchWon;
                GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
                //PhotonNetwork.LeaveRoom(); // main menu leaves room on its own
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName, true);
            }
        }
    }

    private void RoundEndCleaning()
    {
        // roundEndCleaning():
        //  setCustomProps both not ready
        //  CSM soft reset
        ExitGames.Client.Photon.Hashtable roundEndCleaning = new ExitGames.Client.Photon.Hashtable();
        roundEndCleaning.Add("isMasterReady", false);
        roundEndCleaning.Add("isJoinedReady", false);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roundEndCleaning);
        CrossSceneManager.instance.ResetInBetweenRounds();

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
                string playerManaTextTemplate = "Mana: Inf";
                d_playerManaTextField.text = playerManaTextTemplate;
            }
            else
            {
                string playerManaTextTemplate = "Mana: " + CrossSceneManager.instance.playerMana;
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
                string playerManaTextTemplate = "Mana: Inf";
                a_playerManaTextField.text = playerManaTextTemplate;
            }
            else
            {
                string playerManaTextTemplate = "Mana: " + CrossSceneManager.instance.playerMana;
                a_playerManaTextField.text = playerManaTextTemplate;
            }
            string enemyHealthTextTemplate = "Enemy health: " + CrossSceneManager.instance.defenderHealth;
            a_enemyHealthTextField.text = enemyHealthTextTemplate;
        }
    }

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
            //early return
            Debug.LogError("Received incorrect wins list: " + defenderWins, gameObject);
            Debug.LogError("leaving");
            PhotonNetwork.Disconnect();
            ChangeSceneAfterNSeconds(5, "HostGame", false);
        }
        CrossSceneManager.instance.didDefenderWin.Clear();
        print("cleared list of wins");
        for (int i = 0; i < defenderWins.Length; i++)
        {
            if (defenderWins[i].Equals('t')) // needs to be '' because its char, not string
            {
                print("added win for defender: true");
                CrossSceneManager.instance.didDefenderWin.Add(true);
            } else if (defenderWins[i].Equals('f'))
            {
                print("added loose for defender: false");
                CrossSceneManager.instance.didDefenderWin.Add(false);
            } else
            {
                Debug.LogError("what have you passed on this cursed land: \"" + defenderWins[i] + "\" of length: " + defenderWins.Length, gameObject);
            }
        }

        //CrossSceneManager.instance.didDefenderWin.Add(firstMatchResult);
    }

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

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        print("SOMEBODY LEFTT");
        isTimerRunning = false;
        msgToPlayerCanvas.gameObject.SetActive(true);
        msgToPlayerCanvas.transform.Find("EnemyLeft").gameObject.SetActive(true);
        PhotonNetwork.LeaveRoom();
        StartCoroutine(GoBackAfterEnemyPlayerLeaves(3));
        base.OnPlayerLeftRoom(otherPlayer);
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
            //print("Sent time and health: " + defenderHealthToSync);
        }
        else
        {
            float _currentTime = (float)stream.ReceiveNext();
            // Basic lag compensation
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            this.currentTime = _currentTime - lag;
            CrossSceneManager.instance.defenderHealth = Mathf.FloorToInt((float)stream.ReceiveNext());
            //print("Received time and health: " + defenderHealthToSync);
        }
    }

}
