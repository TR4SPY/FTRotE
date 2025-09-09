using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections.Generic;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Entity/Entity")]
    public class Entity : MonoBehaviour
    {
        public UnityEvent onAttack;
        public UnityEvent onMagicAttack;
        public UnityEvent<EntityAttackType> onPerformAttack;
        public UnityEvent onTargetSet;
        public UnityEvent<int, Vector3, bool> onDamage;
        public UnityEvent<int, Vector3, bool, Entity> onDamageEx;
        public UnityEvent onBlock;
        public UnityEvent onStunned;
        public UnityEvent onDie;
        public UnityEvent onRevive;
        public bool isAgent = false;
        public bool isPlayer = false;
        public Nametag nametag;

        [Tooltip("The tags of Game Objects that this Entity identifies as potential targets.")]
        public List<string> targetTags;

        [Tooltip("The maximum speed this Entity will use to move around.")]
        public float moveSpeed = 6f;

        [Tooltip("The rotation in degrees per second the Entity will rotate to face directions.")]
        public float rotationSpeed = 720f;

        [Tooltip("The default maximum distance to perform attacks. Internally, this can be overridden by weapons.")]
        public float attackDistance = 1f;

        [Tooltip("The force applied toward the ground to keep the Entity grounded.")]
        public float groundSnapForce = 10f;

        [Tooltip("The reference to the Hitbox used to perform melee attacks.")]
        public EntityHitbox hitbox;

        [Header("Auto Collection")]
        [Tooltip("If true, this Entity will automatically try to collect an item when touching it.")]
        public bool autoCollectItems;

        [Tooltip("The time in seconds between each auto collection try.")]
        public float autoCollectCoolDown = 0.1f;

        protected Collider[] m_colliders;
        protected CharacterController m_controller;
        protected Rigidbody m_rigidbody;
        protected EntityAnimator m_animator;
        protected EntityStatsManager m_stats;

        protected float m_lastHitTime;
        protected float m_lastAutoCollectTime;
        protected float m_lastAttackTime;
        protected float m_lastComboTime;

        protected int m_waypointsSize;
        protected int m_currentWaypoint = -1;
        protected NavMeshPath m_path;
        protected Vector3[] m_waypoints = new Vector3[k_waypointsSize];

        protected List<Entity> m_damageFrom = new List<Entity>();

        public EntityStateMachine states { get; protected set; }
        public EntitySkillManager skills { get; protected set; }
        public EntityInputs inputs { get; protected set; }
        public EntityItemManager items { get; protected set; }
        public EntityInventory inventory { get; protected set; }

        /// <summary>
        /// List of entities summoned by this one.
        /// </summary>
        public List<Entity> summonedEntities { get; set; } = new();

        /// <summary>
        /// List of entities that are attacking this one.
        /// </summary>
        public List<Entity> attackedBy { get; set; } = new();

        /// <summary>
        /// The current combo index of the Entity.
        /// </summary>
        public int comboIndex { get; protected set; }

        public Vector3 position => transform.position;

        public CharacterController controller
        {
            get
            {
                InitializeController();
                return m_controller;
            }
        }

        public EntityStatsManager stats
        {
            get
            {
                InitializeStats();
                return m_stats;
            }
        }

        /// <summary>
        /// The current velocity of the Entity.
        /// </summary>
        /// <value></value>
        public Vector3 velocity { get; set; }

        /// <summary>
        /// The current X and Z velocity of the Entity.
        /// </summary>
        public Vector3 lateralVelocity
        {
            get { return new Vector3(velocity.x, 0, velocity.z); }
            set { velocity = new Vector3(value.x, velocity.y, value.z); }
        }

        /// <summary>
        /// The current Y velocity of the Entity.
        /// </summary>
        public Vector3 verticalVelocity
        {
            get { return new Vector3(0, velocity.y, 0); }
            set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
        }

        public Vector3 lookDirection { get; set; }
        public Vector3 initialPosition { get; protected set; }

        public Transform target { get; protected set; }
        public Entity targetEntity { get; protected set; }

        public float attackDuration { get; protected set; }
        public float skillDuration { get; protected set; }
        public float blockDuration { get; protected set; }
        public float stunDuration { get; protected set; }

        public Interactive targetInteractive { get; set; }

        protected const float k_initialRadius = 0.25f;
        protected const float k_minDistanceToMove = 0.25f;
        protected const float k_maxHitRate = 0.15f;
        protected const int k_waypointsSize = 256;

        /// <summary>
        /// Returns true if the Entity is active (responding to movement).
        /// </summary>
        public bool isActive => controller.enabled;

        /// <summary>
        /// Returns true if the Entity have any health.
        /// </summary>
        public virtual bool isDead => stats != null && stats.health <= 0;

        /// <summary>
        /// Returns true if the Entity is performing an attack.
        /// </summary>
        public bool isAttacking => states.IsCurrent<AttackEntityState>() || states.IsCurrent<UseSkillEntityState>();

        /// <summary>
        /// Returns true if the Entity is in the blocking state.
        /// </summary>
        public bool isBlocking => states.IsCurrent<BlockEntityState>();

        /// <summary>
        /// Returns true if the Entity is in the stunned state.
        /// </summary>
        public bool isStunned => states.IsCurrent<StunnedEntityState>();

        /// <summary>
        /// If true, makes the Entity use the current Skill in the next attack.
        /// </summary>
        public bool useSkill { get; set; }

        /// <summary>
        /// If true, allows the Entity to move to the next destination, which
        /// can be the current assigned target or a waypoint.
        /// </summary>
        public bool canUpdateDestination { get; set; } = true;

        /// <summary>
        /// If true, the Entity is currently performing a combo attack.
        /// </summary>
        public bool isPerformingCombo { get; set; }

        /// <summary>
        /// If true, the Entity is controlled by the Player's inputs.
        /// </summary>
        public bool usingInputs => inputs;

        /// <summary>
        /// Returns true if the Entity can move.
        /// </summary>
        public virtual bool canMove => !isBlocking && !isStunned;

        protected virtual void InitializeController()
        {
            if (m_controller) return;

            if (!TryGetComponent(out m_controller))
            {
                m_controller = gameObject.AddComponent<CharacterController>();
                m_controller.radius = k_initialRadius;
            }

            m_controller.minMoveDistance = 0;
            m_controller.skinWidth = m_controller.radius * 0.1f;
            initialPosition = transform.position;
        }

        protected virtual void InitializeRigidbody()
        {
            if (!TryGetComponent(out m_rigidbody))
            {
                m_rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            m_rigidbody.isKinematic = true;
        }

        protected virtual void InitializeStateMachine()
        {
            states = new EntityStateMachine(this);
            states.ChangeTo<IdleEntityState>();
        }

        protected virtual void InitializeStats()
        {
            if (m_stats) return;

            m_stats = GetComponent<EntityStatsManager>();
        }

        protected virtual void InitializeSkillManager() => skills = GetComponent<EntitySkillManager>();
        protected virtual void InitializeItemManager() => items = GetComponent<EntityItemManager>();
        protected virtual void InitializeInventory() => inventory = GetComponent<EntityInventory>();
        protected virtual void InitializePath() => m_path = new NavMeshPath();
        protected virtual void InitializeColliders() => m_colliders = GetComponents<Collider>();
        protected virtual void InitializeInputs() => inputs = GetComponent<EntityInputs>();
        protected virtual void InitializeAnimator() => m_animator = GetComponent<EntityAnimator>();

        /// <summary>
        /// Moves the Entity to a specific position and rotation in the world space.
        /// </summary>
        /// <param name="position">The position to teleport in world space.</param>
        /// <param name="rotation">The rotation to teleport in world space.</param>
        public virtual void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false;
            m_currentWaypoint = -1;
            velocity = Vector3.zero;
            m_waypoints = new Vector3[k_waypointsSize];
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
        }

        /// <summary>
        /// Returns the current distance to the assigned target.
        /// </summary>
        public virtual float GetDistanceToDestination()
        {
            var waypointIdx = m_waypointsSize - 1;

            if (m_waypoints == null || waypointIdx <= m_waypoints.Length) return 0;

            var waypoint = m_waypoints[m_waypointsSize - 1];
            var destination = new Vector3(waypoint.x, position.y, waypoint.z);

            return Vector3.Distance(position, destination);
        }

        /// <summary>
        /// Returns the distance to a point in world space without considering its Y position.
        /// </summary>
        /// <param name="point">The point you want to get the distance to.</param>
        public virtual float GetDistanceTo(Vector3 point)
        {
            point.y = position.y;
            return Vector3.Distance(position, point);
        }

        /// <summary>
        /// Returns the distance to the current assigned target.
        /// Returns zero if the current target is not set (null).
        /// </summary>
        public virtual float GetDistanceToTarget()
        {
            if (!target) return 0;

            return GetDistanceTo(target.position);
        }

        /// <summary>
        /// Returns a vector towards a point in world space without considering its Y position.
        /// </summary>
        /// <param name="point">The point you want the direction.</param>
        public virtual Vector3 GetDirectionTo(Vector3 point)
        {
            point.y = transform.position.y;
            return (point - transform.position).normalized;
        }

        /// <summary>
        /// Returns a vector towards the current target without considering its Y position.
        /// </summary>
        public virtual Vector3 GetDirectionToTarget()
        {
            if (!target) return transform.forward;

            return GetDirectionTo(target.position);
        }

        /// <summary>
        /// Returns the current allowed attack distance, based on the current skill and
        /// the current equipped weapon, if any. Otherwise, it returns the default attack distance.
        /// </summary>
        public virtual float GetAttackDistance()
        {
            if (useSkill && skills.IsAttack()) return skills.GetSkillAttack().minAttackDistance;
            if (items.IsUsingBow()) return items.GetBow().shotDistance;

            return attackDistance;
        }

        /// <summary>
        /// Returns true if the Entity is close enough to attack the assigned target based on
        /// the current attack distance (evaluated by the current skill and equipped weapon).
        /// </summary>
        public virtual bool IsCloseToAttackTarget() => GetDistanceToTarget() <= GetAttackDistance();

        /// <summary>
        /// Returns true if the Entity can perform the next combo attack based on the current time.
        /// </summary>
        public virtual bool CanPerformNextCombo() => isPerformingCombo &&
            m_lastComboTime > m_lastAttackTime && Time.time > m_lastComboTime + stats.nextComboDelay;

        /// <summary>
        /// Moves the Entity along the current NavMesh path while updating the waypoints.
        /// </summary>
        public virtual void HandleWaypointMovement()
        {
            if (m_currentWaypoint < 0) return;

            var waypoint = m_waypoints[m_currentWaypoint + 1];
            var point = new Vector3(waypoint.x, position.y, waypoint.z);
            var direction = point - position;
            var distance = direction.magnitude;

            direction = direction.normalized;
            lateralVelocity = direction * moveSpeed;

            if (distance <= 0.1f)
            {
                m_currentWaypoint++;

                if (m_currentWaypoint >= m_waypointsSize - 1)
                {
                    m_currentWaypoint = -1;
                    lateralVelocity = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Calculates a NavMesh Path to a given destination to be used by the waypoint movement.
        /// </summary>
        /// <param name="destination">The destination you want to reach.</param>
        /// <returns>Returns true if a complete or partial path was found.</returns>
        public virtual bool TryCalculatePath(Vector3 destination)
        {
            if (NavMesh.CalculatePath(position, destination, NavMesh.AllAreas, m_path))
            {
                m_currentWaypoint = 0;
                m_waypointsSize = m_path.GetCornersNonAlloc(m_waypoints);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves the Entity to the current assigned target.
        /// </summary>
        public virtual void MoveToTarget() => MoveTo(target.position);

        /// <summary>
        /// Moves the Entity to a given point in world space.
        /// </summary>
        /// <param name="point">The point you want the Entity to move to.</param>
        public virtual void MoveTo(Vector3 point)
        {
            // Debug.Log($"[{name}] MoveTo called. Target: {point}. IsAgent: {isAgent}");
            if (canUpdateDestination &&
                GetDistanceTo(point) > k_minDistanceToMove &&
                TryCalculatePath(point))
            {
                states.ChangeTo<MoveToDestinationEntityState>();
            }
        }

        /// <summary>
        /// Moves the Entity to a given target and performs an attack when reaching it.
        /// </summary>
        /// <param name="target">The target you want to attack.</param>
        public virtual void MoveToAttack(Transform target)
        {
            if ((isAttacking && !isPerformingCombo) || !canUpdateDestination)
                return;

            SetTarget(target);

            if (!IsCloseToAttackTarget())
            {
                states.ChangeTo<MoveToAttackEntityState>();
                return;
            }

            Attack();
        }

        /// <summary>
        /// Updates the current target of this Entity.
        /// </summary>
        /// <param name="target">The target you want to assign.</param>
        public virtual void SetTarget(Transform target)
        {
            if (this.target == target) return;

            if (target && GameTags.IsEntity(target.gameObject))
                targetEntity = target.GetComponent<Entity>();
            else
                targetEntity = null;

            this.target = target;
            onTargetSet.Invoke();
        }

        /// <summary>
        /// Smoothly rotates the Entity to the current movement direction.
        /// </summary>
        public virtual void FaceMoveDirection() => FaceTo(lateralVelocity);

        /// <summary>
        /// Smoothly rotates the Entity to the look direction (interest point).
        /// </summary>
        public virtual void FaceLookDirection() => FaceTo(lookDirection);

        /// <summary>
        /// Smoothly rotates the Entity to face a given direction.
        /// </summary>
        /// <param name="direction">The direction you want the Entity to face.</param>
        public virtual void FaceTo(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0)
            {
                var delta = rotationSpeed * Time.deltaTime;
                var target = Quaternion.LookRotation(direction, Vector3.up);
                var rotation = Quaternion.RotateTowards(transform.rotation, target, delta);
                transform.rotation = rotation;
            }
        }

        /// <summary>
        /// Moves the Entity towards the ground to keep it grounded.
        /// </summary>
        public virtual void SnapToGround() => verticalVelocity = Vector3.down * groundSnapForce;

        /// <summary>
        /// Forces the Entity to enter the Idle state to make it stop any movements or attacks.
        /// </summary>
        public virtual void StandStill() => states.ChangeTo<IdleEntityState>();

        /// <summary>
        /// Makes the Entity attack without a target.
        /// </summary>
        public virtual void FreeAttack()
        {
            SetTarget(null);
            Attack();
        }

        /// <summary>
        /// Makes the Entity attack.
        /// </summary>
        public virtual void Attack()
        {
            if (Time.time > m_lastComboTime + stats.timeToStopCombo)
                CancelCombo();

            bool performed = false;

            if (useSkill && !isAttacking && skills.CanUseSkill())
            {
                skillDuration = m_animator.GetAttackAnimationLength();
                states.ChangeTo<UseSkillEntityState>();
                performed = true;
            }

            if (!performed && (!isAttacking || CanPerformNextCombo()))
            {
                m_lastAttackTime = Time.time;
                attackDuration = m_animator.GetAttackAnimationLength();
                states.ChangeTo<AttackEntityState>(true);
            }
        }

        /// <summary>
        /// Performs an attack, while calculating the damage and critical, considering
        /// the current skill, weapon, and stats. It also shoots a projectile if
        /// the Entity is equipping a bow.
        /// </summary>
        public virtual void PerformAttack()
        {
            var attackType = EntityAttackType.Weapon;

            if (useSkill)
            {
                skills.PerformSkill();
                attackType = EntityAttackType.Skill;
            }
            else if (items && items.IsUsingBow())
            {
                // Ensure the projectile is shot towards the full 3D position of the target
                Vector3 targetPosition = target ? target.position : transform.position + transform.forward * items.GetBow().shotDistance;
                Vector3 direction = (targetPosition - transform.position).normalized;
                items.ShootProjectile(direction);
            }
            else
            {
                var damage = stats.GetDamage(out var critical);

                if (!items || !items.IsUsingBlade())
                    attackType = EntityAttackType.Melee;

                hitbox.SetDamage(damage, critical);
                hitbox.Toggle();
            }

            onPerformAttack.Invoke(attackType);
        }

        public virtual void TakeMagicDamage(int damage, Entity origin = null)
        {
            int resistance = stats.magicResistance;
            int finalDamage = Mathf.Max(damage - resistance, 1);
            stats.health -= finalDamage;

            onDamage?.Invoke(finalDamage, position, false); 
            onDamageEx?.Invoke(finalDamage, position, false, origin);

            if (stats.health <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Increments the current combo index of the Entity.
        /// </summary>
        public virtual void IncrementCombo()
        {
            if (Time.time == m_lastComboTime)
                return;

            comboIndex++;
            isPerformingCombo = true;
            m_lastComboTime = Time.time;

            if (comboIndex >= stats.maxCombos)
                CancelCombo();
        }

        /// <summary>
        /// Cancels the current combo of the Entity.
        /// </summary>
        public virtual void CancelCombo()
        {
            comboIndex = 0;
            isPerformingCombo = false;
        }

        /// <summary>
        /// Makes the Entity start moving along random waypoints.
        /// </summary>
        public virtual void StartRandomMovement() => states.ChangeTo<RandomMovementEntityState>();

        /// <summary>
        /// Evaluates if the Player is trying to move the Entity with directional
        /// movement and change the state to the Directional Movement Entity State.
        /// </summary>
        public virtual void EvaluateDirectionalMovement()
        {
            if (!usingInputs || !canMove)
                return;

            var moveDirection = inputs.GetMoveDirection();

            if (moveDirection.sqrMagnitude > 0)
            {
                target = null;
                states.ChangeTo<DirectionalMovementEntityState>();
            }
        }

        private void Start()
        {
            if (isAgent)
            {
                // Debug.Log("Skipping stats initialization for AI Agent.");
                return;
            }

            if (stats == null)
            {
                Debug.LogError($"{name} does not have stats initialized!");
                return;
            }
        }

        /// <summary>
        /// Kills the Entity, stopping its moving and disabling its colliders.
        /// This method also notifies other entities which attacked this one that
        /// this Entity was defeated.
        /// </summary>
        public virtual void Die()
        {
            // Debug.Log($"[Entity] {name} → Die() start. Health= {stats.health}. Marking as dead…");

            stats.health = 0;
            NotifyDefeat();
            SetCollidersEnabled(false);

            states.ChangeTo<DeadEntityState>();

            // Debug.Log($"[Entity] {name} → Die() done. state= {states.current.GetType().Name}");

            onDie?.Invoke();

        if (CompareTag("Entity/Player"))
        {
            var playerLogger = GetComponent<PlayerBehaviorLogger>();
            if (playerLogger != null)
            {
                var entity = GetComponent<Entity>();
                if (entity != null)
                {
                    playerLogger.LogPlayerDeath(entity);
                }
                // Debug.Log("Player death logged for Entity/Player.");
            }
            else
            {
                Debug.LogError("PlayerBehaviorLogger not found on player entity!");
            }
        }
        else if (CompareTag("Entity/AI_Agent"))
        {
            // Debug.Log("Agent death detected, logging handled by AgentController.");
        }
        else if (CompareTag("Entity/Enemy"))
        {
            // Debug.Log($"Enemy {name} defeated.");
        }
    }

        /// <summary>
        /// Brings the Entity back to life, restoring its movements and colliders.
        /// </summary>
        public virtual void Revive()
        {
            SetCollidersEnabled(true);
            stats.Revitalize();
            states.ChangeTo<IdleEntityState>();
            onRevive.Invoke();

            MinimapIcon minimapIcon = GetComponent<MinimapIcon>();
            if (minimapIcon != null)
            {
                minimapIcon.SetVisibility(true);
                minimapIcon.AddIconToMinimap();
            }
        }

        /// <summary>
        /// Applies damage to this Entity. This method considers the defense calculated
        /// from the current Entity Stats.
        /// </summary>
        /// <param name="other">The Entity that performed the attack.</param>
        /// <param name="amount">The amount of damage.</param>
        /// <param name="critical">Notification if the damage is critical.</param>
        public virtual void Damage(Entity other, int amount, bool critical)
        {
            if (isDead || Time.time <= m_lastHitTime + k_maxHitRate)
                return;

            if (stats && Random.value <= stats.chanceToBlock && !isAttacking)
            {
                Block(other);
                return;
            }

            var total = Mathf.Max(1, amount - (int)(stats.defense / 2f));
            stats.health -= total;
            m_lastHitTime = Time.time;

            if (!m_damageFrom.Contains(other))
                m_damageFrom.Add(other);

            onDamage?.Invoke(total, other.position, critical);
            onDamageEx?.Invoke(total, other.position, critical, other); 

            if (stats.health == 0)
                Die();
            else if (other.stats && Random.value <= other.stats.stunChance)
                Stun();
        }

        protected virtual void Block(Entity other)
        {
            blockDuration = m_animator.GetBlockAnimationLength();
            lookDirection = GetDirectionTo(other.position);
            states.ChangeTo<BlockEntityState>();
            onBlock.Invoke();
        }

        protected virtual void Stun()
        {
            if (stats && stats.immuneToStun)
                return;

            stunDuration = m_animator.GetStunAnimationLength();
            states.ChangeTo<StunnedEntityState>();
            onStunned.Invoke();
        }

        protected virtual void NotifyDefeat()
        {
            foreach (var entity in m_damageFrom)
            {
                if (!entity)
                    continue;

                if (entity.CompareTag(GameTags.Summoned) &&
                    entity.TryGetComponent(out EntityAI aI))
                {
                    aI.leader.stats.OnDefeatEntity(this);
                    continue;
                }

                entity.stats.OnDefeatEntity(this);
            }
        }

        /// <summary>
        /// Disables or enables all the colliders used by the Entity.
        /// </summary>
        public virtual void SetCollidersEnabled(bool value)
        {
            foreach (var collider in m_colliders)
            {
                collider.enabled = value;
            }
        }

        /// <summary>
        /// Adds an Entity to the list of entities that are attacking this one.
        /// </summary>
        /// <param name="entity">The Entity you want to add.</param>
        public virtual void AddAttackedBy(Entity entity)
        {
            if (entity && !attackedBy.Contains(entity))
                attackedBy.Add(entity);
        }

        /// <summary>
        /// Removes an Entity from the list of entities that are attacking this one.
        /// </summary>
        /// <param name="entity">The Entity you want to remove.</param>
        public virtual void RemoveAttackedBy(Entity entity)
        {
            if (entity && attackedBy.Contains(entity))
                attackedBy.Remove(entity);
        }

        protected virtual void HandleInteractive(Collider other)
        {
            if (targetInteractive && (other.CompareTag(GameTags.Interactive) ||
                other.CompareTag(GameTags.Collectible)))
            {
                if (other.TryGetComponent(out Interactive interactive) &&
                    interactive == targetInteractive)
                {
                    targetInteractive.Interact(this);
                    targetInteractive = null;
                    StandStill();
                }
            }
        }

        protected virtual void HandleAutoCollect(Collider other)
        {
            bool autoLoot = autoCollectItems;

            if (GameSettings.instance != null && GameSettings.instance.GetAutoLoot())
                autoLoot = true;

            if (!autoLoot || !other.CompareTag(GameTags.Collectible) ||
                Time.time < m_lastAutoCollectTime + autoCollectCoolDown)
                return;

            other.GetComponent<Interactive>()?.Interact(this);
            m_lastAutoCollectTime = Time.time;
        }

        protected virtual void Awake()
        {
            InitializeController();
            InitializeRigidbody();
            InitializeStateMachine();
            InitializeStats();
            InitializeSkillManager();
            InitializeInputs();
            InitializeItemManager();
            InitializeInventory();
            InitializePath();
            InitializeColliders();
            InitializeAnimator();
        }

        protected virtual void Update()
        {
            // Debug.Log($"[Entity.Update] {name}, isDead={isDead}, currentState={states.current?.GetType()?.Name}");
            
            if (controller.enabled)
            {
                states.HandleStep();

                // Ruch
                controller.Move(velocity * Time.deltaTime);

                // Obrót w kierunku ruchu dla agenta
                if (isAgent && velocity.sqrMagnitude > 0)
                {
                    FaceMoveDirection();
                }
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            HandleInteractive(other);
            HandleAutoCollect(other);
        }
    }
}
