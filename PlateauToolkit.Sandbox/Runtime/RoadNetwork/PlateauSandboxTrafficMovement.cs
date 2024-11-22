using AWSIM;

namespace PlateauToolkit.Sandbox
{
    interface IPlateauSandboxTrafficObject : IPlateauSandboxPlaceableObject
    {
        float GetWheelBase();

        void UpdateVisual(float speed, float steerAngle);
    }

    public class PlateauSandboxTrafficMovement : NPCVehicle
    {

        private void Awake()
        {
            Initialize(gameObject);
        }
    }
}
