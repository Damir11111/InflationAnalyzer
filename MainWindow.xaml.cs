using InflationAnalyzer.Models;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace InflationAnalyzer
{
    public partial class MainWindow : Window
    {
        // Основные данные
        private List<InflationData> inflationData =
            new List<InflationData>();

        // Прогнозные данные
        private List<InflationData> forecastData =
            new List<InflationData>();

        public MainWindow()
        {
            InitializeComponent();
        }

        // Загрузка TXT файла
        private void LoadButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog =
                new OpenFileDialog();

            openFileDialog.Filter =
                "Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            string filePath =
                openFileDialog.FileName;

            inflationData.Clear();

            var lines =
                File.ReadAllLines(filePath).Skip(1);

            foreach (var line in lines)
            {
                var parts = line.Split(',');

                inflationData.Add(
                    new InflationData
                    {
                        Year = int.Parse(parts[0]),

                        Inflation = double.Parse(
                            parts[1],
                            CultureInfo.InvariantCulture),

                        ApartmentPrice = double.Parse(
                            parts[2],
                            CultureInfo.InvariantCulture)
                    });
            }

            // Вывод таблицы
            InflationGrid.ItemsSource =
                inflationData;

            // Построение графика
            DrawChart();

            MessageBox.Show(
                "TXT file loaded successfully!");
        }

        // Построение графика
        private void DrawChart()
        {
            var plotModel = new PlotModel
            {
                Title = "Inflation Dynamics"
            };

            // Реальные данные
            var realSeries = new LineSeries
            {
                Title = "Real Inflation"
            };

            foreach (var item in inflationData)
            {
                realSeries.Points.Add(
                    new DataPoint(
                        item.Year,
                        item.Inflation));
            }

            plotModel.Series.Add(realSeries);

            // Прогноз
            if (forecastData.Count > 0)
            {
                var forecastSeries =
                    new LineSeries
                    {
                        Title = "Forecast"
                    };

                foreach (var item in forecastData)
                {
                    forecastSeries.Points.Add(
                        new DataPoint(
                            item.Year,
                            item.Inflation));
                }

                plotModel.Series.Add(
                    forecastSeries);
            }

            PlotView.Model = plotModel;
        }

        // Прогноз методом скользящей средней
        private void ForecastButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Проверка загрузки данных
            if (inflationData.Count == 0)
            {
                MessageBox.Show(
                    "Load TXT file first!");

                return;
            }

            // Проверка года
            if (!int.TryParse(
                YearsTextBox.Text,
                out int targetYear))
            {
                MessageBox.Show(
                    "Enter valid year!");

                return;
            }

            forecastData.Clear();

            var lastItem =
                inflationData.Last();

            int currentYear =
                lastItem.Year;

            double apartmentPrice =
                lastItem.ApartmentPrice;

            // Проверка будущего года
            if (targetYear <= currentYear)
            {
                MessageBox.Show(
                    "Enter future year!");

                return;
            }

            // Список значений инфляции
            List<double> inflationValues =
                inflationData
                .Select(x => x.Inflation)
                .ToList();

            // Прогнозирование
            for (int year = currentYear + 1;
                 year <= targetYear;
                 year++)
            {
                int count =
                    inflationValues.Count;

                // Скользящая средняя за 3 года
                double movingAverage =
                    (inflationValues[count - 1] +
                     inflationValues[count - 2] +
                     inflationValues[count - 3]) / 3;

                inflationValues.Add(
                    movingAverage);

                // Прогноз цены квартиры
                apartmentPrice +=
                    apartmentPrice *
                    (movingAverage / 100);

                forecastData.Add(
                    new InflationData
                    {
                        Year = year,

                        Inflation =
                            Math.Round(
                                movingAverage,
                                2),

                        ApartmentPrice =
                            Math.Round(
                                apartmentPrice,
                                0)
                    });
            }

            // Обновление таблицы
            InflationGrid.ItemsSource =
                null;

            InflationGrid.ItemsSource =
                forecastData;

            // Обновление графика
            DrawChart();
            //
            // Финальная цена квартиры
            double finalPrice =
                forecastData.Last()
                .ApartmentPrice;

            MessageBox.Show(
                $"Forecast completed!\n\n" +
                $"Apartment price in {targetYear}: " +
                $"{finalPrice:F0} RUB");
        }
    }
}