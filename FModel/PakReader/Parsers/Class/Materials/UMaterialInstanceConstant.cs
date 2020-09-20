using System;
using System.Collections.Generic;
using System.Linq;
using FModel.Creator;
using PakReader.Materials;
using PakReader.Parsers.Objects;
using PakReader.Parsers.PropertyTagData;
using PakReader.Parsers.StructFallbacks;

namespace PakReader.Parsers.Class.Materials
{
    public class UMaterialInstanceConstant : UMaterialInstance
    {
        public List<FScalarParameterValue> ScalarParameterValues { get; }
        public List<FTextureParameterValue> TextureParameterValues { get; }
        public List<FVectorParameterValue> VectorParameterValues { get; }
        
        public UMaterialInstanceConstant(PackageReader reader) : base(reader)
        {
            ScalarParameterValues = GetOrNull<ArrayProperty>("ScalarParameterValues")?.Value.ToList().ConvertAll(
                input =>
                {
                    var @struct = (input as StructProperty)?.Value;
                    if (@struct == null) return default;
                    var value = @struct.As<FScalarParameterValue>();
                    // Hack since the As<>-method doesn't support nested structs/classes
                    var parameterInfo = (@struct as UObject)?.GetOrNull<StructProperty>("ParameterInfo")?.Value.As<FMaterialParameterInfo>();
                    value.ParameterInfo = parameterInfo;
                    return value;
                }
                );
            TextureParameterValues = GetOrNull<ArrayProperty>("TextureParameterValues")?.Value.ToList().ConvertAll(
                input =>
                {
                    var @struct = (input as StructProperty)?.Value;
                    if (@struct == null) return default;
                    var value = @struct.As<FTextureParameterValue>();
                    // Hack since the As<>-method doesn't support nested structs/classes
                    var parameterInfo = (@struct as UObject)?.GetOrNull<StructProperty>("ParameterInfo")?.Value.As<FMaterialParameterInfo>();
                    var parameterValue = (@struct as UObject)?.GetOrNull<ObjectProperty>("ParameterValue")?.Value.LoadObject<UTexture2D>();
                    value.ParameterInfo = parameterInfo;
                    value.ParameterValue = parameterValue;
                    return value;
                }
            );
            VectorParameterValues = GetOrNull<ArrayProperty>("VectorParameterValues")?.Value.ToList().ConvertAll(
                input =>
                {
                    var @struct = (input as StructProperty)?.Value;
                    if (@struct == null) return default;
                    var value = @struct.As<FVectorParameterValue>();
                    // Hack since the As<>-method doesn't support nested structs/classes
                    var parameterInfo = (@struct as UObject)?.GetOrNull<StructProperty>("ParameterInfo")?.Value.As<FMaterialParameterInfo>();
                    value.ParameterInfo = parameterInfo;
                    return value;
                }
            );
        }

        public override void GetParams(CMaterialParams parameters)
        {
            // get params from linked UMaterial3
            if (Parent != null && Parent != this)
                Parent.GetParams(parameters);
            
            base.GetParams(parameters);
            
            // get local parameters
            var diffWeight = 0;
            var normWeight = 0;
            var specWeight = 0;
            var specPowWeight = 0;
            var opWeight = 0;
            var emWeight = 0;
            var emcWeight = 0;
            var cubeWeight = 0;
            var maskWeight = 0;

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
            
            void CubeMap(bool check, int weight, UTexture tex)
            {
                if (check && weight > cubeWeight) {
                    parameters.Cube = tex;
                    cubeWeight = weight;
                }
            }
            
            void BakedMask(bool check, int weight, UTexture tex)
            {
                if (check && weight > maskWeight) {
                    parameters.Mask = tex;
                    maskWeight = weight;
                }
            }
            
            void EmissiveColor(bool check, int weight, FLinearColor color)
            {
                if (check && weight > emcWeight) {
                    parameters.EmissiveColor = color;
                    emcWeight = weight;
                }
            }

            if (TextureParameterValues.Count > 0)
                parameters.Opacity = null;     // it's better to disable opacity mask from parent material
            
            foreach (var p in TextureParameterValues)
            {
                var name = p.Name;
                var tex = p.ParameterValue;
                if (tex == null) continue;
                
                if (name.Contains("detail", StringComparison.CurrentCultureIgnoreCase)) continue;

                Diffuse(name.Contains("dif", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                Diffuse(name.Contains("albedo", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                Diffuse(name.Contains("color", StringComparison.CurrentCultureIgnoreCase), 80, tex);
                Normal(name.Contains("norm", StringComparison.CurrentCultureIgnoreCase) && !name.Contains("fx", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                SpecPower(name.Contains("specpow", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                Specular(name.Contains("spec", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                Emissive(name.Contains("emiss", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                CubeMap(name.Contains("cube", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                CubeMap(name.Contains("refl", StringComparison.CurrentCultureIgnoreCase), 90, tex);
                Opacity(name.Contains("opac", StringComparison.CurrentCultureIgnoreCase), 90, tex);
                Opacity(name.Contains("trans", StringComparison.CurrentCultureIgnoreCase) && !name.Contains("transm", StringComparison.CurrentCultureIgnoreCase), 80, tex);
                Opacity(name.Contains("opacity", StringComparison.CurrentCultureIgnoreCase), 100, tex);
                Opacity(name.Contains("alpha", StringComparison.CurrentCultureIgnoreCase), 100, tex);
            }
            
            foreach (var p in VectorParameterValues)
            {
                var name = p.Name;
                var color = p.ParameterValue;
                EmissiveColor(name.Contains("Emissive", StringComparison.CurrentCultureIgnoreCase), 100, color);
            }
            
            // try to get diffuse texture when nothing found
            if (parameters.Diffuse == null && TextureParameterValues.Count == 1)
                parameters.Diffuse = TextureParameterValues[0].ParameterValue;
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
                foreach (var value in TextureParameterValues)
                {
                    var tex = value.ParameterValue;
                    if (!outTextures.Contains(tex))
                        outTextures.Add(tex);
                }

                if (Parent != null && Parent != this)
                    Parent.AppendReferencedTextures(outTextures, false);
            }
        }
    }
}