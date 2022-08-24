using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using UnityEngine.UI;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    public class PlayerController : MovementController
    {
        [Header("Character Settings (Normal, Crouched)")]
        [SerializeField] private Vector2 characterHeight = new Vector2(1.8f, 0.835f);
        [SerializeField] private Vector2 characterCenter = new Vector2(0.9f, 0.4175f);
        [Header("Crouch Movement Speed (Normal, Sprint)")]
        [SerializeField] private Vector2 crouchSpeed = new Vector2(1, 1.8f);
        [Header("Crouch Standup Settings")]
        [SerializeField] private GameObject standupCheck;
        [SerializeField] private float overlapSphereRadius = 0.3f;
        [Header("Detected By:")]
        [SerializeField] private List<AIController> enemies;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;


        private PlayerInput _playerInput;
        private StarterAssetsInputs _starterAssetsInputs;

        [Header("UI")]
        [SerializeField] private Canvas detectionStatusCanvas;
        [SerializeField] private Image detectionStatusImage;
        [SerializeField] private Sprite detectedSprite, undetectedSprite;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                detectionStatusCanvas.worldCamera = _mainCamera.GetComponent<Camera>();
            }
        }
        private void OnEnable()
        {
            AIController.OnDetectedEvent += AddDetectingEnemy;
            AIController.OnLostDetectionEvent += RemoveDetectingEnemy;
        }
        private void OnDisable()
        {
            AIController.OnDetectedEvent -= AddDetectingEnemy;
            AIController.OnLostDetectionEvent -= RemoveDetectingEnemy;
        }

        private void AddDetectingEnemy(AIController aIController)
        {
            enemies.Add(aIController);
            if (enemies.Count > 0)
                detectionStatusImage.sprite = detectedSprite;
        }
        private void RemoveDetectingEnemy(AIController aIController)
        {
            enemies.Remove(aIController);
            if (enemies.Count == 0)
                detectionStatusImage.sprite = undetectedSprite;
        }

        protected override void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _starterAssetsInputs = (StarterAssetsInputs)_input;
            _playerInput = GetComponent<PlayerInput>();


            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        protected override void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
        }


        private void LateUpdate()
        {
            CameraRotation();
            detectionStatusCanvas.transform.LookAt(_mainCamera.transform.position, _mainCamera.transform.rotation * Vector3.up);
        }
        protected override void Move()
        {
            if(_input.crouch)
            {
                _controller.height = characterHeight.y;
                _controller.center = Vector2.up * characterCenter.y;
                _animator.SetBool(_animIDCrouch, true);
            }
            else
            {
                Collider[] cols = Physics.OverlapSphere(standupCheck.transform.position, overlapSphereRadius, GroundLayers);
                if (cols == null || cols.Length == 0)
                {
                    _controller.height = characterHeight.x;
                    _controller.center = Vector2.up * characterCenter.x;
                    _animator.SetBool(_animIDCrouch, false);
                }
                else
                {
                    Debug.Log("Too low to stand up");
                    _input.crouch = true;
                }
            }
            base.Move();
        }
        protected override float GetTargetSpeed()
        {
            if(_input.crouch)
                return _input.sprint? crouchSpeed.y : crouchSpeed.x;
            return _input.sprint ? SprintSpeed : MoveSpeed;
        }

        protected override void CalculateTargetRotation(Vector3 inputDirection)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
        }
        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_starterAssetsInputs.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _starterAssetsInputs.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _starterAssetsInputs.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }


        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
    }
}