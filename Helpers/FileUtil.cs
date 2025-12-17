using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeneratingDocs
{
    public class FileUtil
    {
        private readonly IJSRuntime _js;

        public FileUtil(IJSRuntime js)
        {
            _js = js;
        }

        /// <summary>
        /// Saves a PDF file to the user's computer.
        /// </summary>
        public async Task SaveAs(string fileName, byte[] data, CancellationToken ct = default)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("File data cannot be empty.", nameof(data));

            try
            {
                string base64 = Convert.ToBase64String(data);
                await _js.InvokeVoidAsync("saveFile", ct, fileName, base64);
            }
            catch (JSException jsEx)
            {
                Console.WriteLine("FileUtil.SaveAs JS ERROR: " + jsEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FileUtil.SaveAs ERROR: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Opens a PDF document in a new browser tab for preview.
        /// </summary>
        public async Task PreviewPdf(byte[] data, CancellationToken ct = default)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("PDF data cannot be empty.", nameof(data));

            try
            {
                string base64 = Convert.ToBase64String(data);
                await _js.InvokeVoidAsync("openPdfInNewTab", ct, base64);
            }
            catch (JSException jsEx)
            {
                Console.WriteLine("FileUtil.PreviewPdf JS ERROR: " + jsEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FileUtil.PreviewPdf ERROR: " + ex.Message);
                throw;
            }
        }
    }
}
