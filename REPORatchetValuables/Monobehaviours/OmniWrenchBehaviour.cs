using Photon.Pun;
using System.Linq;

namespace RCRepo.Monobehaviours;

internal class OmniWrenchBehaviour : ItemMelee
{
    List<HurtCollider> hurtColliders = [];

    new private void Start()
    {
        Transform parentObject = transform.Find("Object");
        rb = GetComponent<Rigidbody>();
        hurtColliders = [..GetComponentsInChildren<HurtCollider>()];
        hurtColliderRotation = null;
        physGrabObjectImpactDetector = GetComponent<PhysGrabObjectImpactDetector>();
        foreach (var hurtCollider in hurtColliders)
        {
            hurtCollider.gameObject.SetActive(false);
        }
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        swingPoint = parentObject.Find("Swing Point");
        physGrabObject = GetComponent<PhysGrabObject>();
        particleSystem = parentObject.Find("Particles").GetComponent<ParticleSystem>();
        particleSystemGroundHit = base.transform.Find("Particles Ground Hit").GetComponent<ParticleSystem>();
        photonView = GetComponent<PhotonView>();
        itemBattery = GetComponent<ItemBattery>();
        forceGrabPoint = parentObject.Find("Force Grab Point");
        meshHealthy = parentObject.Find("Mesh");
        itemEquippable = GetComponent<ItemEquippable>();
        if (SemiFunc.RunIsArena())
        {
            foreach(var component in hurtColliders)
            {
                component.playerDamage = component.enemyDamage;
            }
        }
    }

    new private void Update()
    {
        if (grabbedTimer > 0f)
        {
            grabbedTimer -= Time.deltaTime;
        }

        if (physGrabObject.grabbed)
        {
            grabbedTimer = 1f;
        }

        if (hitFreezeDelay > 0f)
        {
            hitFreezeDelay -= Time.deltaTime;
            if (hitFreezeDelay <= 0f)
            {
                physGrabObject.FreezeForces(hitFreeze, Vector3.zero, Vector3.zero);
            }
        }

        if (!LevelGenerator.Instance.Generated)
        {
            return;
        }

        if (spawnTimer > 0f)
        {
            prevPosition = swingPoint.position;
            swingTimer = 0f;
            spawnTimer -= Time.deltaTime;
            return;
        }

        if (hitCooldown > 0f)
        {
            hitCooldown -= Time.deltaTime;
        }

        if (enemyOrPVPDurabilityLossCooldown > 0f)
        {
            enemyOrPVPDurabilityLossCooldown -= Time.deltaTime;
        }

        if (groundHitCooldown > 0f)
        {
            groundHitCooldown -= Time.deltaTime;
        }

        if (groundHitSoundTimer > 0f)
        {
            groundHitSoundTimer -= Time.deltaTime;
        }

        DisableHurtBoxWhenEquipping();
        if (itemEquippable.equipTimer > 0f || itemEquippable.unequipTimer > 0f)
        {
            return;
        }

        var anyHurtColliderActive = hurtColliders.Any(hurtCollider => hurtCollider.gameObject.activeSelf);
        soundSwingLoop.PlayLoop(anyHurtColliderActive, 10f, 10f, 3f);
        if (SemiFunc.IsMultiplayer() && !SemiFunc.IsMasterClient() && isSwinging)
        {
            swingTimer = 0.5f;
        }

        if (SemiFunc.IsMasterClientOrSingleplayer() && itemBattery is not null)
        {
            if (itemBattery.batteryLife <= 0f)
            {
                MeleeBreak();
            }
            else
            {
                MeleeFix();
            }

            if (durabilityLossCooldown > 0f)
            {
                durabilityLossCooldown -= Time.deltaTime;
            }

            if (!isBroken)
            {
                if (physGrabObject.grabbedLocal)
                {
                    if (itemBattery.batteryActive)
                    {
                    }
                }
                else
                {
                    _ = itemBattery.batteryActive;
                }
            }
        }

        if (isBroken)
        {
            return;
        }

        if (hitSoundDelayTimer > 0f)
        {
            hitSoundDelayTimer -= Time.deltaTime;
        }

        if (swingPitch != swingPitchTarget && swingPitchTargetProgress >= 1f)
        {
            swingPitch = swingPitchTarget;
        }

        Vector3 vector2 = prevPosition - swingPoint.position;
        Vector3 normalized = swingStartDirection.normalized;
        Vector3 normalized2 = vector2.normalized;
        float num = Vector3.Dot(normalized, normalized2);
        double num2 = 0.85;
        if (!physGrabObject.grabbed)
        {
            num2 = 0.1;
        }

        if ((double)num > num2)
        {
            swingTimer = 0f;
        }

        if (isSwinging)
        {
            ActivateHitbox();
        }

        if (hitTimer > 0f)
        {
            hitTimer -= Time.deltaTime;
        }

        if (swingTimer <= 0f)
        {
            if (hitBoxTimer <= 0f)
            {
                foreach(var hurtCollider in hurtColliders)
                {
                    hurtCollider.gameObject.SetActive(false);
                }
            }
            else
            {
                hitBoxTimer -= Time.deltaTime;
            }

            trailRenderer.emitting = false;
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                isSwinging = false;
            }
        }
        else
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                isSwinging = true;
            }

            if (hitTimer <= 0f)
            {
                hitBoxTimer = 0.2f;
            }

            swingTimer -= Time.deltaTime;
        }
    }

    new private void FixedUpdate()
    {
        DisableHurtBoxWhenEquipping();
        bool isRotating = false;
        foreach (PhysGrabber item in physGrabObject.playerGrabbing)
        {
            if (item.isRotating)
            {
                isRotating = true;
            }
        }

        if (!isRotating)
        {
            Quaternion turnY = currentYRotation;
            Quaternion turnX = Quaternion.Euler(45f, 0f, 0f);
            physGrabObject.TurnXYZ(turnX, turnY, Quaternion.identity);
        }

        if (itemEquippable.equipTimer > 0f || itemEquippable.unequipTimer > 0f || isBroken)
        {
            return;
        }

        if (prevPosUpdateTimer > 0.1f)
        {
            prevPosition = swingPoint.position;
            prevPosUpdateTimer = 0f;
        }
        else
        {
            prevPosUpdateTimer += Time.fixedDeltaTime;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            return;
        }

        if (!isRotating)
        {
            if (torqueStrength != 1f)
            {
                physGrabObject.OverrideTorqueStrength(torqueStrength);
            }

            if (physGrabObject.grabbed)
            {
                if (itemBattery is null || (itemBattery is not null && itemBattery.batteryLife <= 0f))
                {
                    physGrabObject.OverrideTorqueStrength(0.1f);
                }

                physGrabObject.OverrideMaterial(SemiFunc.PhysicMaterialSlippery());
            }
        }

        if (isRotating)
        {
            physGrabObject.OverrideTorqueStrength(4f);
        }

        if (distanceCheckTimer > 0.1f)
        {
            prevPosDistance = Vector3.Distance(prevPosition, swingPoint.position) * 10f * rb.mass;
            distanceCheckTimer = 0f;
        }

        distanceCheckTimer += Time.fixedDeltaTime;
        TurnWeapon();
        Vector3 vector = prevPosition - swingPoint.position;
        float num = 1f;
        if (!physGrabObject.grabbed)
        {
            num = 0.5f;
        }

        if (vector.magnitude > num * swingDetectSpeedMultiplier && swingPoint.position - prevPosition != Vector3.zero)
        {
            swingTimer = 0.2f;
            if (!isSwinging)
            {
                newSwing = true;
            }

            swingDirection = Quaternion.LookRotation(swingPoint.position - prevPosition);
        }
    }

    new public void DisableHurtBoxWhenEquipping()
    {
        if(itemEquippable is null)
        {
            Plugin.logger.LogError("OmniWrench has no ItemEquippable component!");
            itemEquippable = GetComponent<ItemEquippable>();
            if(itemEquippable is null)
            {
                Plugin.logger.LogError($"Failed to find ItemEquippable component on OmniWrench on GameObject {gameObject.name}!");
                return;
            }
        }
        if (itemEquippable.equipTimer > 0f || itemEquippable.unequipTimer > 0f)
        {
            foreach(var hurtCollider in hurtColliders)
            {
                hurtCollider.gameObject.SetActive(false);
            }
            swingTimer = 0f;
            trailRenderer.emitting = false;
        }
    }

    new public void GroundHit()
    {
        base.GroundHit();
    }
}
