using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class ImportTexture : AssetPostprocessor  //导入图片以后自动设置图集和格式//
{


    void OnPreprocessTexture()
    {
        //TextureImporter textureImporter = assetImporter as TextureImporter;


        //textureImporter.textureType = TextureImporterType.Advanced;
        //textureImporter.mipmapEnabled = false;
        ////textureImporter.wrapMode = TextureWrapMode.Clamp;
        //textureImporter.filterMode = FilterMode.Bilinear;
        //textureImporter.npotScale = TextureImporterNPOTScale.None;


        ////暂时先打包成这两种格式，以后再更具路径去优化压缩格式//
        //textureImporter.SetPlatformTextureSettings("Standalone", 256, TextureImporterFormat.ARGB32);
        //textureImporter.SetPlatformTextureSettings("Android", 256, TextureImporterFormat.ETC2_RGBA8, 50);
        //textureImporter.SetPlatformTextureSettings("iPhone", 256, TextureImporterFormat.PVRTC_RGBA4, 50);

        //if (assetPath.Contains("Data/UI"))  //ui界面默认图片
        //{
        //    textureImporter.spriteImportMode = SpriteImportMode.Single;
        //    textureImporter.isReadable = false;
        //}

    }

}