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

    private Vector2 _moveVelocity;

    private RaycastHit2D _groundHit;
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
    }

    private void Update()
    {
        JumpChecks();
        // ResetPushCheck();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, Vector2.zero);
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

    #region Collision
    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_bodyCollider.bounds.center.x, _bodyCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_bodyCollider.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (_groundHit.collider != null)
        {
            _isGrounded = true;
            _rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            _isGrounded = false;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
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
        switch (other.gameObject.tag)
        {
            case "Player":
                if (other.gameObject.transform.parent.transform.parent.GetComponent<DudeController>().Grounded)
                    GetComponent<Rigidbody2D>().bodyType = 0f;
                break;
            case "Mirror":
                if (other.gameObject.transform.parent.transform.parent.GetComponent<MirrorController>().Grounded)
                        GetComponent<Rigidbody2D>().mass = 0f;
                break;
            case "Box":
                if (other.gameObject.transform.parent.transform.parent.GetComponent<Box>().Grounded)
                        GetComponent<Rigidbody2D>().mass = 0f;
                break;
            default:
                Debug.Log("something unaccounted for has happened");
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null)
            return;

        _thingsColliding.Remove(other);
        if (_thingsColliding.Count == 0)
        {
            GetComponent<Rigidbody2D>().mass = 1f;
            return;
        }
        switch (other.gameObject.tag)
        {
            case "Player":
                if (other.gameObject.transform.parent.transform.parent.GetComponent<DudeController>().Grounded)
                    GetComponent<Rigidbody2D>().mass = 1f;
                break;
            case "Mirror":
                if (other.gameObject.transform.parent.transform.parent.GetComponent<MirrorController>().Grounded)
                        GetComponent<Rigidbody2D>().mass = 1f;
                break;
            case "Box":
                if (other.gameObject.transform.parent.transform.parent.GetComponent<Box>().Grounded)
                        GetComponent<Rigidbody2D>().mass = 1f;
                break;
            default:
                Debug.Log("something unaccounted for has happened");
                break;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        foreach (Collider2D coll in _thingsColliding)
        {
            switch (coll.gameObject.tag)
            {
                case "Player":
                    if (coll.gameObject.transform.parent.transform.parent.GetComponent<DudeController>().Grounded)
                    {
                        GetComponent<Rigidbody2D>().mass = 0f;
                        return;
                    }
                    break;
                case "Mirror":
                    if (coll.gameObject.transform.parent.transform.parent.GetComponent<MirrorController>().Grounded)
                    {
                        GetComponent<Rigidbody2D>().mass = 0f;
                        return;
                    }
                    break;
                case "Box":
                    if (coll.gameObject.transform.parent.transform.parent.GetComponent<Box>().Grounded)
                    {
                        GetComponent<Rigidbody2D>().mass = 0f;
                        return;
                    }
                    break;
                default:
                    Debug.Log("something unaccounted for has happened");
                    break;
            }
        }
        GetComponent<Rigidbody2D>().mass = 1f;
    }

    // public void Push(Vector2 direction)
    // {
    //     _xMovement += direction;
    // }

    // private void ResetPushCheck()
    // {
    //     if 
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
