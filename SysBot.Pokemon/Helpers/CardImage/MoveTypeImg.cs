using System.Collections.Generic;

namespace SysBot.Pokemon
{
    public class MoveTypeImg
    {
        #region 技能属性图片
        public static Dictionary<int, string> MoveTypeUrlMapping = new Dictionary<int, string>
        {
            {0, "https://img.imdodo.com/openapitest/upload/cdn/2CE02B92CE2CE2915D242CF14CDD97B4_1696480626077.png"},
            {1, "https://img.imdodo.com/openapitest/upload/cdn/456CA04DE6139F1D5D33F9C4FD78EA3C_1696480685989.png"},
            {2, "https://img.imdodo.com/openapitest/upload/cdn/6A26F0A97777178BA6ABB257CF0E6B4D_1696480720165.png"},
            {3, "https://img.imdodo.com/openapitest/upload/cdn/01AE4F7A6BA50BEB8950910EB9FA3370_1696480743688.png"},
            {4, "https://img.imdodo.com/openapitest/upload/cdn/1A0F49305B6BA81CC8EFD0014D6B690F_1696480754896.png"},
            {5, "https://img.imdodo.com/openapitest/upload/cdn/FE201FB0295F260DCD85741442BB09DD_1696480767879.png"},
            {6, "https://img.imdodo.com/openapitest/upload/cdn/720BA57C30F3F14FF28B556AF13008A6_1696480780120.png"},
            {7, "https://img.imdodo.com/openapitest/upload/cdn/9F58CD23218068A4B922C031ED2DE56F_1696480816437.png"},
            {8, "https://img.imdodo.com/openapitest/upload/cdn/BE5FBA7094B3E242AF77CAD8D0C3D4C9_1696480836975.png"},
            {9, "https://img.imdodo.com/openapitest/upload/cdn/EC5BAEEF38BE5209E86EC529A540BF5E_1696480884372.png"},
            {10, "https://img.imdodo.com/openapitest/upload/cdn/90F8B934D9B47D744AACC6483A3416CD_1696480904695.png"},
            {11, "https://img.imdodo.com/openapitest/upload/cdn/CB465AD8322745DC3823913CF6AD29E0_1696480915807.png"},
            {12, "https://img.imdodo.com/openapitest/upload/cdn/2B607AC81E2FA7327350DC1C9540436A_1696480929641.png"},
            {13, "https://img.imdodo.com/openapitest/upload/cdn/9DACA5DD447213C9244B90603C4E9142_1696480940561.png"},
            {14, "https://img.imdodo.com/openapitest/upload/cdn/C992EE07DF799AB340AF312703A0333B_1696480951866.png"},
            {15, "https://img.imdodo.com/openapitest/upload/cdn/D5849589A7528F9D16BFDAF9E7F3EEF5_1696480967316.png"},
            {16, "https://img.imdodo.com/openapitest/upload/cdn/B0126F715DF56EBA95195523F14AF088_1696480990711.png"},
            {17, "https://img.imdodo.com/openapitest/upload/cdn/EBB606283A27C3C6BCDCE6CB99B6EF4C_1696481011850.png"},
        };
        #endregion

        public string MoveTypeToChinese(int move)
        {
            if (MoveTypeUrlMapping.ContainsKey(move))
            {
                return MoveTypeUrlMapping[move];
            }
            else
            {
                string errorUrl = "https://img.imdodo.com/openapitest/upload/cdn/AEA3F842940BD2E6418AE36231F53BB7_1696061304099.png";
                return errorUrl;
            }
        }
    }
}
