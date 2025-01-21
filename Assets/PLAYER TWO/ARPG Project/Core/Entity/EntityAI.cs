using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Entity))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Entity/Entity AI")]
    public class EntityAI : MonoBehaviour
    {
        [Header("General Settings")]
        [Tooltip("If true, the Entity will ignore the culling system.")]
        public bool ignoreCulling;

        [Header("Detection Settings")]
        [Tooltip("The tags of the Game Objects that this Entity will identify as its target.")]
        public List<string> targetTags;

        [Tooltip("The minimum radius to spot a new target.")]
        public float spotRadius = 5f;

        [Header("Field of View Settings")]
        [Tooltip("Angle of the field of view for spotting targets.")]
        public float viewAngle = 110f; // Example angle in degrees

        [Tooltip("The maximum distance the AI can spot an entity within the view angle.")]
        public float viewDistance = 10f;

        [Tooltip("The minimum distance to space from the Entity view sight after being detected.")]
        public float fleeRadius = 10f;

        [Header("Attack Settings")]
        [Tooltip("If true, the Entity will always attack with the current Skill.")]
        public bool useSkill;

        [Tooltip("A delay in seconds before the Entity starts patrolling.")]
        public float resetMoveDelay = 1f;

        [Tooltip("A delay in seconds between attacks.")]
        public float attackCoolDown = 0.5f;

        [Tooltip("If true, this Entity will add itself to the attackers list of the target.")]
        public bool addToAttackersList = true;

        [Tooltip("If true, this Entity will not attack another if the maximum number of attackers has already attacked that.")]
        public bool limitSimultaneousAttackers = true;

        [Tooltip("If the maximum number of attackers attacks the target, this Entity will not attack it. This value is ignored if Limit Simultaneous Attackers is off.")]
        public int maxSimultaneousAttackers = 2;

        [Header("Search Settings")]
        [Tooltip("If true, this Entity will search the origin of the last damage it take.")]
        public bool searchDamageSource = true;

        [Tooltip("A delay in seconds before starting to look to the last damage origin.")]
        public float searchDamageSourceDelay = 0.3f;

        [Tooltip("The duration in seconds in which this Entity will search for the damage origin.")]
        public float searchDamageSourceDuration = 5f;

        [Header("Companion Settings")]
        [Tooltip("The leader of this AI. It's better to set this from code.")]
        public Entity leader;

        [Tooltip("If true, this AI will try to stay close to its leader.")]
        public bool followLeader;

        [Tooltip("If true, this AI will die with its associated leader.")]
        public bool dieWithLeader = true;

        [Header("Gizmo's Settings")]
        [Tooltip("Height above the ground to draw the spot radius.")]
        public float spotRadiusYOffset = -0.98f;

        [Tooltip("Height above the ground to draw the flee radius.")]
        public float fleeRadiusYOffset = -0.98f;

        [Tooltip("Y Offset for the field of view gizmo's.")]
        public float viewAngleYOffset = -0.98f;

        [Tooltip("Color for the spot radius visualization.")]
        public Color spotRadiusColor = Color.blue;

        [Tooltip("Color for the flee radius visualization.")]
        public Color fleeRadiusColor = Color.red;

        [Tooltip("Color for the field of view visualization.")]
        public Color fieldOfViewColor = Color.green; // Add this line for a new color

        [Tooltip("Number of lines to draw for the circle outline. More lines result in a smoother outline.")]
        public int outlineLines = 36;
        public virtual bool isAgent => m_entity != null && m_entity.isAgent;
        
        protected Entity m_entity;
        protected Camera m_camera;

        protected int m_totalTargetsInSight;
        protected float m_lastAttackTime;
        protected float m_waitingToSearchTime;
        protected float m_nextTargetRefreshTime;
        protected bool m_waitingToSearch;

        protected Plane[] m_frustumPlanes = new Plane[6];
        protected Collider[] m_targetsInSight = new Collider[128];
        protected WaitForSeconds m_resetMoveDelay;
        protected WaitForSeconds m_searchDamageSourceDuration;

        protected const float k_targetRefreshRate = 0.2f;

        /// <summary>
        /// And offset applied to the leader following target position.
        /// </summary>
        public Vector3 leaderOffset { get; set; }


        /// <summary>
        /// Returns the bounding box of this Entity.
        /// </summary>
        public virtual Bounds bounds => m_entity.controller.bounds;

        /// <summary>
        /// Returns true if this Entity is dead.
        /// </summary>
        public virtual bool isDead => m_entity != null && m_entity.stats != null && m_entity.isDead;

        /// <summary>
        /// Returns true if the Entity is able to move.
        /// </summary>
        public virtual bool canMove => !m_entity.isBlocking && !m_entity.isStunned;

        protected virtual void InitializeWaits()
        {
            m_resetMoveDelay = new WaitForSeconds(resetMoveDelay);
            m_searchDamageSourceDuration = new WaitForSeconds(searchDamageSourceDuration);
        }

        protected virtual void InitializeCamera() => m_camera = Camera.main;

        protected virtual void InitializeEntity()
        {
            m_entity = GetComponent<Entity>();
            m_entity.states.ChangeTo<RandomMovementEntityState>();
            m_entity.useSkill = useSkill;
            m_entity.targetTags = targetTags;
        }

        protected virtual void InitializeCallback()
        {
            m_entity.onDamage.AddListener(OnDamage);
            m_entity.onDie.AddListener(OnDie);
        }

        protected virtual void InitializeLeader()
        {
            if (!leader) return;

            if (dieWithLeader)
                leader.onDie.AddListener(() => m_entity.Die());
        }

        protected virtual void HandleEntityOptimization()
        {
            if (m_entity.isAgent)
            {
                m_entity.enabled = true; // AI Agent jest zawsze aktywny
                return;
            }

            GeometryUtility.CalculateFrustumPlanes(m_camera, m_frustumPlanes);
            m_entity.enabled = GeometryUtility.TestPlanesAABB(m_frustumPlanes, m_entity.controller.bounds);

            if (!m_entity.isActive && !m_entity.enabled)
                gameObject.SetActive(false);
        }

        protected virtual void RegisterAI() => LevelAIManager.instance.RegisterAI(this);

        protected virtual void HandleViewSight()
        {
            // Wyłącz obsługę ataku dla AI Agenta
            if (m_entity.isAgent)
            {
                return; // AI Agent nie wymaga logiki ataku
            }
            
            if (m_entity.target || Time.time < m_nextTargetRefreshTime) return;

            m_nextTargetRefreshTime = Time.time + k_targetRefreshRate;
            SearchTarget();
        }

        protected virtual void SearchTarget(bool ignoreSimultaneousAttacks = false)
        {
            if (m_entity.target) return;

            // Detect targets within the spot radius (360-degree check)
            Collider[] targetsInSpotRadius = Physics.OverlapSphere(transform.position, spotRadius);
            foreach (var potentialTarget in targetsInSpotRadius)
            {
                if (GameTags.InTagList(potentialTarget, targetTags))
                {
                    SpotTarget(potentialTarget.GetComponent<Entity>(), ignoreSimultaneousAttacks);
                    return; // Exit if a target is found within the spot radius
                }
            }

            // Detect targets within the view distance and angle (conical check)
            m_totalTargetsInSight = Physics.OverlapSphereNonAlloc(transform.position, viewDistance, m_targetsInSight);

            Vector3 directionToTarget;
            float distanceToTarget;
            Entity entityComponent;

            for (int i = 0; i < m_totalTargetsInSight; i++)
            {
                if (m_targetsInSight[i] != null)
                {
                    directionToTarget = (m_targetsInSight[i].transform.position - transform.position).normalized;
                    distanceToTarget = Vector3.Distance(transform.position, m_targetsInSight[i].transform.position);
                    entityComponent = m_targetsInSight[i].GetComponent<Entity>();

                    // Check if potential target is within the field of view angle
                    if (Vector3.Angle(transform.forward, directionToTarget) <= viewAngle / 2 && distanceToTarget <= viewDistance)
                    {
                        if (GameTags.InTagList(m_targetsInSight[i], targetTags) && entityComponent != null)
                        {
                            SpotTarget(entityComponent, ignoreSimultaneousAttacks);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Assign a target to the Entity and handles its attackers list.
        /// </summary>
        /// <param name="target">The target you want to assign.</param>
        /// <param name="ignoreSimultaneousAttacks">If true, the Entity will attack the target even if the maximum number of attackers has already attacked it.</param>
        public virtual void SpotTarget(Entity target, bool ignoreSimultaneousAttacks = false)
        {
            if (!ignoreSimultaneousAttacks && limitSimultaneousAttackers &&
                target.attackedBy.Count >= maxSimultaneousAttackers)
            {
                return;
            }

            StopAllCoroutines();
            m_entity.SetTarget(target.transform);

            if (m_entity.targetEntity && addToAttackersList)
                m_entity.targetEntity.attackedBy.Add(m_entity);

            // Logowanie rozpoczęcia walki - DODANO 29 GRUDNIA 2024 - 0001
            var playerLogger = Object.FindFirstObjectByType<PlayerBehaviorLogger>();
            if (playerLogger != null)
            {
                playerLogger.StartCombat();
            }
        }

        protected virtual void HandleTargetFlee()
        {
            // Wyłącz obsługę ataku dla AI Agenta
            if (m_entity.isAgent)
            {
                return; // AI Agent nie wymaga logiki ataku
            }
            
            if (!m_entity.target) return;

            if (m_entity.GetDistanceToTarget() > fleeRadius) StopAttack();
        }

        protected virtual void HandleAttack()
        {
            // Wyłącz obsługę ataku dla AI Agenta
            if (m_entity.isAgent)
            {
                return; // AI Agent nie wymaga logiki ataku
            }

            if (!m_entity.target || m_entity.isAttacking || !canMove)
                return;

            if (m_entity.IsCloseToAttackTarget())
            {
                if (Time.time - m_lastAttackTime > attackCoolDown)
                {
                    m_lastAttackTime = Time.time + m_entity.skillDuration;
                    m_entity.Attack();

                    if (!m_entity.target || (m_entity.targetEntity && m_entity.targetEntity.isDead))
                        StopAttack();
                }
                else
                {
                    m_entity.StandStill();
                }
            }
            else
            {
                m_entity.MoveToTarget();
            }
        }

        protected virtual void HandleFollowing()
        {
            // Wyłącz obsługę ataku dla AI Agenta
            if (m_entity.isAgent)
            {
                return; // AI Agent nie wymaga logiki ataku
            }

            if (!followLeader || !leader || m_entity.target ||
                m_entity.isAttacking || !canMove)
                return;

            var destination = leader.position + leaderOffset;
            m_entity.MoveTo(destination);
        }

        protected virtual void OnDamage(int amount, Vector3 source, bool critical)
        {
            if (m_entity.target) return;

            if (m_entity.GetDistanceTo(source) < spotRadius)
            {
                SearchTarget(true);
            }
            else if (searchDamageSource)
            {
                StopAllCoroutines();
                StartCoroutine(SearchDamageSourceRoutine(source));
            }
        }

        protected virtual void OnDie()
        {
            StopAllCoroutines();
            LoseTarget();

            // Logowanie śmierci przeciwnika - DODANO 29 GRUDNIA 2024 - 0001
            var playerLogger = Object.FindFirstObjectByType<PlayerBehaviorLogger>();
            if (playerLogger != null)
            {
                playerLogger.LogDifficultyMultiplier();
                playerLogger.LogEnemiesDefeated();
            }
        }

        /// <summary>
        /// Makes the Entity stop attacking its assigned target.
        /// </summary>
        public virtual void StopAttack()
        {
            StopAllCoroutines();
            StartCoroutine(StopAttackRoutine());
        }

        /// <summary>
        /// Removes the target of the Entity.
        /// </summary>
        public virtual void LoseTarget()
        {
            m_entity.targetEntity?.attackedBy.Remove(m_entity);
            m_entity.SetTarget(null);

            // Logowanie zakończenia walki - DODANO 29 GRUDNIA 2024 - 0001
            var playerLogger = Object.FindFirstObjectByType<PlayerBehaviorLogger>();
            if (playerLogger != null)
            {
                playerLogger.EndCombat();
            }
        }

        protected virtual IEnumerator StopAttackRoutine()
        {
            LoseTarget();
            m_entity.StandStill();
            yield return m_resetMoveDelay;
            m_entity.StartRandomMovement();
        }

        protected virtual IEnumerator SearchDamageSourceRoutine(Vector3 source)
        {
            m_entity.StandStill();

            if (!m_waitingToSearch)
            {
                m_waitingToSearch = true;
                m_waitingToSearchTime = Time.time;
            }

            while (Time.time - m_waitingToSearchTime < searchDamageSourceDelay)
            {
                yield return null;
            }

            m_entity.MoveTo(source);
            yield return m_searchDamageSourceDuration;
            m_entity.StartRandomMovement();
            m_waitingToSearch = false;
        }

        protected virtual bool CanUpdateAI()
        {
            if (m_entity.isAgent)
            {
                return true; // AI Agent jest zawsze aktywny
            }

            return gameObject.activeSelf && m_entity.enabled && m_entity.isActive;
        }

        public virtual void AIUpdate()
        {
            if (CanUpdateAI())
            {
                HandleViewSight();
                HandleTargetFlee();
            }
        }

        protected virtual void Start()
        {
            InitializeWaits();
            InitializeCamera();
            InitializeEntity();
            InitializeCallback();
            InitializeLeader();
            RegisterAI();
        }

        protected virtual void Update()
        {
            HandleEntityOptimization();

            if (CanUpdateAI())
            {
                HandleViewSight();
                HandleAttack();
                HandleFollowing();
                HandleTargetFlee();
            }
        }

        protected virtual void OnDrawGizmos()
        {
            DrawFieldOfViewGizmoOnGround();
            DrawRadiusGizmoOnGround(spotRadius, spotRadiusColor, spotRadiusYOffset);
            DrawRadiusGizmoOnGround(fleeRadius, fleeRadiusColor, fleeRadiusYOffset);
        }

        private void DrawFieldOfViewGizmoOnGround()
        {
            Vector3 origin = transform.position + Vector3.up * viewAngleYOffset;
            Vector3 forward = transform.forward;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle / 2, Vector3.up);

            Vector3 leftDir = leftRayRotation * forward;
            Vector3 rightDir = rightRayRotation * forward;

            Vector3 leftRayEnd = origin + leftDir * viewDistance;
            Vector3 rightRayEnd = origin + rightDir * viewDistance;

            // Draw lines from AI to the ends of the field of view using the field of view color
            Gizmos.color = fieldOfViewColor;
            Gizmos.DrawLine(origin, leftRayEnd);
            Gizmos.DrawLine(origin, rightRayEnd);

            // Draw the arc between the ends using the field of view color
            DrawArcGizmoOnGround(origin, leftDir, rightDir, viewDistance, fieldOfViewColor);

            // Optional: Draw lines across the field of view using the field of view color
            for (int i = 0; i < outlineLines; i++)
            {
                Quaternion rotation = Quaternion.AngleAxis((-viewAngle / 2) + (viewAngle / (outlineLines - 1)) * i, Vector3.up);
                Gizmos.DrawLine(origin, origin + rotation * forward * viewDistance);
            }
        }

        private void DrawRadiusGizmoOnGround(float radius, Color color, float yOffset)
        {
            Gizmos.color = color;
            Vector3 center = transform.position + Vector3.up * yOffset;

            // Draw the radius as a circle on the ground
            DrawCircleGizmoOnGround(center, radius, color);
        }

        private void DrawArcGizmoOnGround(Vector3 origin, Vector3 startDir, Vector3 endDir, float radius, Color color)
        {
            Gizmos.color = color;
            float step = viewAngle / outlineLines;
            Vector3 prevPoint = origin + startDir * radius;
            Vector3 newPoint = Vector3.zero;

            for (int i = 1; i <= outlineLines; i++)
            {
                Quaternion rotation = Quaternion.AngleAxis(step * i - viewAngle / 2, Vector3.up);
                newPoint = origin + rotation * transform.forward * radius;
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        private void DrawCircleGizmoOnGround(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            float deltaTheta = (2f * Mathf.PI) / outlineLines;
            float theta = 0f;

            Vector3 oldPos = center + new Vector3(radius, 0f, 0f);
            for (int i = 0; i < outlineLines + 1; i++)
            {
                Vector3 newPos = center + new Vector3(Mathf.Cos(theta) * radius, 0f, Mathf.Sin(theta) * radius);
                Gizmos.DrawLine(oldPos, newPos);
                oldPos = newPos;
                theta += deltaTheta;
            }
        }
    }
}
