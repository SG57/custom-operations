using System;
using System.Collections.Generic;
using System.IO;

namespace Custom_Operations;

public class CustomOperationsConfiguration
{
    public static string ConfigFile =>
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData
            )
            , "custom_operations.config"
        );

    public List<OperationItem>? OperationItems { get; set; }
}
