using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ProjectileShieldController : MonoBehaviour
{
    TargetingSystem ts;

    public List<GameObject> projectiles = new List<GameObject>();
    Transform player;
    public GameObject target;

    public float rotateSpeed;
    Quaternion currentRotation;
    float zRotation;
    Vector3 upVector;

    public int targetCount;

    public GameObject projectilePrefab;

    [SerializeField] GameObject shieldProjectile1;
    [SerializeField] GameObject shieldProjectile2;
    [SerializeField] GameObject shieldProjectile3;

    [SerializeField] GameObject currentProjectile;

    [SerializeField] float shootForce;
    [SerializeField] float shootDelay;

    public bool hasTarget;

    private void Start()
    {
        ts = transform.root.GetComponent<TargetingSystem>();
        player = transform.root;
        CallProjectiles();
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.up * 10, Color.green);
        Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.blue);
        Debug.DrawLine(transform.position, transform.position + transform.right * 10, Color.red);


        currentRotation = transform.rotation;
        zRotation = this.transform.eulerAngles.z;
        upVector = transform.up;

        Debug.Log("z rotation: " + zRotation);

        shootDelay -= Time.deltaTime;

        if (shootDelay <= 0)
        {
            shootDelay = 0;
        }

        if (Input.GetButtonDown("Throw"))
        {
            if (projectiles.Count < 1)
            {
                CallProjectiles();
            }
        }

        if(Input.GetButtonUp("Throw"))
        {
            if (shootDelay <= 0)
            {
                if (hasTarget)
                {
                    if (ts.lockedOn)
                    {
                        //Debug.DrawLine(currentProjectile.transform.position, target.transform.position, Color.yellow, 10, false);
                        
                        LockOnShoot();
                    }
                    else ShootAtTargets();
                }
                else ShootProjectile();
            }
        }

        if (projectiles.Count > 0)
        {
            RotateProjectiles();
        }

        //Debug.Log("Projectiles Count: " + projectiles.Count);
    }

    void CallProjectiles()
    {
        Debug.Log("Get Projectiles");
        shootDelay = 0.5f;

        if (shieldProjectile1 == null)
        {
            shieldProjectile1 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, currentRotation);
            projectiles.Add(shieldProjectile1);

            ShieldProjectile SP = shieldProjectile1.GetComponent<ShieldProjectile>();
            SP.interactDelay = 0.1f;

            PositionInCircle();
        }

        if (shieldProjectile2 == null)
        {
            shieldProjectile2 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, currentRotation);
            projectiles.Add(shieldProjectile2);

            ShieldProjectile SP = shieldProjectile2.GetComponent<ShieldProjectile>();
            SP.interactDelay = 0.1f;

            PositionInCircle();
        }

        if (shieldProjectile3 == null)
        {
            shieldProjectile3 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, currentRotation);
            projectiles.Add(shieldProjectile3);

            ShieldProjectile SP = shieldProjectile3.GetComponent<ShieldProjectile>();
            SP.interactDelay = 0.1f;

            PositionInCircle();
        }
    }

    public void PositionInCircle()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            float radius = 1.5f;
            float angle = i * Mathf.PI * 2f / radius;
            Vector3 newPos = Vector3.zero;

            newPos = transform.position + (new Vector3(0, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));

            /*if (zRotation <= 90)
            {
                Debug.Log("Radius y axis");
                newPos = transform.position + (new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
            }
            else if(zRotation > 90)
            {
                Debug.Log("Radius x axis");
                newPos = transform.position + (new Vector3(0, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }*/

            projectiles[i].transform.position = newPos;
        }
    }

    void RotateProjectiles()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].transform.RotateAround(transform.position, upVector, rotateSpeed);
        }
    }

    void ShootProjectile()
    {
        Debug.Log("Shoot Projectile");

        currentProjectile = projectiles[0];
        currentProjectile.transform.parent = null;
        projectiles.Remove(currentProjectile);

        ShieldProjectile SP = currentProjectile.GetComponent<ShieldProjectile>();
        Rigidbody projRb = currentProjectile.GetComponent<Rigidbody>();

        projRb.isKinematic = false;

        projRb.AddForce(player.forward * shootForce, ForceMode.Impulse);
        SP.shot = true;


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

    void ShootAtTargets()
    {
        //Shoot each projectile at each assigned targets.
        Debug.Log("Shoot at Targets");

        for (int i = 0; i < ts.visibleTargets.Count; i++)
        {
            foreach (GameObject nextTarget in ts.visibleTargets) //Sets nextTarget in list to be target and move shield towards target.
            {
                target = nextTarget;
                Vector3 nextTargetPos = nextTarget.transform.position;

                currentProjectile = projectiles[0];
                currentProjectile.transform.parent = null;
                projectiles.Remove(currentProjectile);

                ShieldProjectile SP = currentProjectile.GetComponent<ShieldProjectile>();
                Rigidbody projRb = currentProjectile.GetComponent<Rigidbody>();

                projRb.isKinematic = false;

                Vector3 shootDir = (nextTargetPos - currentProjectile.transform.position).normalized;

                projRb.AddForce(shootDir * shootForce, ForceMode.Impulse);
                SP.shot = true;

                if (currentProjectile == shieldProjectile1)
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

        ts.visibleTargets.Clear();
        Debug.Log("Clear Visible Targets");
    }

    void LockOnShoot()
    {
        Debug.Log("Lock On Shoot");

        currentProjectile = projectiles[0];
        currentProjectile.transform.parent = null;
        projectiles.Remove(currentProjectile);

        ShieldProjectile SP = currentProjectile.GetComponent<ShieldProjectile>();
        Rigidbody projRb = currentProjectile.GetComponent<Rigidbody>();

        projRb.isKinematic = false;

        Vector3 lockOnDir = (target.transform.position - currentProjectile.transform.position).normalized;

        projRb.AddForce(lockOnDir * shootForce, ForceMode.Impulse);
        SP.shot = true;

        if (currentProjectile == shieldProjectile1)
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
