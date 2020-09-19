using PakReader.Materials;
using PakReader.Parsers.Class.Materials;

namespace PakReader.Parsers.Class
{
    public abstract class UTexture : UUnrealMaterial
    {
        protected UTexture(PackageReader reader) : base(reader)
        {
        }

        public override void GetParams(CMaterialParams parameters)
        {
            // ???
        }
    }
}