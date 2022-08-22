using UnityEngine;
using UnityEngine.InputSystem;
namespace StarterAssets
{
	public class MovementInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector3 move;
        public bool jump;
		public bool sprint;
        [Header("Movement Settings")]
		public bool analogMovement;

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}
		public void MoveInput(Vector3 newMoveDirection)
		{
			move = new Vector2(newMoveDirection.x, newMoveDirection.z);
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
	}
	
}