using PakReader.Parsers.Class;
using PakReader.Parsers.Objects;

namespace PakReader.Parsers.StructFallbacks
{
    public class FTextureParameterValue : IUStruct
    {
        public FName ParameterName { get; set; }
        public FMaterialParameterInfo ParameterInfo { get; set; }
        public UTexture2D ParameterValue;

        public string Name => !string.IsNullOrEmpty(ParameterName.String)
            ? ParameterName.String
            : ParameterInfo.Name.String;
    }
}