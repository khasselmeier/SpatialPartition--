using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace SpatialPartitionPattern
{
    public class GameController : MonoBehaviour
    {
        public GameObject friendlyObj;
        public GameObject enemyObj;

        public TMP_Text timeSpentText;
        public TMP_Text spatialPartitionStatusText;
        public TMP_Text fpsText;
        public TMP_Text entityCountText;

        // Change materials to detect which enemy is the closest
        public Material enemyMaterial;
        public Material closestEnemyMaterial;

        // To get a cleaner workspace, parent all soldiers to these empty gameobjects
        public Transform enemyParent;
        public Transform friendlyParent;

        // Store all soldiers in these lists
        List<Soldier> enemySoldiers = new List<Soldier>();
        List<Soldier> friendlySoldiers = new List<Soldier>();

        // Save the closest enemies to easier change back its material
        List<Soldier> closestEnemies = new List<Soldier>();

        // Grid data
        float mapWidth = 1000f;
        int cellSize = 100;

        // Number of enemies(fish) and friendly(shark) soldiers
        public int numberOfEnemies = 300;
        public int numberOfFriendlies = 200;

        // The Spatial Partition grid
        Grid grid;

        float updateTimeSpent = 0f;
        bool isSpatialPartitionEnabled = true;

        // FPS tracking
        float deltaTime = 0.0f;

        void Start()
        {
            // Create a new grid
            grid = new Grid((int)mapWidth, cellSize);

            // Add random enemies and store them in a list
            for (int i = 0; i < numberOfEnemies; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));

                GameObject newEnemy = Instantiate(enemyObj, randomPos, Quaternion.identity) as GameObject;

                enemySoldiers.Add(new Enemy(newEnemy, mapWidth, grid));

                newEnemy.transform.parent = enemyParent;

                if (!newEnemy.GetComponent<Collider>())
                {
                    newEnemy.AddComponent<BoxCollider>().isTrigger = true;
                }
            }

            // Add random friendlies and store them in a list
            for (int i = 0; i < numberOfFriendlies; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));

                GameObject newFriendly = Instantiate(friendlyObj, randomPos, Quaternion.identity) as GameObject;

                friendlySoldiers.Add(new Friendly(newFriendly, mapWidth));

                newFriendly.transform.parent = friendlyParent;

                if (!newFriendly.GetComponent<Collider>())
                {
                    newFriendly.AddComponent<BoxCollider>();
                }

                if (!newFriendly.GetComponent<Rigidbody>())
                {
                    newFriendly.AddComponent<Rigidbody>().isKinematic = true;
                }
            }

            UpdateSpatialPartitionStatusText();
            UpdateEntityCountText();
        }

        void Update()
        {
            // Toggle spatial partitioning on/off when the spacebar is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isSpatialPartitionEnabled = !isSpatialPartitionEnabled;
                UpdateSpatialPartitionStatusText();
            }

            // Start measuring time
            float startTime = Time.realtimeSinceStartup;

            // Move the enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                enemySoldiers[i].Move();
            }

            // Reset material of the closest enemies
            for (int i = 0; i < closestEnemies.Count; i++)
            {
                closestEnemies[i].soldierMeshRenderer.material = enemyMaterial;
            }

            closestEnemies.Clear();

            // For each friendly, find the closest enemy and change its color and chase it
            for (int i = 0; i < friendlySoldiers.Count; i++)
            {
                Soldier closestEnemy;

                if (isSpatialPartitionEnabled)
                {
                    closestEnemy = grid.FindClosestEnemy(friendlySoldiers[i]);
                }
                else
                {
                    closestEnemy = FindClosestEnemySlow(friendlySoldiers[i]);
                }

                if (closestEnemy != null)
                {
                    closestEnemy.soldierMeshRenderer.material = closestEnemyMaterial;
                    closestEnemies.Add(closestEnemy);
                    friendlySoldiers[i].Move(closestEnemy);
                }
            }

            // Calculate time spent in Update
            float endTime = Time.realtimeSinceStartup;
            updateTimeSpent = (endTime - startTime) * 1000.0f;

            if (timeSpentText != null)
            {
                timeSpentText.text = "Time spent in Update: " + updateTimeSpent.ToString("F6") + " ms";
            }

            // Tracks FPS
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            if (fpsText != null)
            {
                fpsText.text = string.Format("FPS: {0:0.}", fps);
            }

            UpdateEntityCountText();
        }

        // Find the closest enemy - slow version
        Soldier FindClosestEnemySlow(Soldier soldier)
        {
            Soldier closestEnemy = null;

            float bestDistSqr = Mathf.Infinity;

            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                float distSqr = (soldier.soldierTrans.position - enemySoldiers[i].soldierTrans.position).sqrMagnitude;

                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;
                    closestEnemy = enemySoldiers[i];
                }
            }

            return closestEnemy;
        }

        void UpdateSpatialPartitionStatusText()
        {
            if (spatialPartitionStatusText != null)
            {
                spatialPartitionStatusText.text = "Spatial Partition: " + (isSpatialPartitionEnabled ? "Enabled" : "Disabled");

                // Change the color of the text based on the status of spatial partitioning (green = on, red = off)
                if (isSpatialPartitionEnabled)
                {
                    spatialPartitionStatusText.color = Color.green;
                }
                else
                {
                    spatialPartitionStatusText.color = Color.red;
                }
            }
        }

        void UpdateEntityCountText()
        {
            if (entityCountText != null)
            {
                // Lists the amount of fish and sharks in the scene
                entityCountText.text = $"In Scene: Fish: {enemySoldiers.Count} | Sharks: {friendlySoldiers.Count}";
            }
        }
    }
}