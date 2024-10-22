using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MirrorController : MonoBehaviour
{
    [Header("References")]
    public DudeMovementStats MoveStats;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private Collider2D _feetCollider;

    private Rigidbody2D _rb;
    private Animator _anim;

    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    private RaycastHit2D _headHit;
    private RaycastHit2D _groundHit;
    private bool _bumpedHead;
    private bool _isGrounded;
    public bool Grounded
    {
        get { return _isGrounded; }
        set { _isGrounded = value; }
    }

    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _jumpsUsed;

    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;
    
    private float _coyoteTimer;

    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _thingsColliding = new List<Collider2D>();
    }

    private void Update()
    {
        JumpChecks();
        CountTimers();
    }

    private Vector2 _moveInput;
    private bool _jumpPress;
    private bool _jumpRelease;
    public void SetInput(InputPoint inputPoint)
    {
        _moveInput = inputPoint.MovementInput;
        _jumpPress = inputPoint.JumpPressed;
        _jumpRelease = inputPoint.JumpReleased;
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        _anim.SetFloat("YVelocity", VerticalVelocity);
        if (_moveInput == Vector2.zero || _thingsColliding.Count == 0)
            _anim.SetBool("Pushing", false);

        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, _moveInput);
            if (_groundObject != null)
            {
                _rb.velocity = new Vector3(_rb.velocity.x + _groundObject.GetComponent<VelocityCalculator>().Velocity.x,
                    _rb.velocity.y + _groundObject.GetComponent<VelocityCalculator>().Velocity.y);
            }
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, _moveInput);
        }
    }

    #region Move

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
            if (moveInput.x != 0)
                _anim.SetBool("Walking", true);
        }
        else
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
            _anim.SetBool("Walking", false);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        _isFacingRight = moveInput.x > 0 ? true : false;
        GetComponent<SpriteRenderer>().flipX = !_isFacingRight;
    }
    #endregion

    #region Collision
    private GameObject _groundObject;
    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (_groundHit.collider != null && VerticalVelocity <= 0f)
        {
            _isGrounded = true;
            _anim.SetBool("Grounded", true);
            _rb.mass = 1f;
            _rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            if (_groundHit.collider.tag == "Box")
            {
                _groundObject = _groundHit.collider.transform.parent.transform.parent.gameObject;
            }
            else if (_groundHit.collider.tag == "Moving Platform")
            {
                _groundObject = _groundHit.collider.gameObject;
            }
            Vector2 contactPoint = _groundHit.collider.gameObject.GetComponent<Collider2D>().ClosestPoint(transform.position);
            transform.position = new Vector3(transform.position.x, contactPoint.y + 0.015f, transform.position.z);
        }
        else
        {
            _isGrounded = false;
            _anim.SetBool("Grounded", false);
            _rb.mass = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _groundObject = null;
        }
    }

    private List<Collider2D> _thingsColliding;
    private void OnTriggerEnter2D(Collider2D other)
    {
        Vector2 contactPoint = other.gameObject.GetComponent<Collider2D>().ClosestPoint(transform.position);
        if (contactPoint.y < transform.position.y || contactPoint.y > transform.position.y + 0.9f)
            return;
        if (other.gameObject.tag == "Box" && _moveInput != Vector2.zero && _isGrounded)
        {
            _thingsColliding.Add(other);
            _anim.SetBool("Pushing", true);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Vector2 contactPoint = other.gameObject.GetComponent<Collider2D>().ClosestPoint(transform.position);
        if (contactPoint.y < transform.position.y || contactPoint.y > transform.position.y + 0.9f)
            return;
        if (other.gameObject.tag == "Box" && _moveInput != Vector2.zero && _isGrounded)
        {
            _anim.SetBool("Pushing", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _thingsColliding.Remove(other);
    }


    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if (_headHit.collider != null)
            _bumpedHead = true;
        else
            _bumpedHead = false;
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }
    #endregion

    #region Jump
    private void JumpChecks()
    {
        if (_jumpPress)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        if (_jumpRelease)
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }
            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isFastFalling = true;
                    _isPastApexThreshold = false;
                    _fastFallTime = 0f;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallTime = VerticalVelocity;
                }
            }

        }

        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);
        }

        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _jumpsUsed = 0;

            VerticalVelocity = -MoveStats.MaxFallSpeed;
        }
    }

    private void InitiateJump(int jumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _jumpsUsed += jumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        if (_jumpPress)
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (_isJumping)
        {
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }

            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }
                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.deltaTime;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                        _isPastApexThreshold = false;
                }
            }
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        if (_isFastFalling)
        {
            VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            _fastFallTime += Time.fixedDeltaTime;
        }

        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
                _isFalling = true;
            VerticalVelocity = -MoveStats.MaxFallSpeed;
        }

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);

        _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);
    }
    #endregion 

    #region Timers

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = MoveStats.JumpCoyoteTime;
        }
    }
    #endregion 
}
