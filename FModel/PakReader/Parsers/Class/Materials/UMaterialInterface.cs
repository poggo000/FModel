using FModel.PakReader.Parsers.Objects;
using PakReader.Materials;

namespace PakReader.Parsers.Class.Materials
{
    public class UMaterialInterface : UUnrealMaterial
    {
        // I think those aren't used in UE4 but who knows
        public UTexture FlattenedTexture { get; set; }
        public UTexture MobileBaseTexture { get; set; }
        public UTexture MobileNormalTexture { get; set; }
        public bool bUseMobileSpecular { get; set; }
        public float MobileSpecularPower { get; set; } = 16.0f;
        public EMobileSpecularMask MobileSpecularMask { get; set; } = EMobileSpecularMask.MSM_Constant;
        public UTexture MobileMaskTexture { get; set; }
        
        public UMaterialInterface(PackageReader reader) : base(reader)
        {
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