using UnityEngine;
using UnityEngine.Splines;
using System.Linq;
using Unity.Mathematics;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class PlayerController : MonoBehaviour
    {
        [Header("Spline")]
        public SplineContainer splineContainer;
        [Range(0f, 1f)]
        public float startT = 0f;

        [Header("Movement")]
        public float idleSpeed = 0.5f;
        public float acceleration = 6f;
        public float deceleration = 4f;
        public float rotationSmoothTime = 0.12f;

        [Header("Junction")]
        public float knotThreshold = 0.8f;
        public float minDecisionDot = 0.3f;

        PlayerStatsAggregator _stats;
        CharacterController _controller;
        StarterAssetsInputs _input;

        Spline _currentSpline;
        int _currentSplineIndex;
        float _currentT;
        float _distance;
        float _splineLength;
        float _currentSpeed;
        float _lastDirection = 1f;
        float _rotationVelocity = 0f;
        bool _movementLocked = false;

        public int CurrentSplineIndex => _currentSplineIndex;
        public Vector2 RawInput => _input.move;
        public float LastDirection => _lastDirection;
        public float CurrentSpeed => _currentSpeed;

        void Awake() => _stats = GetComponent<PlayerStatsAggregator>();

        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            _currentSplineIndex = 0;
            _currentSpline = splineContainer.Splines.First();
            _splineLength = CalcLength(_currentSpline);
            _currentT = startT;
            _distance = _currentT * _splineLength;
            _currentSpeed = idleSpeed;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            if (_movementLocked) return;

            Vector3 tangent = GetTangent();
            Vector3 inputDir = GetInputDir();

            UpdateSpeed(inputDir, tangent);
            Move();
            Rotate(tangent);
        }

        public Vector3 GetInputDir()
        {
            Vector2 v = _input.move;
            return new Vector3(v.x, 0f, v.y).normalized;
        }

        Vector3 GetTangent()
        {
            Vector3 t = splineContainer.transform.TransformDirection(
                            _currentSpline.EvaluateTangent(_currentT));
            t.y = 0f;
            return t.sqrMagnitude < 0.001f ? transform.forward : t.normalized;
        }

        void UpdateSpeed(Vector3 inputDir, Vector3 tangent)
        {
            if (inputDir.sqrMagnitude < 0.001f)
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, idleSpeed,
                                            Time.deltaTime * deceleration);
                return;
            }

            float dot = Vector3.Dot(inputDir, tangent);

            if (dot > 0.4f)
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, _stats.MoveSpeed,
                                             Time.deltaTime * acceleration);
                _lastDirection = 1f;
            }
            else if (dot < -0.4f)
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, _stats.MoveSpeed,
                                             Time.deltaTime * acceleration);
                _lastDirection = -1f;
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, idleSpeed,
                                            Time.deltaTime * deceleration);
            }
        }

        void Move()
        {
            float deltaT = (_currentSpeed / _splineLength) * Time.deltaTime;
            _currentT += _lastDirection * deltaT;

            if (_currentSpline.Closed)
                _currentT = Mathf.Repeat(_currentT, 1f);
            else
                _currentT = Mathf.Clamp01(_currentT);

            _distance = _currentT * _splineLength;

            Vector3 pos = splineContainer.transform.TransformPoint(
                              _currentSpline.EvaluatePosition(_currentT));
            _controller.Move(pos - transform.position);
        }

        void Rotate(Vector3 tangent)
        {
            if (tangent.sqrMagnitude < 0.001f) return;
            float targetY = Quaternion.LookRotation(tangent * _lastDirection).eulerAngles.y;
            float y = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetY,
                                             ref _rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        /// <summary>
        /// Troca de spline.
        ///
        /// knotWorldPos: posição world do knot de junção na nova spline,
        /// calculada pelo SplineCollision antes de chamar este método.
        /// Usamos ela para ancorar _currentT exatamente no knot —
        /// sem GetNearestPoint (que pode errar em splines com curvas),
        /// sem offset para a frente.
        ///
        /// direction e speed são passados explicitamente para que o
        /// UpdateSpeed não cause desaceleração nos frames logo após a troca.
        /// </summary>
        public void SwitchToSplineIndex(int splineIndex, float direction,
                                        float knotT, float speedOverride)
        {
            if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return;

            Spline newSpline = splineContainer.Splines[splineIndex];

            _currentSplineIndex = splineIndex;
            _currentSpline = newSpline;
            _lastDirection = direction;
            _currentT = knotT;
            _currentSpeed = speedOverride;   // evita desaceleração pós-troca
            _splineLength = CalcLength(newSpline);
            _distance = _currentT * _splineLength;
        }

        float CalcLength(Spline s) =>
            SplineUtility.CalculateLength(s, splineContainer.transform.localToWorldMatrix);

        public void SetMovementLocked(bool locked)
        {
            _movementLocked = locked;
            if (locked) _currentSpeed = 0f;
        }

        public void SpendCoins(int amount) =>
            _stats.Coins = Mathf.Max(0, _stats.Coins - amount);
    }
}