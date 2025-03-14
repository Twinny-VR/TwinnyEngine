using UnityEngine.UI;
using UnityEngine;


namespace Twinny.UI
{

public class Compass : MonoBehaviour
{
		[SerializeField]
	private RawImage _compassBar;
	private Transform _cameraTransform;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;
        }

        public void Update()
	{
		//Get a handle on the Image's uvRect
		_compassBar.uvRect = new Rect(_cameraTransform.localEulerAngles.y / 360, 0, 1, 1);

			/*
		// Get a copy of your forward vector
		Vector3 forward = _cameraTransform.transform.forward;

		// Zero out the y component of your forward vector to only get the direction in the X,Z plane
		forward.y = 0;

		//Clamp our angles to only 5 degree increments
		float headingAngle = Quaternion.LookRotation(forward).eulerAngles.y;
		headingAngle = 5 * (Mathf.RoundToInt(headingAngle / 5.0f));

		//Convert float to int for switch
		int displayangle = Mathf.RoundToInt(headingAngle);
			*/
		
	}
}
}