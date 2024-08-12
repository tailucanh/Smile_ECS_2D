using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

public partial struct EnemySystem : ISystem
{
    private EntityManager _entityManager;

    private Entity _playerEntity;


    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        _playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();

        LocalTransform playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);

        NativeArray<Entity> allEntities = _entityManager.GetAllEntities();

        foreach (Entity entity in allEntities)
        {
            if (_entityManager.HasComponent<EnemyCompoment>(entity))
            {
                // move towards player
                LocalTransform enemyTransform = _entityManager.GetComponentData<LocalTransform>(entity);
                EnemyCompoment enemyCompnent = _entityManager.GetComponentData<EnemyCompoment>(entity);
                float3 moveDirection = math.normalize(playerTransform.Position - enemyTransform.Position);

                enemyTransform.Position += enemyCompnent.EnemySpeed * SystemAPI.Time.DeltaTime * moveDirection;
                // look at player
                float3 direction = math.normalize(playerTransform.Position - enemyTransform.Position);
                float angle = math.atan2(direction.y, direction.x);
                angle -= math.radians(90f);
                quaternion lookRot = quaternion.AxisAngle(new float3(0, 0, 1), angle);
                enemyTransform.Rotation = lookRot;

                _entityManager.SetComponentData(entity, enemyTransform);
            }
        }
    }



}
