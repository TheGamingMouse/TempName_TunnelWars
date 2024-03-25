using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour, IDataPersistence
{
    #region Variables

    [Header("Ints")]
    int cIndex1;
    int cIndex2;
    int levelCount;
    int deathCount;

    [Header("Bools")]
    [SerializeField] bool watchCursor;

    [Header("Lists")]
    List<Color> colors1 = new();
    List<Color> colors2 = new();
    
    [Header("GameObjects")]
    [SerializeField] GameObject mainCam; // SerializeField is Important!
    [SerializeField] GameObject customCam; // SerializeField is Important!
    [SerializeField] GameObject playerLeftArrow; // SerializeField is Important!
    [SerializeField] GameObject playerRightArrow; // SerializeField is Important!
    [SerializeField] GameObject rifleLeftArrow; // SerializeField is Important!
    [SerializeField] GameObject rifleRightArrow; // SerializeField is Important!
    [SerializeField] GameObject playerModel; // SerializeField is Important!
    [SerializeField] GameObject rifleModel; // SerializeField is Important!
    [SerializeField] GameObject rifleModelPivot; // SerializeField is Important!
    GameObject playerObject;

    [Header("Buttons")]
    [SerializeField] Button continueButton;
    [SerializeField] Button loadButton;

    [Header("Vector3s")]
    Vector3 mousePos;
    Vector3 pressPoint;

    [Header("Quaternions")]
    Quaternion startRotation;
    Quaternion initialRotation;

    [Header("Images")]
    Image bodyColor;
    Image rifleColor;

    [Header("Colors")]
    Color c1 = Color.black;
    Color c2 = Color.blue;
    Color c3 = Color.cyan;
    Color c4 = Color.gray;
    Color c5 = Color.green;
    Color c6 = Color.magenta;
    Color c7 = Color.red;
    Color c8 = Color.white;
    Color c9 = Color.yellow;

    [Header("Components")]
    [SerializeField] SaveSlotsMenu saveSlotsMenu;
    
    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        mainCam.GetComponent<CinemachineVirtualCamera>().enabled = true;
        customCam.GetComponent<CinemachineVirtualCamera>().enabled = false;

        bodyColor = transform.Find("CustomizeMenu/PlayerCustom/PlayerCustomImage").GetComponentInChildren<Image>();
        rifleColor = transform.Find("CustomizeMenu/RifleCustom/RifleCustomImage").GetComponentInChildren<Image>();

        playerObject = GameObject.FindWithTag("Player");

        initialRotation = playerObject.transform.rotation;

        AddColorsToList();

        if (!DataPersistenceManager.Instance.HasGameData())
        {
            continueButton.interactable = false;
            loadButton.interactable = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        watchCursor = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (watchCursor)
        {
            FollowCursor();

            playerObject.transform.rotation = initialRotation;
        }
        else
        {
            playerModel.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            rifleModelPivot.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

            DragWithMouse();
        }

        UpdatePlayerColor();
        UpdateRifleColor();
    }
    
    #endregion

    #region Methods

    public void EnableCustomCam()
    {
        mainCam.GetComponent<CinemachineVirtualCamera>().enabled = false;
        customCam.GetComponent<CinemachineVirtualCamera>().enabled = true;

        watchCursor = false;
    }

    public void EnableMainCam()
    {
        mainCam.GetComponent<CinemachineVirtualCamera>().enabled = true;
        customCam.GetComponent<CinemachineVirtualCamera>().enabled = false;

        watchCursor = true;
    }

    void AddColorsToList()
    {
        colors1.Add(c1);
        colors1.Add(c2);
        colors1.Add(c3);
        colors1.Add(c4);
        colors1.Add(c5);
        colors1.Add(c6);
        colors1.Add(c7);
        colors1.Add(c8);
        colors1.Add(c9);

        colors2.Add(c1);
        colors2.Add(c2);
        colors2.Add(c3);
        colors2.Add(c4);
        colors2.Add(c5);
        colors2.Add(c6);
        colors2.Add(c7);
        colors2.Add(c8);
        colors2.Add(c9);
    }

    public void PlayerColorGoRight()
    {
        cIndex1++;
        if (cIndex1 > colors1.Count - 1)
        {
            cIndex1 = 0;
        }
    }

    public void PlayerColorGoLeft()
    {
        cIndex1--;
        if (cIndex1 < 0)
        {
            cIndex1 = colors1.Count - 1;
        }
    }

    public void RifleColorGoRight()
    {
        cIndex2++;
        if (cIndex2 > colors2.Count - 1)
        {
            cIndex2 = 0;
        }
    }

    public void RifleColorGoLeft()
    {
        cIndex2--;
        if (cIndex2 < 0)
        {
            cIndex2 = colors2.Count - 1;
        }
    }

    void UpdatePlayerColor()
    {
        bodyColor.color = colors1[cIndex1];
    }

    void UpdateRifleColor()
    {
        rifleColor.color = colors2[cIndex2];
    }

    void FollowCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            mousePos = hit.point;
        }
        
        Vector3 playerDirection = new(mousePos.x - playerModel.transform.position.x, playerModel.transform.position.y, playerModel.transform.position.z);
        playerModel.transform.forward = playerDirection;

        rifleModelPivot.transform.LookAt(mousePos);
    }

    void DragWithMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressPoint = Input.mousePosition;
            startRotation = playerObject.transform.rotation;
        }
        else if (Input.GetMouseButton(0))
        {
            float DistToNewMousePos = (pressPoint - Input.mousePosition).x;
            playerObject.transform.rotation = startRotation * Quaternion.Euler(Vector3.up * DistToNewMousePos);
        }
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(levelCount);
    }

    public void LoadGameMenu()
    {
        saveSlotsMenu.ActivateMenu(true);
    }

    public void NewGameMenu()
    {
        saveSlotsMenu.ActivateMenu(false);
    }
    
    // public void NewGame()
    // {
    //     DataPersistenceManager.Instance.NewGame();
    //     SceneManager.LoadSceneAsync("Level1");
    // }

    public void QuitGame()
    {
        Debug.Log("Quiter");
        Application.Quit();
    }

    public void LoadData(GameData data)
    {
        cIndex1 = data.playerColor;
        cIndex2 = data.rifleColor;
        levelCount = data.levelCount;
        deathCount = data.deathCount;
    }

    public void SaveData(ref GameData data)
    {
        data.playerColor = cIndex1;
        data.rifleColor = cIndex2;
    }

    #endregion
}
