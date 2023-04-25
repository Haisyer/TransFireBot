using MathNet.Numerics.LinearAlgebra.Factorization;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
namespace SysBot.Pokemon.Helpers
{
    /// <summary>
    /// 帮助实现通过文件交换宝可梦的工具类
    /// </summary>
    public class FileTradeHelper<T> where T : PKM, new()
    {
        /// <summary>
        /// 得到对应版本的宝可梦实例
        /// </summary>
        /// <param name="data">存有宝可梦数据的数组</param>
        /// <returns>PKM实例</returns>
        public static PKM? GetPokemon(byte[] data) => typeof(T) switch
        {
            Type pkm when pkm == typeof(PK8) => new PK8(data),
            Type pkm when pkm == typeof(PB8) => new PB8(data),
            Type pkm when pkm == typeof(PA8) => new PA8(data),
            Type pkm when pkm == typeof(PK9) => new PK9(data),
            _ => null
        };
        /// <summary>
        /// 对应版本中存储一个宝可梦数据所需要的字节大小
        /// </summary>
        public static readonly Dictionary<Type, int> pokemonSizeInFile = new()
        {
            { typeof(PK8), 344 },
            { typeof(PB8), 344 },
            { typeof(PA8), 376 },
            { typeof(PK9), 344 }
        };
        /// <summary>
        ///对应版本的Bin文件中存储一个宝可梦数据所需要的字节大小
        /// </summary>
        private static readonly Dictionary<Type, int> pokemonSizeInBin = new()
        {
            { typeof(PK8), 344 },
            { typeof(PB8), 344 },
            { typeof(PA8), 360 },
            { typeof(PK9), 344 }
        };
        /// <summary>
        /// 对应版本的Bin文件中可存储的最大宝可梦数量
        /// </summary>
        public static readonly Dictionary<Type, int> maxCountInBin = new()
        {
            { typeof(PK8), 960 },
            { typeof(PB8), 1200 },
            { typeof(PA8), 960 },
            { typeof(PK9), 960 }
        };

        /// <summary>
        /// 将bin文件数据转换成对应版本的PKM实例并存到List中
        /// </summary>
        /// <param name="bindata">存储宝可梦数据的数组</param>
        /// <returns></returns>
        public static List<T> BinToList(byte[] binData)
        {  
            int size = pokemonSizeInBin[typeof(T)];
            int times = binData.Length % size == 0 ? (binData.Length / size) : (binData.Length / size + 1);
            List<T> pkmBytes = new();
            for (var i = 0; i < times; i++)
            {
                int start = i * size;
                int end = (start + size) > binData.Length ? binData.Length : (start + size);
                var tp = GetPokemon(binData[start..end]);

                if (tp != null && tp is T pkm && tp.Species > 0 && tp.Valid)
                {
                        pkmBytes.Add(pkm); 
                }
            }
            return pkmBytes;
        }

        /// <summary>
        ///  将pk文件数据转换成对应版本的PKM实例并存到List中
        /// </summary>
        /// <param name="fileData">存储宝可梦数据的数组</param>
        /// <returns></returns>
        public static List<T> SingleToList(byte[] fileData)
        {  
            List<T> pkmBytes = new();
            
            var pk = GetPokemon(fileData);
            if (pk != null && pk is T pkm && pk.Species > 0 && pk.Valid)
            {
                    pkmBytes.Add(pkm);
            }

            return pkmBytes;
        }
        /// <summary>
        /// 将文件数据转换成对应版本的PKM实例并存到List中
        /// </summary>
        /// <param name="bufferData">存储宝可梦数据的数组</param>
        /// <returns></returns>
        public static List<T> DataToList(byte[] bufferData)
        {  
            long length = bufferData.Length;
            List<T> pkmBytes = new();

            if (IsValidPokemonFileSize(length))
            {
                pkmBytes = SingleToList(bufferData);
            }
            else
            {
                pkmBytes = BinToList(bufferData);
            }

            return pkmBytes;
        }


        /// <summary>
        /// 文件名称是否有效
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>
        /// <para>true: 有效</para>
        /// <para>false: 无效</para>
        /// </returns>
        public static bool IsValidFileName(string fileName)
        {
            string fileExtension = fileName?.Split('.').Last().ToLower() ?? "";
            return (fileExtension == typeof(T).Name.ToLower()) || (fileExtension == "bin");
        }

        /// <summary>
        /// 文件大小是否有效
        /// </summary>
        /// <param name="size">文件大小</param>
        /// <returns>
        /// <para>true: 有效</para>
        /// <para>false: 无效</para>
        /// </returns>
        public static bool IsValidFileSize(long size) => IsValidPokemonFileSize(size) || IsValidBinFileSize(size);

        /// <summary>
        /// 对应版本的pk文件大小是否有效
        /// </summary>
        /// <param name="size">存储一个宝可梦数据的字节大小</param>
        /// <returns>
        /// <para>true: 有效</para>
        /// <para>false: 无效</para>
        /// </returns>
        public static bool IsValidPokemonFileSize(long size) => size == pokemonSizeInFile[typeof(T)];

        /// <summary>
        /// 对应版本的Bin文件大小是否有效
        /// </summary>
        /// <param name="size"></param>
        /// <returns>
        /// <para>true: 有效</para>
        /// <para>false: 无效</para>
        /// </returns>
        public static bool IsValidBinFileSize(long size) => (size > 0) && (size <= maxPokemonCountInBin * pokemonSizeInBin[typeof(T)]) && (size % pokemonSizeInBin[typeof(T)] == 0);

        /// <summary>
        /// <para>对应版本的Bin文件中可存储的最大宝可梦数量</para>
        /// <para>将字典类型变为int类型</para>
        /// </summary>
        public static int maxPokemonCountInBin => maxCountInBin[typeof(T)];

    }
}