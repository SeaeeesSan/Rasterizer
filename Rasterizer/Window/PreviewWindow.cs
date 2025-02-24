using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace Rasterizer.Window;

public class PreviewWindow : System.Windows.Window
{
    private int Width { get; }
    private int Height { get; }
    private System.Windows.Window _window;
    
    private Thread _thread;
    private Image _image;

    public PreviewWindow(int width, int height)
    {
        Width = width;
        Height = height;
        
        _thread = new Thread(() =>
        {
            Application app = new Application();
            
            _window = new System.Windows.Window
            {
                Title = "My WPF Window",
                Width = width,
                Height = height
            };
            
            _image = new Image
            {
                Width = width,
                Height = height
            };
            
            _window.Content = _image;
            
            app.Run(_window);
        });
        _thread.SetApartmentState(ApartmentState.STA);
        
    }
    
    [System.STAThread]
    public void Show()
    {
       
        _thread.Start();
    }
    
    public void UpdateImage(System.Drawing.Bitmap bitmap)
    {
        if (_image != null)
        {
            _image.Dispatcher.Invoke(() =>
            {
                // bitmapがnullなら何もしない
                if (bitmap != null)
                {
                    _image.Source = ConvertBitmapToBitmapImage(bitmap);
                }
            }, DispatcherPriority.Background);
        }
    }
    
    public BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
    {
        using (var memory = new MemoryStream())
        {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;
            
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}