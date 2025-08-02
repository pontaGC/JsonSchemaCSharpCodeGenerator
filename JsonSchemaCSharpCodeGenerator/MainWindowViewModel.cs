using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using Prism.Commands;
using Prism.Mvvm;

namespace JsonSchemaCSharpCodeGenerator
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly DelegateCommand<Window> browserJsonSchemaFileDialogCommand;
        private readonly DelegateCommand<Window> generateCSharpClassCommand;
        private readonly DelegateCommand<Window> copyCsharpCodeCommand;

        private string jsonSchemaFilePath;
        private string csharpNamespace = "YourNamspace";
        private string generateCSharpCode;

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            this.browserJsonSchemaFileDialogCommand = new DelegateCommand<Window>(this.BrowserJsonSchemaFileDialog);
            this.generateCSharpClassCommand = new DelegateCommand<Window>(
                async window => await this.GenerateCSharpClass(window),
                window => !string.IsNullOrEmpty(this.JsonSchemaFilePath))
                .ObservesProperty(() => this.JsonSchemaFilePath);
            this.copyCsharpCodeCommand = new DelegateCommand<Window>(this.CopyCSharpCode, this.CanCopyCSharpCode)
                .ObservesProperty(() => this.GenerateCSharpCode);
        }

        #endregion

        #region Properties

        public string JsonSchemaFilePath
        {
            get => this.jsonSchemaFilePath;
            set => this.SetProperty(ref this.jsonSchemaFilePath, value);
        }

        public string CSharpNamespace
        {
            get => this.csharpNamespace;
            set => this.SetProperty(ref this.csharpNamespace, value);
        }

        public string GenerateCSharpCode
        {
            get => this.generateCSharpCode;
            set => this.SetProperty(ref this.generateCSharpCode, value);
        }

        public ICommand BrowserJsonSchemaFileDialogCommand => this.browserJsonSchemaFileDialogCommand;

        public ICommand GenerateCSharpClassCommand => this.generateCSharpClassCommand;

        public ICommand CopyCsharpCodeCommand => this.copyCsharpCodeCommand;

        #endregion

        #region Private Methods

        private void BrowserJsonSchemaFileDialog(Window window)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Schema Files (*.json)|*.json",
                Title = "Select JSON Schema File"
            };

            if (dialog.ShowDialog(window) == true)
            {
                this.JsonSchemaFilePath = dialog.FileName;
            }
        }

        private async Task GenerateCSharpClass(Window window)
        {
            try
            {
                var schema = await JsonSchema.FromFileAsync(this.JsonSchemaFilePath);
                var generatorSettings = new CSharpGeneratorSettings()
                {
                    Namespace = this.CSharpNamespace,
                };
                var generator = new CSharpGenerator(schema, generatorSettings);
                var file = generator.GenerateFile();
                this.GenerateCSharpCode = file;

                MessageBox.Show(window, "C# class generated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(window, $"Error generating C# class: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanCopyCSharpCode(Window window)
        {
            return !string.IsNullOrEmpty(this.GenerateCSharpCode);
        }

        private void CopyCSharpCode(Window window)
        {
            try
            {
                RetryHelper.InvokeWithRetry(() => Clipboard.SetText(this.GenerateCSharpCode));
                MessageBox.Show(window, "Copied C# code", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying C# code: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
