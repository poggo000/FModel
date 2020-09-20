using PakReader.Parsers.Objects;

namespace PakReader.Parsers.StructFallbacks
{
    public class FScalarParameterValue : IUStruct
    {
        public FName ParameterName { get; set; }
        public float ParameterValue { get; set; }
        public FMaterialParameterInfo ParameterInfo { get; set; }
    }
}