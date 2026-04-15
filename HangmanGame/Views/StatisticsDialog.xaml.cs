using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HangmanGame.Models;
using HangmanGame.Services;

namespace HangmanGame.Views
{
    public partial class StatisticsDialog : Window
    {
        public StatisticsDialog(StatisticsService statisticsService, List<string> userNames)
        {
            InitializeComponent();
            BuildStatisticsUI(statisticsService, userNames);
        }

        private void BuildStatisticsUI(StatisticsService statisticsService, List<string> userNames)
        {
            var allStats = statisticsService.GetAllStatistics(userNames);

            foreach (var stats in allStats)
            {
                var tabItem = new TabItem
                {
                    Header = stats.UserName
                };

                var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                var panel = new StackPanel { Margin = new Thickness(16) };

                // Overall stats
                var overallBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(227, 242, 253)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var overallGrid = new Grid();
                overallGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                overallGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var totalPanel = CreateStatBlock("🎮 Total Games", stats.TotalGamesPlayed.ToString());
                var wonPanel = CreateStatBlock("🏆 Games Won", $"{stats.TotalGamesWon} ({WinRate(stats.TotalGamesPlayed, stats.TotalGamesWon)}%)");
                Grid.SetColumn(wonPanel, 1);

                overallGrid.Children.Add(totalPanel);
                overallGrid.Children.Add(wonPanel);
                overallBorder.Child = overallGrid;
                panel.Children.Add(overallBorder);

                // Category stats
                if (stats.CategoryStats.Count > 0)
                {
                    var catHeader = new TextBlock
                    {
                        Text = "By Category",
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                        Margin = new Thickness(0, 0, 0, 8)
                    };
                    panel.Children.Add(catHeader);

                    foreach (var kvp in stats.CategoryStats)
                    {
                        var catBorder = new Border
                        {
                            Background = Brushes.White,
                            CornerRadius = new CornerRadius(6),
                            BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                            BorderThickness = new Thickness(1),
                            Padding = new Thickness(12, 8, 12, 8),
                            Margin = new Thickness(0, 0, 0, 6)
                        };

                        var catGrid = new Grid();
                        catGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        catGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        catGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                        var nameBlock = new TextBlock
                        {
                            Text = kvp.Key,
                            FontSize = 14,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontWeight = FontWeights.Medium
                        };

                        var playedBlock = new TextBlock
                        {
                            Text = $"Played: {kvp.Value.GamesPlayed}",
                            FontSize = 13,
                            Foreground = new SolidColorBrush(Color.FromRgb(97, 97, 97)),
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(16, 0, 0, 0)
                        };
                        Grid.SetColumn(playedBlock, 1);

                        var wonBlock = new TextBlock
                        {
                            Text = $"Won: {kvp.Value.GamesWon} ({WinRate(kvp.Value.GamesPlayed, kvp.Value.GamesWon)}%)",
                            FontSize = 13,
                            Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50)),
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(16, 0, 0, 0)
                        };
                        Grid.SetColumn(wonBlock, 2);

                        catGrid.Children.Add(nameBlock);
                        catGrid.Children.Add(playedBlock);
                        catGrid.Children.Add(wonBlock);
                        catBorder.Child = catGrid;
                        panel.Children.Add(catBorder);
                    }
                }
                else
                {
                    var noStats = new TextBlock
                    {
                        Text = "No games played yet.",
                        FontSize = 14,
                        Foreground = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    panel.Children.Add(noStats);
                }

                scrollViewer.Content = panel;
                tabItem.Content = scrollViewer;
                UsersTabControl.Items.Add(tabItem);
            }

            if (UsersTabControl.Items.Count == 0)
            {
                var emptyTab = new TabItem { Header = "No Users" };
                var block = new TextBlock
                {
                    Text = "No user statistics available.",
                    FontSize = 15,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(158, 158, 158))
                };
                emptyTab.Content = block;
                UsersTabControl.Items.Add(emptyTab);
            }
        }

        private static StackPanel CreateStatBlock(string label, string value)
        {
            var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(97, 97, 97)),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            panel.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(21, 101, 192)),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return panel;
        }

        private static int WinRate(int total, int won)
        {
            if (total == 0) return 0;
            return (int)((double)won / total * 100);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
