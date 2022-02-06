using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Custom_Operations;

internal partial class MainWindow
{
    private CustomOperationsConfiguration Configuration { get; }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        Configuration = LoadConfiguration();

        for (var index = 1; index <= 9; index++)
        {
            AddOperationItem(index);
        }
        AddOperationItem(0);

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(20)
        };
        timer.Tick += OnTimerOnTick;
        timer.Start();
    }

    private void OnTimerOnTick(object? sender, EventArgs args)
    {
        try
        {
            Close();
        }
        catch
        {
            // ignored
        }
    }

    private void AddOperationItem(int index)
    {
        var operationItem = Configuration.OperationItems?.FirstOrDefault(item => item.Index == index)
                            ?? new OperationItem
                            {
                                Index = index
                            };
        OperationsListBox.Items.Add(operationItem);
    }

    private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            Close();
        }
        catch
        {
            // ignored
        }
    }

    private void MainWindow_OnDeactivated(object? sender, EventArgs e)
    {
        try
        {
            Close();
        }
        catch
        {
            // ignored
        }
    }

    private CustomOperationsConfiguration LoadConfiguration()
    {
        if (!File.Exists(CustomOperationsConfiguration.ConfigFile))
        {
            CreateDefaultConfigFile();
        }
        var configJson = File.ReadAllText(CustomOperationsConfiguration.ConfigFile);

        return JsonSerializer.Deserialize<CustomOperationsConfiguration>(configJson)
               ?? throw new NullReferenceException();
    }

    private void CreateDefaultConfigFile()
    {
        File.WriteAllText(
            CustomOperationsConfiguration.ConfigFile
            , FormatJsonText(
                JsonSerializer.Serialize(
                    new CustomOperationsConfiguration
                    {
                        OperationItems = new List<OperationItem>
                        {
                            new()
                            {
                                Index = 1
                                , Name = "Google"
                                , Type = OperationType.StartProcess
                                , Arguments = "https://www.google.com"
                            }
                        }
                    }
                )
            )
        );
    }

    private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        ExecuteOperation(OperationItem.EditConfig);
    }

    private void ExecuteOperation(OperationItem? operationItem)
    {
        if (operationItem == null)
        {
            return;
        }
        var processStartInfo = new ProcessStartInfo
        {
            WorkingDirectory = GetCwd()
        };

        switch (operationItem?.Type)
        {
            case OperationType.CopyToClipboard:
                Clipboard.SetText(operationItem.Arguments?.ToString() ?? throw new InvalidOperationException());
                break;

            case OperationType.StartProcess:
                var arguments = JsonSerializer.Deserialize<string[]>(operationItem.Arguments?.ToString() ?? throw new InvalidOperationException("Missing arguments"));
                processStartInfo.UseShellExecute = true;
                processStartInfo.FileName = arguments?[0] ?? throw new InvalidOperationException("Missing process argument.");
                if (arguments.Length >= 2)
                {
                    processStartInfo.Arguments = arguments[1];
                }
                Process.Start(processStartInfo);
                break;
        }
    }

    private string? GetCwd()
    {
        var strExeFilePath = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(strExeFilePath);
    }

    private static string FormatJsonText(string jsonString)
    {
        using var doc = JsonDocument.Parse(
            jsonString
            , new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            }
        );
        var memoryStream = new MemoryStream();
        using (
            var utf8JsonWriter = new Utf8JsonWriter(
                memoryStream
                , new JsonWriterOptions
                {
                    Indented = true
                }
            )
        )
        {
            doc.WriteTo(utf8JsonWriter);
        }
        return new System.Text.UTF8Encoding()
            .GetString(memoryStream.ToArray());
    }

    private void MainWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        int? index = null;
        switch (e.Key)
        {
            case Key.NumPad0:
            case Key.D0:
                index = 0;
                break;
            case Key.NumPad1:
            case Key.D1:
                index = 1;
                break;
            case Key.NumPad2:
            case Key.D2:
                index = 2;
                break;
            case Key.NumPad3:
            case Key.D3:
                index = 3;
                break;
            case Key.NumPad4:
            case Key.D4:
                index = 4;
                break;
            case Key.NumPad5:
            case Key.D5:
                index = 5;
                break;
            case Key.NumPad6:
            case Key.D6:
                index = 6;
                break;
            case Key.NumPad7:
            case Key.D7:
                index = 7;
                break;
            case Key.NumPad8:
            case Key.D8:
                index = 8;
                break;
            case Key.NumPad9:
            case Key.D9:
                index = 9;
                break;
        }
        if (index != null)
        {
            var operationItem = Configuration.OperationItems?.FirstOrDefault(item => item.Index == index);
            if (operationItem != null)
            {
                ExecuteOperation(operationItem);
            }
        }

        e.Handled = true;
        Close();
    }

    private void ListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ExecuteOperation((sender as ListBoxItem)?.DataContext as OperationItem);
        e.Handled = true;
        Close();
    }
}
