using System;
using MonoPunk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Shapes;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace DungeonRacer
{
	class Player : Entity
	{
		public event Action<Player, ItemType> OnCollect;
		public event Action<Player, ItemType> OnUse;
		public event Action<Player, float> OnModifyHp;
		public event Action<Player, float> OnModifyMp;

		public float Hp { get; private set; }
		public int MaxHp { get; private set; }
		public float Mp { get; private set; }
		public int MaxMp { get; private set; }

		public float Speed { get; private set; }
		public float Angle { get; private set; }
		public float DriftPct { get; private set; }

		private Vector2 velocity;
		public Vector2 Velocity { get { return velocity; } }

		private float driftAngle;
		private float angularVelocity;

		public bool Paused { get; set; } // FIXME

		private float enginePct;

		private PlayerData data;
		private readonly Dictionary<ItemType, int> inventory = new Dictionary<ItemType, int>();

		private readonly Animator sprite;

		private readonly SoundEffectInstance engineSound;
		//private readonly SoundEffectInstance driftSound;

		public Player(PlayerData data, DungeonTile tile) : base()
		{
			this.data = data;
			X = tile.X * Global.TileSize + Global.TileSize / 2;
			Y = tile.Y * Global.TileSize + Global.TileSize / 2;
			Type = Global.TypePlayer;
			Layer = Global.LayerMain;
			Collider = data.PixelMask;
			OriginX = Width / 2;
			OriginY = Height / 2;

			Hp = data.Hp;
			MaxHp = data.Hp;

			Mp = data.Mp;
			MaxMp = data.Mp;

			sprite = new Animator(data.Anim);
			sprite.CenterOrigin();
			sprite.Play("idle");
			Add(sprite);

			engineSound = Asset.LoadSoundEffect("sfx/engine").CreateInstance();
			engineSound.Volume = 0.0f;
			engineSound.IsLooped = true;
			engineSound.Play();

			//driftSound = Asset.LoadSoundEffect("sfx/drift").CreateInstance();
			//driftSound.Volume = 0.0f;
			//driftSound.IsLooped = true;
			//driftSound.Play();

			Engine.Track(this, "Speed");
			Engine.Track(this, "Angle");
			Engine.Track(this, "DriftPct");
			Engine.Track(this, "enginePct");
			Engine.Track(this, "velocity");
			Engine.Track(this, "driftAngle");
		}

		public void SetData(PlayerData data)
		{
			this.data = data;
		}

		public void Damage(float value)
		{
			Hp -= value;
			if (Hp <= 0.0f)
			{
				Hp = 0.0f;
			}
			OnModifyHp?.Invoke(this, value);

			if (Hp == 0.0f)
			{
				// TODO die
			}
		}

		public void Heal(float value)
		{
			Hp += value;
			if (Hp > MaxHp) Hp = MaxHp;
			OnModifyHp?.Invoke(this, value);
		}

		public void GainMp(float value)
		{
			Mp += value;
			if (Mp > MaxMp) Mp = MaxMp;
			OnModifyMp?.Invoke(this, value);
		}

		public void LooseMp(float value)
		{
			Mp -= value;
			if (Mp < 0.0f) Mp = 0.0f;
			OnModifyMp?.Invoke(this, -value);
		}

		public void AddItem(ItemType item)
		{
			inventory.TryGetValue(item, out int count);
			inventory[item] = count + 1;
			OnCollect?.Invoke(this, item);
		}

		public bool UseItem(ItemType item)
		{
			if (!inventory.TryGetValue(item, out int count) || count == 0)
			{
				return false;
			}

			inventory[item] = count - 1;
			OnUse?.Invoke(this, item);
			return true;
		}

		public int GetItemCount(ItemType item)
		{
			inventory.TryGetValue(item, out int count);
			return count;
		}

		protected override void OnUpdate(float deltaTime)
		{
			if (Paused)
			{
				engineSound.Volume = 0.0f;
				//driftSound.Volume = 0.0f;
				return;
			}

			base.OnUpdate(deltaTime);

			if (Mp > 0.0f && Input.IsDown("special"))
			{
				velocity += Vector2.UnitX.Rotate(Angle) * data.BoostForce * deltaTime;
				enginePct = 1.0f;
				LooseMp(data.BoostManaPerSec * deltaTime);
			}
			else if (Input.IsDown("move_front"))
			{
				// front gear
				enginePct += deltaTime * (1.0f - enginePct) * data.EngineSpeed;
				enginePct = Mathf.Clamp(enginePct, 0.1f, 1.0f);
				velocity += Vector2.UnitX.Rotate(Angle) * data.FrontGearForce * deltaTime * enginePct;
			}
			else
			{
				enginePct *= data.EngineDecay;
				if (enginePct < 0.1f) enginePct = 0.0f;
			}

			var goingForward = Vector2.Dot(velocity, Vector2.UnitX.Rotate(Angle)) > 3.0f;

			if (Input.IsDown("move_back"))
			{
				if (goingForward)
				{
					// break
					velocity *= data.BreakFriction;
				}
				else
				{
					// rear gear
					velocity -= Vector2.UnitX.Rotate(Angle) * data.RearGearForce * deltaTime;
				}
			}

			var speedPct = Speed / 100.0f;
			if (Input.IsDown("left"))
			{
				angularVelocity -= data.TurnSpeed * deltaTime * speedPct;
			}
			if (Input.IsDown("right"))
			{
				angularVelocity += data.TurnSpeed * deltaTime * speedPct;
			}

			velocity *= data.Friction;
			Angle += angularVelocity;
			while (Angle < 0.0f) Angle += Mathf.Pi2;
			while (Angle > Mathf.Pi2) Angle -= Mathf.Pi2;
			angularVelocity *= data.AngularFriction;

			if (goingForward)
			{
				Speed = velocity.Length();
				driftAngle = Velocity.AngleWith(Vector2.UnitX.Rotate(Angle));
			}
			else
			{
				Speed = -velocity.Length();
				driftAngle = 0.0f;
			}

			MoveBy(velocity * deltaTime, CollisionFlags.NonStop, Global.TypeMap, Global.TypeEntity);

			if (Speed > 80.0f && driftAngle > 0.25f)
			{
				//var speedPct = Mathf.Min(1.0f, (Speed - 80.0f) / 10.0f);
				var anglePct = Mathf.Min(1.0f, (driftAngle - 0.25f) / 0.3f);
				DriftPct = speedPct * anglePct * 0.5f;
			}
			else
			{
				DriftPct = 0.0f;
			}

			engineSound.Pitch = enginePct;
			engineSound.Volume = enginePct;

			//if (DriftPct > 0.0f)
			//{
			//	driftSound.Pitch = 1.0f;
			//	driftSound.Volume = DriftPct * 0.25f;
			//}
			//else
			//{
			//	driftSound.Volume = 0.0f;
			//}

			sprite.Rotation = ((int)(Angle * 16.0f)) / 16.0f;
			sprite.SortOrder = Mathf.Floor(Bottom) * 10;
		}

		protected override bool OnHit(HitInfo info)
		{
			var stop = true;
			if (info.Other is GameEntity)
			{
				stop = ((GameEntity)info.Other).HandlePlayerHit(this, info.DeltaX, info.DeltaY);
			}

			if (stop)
			{
				Speed = 0.0f;
				driftAngle = 0.0f;
				enginePct = 0.0f;

				if (info.IsVerticalMovement)
				{
					velocity.Y = -velocity.Y * PlayerData.BounceRestitution;
				}
				else if (info.IsHorizontalMovement)
				{
					velocity.X = -velocity.X * PlayerData.BounceRestitution;
				}

				Asset.LoadSoundEffect("sfx/hit").Play();
			}

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
