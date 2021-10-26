using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileShieldController : MonoBehaviour
{
    [SerializeField]List<GameObject> projectiles = new List<GameObject>();

    public Transform holder;
    public float rotateSpeed;

    private void Start()
    {
        
    }

    private void Update()
    {
        foreach (GameObject pShield in GameObject.FindGameObjectsWithTag("PShield"))
        {
            if (pShield.activeInHierarchy && !projectiles.Contains(pShield))
            {
                projectiles.Add(pShield);
                PositionInCircle();
            }

            else if(!pShield.activeInHierarchy && projectiles.Contains(pShield))
            {
                projectiles.Remove(pShield);
                PositionInCircle();
            }
        }

            if (projectiles.Count > 0)
            {
                RotateProjectiles();
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
            projectiles[i].transform.RotateAround(holder.transform.position, Vector3.up, rotateSpeed);
        }
    }
}
