using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using OmniCore.Modules.Hash.Abstractions.Enums;
using OmniCore.Modules.Hash.Abstractions.Interfaces;
using OmniCore.Modules.HashCalculator.Resources.Languages;

namespace OmniCore.Modules.HashCalculator.Pages
{
    public sealed partial class HashCalculatorPage : ComponentBase
    {
        #region Injection
        [Inject] private IHashProviderFactory HashFactory { get; set; } = default!;
        [Inject] private IStringLocalizer<HashResources> Localizer { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        #endregion

        private const string ErrorLKey = "Common_Error";
        private string _selectedAlgorithm = "SHA-256";
        private HashOutputFormat _selectedFormat = HashOutputFormat.LowerHex;
        private string _inputText = string.Empty;
        private string _computedHash = string.Empty;
        private List<string> _availableAlgorithms = [];

        // Для работы с файлами
        private FileResult? _selectedFile;
        private bool _isComputing;
        private CancellationTokenSource? _cts;

        protected override void OnInitialized()
        {
            _availableAlgorithms = [.. HashFactory.GetAvailableAlgorithms()];

            if (_availableAlgorithms.Count > 0 && !_availableAlgorithms.Contains(_selectedAlgorithm))
            {
                _selectedAlgorithm = _availableAlgorithms[0];
            }
        }

        #region Text Mode

        private async Task ComputeHashFromTextAsync()
        {
            if (string.IsNullOrEmpty(_inputText) || string.IsNullOrEmpty(_selectedAlgorithm))
            {
                return;
            }

            if (!HashFactory.TryGetProvider(_selectedAlgorithm, out IHashProvider? provider) || provider is null)
            {
                Snackbar.Add(Localizer["Calculator_Error_ProviderNotFound"], Severity.Error);
                return;
            }

            try
            {
                _isComputing = true;
                _computedHash = string.Empty;
                StateHasChanged();

                _computedHash = await provider.CalculateAsync(_inputText, _selectedFormat, null);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"{Localizer[ErrorLKey]}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isComputing = false;
                StateHasChanged();
            }
        }

        #endregion

        #region File Mode

        private async Task PickFileAsync()
        {
            try
            {
                FileResult? result = await FilePicker.Default.PickAsync();
                if (result is not null && !string.IsNullOrEmpty(result.FullPath))
                {
                    _selectedFile = result;
                    _computedHash = string.Empty;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"{Localizer[ErrorLKey]}: {ex.Message}", Severity.Error);
            }
        }

        private void ClearSelectedFile()
        {
            _selectedFile = null;
            _computedHash = string.Empty;
        }

        private async Task ComputeHashFromFileAsync()
        {
            if (_selectedFile is null || string.IsNullOrEmpty(_selectedFile.FullPath))
            {
                return;
            }

            if (!HashFactory.TryGetProvider(_selectedAlgorithm, out IHashProvider? provider) || provider is null)
            {
                Snackbar.Add(Localizer["Calculator_Error_ProviderNotFound"], Severity.Error);
                return;
            }

            _cts = new CancellationTokenSource();
            string filePath = _selectedFile.FullPath;

            try
            {
                _isComputing = true;
                _computedHash = string.Empty;
                StateHasChanged();

                var fileInfo = new FileInfo(filePath);
                long totalBytes = fileInfo.Length;

                if (totalBytes == 0)
                {
                    Snackbar.Add(Localizer["Calculator_Error_EmptyFile"], Severity.Warning);
                    return;
                }

                _computedHash = await provider.CalculateAsync(filePath, _selectedFormat, cancellationToken: _cts.Token);

                StateHasChanged();
                Snackbar.Add(Localizer["Calculator_Success"], Severity.Success);
            }
            catch (OperationCanceledException)
            {
                Snackbar.Add(Localizer["Calculator_Cancelled"], Severity.Warning);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"{Localizer[ErrorLKey]}: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isComputing = false;
                _cts?.Dispose();
                _cts = null;
                StateHasChanged();
            }
        }

        private void CancelComputation()
        {
            _cts?.Cancel();
        }

        #endregion

        #region Helpers

        private async Task CopyResultAsync()
        {
            if (string.IsNullOrEmpty(_computedHash))
            {
                return;
            }

            try
            {
                await Clipboard.SetTextAsync(_computedHash);
                Snackbar.Add(Localizer["Common_Copied"], Severity.Info);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"{Localizer[ErrorLKey]}: {ex.Message}", Severity.Error);
            }
        }

        #endregion
    }
}
