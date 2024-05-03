using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSytem;
using UnityEngine.UI;

// MERGED

public class PlayerController : MonoBehaviour
{
    // Move
    private Vector2 moveDir;
    private Rigidbody rigid;
    public int speed = 9;

    public float minSpinDistance;

    private GameObject spawnPoses;

    public GameObject enemies;

    private bool isCrawling;

    public bool isSpawning;

    public static PlayerController Instance;

    [Header("Projectile")]
    public float minPickUpDistance;
    public Slider projectileBar;
    public int forceChangeMultiple;
    private float shootingForce;
    private float addedForce = 0;
    private GameObject rock = null;
    private bool hasRock;
    private bool isShootingRock;
    public LineRenderer lineRenderer;
    public Transform projectileLight;
    private int linePointCount = 20;

    [Header("Item")]
    public bool hasKey;
    public bool hasFile;
    public ItemUI currentItemUI;
    public GameObject transquillizerPrefab;
    public GameObject minePrefab;

    public GameObject itemUsingInstruction;


    [Header("Stamina")]
    public Slider staminaSlider;
    private float stamina = 100;
    private bool isSpeeding;
    public int staminaChangeMultiple;

    public float Stamina
    {
        get => stamina;
        set
        {
            if (value > 100)
            {
                value = 100;
            }
            else if (value < 0)
            {
                value = 0;
            }
            stamina = value;
            staminaSlider.value = Stamina / 100;
            Color barColor = Color.green;
            if (stamina >= 0 && stamina <= 20)
            {
                barColor = Color.red;
            }
            else if (stamina > 20 && stamina <= 60)
            {
                barColor = Color.yellow;
            }
            else
            {
                barColor = Color.green;
            }
            staminaSlider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = barColor;
        }
    }

    public float ShootingForce
    {
        get => shootingForce;
        set
        {
            shootingForce = value;
            projectileBar.value = ShootingForce / 15;

        }
    }

    public bool IsCrawling
    {
        get => isCrawling;
        set
        {
            isCrawling = value;
            float Yscale = isCrawling ? 0.5f : 1;
            transform.localScale = new Vector3(1, Yscale, 1);
        }
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();

        Instance = this;
        // Singleton setup
        if (Instance == null)
        {
            Debug.LogError("Failed to set instance");
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps the player object consistent across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Ensures only one instance of this GameObject exists
            return;
        }

        spawnPoses = GameObject.Find("PlayerSpawnPoint").transform;
        if (staminaSlider == null)
        {
            Debug.LogError("Stamina slider is not assigned in the inspector.");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        staminaSlider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.green;
        transform.position = spawnPoses.position;
    }

    public void SetCrawling(bool crawling)
    {
        IsCrawling = crawling;
    }

    void Update()
    {
        MissionDisplay();
        HandleInput();
        Shooting();
        Crawling();

        /* MIGHT BE NEEDED
        if (currentItemUI != null)
        {
            itemUsingInstruction.gameObject.SetActive(true);
        }
        else
        {
            itemUsingInstruction.gameObject.SetActive(false);
        }
        */
    }


    void Shooting()
    {
        if (isShootingRock)
        {

            if (ShootingForce >= 15)
            {
                addedForce = -Time.deltaTime * forceChangeMultiple;
            }
            else if (ShootingForce <= 1)
            {
                addedForce = Time.deltaTime * forceChangeMultiple;
            }
            ShootingForce += addedForce;
            float g = Physics.gravity.magnitude;
            float zPos = 0;
            float initialVelocity = shootingForce / transform.GetComponent<Rigidbody>().mass;
            float flyTime = (initialVelocity / g) * 2;
            float zOffset = rock.transform.position.z;
            //    float tOffset = (-initialVelocity + Mathf.Sqrt(initialVelocity * initialVelocity + 2 * g * zOffset)) / g;
            //     flyTime += tOffset;
            zPos = initialVelocity * flyTime + 1.5f;
            projectileLight.transform.localPosition = new Vector3(0, 8, zPos);
            //     Projectileline();
        }
    }

    void Crawling()
    {
        if (IsCrawling)
        {
            Stamina -= Time.deltaTime * staminaChangeMultiple * 2;
            speed = 5;
            if (Stamina <= 0)
            {
                IsCrawling = false;
            }
        }
        else if (isSpeeding)
        {

            Stamina -= Time.deltaTime * staminaChangeMultiple;
            speed = 10;
            if (Stamina <= 0)
            {
                isSpeeding = false;
            }
        }
        else
        {
            Stamina += Time.deltaTime * staminaChangeMultiple;
            speed = 5;
        }
    }
    public void HandleInput()
    {
        // Merged addition.
        // Check for item use
        if (Input.GetMouseButtonDown(0) && currentItemUI != null && currentItemUI.item != null)
        {
            // Use the item
            currentItemUI.item.Use(gameObject); // Changing it to UseItem(currentItemUI) kind of works, but it is still not debugged. 

            // MAYBE - Manage item UI or inventory here
            currentItemUI.ClickItem(); // Need to adjust itemUI accordingly.
        }

        // Deselection
        if (Input.GetMouseButtonDown(1) && currentItemUI != null)
        {
            DeselectItemUI();
        }

        // Toggle inventory screen visibility
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        // Tranquilizer
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseTranquilizerGun();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlaceMine();
        }
    }

    private void DeselectItemUI()
    {
        currentItemUI.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        Cursor.SetCursor(default, Vector2.zero, CursorMode.Auto);
        currentItemUI = null;
    }


    void UseTranquilizerGun()
    {
        Debug.Log("Using tranquilizer");

        // Gets gun from inventory if available
        ItemObject tranquilizer = inventory.GetItem(ItemType.TranquilizerGun);
        if (tranquilizer != null)
        {
            tranquilizer.Use(gameObject);
        }
        else
        {
            Debug.Log("No tranquilizer found in inventory");
        }
    }

    void PlaceMine()
    {
        Debug.Log("Using mine");
        // Gets mine from inventory if available
        ItemObject mine = inventory.GetItem(ItemType.Mine);
        if (mine != null)
        {
            mine.Use(gameObject);
        }
        else
        {
            Debug.Log("No mine found in inventory");
        }
    }

    public void UseItem()
    {
        currentItemUI.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        currentItemUI.UseNum--;
        currentItemUI = null;
        Cursor.SetCursor(default, Vector2.zero, CursorMode.Auto);
    }

    private void Projectileline()
    {
        float initiateVelocity = ShootingForce / rock.GetComponent<Rigidbody>().mass;
        float yAccesserate = Physics.gravity.y / rock.GetComponent<Rigidbody>().mass;
        for (int i = 1; i < linePointCount; i++)
        {
            float zPos = initiateVelocity * i * Time.fixedDeltaTime * 50;
            float yPos = (initiateVelocity + (initiateVelocity - yAccesserate * i * Time.fixedDeltaTime * 50)) / 2 * Time.fixedDeltaTime * i * 50;
            Vector3 pos = transform.TransformDirection(new Vector3(rock.transform.position.x, yPos, zPos));
            lineRenderer.SetPosition(i, pos);
        }
    }

    public void ToggleInventory()
    {

        if (inventoryScreen.activeSelf)
        {
            Debug.Log("Invent");
            inventoryScreen.SetActive(false);
        }
        else
        {
            inventoryScreen.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        rigid.velocity = transform.TransformDirection(new Vector3(moveDir.x, rigid.velocity.y, moveDir.y) * speed);
    }

    public void Move(InputAction.CallbackContext input)
    {
        moveDir = input.ReadValue<Vector2>();
    }

    public void Spin(InputAction.CallbackContext input)
    {

        if (input.performed)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, minSpinDistance))
            {
                if (hit.transform.gameObject.tag == "Enemy")
                {
                    GetComponent<Renderer>().material.color = hit.transform.GetComponent<Renderer>().material.color;

                }
            }
        }
    }

    public void PickUp(InputAction.CallbackContext input)
    {

        if (input.performed)
        {
            if (rock != null && hasRock == false)
            {
                rock.transform.GetComponent<Rock>().SetOnPlayer();
                hasRock = true;
                projectileBar.gameObject.SetActive(true);
                projectileBar.value = 0;
                projectileLight.gameObject.SetActive(true);
                projectileLight.SetParent(transform);
                projectileLight.transform.localPosition = new Vector3(0, 8, 0);
            }
            else if (hasRock == true)
            {
                isShootingRock = true;
                ShootingForce = 0;

            }
        }

        if (input.canceled)
        {
            if (isShootingRock)
            {
                isShootingRock = false;
                hasRock = false;
                rock.transform.GetComponent<Rock>().SetOnPlayer();
                rock.transform.GetComponent<Rigidbody>().AddRelativeForce(0, ShootingForce, ShootingForce, ForceMode.Impulse);
                rock = null;
                projectileBar.gameObject.SetActive(false);
                projectileLight.transform.SetParent(null);
                projectileLight.gameObject.SetActive(false);
            }
        }

    }

    public void Crawl(InputAction.CallbackContext input)
    {
        if (input.performed && Stamina > 0)
        {
            IsCrawling = !IsCrawling;

        }
    }

    public void Speed(InputAction.CallbackContext input)
    {
        if (input.performed && Stamina > 0)
        {
            isSpeeding = true;
        }
        if (input.canceled)
        {
            isSpeeding = false;
        }
    }


    public void MissionDisplay()
    {
        // MissionText is set active in start.
        if (hasFile)
        {
            missionText.SetActive(false);
            escapeText.SetActive(true);
        }
        else
        {
            missionText.SetActive(true);
            escapeText.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Key
        if (other.tag == "Key")
        {
            hasKey = true;
            Destroy(other.gameObject);
        }

        // Rock
        if (other.tag == "Rock") // used to be ElseIf.
        {
            rock = other.gameObject;
        }

        // Files
        if (other.tag == "Files")
        {
            hasFile = true;
            Destroy(other.gameObject);
        }

        // Inventory. Checks for collision with items and moves them to inventory.
        var item = other.GetComponent<Item>();
        if (item)
        {
            inventory.AddItem(item.item, 1);
            Destroy(other.gameObject); // Destroy the item after picking it up
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Transform[] spawnPos = spawnPoses.transform.GetComponentsInChildren<Transform>();
            Transform[] es = enemies.transform.GetComponentsInChildren<Transform>();
            for (int i = 0; i < spawnPos.Length; i++)
            {
                for (int j = 0; j < es.Length; j++)
                {
                    if (Vector3.Distance(spawnPos[i].position, es[j].position) <= 2.5f)
                    {
                        break;
                    }
                    if (j == es.Length - 1)
                    {
                        transform.position = spawnPos[i].position;
                        isSpawning = true;
                        GetComponent<Renderer>().material.color = Color.black;
                        Invoke("ResetSpawn", 5);
                        return;
                    }

                }
            }

        }
    }

    private void ResetSpawn()
    {
        isSpawning = false;
        GetComponent<Renderer>().material.color = Color.gray;
    }

    // Clear the inventory when the application quits
    private void OnApplicationQuit()
    {
        inventory.Container.Clear();
    }
}






