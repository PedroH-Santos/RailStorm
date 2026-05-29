using UnityEngine;
using UnityEngine.Splines;
using System.Linq;
using Unity.Mathematics;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Spline")]
        public SplineContainer splineContainer;
        [Range(0f, 1f)]
        public float startT = 0f;

        [Header("Junction")]
        public float knotThreshold = 0.8f;
        public float minDecisionDot = 0.3f;

        PlayerStatsAggregator _stats;
        CharacterController _controller;
        PlayerInputReader _input;

        Spline _currentSpline;
        int _currentSplineIndex;
        float _currentT;
        float _splineLength;
        float _currentSpeed;
        float _lastDirection = 1f;
        float _rotationVelocity = 0f;
        bool _movementLocked = false;

        public int CurrentSplineIndex => _currentSplineIndex;
        public float LastDirection => _lastDirection;
        public float CurrentSpeed => _currentSpeed;
        public Vector2 RawInput => _input.Move;

        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputReader>();
            _stats = GetComponent<PlayerStatsAggregator>();

            _currentSplineIndex = 0;
            _currentSpline = splineContainer.Splines.First();
            _splineLength = CalcLength(_currentSpline);
            _currentT = startT;
            _currentSpeed = _stats.IdleSpeed;

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
            return new Vector3(_input.Move.x, 0f, _input.Move.y).normalized;
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
                _currentSpeed = Mathf.Lerp(_currentSpeed, _stats.IdleSpeed,
                                            Time.deltaTime * _stats.Deceleration);
                return;
            }

            float dot = Vector3.Dot(inputDir, tangent);

            if (dot > 0.4f)
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, _stats.MoveSpeed,
                                             Time.deltaTime * _stats.Acceleration);
                _lastDirection = 1f;
            }
            else if (dot < -0.4f)
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, _stats.MoveSpeed,
                                             Time.deltaTime * _stats.Acceleration);
                _lastDirection = -1f;
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, _stats.IdleSpeed,
                                            Time.deltaTime * _stats.Deceleration);
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

            Vector3 pos = splineContainer.transform.TransformPoint(
                              _currentSpline.EvaluatePosition(_currentT));
            _controller.Move(pos - transform.position);
        }

        void Rotate(Vector3 tangent)
        {
            if (tangent.sqrMagnitude < 0.001f) return;
            float targetY = Quaternion.LookRotation(tangent * _lastDirection).eulerAngles.y;
            float y = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetY,
                                             ref _rotationVelocity, _stats.RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        float CalcLength(Spline s) =>
            SplineUtility.CalculateLength(s, splineContainer.transform.localToWorldMatrix);

        public void SwitchToSplineIndex(int splineIndex, float direction, float speedOverride)
        {
            if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return;

            Spline newSpline = splineContainer.Splines[splineIndex];

            float3 localPos = splineContainer.transform.InverseTransformPoint(transform.position);
            SplineUtility.GetNearestPoint(newSpline, localPos, out _, out float nearestT);

            _currentSplineIndex = splineIndex;
            _currentSpline = newSpline;
            _lastDirection = direction;
            _currentT = nearestT;
            _currentSpeed = speedOverride;
            _splineLength = CalcLength(newSpline);
        }

        public void SetMovementLocked(bool locked)
        {
            _movementLocked = locked;
            if (locked) _currentSpeed = 0f;
        }
    }
}