using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Configuration;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace WpfAppGraded2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BackgroundWorker worker = new BackgroundWorker();
        public bool pauseFlag = false;
        public int curPosProgBar;
        public List<Circle> dataList = new List<Circle>();
        public CircleList serList = new CircleList();
        public static List<double> steps = new List<double>();
        public double Corner { get; set; }
        public static System.Drawing.Point currentRedPoint;
        public int Xscalar = 0;
        public Ellipse redDot = new Ellipse();
        public System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Normal);
        
        public MainWindow()
        {
            InitializeComponent();
            Corner = 0;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            curPosProgBar = 0;
            this.DataContext = dataList;
            drawCirclesMenuItem.IsChecked = true;
            drawLinesMenuItem.IsChecked = true;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            progBar.Value++;
            Render();
        }
        private void clickExitButton(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBox = System.Windows.MessageBox.Show("Are you sure you want to leave?", "Exit", System.Windows.MessageBoxButton.YesNo);
            if (messageBox == MessageBoxResult.Yes)
                this.Close();
            else
                return;
        }
        private void clickStartButton(object sender, RoutedEventArgs e)
        {
            pauseFlag = false;
            dispatcherTimer.Start();
        }
        private void clickPauseButton(object sender, RoutedEventArgs e)
        {
            pauseFlag = true;
            dispatcherTimer.Stop();
        }
        private void clickResetButton(object sender, RoutedEventArgs e)
        {
            curPosProgBar = 0;
            pauseFlag = true;
            progBar.Value = 0;
            Render();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progBar.Value = e.ProgressPercentage;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = curPosProgBar; i <= 1000; i++, curPosProgBar++)
            {
                if (pauseFlag == true)
                    break;
                (sender as BackgroundWorker).ReportProgress(i);
                Render();
                Thread.Sleep(10);
            }
        }
        private static BitmapImage BmpImageFromBmp(Bitmap bmp)
        {

            using (var memory = new System.IO.MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
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
        private void Render()
        {
            steps = new List<double>();
            for (int i = 0; i < dataList.Count; i++)
            {
                steps.Add(dataList[i].Frequency * ((2*Math.PI) / progBar.Maximum)); // defining each step for each circle
            }
            using (var bmp = new Bitmap((int)circleCanvas.ActualWidth, (int)circleCanvas.ActualHeight))
            using (var gfx = Graphics.FromImage(bmp))
            using (var redPen = new System.Drawing.Pen(System.Drawing.Color.Red))
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gfx.Clear(System.Drawing.Color.White); 
                int centerX = (int)circleCanvas.ActualWidth / 2; // center of the last circle
                int x1 = 0, y1 = 0, x2 = 0, y2 = 0; // line coordinates
                int X = 0, Y = 0; // circle coordinates

                if (progBar.Value == 0)
                {
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        // circle coordinates (upper left corner)
                        X = centerX - dataList[i].Radius;
                        Y = (int)circleCanvas.ActualHeight / 2 - dataList[i].Radius;
                        x1 = centerX;
                        y1 = Y + dataList[i].Radius;
                        x2 = x1 + dataList[i].Radius;
                        y2 = y1;

                        if (drawLinesMenuItem.IsChecked)
                            gfx.DrawLine(pen, x1, y1, x2, y2);
                        if (drawCirclesMenuItem.IsChecked)
                            gfx.DrawEllipse(pen, X, Y, 2 * dataList[i].Radius, 2 * dataList[i].Radius);
                        centerX += dataList[i].Radius;
                        if (dataList.Count - 1 == i)
                        {
                            gfx.DrawEllipse(redPen, x2 - 2, y2 - 2, 4, 4);
                        }
                    }
                }
                else if (progBar.Value != 0 ) // rotation begins
                {
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        if(i == 0)
                        {
                            X = centerX - dataList[i].Radius;
                            Y = (int)circleCanvas.ActualHeight / 2 - dataList[i].Radius; 
                            // line coords
                            x1 = centerX;
                            y1 = Y + dataList[i].Radius;

                            x2 = x1 + (int)(dataList[i].Radius * Math.Cos(steps[i] * progBar.Value));
                            y2 = y1 + (int)(dataList[i].Radius * Math.Sin(steps[i] * progBar.Value));
                        }
                        else
                        {
                            X = x2 - dataList[i].Radius;
                            Y = y2 - dataList[i].Radius;
                            x1 = x2;
                            y1 = y2;

                            x2 = (int)(x1 + dataList[i].Radius * Math.Cos(steps[i] * progBar.Value));
                            y2 = (int)(y1 + dataList[i].Radius * Math.Sin(steps[i] * progBar.Value));
                        }
                        if (drawLinesMenuItem.IsChecked)
                            gfx.DrawLine(pen, x1, y1, x2, y2);
                        if (drawCirclesMenuItem.IsChecked)
                            gfx.DrawEllipse(pen, X, Y, 2 * dataList[i].Radius, 2 * dataList[i].Radius);
                        // painting red dot
                        if(dataList.Count - 1 == i)
                        {
                            gfx.DrawEllipse(redPen, x2 - 2, y2 - 2, 4, 4);
                        }
                    }
                }
                circleImage.Source = BmpImageFromBmp(bmp);
            }
        }


        private void DataList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (Key.Delete == e.Key)
            {
                Render();
            }
        }

        private void DataList_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            if (progBar.Value != 0)
                progBar.Value = 0;
            curPosProgBar = 0;
            pauseFlag = false;
            progBar.Value = 0;
            Render();
        }

        private void DrawCircleMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if(dataList.Count != 0)
                Render();
        }

        private void DrawLinesMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if(dataList.Count != 0)
                Render();
        }

        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            clickResetButton(sender, e);
            dataList = new List<Circle>();
            DataList.DataContext = dataList;
            Xscalar = 0;
            Render();
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // deserialization here
            bool success = false;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "XML(*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && openFileDialog.CheckFileExists == true)
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(CircleList));
                using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    try
                    {
                        serList = (CircleList)deserializer.Deserialize(fs);
                        success = true;
                    }
                    catch (Exception xmlDE)
                    {
                        MessageBox.Show($"Error occured: {xmlDE.Message}");
                    }
                }
            }
            if (success == true)
            {
                dataList = serList.Circles;
                DataList.DataContext = dataList;
            }
            Render();
        }
    
        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // serialization here
            serList.Circles = dataList;
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "XML(*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog.CheckPathExists == true)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CircleList));
                using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate))
                {
                    try
                    {
                        xmlSerializer.Serialize(fs, serList);
                    }
                    catch (Exception xmlE)
                    {
                        MessageBox.Show($"Error occured: {xmlE.Message}");
                    }
                }
            }
        }
        private void drawCirclesMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            if(dataList.Count != 0)
                Render();
        }

        private void drawLinesMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            if(dataList.Count != 0)
                Render();
        }
    }

    [Serializable]
    public class Circle
    {
        public int Radius { get; set; }
        public double Frequency { get; set; }
    }
    [Serializable]
    public class CircleList
    {
        public List<Circle> Circles { get; set; }
        public CircleList()
        {
            Circles = new List<Circle>();
        }
        public CircleList(List<Circle> l)
        {
            Circles = l;
        }
    }
}
