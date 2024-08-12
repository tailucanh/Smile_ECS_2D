using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial class InputSystem : SystemBase
{
    private ControlsECS _controls;
    protected override void OnCreate()
    {
        if(!SystemAPI.TryGetSingleton(out InputComponent input))
        {
            EntityManager.CreateEntity(typeof(InputComponent));
        }
        _controls = new ControlsECS();
        _controls.Enable();
    }

    protected override void OnUpdate()
    {
        Vector2 moveVector = _controls.Player.Move.ReadValue<Vector2>();
        Vector2 mousePos = _controls.Player.MousePos.ReadValue<Vector2>();
        bool shoot = _controls.Player.Shoot.IsPressed();
        SystemAPI.SetSingleton(new InputComponent
        {
            MousePosition = mousePos,
            Movement = moveVector,
            Shoot = shoot
        }); 
    }
}
