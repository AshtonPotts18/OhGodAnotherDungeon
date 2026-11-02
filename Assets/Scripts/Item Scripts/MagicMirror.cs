using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagicMirror : UsableItem
{
    //Reference
    [SerializeField] private Camera playerCamera;

    //Detection Settings
    [SerializeField] private float detectionDistance = 20f;
    [SerializeField] private float detectionRadius = 8f;

    //Ui
    [SerializeField] private Text MirrorText;
    //Scanning settings
    [SerializeField] private float scanningRange = 2f;

    //Scanned enemies
    private HashSet<GameObject> scannedEnemies = new HashSet<GameObject>();

    private void Start()
    {
        if(MirrorText != null)
        {
            MirrorText.gameObject.SetActive(false);
        }
    }

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
            DisplayMessage("No enemies detected");
            return;
        }

        float distance = Vector3.Distance(playerCamera.transform.position, enemy.transform.position);

        DisplayMessage("Enemy detected!\nDistance: " + distance.ToString("F1") + " meters");

        if (scannedEnemies.Contains(enemy))
        {
            DisplayMessage("Enemy Already scanned");
            return;
        }

        if(distance > scanningRange)
        {
            DisplayMessage("Enemy too far to scan!\nDistance: " + distance.ToString("F1") + " meters");
            return;
        }

        scannedEnemies.Add(enemy);

        DisplayMessage("Successfully scanned: " + enemy.name);
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
            GameObject enemy = GetEnemy(col);

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

    private GameObject GetEnemy(Collider col)
    {
        if(col.CompareTag("Enemy"))
        {
            return col.gameObject;
        }

        Transform root = col.transform.root;

        if (col.CompareTag("Enemy"))
        {
            return root.gameObject;
        }

        return null;
    }

    private void DisplayMessage(string message)
    {
        Debug.Log(message);

        if(MirrorText != null)
        {
            MirrorText.text = message;
            MirrorText.gameObject.SetActive(true);
        }
    }
}
