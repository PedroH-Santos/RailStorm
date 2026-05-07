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
    public class Teste : MonoBehaviour
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

        [Header("Knot Detection")]
        [Tooltip("Raio para ENTRAR na zona de decisão de um knot (metros)")]
        public float knotEnterRadius = 0.8f;
        [Tooltip("Raio para SAIR da zona — deve ser maior que knotEnterRadius")]
        public float knotExitRadius = 1.8f;
        [Tooltip("Dot mínimo para aceitar troca de spline")]
        public float minDecisionDot = 0.25f;

        // ── estado ───────────────────────────────────────────────────────────
        CharacterController _controller;
        StarterAssetsInputs _input;

        Spline _currentSpline;
        int _currentSplineIndex;
        float _currentT;
        float _splineLength;
        float _currentSpeed;
        float _lastDirection = 1f;
        float _rotationVelocity;

        bool _inKnotZone = false;
        int _activeKnotIndex = -1;
        bool _switchedThisZone = false;

        // ─────────────────────────────────────────────────────────────────────
        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            _currentSplineIndex = 0;
            _currentSpline = splineContainer.Splines.First();
            _splineLength = CalcLength(_currentSpline);
            _currentT = startT;
            _currentSpeed = idleSpeed;
        }

        void Update()
        {
            Vector3 tangent = GetTangent();
            Vector3 inputDir = GetInputDir();

            UpdateSpeed(inputDir, tangent);
            Move();
            Rotate(tangent);
            CheckKnot(inputDir);
        }

        // ── helpers ──────────────────────────────────────────────────────────
        float CalcLength(Spline s) =>
            SplineUtility.CalculateLength(s, splineContainer.transform.localToWorldMatrix);

        Vector3 GetInputDir()
        {
            Vector2 v = _input.move;
            return new Vector3(v.x, 0f, v.y).normalized;
        }

        Vector3 GetTangent()
        {
            Vector3 t = splineContainer.transform.TransformDirection(
                            _currentSpline.EvaluateTangent(_currentT));
            t.y = 0f;
            return t.normalized;
        }

        Vector3 GetWorldTangentAt(Spline spline, float t)
        {
            Vector3 tan = splineContainer.transform.TransformDirection(
                              spline.EvaluateTangent(t));
            tan.y = 0f;
            return tan.normalized;
        }

        float GetNearestT(Spline spline)
        {
            float3 localPos = splineContainer.transform.InverseTransformPoint(transform.position);
            SplineUtility.GetNearestPoint(spline, localPos, out _, out float nearestT);
            return nearestT;
        }

        // ── velocidade ───────────────────────────────────────────────────────
        void UpdateSpeed(Vector3 input, Vector3 tangent)
        {
            float dot = input.sqrMagnitude > 0.001f
                        ? Vector3.Dot(input, tangent) : 0f;

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

        // ── movimento ────────────────────────────────────────────────────────
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

        // ── rotação ──────────────────────────────────────────────────────────
        void Rotate(Vector3 tangent)
        {
            if (tangent.sqrMagnitude < 0.001f) return;

            Vector3 visual = tangent * _lastDirection;
            float targetY = Quaternion.LookRotation(visual).eulerAngles.y;

            float y = Mathf.SmoothDampAngle(
                          transform.eulerAngles.y, targetY,
                          ref _rotationVelocity, rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        // ── detecção de knot ─────────────────────────────────────────────────
        //
        // • Entra na zona  (dist < knotEnterRadius) → marca zona ativa
        // • Dentro da zona → tenta trocar A CADA FRAME que tiver input
        //                    até conseguir trocar ou sair da zona
        // • Sai da zona    (dist > knotExitRadius)  → libera para próximo knot
        //
        void CheckKnot(Vector3 inputDir)
        {
            int knotCount = _currentSpline.Knots.Count();

            for (int i = 0; i < knotCount; i++)
            {
                Vector3 knotWorld = splineContainer.transform.TransformPoint(
                    (Vector3)(float3)_currentSpline.Knots.ElementAt(i).Position);

                float dist = Vector3.Distance(transform.position, knotWorld);

                // Entrada na zona
                if (!_inKnotZone && dist < knotEnterRadius)
                {
                    _inKnotZone = true;
                    _activeKnotIndex = i;
                    _switchedThisZone = false;
                }

                // Processamento contínuo dentro da zona ativa
                if (_inKnotZone && _activeKnotIndex == i)
                {
                    // Tenta a cada frame enquanto tiver input e não tiver trocado
                    if (!_switchedThisZone && inputDir.sqrMagnitude > 0.001f)
                    {
                        bool switched = TrySwitchSpline(i, inputDir);
                        if (switched) _switchedThisZone = true;
                    }

                    // Saída da zona
                    if (dist > knotExitRadius)
                    {
                        _inKnotZone = false;
                        _activeKnotIndex = -1;
                        _switchedThisZone = false;
                    }

                    return;
                }
            }
        }

        // ── troca de spline ──────────────────────────────────────────────────
        bool TrySwitchSpline(int knotIndex, Vector3 inputDir)
        {
            var links = splineContainer.KnotLinkCollection;
            if (links == null) return false;

            var currentKnot = new SplineKnotIndex
            {
                Spline = _currentSplineIndex,
                Knot = knotIndex
            };

            if (!links.TryGetKnotLinks(currentKnot, out var linkedKnots))
                return false;

            var candidates = new List<SplineKnotIndex>();
            foreach (var k in linkedKnots)
                if (k.Spline != _currentSplineIndex)
                    candidates.Add(k);

            if (candidates.Count == 0) return false;

            float bestDot = minDecisionDot;
            SplineKnotIndex bestTarget = default;
            float bestDir = 1f;
            bool found = false;

            foreach (var target in candidates)
            {
                Spline targetSpline = splineContainer.Splines[target.Spline];
                float nearestT = GetNearestT(targetSpline);
                Vector3 tangent = GetWorldTangentAt(targetSpline, nearestT);

                float dotFwd = Vector3.Dot(inputDir, tangent);
                float dotBwd = Vector3.Dot(inputDir, -tangent);

                if (dotFwd > bestDot) { bestDot = dotFwd; bestTarget = target; bestDir = 1f; found = true; }
                if (dotBwd > bestDot) { bestDot = dotBwd; bestTarget = target; bestDir = -1f; found = true; }
            }

            if (!found) return false;

            SwitchSpline(bestTarget, bestDir);
            return true;
        }

        void SwitchSpline(SplineKnotIndex target, float direction)
        {
            _currentSplineIndex = target.Spline;
            _currentSpline = splineContainer.Splines[_currentSplineIndex];
            _splineLength = CalcLength(_currentSpline);

            float3 localPos = splineContainer.transform.InverseTransformPoint(transform.position);
            SplineUtility.GetNearestPoint(_currentSpline, localPos, out float3 nearestPoint, out float nearestT);

            transform.position = splineContainer.transform.TransformPoint((Vector3)nearestPoint);
            _currentT = nearestT;
            _lastDirection = direction;

            _inKnotZone = false;
            _activeKnotIndex = -1;
            _switchedThisZone = false;
        }
    }
}
