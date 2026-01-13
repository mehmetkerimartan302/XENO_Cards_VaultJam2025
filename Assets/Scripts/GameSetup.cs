using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetup = true;
    
    void Awake()
    {
        if (autoSetup)
            SetupGame();
    }

    void SetupGame()
    {
        if (SpaceEnvironment.Instance == null && FindAnyObjectByType<SpaceEnvironment>() == null)
        {
            GameObject spaceObj = new GameObject("SpaceEnvironment");
            spaceObj.AddComponent<SpaceEnvironment>();
        }
        
        if (BoardManager.Instance == null && FindAnyObjectByType<BoardManager>() == null)
        {
             GameObject boardObj = new GameObject("BoardManager_AUTO_CREATED");
             boardObj.transform.position = new Vector3(0, -0.478f, 0);
             boardObj.AddComponent<BoardManager>();
        }
        
        if (DeckManager.Instance == null && FindAnyObjectByType<DeckManager>() == null)
        {
            GameObject deckObj = new GameObject("DeckManager");
            deckObj.AddComponent<DeckManager>();
        }
        
        if (GameManager.Instance == null && FindAnyObjectByType<GameManager>() == null)
        {
            GameObject gameObj = new GameObject("GameManager");
            gameObj.AddComponent<GameManager>();
        }
        
        if (CombatManager.Instance == null && FindAnyObjectByType<CombatManager>() == null)
        {
            GameObject combatObj = new GameObject("CombatManager");
            combatObj.AddComponent<CombatManager>();
        }
        
        if (EnemyAI.Instance == null && FindAnyObjectByType<EnemyAI>() == null)
        {
            GameObject enemyObj = new GameObject("EnemyAI");
            enemyObj.AddComponent<EnemyAI>();
        }
        
        if (CellVisualManager.Instance == null && FindAnyObjectByType<CellVisualManager>() == null)
        {
            GameObject visualObj = new GameObject("CellVisualManager");
            visualObj.AddComponent<CellVisualManager>();
        }
        
        if (FindAnyObjectByType<AutoUISetup>() == null)
        {
            GameObject uiObj = new GameObject("AutoUISetup");
            uiObj.AddComponent<AutoUISetup>();
        }
        
        SetupCamera();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 1.6f, -6.5f);
            cam.transform.rotation = Quaternion.Euler(12, 0, 0);
            cam.fieldOfView = 70;
        }
    }
}

