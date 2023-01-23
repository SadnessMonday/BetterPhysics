using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SadnessMonday.BetterPhysics.Samples {
    public class EnemySpawner : MonoBehaviour {
        [SerializeField] private DumbFollower enemyPrefab;
        [SerializeField] private Transform target;
        [SerializeField] private float enemiesPerSecond;
        [SerializeField] private int maxEnemies;
        [SerializeField] private Transform[] spawnPoints;

        private HashSet<DumbFollower> livingEnemies = new();
        private float timer;

        private void FixedUpdate() {
            float interval = 1f / enemiesPerSecond;
            timer += Time.deltaTime;

            while (timer >= interval) {
                if (livingEnemies.Count >= maxEnemies) {
                    // do nothing, wait until there's room
                    timer = interval;
                    break;
                }

                SpawnEnemy();
                timer -= interval;
            }
        }

        void SpawnEnemy() {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            DumbFollower newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            newEnemy.target = target;
            
            newEnemy.OnDeath += WhenAnEnemyDies;
            livingEnemies.Add(newEnemy);
        }

        private void WhenAnEnemyDies(DumbFollower source) {
            RemoveEnemy(source);
        }

        public void RemoveEnemy(DumbFollower enemy) {
            enemy.OnDeath -= RemoveEnemy;
            livingEnemies.Remove(enemy);
        }
    }
}