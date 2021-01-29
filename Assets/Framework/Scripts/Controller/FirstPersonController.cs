using UnityEngine;  
using System.Collections;  
using UnityEngine.EventSystems;

using DG.Tweening;

namespace Framework {

	[RequireComponent(typeof (CharacterController))]
	public class FirstPersonController : MonoBehaviour {  

		public enum MouseButton {
			LEFT=0,
			RIGHT,
			MIDDLE,
		}

		// Move
		[Header("Move")]
		[SerializeField] private float m_MoveSpeed = 3;
		[SerializeField] private float m_MoveSpeedSide = 2;

		// Jump
		[Header("Jump")]
		[SerializeField] public bool m_JumpEnable = true;
		[SerializeField] private float m_JumpSpeed = 4f;
		[SerializeField] private float m_StickToGroundForce=1f;
		[SerializeField] private float m_GravityMultiplier=1f;

		// Rotate
		[Header("Rotate")]
		[SerializeField] public float XSensitivity = 2f;
		[SerializeField] public float YSensitivity = 2f;
		[SerializeField] public bool ClampVerticalRotation = true;
		[SerializeField] public float MinimumX = -60F;
		[SerializeField] public float MaximumX = 60F;
		[SerializeField] public MouseButton m_RotateMouseButton = MouseButton.RIGHT;
        [SerializeField] public bool m_RotateSmooth = false;
        [SerializeField] public float m_RotateSmoothTime = 5f;


        // zoom
        [Header("Zoom")]
		public bool m_ZoomEnable = true;
		public float m_ZoomSpeed = 4f;
		[SerializeField] public float ZoomMin = 30f;
		[SerializeField] public float ZoomMax = 60F;
		  

		private Vector3 m_MoveDir = Vector3.zero;
		private CharacterController m_CharacterController;
		private CollisionFlags m_CollisionFlags;
		private bool m_PreviouslyGrounded;

		private Camera m_Camera;
		private bool m_Jump;
		private bool m_Jumping;

        private Quaternion m_CharacterRot;
        private Quaternion m_CameraRot;

        private Vector3 m_CamerPosPre = Vector3.zero;
		public void LockCamera() {
			enabled = false;	
			m_CamerPosPre = m_Camera.transform.position;
		}

		public void UnLockCamera() {
			
			//m_Camera.transform.position = m_CamerPosPre;
			Tweener t = m_Camera.transform.DOMove (m_CamerPosPre, 1);
			t.OnComplete (delegate {
				enabled = true;
			});

		}

		void Start ()
		{  
			m_CharacterController = GetComponent<CharacterController>();
			m_Camera = transform.GetComponentInChildren<Camera>();


        }  

		void Update() {

			// the jump state needs to read here to make sure it is not missed
			if (!m_Jump && m_JumpEnable)
			{
				m_Jump = Input.GetButtonDown("Jump");
			}

			if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
			{
				m_MoveDir.y = 0f;
				m_Jumping = false;
			}
			if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
			{
				m_MoveDir.y = 0f;
			}

			m_PreviouslyGrounded = m_CharacterController.isGrounded;
		}

	    // Update is called once per frame
	    void LateUpdate()
	    {
			if (!enabled) return;
			
			Zoom ();
			Rotate ();
			Move ();
	    }

		void Rotate() {

			if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonDown((int)m_RotateMouseButton))
            {
                m_CharacterRot = transform.localRotation;
                m_CameraRot = m_Camera.transform.localRotation;
            }

            if (Input.GetMouseButton((int)m_RotateMouseButton))
			{
				float yRot = Input.GetAxis("Mouse X") * XSensitivity;
				float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

                m_CharacterRot *= Quaternion.Euler(0f, yRot, 0f);
                m_CameraRot *= Quaternion.Euler(-xRot, 0f, 0f);

                if (m_RotateSmooth)
                {
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, m_CharacterRot,
                        m_RotateSmoothTime * Time.deltaTime);
                    m_Camera.transform.localRotation = Quaternion.Slerp(m_Camera.transform.localRotation, m_CameraRot,
                        m_RotateSmoothTime * Time.deltaTime);
                }
                else
                {
                    transform.localRotation = m_CharacterRot;
                    m_Camera.transform.localRotation = m_CameraRot;
                }

                //transform.Rotate(new Vector3(0, yRot, 0));
				//m_Camera.transform.Rotate (new Vector3(-xRot, 0, 0));

				if (ClampVerticalRotation) {
					Quaternion rot = ClampRotationAroundXAxis(m_Camera.transform.localRotation);
					m_Camera.transform.localRotation = rot;
				}
			}
		}

		void Move() {
			
			Vector3 move = Vector3.zero;

			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			{
				move = transform.forward * m_MoveSpeed;
			} else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			{
				move = -transform.forward * m_MoveSpeed;
			} else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				move = -transform.right * m_MoveSpeedSide;
			} else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				move = transform.right * m_MoveSpeedSide;
			}

			m_MoveDir.x = move.x;
			m_MoveDir.z = move.z;

			if (m_CharacterController.isGrounded)
			{
				m_MoveDir.y = -m_StickToGroundForce;
				if (m_Jump) {
					m_MoveDir.y = m_JumpSpeed;
					m_Jump = false;
					m_Jumping = true;
				}
			} else {
				m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.deltaTime;
			}
			m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.deltaTime);
		}

		void Zoom() {

			if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

			if (!m_ZoomEnable && m_Camera != null)
				return;
			
			float value = 0f;
			value = m_Camera.orthographic ? m_Camera.orthographicSize : m_Camera.fieldOfView;
	
			value -= Input.GetAxis("Mouse ScrollWheel") * m_ZoomSpeed;
			value = Mathf.Clamp(value, ZoomMin, ZoomMax);

			if (m_Camera.orthographic ) {
				m_Camera.orthographicSize = value;
			} else {
				m_Camera.fieldOfView = value;

			}
		}

		Quaternion ClampRotationAroundXAxis(Quaternion q)
		{
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1.0f;

			float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

			angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

			q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

			return q;
		}
	} 
}