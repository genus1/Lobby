using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SetupLocalPlayer : NetworkBehaviour {

    public Text namePrefab;
    public Text nameLabel;
    public Transform namePos;
    string textboxname = "";
    string colourboxname = "";
    public Slider healthPrefab;
    public Slider health;
    public GameObject explosion;
    NetworkStartPosition[] spawnPos;

    //Initialize sync on start
    public override void OnStartClient()
    {
        OnChangeName(pName);
        OnChangeColour(pColour);
        base.OnStartClient();
    }

    [Command]
    public void CmdChangeName(string newName){
        pName = newName;
        nameLabel.text = pName;
    }

    [SyncVar (hook = "OnChangeName")]
    public string pName = "player";

    void OnChangeName(string name) {
        pName = name;
        nameLabel.text = pName;
    }

    //Change Color Code

    [Command]
    public void CmdChangeColour(string newColour){
        SetColor(newColour);
    }

    [SyncVar(hook = "OnChangeColour")]
    public string pColour = "#ffffff";

    void OnChangeColour(string newColour){
        SetColor(newColour);
    }

    private void SetColor(string newColour){
        pColour = newColour;
        Renderer[] rends = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in rends){
            if (r.gameObject.name == "BODY")
                r.material.SetColor("_Color", ColorFromHex(pColour));
        }
    }

    //Credit for method:  http://answers.unity3d.com/questions/812240
    //hex for testing -- green:04BF3404 red: 9F121204 blue: 221E9004
    Color ColorFromHex(string hex){
        hex = hex.Replace("0x", "");
        hex = hex.Replace("#", "");
        byte a = 255;
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        if (hex.Length == 8){
            a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    //Change Health
    [Command]
    public void CmdChangeHealth(int hitValue)
    {
        healthValue += hitValue;
        health.value = healthValue;

        if (health.value <= 0) {
            GameObject e = Instantiate(explosion, this.transform.position, Quaternion.identity);
            NetworkServer.Spawn(e);
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            RpcRespawn();
            healthValue = 100;
        }
    }

    [SyncVar(hook = "OnChangeHealth")]
    public int healthValue = 100;

    void OnChangeHealth(int change)
    {
        healthValue = change;
        health.value = healthValue;
    }

    private void OnGUI()
    { 
        if (isLocalPlayer)
        {
            textboxname = GUI.TextField(new Rect(25, 15, 100, 25), textboxname);
            if (GUI.Button(new Rect(130, 15, 35, 25), "Set"))
                CmdChangeName(textboxname);

            colourboxname = GUI.TextField(new Rect(170, 15, 100, 25), colourboxname);
            if (GUI.Button(new Rect(275, 15, 35, 25), "Set"))
                CmdChangeColour(colourboxname);
        }
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        spawnPos = FindObjectsOfType<NetworkStartPosition>(); //fixes server respawn

        if (!isLocalPlayer) return;

        if(spawnPos != null && spawnPos.Length > 0)
        {
            this.transform.position = spawnPos[Random.Range(0, spawnPos.Length)].transform.position;
        }
    }

    void Awake()
    {
        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(namePrefab, Vector3.zero, Quaternion.identity) as Text;
        nameLabel.transform.SetParent(canvas.transform);

        health = Instantiate(healthPrefab, Vector3.zero, Quaternion.identity) as Slider;
        health.transform.SetParent(canvas.transform);
    }

    // Use this for initialization
    void Start () 
	{
		if(isLocalPlayer)
		{
			GetComponent<MyPlayerController>().enabled = true;
            CameraFollow360.player = this.gameObject.transform;
		}
		else
		{
			GetComponent<MyPlayerController>().enabled = false;
		}
	}

    public void OnDestroy()
    {
        if (nameLabel != null && health != null)
            Destroy(nameLabel.gameObject);
        if (health != null)
            Destroy(health.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isLocalPlayer && collision.gameObject.tag == "bullet")
        {
            CmdChangeHealth(-5);
        }
    }

    private void Update()
    {
        //determine if the object is inside the camera's viewing volume
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 &&
            screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        if (onScreen)
        {
            Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(namePos.position);
            nameLabel.transform.position = nameLabelPos;
            health.transform.position = nameLabelPos + new Vector3(0, 15, 0);
        } else
        {
            nameLabel.transform.position = new Vector3(-1000, -1000, 0);
            health.transform.position = new Vector3(-1000, -1000, 0);
        }     
    }


}
