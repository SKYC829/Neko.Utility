using System;
using System.Collections.Generic;
using System.Text;

namespace Neko.Utility
{
    /// <summary>
    /// 二维码/条形码生成配置
    /// </summary>
    [Serializable]
    public class QbCodeConfiguration
    {
        /// <summary>
        /// 码边距
        /// </summary>
        public int Margin { get; set; }

        /// <summary>
        /// 码长度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 码高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Logo长度
        /// </summary>
        public int LogoWidth { get; set; }

        /// <summary>
        /// Logo宽度
        /// </summary>
        public int LogoHeight { get; set; }

        /// <summary>
        /// 字符编码
        /// </summary>
        public string Charset { get; set; }

        /// <summary>
        /// 二维码默认配置
        /// </summary>
        public static QbCodeConfiguration QrCodeDefault { get; }

        /// <summary>
        /// 条码默认配置
        /// </summary>
        public static QbCodeConfiguration BarCodeDefault { get; }

        static QbCodeConfiguration()
        {
            QrCodeDefault = InitQrCodeDefault();
            BarCodeDefault = InitBarCodeDefault();
        }

        private static QbCodeConfiguration InitQrCodeDefault()
        {
            return new QbCodeConfiguration()
            {
                Margin = 2,
                Width = 250,
                Height = 250,
                LogoWidth = 83,
                LogoHeight = 83,
                Charset = "utf-8"
            };
        }

        private static QbCodeConfiguration InitBarCodeDefault()
        {
            return new QbCodeConfiguration()
            {
                Margin = 15,
                Width = 120,
                Height = 40,
                Charset = "utf-8"
            };
        }
    }
}
