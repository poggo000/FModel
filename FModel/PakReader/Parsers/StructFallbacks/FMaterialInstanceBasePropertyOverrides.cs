using PakReader.Parsers.Objects;

namespace PakReader.Parsers.StructFallbacks
{
    public class FMaterialInstanceBasePropertyOverrides : IUStruct
    {
        public float OpacityMaskClipValue { get; set; }
        public bool DitheredLODTransition { get; set; }
    }
}