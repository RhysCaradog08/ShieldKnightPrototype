using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ProjectileShieldController : MonoBehaviour
{
    [Header("Projectiles")]
    public List<GameObject> projectiles = new List<GameObject>();
    public GameObject shieldProjectile1, shieldProjectile2, shieldProjectile3;
    Quaternion currentRotation;
    float zRotation;
    Vector3 upVector;

    [Header("Shooting")]
    PlayerController pc;
    TargetingSystem ts;
    ShieldProjectile SP;
    Transform player;
    public GameObject target;
    [SerializeField] GameObject currentProjectile;
    [SerializeField] float shootDelay;
    public float shootForce;
    GameObject hitStars;
    public bool hasTarget, aiming;
    bool canShoot;

    [Header("Spiral Attack")]
    public GameObject spiral;

    [Header("Guarding")]
    [SerializeField] float rotateSpeed, idleRotSpeed, guardRotSpeed;
    public GameObject guardTrigger;

    void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
        ts = FindObjectOfType<TargetingSystem>();
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
                    if (ts.visibleTargets.Count > 1)
                    {
                        StartCoroutine(ShootAtConsecutiveTargets());
                    }
                    else
                    {
                        ShootProjectile();
                        ts.visibleTargets.Clear();
                        pc.aiming = false;
                    }
                    /*if (ts.lockedOn)
                    {
                        LockOnShoot();
                    }
                    else
                    {
                    }*/
                }
            }

            if(currentProjectile != null)
            {
                ClearCurrentProjectile();
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

            SP = shieldProjectile1.GetComponent<ShieldProjectile>();
            SP.interactDelay = 0.05f;
        }

        if (shieldProjectile2 == null)
        {
            shieldProjectile2 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, currentRotation);
            projectiles.Add(shieldProjectile2);

            SP = shieldProjectile2.GetComponent<ShieldProjectile>();
            SP.interactDelay = 0.05f;
        }

        if (shieldProjectile3 == null)
        {
            shieldProjectile3 = ObjectPoolManager.instance.CallObject("ShieldProjectile", this.transform, this.transform.position, currentRotation);
            projectiles.Add(shieldProjectile3);

            SP = shieldProjectile3.GetComponent<ShieldProjectile>();
            SP.interactDelay = 0.05f;
        }

        PositionInCircle();
    }

    void ClearCurrentProjectile()
    {
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

    void ClearAllProjectiles()
    {
        Debug.Log("Clear All Projectiles");

        if(shieldProjectile1 != null)
        {
            SP = shieldProjectile1.GetComponent<ShieldProjectile>();
            SP.shot = false;

            ObjectPoolManager.instance.RecallObject(shieldProjectile1);
            shieldProjectile1 = null;
        }

        if (shieldProjectile2 != null)
        {
            SP = shieldProjectile2.GetComponent<ShieldProjectile>();
            SP.shot = false;

            ObjectPoolManager.instance.RecallObject(shieldProjectile2);
            shieldProjectile2 = null;
        }

        if (shieldProjectile3 != null)
        {
            SP = shieldProjectile3.GetComponent<ShieldProjectile>();
            SP.shot = false;

            ObjectPoolManager.instance.RecallObject(shieldProjectile3);
            shieldProjectile3 = null;
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

        pc.anim.SetTrigger("Shoot");
        currentProjectile = projectiles[0];
        currentProjectile.transform.parent = null;
        projectiles.Remove(currentProjectile);

        ShieldProjectile SP = currentProjectile.GetComponent<ShieldProjectile>();

        if (hasTarget)
        {
            SP.target = target;
        }

        SP.shot = true;
    }

    IEnumerator ShootAtConsecutiveTargets()
    {
        //Shoot each projectile at each assigned targets.
        //Debug.Log("Shoot at Targets");
        while (projectiles.Count > 0)
        {
            //pc.aiming = true;
            for (int i = ts.visibleTargets.Count - 1; i >= 0; i--) 
            {
                target = ts.visibleTargets[i];

                ShootProjectile();

                if (currentProjectile != null)
                {
                    ClearCurrentProjectile();
                }

                ts.visibleTargets.RemoveAt(i);

                yield return new WaitForSeconds(0.25f);
            }

            if(projectiles.Count != ts.visibleTargets.Count)
            {
                projectiles.Clear();
                ClearAllProjectiles();
            }
        }

        Debug.Log("Clear Visible Targets");
        pc.aiming = false;
        ts.visibleTargets.Clear();
        ts.closest = null;
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
