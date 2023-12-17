using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace PainteRS
{
    internal class Improc : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public List<Point> BP { get; } = new List<Point>(); //массив для чёрных пикселей
        public Point centerMass;
        public Point centerMat;
        double MinMaxOtnoshenie = 0.0; // отношение минимальной длинны (от центра масс до фигуры) к максимальной
                                       // List <string> formuls = new List<string>();
        List<double> otnosheniya = new List<double>(); //первые 6 элементов для ф. Otnoshenie, остальные в FindRelativeCenterMassOrMat получают координаты центров

        //List <string> namesLibs = new List<string>();

        private string[] _LIST;
        public string[] LIST
        {
            get => _LIST;
            set
            {
                _LIST = value;
                OnPropertyChanged("LIST");
            }

        }
        public Improc(FrameworkElement elem) // получает канвас
        {

            // Get the size of canvas
            Size size = new Size(elem.Width, elem.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            elem.Measure(size);
            elem.Arrange(new Rect(size));


            RenderTargetBitmap bitmap = new RenderTargetBitmap
                ((int)size.Width, (int)size.Height, 96d, 96d,
      PixelFormats.Pbgra32); //создаём "катртинку" размерам с нашь canvas
                             //Обязательно использовать Pbrgba32 !!!!!
            elem.Measure(new Size((int)elem.Width, (int)elem.Height));
            elem.Arrange(new Rect(new Size((int)elem.Width, (int)elem.Height)));

            bitmap.Render(elem);//переносим туда наше полотно

            int widthPich = bitmap.PixelWidth;
            int heightPich = bitmap.PixelHeight;
            int[] array = new int[widthPich * 4 * heightPich]; // массив для пикселей

            bitmap.CopyPixels(array, widthPich * 4, 0); // копирование информации о пикселях в массив

            int x = 0;
            int y = 0;
            List<Color> colors = new List<Color>();
            for (int i = 0; i < array.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(array[i]);
                Color pixel = Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
                colors.Add(pixel);

                // Сверху информация о пикселах каждого раздельно
            }
            //  Снизу заносятся в массив чёрные пиксели

            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i].ToString() == "#FF000000")
                {
                    BP.Add(new Point(x, y));
                }
                if (i == widthPich * (y + 1) - 1)
                {
                    x = -1;
                    ++y;
                }
                x++;
            }

        }
        public (double cxl, double cxr, double cyt, double cyd) Counter()
        {
            double cxl = 0, cxr = 0, cyt = 0, cyd = 0;
            foreach (Point item in BP)
            {
                if (item.X < centerMat.X)
                {
                    cxl++;
                }
                else if (item.X > centerMat.X)
                {
                    cxr++;
                }
                if (item.Y < centerMat.Y)
                {
                    cyd++;
                }
                else if (item.Y > centerMat.Y)
                {
                    cyt++;
                }
            }
            return (cxl, cxr, cyt, cyd);
        }
        public int Calculator(List<string> TheMainFormula, object[] VariablesLol)
        {
            List<object> Probabilities = new List<object>();
            for (int i = 0; i < TheMainFormula.Count(); ++i)
            {
                string newString = string.Format(TheMainFormula[i], VariablesLol);
                newString = newString.Replace(',', '.');
                var result = new DataTable().Compute(newString, "");

                if (result.Equals(0) != true)
                {
                    Probabilities.Add(result);
                }
                else
                {
                    Probabilities.Add(0.00000000001);
                }
                //Probabilities.Add(result);

            }
            var themax = Probabilities.Max();
            int MaxIndex = Probabilities.IndexOf(themax);

            return MaxIndex;
        }
        public List<Point> FindRelativeCenterMassOrMat()
        {
            var centerMatMin = BP[0];
            Point centerMatMax = new Point(0, 0);

            foreach (var item in BP) // находит центр масс и математический центр
            {
                centerMass.X += item.X;
                centerMass.Y += item.Y;
                if (item.Y < centerMatMin.Y) centerMatMin.Y = item.Y;
                else if (item.Y > centerMatMax.Y) centerMatMax.Y = item.Y;
                if (item.X < centerMatMin.X) centerMatMin.X = item.X;
                else if (item.X > centerMatMax.X) centerMatMax.X = item.X;
            }

            centerMat = new Point((centerMatMax.X + centerMatMin.X) / 2,
                                  (centerMatMax.Y + centerMatMin.Y) / 2); // математический центр

            centerMass.X /= BP.Count;
            centerMass.Y /= BP.Count;

            /*otnosheniya[6] = centerMass.X;
            otnosheniya[7] = centerMass.Y;
            otnosheniya[8] = centerMat.X;
            otnosheniya[9] = centerMat.Y;*/

            Point LD = new Point(centerMatMin.X, centerMatMin.Y); // Left Down 
            Point RD = new Point(centerMatMax.X, centerMatMin.Y); // Right down
            Point LU = new Point(centerMatMin.X, centerMatMax.Y); // Left Up
            Point RU = new Point(centerMatMax.X, centerMatMax.Y); // Right Up                                                
            Point LM = new Point(centerMatMin.X, centerMat.Y); // Left Middle 
            Point MU = new Point(centerMat.X, centerMatMax.Y); // Middle up 
            List<Point> points = new List<Point> { centerMass, LD, LU, RD, RU, LM, MU, centerMat };
            return points;
        }
        public List<double> Koordinati(List<Point> cenrtesMass)
        {
            List<double> koordinati = new List<double>();
            foreach (Point item in cenrtesMass)
            {
                koordinati.Add(item.X);
                koordinati.Add(item.Y);
            }
            return koordinati;
        }

        public List<double> VsePeremennie((double, double, double, double) tuple, List<double> LIST1, List<double> LIST2) // создает список всех переменных для ацс
        {
            List<double> peremennie = new List<double>();
            peremennie.Add(tuple.Item1);
            peremennie.Add(tuple.Item2);
            peremennie.Add(tuple.Item3);
            peremennie.Add(tuple.Item4);
            peremennie.AddRange(LIST1);
            peremennie.AddRange(LIST2);
            return peremennie;


        }

        public List<double> Otnoshenie(List<Point> cenrtesMass)
        {

            double mindist = 1000.0;
            double maxdist = 0.0;
            double dist;
            int l = cenrtesMass.Count;
            for (int i = 0; i < cenrtesMass.Count; i++)
            {
                foreach (Point item in BP)
                {
                    //БРАКОВАНО
                    dist = Math.Sqrt(((item.X - cenrtesMass[i].X) * (item.X - cenrtesMass[i].X))
                                   + ((item.Y - cenrtesMass[i].Y) * (item.Y - cenrtesMass[i].Y)));
                    if (dist > maxdist) { maxdist = dist; }
                    if (dist < mindist && dist != 0) { mindist = dist; }
                }

                MinMaxOtnoshenie = mindist / maxdist;
                otnosheniya.Add(MinMaxOtnoshenie);
            }

            return otnosheniya;
        }

        public string WhatsLett() 
        {
            if (otnosheniya[0] < 0.2)
            {
                return "1";
            }
            else if (otnosheniya[0] > 0.35)
            {
                return "0";
            }
            else
            {
                return "NULL";
            }
        }// Возвращает цифру которую мы выводим
    }
}
