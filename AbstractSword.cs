using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SwordMod;

public class AbstractSword : AbstractPhysicalObject
{
    public int TextureIndex = UnityEngine.Random.Range(0, SwordMod.SwordTextureCount);

    public AbstractSword(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID)
        : base(world, type, realizedObject, pos, ID)
    {
        //if we have the TextureIndex, you can bet it'll be an unrecognized attribute
        if (this.unrecognizedAttributes != null && this.unrecognizedAttributes.Length > 0 && Int32.TryParse(this.unrecognizedAttributes[0], out var texIdx))
        {
            if (texIdx < SwordMod.SwordTextureCount)
                TextureIndex = texIdx;
            this.unrecognizedAttributes = this.unrecognizedAttributes.Skip(1).ToArray();
        }
    }

    public AbstractSword(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int textureIndex)
        : this(world, type, realizedObject, pos, ID)
    {
        TextureIndex = textureIndex;
    }

    public override string ToString()
    {
        return base.ToString() + "<oA>" + TextureIndex; //simply appends the texture index
    }
}
