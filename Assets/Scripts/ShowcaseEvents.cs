using UnityEngine;

public class ShowcaseEvents : MonoBehaviour
{
    [SerializeField] private GameObject _mainVirtualCamera;
    [SerializeField] private GameObject _weaponCamera;
    [SerializeField] private GameObject _overviewCamera;
    
    private CameraState _cameraState = CameraState.Main;
    private GameObject _currentPOVCamera;
    
    private bool _badPerformanceMode;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            EnemySpawner.Instance.EnableEnemySpawns();
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            _badPerformanceMode = true;
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (_cameraState == CameraState.Main)
            {
                _mainVirtualCamera.SetActive(false);
                _weaponCamera.SetActive(true);
                
                _overviewCamera.SetActive(true);
                _cameraState = CameraState.Overview;
            }
            else if (_cameraState == CameraState.Overview)
            {
                _overviewCamera.SetActive(false);

                _currentPOVCamera = EnemySpawner.Instance.GetPOVCamera();
                _currentPOVCamera.SetActive(true);
                _cameraState = CameraState.POV;
            }
            else if (_cameraState == CameraState.POV)
            {
                _currentPOVCamera?.SetActive(false);
                
                _mainVirtualCamera.SetActive(true);
                _weaponCamera.SetActive(true);
                _cameraState = CameraState.Main;
            }
        }

        if (_badPerformanceMode)
        {
            for (int i = 0; i < 1000; i++)
            {
                Debug.Log("ALLOCATING STRINGS IS NOT GOOD!");
            }
        }
    }
    
    private enum CameraState
    {
        Main,
        Overview,
        POV
    }

}