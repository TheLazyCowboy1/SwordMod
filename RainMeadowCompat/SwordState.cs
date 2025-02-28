using RainMeadow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sword = SwordMod.Sword;

namespace RainMeadowCompat;

public class SwordState : RealizedWeaponState
{
    [OnlineField]
    public int swingTimer = 0;
    //[OnlineField]
    //public Vector2 swingDir = new Vector2(0, 0);
    [OnlineFieldHalf]
    public float swingDirX = 0;
    [OnlineFieldHalf]
    public float swingDirY = 0;
    [OnlineField]
    public float swingDamage = 0f;
    [OnlineField]
    public float swingExhaustion = 0f;

    public SwordState() { }
    public SwordState(OnlinePhysicalObject opo) : base(opo)
    {
        var sword = (Sword)opo.apo.realizedObject;
        swingTimer = sword.swingTimer;
        swingDirX = sword.swingDir.x;
        swingDirY = sword.swingDir.y;
        swingDamage = sword.swingDamage;
        swingExhaustion = sword.swingExhaustion;
    }

    public override void ReadTo(OnlineEntity onlineEntity)
    {
        var sword = (Sword)((OnlinePhysicalObject)onlineEntity).apo.realizedObject;
        /*Vector2 origPos = sword.firstChunk.pos,
            origVel = sword.firstChunk.vel,
            origRot = sword.rotation;*/

        base.ReadTo(onlineEntity);

        //skip updating pos, vel, and rot
        /*sword.firstChunk.pos = origPos;
        sword.firstChunk.vel = origVel;
        sword.rotation = origRot;*/
        
        sword.swingTimer = swingTimer;
        sword.swingDir.x = swingDirX;
        sword.swingDir.y = swingDirY;
        sword.swingDamage = swingDamage;
        sword.swingExhaustion = swingExhaustion;
    }
}
