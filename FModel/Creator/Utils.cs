using FModel.Utils;
using PakReader.Pak;
using PakReader.Parsers.Class;
using PakReader.Parsers.PropertyTagData;
using SkiaSharp;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using PakReader.Parsers.Objects;

namespace FModel.Creator
{
    static class Utils
    {
        public static string GetFullPath(string partialPath)
        {
            foreach (var fileReader in Globals.CachedPakFiles.Values)
                if (fileReader.TryGetPartialKey(partialPath, out var fullPath))
                {
                    return fullPath;
                }
            return string.Empty;
        }

        public static PakPackage GetPropertyPakPackage(string value)
        {
            string path = Strings.FixPath(value);
            foreach (var fileReader in Globals.CachedPakFiles.Values)
                if (fileReader.TryGetValue(path, out var entry))
                {
                    // kinda sad to use Globals.CachedPakFileMountPoint when the mount point is already in the path ¯\_(ツ)_/¯
                    string mount = path.Substring(0, path.Length - entry.Name.Substring(0, entry.Name.LastIndexOf(".")).Length);
                    return Assets.GetPakPackage(entry, mount);
                }
            return default;
        }
        
        public static bool TryGetPropertyPakPackage(string value, out PakPackage package)
        {
            string path = Strings.FixPath(value);
            foreach (var fileReader in Globals.CachedPakFiles.Values)
                if (fileReader.TryGetValue(path, out var entry))
                {
                    // kinda sad to use Globals.CachedPakFileMountPoint when the mount point is already in the path ¯\_(ツ)_/¯
                    string mount = path.Substring(0, path.Length - entry.Name.Substring(0, entry.Name.LastIndexOf(".")).Length);
                    return Assets.TryGetPakPackage(entry, mount, out package);
                }
            package = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(this IUStruct @struct) where T : IUStruct
        {
            if (@struct is T cast) return cast;
            else if (@struct is UObject @object) return @object.Deserialize<T>();
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LoadObject<T>(this FPackageIndex index) where T : IUExport
        {
            var generic = index.LoadObject();
            if (generic is T export) return export;
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IUExport LoadObject(this FPackageIndex index)
        {
            index.TryLoadObject(out var export);
            return export;
        }

        public static bool TryLoadObject(this FPackageIndex index, out IUExport exportObject)
        {
            var resource = index.Resource;
            exportObject = default;
            return resource switch
            {
                FObjectImport import => import.TryLoadImport(out exportObject),
                FObjectExport export => (exportObject = export.ExportObject.Value) != default,
                _ => false
            };
        }

        public static bool TryLoadImport(this FObjectImport import, out IUExport exportObject)
        {
            //The needed export is located in another asset, try to load it
            exportObject = default;
            var outerImport = import?.OuterIndex.Resource as FObjectImport;
            if (outerImport == null) return false;
            if (TryGetPropertyPakPackage(outerImport.ObjectName.String, out var package))
            { 
                exportObject = package.ExportMap.FirstOrDefault(export =>
                    export.ClassIndex.Name == import.ClassName.String &&
                    export.ObjectName.String == import.ObjectName.String)?.ExportObject.Value;
                return exportObject != default;
            }
            return false;
        }

        public static ArraySegment<byte>[] GetPropertyArraySegmentByte(string value)
        {
            string path = Strings.FixPath(value);
            foreach (var fileReader in Globals.CachedPakFiles.Values)
                if (fileReader.TryGetValue(path, out var entry))
                {
                    // kinda sad to use Globals.CachedPakFileMountPoint when the mount point is already in the path ¯\_(ツ)_/¯
                    string mount = path.Substring(0, path.Length - entry.Name.Substring(0, entry.Name.LastIndexOf(".")).Length);
                    return Assets.GetArraySegmentByte(entry, mount);
                }
            return default;
        }

        public static SKBitmap NewZeroedBitmap(int width, int height) => new SKBitmap(new SKImageInfo(width, height), SKBitmapAllocFlags.ZeroPixels);
        public static SKBitmap Resize(this SKBitmap me, int width, int height)
        {
            var bmp = NewZeroedBitmap(width, height);
            using var pixmap = bmp.PeekPixels();
            me.ScalePixels(pixmap, SKFilterQuality.Medium);
            return bmp;
        }

        public static SKBitmap GetObjectTexture(ObjectProperty o) => GetTexture(o.Value.Resource.OuterIndex.Resource.ObjectName.String);
        public static SKBitmap GetSoftObjectTexture(SoftObjectProperty s) => GetTexture(s.Value.AssetPathName.String);
        public static SKBitmap GetTexture(string s)
        {
            if (s != null)
            {
                if (s.Equals("/Game/UI/Foundation/Textures/BattleRoyale/FeaturedItems/Outfit/T_UI_InspectScreen_annualPass"))
                    s += "_1024";
                else if (s.Equals("/Game/UI/Foundation/Textures/BattleRoyale/BattlePass/T-BattlePass-Season14-Tile") || s.Equals("/Game/UI/Foundation/Textures/BattleRoyale/BattlePass/T-BattlePassWithLevels-Season14-Tile"))
                    s += "_1";
                else if (s.Equals("/Game/UI/Textures/assets/cosmetics/skins/headshot/Skin_Headshot_WolfsBlood_UIT"))
                    s = "/Game/UI/Textures/assets/cosmetics/skins/headshot/Skin_Headshot_Wolfsblood_UIT";
                else if (s.Equals("/Game/UI/Textures/assets/cosmetics/skins/headshot/Skin_Headshot_Timeweaver_UIT"))
                    s = "/Game/UI/Textures/assets/cosmetics/skins/headshot/Skin_Headshot_TimeWeaver_UIT";
            }

            PakPackage p = GetPropertyPakPackage(s);
            if (p.HasExport() && !p.Equals(default))
            {
                var i = p.GetExport<UTexture2D>();
                if (i != null)
                    return SKBitmap.Decode(i.Image.Encode());

                var u = p.GetExport<UObject>();
                if (u != null)
                    if (u.TryGetValue("TextureParameterValues", out var v) && v is ArrayProperty a)
                        if (a.Value.Length > 0 && a.Value[0] is StructProperty str && str.Value is UObject o)
                            if (o.TryGetValue("ParameterValue", out var obj) && obj is ObjectProperty parameterValue)
                                return GetObjectTexture(parameterValue);
            }
            return null;
        }
    }
}
