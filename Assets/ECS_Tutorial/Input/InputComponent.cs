using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct InputComponent : IComponentData
{
    public Vector2 Movement;
    public Vector2 MousePosition;
    public bool Shoot;

}
