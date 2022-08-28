using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace StarterAssets
{
	public class StarterAssetsInputs : MovementInputs
	{
		[Header("Camera Input Values")]
		public Vector2 look;
		
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
        private PlayerController playerController;
		private CostumeChanger costumeChanger;
		private GameManager gameManager;
        private void Awake()
        {
			costumeChanger = GetComponent<CostumeChanger>();
			playerController = GetComponent<PlayerController>();
        }
        private void Start()
        {
			gameManager = GameManager.GetInstance();            
        }

        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnCrouch(InputValue value)
        {
			if(!(crouch && playerController.ShouldCrawl))
				CrouchInput();
        }

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}
		public void OnChange(InputValue value)
		{
			if(playerController.Grounded)
            {
				costumeChanger.ChangeInput(value.isPressed);
            }
		}
		public void OnStart()
        {
			gameManager.OnStartPressed();
        }
		public void OnPause()
        {
			gameManager.OnPausePressed();
		}

		public void InteractInput(bool isPressed)
        {
			if (playerController != null && isPressed)
				playerController.Interact();
        }

        public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
    }
	
}