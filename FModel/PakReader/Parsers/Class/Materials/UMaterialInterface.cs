using System;
using FModel.Creator;
using FModel.PakReader.Parsers.Objects;
using PakReader.Materials;
using PakReader.Parsers.PropertyTagData;

namespace PakReader.Parsers.Class.Materials
{
    public class UMaterialInterface : UUnrealMaterial
    {
        // I think those aren't used in UE4 but who knows
        public UTexture FlattenedTexture { get; }
        public UTexture MobileBaseTexture { get; }
        public UTexture MobileNormalTexture { get; }
        public bool bUseMobileSpecular { get; }
        public float MobileSpecularPower { get; }
        public EMobileSpecularMask MobileSpecularMask { get; }
        public UTexture MobileMaskTexture { get; set; }
        
        public UMaterialInterface(PackageReader reader) : base(reader)
        {
            FlattenedTexture = GetOrNull<ObjectProperty>("FlattenedTexture")?.Value.LoadObject<UTexture>();
            MobileBaseTexture = GetOrNull<ObjectProperty>("MobileBaseTexture")?.Value.LoadObject<UTexture>();
            MobileNormalTexture = GetOrNull<ObjectProperty>("MobileNormalTexture")?.Value.LoadObject<UTexture>();
            bUseMobileSpecular = GetOrNull<BoolProperty>("bUseMobileSpecular")?.Value ?? false;
            MobileSpecularPower = GetOrNull<FloatProperty>("MobileSpecularPower")?.Value ?? 16.0f;
            Enum.TryParse<EMobileSpecularMask>(GetOrNull<EnumProperty>("MobileSpecularMask")?.Value.String, out var mobileSpecularMask);
            MobileSpecularMask = mobileSpecularMask;
            MobileMaskTexture = GetOrNull<ObjectProperty>("MobileMaskTexture")?.Value.LoadObject<UTexture>();
        }

        public override void GetParams(CMaterialParams parameters)
        {
            if (FlattenedTexture != null) parameters.Diffuse = FlattenedTexture;
            if (MobileBaseTexture != null) parameters.Diffuse = MobileBaseTexture;
            if (MobileNormalTexture != null) parameters.Normal = MobileNormalTexture;
            if (MobileMaskTexture != null) parameters.Opacity = MobileMaskTexture;
            parameters.UseMobileSpecular = bUseMobileSpecular;
            parameters.MobileSpecularPower = MobileSpecularPower;
            parameters.MobileSpecularMask = MobileSpecularMask;
        }
    }
}