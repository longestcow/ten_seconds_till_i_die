using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Cinemachine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

public class FirstPersonController : MonoBehaviour
{
	[Header("Player")]
	public float MoveSpeed = 4.0f;
	public float SprintSpeed = 6.0f;
	public float RotationSpeed = 1.0f;
	public float SpeedChangeRate = 10.0f;
	public float currentTime = 0;
	float scaleb = 0.4f;
	public GameObject blackScreen;
	EnemySpawner enemySpawner;
	int currentTimeIndex = 0;
	public TextMeshProUGUI timeText;
	public CinemachineVirtualCamera vcam;
	bool resetting = false, walking = false, dead = false;
	Coroutine shootCoroutine;

	[Space(10)]
	public float JumpHeight = 1.2f;
	public float Gravity = -9.8f;

	[Space(10)]
	public float JumpTimeout = 0.1f;
	public float FallTimeout = 0.15f;

	[Space(10)]
	public Animator animb;
	public Image gunImage;
	public Sprite[] gunSprites;
	public Image timerImage;
	public Sprite[] timerSprites;
	

	[Header("Player Grounded")]
	public bool Grounded = true;
	bool pGrounded = true;
	public float GroundedOffset = -0.14f;
	public float GroundedRadius = 0.5f;
	public LayerMask GroundLayers;

	[Header("Cinemachine")]
	public GameObject CinemachineCameraTarget;
	public float TopClamp = 90.0f;
	public float BottomClamp = -90.0f;

	// cinemachine
	private float _cinemachineTargetPitch;

	// player
	private float _speed;
	private float _rotationVelocity;
	private float _verticalVelocity;
	private float _terminalVelocity = 53.0f;

	// timeout deltatime
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;


	private PlayerInput _playerInput;
	private CharacterController _controller;
	private StarterAssetsInputs _input;
	private GameObject _mainCamera;

	private const float _threshold = 0.01f;

	private bool IsCurrentDeviceMouse
	{
		get
		{
			return _playerInput.currentControlScheme == "KeyboardMouse";

		}
	}

	private void Awake()
	{
		// get a reference to our main camera
		if (_mainCamera == null)
		{
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
	}

	private void Start()
	{
		_controller = GetComponent<CharacterController>();
		_input = GetComponent<StarterAssetsInputs>();
		_playerInput = GetComponent<PlayerInput>();


		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;
		enemySpawner = GameObject.FindGameObjectWithTag("enemySpawner").GetComponent<EnemySpawner>();
		SFXManager.instance.changeMusic(1, transform);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		vcam.m_Lens.FieldOfView=SFXManager.instance.fov;
	}

	private void Update()
	{
		if (dead)
			return;

		TimerCount();
		GroundedCheck();
		JumpAndGravity();
		Move();
		Shoot();
		SetTimeText();

	}

	private void LateUpdate()
	{
		CameraRotation();
	}

	private void GroundedCheck()
	{
		// set sphere position, with offset
		Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		if (Grounded && !pGrounded){
			animb.SetTrigger("jumpDown");
			SFXManager.instance.playSFX(6, transform, 1f);
		}
		if(!Grounded && pGrounded){
			animb.SetTrigger("jumpUp");
		}
		pGrounded = Grounded;
	}

	private void CameraRotation()
	{
		// if there is an input
		if (_input.look.sqrMagnitude >= _threshold)
		{
			//Don't multiply mouse input by Time.deltaTime
			float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
			
			_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
			_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

			// clamp our pitch rotation
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Update Cinemachine camera target pitch
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

			// rotate the player left and right
			transform.Rotate(Vector3.up * _rotationVelocity);
		}
	}

	private void Move()
	{
		// set target speed based on move speed, sprint speed and if sprint is pressed
		float targetSpeed = _input.sprint ? (Grounded?SprintSpeed:1.5f*MoveSpeed): MoveSpeed;

		// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		// if there is no input, set the target speed to 0
		if (_input.move == Vector2.zero) targetSpeed = 0.0f;

		// a reference to the players current horizontal velocity
		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = targetSpeed;
		}

		// normalise input direction
		Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

		if (_input.move != Vector2.zero){
			inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			if (!walking) {
				walking = true;
				animb.SetBool("walk", true);
			}
		}
		else if (walking) {
			walking = false;
			animb.SetBool("walk", false);
		}
		

		_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
	}

	private void JumpAndGravity()
	{
		if (Grounded)
		{
			_fallTimeoutDelta = FallTimeout;

			if (_verticalVelocity < 0.0f){
				_verticalVelocity = -2f;
			}

			if (_input.jump && _jumpTimeoutDelta <= 0.0f)
			{
				// the square root of H * -2 * G = how much velocity needed to reach desired height
				_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
			}

			// jump timeout
			if (_jumpTimeoutDelta >= 0.0f)
			{
				_jumpTimeoutDelta -= Time.deltaTime;
			}
		}
		else
		{
			// reset the jump timeout timer
			_jumpTimeoutDelta = JumpTimeout;

			// fall timeout
			if (_fallTimeoutDelta >= 0.0f)
			{
				_fallTimeoutDelta -= Time.deltaTime;
			}

			// if we are not grounded, do not jump
			_input.jump = false;
		}

		if (_verticalVelocity < _terminalVelocity)
		{
			_verticalVelocity += Gravity * Time.deltaTime;
		}
	}

	private void Shoot()
	{
		if (_input.shoot)
		{
			_input.shoot = false;
			if(shootCoroutine != null)
				StopCoroutine(shootCoroutine);
			shootCoroutine = StartCoroutine(ShootAnim());
				
		}
	}

	IEnumerator ShootAnim()
	{
		gunImage.sprite = gunSprites[0];
		yield return new WaitForSeconds(0.02f);
		gunImage.sprite = gunSprites[1];
        SFXManager.instance.playSFX(0, transform, 1f);

		RaycastHit hit;
		if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit, 100f)){
			Debug.Log("Hit: " + hit.collider.gameObject.name);
    		Debug.Log("Layer: " + hit.collider.gameObject.layer);
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer("enemy"))
			{
				 hit.collider.GetComponent<Walker>().Hurt(hit.point);
			}
		}
		yield return new WaitForSeconds(0.2f);
		gunImage.sprite = gunSprites[0];
		shootCoroutine = null;
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
		Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
	}

	private void TimerCount()
	{
		if (resetting)
			return;
		currentTime += Time.deltaTime * scaleb;
		if(currentTime > 10)
			currentTime = 10;
		if ((int)currentTime != currentTimeIndex)
		{
			currentTimeIndex = (int)currentTime;
			timerImage.sprite = timerSprites[currentTimeIndex];
			float pitch = Mathf.Lerp(0.9f, 1.6f, currentTimeIndex / 10f);
        	SFXManager.instance.playSFX(4, transform, 1f, pitch);

		}
		if (currentTime >= 9.9f)
			Dead();

	}

	void Dead()
	{
		dead=true;
		SFXManager.instance.fadeOut();
		enemySpawner.StopSpawning();
		currentTime = 9.99f;
		SetTimeText();
		blackScreen.SetActive(true);
		StartCoroutine(DeathScene());
	}
	IEnumerator DeathScene()
	{
		SFXManager.instance.playSFX(3, transform, 1f);
		yield return new WaitForSeconds(3.5f);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		currentTime = 10f;
		SetTimeText();
		timerImage.sprite = timerSprites[10];
		currentTimeIndex = (int)currentTime;
		timerImage.sprite = timerSprites[currentTimeIndex];

	}

	public void ResetTime()
	{
		scaleb=1f;
		StartCoroutine(ResetTimeCor());
	}
	IEnumerator ResetTimeCor()
	{
		float timeSplit = 0.3f/currentTimeIndex;
		if (resetting)
			yield break;
		resetting = true;
		for (int i = currentTimeIndex-1; i >= 0; i--)
		{
			float pitch = Mathf.Lerp(0.9f, 1.6f, i / 10f);
        	SFXManager.instance.playSFX(5, transform, 1f, pitch);
			timerImage.sprite = timerSprites[i];
			yield return new WaitForSeconds(timeSplit);
		}
		timerImage.sprite = timerSprites[0];
		currentTime = 0;
		resetting = false;
		
	}

	void SetTimeText()
	{
		float remaining = 10f-currentTime;
		int ss = Mathf.FloorToInt(remaining);
		int ms = Mathf.FloorToInt((remaining - ss) * 100);
		timeText.text = string.Format("{0:00}.{1:00}s", ss, ms);
	}

	private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer==LayerMask.NameToLayer("bloodBath"))
        {
			SFXManager.instance.playSFX(2, transform, 1f);
            Dead();
        }

		else if(collision.gameObject.layer == LayerMask.NameToLayer("enemy"))
		{
			SFXManager.instance.playSFX(2, transform, 2f);
			SFXManager.instance.playSFX(9, transform, 1f);
			// currentTime+=1f;
			Debug.Log("OUCHOUCHOUCH");
		}
		else if(collision.gameObject.layer == LayerMask.NameToLayer("bullet"))
		{
			SFXManager.instance.playSFX(2, transform, 2f);
			Debug.Log("OUCHOUCHOUCH");
			// currentTime+=1f;
			Destroy(collision.gameObject);
		}
    }
}
