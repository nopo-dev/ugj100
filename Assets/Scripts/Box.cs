using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Box : MonoBehaviour
{
    [Header("References")]
    public BoxStats MoveStats;
    [SerializeField] private Collider2D _bodyCollider;
    
    private Rigidbody2D _rb;
    private VelocityCalculator _velocityCalc;
    private Vector2 _moveVelocity;

    private RaycastHit2D[] _groundHits;
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

    // private Vector2 _xMovement;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _thingsColliding = new List<Collider2D>();
        _velocityCalc = GetComponent<VelocityCalculator>();
    }

    private void Update()
    {
        JumpChecks();
        if (_isGrounded && _thingsColliding.Count == 0)
        {
            _velocityCalc.Velocity = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, Vector2.zero);
            if (_groundObject != null)
            {
                _rb.velocity = _groundObject.GetComponent<VelocityCalculator>().Velocity;
            }
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, Vector2.zero);
        }
    }

    #region Move

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        }
        else
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        }
    }
    #endregion

    private GameObject _groundObject;
    #region Collision
    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_bodyCollider.bounds.center.x, _bodyCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_bodyCollider.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHits = Physics2D.BoxCastAll(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        foreach (RaycastHit2D groundHit in _groundHits)
        {
            if (groundHit.collider != null)
            {
                if (groundHit.collider.tag == "Box")
                {
                    _groundObject = groundHit.collider.transform.parent.transform.parent.gameObject;
                }
                // else if (groundHit.collider.tag == "Moving Platform")
                // {
                //     _groundObject = groundHit.collider.gameObject;
                // }
                if (_groundObject != null)
                {
                    if (_groundObject == gameObject)
                    {
                        break;
                    }
                    Vector2 contactPoint = groundHit.collider.gameObject.GetComponent<Collider2D>().ClosestPoint(transform.position);
                    transform.position = new Vector3(transform.position.x, contactPoint.y + 0.015f, transform.position.z);
                }
                _isGrounded = true;
                _rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                break;
            }
            else
            {
                _isGrounded = false;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _groundObject = null;
            }
        }
    }

    private void CollisionChecks()
    {
        IsGrounded();
    }

    private List<Collider2D> _thingsColliding;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        _thingsColliding.Add(other);
        _velocityCalc.TrackVelocity = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null)
            return;

        _thingsColliding.Remove(other);
        if (_thingsColliding.Count == 0)
        {
            _velocityCalc.TrackVelocity = false;
        }
    }

    // private void OnTriggerStay2D(Collider2D other)
    // {
    // }

    #endregion

    #region Jump
    private void JumpChecks()
    {
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;

            VerticalVelocity = -MoveStats.MaxFallSpeed;
        }
    }

    private void Jump()
    {
        if (_isJumping)
        {
            if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
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
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
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
}
