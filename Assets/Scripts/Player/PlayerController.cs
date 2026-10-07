using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        const float DirectionInputThreshold = 0.4f;
        const float NoInputSqr = 0.001f;
        const float FlatTangentSqr = 0.001f;
        const float Forward = 1f;
        const float Backward = -1f;

        [Header("Spline")]
        public SplineContainer splineContainer;
        [Range(0f, 1f)]
        public float startT = 0f;

        PlayerStatsAggregator _stats;
        CharacterController _controller;
        PlayerInputReader _input;

        Spline _currentSpline;
        int _currentSplineIndex;
        float _splineLength;
        float _progressOnSpline;
        float _currentSpeed;
        float _travelDirection = Forward;
        float _turnVelocity;
        bool _movementLocked;

        public int CurrentSplineIndex => _currentSplineIndex;
        public float CurrentSpeed => _currentSpeed;
        public Vector2 RawInput => _input.Move;

        public event Action OnSplineSwitched;

        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputReader>();
            _stats = GetComponent<PlayerStatsAggregator>();

            EnterSpline(0, splineContainer.Splines.First(), startT);
            _currentSpeed = _stats.IdleSpeed;

            ShowMouseCursor();
        }

        void Update()
        {
            if (_movementLocked) return;

            Vector3 trackDirection = TrackDirectionHere();
            UpdateSpeedAndDirection(InputDirection(), trackDirection);
            MoveAlongTrack();
            FaceTravelDirection(trackDirection);
        }

        public void SwitchToSplineIndex(int splineIndex, float travelDirection, float speed)
        {
            if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return;

            Spline newSpline = splineContainer.Splines[splineIndex];
            EnterSpline(splineIndex, newSpline, NearestProgressOn(newSpline));
            _travelDirection = travelDirection;
            _currentSpeed = speed;

            OnSplineSwitched?.Invoke();
        }

        public void SetMovementLocked(bool locked)
        {
            _movementLocked = locked;
            if (locked) _currentSpeed = 0f;
        }

        void EnterSpline(int splineIndex, Spline spline, float progress)
        {
            _currentSplineIndex = splineIndex;
            _currentSpline = spline;
            _splineLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);
            _progressOnSpline = progress;
        }

        float NearestProgressOn(Spline spline)
        {
            float3 localPosition = splineContainer.transform.InverseTransformPoint(transform.position);
            SplineUtility.GetNearestPoint(spline, localPosition, out _, out float nearestProgress);
            return nearestProgress;
        }

        Vector3 InputDirection() => new Vector3(_input.Move.x, 0f, _input.Move.y).normalized;

        Vector3 TrackDirectionHere()
        {
            Vector3 tangent = splineContainer.transform.TransformDirection(_currentSpline.EvaluateTangent(_progressOnSpline));
            tangent.y = 0f;
            return tangent.sqrMagnitude < FlatTangentSqr ? transform.forward : tangent.normalized;
        }

        void UpdateSpeedAndDirection(Vector3 input, Vector3 trackDirection)
        {
            if (TryGetDirectionAlongTrack(input, trackDirection, out float direction))
            {
                _travelDirection = direction;
                ChangeSpeedTowards(_stats.MoveSpeed, _stats.Acceleration);
            }
            else
            {
                ChangeSpeedTowards(_stats.IdleSpeed, _stats.Deceleration);
            }
        }

        static bool TryGetDirectionAlongTrack(Vector3 input, Vector3 trackDirection, out float direction)
        {
            direction = 0f;
            if (input.sqrMagnitude < NoInputSqr) return false;

            float alignment = Vector3.Dot(input, trackDirection);
            if (alignment > DirectionInputThreshold) direction = Forward;
            else if (alignment < -DirectionInputThreshold) direction = Backward;

            return direction != 0f;
        }

        void ChangeSpeedTowards(float targetSpeed, float rate)
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * rate);
        }

        void MoveAlongTrack()
        {
            float progressThisFrame = _currentSpeed / _splineLength * Time.deltaTime;
            _progressOnSpline = KeepOnSpline(_progressOnSpline + _travelDirection * progressThisFrame);

            Vector3 target = splineContainer.transform.TransformPoint(_currentSpline.EvaluatePosition(_progressOnSpline));
            _controller.Move(target - transform.position);
        }

        float KeepOnSpline(float progress) => _currentSpline.Closed ? Mathf.Repeat(progress, 1f) : Mathf.Clamp01(progress);

        void FaceTravelDirection(Vector3 trackDirection)
        {
            float targetYaw = Quaternion.LookRotation(trackDirection * _travelDirection).eulerAngles.y;
            float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref _turnVelocity, _stats.RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        static void ShowMouseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
