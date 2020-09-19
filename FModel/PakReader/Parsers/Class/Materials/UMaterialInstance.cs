namespace PakReader.Parsers.Class.Materials
{
    public class UMaterialInstance : UMaterialInterface
    {
        public UUnrealMaterial Parent { get; set; }
        
        public UMaterialInstance(PackageReader reader) : base(reader)
        {
            
        }
    }
}