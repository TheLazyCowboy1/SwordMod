using RainMeadow;
using static RainMeadowCompat.EasyConfigSync;

namespace RainMeadowCompat;

/**<summary>
 * This file is here for you to safely signal to signal your Rain Meadow data
 * or receive Rain Meadow information
 * through the SafeMeadowInterface class.
 * 
 * Anything you want to call here should always be called by SafeMeadowInterface instead.
 * 
 * All functions in this file are PURELY examples.
 * </summary>
 */
public static class MeadowInterface
{
    [RPCMethod]
    public static void SwordHitRPC(OnlinePhysicalObject sword, OnlineCreature creature, float damage)
    {
        if (sword.apo.realizedObject is SwordMod.Sword s && creature.realizedCreature is Creature c)
            s.HitSomethingClientSide(new SharedPhysics.CollisionResult(c, c.mainBodyChunk, null, true, UnityEngine.Vector2.zero), null, damage);
    }

    //public static ConfigData configData = new();
    static MeadowInterface() {
        MeadowCompatSetup.DataToAdd.Add(typeof(ConfigData));
    }

    /**<summary>
     * This is an example of how you could signal that there has been a change to some data.
     * This function is likely totally unnecessary:
     *  No one's going to be changing config data mid-game, right??
     * But it's here just in case someone does something crazy like that.
     * And it's a good example for how to signal an update to online data.
     * 
     * For example:
     * UpdateRandomizerData() would have:
     * OnlineManager.lobby.GetData<RandomizerData>().UpdateData();
     * </summary>
     */
    public static void UpdateConfigData()
    {
        //configData.UpdateData();
        if (OnlineManager.lobby != null && OnlineManager.lobby.TryGetData<ConfigData>(out var data))
            data.UpdateData();
    }

    public static bool IsOnline() => OnlineManager.lobby != null;
    public static bool IsHost() => OnlineManager.lobby.isOwner;

    public static bool IsMine(AbstractPhysicalObject apo)
    {
        return apo.IsLocal();
    }

    public static bool FriendlyFire() //note: returns false by default
    {
        return OnlineManager.lobby?.gameMode is StoryGameMode story && story.friendlyFire;
    }

    public static void SignalSwordHit(SwordMod.Sword sword, Creature creature, float damage)
    {
        foreach (var player in OnlineManager.players)
            if (!player.isMe)
                player.InvokeRPC(SwordHitRPC, sword.abstractPhysicalObject.GetOnlineObject(), creature.abstractCreature.GetOnlineCreature(), damage);
    }

}
