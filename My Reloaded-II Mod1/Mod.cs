using FlatOut2_SwapGameMode.Configuration;
using FlatOut2_SwapGameMode.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Hooks.Definitions.X86;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;


namespace FlatOut2_SwapGameMode
{    
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
		private readonly ILogger _logger;

		/// <summary>
		/// Entry point into the mod, instance that created this class.
		/// </summary>
		private readonly IMod _owner;

		/// <summary>
		/// Provides access to this mod's configuration.
		/// </summary>
		private Config _configuration;

		/// <summary>
		/// The configuration of the currently executing mod.
		/// </summary>
		private readonly IModConfig _modConfig;

        // C# doesn't have macros?
        private void Print(string Message)
        {
            _logger.WriteLine(Message);
        }

        public struct sRGBA
        {
            public byte b, g, r, a;
        }

        public struct Vector3_pad
        {
            public float x, y, z, pad;
        }

        public struct Quaternion
        {
            public float y, z, x, w;
        }

        public struct Matrix33
        {
            public Vector3_pad right, up, at;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct sVehicle
        {
            [FieldOffset(0x78)]
            public byte* m_sFolderPath;

            [FieldOffset(0x8c)]
            public int m_nFolderPathLength;

            [FieldOffset(0x1b0)]
            public Matrix33 m_mMatrix;
            [FieldOffset(0x1e0)]
            public Vector3_pad m_vPosition;

            [FieldOffset(0x270)]
            public Quaternion m_qRotation;
            [FieldOffset(0x280)]
            public Vector3_pad m_vVelocity;
            [FieldOffset(0x290)]
            public Vector3_pad m_vRotationalVelocity;

            [FieldOffset(0x5cc)]
            public float m_fNitro;

            [FieldOffset(0x1c90)]
            public int m_nTireTextureID;
            [FieldOffset(0x1c94)]
            public int m_nTireModelID;

            [FieldOffset(0x1ca0)]
            public int m_nDriverHandsID;

            [FieldOffset(0x2940)]
            public void* m_pShadowTexture;

            [FieldOffset(0x2ab0)]
            public int m_nRagdollState;

            [FieldOffset(0x4594)]
            public void* m_pSkinDamagedTexture;
            [FieldOffset(0x4598)]
            public void* m_pLightsDamagedTexture;
            [FieldOffset(0x459c)]
            public void* m_pLightsGlowTexture;
            [FieldOffset(0x45a0)]
            public void* m_pLightsGlowLitTexture;

            [FieldOffset(0x463c)]
            public Player* m_pPlayer;

            [FieldOffset(0x6aa0)]
            public float m_fDamage;

            [FieldOffset(0x6ac8)]
            public int m_bGodModeMaybe;

            [FieldOffset(0x6ad8)]
            public int m_nWhat;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PlayerProfile
        {
            [FieldOffset(0xe31)]
            public bool m_bIsFemaleMaybe;

            [FieldOffset(0xe54)]
            public bool m_bDifficulty;

            [FieldOffset(0xe58)]
            public int m_nMoney;

            [FieldOffset(0xe60)]
            public bool m_bActiveEvent;

            [FieldOffset(0xef8)]
            public int m_nMoneySpentCars;
            [FieldOffset(0xefc)]
            public int m_nMoneySpentUpgrades;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Player
        {
            [FieldOffset(0x28)]
            public bool m_bInSession;
            [FieldOffset(0x29)]
            public bool m_bIsFriend;
            [FieldOffset(0x2a)]
            public bool m_bHasVoice;
            [FieldOffset(0x2b)]
            public bool m_bTalking;
            [FieldOffset(0x2c)]
            public bool m_bVoiceThroughSpeakers;
            [FieldOffset(0x2d)]
            public bool m_bMuted;

            [FieldOffset(0x30)]
            public bool m_bKicked;
            [FieldOffset(0x31)]
            public bool m_bTriedKickInThisRace;

            [FieldOffset(0x38)]
            public bool m_bPlaying;
            [FieldOffset(0x39)]
            public bool m_bJoinable;
            [FieldOffset(0x3a)]
            public bool m_bOnline;
            [FieldOffset(0x3b)]
            public bool m_bRecievedRequest;
            [FieldOffset(0x3c)]
            public bool m_bRecievedInvite;

            [FieldOffset(0x48)]
            public Player* m_pPlayer;

            [FieldOffset(0x2d0)]
            public sRGBA m_nBlipColor;
            [FieldOffset(0x2d4)]
            public int m_nBlipIndex;

            [FieldOffset(0x33c)]
            public sVehicle* m_pVehicle;
            [FieldOffset(0x340)]
            public int m_nCarID;
            [FieldOffset(0x344)]
            public int m_nSkin;

            [FieldOffset(0x34c)]
            public int m_bDriverFemale;
            [FieldOffset(0x350)]
            public int m_nDriverSkin;

            [FieldOffset(0x368)]
            public int m_nPlayerId;
            [FieldOffset(0x36c)]
            public int m_nFlags;

            [FieldOffset(0x380)]
            public int m_bDisableControlAndReset;

            [FieldOffset(0x428)]
            public Vector3_pad m_vPosition;

            [FieldOffset(0x49c)]
            public float m_fReadOnlyDamage;

            [FieldOffset(0x684)]
            public float m_fSteerAngle;
            [FieldOffset(0x688)]
            public float m_fGasPedal;
            [FieldOffset(0x68c)]
            public float m_fBrakePedal;
        }

        // I don't know if I need CharSet.Ansi on all of these, but just in case
        [Function([Register.eax], Register.eax, StackCleanup.Callee)]
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public delegate void RenderSkyPtr(int in_EAX, void* pEnvironment, int param_2);

        [Function([Register.ebx, Register.esi], Register.eax, StackCleanup.Callee)]
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int CheckForCheatCodesPtr(byte* unaff_EBX, PlayerProfile* profile_ESI);

        [Function([Register.esi], Register.eax, StackCleanup.Callee)]
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void* CreatePlayerPtr(Player* player_ESI, int playerID);

        [Function([Register.ecx, Register.eax], Register.eax, StackCleanup.Callee)]
        // This function is a __fastcall, I called it anyway by using StdCall,
        // I don't know if that means the parameters would be backwards, but it doesn't pass any though the stack so it worked out.
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void FixCarPtr(sVehicle* car_ECX, int in_EAX);

        public FixCarPtr fixCar;

        public IHook<RenderSkyPtr> skyHook;
        IHook<CheckForCheatCodesPtr> checkCheatCodeHook;
        IHook<CreatePlayerPtr> createPlayerHook;

        // There's probably a built-in way to do this isn't there
        private string CheatCodeToString(char* str)
        {
            string newString = "";
            while (*str != '\0')
            {
                newString += *str;
                str += 1;
            }
            return newString;
        }

        [Function([Register.ebx, Register.esi], Register.eax, StackCleanup.Callee)]
        public int CheckCheatCode(byte* code_EBX, PlayerProfile* profile_ESI)
        {
            string code = CheatCodeToString((char*)code_EBX);

            // Maybe one of these days I can spin this off into an 'Add your own cheat codes' mod
            switch (code)
            {
                case "ZACK":
                    profile_ESI->m_nMoney = 27272727;
                    return 1;

                default:
                    break;
            }

            return checkCheatCodeHook.OriginalFunction(code_EBX, profile_ESI);
        }

        // Can't make a list of pointers for some reason
        List<nint> allPlayers = new List<nint>();

        [Function([Register.esi], Register.eax, StackCleanup.Callee)]
        public void* CreatePlayer(Player* player, int playerID)
        {
            if (playerID == 1)
            {
                timer = 0.0f;
                allPlayers.Clear();
                didThat = false;
            }

            allPlayers.Add((nint)player);

            return createPlayerHook.OriginalFunction(player, playerID);
        }

        public float timer = 0.0f;
        private bool didThat = false;

        public void SwitchPlayers(Player* player1, Player* player2)
        {
            // Swap the damages so that your health bar stays with you
            (player2->m_pVehicle->m_fDamage, player1->m_pVehicle->m_fDamage) = (player1->m_pVehicle->m_fDamage, player2->m_pVehicle->m_fDamage);

            // Swaps the positions, rotations, and velocities so that you stay in the same place after the switch, if the setting is enabled
            if (_configuration.SwitchPositions)
                (player2->m_pVehicle->m_vPosition, player2->m_pVehicle->m_vVelocity, player2->m_pVehicle->m_vRotationalVelocity, player2->m_pVehicle->m_qRotation, player1->m_pVehicle->m_vPosition, player1->m_pVehicle->m_vVelocity, player1->m_pVehicle->m_vRotationalVelocity, player1->m_pVehicle->m_qRotation) = (player1->m_pVehicle->m_vPosition, player1->m_pVehicle->m_vVelocity, player1->m_pVehicle->m_vRotationalVelocity, player1->m_pVehicle->m_qRotation, player2->m_pVehicle->m_vPosition, player2->m_pVehicle->m_vVelocity, player2->m_pVehicle->m_vRotationalVelocity, player2->m_pVehicle->m_qRotation);

            // The vehicle needs to be updated with the new player so the portraits update
            player1->m_pVehicle->m_pPlayer = player2;
            player2->m_pVehicle->m_pPlayer = player1;

            // Tuple method isn't working for some reason
            sVehicle* car = player1->m_pVehicle;
            player1->m_pVehicle = player2->m_pVehicle;
            player2->m_pVehicle = car;

            // If you are alive and just switched with a player that's wrecked, it will fix the car so you don't have to drive a completely destroyed one
            if (player1->m_fReadOnlyDamage != 1.0f && player2->m_fReadOnlyDamage == 1.0f)
                fixCar(player1->m_pVehicle, 0);
        }

        public void PerFrame()
        {
            // At 60 FPS, this should be an accurate timer
            timer += 0.01666666f;

            if (allPlayers.Count > 1)
            {
                if (_configuration.SwitchGameMode)
                {
                    if (timer > _configuration.SwitchTimer)
                    {
                        var rand = new Random();

                        foreach (Player* player in allPlayers)
                        {
                            int index = rand.Next() % allPlayers.Count;

                            // Makes sure you always switch to a different character
                            while (((Player*)allPlayers[index])->m_nPlayerId == player->m_nPlayerId)
                                index = rand.Next() % allPlayers.Count;

                            Player* newPlayer = (Player*)allPlayers[index];

                            SwitchPlayers(player, newPlayer);
                        }

                        timer = 0.0f;
                    }
                }
                else if (!didThat && (_configuration.Character != Config.CharacterEnum.NoChange))
                {
                    SwitchPlayers((Player*)allPlayers[0], (Player*)allPlayers[(int)_configuration.Character]);
                    didThat = true;
                }
            }
        }

        // I tried hooking the RenderRace function but it crashes when you exit, so I settled for hooking RenderSky
        [Function([Register.eax], Register.eax, StackCleanup.Callee)]
        public void PerFrame_Sky(int in_EAX, void* pEnvironment, int param_2)
		{
            PerFrame();
            skyHook.OriginalFunction.Invoke(in_EAX, pEnvironment, param_2);
		}

		private IHook<T> NewHook<T>(T function, long address)
		{
			return _hooks!.CreateHook<T>(function, address).Activate();
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

            fixCar = _hooks.CreateWrapper<FixCarPtr>(0x00427620, out var addr);

            skyHook = NewHook<RenderSkyPtr>(PerFrame_Sky, 0x00592470);
            checkCheatCodeHook = NewHook<CheckForCheatCodesPtr>(CheckCheatCode, 0x00476570);
            createPlayerHook = NewHook<CreatePlayerPtr>(CreatePlayer, 0x0046A400);
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