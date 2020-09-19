using System.Collections.Generic;
using FModel.PakReader.Parsers.Objects;
using PakReader.Parsers.Class.Materials;
using PakReader.Parsers.Objects;

namespace PakReader.Materials
{
    public class CMaterialParams
    {
        public UUnrealMaterial Diffuse { get; set; }
        public UUnrealMaterial Normal { get; set; }
        public UUnrealMaterial Specular { get; set; }
        public UUnrealMaterial SpecPower { get; set; }
        public UUnrealMaterial Opacity { get; set; }
        public UUnrealMaterial Emissive { get; set; }
        public UUnrealMaterial Cube { get; set; }
        public UUnrealMaterial Mask { get; set; }        // multiple mask textures baked into a single one
        // channels (used with Mask texture)
        public ETextureChannel EmissiveChannel { get; set; } = ETextureChannel.TC_NONE;
        public ETextureChannel SpecularMaskChannel { get; set; } = ETextureChannel.TC_NONE;
        public ETextureChannel SpecularPowerChannel { get; set; } = ETextureChannel.TC_NONE;
        public ETextureChannel CubemapMaskChannel { get; set; } = ETextureChannel.TC_NONE;
        // colors
        public FLinearColor EmissiveColor { get; set; }
        // mobile
        public bool UseMobileSpecular { get; set; } = false;
        public float MobileSpecularPower { get; set; } = 0.0f;
        public EMobileSpecularMask MobileSpecularMask { get; set; } = EMobileSpecularMask.MSM_Constant;
        // tweaks
        public bool SpecularFromAlpha { get; set; } = false;
        public bool OpacityFromAlpha { get; set; } = false;

        public void AppendAllTextures(List<UUnrealMaterial> outTextures)
        {
            if (Diffuse != null) outTextures.Add(Diffuse);
            if (Normal != null) outTextures.Add(Normal);
            if (Specular != null) outTextures.Add(Specular);
            if (SpecPower != null) outTextures.Add(SpecPower);
            if (Opacity != null) outTextures.Add(Opacity);
            if (Emissive != null) outTextures.Add(Emissive);
            if (Cube != null) outTextures.Add(Cube);
            if (Mask != null) outTextures.Add(Mask);
        }

        public bool IsNull()
        {
            return Diffuse == null
                   && Normal == null
                   && Specular == null
                   && SpecPower == null
                   && Opacity == null
                   && Emissive == null
                   && Cube == null
                   && Mask == null;
        }
    }
}