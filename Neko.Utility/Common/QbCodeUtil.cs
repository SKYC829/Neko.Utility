using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Windows.Compatibility;

namespace Neko.Utility.Common
{
    /// <summary>
    /// 二维码/条形码帮助类
    /// </summary>
    public sealed class QbCodeUtil
    {
        /// <summary>
        /// 绘制图标
        /// </summary>
        /// <param name="code">码图片数据</param>
        /// <param name="matrix">码图片数据的<see cref="BitMatrix"/></param>
        /// <param name="logo">图标图片数据</param>
        /// <param name="configuration">配置数据</param>
        /// <returns></returns>
        public static Bitmap DrawLogo(Bitmap code, BitMatrix matrix, Bitmap logo, QbCodeConfiguration configuration = null)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code), "绘制Logo时,原码图片数据不允许为空!");
            }

            if (configuration == null)
            {
                return code;
            }

            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix), "绘制Logo时,原码图片的位矩阵不允许为空!");
            }

            if (logo == null)
            {
                return code;
            }

            int[] matrixRectangle = matrix.getEnclosingRectangle();

            if (configuration.LogoWidth < 0)
            {
                configuration.LogoWidth = logo.Width;
            }

            if (configuration.LogoHeight < 0)
            {
                configuration.LogoHeight = logo.Height;
            }

            //logo大小等比缩放至二维码大小的三分之一
            configuration.LogoWidth = Math.Min(configuration.LogoWidth, (int)matrixRectangle[2] / 3);
            configuration.LogoHeight = Math.Min(configuration.LogoHeight, (int)matrixRectangle[3] / 3);

            int marginLeft = (code.Width - configuration.LogoWidth) / 2;
            int marginTop = (code.Height - configuration.LogoHeight) / 2;

            Bitmap result = new Bitmap(code.Width, code.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(code, 0, 0, code.Width, code.Height);
                graphics.FillRectangle(Brushes.White, marginLeft, marginTop, configuration.LogoWidth, configuration.LogoHeight);
                graphics.DrawImage(logo, marginLeft, marginTop, configuration.LogoWidth, configuration.LogoHeight);
            }
            return result;
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="logo">二维码图标图片数据</param>
        /// <param name="configuration">配置数据</param>
        /// <returns></returns>
        public static Bitmap GenerateQrCode(string content, Bitmap logo, QbCodeConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = QbCodeConfiguration.QrCodeDefault;
            }
            MultiFormatWriter formatter = new MultiFormatWriter();
            Dictionary<EncodeHintType, object> configs = new Dictionary<EncodeHintType, object>()
            {
                {EncodeHintType.CHARACTER_SET,configuration.Charset },
                {EncodeHintType.ERROR_CORRECTION,ErrorCorrectionLevel.H },
                {EncodeHintType.MARGIN,configuration.Margin },
                {EncodeHintType.DISABLE_ECI,true },
            };
            BitMatrix matrix = formatter.encode(content, BarcodeFormat.QR_CODE, configuration.Width, configuration.Height, configs);
            BarcodeWriter<Bitmap> writer = new BarcodeWriter<Bitmap>();
            writer.Renderer = new BitmapRenderer();
            Bitmap result = writer.Write(matrix);
            if (logo != null)
            {
                result = DrawLogo(result, matrix, logo, configuration);
            }
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="GenerateQrCode(string, Bitmap, QbCodeConfiguration)"/>
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="logoFile">图标文件路径</param>
        /// <param name="configuration">配置数据</param>
        /// <returns></returns>
        public static Bitmap GenerateQrCode(string content,string logoFile,QbCodeConfiguration configuration = null)
        {
            Bitmap logo = null;
            if(!string.IsNullOrEmpty(logoFile) && File.Exists(logoFile))
            {
                logo = new Bitmap(logoFile);
            }
            return GenerateQrCode(content, logo, configuration);
        }

        /// <summary>
        /// <inheritdoc cref="GenerateQrCode(string, Bitmap, QbCodeConfiguration)"/>
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="configuration">配置数据</param>
        /// <returns></returns>
        public static Bitmap GenerateQrCode(string content,QbCodeConfiguration configuration = null)
        {
            return GenerateQrCode(content, logo: null, configuration);
        }

        /// <summary>
        /// 生成条形码
        /// </summary>
        /// <param name="content">条形码内容</param>
        /// <param name="configuration">配置数据</param>
        /// <returns></returns>
        public static Bitmap GenerateBarCode(string content,QbCodeConfiguration configuration = null)
        {
            if(configuration == null)
            {
                configuration = QbCodeConfiguration.BarCodeDefault;
            }
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.CODE_128;
            writer.Options = new EncodingOptions()
            {
                Height = configuration.Height,
                Width = configuration.Width,
                Margin = configuration.Margin
            };
            writer.Options.Hints[EncodeHintType.CHARACTER_SET] = configuration.Charset;
            Bitmap result = writer.Write(content);
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="ReadCode(Bitmap, string)"/>
        /// </summary>
        /// <param name="codeFile">条形码/二维码图片文件路径</param>
        /// <param name="charset">编码格式</param>
        /// <returns></returns>
        public static string ReadCode(string codeFile,string charset = "utf-8")
        {
            if(string.IsNullOrEmpty(codeFile) || !File.Exists(codeFile))
            {
                return null;
            }
            Bitmap bmp = new Bitmap(codeFile);
            return ReadCode(bmp, charset);
        }

        /// <summary>
        /// 读取条形码/二维码
        /// </summary>
        /// <param name="code">条形码/二维码图片数据</param>
        /// <param name="charset">编码格式</param>
        /// <returns></returns>
        public static string ReadCode(Bitmap code,string charset = "utf-8")
        {
            if(code == null)
            {
                throw new ArgumentNullException(nameof(code), "图片资源不存在!");
            }
            BarcodeReader reader = new BarcodeReader();
            reader.Options = new DecodingOptions()
            {
                CharacterSet = charset,
                PossibleFormats = new List<BarcodeFormat>()
                {
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.CODE_39
                },
                TryHarder = true,
                PureBarcode = true
            };
            reader.AutoRotate = true;
            reader.TryInverted = true;
            Result result = reader.Decode(code);
            if(result == null)
            {
                return null;
            }
            return result.Text;
        }
    }
}
