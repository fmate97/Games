using TMPro;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Camera settings")]
    [SerializeField] public Camera mainCamera;
    [SerializeField] float cameraSpeed = 10f;
    [SerializeField] float cameraXMin = -5f;
    [SerializeField] float cameraXMax = 5f;
    [SerializeField] float cameraY = 10f;
    [SerializeField] float cameraZMin = -15f;
    [SerializeField] float cameraZMax = 15f;
    [Header("Player settings")]
    [SerializeField] int maxPlayerHP = 100;
    [SerializeField] int startGold = 100;
    [Header("UI settings")]
    [SerializeField] GameObject HPBar;
    [SerializeField] GameObject GoldBar;
    [SerializeField] GameObject NotEnoughGoldPanel;
    [SerializeField] TextMeshProUGUI SpiderNumber;
    [SerializeField] TextMeshProUGUI SkeletonNumber;
    [SerializeField] TextMeshProUGUI ShieldSkeletonNumber;
    [SerializeField] GameObject startButton;
    [Header("Enemey movement settings")]
    [SerializeField] float movementSpeed = 0.1f;
    [SerializeField] GameObject[] waypoints;
    [SerializeField] GameObject spiderEnemy;
    [SerializeField] int spiderSpawnNumber = 10;
    [SerializeField] GameObject skeletonEnemy;
    [SerializeField] int skeletonSpawnNumber = 10;
    [SerializeField] GameObject shieldSkeletonEnemy;
    [SerializeField] int shieldSkeletonSpawnNumber = 10;
    [SerializeField] float spawnTime = 5f;

    public bool _gamePause = false, _gameOver = false, _gameVictory = false;

    private bool _startSpawn = false;
    private int _spawnNumber = 0;
    private float _deltaTime = 0f;
    private string _enemyTag = "Enemy";
    private HPBarController _hpBarController;
    private GoldBarController _goldBarController;

    void Start()
    {
        _hpBarController = HPBar.GetComponent<HPBarController>();
        _hpBarController.HPBarInit(maxPlayerHP);
        _goldBarController = GoldBar.GetComponent<GoldBarController>();
        _goldBarController.GoldBarInit(startGold);

        _spawnNumber = spiderSpawnNumber + skeletonSpawnNumber + shieldSkeletonSpawnNumber;

        WaypointsFollower spiderWaypointFollower = spiderEnemy.GetComponent<WaypointsFollower>();
        spiderWaypointFollower.waypoints = waypoints;
        spiderWaypointFollower.movementSpeed = movementSpeed;
        spiderEnemy.GetComponent<EnemyController>().levelController = this;

        WaypointsFollower skeletonWaypointFollower = skeletonEnemy.GetComponent<WaypointsFollower>();
        skeletonWaypointFollower.waypoints = waypoints;
        skeletonWaypointFollower.movementSpeed = movementSpeed;
        skeletonEnemy.GetComponent<EnemyController>().levelController = this;

        WaypointsFollower shieldSkeletonWaypointFollower = shieldSkeletonEnemy.GetComponent<WaypointsFollower>();
        shieldSkeletonWaypointFollower.waypoints = waypoints;
        shieldSkeletonWaypointFollower.movementSpeed = movementSpeed;
        shieldSkeletonEnemy.GetComponent<EnemyController>().levelController = this;

        mainCamera.transform.position = new Vector3((cameraXMax + cameraXMin) / 2, cameraY, cameraZMin);

        NotEnoughGoldPanel.SetActive(false);
        SetEnemyNumberInUI();
    }

    void Update()
    {
        if (Time.timeScale != 0)
        {
            _deltaTime -= Time.deltaTime;
            if (_startSpawn && _deltaTime <= 0f && _spawnNumber > 0)
            {
                SpawnEnemey();
                _deltaTime = spawnTime;
            }

            CameraMove();

            if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                _gamePause = true;
            }

            if (_spawnNumber == 0)
            {
                GameVictory();
            }
        }
    }

    void SpawnEnemey()
    {
        bool successfulSpawn = false;
        do
        {
            switch(Random.Range(0, 3))
            {
                case 0:
                    if(spiderSpawnNumber > 0)
                    {
                        Instantiate(spiderEnemy, waypoints[0].transform.position, Quaternion.identity);
                        spiderSpawnNumber--;
                        successfulSpawn = true;
                    }
                    break;
                case 1:
                    if (skeletonSpawnNumber > 0)
                    {
                        Instantiate(skeletonEnemy, waypoints[0].transform.position, Quaternion.identity);
                        skeletonSpawnNumber--;
                        successfulSpawn = true;
                    }
                    break;
                case 2:
                    if (shieldSkeletonSpawnNumber > 0)
                    {
                        Instantiate(shieldSkeletonEnemy, waypoints[0].transform.position, Quaternion.identity);
                        shieldSkeletonSpawnNumber--;
                        
                        successfulSpawn = true;
                    }
                    break;
            }
        }
        while (!successfulSpawn);

        _spawnNumber--;
        SetEnemyNumberInUI();
    }

    void CameraMove()
    {
        Vector3 cameraTranslate = Vector3.zero;

        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            && mainCamera.transform.position.z <= cameraZMax)
        {
            cameraTranslate += (Vector3.up * cameraSpeed * Time.deltaTime);
        }

        if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            && mainCamera.transform.position.z >= cameraZMin)
        {
            cameraTranslate += (Vector3.down * cameraSpeed * Time.deltaTime);
        }

        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            && mainCamera.transform.position.x >= cameraXMin)
        {
            cameraTranslate += (Vector3.left * cameraSpeed * Time.deltaTime);
        }

        if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            && mainCamera.transform.position.x <= cameraXMax)
        {
            cameraTranslate += (Vector3.right * cameraSpeed * Time.deltaTime);
        }

        mainCamera.transform.Translate(cameraTranslate);
    }

    public void AddGold(int gold)
    {
        _goldBarController.AddGold(gold);
    }

    public bool BuyWithGold(int gold)
    {
        if (_goldBarController.BuyWithGold(gold))
        {
            return true;
        }
        else
        {
            NotEnoughGoldPanel.SetActive(true);
            Invoke("NotEnoughGoldPanelDeactive", 2f);
            return false;
        }
    }

    public void NotEnoughGoldPanelDeactive()
    {
        NotEnoughGoldPanel.SetActive(false);
    }

    public void GetHit(int hitDMG)
    {
        if (!_hpBarController.GetHit(hitDMG))
        {
            _gameOver = true;
        }
    }

    void GameVictory()
    {
        if(GameObject.FindGameObjectsWithTag(_enemyTag).Length == 0)
        {
            _gameVictory = true;
        }
    }

    void SetEnemyNumberInUI()
    {
        SpiderNumber.text = $"x{spiderSpawnNumber}";
        SkeletonNumber.text = $"x{skeletonSpawnNumber}";
        ShieldSkeletonNumber.text = $"x{shieldSkeletonSpawnNumber}";
    }

    public void StartSpawn()
    {
        _startSpawn = true;
        startButton.SetActive(false);
    }
}
