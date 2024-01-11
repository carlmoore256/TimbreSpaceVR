using UnityEngine;
using System;

namespace TimbreSpaceVR.Testing {
    
    public class TestPlayer : MonoBehaviour
    {
        [SerializeField] private CameraPlayerController _cameraPlayerController;

        public void EnableFPV()
        {
            _cameraPlayerController.EnableFPV();
        }

    }

}