//#define ONLINE_FRIENDLY

using FlatOut2_SwapGameMode.Configuration;
using FlatOut2_SwapGameMode.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using System.Runtime.InteropServices;
using System.Numerics;
using Reloaded.Hooks.Definitions.X86;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;
using System.Diagnostics;


namespace FlatOut2_SwapGameMode
{
    internal struct sRGBA
    {
        public byte b, g, r, a;
    }

    internal struct Matrix33
    {
        public Vector4 Right, Up, At;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct Car
    {
        [FieldOffset(0x78)]
        public byte* FolderPath;

        [FieldOffset(0x8c)]
        public int FolderPathLength;

        [FieldOffset(0x1b0)]
        public Matrix33 Matrix;
        [FieldOffset(0x1e0)]
        public Vector4 Position;

        [FieldOffset(0x270)]
        public Quaternion Rotation;
        [FieldOffset(0x280)]
        public Vector4 Velocity;
        [FieldOffset(0x290)]
        public Vector4 RotationalVelocity;

        [FieldOffset(0x5cc)]
        public float Nitro;

        [FieldOffset(0x1c90)]
        public int TireTextureID;
        [FieldOffset(0x1c94)]
        public int TireModelID;

        [FieldOffset(0x1ca0)]
        public int DriverHandsID;

        [FieldOffset(0x2940)]
        public void* ShadowTexture;

        [FieldOffset(0x2ab0)]
        public int RagdollState;

        [FieldOffset(0x4594)]
        public void* SkinDamagedTexture;
        [FieldOffset(0x4598)]
        public void* LightsDamagedTexture;
        [FieldOffset(0x459c)]
        public void* LightsGlowTexture;
        [FieldOffset(0x45a0)]
        public void* LightsGlowLitTexture;

        [FieldOffset(0x463c)]
        public Player* Player;

        [FieldOffset(0x6aa0)]
        public float Damage;

        [FieldOffset(0x6ac8)]
        public int GodModeMaybe;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PlayerProfile
    {
        [FieldOffset(0xe31)]
        public bool IsFemaleMaybe;

        [FieldOffset(0xe54)]
        public byte Difficulty;

        [FieldOffset(0xe58)]
        public int Money;

        [FieldOffset(0xe60)]
        public bool ActiveEvent;

        [FieldOffset(0xef8)]
        public int MoneySpentCars;
        [FieldOffset(0xefc)]
        public int MoneySpentUpgrades;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct Player
    {
        [FieldOffset(0x28)]
        public bool InSession;
        [FieldOffset(0x29)]
        public bool IsFriend;
        [FieldOffset(0x2a)]
        public bool HasVoice;
        [FieldOffset(0x2b)]
        public bool Talking;
        [FieldOffset(0x2c)]
        public bool VoiceThroughSpeakers;
        [FieldOffset(0x2d)]
        public bool Muted;

        [FieldOffset(0x30)]
        public bool Kicked;
        [FieldOffset(0x31)]
        public bool TriedKickInThisRace;

        [FieldOffset(0x38)]
        public bool Playing;
        [FieldOffset(0x39)]
        public bool Joinable;
        [FieldOffset(0x3a)]
        public bool Online;
        [FieldOffset(0x3b)]
        public bool RecievedRequest;
        [FieldOffset(0x3c)]
        public bool RecievedInvite;

        [FieldOffset(0x2d0)]
        public sRGBA BlipColor;
        [FieldOffset(0x2d4)]
        public int BlipIndex;

        [FieldOffset(0x33c)]
        public Car* Car;
        [FieldOffset(0x340)]
        public int CarID;
        [FieldOffset(0x344)]
        public int Skin;

        [FieldOffset(0x34c)]
        public int DriverFemale;
        [FieldOffset(0x350)]
        public int DriverSkin;

        [FieldOffset(0x368)]
        public int PlayerId;
        [FieldOffset(0x36c)]
        public int Flags;

        [FieldOffset(0x380)]
        public int DisableControlAndReset;

        [FieldOffset(0x428)]
        public readonly Vector3 ReadOnlyPosition;

        [FieldOffset(0x49c)]
        public readonly float ReadOnlyDamage;

        [FieldOffset(0x684)]
        public float SteerAngle;
        [FieldOffset(0x688)]
        public float GasPedal;
        [FieldOffset(0x68c)]
        public float BrakePedal;
    }

    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public unsafe class Mod : ModBase // <= Do not Remove.
	{
		/// <summary>
		/// Provides access to the mod loader API.
		/// </summary>
		private readonly IModLoader _modLoader;

		/// <summary>
		/// Provides access to the Reloaded.Hooks API.
		/// </summary>
		/// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
		private readonly Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks? _hooks;

		/// <summary>
		/// Provides access to the Reloaded logger.
		/// </summary>
		private static ILogger _logger;

		/// <summary>
		/// Entry point into the mod, instance that created this class.
		/// </summary>
		private readonly IMod _owner;

		/// <summary>
		/// Provides access to this mod's configuration.
		/// </summary>
		private static Config _configuration;

		/// <summary>
		/// The configuration of the currently executing mod.
		/// </summary>
		private readonly IModConfig _modConfig;

		// C# doesn't have macros?
		private void Print(string Message)
		{
			_logger.WriteLine(Message);
		}

        private IHook<T> NewHook<T>(T function, long address)
        {
            return _hooks!.CreateHook(function, address).Activate();
        }

		[Function([Register.eax], Register.eax, StackCleanup.Callee)]
		private delegate void RenderSkyPtr(int in_EAX, void* pEnvironment, int param_2);

		[Function([Register.ebx, Register.esi], Register.eax, StackCleanup.Callee)]
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		private delegate int CheckForCheatCodesPtr(string unaff_EBX, PlayerProfile* profile_ESI);

		[Function([Register.esi], Register.eax, StackCleanup.Callee)]
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void* CreatePlayerPtr(Player* player_ESI, int playerID);

		[Function([Register.ecx, Register.eax], Register.eax, StackCleanup.Callee)]
		// This function is a __fastcall, I called it anyway by using StdCall,
		// I don't know if that means the parameters would be backwards, but it doesn't pass any though the stack so it worked out.
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void FixCarPtr(Car* car_ECX, int in_EAX);

		private static FixCarPtr FixCar;

		private static IHook<RenderSkyPtr> SkyHook;
		private static IHook<CheckForCheatCodesPtr> CheckCheatCodeHook;
		private static IHook<CreatePlayerPtr> CreatePlayerHook;

		[Function([Register.ebx, Register.esi], Register.eax, StackCleanup.Callee)]
		private int CheckCheatCode(string code, PlayerProfile* profile_ESI)
		{
			switch (code)
			{
				case "ZACK":
					profile_ESI->Money = 27272727;
					return 1;

				default:
					break;
			}

			return CheckCheatCodeHook.OriginalFunction(code, profile_ESI);
		}

		private static readonly Stopwatch Stopwatch = new();

		// Can't make a list of pointers for some reason
		private static readonly List<nint> allPlayers = [];

		[Function([Register.esi], Register.eax, StackCleanup.Callee)]
		private static void* CreatePlayer(Player* player, int playerID)
		{
			if (playerID == 1)
			{
#if ONLINE_FRIENDLY
				RandomIndex = 0;
#endif
				Stopwatch.Restart();
				allPlayers.Clear();
				DidThat = false;
			}

			allPlayers.Add((nint)player);

			return CreatePlayerHook.OriginalFunction(player, playerID);
		}

		private static bool DidThat = false;

		private static void SwitchPlayers(Player* player1, Player* player2)
		{
			// Swap the damages so that your health bar stays with you
			(player2->Car->Damage, player1->Car->Damage) = (player1->Car->Damage, player2->Car->Damage);

			// Swaps the positions, rotations, and velocities so that you stay in the same place after the switch, if the setting is enabled
			if (_configuration.SwitchPositions)
				(player2->Car->Position, player2->Car->Velocity, player2->Car->RotationalVelocity, player2->Car->Rotation, player1->Car->Position, player1->Car->Velocity, player1->Car->RotationalVelocity, player1->Car->Rotation) = (player1->Car->Position, player1->Car->Velocity, player1->Car->RotationalVelocity, player1->Car->Rotation, player2->Car->Position, player2->Car->Velocity, player2->Car->RotationalVelocity, player2->Car->Rotation);

			// The vehicle needs to be updated with the new player so the portraits update
			player1->Car->Player = player2;
			player2->Car->Player = player1;

			// Tuple method isn't working for some reason
			Car* car = player1->Car;
			player1->Car = player2->Car;
			player2->Car = car;

			// If you are alive and just switched with a player that's wrecked, it will fix the car so you don't have to drive a completely destroyed one
			if (player1->ReadOnlyDamage != 1.0f && player2->ReadOnlyDamage == 1.0f)
				FixCar(player1->Car, 0);
		}

#if ONLINE_FRIENDLY
		private static readonly float[] RandomValues = [
			0.1776525471045881f,
			0.3153066083669439f,
			0.43552905632805283f,
			0.09463552370027783f,
			0.31090605197491794f,
			0.7266115141484512f,
			0.4592081408177887f,
			0.646303598532883f,
			0.5612417187522251f,
			0.2897032296271139f,
			0.0758337481208381f,
			0.8518903525286752f,
			0.5224473074796578f,
			0.9663208000419411f,
			0.7532929249015514f,
			0.6105510783822887f,
			0.9103865515446021f,
			0.28705171900014204f,
			0.9780693064756909f,
			0.7365713331586285f,
			0.49410793901546646f,
			0.6390243824607588f,
			0.5587075042969541f,
			0.03618016609339558f,
			0.9664734719470707f,
			0.1823180698924166f,
			0.7997184618280874f,
			0.3547396472490678f,
			0.5631130827400757f,
			0.04338990323107683f,
			0.11626453233184264f,
			0.4545779171861388f,
			0.7307860682117456f,
			0.1560272885812486f,
			0.48091547204163065f,
			0.5035003070415869f,
			0.9832858826621714f,
			0.8942541304555488f,
			0.5068861521066222f,
			0.05329381237913522f,
			0.7625279201306338f,
			0.2876848144473363f,
			0.9163296146003213f,
			0.007285154309466857f,
			0.11027426913201188f,
			0.2060756271617521f,
			0.018407887768394415f,
			0.061516269327666295f,
			0.5139365368528328f,
			0.29195487426935796f,
			0.5274945747413043f,
			0.5243702680088308f,
			0.4719271169780822f,
			0.8133277914910141f,
			0.045972517952653f,
			0.6097608231756627f,
			0.3173235799458465f,
			0.734580111217381f,
			0.9880080312333069f,
			0.5716807739706858f,
			0.30151825009874145f,
			0.2930299367534588f,
			0.10762207791345879f,
			0.057835478611346525f,
			0.028668427576809985f,
			0.5040373125531936f,
			0.8746206528800587f,
			0.40598915639820876f,
			0.5854127662271204f,
			0.3118178907157384f,
			0.5388485796440612f,
			0.30607653694110515f,
			0.010114353569914414f,
			0.010018430687800506f,
			0.29904025973445447f,
			0.8342304084133979f,
			0.773798180979318f,
			0.9128073356489448f,
			0.7195816411126711f,
			0.7806979787335667f,
			0.568712727712032f,
			0.4800737393103154f,
			0.7118150460764361f,
			0.7090239343986664f,
			0.3041204587369928f,
			0.4969783950511959f,
			0.4577134604175289f,
			0.8893199595689466f,
			0.17940366456697487f,
			0.17748211729902907f,
			0.7606613745334354f,
			0.14383673141382003f,
			0.06149055755221078f,
			0.3694523858535155f,
			0.7210927923573467f,
			0.27865194918275094f,
			0.6276160496109573f,
			0.05637394654502792f,
			0.6028760747101863f,
			0.6454074557597768f,
			0.6050492300236754f,
			0.4308483500599215f,
			0.216334130325887f,
			0.8778367504039146f,
			0.8748111482420596f,
			0.994225472467628f,
			0.9293473208886887f,
			0.17918195359090172f,
			0.37652443574533845f,
			0.7223980291033907f,
			0.22646842524776933f,
			0.6290412273396748f,
			0.45368851661358855f,
			0.9393693126585722f,
			0.45879219941284644f,
			0.1702028712913659f,
			0.2908207293020626f,
			0.33673613892162657f,
			0.8266398100626194f,
			0.9259660038350621f,
			0.1314908462796539f,
			0.7088575734198452f,
			0.6395741295605567f,
			0.4112995848210482f,
			0.5197770438765322f,
			0.3097181839320502f,
			0.19584357498632998f,
			0.1104907287175495f,
			0.9090937708066684f,
			0.1854788473095923f,
			0.9983360064723863f,
			0.7771270195300846f,
			0.8486835347026884f,
			0.7431042576009f,
			0.22131177482272424f,
			0.028029481655277633f,
			0.13496235613699636f,
			0.6123066927905049f,
			0.2782355942450766f,
			0.901508118587957f,
			0.7002370751550467f,
			0.8051845551282405f,
			0.890148612588274f,
			0.5438866759022788f,
			0.39053922877633906f,
			0.48534761296103657f,
			0.009317139511185735f,
			0.6632135092980334f,
			0.5689926165945677f,
			0.7696218562196289f,
			0.5751558033858226f,
			0.9202789842597588f,
			0.21590320379287076f,
			0.30892314421551315f,
			0.43446884589909873f,
			0.12332302483622992f,
			0.26423384202960964f,
			0.24469333314007924f,
			0.6744444715616921f,
			0.8778865897877588f,
			0.4630963580482277f,
			0.6342751116317712f,
			0.06891706926922914f,
			0.2505832824687859f,
			0.07366195296813638f,
			0.6229357866267777f,
			0.712552192037561f,
			0.9385539617528683f,
			0.33796816206911706f,
			0.9499853837741179f,
			0.08890594679990282f,
			0.49847671438969254f,
			0.22769646801783827f,
			0.24609936299421897f,
			0.18844653127984023f,
			0.03193087048919352f,
			0.7453403347061918f,
			0.3020190175962233f,
			0.2668566736965209f,
			0.8483876854906476f,
			0.5232080018783934f,
			0.9843211688009166f,
			0.5483067701663005f,
			0.47904328420948616f,
			0.24783435894216643f,
			0.23896356762669346f,
			0.9890347966146097f,
			0.6262350462285486f,
			0.6986671956431266f,
			0.9094002334643007f,
			0.1950892542124678f,
			0.903584677361702f,
			0.7774547857203858f,
			0.81426295577094f,
			0.3420343916844302f,
			0.019136832773957835f,
			0.10177804568449822f,
			0.3268559929795575f,
			0.6609236505228948f,
			0.31666898534663535f,
			0.12365922693125231f,
			0.021449281152456368f,
			0.38605926786911227f,
			0.3764885327126025f,
			0.49161965740878766f,
			0.7960414720511891f,
			0.9460640487720453f,
			0.3034973013041622f,
			0.498587874438992f,
			0.6695325389764087f,
			0.23732234475248626f,
			0.8029651080553365f,
			0.623145936134964f,
			0.3968480567437712f,
			0.4232592838901149f,
			0.35329446465473846f,
			0.3277319187224035f,
			0.7082255034028493f,
			0.6447115931583282f,
			0.4714310012982572f,
			0.752806040293855f,
			0.419555810665311f,
			0.30961000754145374f,
			0.6097460864453442f,
			0.04726032512017442f,
			0.5492920981963031f,
			0.8671639346725873f,
			0.9647449264258295f,
			0.39830933394118495f,
			0.3317478602732208f,
			0.3138172133184408f,
			0.978275377998956f,
			0.15357741754622134f,
			0.5266220385219623f,
			0.6083915192132393f,
			0.7736106033233361f,
			0.04392736735631586f,
			0.13305456861485787f,
			0.6702967671621033f,
			0.5279543335398812f,
			0.6181610944829561f,
			0.8113908503984515f,
			0.5344847076340019f,
			0.49311320042062756f,
			0.5881241617189091f,
			0.004170403012995205f,
			0.2614364452876953f,
			0.45041335876670574f,
			0.42000250027729324f,
			0.5652086868994997f,
			0.5870873654650912f,
			0.8741399562350589f,
			0.10054174551668149f,
			0.8321169433735031f,
			0.8418019176828495f,
			0.5f
		];

		private static byte RandomIndex;

		private static float RandomFloat()
		{
			return RandomValues[RandomIndex++];
		}

		private static int RandomInt(int max)
		{
			return (int)Math.Round(RandomFloat() * max);
		}
#else
		private static Random rand = new();

		private static int RandomInt(int max)
		{
			return rand.Next() % (max + 1);
		}
#endif

		private static void PerFrame()
		{
            if (allPlayers.Count < 2)
                return;

			if (_configuration.SwitchGameMode)
			{
				if (Stopwatch.ElapsedMilliseconds * 0.001 > _configuration.SwitchTimer)
				{
					foreach (Player* player in allPlayers)
					{
						int index = RandomInt(allPlayers.Count - 1);

#if !ONLINE_FRIENDLY
						// Makes sure you always switch to a different character (disabled to make it work online)
						while (((Player*)allPlayers[index])->PlayerId == player->PlayerId)
							index = RandomInt(allPlayers.Count);
#endif

						Player* newPlayer = (Player*)allPlayers[index];

						SwitchPlayers(player, newPlayer);
					}

					Stopwatch.Restart();
				}
			}
			else if (!DidThat)
			{
				if (_configuration.Character != Config.CharacterEnum.NoChange)
					SwitchPlayers((Player*)allPlayers[0], (Player*)allPlayers[(int)_configuration.Character]);

				DidThat = true;
			}
		}

		// I tried hooking the RenderRace function but it crashes when you exit, so I settled for hooking RenderSky
		[Function([Register.eax], Register.eax, StackCleanup.Callee)]
		private void PerFrame_Sky(int in_EAX, void* pEnvironment, int param_2)
		{
			PerFrame();
			SkyHook.OriginalFunction(in_EAX, pEnvironment, param_2);
		}

		public Mod(ModContext context)
		{
			_modLoader = context.ModLoader;
			_hooks = context.Hooks;
			_logger = context.Logger;
			_owner = context.Owner;
			_configuration = context.Configuration;
			_modConfig = context.ModConfig;

			// For more information about this template, please see
			// https://reloaded-project.github.io/Reloaded-II/ModTemplate/

			// If you want to implement e.g. unload support in your mod,
			// and some other neat features, override the methods in ModBase.

			if (_hooks == null)
			{
				Print("Hooks not available!");
				return;
			}

			FixCar = _hooks.CreateWrapper<FixCarPtr>(0x00427620, out var addr);

			SkyHook = NewHook<RenderSkyPtr>(PerFrame_Sky, 0x00592470);
			CheckCheatCodeHook = NewHook<CheckForCheatCodesPtr>(CheckCheatCode, 0x00476570);
			CreatePlayerHook = NewHook<CreatePlayerPtr>(CreatePlayer, 0x0046A400);
		}

		#region Standard Overrides
		public override void ConfigurationUpdated(Config configuration)
		{
			// Apply settings from configuration.
			// ... your code here.
			_configuration = configuration;
			_logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
		}
		#endregion

		#region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public Mod() { }
#pragma warning restore CS8618
		#endregion
	}
}