using System;
using System.Security;
using System.Security.Permissions;
using RWCustom;
using BepInEx;
using MoreSlugcatsEnums = MoreSlugcats.MoreSlugcatsEnums;
using System.Text.RegularExpressions;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.IO;
using Random = UnityEngine.Random;
using RainMeadowCompat;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace SwordMod;

[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
public partial class SwordMod : BaseUnityPlugin
{
    public const string MOD_ID = "LazyCowboy.SwordMod";
    public const string MOD_NAME = "Sword Mod";
    public const string MOD_VERSION = "1.0.6";

    public static SwordModOptions Options;

    public static bool CustomHitSoftSound = false, CustomHitHardSound = false, CustomHitWallSound = false, CustomParrySound = false, CustomSwingSound = false;
    public static string HitSoftSoundId = "Sword_Hit_Soft", HitHardSoundId = "Sword_Hit_Hard", HitWallSoundId = "Sword_Hit_Wall", ParrySoundId = "Sword_Parry", SwingSoundId = "Sword_Swing";

    public SwordMod()
    {
        try
        {
            Options = new SwordModOptions(this, Logger);
            SafeMeadowInterface.InitializeMeadowCompatibility(Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private void OnDisable()
    {
        On.RainWorld.OnModsInit -= RainWorldOnOnModsInit;

        if (IsInit)
        {
            On.AbstractPhysicalObject.Realize -= Sword_Object_Realizer;
            On.Player.Grabability -= Sword_Grabability;
            On.Player.ctor -= Spawn_Sword_On_Player_Spawn;
            On.Room.LoadFromDataString -= Add_Arena_Swords;
            On.Oracle.ctor -= Iterator_Sword_Spawner;
            On.Oracle.Update -= Hide_Moon_In_Wall;
            On.ScavengerAI.CollectScore_PhysicalObject_bool -= Scav_General_Sword_Value;
            On.ScavengerAI.WeaponScore -= Scav_Sword_Weapon_Score;
            On.Scavenger.FlyingWeapon -= Scav_Parry_Sword;
            On.ScavengerAbstractAI.InitGearUp -= Scav_Spawn_Sword;

            if (MoonDeadHook != null)
                MoonDeadHook.Undo();

            SafeMeadowInterface.RemoveHooks();

            IsInit = false;
        }
    }

    BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
    BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;
    private Hook MoonDeadHook = null;

    private bool IsInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsInit) return;

            //Your hooks go here

            //cleanup hooks
            //On.RainWorldGame.ShutDownProcess += RainWorldGameOnShutDownProcess;
            //On.GameSession.ctor += GameSessionOnctor;

            //On.AbstractPhysicalObject.ctor += Sword_Object_Realizer;
            On.AbstractPhysicalObject.Realize += Sword_Object_Realizer;

            On.Player.Grabability += Sword_Grabability;

            On.Player.ctor += Spawn_Sword_On_Player_Spawn;

            On.Room.LoadFromDataString += Add_Arena_Swords;

            On.Oracle.ctor += Iterator_Sword_Spawner;

            On.Oracle.Update += Hide_Moon_In_Wall;

            On.ScavengerAI.CollectScore_PhysicalObject_bool += Scav_General_Sword_Value;
            On.ScavengerAI.WeaponScore += Scav_Sword_Weapon_Score;
            On.Scavenger.FlyingWeapon += Scav_Parry_Sword;

            On.ScavengerAbstractAI.InitGearUp += Scav_Spawn_Sword;

            try
            {
                MoonDeadHook = new Hook(
                    typeof(Oracle).GetProperty("Consious", propFlags).GetGetMethod(),
                    typeof(SwordMod).GetMethod("Make_Moon_Dead", myMethodFlags)
                );
            } catch (Exception ex)
            {
                Logger.LogError(ex);
            }


            //load custom images//atlases
            try
            {
                //Futile.atlasManager.LoadAtlas(AssetManager.ResolveDirectory("assets\\KarmaExpansion") + "\\ExtraKarmaSymbols");
                //SwordAssetPath = AssetManager.ResolveDirectory("assets" + Path.DirectorySeparatorChar + "SwordMod") + Path.DirectorySeparatorChar + "SwordMod_Sword";
                //Futile.atlasManager.LoadImage(SwordAssetPath);
                Futile.atlasManager.LoadAtlas(Regex.Replace(AssetManager.ResolveFilePath("assets" + Path.DirectorySeparatorChar + "SwordMod" + Path.DirectorySeparatorChar + "SwordMod_Sword.png"), ".png", ""));

                //test for custom sounds
                CustomHitSoftSound = File.Exists(AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + HitSoftSoundId + ".wav"));
                CustomHitHardSound = File.Exists(AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + HitHardSoundId + ".wav"));
                CustomHitWallSound = File.Exists(AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + HitWallSoundId + ".wav"));
                CustomParrySound = File.Exists(AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + ParrySoundId + ".wav"));
                CustomSwingSound = File.Exists(AssetManager.ResolveFilePath("loadedsoundeffects" + Path.DirectorySeparatorChar + SwingSoundId + ".wav"));

                //string totalList = "";
                //foreach (var thing in Futile.atlasManager._allElementsByName)
                //    totalList += thing.Key + ", ";
                //Logger.LogInfo(totalList);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }


            //sandbox hooks
            On.MultiplayerUnlocks.SandboxItemUnlocked += Sandbox_Item_Unlocked;
            On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += Sandbox_Symbol_Data;
            On.ItemSymbol.SpriteNameForItem += Sandbox_Sprite_Name;

            try
            {
                Sword.SwordType = new AbstractPhysicalObject.AbstractObjectType("Sword", true); //I really wish I had put "SwordMod_Sword"
                Sword.SandboxID = new MultiplayerUnlocks.SandboxUnlockID("SwordMod_Sword", true);
                MultiplayerUnlocks.ItemUnlockList.Add(Sword.SandboxID);

                //Logger.LogInfo("Sandbox Symbol Size: " + Futile.atlasManager.GetElementWithName("Symbol_FireSpear").sourceSize.ToString());
            } catch (Exception ex)
            {
                Logger.LogError(ex);
            }


            //register expedition perk
            try
            {
                Modding.Expedition.CustomPerks.Register(new Sword_Perk());
            } catch (Exception ex)
            {
                Logger.LogError(ex);
            }


            MachineConnector.SetRegisteredOI(MOD_ID, Options);
            IsInit = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    public Player.ObjectGrabability Sword_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (!Options.AllowDualWielding.Value && obj is Sword)
            return Player.ObjectGrabability.BigOneHand;
        return orig(self, obj);
    }

    #region Spawning_Hooks
    public void Hide_Moon_In_Wall(On.Oracle.orig_Update orig, Oracle self, bool eu)
    {
        orig(self, eu);

        if (Options.RemoveMoon.Value && self.ID == Oracle.OracleID.SL)
            self.firstChunk.pos.x += 100f;
    }

    public delegate bool orig_Get_Oracle_Consious(Oracle self);
    public static bool Make_Moon_Dead(orig_Get_Oracle_Consious orig, Oracle self)
    {
        if (Options.RemoveMoon.Value && self.ID == Oracle.OracleID.SL)
            return false;
        return orig(self);
    }

    public void Iterator_Sword_Spawner(On.Oracle.orig_ctor orig, Oracle self, AbstractPhysicalObject abstractPhysicalObject, Room room)
    {
        orig(self, abstractPhysicalObject, room);

        if (((self.ID == Oracle.OracleID.SL || self.ID == MoreSlugcatsEnums.OracleID.DM) && Options.SpawnAtMoon.Value) || ((self.ID == Oracle.OracleID.SS || self.ID == MoreSlugcatsEnums.OracleID.CL) && Options.SpawnAt5P.Value))
        {
            try
            {
                AbstractPhysicalObject obj = new AbstractPhysicalObject(room.world, Sword.SwordType, null, self.room.GetWorldCoordinate(self.oracleBehavior.OracleGetToPos), room.world.game.GetNewID());
                room.abstractRoom.AddEntity(obj);
                obj.RealizeInRoom();
                Logger.LogInfo("Spawned sword at Iterator; " + self.oracleBehavior.OracleGetToPos.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        if (Options.RemoveMoon.Value)
            self.firstChunk.pos.x += 200f;
    }

    public void Add_Arena_Swords(On.Room.orig_LoadFromDataString orig, Room self, string[] lines)
    {
        bool flag = self.abstractRoom.firstTimeRealized;

        orig(self, lines);

        if (flag && self.world != null && self.game != null
            && self.game.IsArenaSession && self.game.GetArenaGameSession.GameTypeSetup.levelItems
            && (!SafeMeadowInterface.IsOnline() || SafeMeadowInterface.IsHost()))
        {
            for (int i = 0; i < Options.RandomArenaSwords.Value; i++)
                self.abstractRoom.AddEntity(new AbstractPhysicalObject(self.world, Sword.SwordType, null, new WorldCoordinate(self.abstractRoom.index, Random.RandomRange(5, 60), Random.RandomRange(20, 35), -1), self.game.GetNewID(-self.abstractRoom.index)));
        }
    }

    //public void Sword_Object_Realizer(On.AbstractPhysicalObject.orig_ctor orig, AbstractPhysicalObject self, World world, AbstractPhysicalObject.AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID)
    public void Sword_Object_Realizer(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);

        if (self.type == Sword.SwordType)
        {
            self.realizedObject = new Sword(self, self.world);
            Logger.LogDebug("Sword ExtEnum index: " + (int) Sword.SwordType);
        }
    }

    public void Spawn_Sword_On_Player_Spawn(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (!Options.SpawnEveryCycle.Value && !(ModManager.Expedition && Custom.rainWorld.ExpeditionMode && Expedition.ExpeditionGame.activeUnlocks.Contains(Sword_Perk.PERK_ID)))
            return;

        //don't spawn swords for other players; just spawn them for myself
        if (SafeMeadowInterface.IsOnline() && !SafeMeadowInterface.IsMine(abstractCreature))
            return;

        try
        {
            AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(world, Sword.SwordType, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), world.game.GetNewID());
            abstractCreature.Room.AddEntity(abstractPhysicalObject);
            abstractPhysicalObject.RealizeInRoom();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    public void Scav_Spawn_Sword(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
    {
        orig(self);

        float mod = (self.parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite) ? 4f : 1f;
        if (Random.value < Options.ScavSpawnChance.Value * mod)
        {
            AbstractPhysicalObject abstractSword = new AbstractPhysicalObject(self.world, Sword.SwordType, null, self.parent.pos, self.world.game.GetNewID());//new AbstractSpear(self.world, null, self.parent.pos, self.world.game.GetNewID(), self.IsSpearExplosive(num));
            self.world.GetAbstractRoom(self.parent.pos).AddEntity(abstractSword);
            new AbstractPhysicalObject.CreatureGripStick(self.parent, abstractSword, 0, true);
        }
    }
    #endregion

    #region Sandbox_Hooks
    public bool Sandbox_Item_Unlocked(On.MultiplayerUnlocks.orig_SandboxItemUnlocked orig, MultiplayerUnlocks self, MultiplayerUnlocks.SandboxUnlockID unlockID)
    {
        if (unlockID == Sword.SandboxID)
            return true;
        return orig(self, unlockID);
    }
    public IconSymbol.IconSymbolData Sandbox_Symbol_Data(On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
    {
        if (unlockID == Sword.SandboxID)
            return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, Sword.SwordType, 0);
        return orig(unlockID);
    }
    public string Sandbox_Sprite_Name(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        if (itemType == Sword.SwordType)
            return "Symbol_SwordMod_Sword";
        return orig(itemType, intData);
    }
    #endregion

    #region Scav_Hooks
    public int Scav_General_Sword_Value(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        if (obj is Sword)
            return 5;
        return orig(self, obj, weaponFiltered);
    }
    public int Scav_Sword_Weapon_Score(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection)
    {
        if (obj is not Sword)
            return orig(self, obj, pickupDropInsteadOfWeaponSelection);
        if (self.scavenger == null || self.focusCreature == null)
            return orig(self, obj, pickupDropInsteadOfWeaponSelection);
        return (Custom.DistLess(self.scavenger.mainBodyChunk.pos, self.scavenger.room.MiddleOfTile(self.focusCreature.BestGuessForPosition()), self.scavenger.MeleeRange * 1.5f)) ? 15 : 0; //60-210 range
    }
    public void Scav_Parry_Sword(On.Scavenger.orig_FlyingWeapon orig, Scavenger self, Weapon weapon)
    {
        orig(self, weapon);

        //test if thrown weapon is a sword
        if (weapon is Sword && Custom.DistLess(self.mainBodyChunk.pos, weapon.firstChunk.pos + weapon.firstChunk.vel, 150f))
            self.immediatelyThrowAtChunk = weapon.firstChunk;
    }
    #endregion
}
