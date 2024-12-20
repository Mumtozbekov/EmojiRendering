using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using SkiaSharp;

namespace EmojiRendering
{
    public static class EmojiTextParser
    {
        static bool IsEmoji(string textElement)
        {
            int codepoint = char.ConvertToUtf32(textElement, 0);

            // Check for emoji ranges or multi-codepoint sequences
            if ((codepoint >= 0x1F600 && codepoint <= 0x1F64F) || // Emoticons
                (codepoint >= 0x1F300 && codepoint <= 0x1F5FF) || // Misc Symbols and Pictographs
                (codepoint >= 0x1F680 && codepoint <= 0x1F6FF) || // Transport and Map Symbols
                (codepoint >= 0x2600 && codepoint <= 0x26FF) ||   // Misc Symbols
                (codepoint >= 0x2700 && codepoint <= 0x27BF) ||   // Dingbats
                (codepoint >= 0x1F900 && codepoint <= 0x1F9FF) || // Supplemental Symbols and Pictographs
                (codepoint >= 0x1FA70 && codepoint <= 0x1FAFF) || // Symbols and Pictographs Extended-A
                textElement.Contains("\u200D"))                   // ZWJ Sequences
            {
                return true;
            }

            return false;
        }
        // Parse text into segments of emojis and non-emoji text
        public static List<TextSegment> ParseText(string text)
        {
            var segments = new List<TextSegment>();
            var enumerator = StringInfo.GetTextElementEnumerator(text);

            while (enumerator.MoveNext())
            {
                string element = enumerator.GetTextElement();
                if (IsEmoji(element))
                {
                    segments.Add(new TextSegment
                    {
                        Content = element,
                        IsEmoji = true
                    });
                }
                else
                {
                    segments.Add(new TextSegment
                    {
                        Content = element,
                    });
                }
            }


            //    // Refined emoji regex
            //    var emojiRegex = new Regex(
            //        @"[\u231A-\u231B]|" +                    // ⌚ Watch
            //        @"[\u23E9-\u23F3]|" +                   // ⏩ Fast-forward and timers
            //        @"[\u25FD-\u25FE]|" +                   // ◽ White squares
            //        @"[\u2600-\u26FF]|" +                   // ☀ Miscellaneous symbols
            //        @"[\u2702-\u27B0]|" +                   // ✂ Dingbats
            //        @"[\u2934-\u2935]|" +                   // ⤴ Arrows
            //        @"[\u2B05-\u2B07]|" +                   // ⬅ Arrows
            //        @"[\u2B1B-\u2B50]|" +                   // ⬛ Geometric shapes
            //        @"[\u1F004-\u1F0CF]|" +                 // 🀄 Mahjong tiles and playing cards
            //        @"[\u1F170-\u1F251]|" +                 // 🅰 Enclosed characters
            //        @"[\u1F300-\u1F5FF]|" +                 // 🌀 Miscellaneous Symbols and Pictographs
            //        @"[\u1F600-\u1F64F]|" +                 // 😀 Emoticons
            //        @"[\u1F680-\u1F6FF]|" +                 // 🚀 Transport and Map Symbols
            //        @"[\u1F700-\u1F77F]|" +                 // 🛅 Alchemical Symbols
            //        @"[\u1F780-\u1F7FF]|" +                 // 🟂 Geometric Shapes Extended
            //        @"[\u1F800-\u1F8FF]|" +                 // 🠆 Supplemental Arrows
            //        @"[\u1F900-\u1F9FF]|" +                 // 🤰 Supplemental Symbols and Pictographs
            //        @"[\u1FA70-\u1FAFF]|" +                 // 🩰 Symbols and Pictographs Extended
            //        @"[\u1FB00-\u1FBFF]|" +                 // 🬀 Symbols for Legacy Computing
            //        @"[\u1F1E6-\u1F1FF]{2}|" +              // 🇦🇺 Flags (regional indicators)
            //        @"[\uD83C-\uDBFF][\uDC00-\uDFFF]|" +    // Surrogate pairs for emojis
            //        @"\uD83C[\uDF00-\uDFFF]|\uD83D[\uDC00-\uDFFF]|\uD83E[\uDD00-\uDDFF]"+
            //        @"\p{Cs}|",                              // Surrogate pairs

            //        RegexOptions.Compiled);

            //    int lastIndex = 0;
            //    foreach (Match match in emojiRegex.Matches(text))
            //    {
            //        int matchStart = match.Index;

            //        // Add non-emoji text before the match
            //        if (matchStart > lastIndex)
            //        {
            //            segments.Add(new TextSegment
            //            {
            //                Content = text.Substring(lastIndex, matchStart - lastIndex),
            //                IsEmoji = false
            //            });
            //        }

            //        // Add the emoji
            //        segments.Add(new TextSegment
            //        {
            //            Content = match.Value,
            //            IsEmoji = true
            //        });

            //        // Update lastIndex to the end of the match
            //        lastIndex = matchStart + match.Value.Length;
            //    }

            //    // Add remaining text
            //    if (lastIndex < text.Length)
            //    {
            //        segments.Add(new TextSegment
            //        {
            //            Content = text.Substring(lastIndex),
            //            IsEmoji = false
            //        });
            //    }

            return segments;
        }


    }

    public class TextSegment
    {
        public string Content { get; set; }
        public bool IsEmoji { get; set; }
    }

    public static class SkiaExtensions{
        public static BitmapSource ToWpfImageSource(this SKImage skImage)
        {
            using var data = skImage.Encode();
            using var stream = data.AsStream();
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze(); // Allow cross-thread access
            return bitmap;
        }
    }
}
