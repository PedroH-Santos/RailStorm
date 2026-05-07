using UnityEngine;
using UnityEngine.Splines;
using System.Linq;
using Unity.Mathematics;
using System.Collections.Generic;

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
        public float moveSpeed = 6f;
        public float idleSpeed = 0.5f;
        public float acceleration = 6f;
        public float deceleration = 4f;
        public float rotationSmoothTime = 0.12f;

        [Header("Junction")]
        public float knotThreshold = 0.8f;  // world-space distance to trigger junction
        public float minDecisionDot = 0.3f;

        CharacterController _controller;
        StarterAssetsInputs _input;

        Spline _currentSpline;
        int _currentSplineIndex;
        float _currentT;
        float _distance;
        float _splineLength;
        float _currentSpeed;
        float _lastDirection = 1f;
        int _lastProcessedKnot = -1;
        float _rotationVelocity = 0f;
        public int CurrentSplineIndex => _currentSplineIndex;
        public Vector2 RawInput => _input.move;

        public float LastDirection => _lastDirection;


        [Header("Economy")]
        public int Coins = 50;

        bool _movementLocked = false;


        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            _currentSplineIndex = 0;
            _currentSpline = splineContainer.Splines.First();

            _splineLength = SplineUtility.CalculateLength(
                _currentSpline,
                splineContainer.transform.localToWorldMatrix
            );

            _currentT = startT;
            _distance = _currentT * _splineLength;
            _currentSpeed = idleSpeed;
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
            Vector2 input = _input.move;
            return new Vector3(input.x, 0f, input.y).normalized;
        }

        Vector3 GetTangent()
        {
            float3 localTangent = _currentSpline.EvaluateTangent(_currentT);
            Vector3 worldTangent = splineContainer.transform.TransformDirection(localTangent);
            worldTangent.y = 0f;
            if (worldTangent.sqrMagnitude < 0.001f) return transform.forward;
            return worldTangent.normalized;
        }

        void UpdateSpeed(Vector3 inputDir, Vector3 tangent)
        {
            if (inputDir.sqrMagnitude < 0.001f)
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, idleSpeed, Time.deltaTime * deceleration);
                return;
            }

            float dot = Vector3.Dot(inputDir, tangent);

            if (dot > 0.4f)
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, moveSpeed, Time.deltaTime * acceleration);
                _lastDirection = 1f;
            }
            else if (dot < -0.4f)
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, moveSpeed, Time.deltaTime * acceleration);
                _lastDirection = -1f;
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, idleSpeed, Time.deltaTime * deceleration);
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
                _currentSpline.EvaluatePosition(_currentT)
            );
            _controller.Move(pos - transform.position);
        }

        void Rotate(Vector3 tangent)
        {
            if (tangent.sqrMagnitude < 0.001f) return;

            Vector3 faceDir = tangent * _lastDirection;
            float targetY = Quaternion.LookRotation(faceDir).eulerAngles.y;

            float y = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, targetY,
                ref _rotationVelocity, rotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        public void SwitchToSplineIndex(int splineIndex, float direction)
        {
            if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return;

            Spline newSpline = splineContainer.Splines[splineIndex];

            // 1. Find the knot on the new spline closest to the player (XZ only)
            int closestKnot = -1;
            float closestDist = float.MaxValue;

            for (int i = 0; i < newSpline.Count; i++)
            {
                Vector3 knotWorld = splineContainer.transform.TransformPoint(newSpline[i].Position);
                float dist = Vector3.Distance(
                    new Vector3(transform.position.x, 0f, transform.position.z),
                    new Vector3(knotWorld.x, 0f, knotWorld.z));

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestKnot = i;
                }
            }

            if (closestKnot < 0) return;

            // 2. Get the knot's world position DIRECTLY — no GetNearestPoint, no T math
            Vector3 exactKnotWorld = splineContainer.transform.TransformPoint(
                                         newSpline[closestKnot].Position);

            // 3. Walk the spline curve to find the T that actually maps to this knot's position.
            //    We do a brute-force sample because GetNormalizedInterpolation is unreliable
            //    on non-uniform splines (curve lengths differ between knots).
            float bestT = 0f;
            float bestDist = float.MaxValue;
            int samples = 200;

            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;
                Vector3 p = splineContainer.transform.TransformPoint(newSpline.EvaluatePosition(t));
                float d = Vector3.Distance(
                    new Vector3(p.x, 0f, p.z),
                    new Vector3(exactKnotWorld.x, 0f, exactKnotWorld.z));

                if (d < bestDist)
                {
                    bestDist = d;
                    bestT = t;
                }
            }

            // 4. Apply
            _currentSplineIndex = splineIndex;
            _currentSpline = newSpline;
            _lastDirection = direction;
            _currentT = bestT;

            _splineLength = SplineUtility.CalculateLength(
                                _currentSpline,
                                splineContainer.transform.localToWorldMatrix);

            // 5. Place the player at the knot world position — guaranteed exact
            _controller.enabled = false;
            transform.position = exactKnotWorld;
            _controller.enabled = true;

           // Debug.Log($"[Switch] spline={splineIndex} knot={closestKnot} T={bestT:F3} pos={exactKnotWorld}");
        }

        public void SetMovementLocked(bool locked)
        {
            _movementLocked = locked;
            if (locked) _currentSpeed = 0f;
        }

        public void SpendCoins(int amount)
        {
            Coins = Mathf.Max(0, Coins - amount);
        }

    }
}