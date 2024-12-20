using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SkiaSharp;

namespace EmojiRendering
{
    public class EmojiTextBlock : TextBlock
    {

        static EmojiTextBlock()
        {

            FontSizeProperty.OverrideMetadata(typeof(EmojiTextBlock), new FrameworkPropertyMetadata(
                (double)FontSizeProperty.GetMetadata(typeof(TextBlock)).DefaultValue,
                (o, e) => (o as EmojiTextBlock)?.OnFontSizeChanged((double)e.NewValue)));

        }


        public new string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static new readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(EmojiTextBlock), new PropertyMetadata(string.Empty, OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmojiTextBlock emojiTextBlock)
                emojiTextBlock.UpdateInlines();
        }

        private void OnFontSizeChanged(double newValue)
        {
            if (_paint != null)
                _paint.TextSize = (float)newValue;
            UpdateInlines(false);
        }

        static SKTypeface _typeface;
        SKPaint _paint;
        static private Dictionary<string, BitmapSource> _emojiCache;

        public EmojiTextBlock()
        {
            if (_typeface == null)
                _typeface = SKTypeface.FromStream(Application.GetResourceStream(new Uri("pack://application:,,,/Fonts/apple.ttf", UriKind.RelativeOrAbsolute)).Stream);

            if (_emojiCache == null)
                _emojiCache = new();

            _paint = new SKPaint
            {
                TextSize = (float)FontSize,
                IsAntialias = true,
                Typeface = _typeface,
                Color = SKColors.Black
            };

        }


        private void UpdateInlines(bool useCache = true)
        {
            Inlines.Clear();

            if (string.IsNullOrEmpty(Text))
                return;

            // Parse text into segments

            var segments = EmojiTextParser.ParseText(Text);

            foreach (var segment in segments)
            {
                if (segment.IsEmoji)
                {
                    // Create an InlineUIContainer for the emoji
                    var image = CreateEmojiImage(segment.Content, useCache);
                    if (image != null)
                    {
                        var inlineContainer = new InlineUIContainer(image);
                        inlineContainer.BaselineAlignment = BaselineAlignment.Center;
                        Inlines.Add(inlineContainer);

                    }
                }
                else
                {
                    // Add text as a Run
                    Inlines.Add(new Run(segment.Content));
                }
            }
        }

        private System.Windows.Controls.Image CreateEmojiImage(string emoji, bool useCache)
        {
            var image = new Image
            {
                Stretch = Stretch.Uniform,
                Height = FontSize + 2,
                Width = FontSize + 2,
            };
            if (useCache && _emojiCache.TryGetValue(emoji, out var cachedImage))
            {
                image.Source = cachedImage;
                return image;
            }

            try
            {
                using var font = new SKFont(_typeface, (float)FontSize);
                var bounds = new SKRect();
                _paint.TextSize = (int)FontSize;
                _paint.MeasureText(emoji, ref bounds);

                var bitmap = new SKBitmap((int)Math.Ceiling(bounds.Right), (int)Math.Ceiling(bounds.Height));
                using var canvas = new SKCanvas(bitmap);
                float baseline = (float)Math.Ceiling(-bounds.Top);
                canvas.DrawText(emoji, 0, baseline, font, _paint);

                using var skImage = SKImage.FromBitmap(bitmap);
                var imgSource = skImage.ToWpfImageSource();
                image.Source = imgSource;

                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Unspecified);
                RenderOptions.SetClearTypeHint(image, ClearTypeHint.Auto);
                RenderOptions.SetCachingHint(image, CachingHint.Cache);
                _emojiCache[emoji] = imgSource; // Cache the rendered image
                return image;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
