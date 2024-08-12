using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Physics;
using Unity.Mathematics;

[BurstCompile]
public partial struct BulletSystem : ISystem
{

    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        NativeArray<Entity> allEnities = entityManager.GetAllEntities();
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (Entity entity in allEnities)
        {
            if(entityManager.HasComponent<BulletComponent>(entity) && entityManager.HasComponent<BulletLifeTimeComponent>(entity))
            {
                //move bullet
                LocalTransform bulletTrasform = entityManager.GetComponentData<LocalTransform>(entity);
                BulletComponent bulletCompoment = entityManager.GetComponentData<BulletComponent>(entity);

                bulletTrasform.Position += bulletCompoment.Speed * SystemAPI.Time.DeltaTime * bulletTrasform.Right();
                entityManager.SetComponentData(entity, bulletTrasform);

                //decrement timer
                BulletLifeTimeComponent bulletLifeTimeCompoment = entityManager.GetComponentData<BulletLifeTimeComponent>(entity);
                bulletLifeTimeCompoment.RemainingLifeTime -= SystemAPI.Time.DeltaTime;
                if(bulletLifeTimeCompoment.RemainingLifeTime <= 0f)
                {
                    entityManager.DestroyEntity(entity);
                    continue;
                }
                entityManager.SetComponentData(entity, bulletLifeTimeCompoment);

                // Physics
                NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);
                float3 point1 = new float3(bulletTrasform.Position - bulletTrasform.Right() * 0.15f);
                float3 point2 = new float3(bulletTrasform.Position + bulletTrasform.Right() * 0.15f);

                uint layerMask = LayerMaskHelper.GetLayerMaskFromTowLayer(CollisionLayer.Wall, CollisionLayer.Enemy);

                physicsWorld.CapsuleCastAll(point1, point2, bulletCompoment.Size / 2, float3.zero, 1f, ref hits, new CollisionFilter {
                    BelongsTo =  (uint)CollisionLayer.Default,
                    CollidesWith = layerMask
                }); 
                if(hits.Length > 0)
                {
                    for(int i = 0; i < hits.Length; i++)
                    {
                        Entity hitEntity = hits[i].Entity;
                        if (entityManager.HasComponent<EnemyCompoment>(hitEntity))
                        {
                            EnemyCompoment enemyCompoment = entityManager.GetComponentData<EnemyCompoment>(hitEntity);
                            enemyCompoment.CurrrentHeath -= bulletCompoment.Damage;
                            entityManager.SetComponentData(hitEntity, enemyCompoment);

                            if(enemyCompoment.CurrrentHeath <= 0f)
                            {
                                entityManager.DestroyEntity(hitEntity);
                            }
                        }
                            
                    }
                    entityManager.DestroyEntity(entity);
                }
                hits.Dispose();

            }
        }
    }
}
