using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
    private EntityManager _entityManager;
    private Entity _enemySpawnerEntity;
    private EnemySpawnerComponent _enemySpawnerCompoment;

    private Entity _playerEntity;

    private Unity.Mathematics.Random _random;

    public void OnCreate(ref SystemState state)
    {
        _random = Unity.Mathematics.Random.CreateFromIndex((uint)_enemySpawnerCompoment.GetHashCode());
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        _enemySpawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnerComponent>();
        _enemySpawnerCompoment = _entityManager.GetComponentData<EnemySpawnerComponent>(_enemySpawnerEntity);

        _playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();

        SpawnerEnemies(ref state);
    }

    [BurstCompile]
    private void SpawnerEnemies(ref SystemState state)
    {
        // dectement timer
        _enemySpawnerCompoment.CurrentTimeBeforeNextSpawn -= SystemAPI.Time.DeltaTime;
        if(_enemySpawnerCompoment.CurrentTimeBeforeNextSpawn <= 0f)
        {
            for (int i = 0; i < _enemySpawnerCompoment.NumOfEnemiesToSpawnPerSecond; i++)
            {
                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);
                Entity enemyEntity = _entityManager.Instantiate(_enemySpawnerCompoment.EnemyPrefabToSpawn);
                LocalTransform enemyTransform = _entityManager.GetComponentData<LocalTransform>(enemyEntity);
                LocalTransform playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);

                //random spawn point
                float minDistanceSquared = _enemySpawnerCompoment.MinimumDistanceFromPlayer * _enemySpawnerCompoment.MinimumDistanceFromPlayer;
                float2 randomOffset = _random.NextFloat2Direction() * _random.NextFloat(_enemySpawnerCompoment.MinimumDistanceFromPlayer, _enemySpawnerCompoment.EnemySpawnRadius);
                float2 playerPositon = new float2(playerTransform.Position.x, playerTransform.Position.y);
                float2 spawnPosition = playerPositon + randomOffset;
                float distanceSquared = math.lengthsq(spawnPosition - playerPositon);

                if(distanceSquared < minDistanceSquared)
                {
                    spawnPosition = playerPositon + math.normalize(randomOffset) * math.sqrt(minDistanceSquared);
                }
                enemyTransform.Position = new float3(spawnPosition.x, spawnPosition.y, 0f);

                //spawn look direction
                float3 direction = math.normalize(playerTransform.Position - enemyTransform.Position);
                float angle = math.atan2(direction.y, direction.x);
                angle -= math.radians(90f);
                quaternion lookRot = quaternion.AxisAngle(new float3(0, 0, 1), angle);
                enemyTransform.Rotation = lookRot;

                ECB.SetComponent(enemyEntity, enemyTransform);
                ECB.AddComponent(enemyEntity, new EnemyCompoment {
                    CurrrentHeath = 100f,
                    EnemySpeed = 1.25f
                });

                ECB.Playback(_entityManager);
                ECB.Dispose();
            }
            //increment the number of enemies that spawn in each wave
            int desiredEnemiesPerWave = _enemySpawnerCompoment.NumOfEnemiesToSpawnPerSecond + _enemySpawnerCompoment.NumOfEnemiesToSpawnIncrementAmount;
            int enemiesPerWave = math.min(desiredEnemiesPerWave, _enemySpawnerCompoment.MaxNumberOfEnemiesToSpawnPerSecond);
            _enemySpawnerCompoment.MaxNumberOfEnemiesToSpawnPerSecond = enemiesPerWave;
            _enemySpawnerCompoment.CurrentTimeBeforeNextSpawn = _enemySpawnerCompoment.TimeBeforeNextSpawn;
        }
        _entityManager.SetComponentData(_enemySpawnerEntity, _enemySpawnerCompoment);

    }


}
