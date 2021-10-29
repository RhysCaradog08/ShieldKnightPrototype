using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ProjectileShieldController : MonoBehaviour
{
    [SerializeField] List<GameObject> projectiles = new List<GameObject>();

    public float rotateSpeed;
    Quaternion currentRotation;

    [SerializeField] GameObject shieldProjectile1;
    [SerializeField] GameObject shieldProjectile2;
    [SerializeField] GameObject shieldProjectile3;

    [SerializeField] GameObject currentProjectile;

    [SerializeField] float shootForce;
    [SerializeField] float shootDelay;

    private void Start()
    {
        CallProjectiles();
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.up * 10, Color.green);
        Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.blue);
        Debug.DrawLine(transform.position, transform.position + transform.right * 10, Color.red);

        shootDelay -= Time.deltaTime;

        if (shootDelay <= 0)
        {
            shootDelay = 0;
        }

        if (Input.GetButtonDown("Throw"))
        {
            if (projectiles.Count < 1)
            {
                Debug.Log("Call Projectiles");

                CallProjectiles();
            }
            else
            {
                for (int i = 0; i < projectiles.Count; i++)
                {
                    currentProjectile = projectiles[i];
                }

                if (shootDelay <= 0)
                {
                    ShootProjectile();
                }
            }
        }

        if (projectiles.Count > 0)
        {
            RotateProjectiles();
        }
    }

    void CallProjectiles()
    {
        Debug.Log("Get Projectiles");
        shootDelay = 0.5f;

        if (shieldProjectile1 == null)
        {
            shieldProjectile1 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, Quaternion.identity);
            projectiles.Add(shieldProjectile1);
            PositionInCircle();
        }

        if (shieldProjectile2 == null)
        {
            shieldProjectile2 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, Quaternion.identity);
            projectiles.Add(shieldProjectile2);
            PositionInCircle();
        }

        if (shieldProjectile3 == null)
        {
            shieldProjectile3 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, Quaternion.identity);
            projectiles.Add(shieldProjectile3);
            PositionInCircle();
        }
    }

    public void PositionInCircle()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            float radius = 1.5f;
            float angle = i * Mathf.PI * 2f / radius;

            Vector3 newPos = transform.position + (new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));

            projectiles[i].transform.position = newPos;
        }
    }

    void RotateProjectiles()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].transform.RotateAround(transform.position, transform.up, rotateSpeed);
        }
    }

    void ShootProjectile()
    {
        currentProjectile.transform.parent = null;
        projectiles.Remove(currentProjectile);

        Rigidbody projRb = currentProjectile.GetComponentInChildren<Rigidbody>();

        projRb.isKinematic = false;
        projRb.AddForce(transform.forward * shootForce, ForceMode.Impulse);

        if(currentProjectile == shieldProjectile1)
        {
            shieldProjectile1 = null;
        }
        else if (currentProjectile == shieldProjectile2)
        {
            shieldProjectile2 = null;
        }
        else if (currentProjectile == shieldProjectile3)
        {
            shieldProjectile3 = null;
        }

        currentProjectile = null;
    }
}
