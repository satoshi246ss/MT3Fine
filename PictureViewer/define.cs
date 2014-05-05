using System;
//using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.Linq;
//using System.Text;
//using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Blob;
//using VideoInputSharp;
using System.Diagnostics;
//using System.Threading;
//using System.Net.Sockets;
//using System.Net;
using System.Runtime.InteropServices;
using System.IO;

namespace PictureViewer
{
    partial class Form1
    {
        //状態を表す定数
        const int TRUE = 0;
        const int FALSE = 1;
        //上の2つ状態を保持します
              int ImgSaveFlag = FALSE;

        //カメラの状態を表す定数
        const int STOP = 0;
        const int RUN = 1;
        const int SAVE = 2;
        //上の状態を保持します
        int States = 0;

        //状態を表す定数
        const int LOST = 0;
        const int DETECT = 1;
        const int DETECT_IN = 2;
        //上の2つ状態を保持します
        int Mode = LOST;

        //        int DarkMode = FALSE;

        // 時刻基準（BCB互換）
        DateTime TBASE = new DateTime(1899, 12, 30, 0, 0, 0);

        // メイン装置光軸座標
        int xoa = 357;  //320;
        int yoa = 291+3;//240;
        int roa = 10;

        const double fl = 50.0, ccdpx = 0.0056, ccdpy = 0.0056;
        public double dx, dy, theta_c = 0, dt = 1.0 / 120.0;
        public double az0, alt0, vaz0, valt0; // 流星位置、速度（前フレームの値）
        public double az, alt, vaz, valt; // 流星位置、速度
        public double az1, alt1, vaz1, valt1; // 流星位置、速度（次フレームの値）
        public double daz, dalt, dvaz, dvalt; // 流星位置差、速度差（前フレームからの）
        //public double
        position_mesure pos_mes = new position_mesure();


        // 観測開始からのフレーム番号
        int id = 0;
        DateTime LiveStartTime;

        const int MaxFrame = 512;
        const int WIDTH = 640;
        const int HEIGHT = 480;

        ImageData imgdata = new ImageData(WIDTH, HEIGHT);
        CircularBuffer fifo = new CircularBuffer(MaxFrame, WIDTH, HEIGHT);
        TIS.Imaging.ImageBuffer CurrentBuffer = null;
        IplImage img_dmk = new IplImage(WIDTH, HEIGHT, BitDepth.U8, 1);
        IplImage img_dark8 = Cv.LoadImage(@"C:\piccolo\MT3dmk_dark.bmp", LoadMode.GrayScale);
        IplImage img2 = new IplImage(WIDTH, HEIGHT, BitDepth.U8, 1);
        IplImage imgLabel = new IplImage(WIDTH, HEIGHT, CvBlobLib.DepthLabel, 1);
        CvBlobs blobs = new CvBlobs();
        CvFont font = new CvFont(FontFace.HersheyComplex, 0.5, 0.5);

        //CvWindow window1 = new CvWindow("DMK", WindowMode.AutoSize);
        int id_fr = 0;
        double gx, gy, max_val, kgx, kgy, kvx, kvy, sgx, sgy;
        int threshold_blob = 128; // 検出閾値（０－２５５）
        double threshold_min_area = 0.25; // 最小エリア閾値（最大値ｘ0.25)
        CvPoint2D64f max_centroid;
        uint max_label;
        CvBlob maxBlob;
        CvRect blob_rect;
        //double distance,distance_min, d_val;
        CvKalman kalman = Cv.CreateKalman(4, 2);
        int kalman_id = 0;
        // 観測値(kalman)
        CvMat measurement = new CvMat(2, 1, MatrixType.F32C1);
        CvMat correction;
        CvMat prediction;

        Stopwatch sw = new Stopwatch();
        long elapsed0 = 0, elapsed1 = 0, elapsed2 = 0;
        string fr_str;
        private BackgroundWorker worker;
        private BackgroundWorker worker_udp;
        Udp_kv udpkv = new Udp_kv();

        FSI_PID_DATA pid_data = new FSI_PID_DATA();
        MT_MONITOR_DATA mtmon_data = new MT_MONITOR_DATA();
        //        int mmFsiUdpPortMT3Fine = 24413;            // MT3IDS （受信）
        int mmFsiUdpPortMTmonitor = 24415;
        string mmFsiCore_i5 = "192.168.1.211";
        System.Net.Sockets.UdpClient udpc3 = null;
        DriveInfo cDrive = new DriveInfo("C");
        long diskspace;

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint timeEndPeriod(uint uMilliseconds);
        uint time_period = 10;
    }
}
