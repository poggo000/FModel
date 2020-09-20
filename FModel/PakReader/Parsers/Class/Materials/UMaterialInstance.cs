using FModel.Creator;
using PakReader.Parsers.PropertyTagData;
using PakReader.Parsers.StructFallbacks;

namespace PakReader.Parsers.Class.Materials
{
    public class UMaterialInstance : UMaterialInterface
    {
        public UUnrealMaterial Parent { get; }
        public FMaterialInstanceBasePropertyOverrides BasePropertyOverrides { get; }

        public UMaterialInstance(PackageReader reader) : base(reader)
        {
            Parent = GetOrNull<ObjectProperty>("Parent")?.Value.LoadObject<UUnrealMaterial>();
            BasePropertyOverrides = GetOrNull<StructProperty>("BasePropertyOverrides")?.Value?.As<FMaterialInstanceBasePropertyOverrides>() ?? default;
        }
    }
}