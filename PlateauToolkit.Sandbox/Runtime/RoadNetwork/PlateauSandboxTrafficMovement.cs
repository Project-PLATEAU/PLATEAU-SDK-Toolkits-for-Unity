using AWSIM;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// NPCVehicleからPlateauSandboxVehicleにアクセスするためのinterface
    /// </summary>
    interface IPlateauSandboxTrafficObject : IPlateauSandboxPlaceableObject
    {
        float GetWheelBase();

        void UpdateVisual(float speed, float steerAngle);
    }

    /// <summary>
    /// NPCVehicle Wrapper
    /// Spawn時にAssignされる
    /// </summary>
    public class PlateauSandboxTrafficMovement : NPCVehicle
    {
        void Awake()
        {
            Initialize(gameObject);
        }
    }
}
