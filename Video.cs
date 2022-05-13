using AForge.Video.DirectShow;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Diagnostics;

namespace Video
{
    public partial class VideoInputHandler
    {

        private VideoCaptureDevice _device;
        private FilterInfoCollection LoaclWebCamsCollection;
        private VideoCapabilities selCapabilities;


        public VideoInputHandler()
        {
            _device = new VideoCaptureDevice();
        }


        public BitmapImage CreateBitmap(Image img)
        {
            MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        public void RefreshDevice(int selectedIndex)
        {
            LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            FilterInfoCollection f = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _device = new VideoCaptureDevice(LoaclWebCamsCollection[selectedIndex].MonikerString);
            _device.Start();
        }


        public VideoCaptureDevice GetDevice()
        {
            return _device;
        }

        public VideoCapabilities[] VideoCapabilities
        {
            get { return _device.VideoCapabilities; }   // get method
        }

        public void Start()
        {
            _device.Start();
        }

        public void Stop() {
            _device.Stop(); 
        }

        public int FrameRate
        {
            set { _device.DesiredFrameRate = value; }  // set method

        }

        public System.Drawing.Size FrameSize
        {
            set { _device.DesiredFrameSize = value; } // set method
        }

        public VideoCapabilities SelCapabilities   // property
        {
            get { return selCapabilities; }   // get method
            set { selCapabilities = value; }  // set method
        }

    }
}
