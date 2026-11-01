using System.Collections.Generic;
using UnityEngine;

public class MagicMirror : UsableItem
{
    //Reference
    [SerializeField] private Camera playerCamera;

    //Detection Settings
    [SerializeField] private float detectionDistance = 20f;
    [SerializeField] private float detectionRadius = 8f;

    //Scanning settings
    [SerializeField] private float scanningRange = 2f;

    //Scanned enemies
    private HashSet<GameObject> scannedEnemies = new HashSet<GameObject>();

    public override void UseItem()
    {
        base.UseItem();

        playerCamera = Camera.main;

        if(playerCamera == null)
        {
            Debug.LogWarning("Magic Mirror: Player camera not assigned");
            return;
        }

        GameObject enemy = DetectEnemy();

        if(enemy == null)
        {
            Debug.LogWarning("No enemies detetcted");
            return;
        }

        float distance = Vector3.Distance(playerCamera.transform.position, enemy.transform.position);
        if(scannedEnemies.Contains(enemy))
        {
            Debug.Log("Enemy Already scanned");
            return;
        }

        if(distance >scanningRange)
        {
            Debug.Log("Enemy detected, but too far away to scan");
            return;
        }

        scannedEnemies.Add(enemy);

        Debug.Log("Successfully scanned: " + enemy.name);
    }

    private GameObject DetectEnemy()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Enemy") || hit.collider.transform.root.CompareTag("Enemy"))
            {
                return hit.collider.transform.root.CompareTag("Enemy") ? hit.collider.transform.root.gameObject : hit.collider.gameObject;
            }
        }

        Collider[] nearbyObjects = Physics.OverlapSphere(playerCamera.transform.position, detectionRadius);

        GameObject closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in nearbyObjects)
        {
            GameObject enemy = col.transform.root.CompareTag("Enemy") ? col.transform.root.gameObject : col.CompareTag("Enemy") ? col.gameObject : null;

            if (enemy == null) continue;

            float distance = Vector3.Distance(playerCamera.transform.position, enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }
}
