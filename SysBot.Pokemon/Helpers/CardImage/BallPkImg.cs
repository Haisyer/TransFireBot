using System;
using System.Collections.Generic;

namespace SysBot.Pokemon
{
    public class BallPkImg
    {
        #region «Ú÷÷Õº∆¨
        public static Dictionary<int, string> ballUrlMapping = new Dictionary<int, string>
        {
            {1, "https://img.imdodo.com/openapitest/upload/cdn/3B4F0ED4A6E2AA7DBEBBF7D126CE54C1_1695818625603.png"},
            {2, "https://img.imdodo.com/openapitest/upload/cdn/3336D8D5829F4673A12E0A09B2423DE5_1695818632527.png"},
            {3, "https://img.imdodo.com/openapitest/upload/cdn/BA5EDCEE397E2FB9A9BA8E92F0EF14B4_1695818638570.png"},
            {4, "https://img.imdodo.com/openapitest/upload/cdn/A89273546EB474274D49935DAE5AA471_1695818644403.png"},
            {5, "https://img.imdodo.com/openapitest/upload/cdn/D4BC8C6FEAC1B9274FAA68E29407DA1E_1695818644913.png"},
            {6, "https://img.imdodo.com/openapitest/upload/cdn/CA34B05B7ADA486216949267DD4358D2_1695818645383.png"},
            {7, "https://img.imdodo.com/openapitest/upload/cdn/5E67253762EF279FB13D91B19ACA6200_1695818646060.png"},
            {8, "https://img.imdodo.com/openapitest/upload/cdn/33C4B9B56534BD3BAE12CED68C722C24_1695818646566.png"},
            {9, "https://img.imdodo.com/openapitest/upload/cdn/82DD61D2F49F55C3BA85C659A42B52A6_1695818647297.png"},
            {10, "https://img.imdodo.com/openapitest/upload/cdn/0D8DD98E9518CCA78CE4B0BD22784539_1695818626127.png"},
            {11, "https://img.imdodo.com/openapitest/upload/cdn/9B3FAAB5E51612BAF1A548B61862E45C_1695818626856.png"},
            {12, "https://img.imdodo.com/openapitest/upload/cdn/308382981BD99DF0AF983EE2CFD46941_1695818627424.png"},
            {13, "https://img.imdodo.com/openapitest/upload/cdn/C2AB211AF76314FF1D2FE90A616F0C8C_1695818628020.png"},
            {14, "https://img.imdodo.com/openapitest/upload/cdn/CAA0F86A654DD3CB0A11ED8E975FCDB1_1695818628540.png"},
            {15, "https://img.imdodo.com/openapitest/upload/cdn/FCF2D06DFE8A94371A9C802F148B8E06_1695818629163.png"},
            {16, "https://img.imdodo.com/openapitest/upload/cdn/88C3EAA541F281F0646909477D705FFC_1695818629723.png"},
            {17, "https://img.imdodo.com/openapitest/upload/cdn/D6D2CFA6D54F77DBCA193C0B1C17E7FE_1695818630429.png"},
            {18, "https://img.imdodo.com/openapitest/upload/cdn/3E6F711685A6B7090F071541F8412BBB_1695818631167.png"},
            {19, "https://img.imdodo.com/openapitest/upload/cdn/7115ED7521A27BABCDED334DE117D6D3_1695818631832.png"},
            {20, "https://img.imdodo.com/openapitest/upload/cdn/9822354ABF4B42E1BE854C5A17C3C81D_1695818633017.png"},
            {21, "https://img.imdodo.com/openapitest/upload/cdn/C3E20869D944525C98F7B2CD8B5592E8_1695818633671.png"},
            {22, "https://img.imdodo.com/openapitest/upload/cdn/05A08E932AB0B70EE4CAE9CC2DEB76E8_1695818634209.png"},
            {23, "https://img.imdodo.com/openapitest/upload/cdn/E2F4BD6A4945DD5B7D0B6A3A799545BF_1695818634710.png"},
            {24, "https://img.imdodo.com/openapitest/upload/cdn/58D3F214326A7DCCBDDE85BC5FEDF545_1695818635507.png"},
            {25, "https://img.imdodo.com/openapitest/upload/cdn/927301BB3882C91ADF4A3899B9D3A780_1695818636000.png"},
            {26, "https://img.imdodo.com/openapitest/upload/cdn/BF6129FBAA870780DFAA3D1416AC0152_1695818636485.png"},
            {27, "https://img.imdodo.com/openapitest/upload/cdn/768B10DC6EF0D35A69FE19ADD99E28B3_1695818636976.png"},
            {28, "https://img.imdodo.com/openapitest/upload/cdn/C265AD3FF112FE57DD6D2F9B5818F526_1695818637464.png"},
            {29, "https://img.imdodo.com/openapitest/upload/cdn/DD07B7FF63851388F5FA3775CF593D5A_1695818638004.png"},
            {30, "https://img.imdodo.com/openapitest/upload/cdn/4A550DF8D611A4AEFD5FC75E16F68FA0_1695818639076.png"},
            {31, "https://img.imdodo.com/openapitest/upload/cdn/6FC94A66998B69A296F00116D1EFBD23_1695818639686.png"},
            {32, "https://img.imdodo.com/openapitest/upload/cdn/4A6F7AEB55F7500256EBED35FE05F7EF_1695818640217.png"},
            {33, "https://img.imdodo.com/openapitest/upload/cdn/62D6AC905D9DCD3541F8DA7D58B41360_1695818640759.png"},
            {34, "https://img.imdodo.com/openapitest/upload/cdn/14912395A8826C6BCDF1F876EE181B29_1695818641238.png"},
            {35, "https://img.imdodo.com/openapitest/upload/cdn/8CB33786BC7136AF7EFFB53D640FAB18_1695818641893.png"},
            {36, "https://img.imdodo.com/openapitest/upload/cdn/1B63464BEA7CC7105FE5360357A01FEF_1695818642783.png"},
            {37, "https://img.imdodo.com/openapitest/upload/cdn/8219D752C646B30F43F401FDFCEC52A0_1695818643317.png"},

        };
        #endregion
    }
}

