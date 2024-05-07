using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace PhotoMoverWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sourceDirectory = sourceDirectoryTextBox.Text.Trim();
                string destinationDirectory = destinationDirectoryTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(sourceDirectory) || string.IsNullOrWhiteSpace(destinationDirectory))
                {
                    Dispatcher.Invoke(() => resultTextBlock.Text = "Please select both source and destination directories.");
                    return;
                }

                // Ensure the destination directory exists
                Directory.CreateDirectory(destinationDirectory);

                // Initialize counters for successful file operations and total size
                int successfulOperations = 0;
                long totalSizeInBytes = 0;

                // Delegate to update the TextBlock
                Action updateTextBlock = () =>
                {
                    // Example of updating the TextBlock with a message
                    resultTextBlock.Text += "Starting file transfer...\n";

                    // Iterate through all files in the source directory
                    foreach (var filePath in Directory.GetFiles(sourceDirectory))
                    {
                        try
                        {
                            // Get the file name without extension for comparison
                            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

                            // Check if a file with the same name already exists in the destination directory
                            string destinationFilePath = Path.Combine(destinationDirectory, fileNameWithoutExt + Path.GetExtension(filePath));
                            if (File.Exists(destinationFilePath))
                            {
                                resultTextBlock.Text += $"Duplicate file found: {fileNameWithoutExt}. Skipped.\n";
                                continue; // Skip this iteration
                            }

                            // Calculate the size of the file before copying in bytes
                            FileInfo fileInfo = new FileInfo(filePath);
                            long fileSizeInBytes = fileInfo.Length;

                            // Copy the file from the source directory to the destination directory
                            File.Copy(filePath, destinationFilePath, true);

                            // Delete the file from the source directory after copying
                            File.Delete(filePath);

                            // Add the file size to the total size in bytes
                            totalSizeInBytes += fileSizeInBytes;

                            // Increment the counter for successful operations
                            successfulOperations++;
                        }
                        catch (FileNotFoundException ex)
                        {
                            resultTextBlock.Text += $"File not found: {ex.FileName}\n";
                        }
                        catch (IOException ioEx)
                        {
                            resultTextBlock.Text += $"An error occurred while processing the file: {ioEx.Message}\n";
                        }
                    }

                    // Convert the total size from bytes to gigabytes or megabytes
                    double totalSizeInGB = totalSizeInBytes / Math.Pow(1024, 3);
                    double totalSizeInMB = totalSizeInBytes / Math.Pow(1024, 2);

                    // Determine the most appropriate unit for displaying the total size
                    string unit = totalSizeInGB >= 1 ? "GB" : "MB";
                    double totalSizeInAppropriateUnit = totalSizeInGB >= 1 ? totalSizeInGB : totalSizeInMB;

                    // Print the total number of files successfully copied and deleted
                    resultTextBlock.Text += $"{successfulOperations} files copied and deleted successfully.\n";

                    // Print the total size of the copied files in the most appropriate unit
                    resultTextBlock.Text += $"Total size of copied files: {totalSizeInAppropriateUnit:N2} {unit}.\n";
                };

                // Use Dispatcher.Invoke to ensure the UI update runs on the UI thread
                Dispatcher.Invoke(updateTextBlock);
            }
            catch (Exception ex)
            {
                // Use Dispatcher.Invoke to ensure the exception message is displayed on the UI thread
                Dispatcher.Invoke(() => resultTextBlock.Text += $"An unexpected error occurred: {ex.Message}\n");
            }
        }

        private void SelectSourceDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                sourceDirectoryTextBox.Text = dialog.FileName;
            }
        }

        private void SelectDestinationDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                destinationDirectoryTextBox.Text = dialog.FileName;
            }
        }
    }
}
