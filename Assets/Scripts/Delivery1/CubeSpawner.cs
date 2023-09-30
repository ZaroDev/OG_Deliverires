using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Delivery1
{
    public class CubeSpawner : MonoBehaviour
    {
        [Header("Spawner settings")]
        public int startAmount = 2000;
        public float timeToSpawn = 0.3f;
        
        [Header("Cube")]
        public GameObject cubePrefab;
        public float minSpeed = 2.5f;
        public float maxSpeed = 5f;
        public Vector3 maxEndPos;
        public Vector3 minEndPos;
        
        private Coroutine _spawnCubeCoroutine = null;

        public int CubeCount { get; private set; } = 0;
        public static Action<int> OnCubeAdded;

        private void Start()
        {
            for (int i = 0; i < startAmount; i++)
                CreateRandomCube();
        }

        private void Update()
        {
            _spawnCubeCoroutine ??= StartCoroutine(SpawnCubeCoroutine());
        }

        IEnumerator SpawnCubeCoroutine()
        {
            CreateRandomCube();
            yield return new WaitForSeconds(timeToSpawn);
            _spawnCubeCoroutine = null;
        }

        private void CreateRandomCube()
        {
            Instantiate(cubePrefab);
            var movingScript = cubePrefab.GetComponent<MovingCube>();
            movingScript.speed = Random.Range(minSpeed, maxSpeed);
            movingScript.endPos.Random(minEndPos, maxEndPos);
            CubeCount++;
            OnCubeAdded?.Invoke(CubeCount);
        }
    }
}
