using System;
using MonoPunk;
using Microsoft.Xna.Framework;

namespace DungeonRacer
{
	class Player : Entity
	{
		private const float BounceRestitution = 1.0f;

		public float Speed { get; private set; }
		public float Hp { get; private set; }
		public int MaxHp { get; private set; }

		private Vector2 velocity;
		public float VelocityX { get { return velocity.X; } }
		public float VelocityY { get { return velocity.Y; } }

		private float angle;
		private float angularVelocity;		

		//private enum TurnDir
		//{
		//	None,
		//	Left,
		//	Right
		//}
		//private TurnDir turnDir = TurnDir.None;
		//private float turnAngle;

		public int Key { get; private set; }
		public int Money { get; private set; }

		public bool Paused { get; set; }

		private PlayerData data;
		private readonly Animator sprite;

		public Player(PlayerData data, float x, float y) : base(x, y)
		{
			this.data = data;
			Type = Global.TypePlayer;
			Layer = Global.LayerMain;
			SetCenteredHitbox(12, 12);

			Hp = data.Hp;
			MaxHp = data.Hp;

			sprite = new Animator("player");
			sprite.CenterOrigin();
			sprite.Play("idle" + data.SpriteId);
			Add(sprite);

			Engine.Track(this, "Speed");
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
				// TODO die
			}
		}

		public void AddKey()
		{
			Key++;
		}

		public void AddMoney(int value)
		{
			Money += value;
		}

		public bool UseKey()
		{
			if (Key == 0)
			{
				return false;
			}

			Key--;
			return true;
		}

		protected override void OnUpdate(float deltaTime)
		{
			if (Paused) return;
			base.OnUpdate(deltaTime);

			if (Input.IsDown("a"))
			{
				// front gear
				velocity += Vector2.UnitX.Rotate(angle) * data.FrontGearForce * deltaTime;
			}

			if (Input.IsDown("b"))
			{
				var goingBackward = Vector2.Dot(velocity, Vector2.UnitX.Rotate(angle)) <= 5.0f;
				if (goingBackward)
				{
					// rear gear
					velocity -= Vector2.UnitX.Rotate(angle) * data.RearGearForce * deltaTime;
				}
				else
				{
					// break
					velocity *= data.BreakFriction;
				}
			}

			if (Input.IsDown("left"))
			{
				angularVelocity -= data.TurnSpeed * deltaTime;
			}
			if (Input.IsDown("right"))
			{
				angularVelocity += data.TurnSpeed * deltaTime;
			}

			velocity *= data.Friction;
			angle += angularVelocity;
			angularVelocity *= data.AngularFriction;

			MoveBy(velocity * deltaTime, CollisionFlags.NonStop, Global.TypeMap, Global.TypeEntity);
			Speed = velocity.Length();

			sprite.Play(Input.IsDown("a") ? "accel" + data.SpriteId : "idle" + data.SpriteId);
			sprite.Rotation = angle;
			sprite.SortOrder = Mathf.Floor(Bottom) * 10;
		}

		protected override bool OnHit(HitInfo info)
		{
			var stop = true;
			if (info.Other is RoomEntity)
			{
				stop = ((RoomEntity)info.Other).HandlePlayerHit(this, info.DeltaX, info.DeltaY);
			}

			if (stop)
			{
				if (info.IsVerticalMovement)
				{
					velocity.Y = -velocity.Y * BounceRestitution;
				}
				else if (info.IsHorizontalMovement)
				{
					velocity.X = -velocity.X * BounceRestitution;
				}

				Asset.LoadSoundEffect("sfx/hit").Play();
			}

			return stop;
		}

		[AssetLoader]
		public static void LoadAssets()
		{
			var anim = new AnimatorData("gfx/player", 16, 16);
			for (var i = 0; i < 10; i++)
			{
				anim.Add("idle" + i, i * 10);
				anim.Add("accel" + i, AnimatorMode.Loop, 0.1f, i * 10 + 1, i * 10 + 2);
			}
			Asset.AddAnimatorData("player", anim);
		}
	}
}
