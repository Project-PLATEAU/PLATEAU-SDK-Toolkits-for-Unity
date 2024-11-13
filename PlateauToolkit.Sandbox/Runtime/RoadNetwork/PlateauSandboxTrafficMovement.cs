using AWSIM;

namespace PlateauToolkit.Sandbox
{
    interface IPlateauSandboxTrafficObject : IPlateauSandboxPlaceableObject
    {
        //float SecondAxisDistance => 0;

        //float GetVelocityRatio(float lookAheadAngle)
        //{
        //    return 1f;
        //}

        float GetWheelBase();

        //void OnMove(in MovementInfo movementInfo);
        void UpdateVisual(float speed, float steerAngle);
    }

    //[ExecuteAlways]
    public class PlateauSandboxTrafficMovement : NPCVehicle
    {

        private void Awake()
        {
            Initialize(gameObject, transform);
        }
    }
}
