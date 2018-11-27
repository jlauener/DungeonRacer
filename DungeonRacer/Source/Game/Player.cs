using System;
using MonoPunk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Shapes;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace DungeonRacer
{
	enum DamageType
	{
		Wall,
		Spike,
		Lava,
		Entity
	}

	class Player : Entity
	{
		public PlayerData Data { get; }

		public float Speed { get; private set; }
		public float Angle { get; private set; }

		public bool Alive { get { return state != State.GameOver; } }
		public bool Paused
		{
			get { return state == State.Paused; }
			set { state = value ? State.Paused : State.Active; }
		}

		private float enginePct;
		private float driftPct;
		private float tireBloodPct;
		private int bloodFront;
		private int bloodBack;

		private enum State
		{
			Active,
			Paused,
			GameOver
		}
		private State state = State.Active;

		private Vector2 velocity;
		public Vector2 Velocity { get { return velocity; } }

		private Vector2 bounceVelocity;

		private float spikeImmuneCounter;
		private float lavaImmuneCounter;

		private int combo;
		private EntityData comboEntity;
		private float comboCounter;

		private enum EngineState
		{
			Idle,
			FrontGear,
			RearGear,
			Break
		}
		private EngineState engineState = EngineState.Idle;

		private float actualAngle;
		private float driftAngle;
		private float forward;
		private float angularVelocity;

		private float invincibleCounter;

		private readonly Renderable sortNode;
		private readonly Animator sprite;
		private readonly Animator bloodFrontSprite;
		private readonly Animator bloodBackSprite;
		private readonly Animator hurtSprite;
		private readonly Blinker blinker;

		private static SoundEffectInstance engineSound;
		private static SoundEffectInstance[] driftSounds;
		private static SoundEffectInstance breakSound;

		private SoundEffectInstance driftSound;

		public Player(PlayerData data, DungeonTile tile, Direction direction)
		{
			Data = data;

			X = tile.ScreenX + Global.TileSize / 2;
			Y = tile.ScreenY + Global.TileSize / 2;
			Angle = DirectionUtils.GetAngle(direction);

			Type = Global.TypePlayer;
			Layer = Global.LayerMain;
			Collider = data.PixelMask;
			OriginX = Width / 2;
			OriginY = Height / 2;

			sortNode = new Renderable();
			Add(sortNode);

			sprite = new Animator(data.Anim);
			sprite.CenterOrigin();
			sprite.Play("idle");
			sortNode.Add(sprite);

			bloodFrontSprite = new Animator(data.Anim);
			bloodFrontSprite.CenterOrigin();
			sortNode.Add(bloodFrontSprite);

			bloodBackSprite = new Animator(data.Anim);
			bloodBackSprite.CenterOrigin();
			sortNode.Add(bloodBackSprite);

			hurtSprite = new Animator(data.Anim);
			hurtSprite.CenterOrigin();
			sortNode.Add(hurtSprite);

			blinker = new Blinker(PlayerData.InvincibleBlinkInterval, sortNode);
			blinker.Enabled = false;
			Add(blinker);

			if (engineSound == null) engineSound = Asset.LoadSoundEffect("sfx/car_engine").CreateInstance();
			engineSound.Volume = 0.0f;
			engineSound.IsLooped = true;
			engineSound.Play();

			if (driftSounds == null)
			{
				driftSounds = new SoundEffectInstance[2];
				for (var i = 0; i < driftSounds.Length; i++)
				{
					driftSounds[i] = Asset.LoadSoundEffect("sfx/car_drift_" + (i + 1)).CreateInstance();
					driftSounds[i].Volume = 0.7f;
				}
			}

			if (breakSound == null) breakSound = Asset.LoadSoundEffect("sfx/car_break").CreateInstance();
			breakSound.Volume = 0.7f;

			Engine.Track(this, "Position");
			Engine.Track(this, "Speed");
			Engine.Track(this, "Angle");
			Engine.Track(this, "engineState");
			Engine.Track(this, "enginePct");
			Engine.Track(this, "driftPct");
			Engine.Track(this, "tireBloodPct");
			Engine.Track(this, "velocity");
			Engine.Track(this, "bounceVelocity");
			Engine.Track(this, "driftAngle");
			Engine.Track(this, "forward");
		}

		public void Damage(int value, DamageType damageType)
		{
			if (invincibleCounter > 0.0f) return;

			switch (damageType)
			{
				case DamageType.Spike:
					if (Speed < PlayerData.SpikeDamageSpeedMin || spikeImmuneCounter > 0.0f) return;
					break;
				case DamageType.Lava:
					if (lavaImmuneCounter > 0.0f) return;
					break;
			}

			Data.Damage(value, damageType);

			switch (damageType)
			{
				case DamageType.Wall:
					{
						break;
					}
				case DamageType.Spike:
					spikeImmuneCounter += Data.SpikeImmuneDuration;
					ShowHurtFrame(0.15f);
					Scene.GetEntity<Shaker>().Shake(Vector2.Normalize(velocity) * 4.0f);
					break;
				case DamageType.Lava:
					lavaImmuneCounter += Data.LavaImmuneDuration;
					ShowHurtFrame(0.1f);
					Scene.GetEntity<Shaker>().Shake(Vector2.Normalize(velocity) * 3.0f);
					break;
				case DamageType.Entity:
					{
						var pct = value / (float)PlayerData.EntityDamageFeedbackMax;
						ShowHurtFrame(0.1f + pct * 0.2f);
						Scene.GetEntity<Shaker>().Shake(Vector2.Normalize(velocity) * (4.0f + pct * 12.0f));
						break;
					}
			}

			Log.Debug("damage " + value + " hp=" + Data.Hp);

			if (Data.Hp == 0)
			{
				// TODO
				state = State.GameOver;
			}
		}

		private Tween hurtFrameCallback;
		private void ShowHurtFrame(float duration)
		{
			if (hurtFrameCallback != null) hurtFrameCallback.Cancel();
			hurtSprite.Play("hurt");
			hurtFrameCallback = Scene.Callback(duration, hurtSprite.Stop);
		}

		public void Heal(int value)
		{
			Data.Heal(value);
		}

		//public void AddItem(ItemType item)
		//{
		//	//if(item == ItemType.Coin)
		//	//{
		//	//	((GameScene)Scene).AddTime(1.0f);
		//	//}
		//	inventory.TryGetValue(item, out int count);
		//	inventory[item] = count + 1;
		//	OnCollect?.Invoke(this, item);
		//}

		//public bool UseItem(ItemType item)
		//{
		//	if (!inventory.TryGetValue(item, out int count) || count == 0)
		//	{
		//		return false;
		//	}

		//	inventory[item] = count - 1;
		//	OnUse?.Invoke(this, item);
		//	return true;
		//}

		//public int GetItemCount(ItemType item)
		//{
		//	inventory.TryGetValue(item, out int count);
		//	return count;
		//}

		public void AddTireBlood(float pct)
		{
			tireBloodPct += pct;
		}

		protected override void OnUpdate(float deltaTime)
		{
			forward = Vector2.Dot(velocity, Vector2.UnitX.Rotate(Angle));

			var angleId = (int)((Angle / Mathf.Pi2) * PlayerData.AngleResolution);
			actualAngle = (angleId / PlayerData.AngleResolution) * Mathf.Pi2;

			switch (state)
			{
				case State.Active:
					UpdateActive(deltaTime);
					break;
				//case State.Paused:
				//	UpdatePaused(deltaTime);
				//	break;
				case State.GameOver:
					UpdateGameOver(deltaTime);
					break;
			}

			// FIXME... all this is fuzzy..
			if (forward > 3.0f)
			{
				driftAngle = Velocity.AngleWith(Vector2.UnitX.Rotate(Angle));
			}
			else
			{
				driftAngle = 0.0f;
			}

			if (Speed > 80.0f && driftAngle > 0.25f)
			{
				var anglePct = Mathf.Min(1.0f, (driftAngle - 0.25f) / 0.3f);
				driftPct = (Speed / Data.MaxSpeed) * anglePct * 0.5f;
			}
			else
			{
				driftPct = 0.0f;
			}

			if (Speed > 80.0f && driftAngle > 0.6f)
			{
				if (driftSound == null)
				{
					driftSound = driftSounds[Rand.NextInt(driftSounds.Length)];
					driftSound.Play();
				}
			}
			else if (driftAngle < 0.1f)
			{
				if(driftSound != null)
				{
					driftSound.Stop();
					driftSound = null;
				}
			}

			engineSound.Pitch = enginePct;
			engineSound.Volume = enginePct * 0.5f;

			if (tireBloodPct > 0.05f)
			{
				tireBloodPct *= 0.9f;
				Scene.GetEntity<DungeonMap>().DrawGroundEffect(X, Y, "tire", new Color(0xCF, 0x32, 0x32), tireBloodPct * 0.35f, Angle);
			}
			else if (driftPct > 0.1f)
			{
				Scene.GetEntity<DungeonMap>().DrawGroundEffect(X, Y, "tire", new Color(0x00, 0x00, 0x00), driftPct * 0.15f, Angle);
			}

			sprite.Rotation = actualAngle;
			bloodFrontSprite.Rotation = actualAngle;
			bloodBackSprite.Rotation = actualAngle;
			hurtSprite.Rotation = actualAngle;
			sortNode.SortOrder = Mathf.Floor(Bottom) * 10;

			base.OnUpdate(deltaTime);

		}

		//private void UpdatePaused(float deltaTime)
		//{
		//	breakSound.Stop();
		//	foreach (var sound in driftSound) sound.Stop();
		//}

		private void UpdateGameOver(float deltaTime)
		{
			enginePct = 0.0f;
			velocity *= Data.Friction;
			bounceVelocity *= PlayerData.BounceFriction;
			if (bounceVelocity.Length() < 5.0f) bounceVelocity = Vector2.Zero;
			MoveBy((velocity + bounceVelocity) * deltaTime, CollisionFlags.NonStop, Global.TypeSolid, Global.TypeMap);
		}

		private void UpdateActive(float deltaTime)
		{
			if (comboCounter > 0.0f)
			{
				comboCounter -= deltaTime;
				if(comboCounter <= 0.0f)
				{
					comboEntity = null;
					comboCounter = 0.0f;
					combo = 0;

					Data.NotifyComboStop();
				}
			}

			if (invincibleCounter > 0.0f)
			{
				invincibleCounter -= deltaTime;
				if (invincibleCounter <= 0.0f)
				{
					blinker.Enabled = false;
				}
			}

			if (spikeImmuneCounter > 0.0f) spikeImmuneCounter -= deltaTime;
			if (lavaImmuneCounter > 0.0f) lavaImmuneCounter -= deltaTime;

			if (Input.IsDown("a") || Input.IsDown("up"))
			{
				if (forward < -3.0f)
				{
					// break
					engineState = EngineState.Break;
				}
				else
				{
					// front gear
					enginePct += deltaTime * (1.0f - enginePct) * Data.FrontGearSpeed;
					enginePct = Mathf.Clamp(enginePct, 0.1f, 1.0f);
					engineState = EngineState.FrontGear;
				}
			}
			else if (Input.IsDown("b") || Input.IsDown("down"))
			{
				if (forward > 3.0f)
				{
					// break
					engineState = EngineState.Break;
				}
				else
				{
					// rear gear
					enginePct += deltaTime * (1.0f - enginePct) * Data.RearGearSpeed;
					enginePct = Mathf.Clamp(enginePct, 0.1f, 1.0f);
					engineState = EngineState.RearGear;
				}
			}
			else
			{
				// idle
				enginePct *= Data.EngineDecay;
				if (enginePct < 0.1f)
				{
					enginePct = 0.0f;
					engineState = EngineState.Idle;
				}
			}

			var turnDir = 0;
			switch (engineState)
			{
				case EngineState.Break:
					enginePct = 0.0f;
					velocity *= Data.BreakFriction;
					break;
				case EngineState.FrontGear:
					velocity += Vector2.UnitX.Rotate(actualAngle) * Data.FrontGearForce * deltaTime * enginePct;
					turnDir = 1;
					break;
				case EngineState.RearGear:
					velocity -= Vector2.UnitX.Rotate(actualAngle) * Data.RearGearForce * deltaTime * enginePct;
					turnDir = -1;
					break;
			}

			velocity *= Data.Friction;

			Speed = velocity.Length();
			Speed = Mathf.Min(Speed, Data.MaxSpeed);
			var speedPct = Speed / Data.MaxSpeed;

			if (Input.IsDown("left"))
			{
				angularVelocity -= Data.TurnSpeed * deltaTime * speedPct * turnDir;
			}
			if (Input.IsDown("right"))
			{
				angularVelocity += Data.TurnSpeed * deltaTime * speedPct * turnDir;
			}

			Angle += angularVelocity;
			while (Angle < 0.0f) Angle += Mathf.Pi2;
			while (Angle > Mathf.Pi2) Angle -= Mathf.Pi2;
			angularVelocity *= Data.AngularFriction;

			bounceVelocity *= PlayerData.BounceFriction;
			if (bounceVelocity.Length() < 5.0f) bounceVelocity = Vector2.Zero;

			MoveBy((velocity + bounceVelocity) * deltaTime, CollisionFlags.NonStop, Global.TypeCollectible, Global.TypeTrigger, Global.TypeEnemy, Global.TypeSolid, Global.TypeMap);
			TriggerCollisionDetection(Global.TypeTrigger);

			if (engineState == EngineState.Break) breakSound.Play(); else breakSound.Stop();
		}

		protected override bool OnHit(HitInfo hit)
		{
			if (!Alive) return true;

			var bounce = PlayerData.BounceRestitution;
			var stop = false;
			if (hit.Other.Type == Global.TypeMap)
			{
				HandleWallCollision();
				stop = true;
			}
			else if (hit.Other.Type == Global.TypeTrigger)
			{
				stop = HandleTriggerCollision(hit);
			}
			else if (hit.Other is GameEntity)
			{
				stop = HandleEntityCollision(hit);
				if (hit.Other is Enemy) bounce = 2.2f; // FIXME
			}

			if (stop)
			{
				if (hit.IsVerticalMovement)
				{
					bounceVelocity.Y = -velocity.Y * bounce;
					velocity *= 0.2f;
				}
				else if (hit.IsHorizontalMovement)
				{
					bounceVelocity.X = -velocity.X * bounce;
					velocity.X *= 0.2f;
				}

				// FIXME ?
				driftAngle = 0.0f;
			}

			return stop;
		}

		private void HandleWallCollision()
		{
			var speedPct = Speed / Data.MaxSpeed;
			if (speedPct > PlayerData.WallDamageSpeedMin)
			{
				var damagePct = (speedPct - PlayerData.WallDamageSpeedMin) * (1.0f / (1.0f - PlayerData.WallDamageSpeedMin));
				var damage = Data.WallDamageMin + (int)(damagePct * (Data.WallDamageMax - Data.WallDamageMin));
				Damage(damage, DamageType.Wall);

				Scene.GetEntity<Shaker>().Bounce(Vector2.Normalize(-velocity) * 2.0f, 0.07f);
				ShowHurtFrame(0.07f);
				Asset.LoadSoundEffect("sfx/car_hit_hurt").Play();
			}
			else if (speedPct > 0.25f)
			{
				Scene.GetEntity<Shaker>().Bounce(Vector2.Normalize(-velocity) * 1.0f, 0.04f);
				Asset.LoadSoundEffect("sfx/car_hit").Play();
			}
		}

		private bool HandleTriggerCollision(HitInfo hit)
		{
			var trigger = hit.Tile.GetString("trigger");
			if (trigger == "Spike")
			{
				Damage(Data.SpikeDamage, DamageType.Spike);
			}
			else if (trigger == "Lava")
			{
				Damage(Data.LavaDamage, DamageType.Lava);
			}
			else
			{
				Log.Warn("Unknown trigger collision '" + trigger + "'");
			}

			return false;
		}

		private bool HandleEntityCollision(HitInfo hit)
		{
			var entity = (GameEntity)hit.Other;

			var flags = entity.HandlePlayerHit(this, hit.DeltaX, hit.DeltaY);

			if ((flags & HitFlags.Destroy) > 0)
			{
				if (comboEntity == null || comboEntity != entity.Data)
				{
					comboEntity = entity.Data;
					combo = 1;
				}
				else
				{
					combo++;
				}
				comboCounter = PlayerData.ComboWindow;
				Data.NotifyCombo(comboEntity, combo);
			}

			if ((flags & HitFlags.Blood) > 0)
			{
				if (forward > 0.0f)
				{
					if (bloodFront == 0)
					{
						bloodFront++;
						bloodFrontSprite.Play("blood_front_" + bloodFront);
					}
					else if (bloodFront < PlayerData.BloodFrontMax && Rand.NextFloat() < 0.33f)
					{
						bloodFront++;
						bloodFrontSprite.Play("blood_front_" + bloodFront);
					}
				}
				else
				{
					if (bloodBack == 0)
					{
						bloodBack++;
						bloodBackSprite.Play("blood_back_" + bloodBack);
					}
					else if (bloodBack < PlayerData.BloodBackMax && Rand.NextFloat() < 0.33f)
					{
						bloodBack++;
						bloodBackSprite.Play("blood_back_" + bloodBack);
					}
				}
			}

			var stop = (flags & HitFlags.Stop) > 0;
			if (stop) HandleWallCollision();
			return stop;
		}

		protected override void OnRenderDebug(SpriteBatch spriteBatch)
		{
			base.OnRenderDebug(spriteBatch);

			if (Speed > 30.0f)
			{
				spriteBatch.DrawLine(GlobalPosition, GlobalPosition + Velocity, Color.AliceBlue);
			}
			spriteBatch.DrawLine(GlobalPosition, GlobalPosition + Vector2.UnitX.Rotate(Angle) * 20.0f, Color.Yellow);
		}
	}
}
