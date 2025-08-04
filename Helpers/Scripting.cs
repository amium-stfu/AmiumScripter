using AmiumScripter;
using AmiumScripter.Core;
using AmiumScripter.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmiumScripter.Helpers
{
  
    public static class Functions
    {
        public static CancellationToken AToken()
        {
            return TokenManager.CreateToken();
        }

        public static string ToUnc(string uri)
        {
            string computerName = Environment.MachineName;
            string root = Path.GetPathRoot(uri);

            if (string.IsNullOrEmpty(root) || !uri.StartsWith(root))
            {
                throw new ArgumentException("File.Helpers.ToUnc uri invalid " + uri);
            }

            string rootlessPath = uri.Substring(root.Length).Replace(Path.DirectorySeparatorChar, '\\');
            return $@"\\{computerName}\{rootlessPath}";
        }
    }

    public static class ConversionExtensions

    {
        //String extensions for conversion of manipulation

        /// <summary>
        /// Converts a string to an integer by first converting it to a double.
        /// </summary>
        /// <param name="text">The input string to convert.</param>
        /// <returns>The integer value of the input string. Returns 0 if conversion fails.</returns>
        public static int ToInt(this string text)
        {
            return (int)Convert.ToDouble(text);
        }

        /// <summary>
        /// Converts a string to a <see cref="double"/> value, supporting various numeric formats and keywords.
        /// </summary>
        /// <param name="text">The input string to convert. Can represent numbers in decimal, hexadecimal, binary, or as predefined keywords.</param>
        /// <returns>
        /// A <see cref="double"/> value parsed from the input. Returns <see cref="double.NaN"/> if the input is null, empty, invalid, or unrecognized.
        /// </returns>
        public static double ToDouble(this string text)
        {
            if (text == null) return double.NaN;
            if (text == "") return double.NaN;
            text = text.Tolerant();
            if (text.Contains("#")) return double.NaN;
            if (text.Contains("INF")) return double.NaN;
            if (text.Contains("DEF")) return double.NaN;
            if (text.Contains("?")) return double.NaN;
            try
            {


                if (text.Contains("LONG"))
                {
                    return 32;
                }
                if (text.Contains("INT32"))
                {
                    return 32;
                }
                if (text.Contains("INT8"))
                {
                    return 8;
                }
                if (text.Contains("INT"))
                {
                    return 16;
                }
                if (text.Contains("BYTE"))
                {
                    return 8;
                }
                if (text.Contains("CHAR"))
                {
                    return 8;
                }
                if (text.Contains("H"))
                {
                    text = text.Replace("H", "");
                    return Convert.ToUInt32(text, 16);
                }
                if (text.Contains("0X"))
                {
                    text = text.Replace("0X", "");
                    return Convert.ToUInt32(text, 16);
                }
                if (text.Contains("X"))
                {
                    text = text.Replace("X", "");
                    return Convert.ToUInt32(text, 16);
                }
                if (text.Contains("B")) // muss nach h stehen sonst wird b falsch interpretierr
                {
                    text = text.Replace("B", "");
                    return Convert.ToUInt32(text, 2);
                }
            }
            catch
            {
                //    log.Fatal("%ToDouble1% " + text + " " + sender + " " + ex.Message);
            }


            string comma = "" + ("" + 1.1)[1];
            text = text.Replace(" ", "").Replace("#", "").Replace(",", comma).Replace(".", comma);
            if (text.Trim() == "NAN") return double.NaN;
            try
            {
                double d = 0;
                if (double.TryParse(text, out d))
                    return d;

                return double.NaN;//.Parse(text);
            }
            catch
            {
                //   log.Fatal("%ToDouble2% " + text + " " + sender + " " + ex.Message);
            }
            return double.NaN;
        }

        /// <summary>
        /// Converts the input text to uppercase, removes whitespace, and replaces specific German characters.
        /// </summary>
        /// <param name="text">The input string to process. Can be <see langword="null"/>.</param>
        /// <returns>
        /// A new string in uppercase, without spaces or tabs, with German umlauts and "ß" replaced by standard equivalents.
        /// Returns <see langword="null"/> if <paramref name="text"/> is <see langword="null"/>.
        /// </returns>
        public static string Tolerant(this string text)
        {
            if (text == null)
                return null;

            return text.ToUpper().Replace("\009", "").Replace(" ", "").Replace("Ä", "AE").Replace("Ö", "OE").Replace("Ü", "UE").Replace("ß", "SS");
        }

        /// <summary>
        /// Replaces all invalid filename characters with a period ('.').
        /// </summary>
        /// <param name="filename">The file name to sanitize. Cannot be <see langword="null"/>.</param>
        /// <returns>A safe file name with invalid characters replaced.</returns>
        public static string RemoveInvalidCharsFromFilename(this string filename)
        {
            string regex = String.Format("[{0}]", System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())));
            System.Text.RegularExpressions.Regex removeInvalidChars
                = new System.Text.RegularExpressions.Regex(regex
                    , System.Text.RegularExpressions.RegexOptions.Singleline
                    | System.Text.RegularExpressions.RegexOptions.Compiled
                    | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            return removeInvalidChars.Replace(filename, ".");
        }

        static int FindMatchingBracket(string str, int bracketPos)
        {
            if (bracketPos >= str.Length)
                return -1;

            if (str[bracketPos] != '[')
                return -1;

            bool inQuotes = false;
            int bracketCount = 0;
            for (int i = bracketPos; i < str.Length; i++)
            {
                if (str[i] == '\"')
                    inQuotes = !inQuotes;

                if (!inQuotes)
                {
                    if (str[i] == '[')
                        bracketCount++;
                    if (str[i] == ']')
                        bracketCount--;
                }
                if (str[i] == ']' && i > bracketPos && bracketCount == 0)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Removes redundant outer brackets from a string.
        /// </summary>
        /// <param name="orig">The input string. Can be null or empty.</param>
        /// <returns>The string without redundant outer brackets, or the original if none are found.</returns>
        public static string RemoveRedundantBrackets(string orig)
        {
            if (string.IsNullOrEmpty(orig))
                return orig;

            while (FindMatchingBracket(orig, 0) == orig.Length - 1)
                orig = orig.Substring(1, orig.Length - 2).Trim();

            return orig;
        }


        /// <summary>
        /// Converts a Base64-encoded string to an <see cref="Image"/> object.
        /// </summary>
        /// <remarks>This method attempts to decode the provided Base64 string into a byte array and then
        /// create an <see cref="Image"/> object from the byte array. If the input string is invalid or the conversion
        /// fails, the method returns <see langword="null"/> and logs the error to the debug output.</remarks>
        /// <param name="base64String">The Base64-encoded string representing the image data. This string must be a valid Base64-encoded
        /// representation of an image.</param>
        /// <returns>An <see cref="Image"/> object created from the Base64 string, or <see langword="null"/> if the input string
        /// is <see langword="null"/>, empty, or if the conversion fails.</returns>
        public static Image ToImage(this string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to convert Base64 string to Image: {ex.Message}");
                return null;
            }
        }


        //Color extensions for conversion or manipulation

        /// <summary>
        /// Adjusts the brightness of a color by a specified percentage.
        /// </summary>
        /// <param name="color">The original color to adjust.</param>
        /// <param name="percent">
        /// The brightness adjustment in percent. 
        /// Use positive values to lighten and negative values to darken the color.
        /// </param>
        /// <returns>
        /// A new <see cref="System.Drawing.Color"/> with adjusted brightness. 
        /// Returns black if the operation fails.
        /// </returns>
        public static System.Drawing.Color ChangeBrightness(this System.Drawing.Color color, double percent)
        {

            try
            {
                float correctionFactor = (float)percent / 100;

                float red = color.R;
                float green = color.G;
                float blue = color.B;

                if (correctionFactor < 0)
                {
                    correctionFactor = 1 + correctionFactor;
                    red *= correctionFactor;
                    green *= correctionFactor;
                    blue *= correctionFactor;

                    red = red < 0 ? 0 : red;
                    green = green < 0 ? 0 : green;
                    green = green < 0 ? 0 : green;
                }
                else
                {
                    red = (255 - red) * correctionFactor + red;
                    green = (255 - green) * correctionFactor + green;
                    blue = (255 - blue) * correctionFactor + blue;

                    red = red > 255 ? 255 : red;
                    green = green > 255 ? 255 : green;
                    green = green > 255 ? 255 : green;
                }


                return System.Drawing.Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
            }
            catch
            {
                Debug.WriteLine("Change colorbrightness failed");
                return System.Drawing.Color.Black;
            }

        }

        /// <summary>
        /// Changes the transparency (alpha value) of a color based on a percentage.
        /// </summary>
        /// <param name="color">The original color whose transparency will be adjusted.</param>
        /// <param name="percent">
        /// The desired transparency level as a percentage (0–100). 
        /// 0 means fully transparent, 100 means fully opaque.
        /// </param>
        /// <returns>
        /// A new <see cref="System.Drawing.Color"/> with adjusted alpha value.
        /// Returns black if the operation fails or input is out of range.
        /// </returns>
        public static System.Drawing.Color ChangeTransparency(this System.Drawing.Color color, double percent)
        {
            try
            {
                if (percent < 0 || percent > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be between 0 and 100.");
                }

                int alpha = (int)(percent * 255 / 100);

                return System.Drawing.Color.FromArgb(alpha, color.R, color.G, color.B);
            }
            catch (Exception ex)
            {
                // Log the error using your preferred logging framework
                Debug.WriteLine("Change transparency failed: " + ex.Message);
                return System.Drawing.Color.Black;
            }
        }

        /// <summary>
        /// Converts an object to a <see cref="System.Drawing.Color"/> if possible.
        /// </summary>
        /// <param name="color">
        /// The input object to convert. Supported types are <see cref="System.Drawing.Color"/> and <see cref="string"/>.
        /// Strings can be color names (e.g. "Red") or HTML hex codes (e.g. "#FF0000").
        /// </param>
        /// <returns>
        /// A <see cref="System.Drawing.Color"/> representing the input value.
        /// Returns black if the conversion fails or the input type is unsupported.
        /// </returns>
        public static System.Drawing.Color ToColor(this object color)
        {
            try
            {
                if (color is System.Drawing.Color)
                    return (System.Drawing.Color)color;

                if (color is string set)
                {
                    if (set.StartsWith("#"))
                        return System.Drawing.ColorTranslator.FromHtml(set);
                    else
                        return System.Drawing.Color.FromName(set);
                }

                // Log if the input is not a recognized type
                Debug.WriteLine($"ToColor: Unsupported color type - {color?.GetType().Name ?? "null"}");
                return System.Drawing.Color.Black;
            }
            catch (Exception ex)
            {
                // Log exceptions explicitly
                Debug.WriteLine($"ToColor: Error translating color. Exception: {ex.Message}");
                return System.Drawing.Color.Black;
            }
        }



        /// Image extensions for conversion or manipulation

        /// <summary>
        /// Converts a <see cref="System.Drawing.Image"/> to a Base64-encoded string using the specified image format.
        /// </summary>
        /// <param name="image">The image to convert. Must not be <c>null</c>.</param>
        /// <param name="format">The format in which to encode the image (e.g., PNG, JPEG).</param>
        /// <returns>
        /// A Base64-encoded string representation of the image, or <c>null</c> if the conversion fails.
        /// </returns>
        public static string ToBase64String(this Image image, System.Drawing.Imaging.ImageFormat format)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // Convert Image to byte[]
                    image.Save(ms, format);
                    byte[] imageBytes = ms.ToArray();
                    Debug.WriteLine("h:" + image.Height + " w: " + image.Width);

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine("Error: ConvertImageToBase64");
                return null;
            }
        }

        /// <summary>
        /// Determines whether a string is a valid Base64-encoded image representation.
        /// </summary>
        /// <param name="base64">The Base64 string to validate.</param>
        /// <returns>
        /// <c>true</c> if the string is a syntactically valid Base64 string (correct length and characters); otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBase64ImageStringIsValid(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return false;

            base64 = base64.Trim();

            if (base64.Length % 4 != 0)
                return false;

            return Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,2}$");
        }

        /// <summary>
        /// Converts an image to a Base64-encoded JPEG string, with optional scaling and rotation.
        /// </summary>
        /// <param name="image">The image to convert. Must not be <c>null</c>.</param>
        /// <param name="width">
        /// The target width in pixels. 
        /// - If set and <paramref name="height"/> is 0, the image is scaled proportionally based on the original aspect ratio.
        /// - If both <paramref name="width"/> and <paramref name="height"/> are greater than 0, the image is scaled non-proportionally (stretched).
        /// - If both are 0, the original image size is used.
        /// </param>
        /// <param name="height">
        /// The target height in pixels. 
        /// - If set and <paramref name="width"/> is 0, the image is scaled proportionally based on the original aspect ratio.
        /// - If both <paramref name="width"/> and <paramref name="height"/> are greater than 0, the image is scaled non-proportionally (stretched).
        /// - If both are 0, the original image size is used.
        /// </param>
        /// <param name="flipType">
        /// A <see cref="System.Drawing.RotateFlipType"/> value that determines how the image should be rotated or flipped
        /// before encoding. Use <c>RotateNoneFlipNone</c> for no transformation.
        /// </param>
        /// <returns>
        /// A Base64-encoded JPEG string representing the (optionally transformed and scaled) image,
        /// or <c>null</c> if the operation fails.
        /// </returns>
        public static string ToFormatedBase64String(this System.Drawing.Image image,
                                            int width = 0,
                                            int height = 0,
                                            RotateFlipType flipType = RotateFlipType.RotateNoneFlipNone,
                                            System.Drawing.Imaging.ImageFormat format = null)
        {
            if (image == null)
            {
                Debug.WriteLine("Image is null");
                return null;
            }

            try
            {
                // Fallback zu JPEG, falls kein Format übergeben wurde
                format ??= System.Drawing.Imaging.ImageFormat.Jpeg;

                using (var rotatedImage = (Image)image.Clone())
                {
                    rotatedImage.RotateFlip(flipType);

                    double originalWidth = rotatedImage.Width;
                    double originalHeight = rotatedImage.Height;
                    double aspectRatio = originalWidth / originalHeight;

                    double targetWidth = originalWidth;
                    double targetHeight = originalHeight;

                    if (width > 0 && height > 0)
                    {
                        targetWidth = width;
                        targetHeight = height;
                    }
                    else if (width > 0)
                    {
                        targetWidth = width;
                        targetHeight = Math.Round(width / aspectRatio);
                    }
                    else if (height > 0)
                    {
                        targetHeight = height;
                        targetWidth = Math.Round(height * aspectRatio);
                    }

                    using (Bitmap scaled = new Bitmap((int)targetWidth, (int)targetHeight))
                    using (Graphics g = Graphics.FromImage(scaled))
                    {
                        // Hintergrund ggf. transparent (für PNG) oder weiß (für JPEG)
                        if (format.Guid == System.Drawing.Imaging.ImageFormat.Png.Guid)
                            g.Clear(Color.Transparent);
                        else
                            g.Clear(Color.White);

                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        Rectangle destRect = GetBestFitRectangle(
                            rotatedImage.Width, rotatedImage.Height,
                            (int)targetWidth, (int)targetHeight);

                        g.DrawImage(rotatedImage, destRect);

                        return ToBase64String(scaled, format);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to convert image to Base64 string: {ex.Message}");
                return null;
            }
        }

        private static Rectangle GetBestFitRectangle(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            float ratio = Math.Min((float)targetWidth / sourceWidth, (float)targetHeight / sourceHeight);
            int drawWidth = (int)(sourceWidth * ratio);
            int drawHeight = (int)(sourceHeight * ratio);
            int offsetX = (targetWidth - drawWidth) / 2;
            int offsetY = (targetHeight - drawHeight) / 2;
            return new Rectangle(offsetX, offsetY, drawWidth, drawHeight);
        }
    }




}