using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Neuron
{
    public class MessageTypeSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate AudioTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return TextTemplate;

            var typeProp = item.GetType().GetProperty("Type");
            int type = typeProp != null ? (int)typeProp.GetValue(item) : 1;
            if (type == 1) return TextTemplate;

            var fileNameProp = item.GetType().GetProperty("FileName");
            string fileName = fileNameProp?.GetValue(item)?.ToString() ?? "";
            if (string.IsNullOrEmpty(fileName)) return FileTemplate;

            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            string[] imageExts = { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".webp" };
            string[] videoExts = { ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".webm" };
            string[] audioExts = { ".mp3", ".wav", ".ogg", ".wma", ".aac", ".flac" };

            if (imageExts.Contains(ext)) return ImageTemplate;
            if (videoExts.Contains(ext)) return VideoTemplate;
            if (audioExts.Contains(ext)) return AudioTemplate;
            return FileTemplate;
        }
    }
}