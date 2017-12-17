using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FireControl : NetworkBehaviour {

    public GameObject bulletPrefab;
    public GameObject bulletSpawn;

    private float lastShot, shotDelay = 2.0f;  //Delay one second between shots

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown("space"))
        {
            if (Time.time - lastShot > shotDelay)
            {
                lastShot = Time.time;
                CmdShoot();
            }
            
        }
    }

    [Command]
    private void CmdShoot()
    {
        CreateBullet();
        RpcCreateBullet();
    }

    [ClientRpc]
    void RpcCreateBullet()
    {
        if (!isServer) CreateBullet();
    }

    private void CreateBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        Quaternion q = Quaternion.FromToRotation(Vector3.up, transform.forward);
        bullet.transform.rotation = q * bullet.transform.rotation;
        //bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.transform.forward * 4000);
        bullet.GetComponent<Rigidbody>().velocity = bulletSpawn.transform.forward * 75;

        Destroy(bullet, 2.0f);
    }
}
