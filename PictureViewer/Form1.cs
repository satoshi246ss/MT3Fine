﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Blob;
using VideoInputSharp;
using System.Diagnostics; 
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;

namespace PictureViewer
{
    public partial class Form1 : Form
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
        int xoa = 357;//320;
        int yoa = 291;//240;
        int roa = 10;

        const double fl=50.0, ccdpx=0.0056, ccdpy=0.0056;
        public double dx, dy, theta_c=0, dt=1.0/120.0;
        public double az0, alt0, vaz0, valt0; // 流星位置、速度（前フレームの値）
        public double az , alt , vaz , valt ; // 流星位置、速度
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
        double gx, gy, max_val,kgx,kgy,kvx,kvy,sgx,sgy;
        int    threshold_blob     = 128  ; // 検出閾値（０－２５５）
        double threshold_min_area = 0.25 ; // 最小エリア閾値（最大値ｘ0.25)
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

        public Form1()
        {
            InitializeComponent();

            timeBeginPeriod(time_period);

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            worker_udp = new BackgroundWorker();
            worker_udp.WorkerReportsProgress = true;
            worker_udp.WorkerSupportsCancellation = true;
            worker_udp.DoWork += new DoWorkEventHandler(worker_udp_DoWork);
            worker_udp.ProgressChanged += new ProgressChangedEventHandler(worker_udp_ProgressChanged);


            Pid_Data_Send_Init();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.worker_udp.RunWorkerAsync();

            // 有効な画像取り込みデバイスが選択されているかをチェック。
            if (!icImagingControl1.DeviceValid)
            {
                icImagingControl1.ShowDeviceSettingsDialog();

                if (!icImagingControl1.DeviceValid)
                {
                    MessageBox.Show("No device was selected.", "Display Buffer",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
              ///      this.Close();
                    return;
                }
            }

            ObsEndButton.Enabled = false;
            icImagingControl1.LiveDisplayDefault = false;
            icImagingControl1.LiveDisplayHeight = icImagingControl1.Height;
            icImagingControl1.LiveDisplayWidth = icImagingControl1.Width;

            icImagingControl1.LiveCaptureContinuous = true;
            icImagingControl1.LiveDisplay = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            img_dmk.Dispose();
            img_dark8.Dispose();
            img2.Dispose();
            imgLabel.Dispose();
            timeEndPeriod(time_period);
        }

        #region UDP
        // 別スレッド処理（UDP）
        private void worker_udp_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            //バインドするローカルポート番号
            int localPort = 24410;// 24411
            System.Net.Sockets.UdpClient udpc = null; ;
            try
            {
                udpc = new System.Net.Sockets.UdpClient(localPort);

            }
            catch (Exception ex)
            {
                //匿名デリゲートで表示する
                this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, ex.ToString() });
            }


            //文字コードを指定する
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            //データを送信するリモートホストとポート番号
            //string remoteHost = "localhost";
            //   string remoteHost = "192.168.1.204";
            //   int remotePort = 24404;
            //送信するデータを読み込む
            //string sendMsg = "test送信するデータ";
            //byte[] sendBytes = enc.GetBytes(sendMsg);
            //リモートホストを指定してデータを送信する
            // udpc.Send(sendBytes, sendBytes.Length, remoteHost, remotePort);

            string str;
            MOTOR_DATA_KV_SP kmd3    = new MOTOR_DATA_KV_SP();
            MOTOR_DATA_KV_SP kmd3pre = new MOTOR_DATA_KV_SP();
            int size = Marshal.SizeOf(kmd3);
            KV_DATA kd = new KV_DATA();
            int sizekd = Marshal.SizeOf(kd);

            //データを受信する
            System.Net.IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, localPort);
            while (bw.CancellationPending == false)
            {
                byte[] rcvBytes = udpc.Receive(ref remoteEP);
                if (rcvBytes.Length == sizekd)
                {
                    kd = ToStruct1(rcvBytes);
                    bw.ReportProgress(0, kd);
                    // MT3SFに転送
                    //データを送信するリモートホストとポート番号
                  //  string remoteHost = "localhost";
                    //string remoteHost = "192.168.1.204";
                  //  int remotePort = 24422;  // MT3SF
                  //  udpc.Send(rcvBytes, rcvBytes.Length, remoteHost, remotePort);
                    //udpc.Send(rcvBytes, 4, remoteHost, remotePort);
                }
                else if (rcvBytes.Length == size)
                {
                    kmd3 = ToStruct(rcvBytes);
                    if ((kmd3.cmd != kmd3pre.cmd || kmd3.t != kmd3pre.t))
                    {
                        if (kmd3.cmd == 1) //mmMove:1
                        {
                            Mode = DETECT;
                            //this.Invoke(new dlgSetColor(SetTimer), new object[] { timerSaveMainTime, RUN });
                            this.Invoke(new dlgSetColor(SetTimer), new object[] { timerSaveTimeOver, RUN });
                            //保存処理開始
                            if (this.States == RUN)
                            {
                                ImgSaveFlag = TRUE;
                                this.States = SAVE;
                                kalman_init();
                                pos_mes.init();
                            }
                        }
                        else if (kmd3.cmd == 16) //mmLost:16
                        {
                            //Mode = LOST;
                            //ButtonSaveEnd_Click(sender, e);
                        }
                        else if (kmd3.cmd == 17) // mmMoveEnd             17  // 位置決め完了
                        {
                            Mode = DETECT_IN;
                        }
                        else if (kmd3.cmd == 18) // mmTruckEnd            18  // 追尾完了
                        {
                            //保存処理終了
                            timerSaveTimeOver.Stop();
                            Mode = LOST;
                            ButtonSaveEnd_Click(sender, e);
                        }
                        else if (kmd3.cmd == 20) //mmData  20  // send fish pos data
                        {
                            //匿名デリゲートで表示する
                            //this.Invoke(new dlgSetColor(SetTimer), new object[] { timerSaveMainTime, STOP });
                            //this.Invoke(new dlgSetColor(SetTimer), new object[] { timerSaveMainTime, RUN }); // main timer 延長
                        }

                        str = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + " 受信したデータ(kmd3):" + kmd3.cmd + ":" + kmd3.az + ":" + kmd3.alt + "\n";
                        this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, str });
                        //bw.ReportProgress(0, kmd3);
                    }
                    kmd3pre = kmd3;
                }
                else
                {
                    string rcvMsg = enc.GetString(rcvBytes);
                    str = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "受信したデータ:[" + rcvMsg + "]\n";
                    this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, str });
                }

                //str = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "送信元アドレス:{0}/ポート番号:{1}/Size:{2}\n" + remoteEP.Address + "/" + remoteEP.Port + "/" + rcvBytes.Length;
                //this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, str });
            }

            //UDP接続を終了
            udpc.Close();
        }
        //メインスレッドでの処理
        private void worker_udp_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // 画面表示
            if ((id % 1) == 0)
            {
                //MOTOR_DATA_KV_SP kmd3 = (MOTOR_DATA_KV_SP)e.UserState;
                //string s = string.Format("UDP:[{0} {1} az:{2} alt:{3}]\n", kmd3.cmd, kmd3.t, kmd3.az, kmd3.alt);
                udpkv.kd = (KV_DATA)e.UserState;
                udpkv.cal_mt3();

                string s = string.Format("KV:[x2:{0} y2:{1} x2v:{2} y2v:{3} {4} {5}]\n",udpkv.x2pos , udpkv.y2pos, udpkv.x2v, udpkv.y2v, udpkv.binStr_status, udpkv.binStr_request);
                textBox1.Text = s;
            }
        }

        static byte[] ToBytes(MOTOR_DATA_KV_SP obj)
        {
            int size = Marshal.SizeOf(typeof(MOTOR_DATA_KV_SP));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        static byte[] ToBytes(FSI_PID_DATA obj)
        {
            int size = Marshal.SizeOf(typeof(FSI_PID_DATA));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public static MOTOR_DATA_KV_SP ToStruct(byte[] bytes)
        {
            GCHandle gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            MOTOR_DATA_KV_SP result = (MOTOR_DATA_KV_SP)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(MOTOR_DATA_KV_SP));
            gch.Free();
            return result;
        }

        public static KV_DATA ToStruct1(byte[] bytes)
        {
            GCHandle gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            KV_DATA result = (KV_DATA)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(KV_DATA));
            gch.Free();
            return result;
        }
        #endregion

        #region キャプチャー
        // 別スレッド処理（キャプチャー）
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;
            Stopwatch sw = new Stopwatch();
            string str;
            id = 0;

            //PID送信用UDP
            //バインドするローカルポート番号
            FSI_PID_DATA pid_data = new FSI_PID_DATA();
            int localPort = 24406;
            System.Net.Sockets.UdpClient udpc2 = null; ;
            try
            {
                udpc2 = new System.Net.Sockets.UdpClient(localPort);
            }
            catch (Exception ex)
            {
                //匿名デリゲートで表示する
                this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, ex.ToString() });
            }

            //videoInputオブジェクト
            const int DeviceID = 0;// 0;      // 3 (pro), 4(piccolo)  7(DMK)
            const int CaptureFps = 30;  // 30
            int interval = (int)(1000 / CaptureFps / 10);
            const int CaptureWidth  = 640;
            const int CaptureHeight = 480;
            // 画像保存枚数
            int mmFsiPostRec = 60;
            int save_counter = mmFsiPostRec;

            using (VideoInput vi = new VideoInput())
            {
                vi.SetIdealFramerate(DeviceID, CaptureFps);
                vi.SetupDevice(DeviceID, CaptureWidth, CaptureHeight);

                int width = vi.GetWidth(DeviceID);
                int height = vi.GetHeight(DeviceID);

                using (IplImage img = new IplImage(width, height, BitDepth.U8, 3))
                using (IplImage img_dark8 = Cv.LoadImage(@"C:\piccolo\MT3V_dark.bmp", LoadMode.GrayScale))
                //using (IplImage img_dark = new IplImage(width, height, BitDepth.U8, 3))
                using (IplImage img_mono = new IplImage(width, height, BitDepth.U8, 1))
                using (IplImage img2 = new IplImage(width, height, BitDepth.U8, 1))
                //                    using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
                using (CvFont font = new CvFont(FontFace.HersheyComplex, 0.5, 0.5))
                using (CvWindow window0 = new CvWindow("FIFO0", WindowMode.AutoSize))
                {
                    this.Size = new Size(width + 12, height + 148);
                    double min_val, max_val;
                    CvPoint min_loc, max_loc;
                    int size = 15;
                    int size2x = size / 2;
                    int size2y = size / 2;
                    //int num = 0;
                    double sigma = 3;
                    long elapsed0 = 0, elapsed1 = 0;
                    double framerate0 = 0, framerate1 = 0;
                    double alfa_fr = 0.999;
                    sw.Start();
                    while (bw.CancellationPending == false)
                    {
                        if (vi.IsFrameNew(DeviceID))
                        {
                            DateTime dn = DateTime.Now; //取得時刻
                            vi.GetPixels(DeviceID, img.ImageData, false, true);
                            //str = String.Format("ID:{0}", id);
                            //img.PutText(str, new CvPoint(10, 450), font, new CvColor(0, 255, 100));
                            Cv.CvtColor(img, img_mono, ColorConversion.BgrToGray);
                            Cv.Sub(img_mono, img_dark8, imgdata.img); // dark減算
                            imgdata.id = ++id;
                            imgdata.t = dn;
                            imgdata.ImgSaveFlag = !(ImgSaveFlag != 0); //int->bool変換
                            if (fifo.Count == MaxFrame - 1) fifo.EraseLast();
                            fifo.InsertFirst(imgdata);
                            // 位置検出
                            Cv.Smooth(imgdata.img, img2, SmoothType.Gaussian, size, 0, sigma, 0);
                            CvRect rect = new CvRect(1, 1, width - 2, height - 2);
                            Cv.SetImageROI(img2, rect);
                            Cv.MinMaxLoc(img2, out  min_val, out  max_val, out  min_loc, out  max_loc, null);
                            Cv.ResetImageROI(img2);
                            max_loc.X += 1; // 基準点が(1,1)のため＋１
                            max_loc.Y += 1;
                            window0.ShowImage(img2);

                            double m00, m10, m01, gx, gy;
                            size2x = size2y = size / 2;
                            if (max_loc.X - size2x < 0) size2x = max_loc.X;
                            if (max_loc.Y - size2y < 0) size2y = max_loc.Y;
                            if (max_loc.X + size2x >= width) size2x = width - max_loc.X - 1;
                            if (max_loc.Y + size2y >= height) size2y = height - max_loc.Y - 1;
                            rect = new CvRect(max_loc.X - size2x, max_loc.Y - size2y, size2x, size2y);
                            CvMoments moments;
                            Cv.SetImageROI(img2, rect);
                            Cv.Moments(img2, out moments, false);
                            Cv.ResetImageROI(img2);
                            m00 = Cv.GetSpatialMoment(moments, 0, 0);
                            m10 = Cv.GetSpatialMoment(moments, 1, 0);
                            m01 = Cv.GetSpatialMoment(moments, 0, 1);
                            gx = max_loc.X - size2x + m10 / m00;
                            gy = max_loc.Y - size2y + m01 / m00;

                            // 画面表示
                            str = String.Format("ID:{0:D2} ", id) + dn.ToString("yyyyMMdd_HHmmss_fff") + String.Format(" ({0,000:F2},{1,000:F2}) ({2,000:0},{3,000:0})({4,0:F1})", gx, gy, max_loc.X, max_loc.Y, max_val);
                            if (imgdata.ImgSaveFlag) str += " True";
                            img.PutText(str, new CvPoint(10, 20), font, new CvColor(0, 255, 100));
                            img.Circle(max_loc, 10, new CvColor(255, 255, 100));
                            bw.ReportProgress(0, img);

                            // PID data send for UDP
                            if (ImgSaveFlag == TRUE)
                            {
                                //データを送信するリモートホストとポート番号
                                string remoteHost = "192.168.1.206";
                                int remotePort = 24410;
                                //送信するデータを読み込む
                                ++(pid_data.id);
                                pid_data.swid = 24402;          // 仮　mmFsiUdpPortFSI2
                                pid_data.t = TDateTimeDouble(DateTime.Now);
                                pid_data.dx = (float)(gx - xoa);
                                pid_data.dy = (float)(gy - yoa);
                                byte[] sendBytes = ToBytes(pid_data);
                                //リモートホストを指定してデータを送信する
                                udpc2.Send(sendBytes, sendBytes.Length, remoteHost, remotePort);
                            }

                            // 処理速度
                            elapsed0 = sw.ElapsedTicks - elapsed1; // 1frameのticks
                            elapsed1 = sw.ElapsedTicks;
                            framerate0 = alfa_fr * framerate1 + (1 - alfa_fr) * (Stopwatch.Frequency / (double)elapsed0);
                            framerate1 = framerate0;

                            str = String.Format("fr time = {0}({1}){2:F1}", sw.Elapsed, id, framerate0); //," ", sw.ElapsedMilliseconds);
                            //匿名デリゲートで現在の時間をラベルに表示する
                            this.Invoke(new dlgSetString(ShowText), new object[] { textBox1, str });
                            //img.ToBitmap(bitmap);
                            //pictureBox1.Refresh();

                        }
                        Application.DoEvents();
                        Thread.Sleep(interval);
                    }
                    this.States = STOP;
                    this.Invoke(new dlgSetColor(SetColor), new object[] { ObsStart, this.States });
                    this.Invoke(new dlgSetColor(SetColor), new object[] { ObsEndButton, this.States });
                    vi.StopDevice(DeviceID);
                    udpc2.Close();
                }
            }
        }
        //BCB互換TDatetime値に変換
        private double TDateTimeDouble(DateTime t)
        {
            TimeSpan ts = t - TBASE;   // BCB 1899/12/30 0:0:0 からの経過日数
            return (ts.TotalDays);
        }

        //現在の時刻の表示と、タイマーの表示に使用されるデリゲート
        delegate void dlgSetString(object lbl, string text);
        //ボタンのカラー変更に使用されるデリゲート
        delegate void dlgSetColor(object lbl, int state);

        //デリゲートで別スレッドから呼ばれてラベルに現在の時間又は
        //ストップウオッチの時間を表示する
        private void ShowRText(object sender, string str)
        {
            RichTextBox rtb = (RichTextBox)sender;　//objectをキャストする
            rtb.AppendText(str);
        }
        private void ShowText(object sender, string str)
        {
            TextBox rtb = (TextBox)sender;　//objectをキャストする
            rtb.Text = str;
        }
        private void SetColor(object sender, int sta)
        {
            Button rtb = (Button)sender;　//objectをキャストする
            if (sta == RUN)
            {
                rtb.BackColor = Color.Red;
            }
            else if (sta == STOP)
            {
                rtb.BackColor = Color.FromKnownColor(KnownColor.Control);
            }
        }
        private void SetTimer(object sender, int sta)
        {
            System.Windows.Forms.Timer tim = (System.Windows.Forms.Timer)sender;　//objectをキャストする
            if (sta == RUN)
            {
                tim.Start();
            }
            else if (sta == STOP)
            {
                tim.Stop();
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // 画面表示
            if ((id % 1) == 0)
            {
                IplImage image = (IplImage)e.UserState;

                Cv.Circle(image, new CvPoint(xoa, yoa), roa, new CvColor(0, 255, 0));
                Cv.Line(image, new CvPoint(xoa + roa, yoa + roa), new CvPoint(xoa - roa, yoa - roa), new CvColor(0, 255, 0));
                Cv.Line(image, new CvPoint(xoa - roa, yoa + roa), new CvPoint(xoa + roa, yoa - roa), new CvColor(0, 255, 0));

                pictureBox1.Image = image.ToBitmap();
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                this.ObsStart.BackColor = Color.FromKnownColor(KnownColor.Control);
                this.ObsEndButton.BackColor = Color.FromKnownColor(KnownColor.Control);
            }
            this.States = STOP;
        }
        #endregion

        private void ShowButton_Click(object sender, EventArgs e)
        {
            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    pictureBox1.Load(openFileDialog1.FileName);
            //}

            dx = 1; dy = 2;
            double azc = 90, altc = 30;
            udpkv.cxcy2azalt(dx, dy, azc, altc, udpkv.mt3mode, theta_c, fl, ccdpx, ccdpx, ref az, ref alt);

            label_frame_rate.Text = String.Format("FrDrop:{0} / {1}", icImagingControl1.DeviceCountOfFramesDropped, icImagingControl1.DeviceCountOfFramesNotDropped);
            Pid_Data_Send();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            /* if (checkBox1.Checked)
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            else
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
             */
        }

        private void ObsEndButton_Click(object sender, EventArgs e)
        {
            icImagingControl1.LiveStop();
            this.ObsStart.BackColor = Color.FromKnownColor(KnownColor.Control);
            this.States = STOP;
            // BackgroundWorkerを停止.
            if (worker.IsBusy)
            {
                //this.worker.CancelAsync();
                this.ObsEndButton.BackColor = Color.Red;
            }
        }

        private void ObsStart_Click(object sender, EventArgs e)
        {
            icImagingControl1.LiveStart();
            LiveStartTime = DateTime.Now;
            this.ObsStart.BackColor = Color.Red;
            this.States = RUN;
            this.ObsEndButton.Enabled = true;
            // BackgroundWorkerを開始
            if (!worker.IsBusy)
            {
                //this.worker.RunWorkerAsync();
                this.ObsStart.BackColor = Color.Red;
                this.States = RUN;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (this.States == RUN)
            {
                /*  int NoCapDev = 8;
                  DateTime dtNow = DateTime.Now;
                  string savedir = @"C:\piccolo\";
                  string fn = savedir + dtNow.ToString("yyyyMMdd") + @"\";
                  // フォルダ (ディレクトリ) が存在しているかどうか確認する
                  if (!System.IO.Directory.Exists(fn))
                  {
                      System.IO.Directory.CreateDirectory(fn);
                  }
                  fn += string.Format("{00}_", NoCapDev) + dtNow.ToString("yyyyMMdd_HHmmss_fff") + ".avi";
                  */
                // AVI保存
                //icImagingControl1.LiveDisplay = false;
                ///icImagingControl1.AviStartCapture(fn, "RGB8");

                ImgSaveFlag = TRUE;
                this.States = SAVE;
            }
        }

        private void ButtonSaveEnd_Click(object sender, EventArgs e)
        {
            /*
            if (this.States == SAVE)
            {
                ///icImagingControl1.AviStopCapture();
                this.buttonSave.BackColor = Color.FromKnownColor(KnownColor.Control);
            } */
            ImgSaveFlag = FALSE;
            this.States = RUN;
            gx = -1; gy = -1;
        }

        private void timerSaveTimeOver_Tick(object sender, EventArgs e)
        {
            timerSaveTimeOver.Stop();
            Mode = LOST;
            ButtonSaveEnd_Click(sender, e);
        }

        private void buttonMakeDark_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox1.Checked = false;
                //DarkMode = TRUE;
                timerDisplay.Enabled = true;
            }
        }

        private void timerMakeDark_Tick(object sender, EventArgs e)
        {
            timerDisplay.Enabled = false;
            //DarkMode = FALSE;
        }

        private void timerObsOnOff_Tick(object sender, EventArgs e)
        {
            TimeSpan nowtime = DateTime.Now - DateTime.Today;
            TimeSpan endtime = new TimeSpan(7, 0, 0);
            TimeSpan starttime = new TimeSpan(17, 3, 0);

            if (nowtime.CompareTo(endtime) >= 0 && nowtime.CompareTo(starttime) <= 0)
            {
                // DayTime
                if (this.States == RUN && checkBoxObsAuto.Checked)
                {
                    ObsEndButton_Click(sender, e);
                    timerWaitShutdown.Start();
                }
            }
            else
            {
                //NightTime
                if (this.States == STOP && checkBoxObsAuto.Checked)
                {
                    ObsStart_Click(sender, e);
                }
            }
        }
        private void timerWaitShutdown_Tick(object sender, EventArgs e)
        {
            shutdown(sender, e);
        }

        /// <summary>
        /// システムシャットダウン
        /// </summary>
        /// <param name="capacity">シャットダウン</param>
        private void shutdown(object sender, EventArgs e)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "shutdown.exe";
            //コマンドラインを指定
            psi.Arguments = "-s -f";
            //ウィンドウを表示しないようにする（こうしても表示される）
            psi.CreateNoWindow = true;
            //起動
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);
        }

        private void timerDisplay_Tick(object sender, EventArgs e)
        {
            icImagingControl1.Display();
            //window1.ShowImage(img2); //img2
            label_frame_rate.Text = fr_str;

            // 保存用データをキューへ
            if (ImgSaveFlag == FALSE && this.States == RUN && CurrentBuffer != null)
            {
                gx = gy = max_val = 0.0;
                imgdata_push_FIFO();
            }

            if (this.States == SAVE)
            {
                this.buttonSave.BackColor = Color.Red;
                this.buttonSave.Enabled = false;
                this.ButtonSaveEnd.Enabled = true;
            }
            if (this.States == RUN)
            {
                this.buttonSave.BackColor = Color.FromKnownColor(KnownColor.Control);
                this.buttonSave.Enabled = true;
                this.ButtonSaveEnd.Enabled = false;
                this.ObsStart.BackColor = Color.Red;
                this.ObsStart.Enabled = false;
                this.ObsEndButton.Enabled = true;
            }
            if (this.States == STOP)
            {
                this.buttonSave.BackColor = Color.FromKnownColor(KnownColor.Control);
                this.buttonSave.Enabled = false;
                this.ButtonSaveEnd.Enabled = false;
                this.ObsStart.BackColor = Color.FromKnownColor(KnownColor.Control);
                this.ObsStart.Enabled = true;
                this.ObsEndButton.Enabled = false;
            }
        }

        // 毎フレーム呼び出し(120fr/s)
        private void icImagingControl1_ImageAvailable(object sender, TIS.Imaging.ICImagingControl.ImageAvailableEventArgs e)
        {
            double framerate0 = 0, framerate1 = 0 ;//, alfa_fr = 0.99;
            sw.Start();
            try
            {
                CurrentBuffer = icImagingControl1.ImageBuffers[e.bufferIndex];
                img_dmk.ImageData = CurrentBuffer.GetImageDataPtr();
                //icImagingControl1.DisplayImageBuffer(CurrentBuffer);
                
                elapsed0 = sw.ElapsedTicks;
                Cv.Copy(img_dmk, imgdata.img); //画像コピー

                // 保存用データをキューへ
                if (ImgSaveFlag == TRUE)
                {
                    #region 位置検出1  //minmax
                    /*
                    double min_val;
                    CvPoint min_loc, max_loc;
                    int size = 15;
                    int size2x = size / 2;
                    int size2y = size / 2;
                    double sigma = 3;

                    Cv.Smooth(imgdata.img, img2, SmoothType.Gaussian, size, 0, sigma, 0);
                    CvRect rect = new CvRect(1, 1, WIDTH - 2, HEIGHT - 2);
                    Cv.SetImageROI(img2, rect);
                    Cv.MinMaxLoc(img2, out  min_val, out  max_val, out  min_loc, out  max_loc, null);
                    Cv.ResetImageROI(img2);
                    max_loc.X += 1; // 基準点が(1,1)のため＋１
                    max_loc.Y += 1;

                    double m00, m10, m01;
                    if (max_loc.X - size2x < 0) size2x = max_loc.X;
                    if (max_loc.Y - size2y < 0) size2y = max_loc.Y;
                    if (max_loc.X + size2x >= WIDTH) size2x = WIDTH - max_loc.X - 1;
                    if (max_loc.Y + size2y >= HEIGHT) size2y = HEIGHT - max_loc.Y - 1;
                    rect = new CvRect(max_loc.X - size2x, max_loc.Y - size2y, size, size);
                    CvMoments moments;
                    Cv.SetImageROI(img2, rect);
                    Cv.Moments(img2, out moments, false);
                    Cv.ResetImageROI(img2);
                    m00 = Cv.GetSpatialMoment(moments, 0, 0);
                    m10 = Cv.GetSpatialMoment(moments, 1, 0);
                    m01 = Cv.GetSpatialMoment(moments, 0, 1);
                    gx = max_loc.X - size2x + m10 / m00;
                    gy = max_loc.Y - size2y + m01 / m00;
                    */
                    #endregion

                    #region 位置検出2  //Blob
                    Cv.Threshold(imgdata.img, img2, threshold_blob, 255, ThresholdType.Binary); //2ms
                    blobs.Label(img2, imgLabel); //1.4ms
                    max_label = blobs.GreaterBlob();
                    if (max_label > 0)
                    {
                        uint min_area = Math.Max(2,(uint)(threshold_min_area * blobs[max_label].Area) );
                        blobs.FilterByArea(min_area, uint.MaxValue); //0.001ms  面積がmin_area未満のblobを削除
                    }

                    #region OldBlob
                    //max_label = blobs.GreaterBlob();
                    //if (blobs.Count > 1 && gx >= 0)
                    //{
                    //    uint min_area = (uint)(threshold_min_area * blobs[max_label].Area);
                    //    blobs.FilterByArea(min_area, uint.MaxValue); //0.001ms

                    //    // 最適blobの選定（area大　かつ　前回からの距離小）
                    //    double x = blobs[max_label].Centroid.X;
                    //    double y = blobs[max_label].Centroid.Y;
                    //    //CvRect rect;
                    //    distance_min = ((x - gx) * (x - gx) + (y - gy) * (y - gy)); //Math.Sqrt(
                    //    foreach (var item in blobs)
                    //    {
                    //        //Console.WriteLine("{0} | Centroid:{1} Area:{2}", item.Key, item.Value.Centroid, item.Value.Area);
                    //        x = item.Value.Centroid.X;
                    //        y = item.Value.Centroid.Y;
                    //        //rect = item.Value.Rect;
                    //        distance = ((x - gx) * (x - gx) + (y - gy) * (y - gy)); //将来はマハラノビス距離
                    //        if (distance < distance_min)
                    //        {
                    //            d_val = (item.Value.Area) / max_val;
                    //            if (distance <= 25) //近距離(5pix)
                    //            {
                    //                if (d_val >= 0.4)//&& d_val <= 1.2)
                    //                {
                    //                    max_label = item.Key;
                    //                    distance_min = distance;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (d_val >= 0.8 && d_val <= 1.5)
                    //                {
                    //                    max_label = item.Key;
                    //                    distance_min = distance;
                    //                }
                    //            }
                    //        }
                    //        //w.WriteLine("{0} {1} {2} {3} {4}", dis, dv, i, item.Key, item.Value.Area);
                    //    }
                    //}
                    #endregion

                    max_label = pos_mes.mesure(blobs);
                    // ターゲット有り
                    if (max_label > 0)
                    {
                        maxBlob = blobs[max_label];
                        max_centroid = maxBlob.Centroid;
                        gx = sgx = max_centroid.X;
                        gy = sgy = max_centroid.Y;
                        max_val = maxBlob.Area;
                        blob_rect = maxBlob.Rect;

                        // 観測値(kalman)
                        //float[] m = { (float)(gx - xoa), (float)(gy - yoa) };
                        //CvMat measurement = Cv.Mat(2, 1, MatrixType.F32C1, m);
                        measurement.Set2D(0, 0, (float)(gx - xoa));
                        measurement.Set2D(1, 0, (float)(gy - yoa));
                        if (kalman_id == 0 || udpkv.kalman_init_flag != 0)
                        {
                            udpkv.kalman_init_flag = 0;
                            kalman_init();
                            // 初期値設定
                            double errcov = 1.0;
                            kalman.StatePost.Set1D(0, measurement.Get1D(0));
                            kalman.StatePost.Set1D(1, measurement.Get1D(1));
                            Cv.SetIdentity(kalman.ErrorCovPost, Cv.RealScalar(errcov));
                        }
                        kalman_id++;
                        // 修正フェーズ(kalman)
                        correction = Cv.KalmanCorrect(kalman, measurement);
                        // 予測フェーズ(kalman)
                        prediction = Cv.KalmanPredict(kalman);
                        kgx = prediction.DataArraySingle[0] + xoa;
                        kgy = prediction.DataArraySingle[1] + yoa;
                        kvx = prediction.DataArraySingle[2];
                        kvy = prediction.DataArraySingle[3];
                        // カルマン　or　観測重心　の選択
                        //if ((Math.Abs(kgx - gx) + Math.Abs(kgy - gy) < 15))  // 
                        if (kalman_id > 3)
                        {
                            sgx = kgx;
                            sgy = kgy;
                            //imgSrc.Circle(new CvPoint((int)(prediction.DataArraySingle[0] + xoa), (int)(prediction.DataArraySingle[1] + yoa)), 30, new CvColor(100, 100, 255));
                            //w2.WriteLine("{0:D3} {1:F2} {2:F2} {3:F2} {4:F2} {5} {6} {7}", i, max_centroid.X, max_centroid.Y, prediction.DataArraySingle[0] + xc, prediction.DataArraySingle[1] + yc, vm, dx, dy);
                        }
                        // 目標位置からの誤差(pix)からターゲットの位置を計算 速度はカルマン推定値
                        dx = sgx; dy = sgy;
                        udpkv.cxcy2azalt(-(gx-xoa)      , -(gy-yoa)      , udpkv.az2_c, udpkv.alt2_c, udpkv.mt3mode, theta_c, fl, ccdpx, ccdpx, ref az, ref alt);
                        udpkv.cxcy2azalt(-(gx-xoa + kvx), -(dy-yoa + kvy), udpkv.az2_c, udpkv.alt2_c, udpkv.mt3mode, theta_c, fl, ccdpx, ccdpx, ref az1, ref alt1);
                        vaz = udpkv.vaz2_kv + (az1 - az) / dt;
                        valt = udpkv.valt2_kv + (alt1 - alt) / dt;

                        //daz = az - udpkv.az2_c; dalt = alt - udpkv.alt2_c;             //位置誤差
                        //dvaz = (daz - daz1) / dt; dvalt = (dalt - dalt1) / dt;        //速度誤差
                        //diff_vaz = (az - az_pre1) / dt; diff_valt = (alt - alt_pre1) / dt; //速度差

                        az0 = az; alt0 = alt;

                        // 観測データ送信
                        Pid_Data_Send();
                    }
                    else
                    {
                        gx = gy = 0;
                        sgx = sgy = 0;
                        max_val = 0;
                    } 
                    #endregion

                    elapsed1 = sw.ElapsedTicks;
                    imgdata_push_FIFO();
                }
            }
            catch (Exception ex)
            {
                //匿名デリゲートで表示する
                this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, ex.ToString() });
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }

            elapsed2 = sw.ElapsedTicks; sw.Stop(); sw.Reset();
            // 処理速度
            //framerate0 = alfa_fr * framerate1 + (1 - alfa_fr) * (Stopwatch.Frequency / (double)elapsed2);
            framerate0 = ++id_fr / (this.icImagingControl1.ReferenceTimeCurrent - this.icImagingControl1.ReferenceTimeStart);
            framerate1 = framerate0;

            double sf = (double)Stopwatch.Frequency / 1000; //msec
            fr_str = String.Format("ID:{0,5:D1} L0:{1,4:F2} L1:{2,4:F2} L2:{3,4:F2} fr:{4,5:F1}", id_fr, elapsed0 / sf, elapsed1 / sf, elapsed2 / sf, framerate0);
            // 文字入れ
            //string st = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            //img_dmk.PutText(st, new CvPoint(10, 470), font, new CvColor(0, 100, 100));
        }

        /// <summary>
        /// kalman 初期化ルーチン
        /// </summary>
        /// <param name="elem">読み出した要素</param> 
        private void kalman_init()
        {
            // 初期化(kalman)
            kalman_id = 0;
            Cv.SetIdentity(kalman.MeasurementMatrix, Cv.RealScalar(1.0));
            Cv.SetIdentity(kalman.ProcessNoiseCov, Cv.RealScalar(1e-4));
            Cv.SetIdentity(kalman.MeasurementNoiseCov, Cv.RealScalar(0.001));
            Cv.SetIdentity(kalman.ErrorCovPost, Cv.RealScalar(1.0));
            measurement.Zero();

            // 等速直線運動モデル(kalman)
            kalman.TransitionMatrix.Set2D(0, 0, 1.0f);
            kalman.TransitionMatrix.Set2D(0, 1, 0.0f);
            kalman.TransitionMatrix.Set2D(0, 2, 1.0f);
            kalman.TransitionMatrix.Set2D(0, 3, 0.0f);

            kalman.TransitionMatrix.Set2D(1, 0, 0.0f);
            kalman.TransitionMatrix.Set2D(1, 1, 1.0f);
            kalman.TransitionMatrix.Set2D(1, 2, 0.0f);
            kalman.TransitionMatrix.Set2D(1, 3, 1.0f);

            kalman.TransitionMatrix.Set2D(2, 0, 0.0f);
            kalman.TransitionMatrix.Set2D(2, 1, 0.0f);
            kalman.TransitionMatrix.Set2D(2, 2, 1.0f);
            kalman.TransitionMatrix.Set2D(2, 3, 0.0f);

            kalman.TransitionMatrix.Set2D(3, 0, 0.0f);
            kalman.TransitionMatrix.Set2D(3, 1, 0.0f);
            kalman.TransitionMatrix.Set2D(3, 2, 0.0f);
            kalman.TransitionMatrix.Set2D(3, 3, 1.0f);
        }

        // FIFO pushルーチン
        private void imgdata_push_FIFO()
        {
            // 文字入れ
            //String str = String.Format("ID:{0,6:D1} ", imgdata.id) + imgdata.t.ToString("yyyyMMdd_HHmmss_fff") + String.Format(" ({0,6:F1},{1,6:F1})({2,6:F1})", gx, gy, max_val);
            //img_dmk.PutText(str, new CvPoint(10, 460), font, new CvColor(255, 100, 100));

            //try
            //{
            //Cv.Sub(img_dmk, img_dark8, imgdata.img); // dark減算  フレーム遅れのため、取得時に移動
            //Cv.Copy(img_dmk, imgdata.img);
            imgdata.id = id_fr;
            imgdata.t = LiveStartTime.AddSeconds(CurrentBuffer.SampleEndTime);
            imgdata.ImgSaveFlag = !(ImgSaveFlag != 0); //int->bool変換
            imgdata.gx  = gx;
            imgdata.gy  = gy;
            imgdata.kgx = kgx;
            imgdata.kgy = kgy;
            imgdata.kvx = kvx;
            imgdata.kvy = kvy;
            imgdata.vmax = max_val;
            imgdata.blobs = blobs;
            imgdata.udpkv1 = (Udp_kv) udpkv.Clone();
            imgdata.az   = az;
            imgdata.alt  = alt;
            imgdata.vaz  = vaz;
            imgdata.valt = valt;
            imgdata.detect_mode = pid_data.kalman_state;
            if (fifo.Count == MaxFrame - 1) fifo.EraseLast();
            fifo.InsertFirst(imgdata);
            /*}
            catch (Exception ex)
            {
                //匿名デリゲートで表示する
                this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, ex.ToString() });
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }*/
        }

        // PID data送信ルーチン
        private void Pid_Data_Send_Init()
        {
            //PID送信用UDP
            //バインドするローカルポート番号
            //FSI_PID_DATA pid_data = new FSI_PID_DATA();
            int localPort = 24412;  //mmFsiUdpPortMT3FineS
            //System.Net.Sockets.UdpClient udpc3 = null ;
            try
            {
                udpc3 = new System.Net.Sockets.UdpClient(localPort);
            }
            catch (Exception ex)
            {
                //匿名デリゲートで表示する
                this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, ex.ToString() });
            }
        }
        // PID data送信ルーチン
        private void Pid_Data_Send()
        {
            // PID data send for UDP
            //データを送信するリモートホストとポート番号
            string remoteHost = "192.168.1.206";
            int remotePort = 24410; // KV1000SpCam
            //送信するデータを読み込む
            ++(pid_data.id);
            pid_data.swid = 24412;          // mmFsiUdpPortMT3FineS
            pid_data.t = TDateTimeDouble(LiveStartTime.AddSeconds(CurrentBuffer.SampleEndTime));//(DateTime.Now);
            pid_data.vmax = (ushort)(max_val);
            pid_data.dx   = (float)(sgx - xoa);
            pid_data.dy   = (float)(sgy - yoa);
            pid_data.az   = (float)(az);
            pid_data.alt  = (float)(alt);
            pid_data.vaz  = (float)(vaz);
            pid_data.valt = (float)(valt);
            if (udpkv.mt3state_move == 0 && udpkv.mt3state_center == 0 && udpkv.mt3state_truck != 0) pid_data.kalman_state = 1;
            else pid_data.kalman_state = 0;
 
            byte[] sendBytes = ToBytes(pid_data);

            try
            {
                //リモートホストを指定してデータを送信する
                udpc3.Send(sendBytes, sendBytes.Length, remoteHost, remotePort);
            }
            catch (Exception ex)
            {
                //匿名デリゲートで表示する
                this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, ex.ToString() });
            }
        }

        /// <summary>
        /// MTmon status 送信ルーチン
        /// </summary>
        /// <remarks>
        /// MTmon status send
        /// </remarks>
        private void MTmon_Data_Send(object sender)
        {
            // MTmon status for UDP
            //データを送信するリモートホストとポート番号
            string remoteHost = mmFsiCore_i5;
            int remotePort = mmFsiUdpPortMTmonitor;
            //送信するデータを読み込む
            mtmon_data.id = 4; //MT3Fine
            mtmon_data.diskspace = (int)(diskspace / (1024 * 1024 * 1024));
            mtmon_data.obs = (byte)this.States;

            //mtmon_data.obs = this.States ; 
            byte[] sendBytes = ToBytes(mtmon_data);

            try
            {
                //リモートホストを指定してデータを送信する
                udpc3.Send(sendBytes, sendBytes.Length, remoteHost, remotePort);
            }
            catch (Exception ex)
            {
                //匿名デリゲートで表示する
                this.Invoke(new dlgSetString(ShowRText), new object[] { richTextBox1, ex.ToString() });
            }
        }
        static byte[] ToBytes(MT_MONITOR_DATA obj)
        {
            int size = Marshal.SizeOf(typeof(MT_MONITOR_DATA));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        private void timerMTmonSend_Tick(object sender, EventArgs e)
        {
            MTmon_Data_Send(sender);
        }
         
        private void timer1min_Tick(object sender, EventArgs e)
        {
            diskspace = cDrive.TotalFreeSpace;
        }

     }
}

