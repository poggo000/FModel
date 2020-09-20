using System;
using System.Collections.Generic;
using System.Linq;
using FModel.Creator;
using FModel.PakReader.Parsers.Objects;
using PakReader.Materials;
using PakReader.Parsers.PropertyTagData;

namespace PakReader.Parsers.Class.Materials
{
    public class UMaterial : UMaterialInterface
    {
        public bool TwoSided { get; }
        public bool bDisableDepthTest { get; }
        public bool bIsMasked { get; }
        public EBlendMode BlendMode { get; }
        public float OpacityMaskClipValue { get; }
        public List<UTexture> ReferencedTextures { get; }
        
        public UMaterial(PackageReader reader, long validPos) : base(reader)
        {
            TwoSided = GetOrNull<BoolProperty>("TwoSided")?.Value ?? false;
            bDisableDepthTest = GetOrNull<BoolProperty>("bDisableDepthTest")?.Value ?? false;
            bIsMasked = GetOrNull<BoolProperty>("bIsMasked")?.Value ?? false;
            // I'm confused, apparently it's a bool property but a enum ???
            Enum.TryParse<EBlendMode>(GetOrNull<EnumProperty>("BlendMode")?.Value.String, out var blendMode);
            BlendMode = blendMode;
            OpacityMaskClipValue = GetOrNull<FloatProperty>("OpacityMaskClipValue")?.Value ?? 0.333f;
            // TODO Uhm 223 referenced textures doesn't seem right
            ReferencedTextures = GetOrNull<ArrayProperty>("ReferencedTextures")?.Value.ToList().ConvertAll(input => (input as ObjectProperty)?.Value.LoadObject<UTexture>()) ?? new List<UTexture>();
            // UE4 has complex FMaterialResource format, so avoid reading anything here, but
            // scan package's imports for UTexture objects instead
            ScanForTextures(reader);
            reader.Position = validPos;
        }

        private void ScanForTextures(PackageReader reader)
        {
            //!! NOTE: this code will not work when textures are located in the same package - they don't present in import table
            //!! but could be found in export table. That's true for Simplygon-generated materials.
            foreach (var imp in reader.ImportMap)
            {
                if (imp.ClassName.String.StartsWith("Texture", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (imp.TryLoadImport(out var obj) && obj is UTexture tex)
                        ReferencedTextures.Add(tex);
                }
            }
        }

        public override void GetParams(CMaterialParams parameters)
        {
            base.GetParams(parameters);

            var diffWeight = 0;
            var normWeight = 0;
            var specWeight = 0;
            var specPowWeight = 0;
            var opWeight = 0;
            var emWeight = 0;

            void Diffuse(bool check, int weight, UTexture tex)
            {
                if (check && weight > diffWeight) {
                    parameters.Diffuse = tex;
                    diffWeight = weight;
                }
            }
            
            void Normal(bool check, int weight, UTexture tex)
            {
                if (check && weight > normWeight) {
                    parameters.Normal = tex;
                    normWeight = weight;
                }
            }
            
            void Specular(bool check, int weight, UTexture tex)
            {
                if (check && weight > specWeight) {
                    parameters.Specular = tex;
                    specWeight = weight;
                }
            }
            
            void SpecPower(bool check, int weight, UTexture tex)
            {
                if (check && weight > specPowWeight) {
                    parameters.SpecPower = tex;
                    specPowWeight = weight;
                }
            }
            
            void Opacity(bool check, int weight, UTexture tex)
            {
                if (check && weight > opWeight) {
                    parameters.Opacity = tex;
                    opWeight = weight;
                }
            }
            
            void Emissive(bool check, int weight, UTexture tex)
            {
                if (check && weight > emWeight) {
                    parameters.Emissive = tex;
                    emWeight = weight;
                }
            }

            for (int i = 0; i < ReferencedTextures.Count; i++)
            {
                var tex = ReferencedTextures[i];
                var name = tex.Export.ObjectName.String;
                if (name.Contains("noise", StringComparison.CurrentCultureIgnoreCase)) continue;
                if (name.Contains("detail", StringComparison.CurrentCultureIgnoreCase)) continue;
                
                Diffuse(name.Contains("diff", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                Normal(name.Contains("norm", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                Diffuse(name.EndsWith("_Tex", StringComparison.CurrentCultureIgnoreCase), 80, tex);
                Diffuse(name.Contains("_Tex", StringComparison.CurrentCultureIgnoreCase), 60, tex);
                Diffuse(name.Contains("_D", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                Opacity(name.Contains("_OM", StringComparison.CurrentCultureIgnoreCase), 20, tex);

                Diffuse(name.Contains("_DI", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                Diffuse(name.Contains("_D", StringComparison.CurrentCultureIgnoreCase), 11, tex);
                Diffuse(name.Contains("_Albedo", StringComparison.CurrentCultureIgnoreCase), 19, tex);
                Diffuse(name.EndsWith("_C", StringComparison.CurrentCultureIgnoreCase), 10, tex);
                Diffuse(name.EndsWith("_CM", StringComparison.CurrentCultureIgnoreCase), 12, tex);
                Normal(name.EndsWith("_N", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                Normal(name.EndsWith("_NM", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                Normal(name.Contains("_N", StringComparison.CurrentCultureIgnoreCase), 9, tex);

                Specular(name.EndsWith("_S", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                Specular(name.Contains("_S_", StringComparison.CurrentCultureIgnoreCase), 15, tex);
                SpecPower(name.EndsWith("_SP", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                SpecPower(name.EndsWith("_SM", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                SpecPower(name.Contains("_SP", StringComparison.CurrentCultureIgnoreCase), 9, tex);
                Emissive(name.EndsWith("_E", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                Emissive(name.EndsWith("_EM", StringComparison.CurrentCultureIgnoreCase), 21, tex);
                Opacity(name.EndsWith("_A", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                if (bIsMasked)
                    Opacity(name.EndsWith("_Mask", StringComparison.CurrentCultureIgnoreCase), 2, tex);

                Diffuse(name.StartsWith("df_", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                Specular(name.StartsWith("sp_", StringComparison.CurrentCultureIgnoreCase), 20, tex);
                Normal(name.StartsWith("no_", StringComparison.CurrentCultureIgnoreCase), 20, tex);

                Normal(name.Contains("Norm", StringComparison.CurrentCultureIgnoreCase), 80, tex);
                Emissive(name.Contains("Emis", StringComparison.CurrentCultureIgnoreCase), 80, tex);
                Specular(name.Contains("Specular", StringComparison.CurrentCultureIgnoreCase), 80, tex);
                Opacity(name.Contains("Opac", StringComparison.CurrentCultureIgnoreCase), 80, tex);
                Opacity(name.Contains("Alpha", StringComparison.CurrentCultureIgnoreCase), 100, tex);

                Diffuse(i == 0, 1, tex);    // 1st texture as lowest weight
            }
            
            // do not allow normal map became a diffuse
            if ((parameters.Diffuse == parameters.Normal && diffWeight < normWeight) ||
                (parameters.Diffuse != null && parameters.Diffuse.IsTextureCube))
            {
                parameters.Diffuse = null;
            }
        }

        public override void AppendReferencedTextures(List<UUnrealMaterial> outTextures, bool onlyRendered)
        {
            if (onlyRendered)
            {
                // default implementation does that
                base.AppendReferencedTextures(outTextures, true);
            }
            else
            {
                foreach (var referencedTexture in ReferencedTextures)
                {
                    if (!outTextures.Contains(referencedTexture))
                        outTextures.Add(referencedTexture);
                }
            }
        }
    }
}