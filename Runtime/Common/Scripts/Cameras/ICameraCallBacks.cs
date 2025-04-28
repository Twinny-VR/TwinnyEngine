using UnityEngine;

namespace Twinny.System.Cameras
{
    public interface ICameraCallBacks 
    {
        void OnChangeCamera(CameraHandler camera);
    }
}
