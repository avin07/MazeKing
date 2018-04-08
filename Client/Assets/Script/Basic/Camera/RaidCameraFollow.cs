using UnityEngine;
using System.Collections;

public class RaidCameraFollow : MonoBehaviour 
{
        public Transform cameraTransform;
        public Transform _target;
        public float angleVelocity = 0f;
        public float angularSmoothLag = 0.3f;
        public float angularMaxSpeed = 60f;

        public float heightSmoothLag = 0.3f;
        public float heightVelocity = 0f;
        public float distance = 4f;
        public float height = 0.5f;
        public float clampHeadPositionScreenSpace = 0.75f;

        // Use this for initialization
	void Awake() 
        {
                cameraTransform = Camera.main.transform;
                //_target = RaidManager.GetInst().MainHero.transform;
	}
        public void SetAngleImme()
        {
                float currentAngle = cameraTransform.eulerAngles.y;
                Quaternion qua = cameraTransform.rotation;
                qua.eulerAngles = new Vector3(cameraTransform.eulerAngles.x, _target.rotation.eulerAngles.y, cameraTransform.eulerAngles.z);
                cameraTransform.rotation = qua;
        }
        float AngleDistance(float a, float b)
        {
                a = Mathf.Repeat(a, 360);
                b = Mathf.Repeat(b, 360);
                return Mathf.Abs(b - a);
        }

	// Update is called once per frame
        void LateUpdate()
        {
                if (_target == null)
                {
                        //_target = RaidManager.GetInst().MainHero.transform;
                }
                if (_target == null)
                {
                        return;

                }
                Vector3 targetCenter = _target.position;
                Vector3 targetHead = _target.position + Vector3.up * 1f;

                // Calculate the current & target rotation angles
                float originalTargetAngle = _target.eulerAngles.y;
                float currentAngle = cameraTransform.eulerAngles.y;

                // Adjust real target angle when camera is locked
                float targetAngle = originalTargetAngle;

                //                         if (controller.GetLockCameraTimer() < lockCameraTimeout)
                //                         {
                //                                 targetAngle = currentAngle;
                //                         }

                // Lock the camera when moving backwards!
                // * It is really confusing to do 180 degree spins when turning around.
//                 if (AngleDistance(currentAngle, targetAngle) > 160 /*&& controller.IsMovingBackwards()*/)
//                         targetAngle += 180;
                currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, angularSmoothLag, angularMaxSpeed);

                // Convert the angle into a rotation, by which we then reposition the camera
                Quaternion currentRotation = Quaternion.Euler(0, currentAngle, 0);

                // Set the position of the camera on the x-z plane to:
                // distance meters behind the target
                cameraTransform.position = targetHead;
                cameraTransform.position += currentRotation * Vector3.back * distance + Vector3.up * height;
                // Set the height of the camera
                //cameraTransform.position.y = currentHeight;

                // Always look at the target	
                SetUpRotation(targetCenter, targetHead);

        }
        void SetUpRotation(Vector3 centerPos, Vector3 headPos)
        {
                // Now it's getting hairy. The devil is in the details here, the big issue is jumping of course.
                // * When jumping up and down we don't want to center the guy in screen space.
                //  This is important to give a feel for how high you jump and avoiding large camera movements.
                //   
                // * At the same time we dont want him to ever go out of screen and we want all rotations to be totally smooth.
                //
                // So here is what we will do:
                //
                // 1. We first find the rotation around the y axis. Thus he is always centered on the y-axis
                // 2. When grounded we make him be centered
                // 3. When jumping we keep the camera rotation but rotate the camera to get him back into view if his head is above some threshold
                // 4. When landing we smoothly interpolate towards centering him on screen
                var cameraPos = cameraTransform.position;
                var offsetToCenter = centerPos - cameraPos;

                // Generate base rotation only around y-axis
                var yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0, offsetToCenter.z));

                var relativeOffset = Vector3.forward * distance + Vector3.down * height;
                cameraTransform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);

                // Calculate the projected center position and top position in world space
                Ray centerRay = cameraTransform.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
                Ray topRay = cameraTransform.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, clampHeadPositionScreenSpace, 1f));

                Vector3 centerRayPos = centerRay.GetPoint(distance);
                Vector3 topRayPos = topRay.GetPoint(distance);

                float centerToTopAngle = Vector3.Angle(centerRay.direction, topRay.direction);

                float heightToAngle = centerToTopAngle / (centerRayPos.y - topRayPos.y);

                float extraLookAngle = heightToAngle * (centerRayPos.y - centerPos.y);
                if (extraLookAngle < centerToTopAngle)
                {
                        extraLookAngle = 0;
                }
                else
                {
                        extraLookAngle = extraLookAngle - centerToTopAngle;
                        cameraTransform.rotation *= Quaternion.Euler(-extraLookAngle, 0, 0);
                }
        }
}
