using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SwordMod
{
    public class Sword : Weapon
    {
        public static AbstractPhysicalObject.AbstractObjectType SwordType = new AbstractPhysicalObject.AbstractObjectType("Sword", true);

        //variables
        public int swingTimer = 0;
        public Vector2 swingDir = new Vector2(0, 0);
        public float swingDamage = 0f;
        public float swingExhaustion = 0f;

        public bool canHitWall = true;
        public bool canHitCreatureSound = true;

        //options
        public static int TotalSwingTime => (SwordMod.Options.SwingTime != null) ? SwordMod.Options.SwingTime.Value : 15;
        public int ParryWindow => Mathf.RoundToInt((1f + swingExhaustion) * ((SwordMod.Options.ParryWindow != null) ? SwordMod.Options.ParryWindow.Value : 3));
        public int CreatureDamageTime => TotalSwingTime - ParryWindow; //factors in player exhaustion ^^^
        
        public static float SwingDamageModifier => (SwordMod.Options.SwingDamageModifier != null) ? SwordMod.Options.SwingDamageModifier.Value : 1f;
        public static float KnockbackModifier => (SwordMod.Options.KnockbackModifier != null) ? SwordMod.Options.KnockbackModifier.Value : 1f;
        public static float VerticalKnockbackModifier => (SwordMod.Options.VerticalKnockbackModifier != null) ? SwordMod.Options.VerticalKnockbackModifier.Value : 1f;
        public static float StunModifier => (SwordMod.Options.StunModifier != null) ? SwordMod.Options.StunModifier.Value : 1f;
        public static float LungeModifier => (SwordMod.Options.LungeModifier != null) ? SwordMod.Options.LungeModifier.Value : 1f;
        public static bool AllowParries => (SwordMod.Options.AllowParries != null) ? SwordMod.Options.AllowParries.Value : true;
        
        //extras
        public static bool OmnidirectionalSwings => (SwordMod.Options.OmnidirectionalSwings != null) ? SwordMod.Options.OmnidirectionalSwings.Value : false;
        public static float HorizontalPushbackModifier => (SwordMod.Options.HorizontalPushbackModifier != null) ? SwordMod.Options.HorizontalPushbackModifier.Value : 0f;
        public static float VerticalPushbackModifier => (SwordMod.Options.VerticalPushbackModifier != null) ? SwordMod.Options.VerticalPushbackModifier.Value : 0f;
        public static float DownswingLengthModifier => (SwordMod.Options.DownswingLengthModifier != null) ? SwordMod.Options.DownswingLengthModifier.Value : 1f;
        public static bool UseStaminaMechanics => (SwordMod.Options.UseStaminaMechanics != null) ? SwordMod.Options.UseStaminaMechanics.Value : false;
        public static float ExhaustionRateModifier => (SwordMod.Options.ExhaustionRateModifier != null) ? SwordMod.Options.ExhaustionRateModifier.Value : 1.0f;
        public static float ExhaustionDamageModifier => (SwordMod.Options.ExhaustionDamageModifier != null) ? SwordMod.Options.ExhaustionDamageModifier.Value : 0.2f;

        public override bool HeavyWeapon
        {
            get
            {
                return true;
            }
        }

        //ctor
        public Sword(AbstractPhysicalObject obj, World world) : base(obj, world)
        {
            base.bodyChunks = new BodyChunk[1];
            //base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f); //copied from spear
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 15f, 0.3f);
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
            base.airFriction = 0.999f;
            base.gravity = 0.9f;
            this.bounce = 0.4f;
            this.surfaceFriction = 0.4f;
            this.collisionLayer = 2;
            base.waterFriction = 0.98f;
            base.buoyancy = 0.4f;
            //this.pivotAtTip = false;
            //this.lastPivotAtTip = false;
            //this.stuckBodyPart = -1;
            base.firstChunk.loudness = 7f;
            this.tailPos = base.firstChunk.pos;
            this.soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);

            base.doNotTumbleAtLowSpeed = true;
            base.canBeHitByWeapons = true;

            this.color = Color.white;
            this.rotation = Custom.DegToVec(180f);

            this.ChangeMode(Weapon.Mode.Free);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            //sLeaser.sprites[0] = new FSprite("BioSpear2", true);
            sLeaser.sprites[0] = new FSprite("SwordMod_Sword", true);

            rCam.room.world.game.rainWorld.HandleLog("[Sword Mod]: Sword sprite details: w:" + sLeaser.sprites[0].width + ", h:" + sLeaser.sprites[0].height, "no stack", UnityEngine.LogType.Log);

            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
            
            Vector3 vector2 = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
            sLeaser.sprites[0].x = vector.x - camPos.x;
            sLeaser.sprites[0].y = vector.y - camPos.y;
            //sLeaser.sprites[0].anchorY = Mathf.Lerp(this.lastPivotAtTip ? 0.85f : 0.5f, this.pivotAtTip ? 0.85f : 0.5f, timeStacker);
            sLeaser.sprites[0].anchorY = (this.mode == Weapon.Mode.Free) ? 0.5f : 0.05f; //hold at hilt
            sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector2);
            sLeaser.sprites[0].color = this.color;
            
            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void Update(bool eu)
        {
            this.rotationSpeed = 0;

            if (swingTimer > 0 && this.thrownBy != null)
            {

                this.mode = Weapon.Mode.Thrown;

                //parries
                if (swingTimer > CreatureDamageTime && swingDamage > 0)
                    this.DetectAndParryThrownObjects();

                //set pos and rotation
                //this.lastRotation = this.rotation;
                if (Mathf.Abs(swingDir.x) < 1f)
                    this.setRotation = Custom.DegToVec(90f - swingDir.y * (90f + swingDir.x * -90f * (0.7f - (float)swingTimer / (float)TotalSwingTime)) + Mathf.Sin(Mathf.PI * 3f * (float)swingTimer / (float)TotalSwingTime) * (1f + 9f * swingExhaustion));
                else
                    this.setRotation = Custom.DegToVec(swingDir.x * (90f + swingDir.y * -90f * (0.7f - (float)swingTimer / (float)TotalSwingTime)) + Mathf.Sin(Mathf.PI * 3f * (float)swingTimer / (float)TotalSwingTime) * (1f + 9f * swingExhaustion));
                //this.rotationSpeed = swingDir.x * swingDir.y * 90f / TotalSwingTime;

                //this.firstChunk.lastLastPos = this.firstChunk.lastPos;
                this.firstChunk.lastPos = this.firstChunk.pos;

                //this.firstChunk.lastPos = this.thrownBy.firstChunk.pos + (Mathf.Sin(Mathf.PI * (float)swingTimer / (float)TotalSwingTime) * 6f + 4f) * this.rotation; //the visual location of the sword

                this.firstChunk.pos = this.thrownBy.firstChunk.pos + (Mathf.Sin(Mathf.PI * (float)swingTimer / (float)TotalSwingTime) * 20f - 30f) * this.rotation; //the location of the sword for hit-checking
                //this.firstChunk.vel = this.thrownBy.firstChunk.vel + this.setRotation.Value * (Mathf.Sin(Mathf.PI * (float)swingTimer / (float)TotalSwingTime) * 15f + 40f);
                this.firstChunk.vel = this.thrownBy.firstChunk.vel + this.setRotation.Value * 75f * ((Mathf.Abs(swingDir.x) < 1f && swingDir.y < 0f) ? DownswingLengthModifier : 1f) * (1f - 0.4f * swingExhaustion);

                swingTimer--;

                if (swingTimer == TotalSwingTime)
                    this.firstChunk.lastPos = this.firstChunk.pos;

                if (swingDamage <= 0f)
                    swingTimer = 0;

                if (swingTimer <= 0)
                {
                    this.firstChunk.vel *= 0f;
                    this.ChangeMode(Weapon.Mode.Free);
                    //this.setRotation = Custom.DegToVec(180f);

                    //return to thrower's grasp if possible
                    bool grabbed = false;
                    for (int i = 0; i < this.thrownBy.grasps.Length; i++)
                    {
                        if (this.thrownBy.grasps[i] == null || this.thrownBy.grasps[i].grabbed == null)
                        {
                            grabbed = this.thrownBy.Grab(this, i, 0, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, true, false);
                            if (grabbed)
                                break;
                        }
                    }

                }
            }

            Creature oldThrownBy = this.thrownBy;

            base.Update(eu);

            this.thrownBy = oldThrownBy;

            if (swingTimer > 0 && this.thrownBy != null)
            {
                this.rotationSpeed = 0;

                this.mode = Weapon.Mode.Thrown;

                //set pos and rotation
                this.firstChunk.lastPos = this.firstChunk.lastLastPos; //replace the hitbox location of the sword with its visual location
                if (Mathf.Abs(swingDir.x) < 1f)
                    this.rotation = Custom.DegToVec(90f - swingDir.y * (90f + swingDir.x * -90f * (0.7f - (float)swingTimer / (float)TotalSwingTime)) + Mathf.Sin(Mathf.PI * 3f * (float)swingTimer / (float)TotalSwingTime) * (1f + 9f * swingExhaustion));
                else
                    this.rotation = Custom.DegToVec(swingDir.x * (90f + swingDir.y * -90f * (0.7f - (float)swingTimer / (float)TotalSwingTime)) + Mathf.Sin(Mathf.PI * 3f * (float)swingTimer / (float)TotalSwingTime) * (1f + 9f * swingExhaustion));

                this.firstChunk.pos = this.thrownBy.firstChunk.pos + (Mathf.Sin(Mathf.PI * (float)swingTimer / (float)TotalSwingTime) * 6f + 4f) * this.rotation;
                //this.firstChunk.vel *= 0f;
                

                //check for wall collision
                Vector2 properPosition = this.firstChunk.pos;
                Vector2 properLastPos = this.firstChunk.lastPos;
                this.firstChunk.pos += this.rotation * 15f;

                this.firstChunk.vel = 15f * swingDir;
                this.firstChunk.checkAgainstSlopesVertically();

                this.firstChunk.pos += this.rotation * 15f;
                this.firstChunk.CheckVerticalCollision();
                this.firstChunk.CheckHorizontalCollision();
                if (this.firstChunk.ContactPoint.x != 0 || this.firstChunk.ContactPoint.y != 0)
                    this.ActuallyHitWall();

                this.firstChunk.pos = properPosition;
                this.firstChunk.lastPos = properLastPos;
                this.firstChunk.vel = this.thrownBy.firstChunk.vel + this.rotation * 55f;
            }
        }

        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
            this.firstChunk.vel = throwDir.ToVector2() * 70f;

            swingDir.x = throwDir.x;
            swingDir.y = throwDir.y;
            if (thrownBy is Player)
            {
                var input = (thrownBy as Player).input[0];
                if (throwDir.y != 0 || (OmnidirectionalSwings && input.x == 0 && input.y != 0))
                {
                    swingDir.x = input.x * 0.5f;
                    if (swingDir.x == 0)
                        swingDir.x = Mathf.Clamp(input.analogueDir.x * 1.5f, -0.5f, 0.5f);
                    if (swingDir.y == 0)
                        swingDir.y = input.y;
                }
                else
                {   
                    swingDir.y = input.y;
                    if (swingDir.y == 0)
                        swingDir.y = Mathf.Clamp(input.analogueDir.y * 1.5f, -1f, 1f);
                }

                //exhaustion
                swingExhaustion = (thrownBy as Player).aerobicLevel;
                swingExhaustion *= swingExhaustion; //square swingExhaustion
                if ((thrownBy as Player).gourmandExhausted)
                    swingExhaustion = 1f;

                //throwing ability damage modifiers
                int throwingAbility = (thrownBy as Player).slugcatStats.throwingSkill;
                if (throwingAbility == 2)
                    swingDamage = 1.25f;
                else if (throwingAbility == 0)
                    swingDamage = 0.8f;
                else
                    swingDamage = 1f;

                if ((thrownBy as Player).isGourmand) //Gourmand code
                {
                    swingDamage = ((thrownBy as Player).gourmandExhausted) ? 0.3f : 3f;
                    if (SwordMod.Options.ExhaustsGourmand.Value)
                        (thrownBy as Player).AerobicIncrease(5f);
                }

                else if (UseStaminaMechanics && !(thrownBy as Player).gourmandExhausted) //make player exhausted
                {
                    //aerobic increase relative to throwing damage (e.g: Monk gets less tired from a swing than Artificer)
                    (thrownBy as Player).AerobicIncrease(ExhaustionRateModifier * swingDamage - 0.75f); //-0.75f counteracts increase from other throwing code

                    //make player exhausted?
                    if ((thrownBy as Player).aerobicLevel >= 0.9f)
                    {
                        (thrownBy as Player).aerobicLevel = 0.6f;
                        (thrownBy as Player).gourmandExhausted = true;
                    }
                }

            }
            else //non-player code
            {
                swingDamage = 0.5f;
                swingExhaustion = 0.2f;
            }

            if (Mathf.Abs(swingDir.x) < 1f)
                swingDir.x += Random.value * 0.8f - 0.4f;
            else
                swingDir.y += Random.value * 0.8f - 0.4f;
            //swingDamage *= SwingDamageModifier;

            //exhaustion
            if (UseStaminaMechanics)
            {
                swingExhaustion = Mathf.Clamp01(swingExhaustion);
                swingDamage *= 1f - (1 - ExhaustionDamageModifier) * swingExhaustion;
            }
            else
                swingExhaustion = 0f;

            swingTimer = TotalSwingTime + 1;
            canHitWall = true;
            canHitCreatureSound = true;

            //hit all weapons in front of it
            this.DetectAndParryThrownObjects();

            //add velocity to player if thrown by mid-air player
            if (thrownBy is Player && (thrownBy as Player).canJump <= 0)
            {
                var input = (thrownBy as Player).input[0];
                Vector2 stickDir = input.analogueDir;
                if (Mathf.Abs(stickDir.x) > 0.01f)
                    thrownBy.firstChunk.vel += stickDir * 5f * LungeModifier;
                else
                    thrownBy.firstChunk.vel += 5f * (new Vector2(input.x, input.y)).normalized * LungeModifier;
            }

            //sound
            if (SwordMod.CustomSwingSound)
                this.room.PlayCustomChunkSound(SwordMod.SwingSoundId, this.firstChunk, swingDamage, 1f);
            else
                room.PlaySound(SoundID.Slugcat_Throw_Rock, base.firstChunk, false, swingDamage * 1.5f, 1.5f + 0.3f * UnityEngine.Random.value);
        }

        private void DetectAndParryThrownObjects()
        {
            //manually check for other swords
            Vector2 origPos = this.firstChunk.pos;
            if (Mathf.Abs(swingDir.x) < 1f)
                this.firstChunk.pos = thrownBy.firstChunk.pos + 60f * Custom.DegToVec(90f - swingDir.y * (90f - 60f * swingDir.x));
            else
                this.firstChunk.pos = thrownBy.firstChunk.pos + 60f * Custom.DegToVec(swingDir.x * (90f - 60f * swingDir.y));
            foreach (PhysicalObject obj in this.room.physicalObjects[0])
            {
                float mod = (obj is Sword) ? 1f : 2f;
                if (Mathf.Abs(swingDir.x) < 1f)
                {
                    if (obj != this && obj is Weapon && Mathf.Abs(this.firstChunk.pos.x - obj.firstChunk.pos.x) < 50f * mod && Mathf.Abs(this.firstChunk.pos.y - obj.firstChunk.pos.y) < 100f * mod)
                        this.HitAnotherThrownWeapon(obj as Weapon);
                }
                else
                {
                    if (obj != this && obj is Weapon && Mathf.Abs(this.firstChunk.pos.x - obj.firstChunk.pos.x) < 100f * mod && Mathf.Abs(this.firstChunk.pos.y - obj.firstChunk.pos.y) < 50f * mod)
                        this.HitAnotherThrownWeapon(obj as Weapon);
                }
            }
            this.firstChunk.pos = origPos;
        }

        //private bool actuallyHitWall = false;
        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj == null)
            {
                //if (result.hitSomething)
                //{
                    //actuallyHitWall = true;
                    //this.HitWall();
                //}
                return false;
            }

            /*
            if (result.chunk == null)
            {
                if (result.onAppendagePos != null)
                {
                    (result.obj as PhysicalObject.IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, 0.1f * KnockbackModifier * swingDamage * this.firstChunk.mass * (base.firstChunk.vel * 0.5f + swingDir * 3f));
                }
                return false;
            }
            */

            if (result.obj is Creature && swingTimer <= CreatureDamageTime)
            {
                //don't let scavengers hit other scavengers
                if (this.thrownBy is Scavenger && result.obj is Scavenger)
                    return false;

                //don't hit players when spears hit is off
                if (result.obj is Player && ((!this.room.game.IsArenaSession && ModManager.CoopAvailable && !Custom.rainWorld.options.friendlyFire) || (this.room.game.IsArenaSession && !this.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers)))
                {
                    return false;
                }

                float massMult = (result.chunk == null) ? 0.1f : ((result.chunk.mass < this.firstChunk.mass) ? result.chunk.mass / this.firstChunk.mass : 1f);
                if (swingDir.y < -0.5f) //down-swing
                {
                    (result.obj as Creature).Violence(base.firstChunk, KnockbackModifier * swingDamage * this.firstChunk.mass * massMult * (base.firstChunk.vel * 0.5f + swingDir * 3f), result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, swingDamage * SwingDamageModifier, StunModifier * swingDamage * 24f * (1f + 0.3f * this.thrownBy.mainBodyChunk.vel.magnitude));
                }
                else if (swingDir.y > 0.5f) //up-swing
                {
                    (result.obj as Creature).Violence(base.firstChunk, KnockbackModifier * swingDamage * this.firstChunk.mass * massMult * (base.firstChunk.vel * 0.5f + new Vector2(swingDir.x * 3f, swingDir.y * 30f * VerticalKnockbackModifier)), result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, swingDamage * SwingDamageModifier, StunModifier * swingDamage * 16f * (1f + 0.1f * this.thrownBy.mainBodyChunk.vel.magnitude));
                }
                else //stab
                {
                    (result.obj as Creature).Violence(base.firstChunk, KnockbackModifier * swingDamage * this.firstChunk.mass * massMult * (base.firstChunk.vel * 1.0f + swingDir * 3f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, swingDamage * SwingDamageModifier, StunModifier * swingDamage * 8f * (1f + 0.2f * this.thrownBy.mainBodyChunk.vel.magnitude));
                }

                Vector2 hitPos;
                try
                {
                    hitPos = (result.chunk == null) ? result.onAppendagePos.appendage.segments[result.onAppendagePos.prevSegment] : result.chunk.pos;
                } catch (Exception ex)
                {
                    hitPos = this.firstChunk.pos + this.firstChunk.vel * 1.5f;
                }

                //sounds and particles
                //if sticks in creature (or would, if it were a spear)
                if ((result.obj as Creature).SpearStick(this, swingDamage * SwingDamageModifier, result.chunk, result.onAppendagePos, this.firstChunk.vel))
                {
                    if (canHitCreatureSound)
                    {
                        if (SwordMod.CustomHitSoftSound)
                            this.room.PlayCustomChunkSound(SwordMod.HitSoftSoundId, this.firstChunk, swingDamage, 0.9f + 0.3f * UnityEngine.Random.value);
                        else
                            this.room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk, false, swingDamage * 1.3f, 1.2f + 0.3f * UnityEngine.Random.value);
                    }
                    for (int i = 0; i < Mathf.Floor(5 * swingDamage); i++)
                    {
                        this.room.AddObject(new WaterDrip(Vector2.Lerp(this.firstChunk.pos + this.firstChunk.vel, hitPos, Random.value), Custom.RNV() * 20f * Random.value, false));
                    }
                }
                else //if bounces off creature
                {
                    if (canHitCreatureSound)
                    {
                        if (SwordMod.CustomHitHardSound)
                            this.room.PlayCustomChunkSound(SwordMod.HitHardSoundId, this.firstChunk, swingDamage, 0.9f + 0.3f * UnityEngine.Random.value);
                        else
                            this.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, base.firstChunk, false, swingDamage, 1.2f + 0.3f * UnityEngine.Random.value);
                    }
                    for (int i = 0; i < Mathf.Floor(5 * swingDamage); i++)
                    {
                        this.room.AddObject(new Spark(Vector2.Lerp(this.firstChunk.pos + this.firstChunk.vel, hitPos, Random.value), Custom.RNV() * 20f * Random.value, new Color(0.8f, 0.7f, 0.7f), null, 2, 4));
                    }
                }
                canHitCreatureSound = false;

                if (ModManager.MSC && result.obj is Player)
                {
                    Player player = result.obj as Player;
                    player.playerState.permanentDamageTracking += (double)((swingDamage * SwingDamageModifier) / player.Template.baseDamageResistance);
                    if (player.playerState.permanentDamageTracking >= 1.0)
                    {
                        player.Die();
                    }
                }

                //swing pushback
                Vector2 pushDir = ((this.thrownBy.mainBodyChunk.pos - hitPos).normalized - swingDir.normalized + (this.thrownBy.mainBodyChunk.pos - (this.firstChunk.pos + this.firstChunk.vel)).normalized) / 3f;
                pushDir.x *= HorizontalPushbackModifier;
                pushDir.y *= VerticalPushbackModifier;
                pushDir *= swingDamage * 20f;

                //if (this.thrownBy.mainBodyChunk.vel.y > 0)
                    pushDir.y -= this.thrownBy.mainBodyChunk.vel.y * 0.5f;
                //else
                    //pushDir.y -= this.thrownBy.mainBodyChunk.vel.y * 0.5f;
                if (pushDir.y < 0f)
                    pushDir.y = 0f;
                this.thrownBy.mainBodyChunk.vel += pushDir; //* 1f

                swingDamage *= 0.5f;

                //canHitWall = false;

                return true;
            }
            else if (result.obj is Weapon)
            {
                this.HitAnotherThrownWeapon(result.obj as Weapon);
                return true;
            }

            return false;
        }

        public override void HitByWeapon(Weapon weapon)
        {
            base.HitByWeapon(weapon);

            if (weapon is Sword && AllowParries && this.thrownBy != null && weapon.thrownBy != null && weapon.thrownBy.abstractCreature.ID != this.thrownBy.abstractCreature.ID)
            {
                if (SwordMod.CustomParrySound)
                    this.room.PlayCustomChunkSound(SwordMod.ParrySoundId, this.firstChunk, swingDamage, 0.9f + 0.3f * UnityEngine.Random.value);
                else
                    this.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, base.firstChunk, false, swingDamage * 1.8f, 1.7f + 0.3f * UnityEngine.Random.value);

                //apply force to both wielders
                weapon.thrownBy.firstChunk.vel.x += swingDamage * swingDir.x * 5f * (1 + HorizontalPushbackModifier);

                this.thrownBy.firstChunk.vel.x += (weapon as Sword).swingDamage * (weapon as Sword).swingDir.x * 5f * (1 + HorizontalPushbackModifier);

                //if (Math.Abs(swingDir.x) < 0.5f && swingDir.y < 0) //if I am swinging down, pogo upward
                //{
                    this.thrownBy.firstChunk.vel.y *= 0.5f;
                    this.thrownBy.firstChunk.vel.y -= (weapon as Sword).swingDamage * swingDir.y * 15f * VerticalPushbackModifier;
                //}
                //if (Math.Abs((weapon as Sword).swingDir.x) < 0.5f && (weapon as Sword).swingDir.y < 0) //if the other is swinging down, pogo upward
                //{
                    weapon.thrownBy.firstChunk.vel.y *= 0.5f;
                    weapon.thrownBy.firstChunk.vel.y -= swingDamage * (weapon as Sword).swingDir.y * 15f * VerticalPushbackModifier;
                //}

                float oldSwingDamage = swingDamage;
                swingDamage -= (weapon as Sword).swingDamage;
                (weapon as Sword).swingDamage -= oldSwingDamage;

                swingDamage *= 0.5f;
                (weapon as Sword).swingDamage *= 0.5f;
            }
        }

        new public void HitAnotherThrownWeapon(Weapon obj)
        {
            if (obj.mode != Weapon.Mode.Thrown)
                return;

            if (canHitWall && this.thrownBy != null && obj.thrownBy != null && this.thrownBy.abstractCreature.ID != obj.thrownBy.abstractCreature.ID)
            {
                if (obj.firstChunk.pos.x - obj.firstChunk.lastPos.x < 0f == base.firstChunk.pos.x - base.firstChunk.lastPos.x < 0f)
                {
                    return;
                }
                if (this.abstractPhysicalObject.world.game.IsArenaSession && this.thrownBy != null && this.thrownBy is Player)
                {
                    this.abstractPhysicalObject.world.game.GetArenaGameSession.arenaSitting.players[0].parries++;
                }
                Vector2 vector = Vector2.Lerp(obj.firstChunk.lastPos, base.firstChunk.lastPos, 0.5f);
                int num = 15;
                for (int i = 0; i < num; i++)
                {
                    this.room.AddObject(new Spark(vector + Custom.DegToVec(Random.value * 360f) * 5f * Random.value, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(2f, 7f, Random.value) * (float)num, new Color(1f, 1f, 1f), null, 10, 170));
                }

                canHitWall = false;
            }

            this.HitByWeapon(obj);

            //deflect non-sword weapons
            if (obj is not Sword)
            {
                obj.ChangeMode(Weapon.Mode.Free);
                obj.firstChunk.vel += KnockbackModifier * this.swingDir * 30f;
            }
        }

        public override void HitWall()
        {
            //base.HitWall(); //this prevents the rock hit sound from playing
        }

        public void ActuallyHitWall()
        {
            if (canHitWall)
            {
                if (this.room.BeingViewed)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        this.room.AddObject(new Spark(base.firstChunk.pos + this.firstChunk.vel, Custom.DegToVec(Random.value * 360f) * 10f * Random.value + -this.firstChunk.vel, new Color(1f, 1f, 1f), null, 2, 4));
                    }
                }
                this.room.ScreenMovement(new Vector2?(base.firstChunk.pos), this.throwDir.ToVector2() * 1.5f, 0f);
                if (SwordMod.CustomHitWallSound)
                    this.room.PlayCustomChunkSound(SwordMod.HitWallSoundId, this.firstChunk, swingDamage, 0.9f + 0.3f * UnityEngine.Random.value);
                else
                    this.room.PlaySound(SoundID.Spear_Bounce_Off_Wall, base.firstChunk, false, swingDamage, 1.2f + 0.3f * UnityEngine.Random.value);

                canHitWall = false;
            }
        }

    }
}
