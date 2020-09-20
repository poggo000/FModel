using PakReader.Parsers.Objects;

namespace PakReader.Parsers.StructFallbacks
{
    public class FVectorParameterValue : IUStruct
    {
        public FName ParameterName { get; set; }
        public FMaterialParameterInfo ParameterInfo { get; set; }
        public FLinearColor ParameterValue;
        
        public string Name => !string.IsNullOrEmpty(ParameterName.String)
            ? ParameterName.String
            : ParameterInfo.Name.String;
    }
}