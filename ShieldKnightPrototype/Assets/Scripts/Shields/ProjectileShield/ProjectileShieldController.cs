using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ProjectileShieldController : MonoBehaviour
{
    TargetingSystem ts;

    public List<GameObject> projectiles = new List<GameObject>();
    Transform player;

    Quaternion currentRotation;
    float zRotation;
    Vector3 upVector;

    [Header("Projectiles")]
    public GameObject shieldProjectile1, shieldProjectile2, shieldProjectile3;

    [Header("Shooting")]
    public GameObject target;
    [SerializeField] GameObject currentProjectile;
    [SerializeField] float shootForce, shootDelay;

    [Header("Spiral Attack")]
    public GameObject spiral;

    [Header("Guarding")]
    [SerializeField] float rotateSpeed, idleRotSpeed, guardRotSpeed;
    public GameObject guardTrigger;

    [Header("Booleans")]
    public bool hasTarget;
    bool canShoot;

    void Awake()
    {
        ts = transform.root.GetComponent<TargetingSystem>();
        player = transform.root;       
        rotateSpeed = idleRotSpeed;
    }

    private void Start()
    {
        Invoke("CallProjectiles", 0.25f);
        guardTrigger.SetActive(false);
        canShoot = true;
    }

    private void Update()
    {
        /*Debug.DrawLine(transform.position, transform.position + transform.up * 10, Color.green);
        Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.blue);
        Debug.DrawLine(transform.position, transform.position + transform.right * 10, Color.red);*/

        currentRotation = transform.rotation;

        shootDelay -= Time.deltaTime;

        if (shootDelay <= 0)
        {
            shootDelay = 0;
        }

        if (target != null)
        {
            hasTarget = true;
        }
        else hasTarget = false;

        if (Input.GetButtonDown("Throw"))
        {
            if (projectiles.Count < 1)
            {
                CallProjectiles();
            }
        }

        if(Input.GetButtonUp("Throw"))
        {
            if (canShoot)
            {
                if (shootDelay <= 0)
                {
                    if (hasTarget)
                    {
                        if (ts.lockedOn)
                        {
                            LockOnShoot();
                        }
                        else ShootAtTargets();
                    }
                    else ShootProjectile();
                }
            }
        }

        if(Input.GetButtonUp("Barge"))
        {
            if(projectiles.Count == 3)
            {
                SpiralAttack();
            }
        }

        if (projectiles.Count > 0)
        {
            RotateProjectiles();
        }

        if(Input.GetButtonDown("Guard"))
        {
            if (projectiles.Count < 1)
            {
                CallProjectiles();
            }
        }

        if (Input.GetButton("Guard"))
        {
            canShoot = false;
            rotateSpeed = guardRotSpeed;

            if (projectiles.Count > 0)
            {
                guardTrigger.SetActive(true);
            }
            else guardTrigger.SetActive(false);
        }
        else
        {
            canShoot = true;
            rotateSpeed = idleRotSpeed;
            guardTrigger.SetActive(false);
        }
    }

    void CallProjectiles()
    {
        //Debug.Log("Get Projectiles");
        shootDelay = 0.5f;

        if (shieldProjectile1 == null)
        {
            shieldProjectile1 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, currentRotation);
            projectiles.Add(shieldProjectile1);

            ShieldProjectile SP = shieldProjectile1.GetComponent<ShieldProjectile>();
            SP.interactDelay = 0.1f;
        }

        if (shieldProjectile2 == null)
        {
            shieldProjectile2 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, currentRotation);
            projectiles.Add(shieldProjectile2);

            ShieldProjectile SP = shieldProjectile2.GetComponent<ShieldProjectile>();
            SP.interactDelay = 0.1f;
        }

        if (shieldProjectile3 == null)
        {
            shieldProjectile3 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, currentRotation);
            projectiles.Add(shieldProjectile3);

            ShieldProjectile SP = shieldProjectile3.GetComponent<ShieldProjectile>();
            SP.interactDelay = 0.1f;
        }

        PositionInCircle();
    }

    public void PositionInCircle()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            float radius = 1.5f;
            float angle = i * Mathf.PI * 2f / radius;
            Vector3 newPos = Vector3.zero;

            newPos = transform.position + (new Vector3(0, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));

            projectiles[i].transform.position = newPos;
        }
    }

    void RotateProjectiles()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            if (projectiles[i].transform.parent == this.transform)
            {
                projectiles[i].transform.RotateAround(transform.position, transform.up, rotateSpeed);
            }
        }
    }

    void ShootProjectile()
    {
        //Debug.Log("Shoot Projectile");

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
        ts.visibleTargets.Clear();
    }

    void ShootAtTargets()
    {
        //Shoot each projectile at each assigned targets.
        //Debug.Log("Shoot at Targets");

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
        Debug.Log("Clear Visible Targets");
        ts.visibleTargets.Clear();
        ts.closest = null;
        }

    }

    void LockOnShoot()
    {
        //Debug.Log("Lock On Shoot");

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

    void SpiralAttack()
    {
        SpiralAttack sa = FindObjectOfType<SpiralAttack>();

        Rigidbody rb = spiral.GetComponent<Rigidbody>();
        rb.isKinematic = false;

        for(int i = 0; i < projectiles.Count; i++)
        {
            projectiles[i].transform.parent = spiral.transform;
            projectiles[i].transform.RotateAround(spiral.transform.position, transform.up, rotateSpeed);
        }

        if(target != null)
        {
            Vector3 lockOnDir = (target.transform.position - rb.transform.position).normalized;
            sa.shot = true;
            rb.AddForce(lockOnDir * shootForce, ForceMode.Impulse);
        }
        else
        {
            sa.shot = true;
            rb.AddForce(transform.forward * shootForce, ForceMode.Impulse);
        }
    }
}
